using System.Text.RegularExpressions;
using eVidencia.BLL;
using eVidencia.Entidades;
using eVidencia.Web.Servicios;
using Microsoft.AspNetCore.Mvc;

namespace eVidencia.Web.Pages.Administracion;

/// <summary>
/// Alta y edición de un Subproceso. Origen: eVidencias.Web/Administracion/Subproceso.aspx.cs.
/// El estado que Web Forms guardaba en ViewState (Accion, IdSubproceso) viaja aquí en campos ocultos del formulario.
/// </summary>
[IgnoreAntiforgeryToken] // El original (Web Forms) no usa token antifalsificación; pendiente de decidir en el proyecto.
public partial class SubprocesoModel(GestorSubprocesos gestorSubprocesos, GestorProcesos gestorProcesos) : PaginaBase
{
    public Accion Accion { get; private set; } = Accion.Crear;
    public int IdSubproceso { get; private set; } = -1;

    // Valores de los campos del formulario
    public string Codigo { get; private set; } = "";
    public string Nombre { get; private set; } = "";
    public string Proceso { get; private set; } = "-1";
    public List<PROCESO> Procesos { get; private set; } = [];

    // Validación (equivalente a los validadores del grupo "ValidaProceso" de Subproceso.aspx)
    public List<string> ErroresDeValidacion { get; } = [];
    public bool ErrorEnCodigo { get; private set; }
    public bool ErrorEnNombre { get; private set; }
    public bool ErrorEnProceso { get; private set; }

    /// <summary>Origen: Page_Load (Subproceso.aspx.cs:47-79).</summary>
    public void OnGet()
    {
        ViewData["Title"] = "eVidencia: Subproceso";

        string? parametroAccion = Parametro("Accion");

        if (string.IsNullOrEmpty(parametroAccion) || parametroAccion == "crear")
        {
            Accion = Accion.Crear;
        }
        else if (parametroAccion == "editar")
        {
            Accion = Accion.Editar;

            if (!int.TryParse(Parametro("IdSubproceso"), out int parsedIdSubproceso))
            {
                // El original reutiliza aquí el texto del parámetro IdProceso (Subproceso.aspx.cs:66).
                throw new ApplicationException(WebGlobal.ErrorFormatoParametroIdProcesoNoValido);
            }

            IdSubproceso = parsedIdSubproceso;
        }
        else
        {
            throw new ApplicationException(WebGlobal.ErrorAccionNoReconocida);
        }

        InicializarFormulario();
    }

    /// <summary>Origen: btnGuardar_Click (Subproceso.aspx.cs:128-168).</summary>
    public IActionResult OnPost()
    {
        ViewData["Title"] = "eVidencia: Subproceso";

        // Estado que el original guardaba en ViewState
        Accion = Request.Form["hdnAccion"] == nameof(Accion.Editar) ? Accion.Editar : Accion.Crear;
        IdSubproceso = int.TryParse(Request.Form["hdnIdSubproceso"], out int id) ? id : -1;

        Codigo = Request.Form["txtCodigo"].ToString();
        Nombre = Request.Form["txtNombre"].ToString();
        Proceso = Request.Form["ddProceso"].ToString();
        Procesos = gestorProcesos.ConsultarProcesos();

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
                    SUBPROCESO subproceso = new()
                    {
                        Codigo = Codigo,
                        Nombre = Nombre,
                        IdProceso = int.Parse(Proceso)
                    };

                    IdSubproceso = gestorSubprocesos.CrearSubproceso(subproceso);
                    Notificar(WebGlobal.TituloCreacion, WebGlobal.Creacion);

                    Accion = Accion.Editar;
                    InicializarFormulario();
                }
                else
                {
                    SUBPROCESO subproceso = gestorSubprocesos.ConsultarSubproceso(IdSubproceso);

                    subproceso.Codigo = Codigo;
                    subproceso.Nombre = Nombre;
                    subproceso.IdProceso = int.Parse(Proceso);

                    gestorSubprocesos.ActualizarSubproceso(subproceso);
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

    /// <summary>Origen: InicializarFormulario (Subproceso.aspx.cs:86-121).</summary>
    private void InicializarFormulario()
    {
        // Desplegable de procesos: los no borrados, ordenados por código
        Procesos = gestorProcesos.ConsultarProcesos();

        if (Accion == Accion.Crear)
        {
            Codigo = "";
            Nombre = "";
            Proceso = "-1";
        }
        else
        {
            SUBPROCESO subproceso = gestorSubprocesos.ConsultarSubproceso(IdSubproceso);

            Codigo = subproceso.Codigo ?? "";
            Nombre = subproceso.Nombre ?? "";
            Proceso = subproceso.IdProceso.ToString();
        }
    }

    /// <summary>
    /// Mismas reglas, mensajes y orden que los validadores de Subproceso.aspx
    /// (RequiredFieldValidator y RegularExpressionValidator del grupo "ValidaProceso").
    /// </summary>
    private void Validar()
    {
        if (Codigo.Trim().Length == 0)
        {
            ErroresDeValidacion.Add("El campo Código es obligatorio");
            ErrorEnCodigo = true;
        }
        else if (!Hasta10().IsMatch(Codigo))
        {
            ErroresDeValidacion.Add("El campo Código puede tener 10 carácteres como máximo");
            ErrorEnCodigo = true;
        }

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

        // Proceso: obligatorio (valor inicial -1)
        if (Proceso.Trim() == "-1" || !Procesos.Any(p => p.IdProceso.ToString() == Proceso))
        {
            ErroresDeValidacion.Add("El campo de Proceso es obligatorio");
            ErrorEnProceso = true;
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
