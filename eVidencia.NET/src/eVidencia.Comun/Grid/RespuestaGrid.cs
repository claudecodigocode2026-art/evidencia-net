namespace eVidencia.Comun.Grid;

/// <summary>
/// Respuesta JSON que espera jqGrid. Mismos nombres que la respuesta original
/// (AdministracionService.asmx.cs:165: page, total, records, rows).
/// </summary>
public sealed class RespuestaGrid<T>
{
    public int page { get; init; }
    public int total { get; init; }
    public int records { get; init; }
    public required IReadOnlyList<T> rows { get; init; }

    public static RespuestaGrid<T> Crear(int pagina, int filasPorPagina, int totalElementos, IReadOnlyList<T> filas) => new()
    {
        page = pagina,
        total = (int)Math.Ceiling((double)totalElementos / (double)filasPorPagina),
        records = totalElementos,
        rows = filas
    };
}
