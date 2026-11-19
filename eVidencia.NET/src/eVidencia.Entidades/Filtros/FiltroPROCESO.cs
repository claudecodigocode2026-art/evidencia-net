namespace eVidencia.Entidades.Filtros;

/// <summary>
/// Origen: eVidencias.Entidades/Filtros/FiltroPROCESO.cs. La pantalla de Procesos solo usa Codigo y Nombre
/// (GestorProcesos.ConstruirFiltroProceso).
/// </summary>
public class FiltroPROCESO
{
    public string? Nombre { get; set; }
    public string? Codigo { get; set; }
    public List<int>? ListaIdTipoAuditoria { get; set; }
    public List<int>? ListaIdSubproceso { get; set; }
}
