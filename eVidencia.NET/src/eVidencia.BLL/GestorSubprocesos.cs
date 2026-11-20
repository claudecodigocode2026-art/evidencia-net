using System.Security;
using eVidencia.DAL;
using eVidencia.Entidades;
using eVidencia.Entidades.Filtros;

namespace eVidencia.BLL;

/// <summary>
/// Lógica de negocio de los subprocesos.
/// Origen: eVidencias.BLL/GestorSubprocesos.cs (559 líneas). Se migran los métodos que usan las pantallas
/// Subprocesos.aspx y Subproceso.aspx y el alta interna que usa la Administración de Procesos.
/// Pendientes: las consultas para procesos asignados (líneas 378-537) y la exportación a Excel.
/// </summary>
public class GestorSubprocesos(
    IRepositorioSubprocesos subprocesos,
    IRepositorioProcesos procesos,
    GestorUsuarios gestorUsuarios,
    GestorLogDeEntidades gestorLog)
{
    public enum AccionSubproceso
    {
        ConsultarSubproceso,
        CrearSubproceso,
        ActualizarSubproceso,
        EliminarSubproceso
    }

    /// <summary>Origen: AccionesPermitidas (línea 46).</summary>
    public List<AccionSubproceso> AccionesPermitidas()
    {
        List<AccionSubproceso> acciones = [];
        USUARIO usuario = gestorUsuarios.ConsultarUsuarioAutenticado();
        switch (usuario.IdPerfilDeUsuario)
        {
            case (int)PerfilDeUsuario.Administrador:
                acciones.Add(AccionSubproceso.ConsultarSubproceso);
                acciones.Add(AccionSubproceso.ActualizarSubproceso);
                acciones.Add(AccionSubproceso.CrearSubproceso);
                acciones.Add(AccionSubproceso.EliminarSubproceso);
                break;
            case (int)PerfilDeUsuario.ResponsableDeAuditoria:
            case (int)PerfilDeUsuario.AdministradorPais:
            case (int)PerfilDeUsuario.ResponsableEmpresa:
                acciones.Add(AccionSubproceso.ConsultarSubproceso);
                acciones.Add(AccionSubproceso.CrearSubproceso);
                acciones.Add(AccionSubproceso.EliminarSubproceso);
                break;
            case (int)PerfilDeUsuario.Auditor:
            case (int)PerfilDeUsuario.AuditorExterno:
            case (int)PerfilDeUsuario.ResponsableControl:
            case (int)PerfilDeUsuario.ResponsableSeguimientoArea:
                acciones.Add(AccionSubproceso.ConsultarSubproceso);
                break;
        }
        return acciones;
    }

    /// <summary>
    /// Crea un subproceso sin validar duplicados (lo usa el alta de procesos para el subproceso "Otros").
    /// Origen: CrearSubprocesoInterno (línea 237).
    /// </summary>
    public int CrearSubprocesoInterno(SUBPROCESO subproceso)
    {
        if (!AccionesPermitidas().Contains(AccionSubproceso.CrearSubproceso))
        {
            throw new SecurityException(string.Format(DatosMaestros.UsuarioSinPermisosParaRealizarAccion, gestorUsuarios.UsuarioActual));
        }

        SUBPROCESO nuevoSubproceso = new()
        {
            Codigo = subproceso.Codigo,
            Nombre = subproceso.Nombre,
            IdProceso = subproceso.IdProceso,
            TipoOtros = subproceso.TipoOtros
        };
        nuevoSubproceso.IdSubproceso = subprocesos.Insertar(nuevoSubproceso);

        // En el origen, el registro de auditoría lee el nombre del proceso por la relación SUBPROCESO.PROCESO.
        nuevoSubproceso.PROCESO = procesos.ObtenerPorId(nuevoSubproceso.IdProceso);
        gestorLog.LogDeSubproceso(TipoDeAccionLog.Alta, null, nuevoSubproceso);

        return nuevoSubproceso.IdSubproceso;
    }

    /// <summary>
    /// Listado paginado: no incluye los subprocesos borrados ni los de tipo "Otros".
    /// Origen: ConsultarSubprocesoConFiltro paginado (línea 124).
    /// </summary>
    public List<SUBPROCESO> ConsultarSubprocesoConFiltro(FiltroSUBPROCESO? filtro, string orden, int numeroPagina, int numeroElementosPorPagina, out int numeroElementosTotales)
    {
        ExigirAccion(AccionSubproceso.ConsultarSubproceso);
        return subprocesos.ObtenerFiltrados(filtro, orden, numeroPagina, numeroElementosPorPagina, out numeroElementosTotales);
    }

    /// <summary>Origen: ConsultarSubproceso (línea 351).</summary>
    public SUBPROCESO ConsultarSubproceso(int idSubproceso)
    {
        ExigirAccion(AccionSubproceso.ConsultarSubproceso);
        return subprocesos.ObtenerPorId(idSubproceso)
            ?? throw new ApplicationException(DatosMaestros.SubprocesosNoExisteProceso);
    }

    /// <summary>Origen: CrearSubproceso (línea 270). Siempre crea un subproceso que no es de tipo "Otros".</summary>
    public int CrearSubproceso(SUBPROCESO subproceso)
    {
        ExigirAccion(AccionSubproceso.CrearSubproceso);
        ValidarSubproceso(subproceso);

        SUBPROCESO nuevoSubproceso = new()
        {
            Codigo = subproceso.Codigo,
            Nombre = subproceso.Nombre,
            IdProceso = subproceso.IdProceso,
            TipoOtros = false
        };
        nuevoSubproceso.IdSubproceso = subprocesos.Insertar(nuevoSubproceso);

        // El registro de auditoría lee el nombre del proceso por la relación SUBPROCESO.PROCESO.
        SUBPROCESO guardado = subprocesos.ObtenerPorId(nuevoSubproceso.IdSubproceso) ?? nuevoSubproceso;
        gestorLog.LogDeSubproceso(TipoDeAccionLog.Alta, null, guardado);

        return nuevoSubproceso.IdSubproceso;
    }

    /// <summary>Origen: ActualizarSubproceso (línea 149). Guarda código, nombre y proceso.</summary>
    public void ActualizarSubproceso(SUBPROCESO subproceso)
    {
        ExigirAccion(AccionSubproceso.ActualizarSubproceso);
        ValidarSubproceso(subproceso);

        SUBPROCESO subprocesoAntiguo = subprocesos.ObtenerPorId(subproceso.IdSubproceso)
            ?? throw new ApplicationException(DatosMaestros.SubprocesosNoExisteProceso);

        subprocesos.Actualizar(new SUBPROCESO
        {
            IdSubproceso = subproceso.IdSubproceso,
            Codigo = subproceso.Codigo,
            Nombre = subproceso.Nombre,
            IdProceso = subproceso.IdProceso
        });

        SUBPROCESO subprocesoActual = subprocesos.ObtenerPorId(subproceso.IdSubproceso)!;
        gestorLog.LogDeSubproceso(TipoDeAccionLog.Modificacion, subprocesoAntiguo, subprocesoActual);
    }

    /// <summary>
    /// Eliminación lógica. No se permite si el subproceso se usa en algún proceso asignado no borrado.
    /// Origen: EliminarSubproceso (línea 305).
    /// </summary>
    public void EliminarSubproceso(int idSubproceso)
    {
        ExigirAccion(AccionSubproceso.EliminarSubproceso);

        SUBPROCESO subproceso = subprocesos.ObtenerPorId(idSubproceso)
            ?? throw new ApplicationException(DatosMaestros.SubprocesosNoExisteProceso);

        if (subprocesos.ContarProcesosAsociadosNoBorrados(idSubproceso) > 0)
        {
            throw new ApplicationException(DatosMaestros.SubprocesosEstaAsociadoAunPA);
        }

        subprocesos.MarcarBorrado(idSubproceso);
        gestorLog.LogDeSubproceso(TipoDeAccionLog.Baja, subproceso, null);
    }

    /// <summary>
    /// No puede haber dos subprocesos no borrados con el mismo código. La comprobación incluye los de tipo "Otros".
    /// Origen: ValidarSubproceso (línea 190).
    /// </summary>
    private void ValidarSubproceso(SUBPROCESO subproceso)
    {
        if (subprocesos.ContarConMismoCodigo(subproceso.Codigo, subproceso.IdSubproceso) != 0)
        {
            throw new ApplicationException(DatosMaestros.SubprocesosYaexisteSubprocesoMismoCodigo);
        }
    }

    private void ExigirAccion(AccionSubproceso accion)
    {
        if (!AccionesPermitidas().Contains(accion))
        {
            throw new SecurityException(string.Format(DatosMaestros.UsuarioSinPermisosParaRealizarAccion, gestorUsuarios.UsuarioActual));
        }
    }
}
