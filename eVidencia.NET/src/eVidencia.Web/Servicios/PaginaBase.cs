using System.Security;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace eVidencia.Web.Servicios;

/// <summary>
/// Comportamiento común de las páginas. Origen: eVidencias.Web/BasePage.cs y Util.cs.
/// La presentación (diálogo y aviso) es la del mockup responsive; los textos son los del original.
/// </summary>
public abstract class PaginaBase : PageModel
{
    /// <summary>
    /// Origen: BasePage.GestionarExcepciones (BasePage.cs:28-49). Las excepciones de negocio y de seguridad
    /// se muestran en un diálogo; el resto se relanza para que llegue a la página de error global.
    /// </summary>
    protected void GestionarExcepciones(Exception ex)
    {
        string tituloError;
        switch (ex)
        {
            case SecurityException:
                tituloError = "Error de seguridad";
                break;
            case ApplicationException:
                tituloError = "Se ha producido un error";
                break;
            default:
                System.Runtime.ExceptionServices.ExceptionDispatchInfo.Capture(ex).Throw();
                return;
        }

        ViewData["ScriptInicio"] = $"EV.alerta({Js(ex.Message)}, [], {Js(tituloError)});";
    }

    /// <summary>Origen: Util.Notificar (Util.cs:41-44). El mockup muestra solo el mensaje, bajo el rótulo "Notificación".</summary>
    protected void Notificar(string titulo, string mensaje)
    {
        ViewData["ScriptInicio"] = $"EV.notificar({Js(mensaje)});";
    }

    /// <summary>Muestra los errores de validación del formulario en un diálogo, como el mockup.</summary>
    protected void MostrarErroresDeValidacion(IEnumerable<string> errores)
    {
        ViewData["ScriptInicio"] = $"EV.errores({JsonSerializer.Serialize(errores)});";
    }

    /// <summary>Literal de JavaScript seguro para incrustar en la página.</summary>
    private static string Js(string texto) => JsonSerializer.Serialize(texto);
}
