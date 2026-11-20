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
    public const string SociedadesNoExisteSociedad = "No existe la sociedad";
    public const string SociedadesYaExisteSociedadMismoeSif = "Ya existe otra sociedad con este mismo código eSif.";
    public const string SociedadesTieneProcesosAsociados = "La sociedad tiene procesos asociados.";
    public const string SociedadesTieneUsuariosAsociados = "La sociedad tiene usuarios asociados";
    public const string TipoAuditoriasNoExisteTipoAuditoria = "No existe el tipo de auditoría";
    /// <summary>Texto distinto (sin tilde) que usa el original solo al actualizar (GestorTiposDeAuditoria.cs:106).</summary>
    public const string TiposAuditoriaNoExisteTipoAuditoria = "No existe el tipo de auditoria";
    public const string TipoAuditoriasYaExisteTipoAuditoriaMismoCodigo = "Ya existe otro tipo de auditoría con este mismo código";
    public const string TipoAuditoriasTieneProcesosAsociados = "El tipo de auditorías tiene procesos asociados";
    public const string TipoAuditoriasTieneUsuariosAsociados = "El tipo de auditorías tiene usuarios asociados";
    public const string TipoDeEmpresa_1 = "Auditora";
    public const string TipoDeEmpresa_2 = "Auditada";
    public const string TipoDeEmpresa_3 = "Ambas";
    public const string UsuariosNoExisteElUsuario = "No existe el usuario";
}
