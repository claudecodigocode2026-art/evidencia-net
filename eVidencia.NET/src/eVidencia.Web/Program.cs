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
//   Datos:Modo            "Memoria" (sin base de datos) o "SqlServer"
//   Datos:CadenaConexion  cadena de conexión cuando el modo es "SqlServer"
//   Demo:UsuarioFijo      UID del usuario con el que entra la demo (debe existir en la tabla USUARIO)
// ---------------------------------------------------------------------------------------------
string modoDatos = builder.Configuration["Datos:Modo"] ?? "Memoria";
string usuarioFijo = builder.Configuration["Demo:UsuarioFijo"] ?? DatosDeEjemplo.UidUsuarioDemo;

builder.Services.AddRazorPages();

// Las páginas escriben los caracteres acentuados tal cual, como hacía Web Forms.
builder.Services.Configure<Microsoft.Extensions.WebEncoders.WebEncoderOptions>(o =>
    o.TextEncoderSettings = new System.Text.Encodings.Web.TextEncoderSettings(System.Text.Unicode.UnicodeRanges.All));

// Usuario fijo: sustituye, solo en la demo, a la autenticación del original (Login.aspx + LogonUser).
builder.Services.AddSingleton<IUsuarioActual>(new UsuarioFijo(usuarioFijo));

if (modoDatos.Equals("SqlServer", StringComparison.OrdinalIgnoreCase))
{
#if SIN_SQLSERVER
    throw new InvalidOperationException("Esta compilación no incluye el proveedor de SQL Server (se compiló con SinSqlServer=true).");
#else
    string cadena = builder.Configuration["Datos:CadenaConexion"]
        ?? throw new InvalidOperationException("Falta Datos:CadenaConexion en appsettings.json.");
    builder.Services.AddSingleton<IFabricaConexiones>(
        new FabricaConexiones(Microsoft.Data.SqlClient.SqlClientFactory.Instance, cadena));
    builder.Services.AddScoped<IRepositorioProcesos, RepositorioProcesosSql>();
    builder.Services.AddScoped<IRepositorioSubprocesos, RepositorioSubprocesosSql>();
    builder.Services.AddScoped<IRepositorioUsuarios, RepositorioUsuariosSql>();
    builder.Services.AddScoped<IRepositorioLogDeEntidades, RepositorioLogDeEntidadesSql>();
    builder.Services.AddSingleton(new InfoDemo(usuarioFijo, "SQL Server"));
#endif
}
else if (modoDatos.Equals("Memoria", StringComparison.OrdinalIgnoreCase))
{
    builder.Services.AddSingleton<AlmacenMemoria>();
    builder.Services.AddScoped<IRepositorioProcesos, RepositorioProcesosMemoria>();
    builder.Services.AddScoped<IRepositorioSubprocesos, RepositorioSubprocesosMemoria>();
    builder.Services.AddScoped<IRepositorioUsuarios, RepositorioUsuariosMemoria>();
    builder.Services.AddScoped<IRepositorioLogDeEntidades, RepositorioLogDeEntidadesMemoria>();
    builder.Services.AddSingleton(new InfoDemo(usuarioFijo, "de ejemplo en memoria (se pierden al parar la aplicación)"));
}
else
{
    throw new InvalidOperationException($"Datos:Modo no reconocido: '{modoDatos}'. Valores válidos: Memoria, SqlServer.");
}

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
