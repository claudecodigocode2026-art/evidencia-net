using System.Globalization;
using System.Net;
using System.Text.Json;
using eVidencia.BLL;
using eVidencia.Comun.Datos;
using eVidencia.Comun.Grid;
using eVidencia.Comun.Seguridad;
using eVidencia.DAL;
using eVidencia.DAL.Memoria;
using eVidencia.DAL.SqlServer;
using eVidencia.Entidades;
using eVidencia.Entidades.Filtros;
using eVidencia.Web.Servicios;
using Microsoft.AspNetCore.Localization;

var builder = WebApplication.CreateBuilder(args);

// ---------------------------------------------------------------------------------------------
// Configuración de la demo (appsettings.json)
//   Datos:Modo            "Excel" (libro Excel con una hoja por tabla), "Memoria" (datos de ejemplo) o "SqlServer"
//   Datos:FicheroExcel    rutas de libro separadas por ';' (relativas al proyecto web); se usa el primero que exista
//   Datos:CadenaConexion  cadena de conexión cuando el modo es "SqlServer"
//   Demo:UsuarioFijo      UID del usuario con el que entra la demo (debe existir en la tabla USUARIO).
//                         Vacío en los modos Excel y Memoria: entra el primer Administrador de la tabla.
// ---------------------------------------------------------------------------------------------
string modoDatos = builder.Configuration["Datos:Modo"] ?? "Excel";
string usuarioFijo = builder.Configuration["Demo:UsuarioFijo"] ?? "";

builder.Services.AddRazorPages();

// Las páginas escriben los caracteres acentuados tal cual.
builder.Services.Configure<Microsoft.Extensions.WebEncoders.WebEncoderOptions>(o =>
    o.TextEncoderSettings = new System.Text.Encodings.Web.TextEncoderSettings(System.Text.Unicode.UnicodeRanges.All));

if (modoDatos.Equals("SqlServer", StringComparison.OrdinalIgnoreCase))
{
#if SIN_SQLSERVER
    throw new InvalidOperationException("Esta compilación no incluye el proveedor de SQL Server (se compiló con SinSqlServer=true).");
#else
    string cadena = builder.Configuration["Datos:CadenaConexion"]
        ?? throw new InvalidOperationException("Falta Datos:CadenaConexion en appsettings.json.");
    if (usuarioFijo == "")
        throw new InvalidOperationException("En modo SqlServer hay que indicar Demo:UsuarioFijo en appsettings.json.");
    builder.Services.AddSingleton<IFabricaConexiones>(
        new FabricaConexiones(Microsoft.Data.SqlClient.SqlClientFactory.Instance, cadena));
    builder.Services.AddScoped<IRepositorioProcesos, RepositorioProcesosSql>();
    builder.Services.AddScoped<IRepositorioSubprocesos, RepositorioSubprocesosSql>();
    builder.Services.AddScoped<IRepositorioUsuarios, RepositorioUsuariosSql>();
    builder.Services.AddScoped<IRepositorioLogDeEntidades, RepositorioLogDeEntidadesSql>();
    builder.Services.AddSingleton(new InfoDemo(usuarioFijo, "SQL Server"));
#endif
}
else if (modoDatos.Equals("Excel", StringComparison.OrdinalIgnoreCase) || modoDatos.Equals("Memoria", StringComparison.OrdinalIgnoreCase))
{
    AlmacenMemoria almacen;
    string descripcion;

    if (modoDatos.Equals("Excel", StringComparison.OrdinalIgnoreCase))
    {
        // Datos:FicheroExcel admite varias rutas separadas por ';': se usa el primer libro que exista.
        string configurado = builder.Configuration["Datos:FicheroExcel"]
            ?? throw new InvalidOperationException("Falta Datos:FicheroExcel en appsettings.json.");
        string? ruta = configurado
            .Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(r => Path.GetFullPath(Path.Combine(builder.Environment.ContentRootPath, r)))
            .FirstOrDefault(File.Exists);
        if (ruta != null)
        {
            almacen = AlmacenMemoria.DesdeExcel(ruta);
            descripcion = almacen.Origen + " (los cambios no se guardan en el libro)";
        }
        else
        {
            // Sin libro, la demo arranca con los datos de ejemplo y lo avisa en el pie de página.
            almacen = AlmacenMemoria.DeEjemplo();
            descripcion = "de ejemplo: no se ha encontrado ningún libro Excel en la carpeta datos";
        }
    }
    else
    {
        almacen = AlmacenMemoria.DeEjemplo();
        descripcion = almacen.Origen + " (se pierden al parar la aplicación)";
    }

    if (usuarioFijo == "")
    {
        usuarioFijo = almacen.Usuarios
            .Where(u => !u.Borrado && u.IdPerfilDeUsuario == (int)PerfilDeUsuario.Administrador && !string.IsNullOrEmpty(u.UID))
            .OrderBy(u => u.IdUsuario)
            .Select(u => u.UID!)
            .FirstOrDefault()
            ?? throw new InvalidOperationException("Los datos no contienen ningún usuario con perfil Administrador.");
    }

    builder.Services.AddSingleton(almacen);
    builder.Services.AddScoped<IRepositorioProcesos, RepositorioProcesosMemoria>();
    builder.Services.AddScoped<IRepositorioSubprocesos, RepositorioSubprocesosMemoria>();
    builder.Services.AddScoped<IRepositorioUsuarios, RepositorioUsuariosMemoria>();
    builder.Services.AddScoped<IRepositorioLogDeEntidades, RepositorioLogDeEntidadesMemoria>();
    builder.Services.AddSingleton(new InfoDemo(usuarioFijo, descripcion));
}
else
{
    throw new InvalidOperationException($"Datos:Modo no reconocido: '{modoDatos}'. Valores válidos: Excel, Memoria, SqlServer.");
}

