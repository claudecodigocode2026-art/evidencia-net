using eVidencia.DAL;
using eVidencia.Entidades;

namespace eVidencia.BLL;

/// <summary>Origen: eVidencias.BLL/GestorLogDeEntidades.cs:17-22.</summary>
public enum TipoDeAccionLog
{
    Alta = 1,
    Baja = 2,
    Modificacion = 3
}

/// <summary>
/// Registro de auditoría de cambios en entidades (tabla LOG_DE_ENTIDADES).
/// Origen: eVidencias.BLL/GestorLogDeEntidades.cs (1.635 líneas); aquí solo lo que usan Proceso y Subproceso.
/// Los textos del registro son literales en el código original y se mantienen idénticos.
/// </summary>
public class GestorLogDeEntidades(IRepositorioLogDeEntidades logs, GestorUsuarios gestorUsuarios)
{
    private List<string> _listaDeCambiosLog = [];

    /// <summary>Origen: LogDeProceso (línea 670).</summary>
    public void LogDeProceso(TipoDeAccionLog accion, PROCESO? antiguo = null, PROCESO? nuevo = null)
    {
        _listaDeCambiosLog = [];

        PROCESO procesoDeReferencia = accion switch
        {
            TipoDeAccionLog.Modificacion or TipoDeAccionLog.Baja => antiguo!,
            _ => nuevo!
        };

        string descripcion;
        switch (accion)
        {
            case TipoDeAccionLog.Alta:
                GenerarListaDeCambiosEnProceso(antiguo, nuevo!);
                descripcion = string.Format("Se ha creado el Proceso con el nombre: {0}", procesoDeReferencia.Nombre);
                descripcion = descripcion + Environment.NewLine + string.Join(Environment.NewLine, _listaDeCambiosLog);
                break;

            case TipoDeAccionLog.Modificacion:
                GenerarListaDeCambiosEnProceso(antiguo, nuevo!);
                descripcion = string.Format("Se ha modificado el Proceso con el nombre: {0}", procesoDeReferencia.Nombre);
                descripcion = descripcion + Environment.NewLine + string.Join(Environment.NewLine, _listaDeCambiosLog);
                break;

            case TipoDeAccionLog.Baja:
                descripcion = string.Format("Se ha eliminado el Proceso con el nombre: {0}", procesoDeReferencia.Nombre);
                break;

            default:
                descripcion = "";
                break;
        }

        LogDeEntidad(accion, descripcion, idProceso: procesoDeReferencia.IdProceso, idSubproceso: null);
    }

    /// <summary>Origen: LogDeSubproceso (línea 739).</summary>
    public void LogDeSubproceso(TipoDeAccionLog accion, SUBPROCESO? antiguo = null, SUBPROCESO? nuevo = null)
    {
        _listaDeCambiosLog = [];

        SUBPROCESO subprocesoDeReferencia = accion switch
        {
            TipoDeAccionLog.Modificacion or TipoDeAccionLog.Baja => antiguo!,
            _ => nuevo!
        };

        string descripcion;
        switch (accion)
        {
            case TipoDeAccionLog.Alta:
                GenerarListaDeCambiosEnSubproceso(antiguo, nuevo!);
                descripcion = string.Format("Se ha creado el Subproceso con el nombre: {0}", subprocesoDeReferencia.Nombre);
                descripcion = descripcion + Environment.NewLine + string.Join(Environment.NewLine, _listaDeCambiosLog);
                break;

            case TipoDeAccionLog.Modificacion:
                GenerarListaDeCambiosEnSubproceso(antiguo, nuevo!);
                descripcion = string.Format("Se ha modificado el Subproceso con el nombre: {0}", subprocesoDeReferencia.Nombre);
                descripcion = descripcion + Environment.NewLine + string.Join(Environment.NewLine, _listaDeCambiosLog);
                break;

            case TipoDeAccionLog.Baja:
                descripcion = string.Format("Se ha eliminado el Subproceso con el nombre: {0}", subprocesoDeReferencia.Nombre);
                break;

            default:
                descripcion = "";
                break;
        }

        LogDeEntidad(accion, descripcion, idProceso: subprocesoDeReferencia.IdProceso, idSubproceso: subprocesoDeReferencia.IdSubproceso);
    }

    /// <summary>Origen: LogDeEntidad (línea 449).</summary>
    private void LogDeEntidad(TipoDeAccionLog accion, string descripcion, int? idProceso, int? idSubproceso)
    {
        USUARIO usuarioAutenticado = gestorUsuarios.ConsultarUsuarioAutenticado();

        logs.Insertar(new LOG_DE_ENTIDADES
        {
            IdTipoDeAccion = (int)accion,
            Descripcion = descripcion,
            Fecha = DateTime.Now,
            IdUsuario = usuarioAutenticado.IdUsuario,
            IdSociedad = null,
            IdPeriodo = null,
            IdProceso = idProceso,
            IdPlanificadorDeEvidencias = null,
            IdActividadDeControl = null,
            IdSubproceso = idSubproceso,
            IdAreaResponsable = null
        });
    }

    /// <summary>Origen: GenerarListaDeCambiosEnProceso (línea 1369).</summary>
    private void GenerarListaDeCambiosEnProceso(PROCESO? antiguo, PROCESO nuevo)
    {
        antiguo ??= new PROCESO();

        DescribirCambio("Código", antiguo.Codigo, nuevo.Codigo);
        DescribirCambio("Nombre", antiguo.Nombre, nuevo.Nombre);
        DescribirCambio("Tipo de Proceso",
            antiguo.TIPO_DE_PROCESO == null ? "" : antiguo.TIPO_DE_PROCESO.Nombre,
            nuevo.TIPO_DE_PROCESO == null ? "" : nuevo.TIPO_DE_PROCESO.Nombre);
    }

    /// <summary>Origen: GenerarListaDeCambiosEnSubproceso (línea 1387).</summary>
    private void GenerarListaDeCambiosEnSubproceso(SUBPROCESO? antiguo, SUBPROCESO nuevo)
    {
        antiguo ??= new SUBPROCESO();

        DescribirCambio("Código", antiguo.Codigo, nuevo.Codigo);
        DescribirCambio("Nombre", antiguo.Nombre, nuevo.Nombre);
        DescribirCambio("Proceso",
            antiguo.PROCESO == null ? "" : antiguo.PROCESO.Nombre,
            nuevo.PROCESO == null ? "" : nuevo.PROCESO.Nombre);
    }

    /// <summary>Origen: DescribirCambio (línea 1546).</summary>
    private void DescribirCambio(string nombreDelCampo, string? antiguo, string? nuevo)
    {
        if (antiguo != nuevo)
        {
            _listaDeCambiosLog.Add(string.Format("\n *[Campo {0}:]* {1}", nombreDelCampo, FormatearCambio(antiguo, nuevo)));
        }
    }

    /// <summary>Origen: FormatearCambio (línea 1563).</summary>
    private static string FormatearCambio(string? antiguo, string? nuevo)
    {
        if (antiguo != null && nuevo != null)
        {
            return string.Format("Valor antiguo: {0} - Valor nuevo: {1} ", antiguo, nuevo);
        }
        else if (nuevo != null)
        {
            return string.Format("Valor nuevo: {0} ", nuevo);
        }
        else if (antiguo != null)
        {
            return string.Format("Valor antiguo: {0} ", antiguo);
        }
        else
        {
            return "No hay valores.";
        }
    }
}
