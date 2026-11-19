using System.Security;
using eVidencia.DAL;
using eVidencia.Entidades;

namespace eVidencia.BLL;

/// <summary>
/// Parte de GestorSubprocesos que necesita la Administración de Procesos.
/// Origen: eVidencias.BLL/GestorSubprocesos.cs (559 líneas); aquí AccionesPermitidas y CrearSubprocesoInterno.
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
}
