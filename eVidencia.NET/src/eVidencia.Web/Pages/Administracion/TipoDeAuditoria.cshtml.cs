using System.Text.RegularExpressions;
using eVidencia.BLL;
using eVidencia.Entidades;
using eVidencia.Web.Servicios;
using Microsoft.AspNetCore.Mvc;

namespace eVidencia.Web.Pages.Administracion;

/// <summary>
/// Alta y edición de un Tipo de Auditoría. Origen: eVidencias.Web/Administracion/TipoDeAuditoria.aspx.cs.
/// El estado que Web Forms guardaba en ViewState (Accion, IdTipoDeAuditoria) viaja aquí en campos ocultos del formulario.
/// </summary>
[IgnoreAntiforgeryToken] // El original (Web Forms) no usa token antifalsificación; pendiente de decidir en el proyecto.
public partial class TipoDeAuditoriaModel(GestorTiposDeAuditoria gestorTiposDeAuditoria) : PaginaBase
{
    public Accion Accion { get; private set; } = Accion.Crear;
    public int IdTipoDeAuditoria { get; private set; } = -1;

    // Valores de los campos del formulario
    public string Codigo { get; private set; } = "";
    public string Descripcion { get; private set; } = "";

    // Validación (equivalente a los validadores del grupo "ValidaTipoAuditoria" de TipoDeAuditoria.aspx)
    public List<string> ErroresDeValidacion { get; } = [];
    public bool ErrorEnCodigo { get; private set; }
    public bool ErrorEnDescripcion { get; private set; }

    /// <summary>Origen: Page_Load (TipoDeAuditoria.aspx.cs:44-75).</summary>
    public void OnGet()
    {
        ViewData["Title"] = "eVidencia: Tipo de Auditoría";

        string? parametroAccion = Parametro("Accion");

        if (string.IsNullOrEmpty(parametroAccion) || parametroAccion == "crear")
        {
            Accion = Accion.Crear;
        }
        else if (parametroAccion == "editar")
        {
            Accion = Accion.Editar;

            if (!int.TryParse(Parametro("IdTipoDeAuditoria"), out int parsedIdTipoDeAuditoria))
            {
                throw new ApplicationException(WebGlobal.ErrorFormatoParametroIdTipoAuditoriaNoValido);
            }

            IdTipoDeAuditoria = parsedIdTipoDeAuditoria;
        }
        else
        {
            throw new ApplicationException(WebGlobal.ErrorAccionNoReconocida);
        }

        InicializarFormulario();
    }

    /// <summary>Origen: btnGuardar_Click (TipoDeAuditoria.aspx.cs:124-168).</summary>
    public IActionResult OnPost()
    {
        ViewData["Title"] = "eVidencia: Tipo de Auditoría";

        // Estado que el original guardaba en ViewState
        Accion = Request.Form["hdnAccion"] == nameof(Accion.Editar) ? Accion.Editar : Accion.Crear;
        IdTipoDeAuditoria = int.TryParse(Request.Form["hdnIdTipoDeAuditoria"], out int id) ? id : -1;

        Codigo = Request.Form["txtCodigo"].ToString();
        Descripcion = Request.Form["txtDescripcion"].ToString();

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
                    TIPO_DE_AUDITORIA tipoAuditoria = new()
                    {
                        Codigo = Codigo,
                        Descripcion = Descripcion
                    };

                    IdTipoDeAuditoria = gestorTiposDeAuditoria.CrearTipoDeAuditoria(tipoAuditoria);
                    Notificar(WebGlobal.TituloCreacion, WebGlobal.Creacion);

                    Accion = Accion.Editar;
                    InicializarFormulario();
                }
                else
                {
                    TIPO_DE_AUDITORIA tipoAuditoria = gestorTiposDeAuditoria.ConsultarTipoDeAuditoria(IdTipoDeAuditoria);

                    tipoAuditoria.Codigo = Codigo;
                    tipoAuditoria.Descripcion = Descripcion;

                    gestorTiposDeAuditoria.ActualizarTipoDeAuditoria(tipoAuditoria);
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

    /// <summary>Origen: InicializarFormulario (TipoDeAuditoria.aspx.cs:84-110).</summary>
    private void InicializarFormulario()
    {
        if (Accion == Accion.Crear)
        {
            Codigo = "";
            Descripcion = "";
        }
        else
        {
            TIPO_DE_AUDITORIA tipoAuditoria = gestorTiposDeAuditoria.ConsultarTipoDeAuditoria(IdTipoDeAuditoria);

            Codigo = tipoAuditoria.Codigo ?? "";
            Descripcion = tipoAuditoria.Descripcion ?? "";
        }
    }

    /// <summary>
    /// Mismas reglas, mensajes y orden que los validadores de TipoDeAuditoria.aspx (grupo "ValidaTipoAuditoria").
    /// El código debe tener exactamente 2 caracteres (expresión "^[\s\S]{2,2}$" del original), aunque el
    /// mensaje del original diga "2 carácteres como máximo".
    /// </summary>
    private void Validar()
    {
        if (Codigo.Trim().Length == 0)
        {
            ErroresDeValidacion.Add("El campo Código es obligatorio");
            ErrorEnCodigo = true;
        }
        else if (!Exactamente2().IsMatch(Codigo))
        {
            ErroresDeValidacion.Add("El campo Código debe tener 2 carácteres como máximo");
            ErrorEnCodigo = true;
        }

        if (Descripcion.Trim().Length == 0)
        {
            ErroresDeValidacion.Add("El campo Descripción es obligatorio");
            ErrorEnDescripcion = true;
        }
        else if (!Hasta250().IsMatch(Descripcion))
        {
            ErroresDeValidacion.Add("El campo Descripción debe tener 250 caracteres como máximo");
            ErrorEnDescripcion = true;
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

    [GeneratedRegex(@"^[\s\S]{2,2}$")]
    private static partial Regex Exactamente2();

    [GeneratedRegex(@"^[\s\S]{0,250}$")]
    private static partial Regex Hasta250();
}
