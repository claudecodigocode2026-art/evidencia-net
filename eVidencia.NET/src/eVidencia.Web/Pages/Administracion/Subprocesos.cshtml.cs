using eVidencia.BLL;
using eVidencia.Entidades;
using eVidencia.Web.Servicios;
using Microsoft.AspNetCore.Mvc;

namespace eVidencia.Web.Pages.Administracion;

/// <summary>
/// Listado de Subprocesos. Origen: eVidencias.Web/Administracion/Subprocesos.aspx.cs.
/// </summary>
[IgnoreAntiforgeryToken] // El original (Web Forms) no usa token antifalsificación; pendiente de decidir en el proyecto.
public class SubprocesosModel(GestorSubprocesos gestorSubprocesos, GestorProcesos gestorProcesos) : PaginaBase
{
    /// <summary>Procesos del desplegable de filtro. Origen: InicializarFormulario (Subprocesos.aspx.cs:28-38).</summary>
    public List<PROCESO> ProcesosDelFiltro { get; private set; } = [];

    /// <summary>True si se llega desde esta página o desde la de detalle (se recupera la configuración del listado).</summary>
    public bool EsVolver
    {
        get
        {
            string? referente = Request.Headers.Referer;
            if (string.IsNullOrEmpty(referente) || !Uri.TryCreate(referente, UriKind.Absolute, out Uri? uri)) return false;
            return uri.AbsolutePath == "/Administracion/Subprocesos.aspx" || uri.AbsolutePath == "/Administracion/Subproceso.aspx";
        }
    }

    public void OnGet()
    {
        InicializarFormulario();
    }

    /// <summary>Origen: btnEliminar_Click (Subprocesos.aspx.cs:41-57).</summary>
    public IActionResult OnPostEliminar(string? hdnId)
    {
        try
        {
            if (!string.IsNullOrEmpty(hdnId))
            {
                gestorSubprocesos.EliminarSubproceso(int.Parse(hdnId));
                Notificar(WebGlobal.TituloEliminacion, WebGlobal.Eliminacion);
            }
        }
        catch (Exception ex)
        {
            GestionarExcepciones(ex);
        }
        InicializarFormulario();
        return Page();
    }

    private void InicializarFormulario()
    {
        ViewData["Title"] = "eVidencia: Subprocesos";
        ProcesosDelFiltro = gestorProcesos.ConsultarProcesosParaSubprocesos();
    }
}