// Usuario fijo: sustituye, solo en la demo, a la autenticación del original (Login.aspx + LogonUser).
builder.Services.AddSingleton<IUsuarioActual>(new UsuarioFijo(usuarioFijo));

// Lógica de negocio
builder.Services.AddScoped<GestorUsuarios>();
builder.Services.AddScoped<GestorLogDeEntidades>();
builder.Services.AddScoped<GestorSubprocesos>();
builder.Services.AddScoped<GestorProcesos>();

var app = builder.Build();

// La demo trabaja solo en español (el original admite además en-US y pt-BR).
CultureInfo cultura = new("es-ES");
app.UseRequestLocalization(new RequestLocalizationOptions
{
    DefaultRequestCulture = new RequestCulture(cultura),
    SupportedCultures = [cultura],
    SupportedUICultures = [cultura],
    RequestCultureProviders = []
});

app.UseExceptionHandler("/Error.aspx");
app.UseStaticFiles();
app.UseRouting();
app.MapRazorPages();

app.MapGet("/", () => Results.Redirect("/Administracion/Procesos.aspx"));

// ---------------------------------------------------------------------------------------------
// Servicio del listado de Procesos para jqGrid.
// Origen: eVidencias.Web/Administracion/AdministracionService.asmx.cs:154-170 (ConsultarGridProcesos).
// Se mantiene la misma dirección y el mismo formato de respuesta que el servicio ASMX original.
// ---------------------------------------------------------------------------------------------
JsonSerializerOptions opcionesJson = new() { PropertyNamingPolicy = null };

app.MapPost("/Administracion/AdministracionService.asmx/ConsultarGridProcesos", async (HttpContext http, GestorProcesos gestorProcesos) =>
{
    IFormCollection form = await http.Request.ReadFormAsync();
    int page = int.TryParse(form["page"], out int p) ? p : -1;
    int rows = int.TryParse(form["rows"], out int r) ? r : -1;
    string sidx = form["sidx"].ToString();
    string sord = form["sord"].ToString();
    string filtro = form["filtro"].ToString();

    string orden = sidx + " " + sord;

    FiltroPROCESO? filtroProceso = string.IsNullOrWhiteSpace(filtro) ? null : JsonSerializer.Deserialize<FiltroPROCESO>(filtro);

    List<PROCESO> procesos = gestorProcesos.ConsultarProcesoConFiltro(filtroProceso, orden, page, rows, out int numeroElementosTotales);
    var listaDeProcesos = procesos.Select(s => new
    {
        s.IdProceso,
        Codigo = WebUtility.HtmlEncode(s.Codigo),
        Nombre = WebUtility.HtmlEncode(s.Nombre),
        TipoDeProceso = WebUtility.HtmlEncode(s.TIPO_DE_PROCESO!.Nombre)
    }).ToList();

    var respuesta = RespuestaGrid<object>.Crear(page, rows, numeroElementosTotales, listaDeProcesos.Cast<object>().ToList());
    return Results.Text(JsonSerializer.Serialize(respuesta, opcionesJson), "application/json");
});

app.Run();

/// <summary>Permite referenciar el programa desde pruebas.</summary>
public partial class Program;
