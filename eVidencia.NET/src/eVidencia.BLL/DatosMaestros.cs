namespace eVidencia.BLL;

/// <summary>
/// Textos de la lógica de negocio. Origen: eVidencias.BLL/DatosMaestros.resx (español).
/// La demo solo incluye el español; el original tiene además en-US y pt-BR.
/// </summary>
public static class DatosMaestros
{
    public const string UsuarioSinPermisosParaRealizarAccion = "El usuario {0} no tiene permisos para realizar esta acción.";
    public const string ProcesosNoExisteProceso = "No existe el proceso";
    public const string ProcesosYaExisteProcesoMismoCodigo = "Ya existe otro proceso con este mismo código";
    public const string ProcesosTieneProcesosAsociados = "El Proceso tiene procesos asociados";
    public const string SubprocesosNoExisteProceso = "No existe el subproceso";
    public const string SubprocesosYaexisteSubprocesoMismoCodigo = "Ya existe otro subproceso con este mismo código";
    public const string SubprocesosEstaAsociadoAunPA = "No se puede eliminar el Subproceso porque esta asociado a Procesos Asignados.";
    public const string UsuariosNoExisteElUsuario = "No existe el usuario";
}
