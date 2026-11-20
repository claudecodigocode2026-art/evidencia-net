using eVidencia.BLL;
using eVidencia.Entidades;
using eVidencia.Web.Servicios;
using Microsoft.AspNetCore.Mvc;

namespace eVidencia.Web.Pages.Administracion;

/// <summary>
/// Listado de Sociedades. Origen: eVidencias.Web/Administracion/Sociedades.aspx.cs.
/// </summary>
[IgnoreAntiforgeryToken] // El original (Web Forms) no usa token antifalsificación; pendiente de decidir en el proyecto.
public class SociedadesModel(GestorSociedades gestorSociedades, GestorDatosMaestros gestorDatosMaestros) : PaginaBase
{
    /// <summary>Desplegables de filtro. Origen: InicializarFormulario (Sociedades.aspx.cs:32-49).</summary>
    public List<TIPO_DE_EMPRESA> TiposDeEmpresa { get; private set; } = [];
    public List<PAIS> Paises { get; private set; } = [];

    /// <summary>True si se llega desde esta página o desde la de detalle (se recupera la configuración del listado).</summary>
    public bool EsVolver
    {
        get
        {
            string? referente = Request.Headers.Referer;
            if (string.IsNullOrEmpty(referente) || !Uri.TryCreate(referente, UriKind.Absolute, out Uri? uri)) return false;
            return uri.AbsolutePath == "/Administracion/Sociedades.aspx" || uri.AbsolutePath == "/Administracion/Sociedad.aspx";
        }
    }

    public void OnGet()
    {
        InicializarFormulario();
    }

    /// <summary>Origen: btnEliminar_Click (Sociedades.aspx.cs:51-67).</summary>
    public IActionResult OnPostEliminar(string? hdnId)
    {
        try
        {
            if (!string.IsNullOrEmpty(hdnId))
            {
                gestorSociedades.EliminarSociedad(int.Parse(hdnId));
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
        ViewData["Title"] = "eVidencia: Sociedades";
        TiposDeEmpresa = gestorDatosMaestros.ConsultarTiposDeEmpresa();
        Paises = gestorDatosMaestros.ConsultarPaises();
    }
}
