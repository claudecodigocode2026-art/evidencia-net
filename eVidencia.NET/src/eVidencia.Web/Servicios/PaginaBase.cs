using System.Security;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace eVidencia.Web.Servicios;

/// <summary>
/// Comportamiento común de las páginas. Origen: eVidencias.Web/BasePage.cs y Util.cs.
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

        ViewData["ScriptInicio"] = string.Format("MostarExcepciones('{0}', '{1}');",
            JavaScriptEncoder.Default.Encode(tituloError), JavaScriptEncoder.Default.Encode(ex.Message));
    }

    /// <summary>Origen: Util.Notificar (Util.cs:41-44).</summary>
    protected void Notificar(string titulo, string mensaje)
    {
        ViewData["ScriptInicio"] = string.Format("setTimeout(function(){{Notificar('{0}','{1}')}}, 500);",
            JavaScriptEncoder.Default.Encode(titulo), JavaScriptEncoder.Default.Encode(mensaje));
    }
}
