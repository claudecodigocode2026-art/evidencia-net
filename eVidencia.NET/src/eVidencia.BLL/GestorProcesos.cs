using System.Security;
using eVidencia.DAL;
using eVidencia.Entidades;
using eVidencia.Entidades.Filtros;

namespace eVidencia.BLL;

/// <summary>
/// Lógica de negocio de la Administración de Procesos.
/// Origen: eVidencias.BLL/GestorProcesos.cs (620 líneas). Se migran los métodos que usan las pantallas
/// Procesos.aspx y Proceso.aspx; el resto (consultas para otras pantallas y la exportación) queda fuera de la demo.
/// Las reglas, su orden y los mensajes son los del código original.
/// </summary>
public class GestorProcesos(
    IRepositorioProcesos procesos,
    IRepositorioSubprocesos subprocesos,
    GestorUsuarios gestorUsuarios,
    GestorSubprocesos gestorSubprocesos,
    GestorLogDeEntidades gestorLog)
{
    public enum AccionProceso
    {
        ConsultarProceso,
        CrearProceso,
        ActualizarProceso,
        EliminarProceso
    }

    /// <summary>
    /// Acciones permitidas para el usuario autenticado. Origen: AccionesPermitidas (línea 160).
    /// </summary>
    public List<AccionProceso> AccionesPermitidas()
    {
        List<AccionProceso> acciones = [];
        USUARIO usuario = gestorUsuarios.ConsultarUsuarioAutenticado();
        switch (usuario.IdPerfilDeUsuario)
        {
            case (int)PerfilDeUsuario.Administrador:
                acciones.Add(AccionProceso.ConsultarProceso);
                acciones.Add(AccionProceso.ActualizarProceso);
                acciones.Add(AccionProceso.CrearProceso);
                acciones.Add(AccionProceso.EliminarProceso);
                break;
            case (int)PerfilDeUsuario.Auditor:
            case (int)PerfilDeUsuario.ResponsableDeAuditoria:
            case (int)PerfilDeUsuario.ResponsableEmpresa:
            case (int)PerfilDeUsuario.AdministradorPais:
            case (int)PerfilDeUsuario.AuditorExterno:
            case (int)PerfilDeUsuario.ResponsableControl:
            case (int)PerfilDeUsuario.ResponsableSeguimientoArea:
                acciones.Add(AccionProceso.ConsultarProceso);
                break;
        }
        return acciones;
    }

    /// <summary>Origen: ConsultarProcesoConFiltro paginado (línea 384).</summary>
    public List<PROCESO> ConsultarProcesoConFiltro(FiltroPROCESO? filtro, string orden, int numeroPagina, int numeroElementosPorPagina, out int numeroElementosTotales)
    {
        Exigir(AccionProceso.ConsultarProceso);
        return procesos.ObtenerFiltrados(filtro, orden, numeroPagina, numeroElementosPorPagina, out numeroElementosTotales);
    }

    /// <summary>Origen: ConsultarProceso (línea 246).</summary>
    public PROCESO ConsultarProceso(int idProceso)
    {
        Exigir(AccionProceso.ConsultarProceso);
        return procesos.ObtenerPorId(idProceso)
            ?? throw new ApplicationException(DatosMaestros.ProcesosNoExisteProceso);
    }

    /// <summary>Procesos no borrados ordenados por código. Origen: ConsultarProcesos (línea 120).</summary>
    public List<PROCESO> ConsultarProcesos()
    {
        Exigir(AccionProceso.ConsultarProceso);
        return procesos.ObtenerNoBorradosPorCodigo();
    }

    /// <summary>
    /// Procesos no borrados que tienen algún subproceso, para el filtro del listado de Subprocesos.
    /// Como en el origen, no comprueba permisos y cuenta también los subprocesos borrados.
    /// El origen no indica orden; aquí se devuelven por identificador.
    /// Origen: ConsultarProcesosParaSubprocesos (línea 552).
    /// </summary>
    public List<PROCESO> ConsultarProcesosParaSubprocesos() => procesos.ObtenerConSubprocesos();

    /// <summary>Origen: ConsultarTiposProcesos (línea 462).</summary>
    public List<TIPO_DE_PROCESO> ConsultarTiposProcesos()
    {
        Exigir(AccionProceso.ConsultarProceso);
        return procesos.ObtenerTiposDeProceso();
    }

    /// <summary>
    /// Crea un proceso y su subproceso "Otros". Origen: CrearProceso (línea 481).
    /// </summary>
    public int CrearProceso(PROCESO proceso)
    {
        Exigir(AccionProceso.CrearProceso);
        ValidarProceso(proceso);

        PROCESO nuevoProceso = new()
        {
            Codigo = proceso.Codigo,
            Nombre = proceso.Nombre,
            IdTipoDeProceso = proceso.IdTipoDeProceso
        };
        nuevoProceso.IdProceso = procesos.Insertar(nuevoProceso);

        // En el origen, el registro de auditoría lee el tipo por la relación PROCESO.TIPO_DE_PROCESO.
        gestorLog.LogDeProceso(TipoDeAccionLog.Alta, null, procesos.ObtenerPorId(nuevoProceso.IdProceso) ?? nuevoProceso);

        // Crear subproceso Otros y asociarlo al nuevo proceso
        SUBPROCESO subproceso = new()
        {
            Codigo = "",
            Nombre = "Otros",
            IdProceso = nuevoProceso.IdProceso,
            TipoOtros = true
        };
        gestorSubprocesos.CrearSubprocesoInterno(subproceso);

        return nuevoProceso.IdProceso;
    }

    /// <summary>Origen: ActualizarProceso (línea 189).</summary>
    public void ActualizarProceso(PROCESO proceso)
    {
        Exigir(AccionProceso.ActualizarProceso);
        ValidarProceso(proceso);

        PROCESO? procesoAntiguo = procesos.ObtenerPorId(proceso.IdProceso);
        if (procesoAntiguo == null)
        {
            throw new ApplicationException(DatosMaestros.ProcesosNoExisteProceso);
        }

        procesos.Actualizar(new PROCESO
        {
            IdProceso = proceso.IdProceso,
            Codigo = proceso.Codigo,
            Nombre = proceso.Nombre,
            IdTipoDeProceso = proceso.IdTipoDeProceso
        });

        PROCESO procesoActual = procesos.ObtenerPorId(proceso.IdProceso)!;
        gestorLog.LogDeProceso(TipoDeAccionLog.Modificacion, procesoAntiguo, procesoActual);
    }

    /// <summary>
    /// Eliminación lógica de un proceso. Origen: EliminarProceso (línea 411).
    /// </summary>
    public void EliminarProceso(int idProceso)
    {
        Exigir(AccionProceso.EliminarProceso);

        PROCESO? proceso = procesos.ObtenerPorId(idProceso);
        if (proceso == null)
        {
            throw new ApplicationException(DatosMaestros.ProcesosNoExisteProceso);
        }

        // Comprobamos que el proceso no esté en uso
        if (procesos.ContarProcesosAsociados(idProceso) > 0)
        {
            throw new ApplicationException(DatosMaestros.ProcesosTieneProcesosAsociados);
        }

        procesos.MarcarBorrado(idProceso);
        proceso.Borrado = true;

        SUBPROCESO? subproceso = subprocesos.ObtenerOtrosDeProceso(idProceso);
        if (subproceso != null)
        {
            subprocesos.MarcarBorrado(subproceso.IdSubproceso);
        }

        gestorLog.LogDeProceso(TipoDeAccionLog.Baja, proceso, null);
    }

    /// <summary>Origen: ValidarProceso (línea 230).</summary>
    private void ValidarProceso(PROCESO proceso)
    {
        if (procesos.ContarConMismoCodigo(proceso.Codigo, proceso.IdProceso) != 0)
        {
            throw new ApplicationException(DatosMaestros.ProcesosYaExisteProcesoMismoCodigo);
        }
    }

    private void Exigir(AccionProceso accion)
    {
        if (!AccionesPermitidas().Contains(accion))
        {
            throw new SecurityException(string.Format(DatosMaestros.UsuarioSinPermisosParaRealizarAccion, gestorUsuarios.UsuarioActual));
        }
    }
}
