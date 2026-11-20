namespace eVidencia.Entidades.Filtros;

/// <summary>
/// Filtro del listado de Subprocesos. Origen: eVidencias.Entidades/Filtros/FiltroSUBPROCESO.cs.
/// </summary>
public class FiltroSUBPROCESO
{
    public string? Nombre { get; set; }
    public string? Codigo { get; set; }
    public List<int>? ListaIdProceso { get; set; }
}
