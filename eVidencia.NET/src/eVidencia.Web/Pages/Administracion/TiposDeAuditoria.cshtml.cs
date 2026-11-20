using eVidencia.BLL;
using eVidencia.Web.Servicios;
using Microsoft.AspNetCore.Mvc;

namespace eVidencia.Web.Pages.Administracion;

/// <summary>
/// Listado de Tipos de Auditoría. Origen: eVidencias.Web/Administracion/TiposDeAuditoria.aspx.cs.
/// </summary>
[IgnoreAntiforgeryToken] // El original (Web Forms) no usa token antifalsificación; pendiente de decidir en el proyecto.
public class TiposDeAuditoriaModel(GestorTiposDeAuditoria gestorTiposDeAuditoria) : PaginaBase
{
    /// <summary>True si se llega desde esta página o desde la de detalle (se recupera la configuración del listado).</summary>
    public bool EsVolver
    {
        get
        {
            string? referente = Request.Headers.Referer;
            if (string.IsNullOrEmpty(referente) || !Uri.TryCreate(referente, UriKind.Absolute, out Uri? uri)) return false;
            return uri.AbsolutePath == "/Administracion/TiposDeAuditoria.aspx" || uri.AbsolutePath == "/Administracion/TipoDeAuditoria.aspx";
        }
    }

    public void OnGet()
    {
        ViewData["Title"] = "eVidencia: Tipos de Auditorías";
    }

    /// <summary>Origen: btnEliminar_Click (TiposDeAuditoria.aspx.cs).</summary>
    public IActionResult OnPostEliminar(string? hdnId)
    {
        ViewData["Title"] = "eVidencia: Tipos de Auditorías";
        try
        {
            if (!string.IsNullOrEmpty(hdnId))
            {
                gestorTiposDeAuditoria.EliminarTipoDeAuditoria(int.Parse(hdnId));
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
