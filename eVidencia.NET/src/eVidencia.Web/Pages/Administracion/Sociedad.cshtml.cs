using System.Text.RegularExpressions;
using eVidencia.BLL;
using eVidencia.Entidades;
using eVidencia.Web.Servicios;
using Microsoft.AspNetCore.Mvc;

namespace eVidencia.Web.Pages.Administracion;

/// <summary>
/// Alta y edición de una Sociedad. Origen: eVidencias.Web/Administracion/Sociedad.aspx.cs.
/// El estado que Web Forms guardaba en ViewState (Accion, IdSociedad) viaja aquí en campos ocultos del formulario.
/// </summary>
[IgnoreAntiforgeryToken] // El original (Web Forms) no usa token antifalsificación; pendiente de decidir en el proyecto.
public partial class SociedadModel(GestorSociedades gestorSociedades, GestorDatosMaestros gestorDatosMaestros) : PaginaBase
{
    public Accion Accion { get; private set; } = Accion.Crear;
    public int IdSociedad { get; private set; } = -1;

    // Valores de los campos del formulario
    public string CodigoEsif { get; private set; } = "";
    public string Nombre { get; private set; } = "";
    public string TipoDeEmpresa { get; private set; } = "-1";
    public string Pais { get; private set; } = "-1";
    public List<TIPO_DE_EMPRESA> TiposDeEmpresa { get; private set; } = [];
    public List<PAIS> Paises { get; private set; } = [];

    // Validación (equivalente a los validadores del grupo "ValidaSociedad" de Sociedad.aspx)
    public List<string> ErroresDeValidacion { get; } = [];
    public bool ErrorEnCodigoEsif { get; private set; }
    public bool ErrorEnNombre { get; private set; }
    public bool ErrorEnTipoDeEmpresa { get; private set; }
    public bool ErrorEnPais { get; private set; }

    /// <summary>Origen: Page_Load (Sociedad.aspx.cs:45-79).</summary>
    public void OnGet()
    {
        ViewData["Title"] = "eVidencia: Sociedad";

        string? parametroAccion = Parametro("Accion");

        if (string.IsNullOrEmpty(parametroAccion) || parametroAccion == "crear")
        {
            Accion = Accion.Crear;
        }
        else if (parametroAccion == "editar")
        {
            Accion = Accion.Editar;

            if (!int.TryParse(Parametro("IdSociedad"), out int parsedIdSociedad))
            {
                throw new ApplicationException(WebGlobal.ErrorFormatoParametroIdSociedadNoValido);
            }

            IdSociedad = parsedIdSociedad;
        }
        else
        {
            throw new ApplicationException(WebGlobal.ErrorAccionNoReconocida);
        }

        InicializarFormulario();
    }

    /// <summary>Origen: btnGuardar_Click (Sociedad.aspx.cs:140-184).</summary>
    public IActionResult OnPost()
    {
        ViewData["Title"] = "eVidencia: Sociedad";

        // Estado que el original guardaba en ViewState
        Accion = Request.Form["hdnAccion"] == nameof(Accion.Editar) ? Accion.Editar : Accion.Crear;
        IdSociedad = int.TryParse(Request.Form["hdnIdSociedad"], out int id) ? id : -1;

        CodigoEsif = Request.Form["txtCodEsif"].ToString();
        Nombre = Request.Form["txtNombre"].ToString();
        TipoDeEmpresa = Request.Form["ddTipoDeEmpresa"].ToString();
        Pais = Request.Form["ddPais"].ToString();
        CargarDesplegables();

        Validar();

        if (ErroresDeValidacion.Count > 0)
        {
            MostrarErroresDeValidacion(ErroresDeValidacion);
        }
        else
        {
            try
            {
                if (Accion == Accion.Crear)
                {
                    SOCIEDAD sociedad = new()
                    {
                        CodigoEsif = CodigoEsif,
                        Nombre = Nombre,
                        IdTipoDeEmpresa = int.Parse(TipoDeEmpresa),
                        IdPais = int.Parse(Pais)
                    };

                    IdSociedad = gestorSociedades.CrearSociedad(sociedad);
                    Notificar(WebGlobal.TituloCreacion, WebGlobal.Creacion);

                    Accion = Accion.Editar;
                    InicializarFormulario();
                }
                else
                {
                    SOCIEDAD sociedad = gestorSociedades.ConsultarSociedad(IdSociedad);

                    sociedad.CodigoEsif = CodigoEsif;
                    sociedad.Nombre = Nombre;
                    sociedad.IdTipoDeEmpresa = int.Parse(TipoDeEmpresa);
                    sociedad.IdPais = int.Parse(Pais);

                    gestorSociedades.ActualizarSociedad(sociedad);
                    Notificar(WebGlobal.TituloActualizacion, WebGlobal.Actualizacion);
                    InicializarFormulario();
                }
            }
            catch (Exception ex)
            {
                GestionarExcepciones(ex);
            }
        }

        return Page();
    }

