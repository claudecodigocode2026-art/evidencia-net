namespace eVidencia.Comun.Grid;

/// <summary>Un criterio de ordenación ya validado.</summary>
public sealed record CriterioOrden(string Campo, bool Descendente);

/// <summary>
/// Interpreta la cadena de orden que envían los listados ("Codigo asc", "TIPO_DE_PROCESO.Nombre desc").
/// Origen: eVidencias.Util/OrderByHelper.cs, que resolvía el campo por reflexión.
/// Aquí el campo debe estar en una lista de campos permitidos: componente común reutilizable
/// por cualquier listado de cualquier aplicación.
/// </summary>
public static class OrdenGrid
{
    public static List<CriterioOrden> Interpretar(string? orden, IReadOnlyCollection<string> camposPermitidos)
    {
        List<CriterioOrden> criterios = [];
        if (string.IsNullOrWhiteSpace(orden)) return criterios;

        foreach (string parte in orden.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        {
            string[] trozos = parte.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (trozos.Length == 0) continue;

            string? campo = camposPermitidos.FirstOrDefault(c => c == trozos[0]);
            if (campo == null)
                throw new ArgumentException($"Campo de ordenación no permitido: '{trozos[0]}'.");

            bool descendente = trozos.Length > 1 && trozos[1].Equals("desc", StringComparison.OrdinalIgnoreCase);
            criterios.Add(new CriterioOrden(campo, descendente));
        }
        return criterios;
    }
}
