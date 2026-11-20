using System.Security;
using eVidencia.DAL;
using eVidencia.Entidades;

namespace eVidencia.BLL;

/// <summary>
/// Catálogos de la aplicación. Origen: eVidencias.BLL/GestorDatosMaestros.cs.
/// Se migran los que usan las pantallas de Sociedades: tipos de empresa y países.
/// </summary>
public class GestorDatosMaestros(IRepositorioSociedades sociedades, GestorUsuarios gestorUsuarios)
{
    /// <summary>
    /// Tipos de empresa con el nombre traducido, ordenados por nombre.
    /// Origen: ConsultarTiposDeEmpresa (línea 351).
    /// </summary>
    public List<TIPO_DE_EMPRESA> ConsultarTiposDeEmpresa()
    {
        ExigirConsulta();
        List<TIPO_DE_EMPRESA> tipos = sociedades.ObtenerTiposDeEmpresa();
        tipos.ForEach(t => t.Nombre = Traducir("TipoDeEmpresa", t.IdTipoDeEmpresa, t.Nombre));
        return tipos.OrderBy(t => t.Nombre, StringComparer.Create(new System.Globalization.CultureInfo("es-ES"), ignoreCase: true)).ToList();
    }

    /// <summary>Países ordenados por nombre. Origen: ConsultarPaises (línea 143).</summary>
    public List<PAIS> ConsultarPaises()
    {
        ExigirConsulta();
        return sociedades.ObtenerPaises();
    }

    /// <summary>
    /// Texto de un valor de catálogo en el idioma de la aplicación (solo español en la demo).
    /// Origen: GestorDatosMaestros.Traducir, que busca la clave "{catálogo}_{id}" en DatosMaestros.resx.
    /// Si no hay traducción se devuelve el nombre guardado en la tabla.
    /// </summary>
    public static string Traducir(string catalogo, int id, string nombreEnTabla) => (catalogo, id) switch
    {
        ("TipoDeEmpresa", 1) => DatosMaestros.TipoDeEmpresa_1,
        ("TipoDeEmpresa", 2) => DatosMaestros.TipoDeEmpresa_2,
        ("TipoDeEmpresa", 3) => DatosMaestros.TipoDeEmpresa_3,
        _ => nombreEnTabla
    };

    /// <summary>Origen: AccionesPermitidas (línea 373): cualquier usuario autenticado puede consultar los catálogos.</summary>
    private void ExigirConsulta()
    {
        USUARIO? usuario = gestorUsuarios.ConsultarUsuarioAutenticado();
        if (usuario == null)
        {
            throw new SecurityException(string.Format(DatosMaestros.UsuarioSinPermisosParaRealizarAccion, gestorUsuarios.UsuarioActual));
        }
    }
}
