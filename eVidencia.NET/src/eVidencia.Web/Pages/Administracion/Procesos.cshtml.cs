using eVidencia.BLL;
using eVidencia.Web.Servicios;
using Microsoft.AspNetCore.Mvc;

namespace eVidencia.Web.Pages.Administracion;

/// <summary>
/// Listado de Procesos. Origen: eVidencias.Web/Administracion/Procesos.aspx.cs.
/// </summary>
[IgnoreAntiforgeryToken] // El original (Web Forms) no usa token antifalsificación; pendiente de decidir en el proyecto.
public class ProcesosModel(GestorProcesos gestorProcesos) : PaginaBase
{
    /// <summary>
    /// True si se llega desde esta página o desde la de detalle (se recupera la configuración del listado).
    /// Origen: expresión EsVolver de Procesos.aspx (líneas 11-13).
    /// </summary>
    public bool EsVolver
    {
        get
        {
            string? referente = Request.Headers.Referer;
            if (string.IsNullOrEmpty(referente) || !Uri.TryCreate(referente, UriKind.Absolute, out Uri? uri)) return false;
            return uri.AbsolutePath == "/Administracion/Procesos.aspx" || uri.AbsolutePath == "/Administracion/Proceso.aspx";
        }
    }

    public void OnGet()
    {
        ViewData["Title"] = "eVidencia: Procesos";
    }

    /// <summary>Origen: btnEliminar_Click (Procesos.aspx.cs:33-49).</summary>
    public IActionResult OnPostEliminar(string? hdnId)
    {
        ViewData["Title"] = "eVidencia: Procesos";
        try
        {
            if (!string.IsNullOrEmpty(hdnId))
            {
                gestorProcesos.EliminarProceso(int.Parse(hdnId));
                Notificar(WebGlobal.TituloEliminacion, WebGlobal.Eliminacion);
            }
        }
        catch (Exception ex)
        {
            GestionarExcepciones(ex);
        }
        return Page();
    }
}
