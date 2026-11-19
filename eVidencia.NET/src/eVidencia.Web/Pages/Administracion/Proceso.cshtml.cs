using System.Text.RegularExpressions;
using eVidencia.BLL;
using eVidencia.Entidades;
using eVidencia.Web.Servicios;
using Microsoft.AspNetCore.Mvc;

namespace eVidencia.Web.Pages.Administracion;

/// <summary>Origen: enumerado Accion de la web original.</summary>
public enum Accion
{
    Crear,
    Editar
}

/// <summary>
/// Alta y edición de un Proceso. Origen: eVidencias.Web/Administracion/Proceso.aspx.cs.
/// El estado que Web Forms guardaba en ViewState (Accion, IdProceso) viaja aquí en campos ocultos del formulario.
/// </summary>
[IgnoreAntiforgeryToken] // El original (Web Forms) no usa token antifalsificación; pendiente de decidir en el proyecto.
public partial class ProcesoModel(GestorProcesos gestorProcesos) : PaginaBase
{
    public Accion Accion { get; private set; } = Accion.Crear;
    public int IdProceso { get; private set; } = -1;

    // Valores de los campos del formulario
    public string Codigo { get; private set; } = "";
    public string Nombre { get; private set; } = "";
    public string TipoProceso { get; private set; } = "-1";
    public List<TIPO_DE_PROCESO> TiposDeProceso { get; private set; } = [];

    // Validación (equivalente a los validadores del grupo "ValidaProceso" de Proceso.aspx)
    public List<string> ErroresDeValidacion { get; } = [];
    public bool ErrorEnCodigo { get; private set; }
    public bool ErrorEnNombre { get; private set; }
    public bool ErrorEnTipoProceso { get; private set; }

    /// <summary>Origen: Page_Load (Proceso.aspx.cs:47-79).</summary>
    public void OnGet()
    {
        ViewData["Title"] = "eVidencia: Proceso";

        // Recogemos los parámetros de la página
        string? parametroAccion = Parametro("Accion");

        if (string.IsNullOrEmpty(parametroAccion) || parametroAccion == "crear")
        {
            Accion = Accion.Crear;
        }
        else if (parametroAccion == "editar")
        {
            Accion = Accion.Editar;

            if (!int.TryParse(Parametro("IdProceso"), out int parsedIdProceso))
            {
                throw new ApplicationException(WebGlobal.ErrorFormatoParametroIdProcesoNoValido);
            }

            IdProceso = parsedIdProceso;
        }
        else
        {
            throw new ApplicationException(WebGlobal.ErrorAccionNoReconocida);
        }

        InicializarFormulario();
    }

    /// <summary>Origen: btnGuardar_Click (Proceso.aspx.cs:135-180).</summary>
    public IActionResult OnPost()
    {
        ViewData["Title"] = "eVidencia: Proceso";

        // Estado que el original guardaba en ViewState
        Accion = Request.Form["hdnAccion"] == nameof(Accion.Editar) ? Accion.Editar : Accion.Crear;
        IdProceso = int.TryParse(Request.Form["hdnIdProceso"], out int id) ? id : -1;

        Codigo = Request.Form["txtCodigo"].ToString();
        Nombre = Request.Form["txtNombre"].ToString();
        TipoProceso = Request.Form["ddTipoProceso"].ToString();
        TiposDeProceso = gestorProcesos.ConsultarTiposProcesos();

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
                    PROCESO proceso = new()
                    {
                        Codigo = Codigo,
                        Nombre = Nombre,
                        IdTipoDeProceso = int.Parse(TipoProceso)
                    };

                    IdProceso = gestorProcesos.CrearProceso(proceso);
                    Notificar(WebGlobal.TituloCreacion, WebGlobal.Creacion);

                    Accion = Accion.Editar;
                    InicializarFormulario();
                }
                else
                {
                    PROCESO proceso = gestorProcesos.ConsultarProceso(IdProceso);

                    proceso.Codigo = Codigo;
                    proceso.Nombre = Nombre;
                    proceso.IdTipoDeProceso = int.Parse(TipoProceso);

                    gestorProcesos.ActualizarProceso(proceso);
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

    /// <summary>
    /// Inicializa los datos del formulario en función del elemento que se está mostrando y la acción.
    /// Origen: InicializarFormulario (Proceso.aspx.cs:89-127).
    /// </summary>
    private void InicializarFormulario()
    {
        // Inicializamos el desplegable de tipos de proceso
        TiposDeProceso = gestorProcesos.ConsultarTiposProcesos();

        if (Accion == Accion.Crear)
        {
            Codigo = "";
            Nombre = "";
            TipoProceso = "-1";
        }
        else
        {
            PROCESO proceso = gestorProcesos.ConsultarProceso(IdProceso);

            Codigo = proceso.Codigo ?? "";
            Nombre = proceso.Nombre ?? "";
            TipoProceso = proceso.IdTipoDeProceso.ToString();
        }
    }

    /// <summary>
    /// Mismas reglas, mensajes y orden que los validadores de Proceso.aspx
    /// (RequiredFieldValidator y RegularExpressionValidator del grupo "ValidaProceso").
    /// </summary>
    private void Validar()
    {
        // Código: obligatorio y 10 caracteres como máximo
        if (Codigo.Trim().Length == 0)
        {
            ErroresDeValidacion.Add("El campo Código es obligatorio");
            ErrorEnCodigo = true;
        }
        else if (!Hasta10().IsMatch(Codigo))
        {
            ErroresDeValidacion.Add("El campo Código eSif puede tener 10 carácteres como máximo");
            ErrorEnCodigo = true;
        }

        // Nombre: obligatorio y 250 caracteres como máximo
        if (Nombre.Trim().Length == 0)
        {
            ErroresDeValidacion.Add("El campo Nombre es obligatorio");
            ErrorEnNombre = true;
        }
        else if (!Hasta250().IsMatch(Nombre))
        {
            ErroresDeValidacion.Add("El campo Nombre debe tener 250 caracteres como máximo");
            ErrorEnNombre = true;
        }

        // Tipo de Proceso: obligatorio (valor inicial -1)
        if (TipoProceso.Trim() == "-1" || !TiposDeProceso.Any(t => t.IdTipoDeProceso.ToString() == TipoProceso))
        {
            ErroresDeValidacion.Add("El campo Tipo de Proceso es obligatorio");
            ErrorEnTipoProceso = true;
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

    [GeneratedRegex(@"^[\s\S]{0,10}$")]
    private static partial Regex Hasta10();

    [GeneratedRegex(@"^[\s\S]{0,250}$")]
    private static partial Regex Hasta250();
}
