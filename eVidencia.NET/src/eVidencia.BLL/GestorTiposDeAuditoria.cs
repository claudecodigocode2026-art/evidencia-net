using System.Security;
using eVidencia.DAL;
using eVidencia.Entidades;
using eVidencia.Entidades.Filtros;

namespace eVidencia.BLL;

/// <summary>
/// Lógica de negocio de los tipos de auditoría.
/// Origen: eVidencias.BLL/GestorTiposDeAuditoria.cs (406 líneas). Se migran los métodos que usan las pantallas
/// TiposDeAuditoria.aspx y TipoDeAuditoria.aspx.
/// Pendientes: las consultas que usan otras pantallas (líneas 186-228 y 355-388) y la exportación a Excel.
/// </summary>
public class GestorTiposDeAuditoria(
    IRepositorioTiposDeAuditoria tiposDeAuditoria,
    GestorUsuarios gestorUsuarios,
    GestorLogDeEntidades gestorLog)
{
    public enum AccionTiposDeAuditoria
    {
        CrearTipoDeAuditoria,
        ActualizarTipoDeAuditoria,
        EliminarTipoDeAuditoria,
        ConsultarTipoDeAuditoria
    }

    /// <summary>
    /// El Administrador puede todo; Auditor, Responsable de Auditoría, Responsable Empresa y Administrador País
    /// solo consultan; Auditor Externo, Responsable de Control y Responsable de Seguimiento de Área no pueden nada.
    /// Origen: AccionesPermitidas (línea 230).
    /// </summary>
    public List<AccionTiposDeAuditoria> AccionesPermitidas()
    {
        List<AccionTiposDeAuditoria> acciones = [];
        USUARIO usuario = gestorUsuarios.ConsultarUsuarioAutenticado();
        switch (usuario.IdPerfilDeUsuario)
        {
            case (int)PerfilDeUsuario.Administrador:
                acciones.Add(AccionTiposDeAuditoria.CrearTipoDeAuditoria);
                acciones.Add(AccionTiposDeAuditoria.ActualizarTipoDeAuditoria);
                acciones.Add(AccionTiposDeAuditoria.EliminarTipoDeAuditoria);
                acciones.Add(AccionTiposDeAuditoria.ConsultarTipoDeAuditoria);
                break;
            case (int)PerfilDeUsuario.Auditor:
            case (int)PerfilDeUsuario.ResponsableDeAuditoria:
            case (int)PerfilDeUsuario.ResponsableEmpresa:
            case (int)PerfilDeUsuario.AdministradorPais:
                acciones.Add(AccionTiposDeAuditoria.ConsultarTipoDeAuditoria);
                break;
            case (int)PerfilDeUsuario.AuditorExterno:
            case (int)PerfilDeUsuario.ResponsableControl:
            case (int)PerfilDeUsuario.ResponsableSeguimientoArea:
                break;
        }
        return acciones;
    }

    /// <summary>Listado paginado de tipos de auditoría no borrados. Origen: ConsultarTipoAuditoriaConFiltro paginado (línea 281).</summary>
    public List<TIPO_DE_AUDITORIA> ConsultarTipoAuditoriaConFiltro(FiltroTIPO_DE_AUDITORIA? filtro, string orden, int numeroPagina, int numeroElementosPorPagina, out int numeroElementosTotales)
    {
        ExigirAccion(AccionTiposDeAuditoria.ConsultarTipoDeAuditoria);
        return tiposDeAuditoria.ObtenerFiltrados(filtro, orden, numeroPagina, numeroElementosPorPagina, out numeroElementosTotales);
    }

    /// <summary>Origen: ConsultarTipoDeAuditoria (línea 159).</summary>
    public TIPO_DE_AUDITORIA ConsultarTipoDeAuditoria(int idTipoDeAuditoria)
    {
        ExigirAccion(AccionTiposDeAuditoria.ConsultarTipoDeAuditoria);
        return tiposDeAuditoria.ObtenerPorId(idTipoDeAuditoria)
            ?? throw new ApplicationException(DatosMaestros.TipoAuditoriasNoExisteTipoAuditoria);
    }

    /// <summary>Origen: CrearTipoDeAuditoria (línea 42).</summary>
    public int CrearTipoDeAuditoria(TIPO_DE_AUDITORIA tipoDeAuditoria)
    {
        ExigirAccion(AccionTiposDeAuditoria.CrearTipoDeAuditoria);
        ValidarTipoDeAuditoria(tipoDeAuditoria);

        TIPO_DE_AUDITORIA nuevoTipoDeAuditoria = new()
        {
            Codigo = tipoDeAuditoria.Codigo,
            Descripcion = tipoDeAuditoria.Descripcion
        };
        nuevoTipoDeAuditoria.IdTipoDeAuditoria = tiposDeAuditoria.Insertar(nuevoTipoDeAuditoria);

        gestorLog.LogDeTipoAuditoria(TipoDeAccionLog.Alta, null, nuevoTipoDeAuditoria);

        return nuevoTipoDeAuditoria.IdTipoDeAuditoria;
    }

    /// <summary>
    /// Guarda código y descripción. Origen: ActualizarTipoDeAuditoria (línea 74).
    /// Como en el origen, si el tipo no existe el texto del error es distinto (sin tilde) del que dan la consulta y la baja.
    /// </summary>
    public void ActualizarTipoDeAuditoria(TIPO_DE_AUDITORIA tipoDeAuditoria)
    {
        ExigirAccion(AccionTiposDeAuditoria.ActualizarTipoDeAuditoria);
        ValidarTipoDeAuditoria(tipoDeAuditoria);

        TIPO_DE_AUDITORIA tipoDeAuditoriaAntigua = tiposDeAuditoria.ObtenerPorId(tipoDeAuditoria.IdTipoDeAuditoria)
            ?? throw new ApplicationException(DatosMaestros.TiposAuditoriaNoExisteTipoAuditoria);

        TIPO_DE_AUDITORIA tipoDeAuditoriaActual = new()
        {
            IdTipoDeAuditoria = tipoDeAuditoria.IdTipoDeAuditoria,
            Codigo = tipoDeAuditoria.Codigo,
            Descripcion = tipoDeAuditoria.Descripcion
        };
        tiposDeAuditoria.Actualizar(tipoDeAuditoriaActual);

        gestorLog.LogDeTipoAuditoria(TipoDeAccionLog.Modificacion, tipoDeAuditoriaAntigua, tipoDeAuditoriaActual);
    }

    /// <summary>
    /// Eliminación lógica. No se permite si el tipo tiene procesos asignados o usuarios; como en el origen,
    /// cuentan también los procesos asignados borrados y los usuarios dados de baja.
    /// Origen: EliminarTipoDeAuditoria (línea 113).
    /// </summary>
    public void EliminarTipoDeAuditoria(int idTipoDeAuditoria)
    {
        ExigirAccion(AccionTiposDeAuditoria.EliminarTipoDeAuditoria);

        TIPO_DE_AUDITORIA tipoDeAuditoria = tiposDeAuditoria.ObtenerPorId(idTipoDeAuditoria)
            ?? throw new ApplicationException(DatosMaestros.TipoAuditoriasNoExisteTipoAuditoria);

        UsoDeTipoDeAuditoria uso = tiposDeAuditoria.ObtenerUso(idTipoDeAuditoria);

        if (uso.ProcesosAsociados > 0)
        {
            throw new ApplicationException(DatosMaestros.TipoAuditoriasTieneProcesosAsociados);
        }

        if (uso.Usuarios > 0)
        {
            throw new ApplicationException(DatosMaestros.TipoAuditoriasTieneUsuariosAsociados);
        }

        tiposDeAuditoria.MarcarBorrado(idTipoDeAuditoria);
        gestorLog.LogDeTipoAuditoria(TipoDeAccionLog.Baja, tipoDeAuditoria, null);
    }

    /// <summary>No puede haber dos tipos de auditoría no borrados con el mismo código. Origen: ValidarTipoDeAuditoria (línea 261).</summary>
    private void ValidarTipoDeAuditoria(TIPO_DE_AUDITORIA tipoDeAuditoria)
    {
        if (tiposDeAuditoria.ContarConMismoCodigo(tipoDeAuditoria.Codigo, tipoDeAuditoria.IdTipoDeAuditoria) != 0)
        {
            throw new ApplicationException(DatosMaestros.TipoAuditoriasYaExisteTipoAuditoriaMismoCodigo);
        }
    }

    private void ExigirAccion(AccionTiposDeAuditoria accion)
    {
        if (!AccionesPermitidas().Contains(accion))
        {
            throw new SecurityException(string.Format(DatosMaestros.UsuarioSinPermisosParaRealizarAccion, gestorUsuarios.UsuarioActual));
        }
    }
}
