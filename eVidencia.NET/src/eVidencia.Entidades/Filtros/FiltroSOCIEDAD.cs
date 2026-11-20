namespace eVidencia.Entidades.Filtros;

/// <summary>Filtro del listado de Sociedades. Origen: eVidencias.Entidades/Filtros/FiltroSOCIEDAD.cs.</summary>
public class FiltroSOCIEDAD
{
    public string? Nombre { get; set; }
    public List<int>? ListaIdPais { get; set; }
    public List<int>? ListaIdTipoEmpresa { get; set; }
}