    /// <summary>Origen: InicializarFormulario (Sociedad.aspx.cs:89-133).</summary>
    private void InicializarFormulario()
    {
        CargarDesplegables();

        if (Accion == Accion.Crear)
        {
            CodigoEsif = "";
            Nombre = "";
            TipoDeEmpresa = "-1";
            Pais = "-1";
        }
        else
        {
            SOCIEDAD sociedad = gestorSociedades.ConsultarSociedad(IdSociedad);

            CodigoEsif = sociedad.CodigoEsif ?? "";
            Nombre = sociedad.Nombre ?? "";
            TipoDeEmpresa = sociedad.IdTipoDeEmpresa.ToString();
            Pais = sociedad.IdPais.ToString();
        }
    }

    private void CargarDesplegables()
    {
        TiposDeEmpresa = gestorDatosMaestros.ConsultarTiposDeEmpresa();
        Paises = gestorDatosMaestros.ConsultarPaises();
    }

    /// <summary>
    /// Mismas reglas, mensajes y orden que los validadores de Sociedad.aspx (grupo "ValidaSociedad").
    /// Los textos son los del recurso en español (App_LocalResources/Sociedad.aspx.resx), que sustituyen
    /// a los escritos en la página.
    /// </summary>
    private void Validar()
    {
        if (CodigoEsif.Trim().Length == 0)
        {
            ErroresDeValidacion.Add("El campo Código eSif no puede estar vacío");
            ErrorEnCodigoEsif = true;
        }
        else if (!Hasta4().IsMatch(CodigoEsif))
        {
            ErroresDeValidacion.Add("El campo Código eSif puede tener 4 carácteres como máximo");
            ErrorEnCodigoEsif = true;
        }

        if (Nombre.Trim().Length == 0)
        {
            ErroresDeValidacion.Add("El campo Nombre no puede estar vacío");
            ErrorEnNombre = true;
        }
        else if (!Hasta50().IsMatch(Nombre))
        {
            ErroresDeValidacion.Add("El campo Nombre debe tener 50 caracteres como máximo");
            ErrorEnNombre = true;
        }

        // Tipo de empresa y país: obligatorios (valor inicial -1)
        if (TipoDeEmpresa.Trim() == "-1" || !TiposDeEmpresa.Any(t => t.IdTipoDeEmpresa.ToString() == TipoDeEmpresa))
        {
            ErroresDeValidacion.Add("El campo Tipo de Empresa no puede estar vacío");
            ErrorEnTipoDeEmpresa = true;
        }

        if (Pais.Trim() == "-1" || !Paises.Any(p => p.IdPais.ToString() == Pais))
        {
            ErroresDeValidacion.Add("El campo País no puede estar vacío");
            ErrorEnPais = true;
        }
    }

    /// <summary>Lee un parámetro de la petición sin distinguir mayúsculas en el nombre, como Request["..."].</summary>
    private string? Parametro(string nombre)
    {
        foreach (var par in Request.Query)
        {
            if (string.Equals(par.Key, nombre, StringComparison.OrdinalIgnoreCase)) return par.Value.ToString();
        }
        return null;
    }

    [GeneratedRegex(@"^[\s\S]{0,4}$")]
    private static partial Regex Hasta4();

    [GeneratedRegex(@"^[\s\S]{0,50}$")]
    private static partial Regex Hasta50();
}
