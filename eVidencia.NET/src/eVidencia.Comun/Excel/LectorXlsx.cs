using System.Globalization;
using System.IO.Compression;
using System.Xml.Linq;

namespace eVidencia.Comun.Excel;

/// <summary>
/// Lector mínimo de libros Excel (.xlsx) sin dependencias externas: una hoja se lee como una tabla
/// cuya primera fila son los nombres de columna. Componente común reutilizable.
/// Admite textos en línea y cadenas compartidas; las fechas se devuelven como número de serie de Excel.
/// </summary>
public sealed class LectorXlsx : IDisposable
{
    private static readonly XNamespace Ns = "http://schemas.openxmlformats.org/spreadsheetml/2006/main";
    private static readonly XNamespace NsRel = "http://schemas.openxmlformats.org/officeDocument/2006/relationships";
    private static readonly XNamespace NsPaquete = "http://schemas.openxmlformats.org/package/2006/relationships";

    private readonly ZipArchive _zip;
    private readonly Dictionary<string, string> _rutaDeHoja = new(StringComparer.OrdinalIgnoreCase);
    private readonly List<string> _cadenasCompartidas = [];

    public LectorXlsx(string ruta)
    {
        _zip = ZipFile.OpenRead(ruta);

        Dictionary<string, string> destinoDeRelacion = Cargar("xl/_rels/workbook.xml.rels")
            .Descendants(NsPaquete + "Relationship")
            .ToDictionary(r => (string)r.Attribute("Id")!, r => (string)r.Attribute("Target")!);

        foreach (XElement hoja in Cargar("xl/workbook.xml").Descendants(Ns + "sheet"))
        {
            string destino = destinoDeRelacion[(string)hoja.Attribute(NsRel + "id")!];
            _rutaDeHoja[(string)hoja.Attribute("name")!] = destino.StartsWith('/') ? destino.TrimStart('/') : "xl/" + destino;
        }

        if (_zip.GetEntry("xl/sharedStrings.xml") != null)
        {
            foreach (XElement si in Cargar("xl/sharedStrings.xml").Descendants(Ns + "si"))
                _cadenasCompartidas.Add(string.Concat(si.Descendants(Ns + "t").Select(t => t.Value)));
        }
    }

    public IReadOnlyCollection<string> Hojas => _rutaDeHoja.Keys;

    public bool ExisteHoja(string nombre) => _rutaDeHoja.ContainsKey(nombre);

    /// <summary>Filas de la hoja como diccionarios columna → valor (null si la celda está vacía).</summary>
    public List<Dictionary<string, string?>> LeerTabla(string hoja)
    {
        if (!_rutaDeHoja.TryGetValue(hoja, out string? ruta))
            throw new InvalidOperationException($"El libro Excel no contiene la hoja '{hoja}'.");

        List<Dictionary<string, string?>> filas = [];
        List<string?> cabecera = [];
        bool primera = true;

        foreach (XElement fila in Cargar(ruta).Descendants(Ns + "row"))
        {
            List<string?> valores = [];
            foreach (XElement celda in fila.Elements(Ns + "c"))
            {
                int columna = IndiceDeColumna((string?)celda.Attribute("r"), valores.Count);
                while (valores.Count <= columna) valores.Add(null);
                valores[columna] = ValorDeCelda(celda);
            }

            if (primera)
            {
                cabecera = valores;
                primera = false;
                continue;
            }

            if (valores.All(v => v == null)) continue;

            Dictionary<string, string?> registro = new(StringComparer.OrdinalIgnoreCase);
            for (int i = 0; i < cabecera.Count; i++)
            {
                if (cabecera[i] == null) continue;
                registro[cabecera[i]!] = i < valores.Count ? valores[i] : null;
            }
            filas.Add(registro);
        }
        return filas;
    }

    private string? ValorDeCelda(XElement celda)
    {
        string? tipo = (string?)celda.Attribute("t");
        if (tipo == "inlineStr")
            return string.Concat(celda.Descendants(Ns + "t").Select(t => t.Value));

        string? v = celda.Element(Ns + "v")?.Value;
        if (v == null) return null;
        if (tipo == "s") return _cadenasCompartidas[int.Parse(v, CultureInfo.InvariantCulture)];
        return v;
    }

    /// <summary>Índice (desde 0) de la columna a partir de la referencia de celda ("C7" → 2).</summary>
    private static int IndiceDeColumna(string? referencia, int porDefecto)
    {
        if (string.IsNullOrEmpty(referencia)) return porDefecto;
        int indice = 0;
        foreach (char c in referencia)
        {
            if (!char.IsLetter(c)) break;
            indice = indice * 26 + (char.ToUpperInvariant(c) - 'A' + 1);
        }
        return indice - 1;
    }

    private XDocument Cargar(string entrada)
    {
        ZipArchiveEntry e = _zip.GetEntry(entrada)
            ?? throw new InvalidOperationException($"El libro Excel no contiene '{entrada}'.");
        using Stream s = e.Open();
        return XDocument.Load(s);
    }

    public void Dispose() => _zip.Dispose();
}

/// <summary>Conversión de valores de celda a tipos de .NET.</summary>
public static class ValorXlsx
{
    public static int Entero(string? v) => (int)double.Parse(v!, CultureInfo.InvariantCulture);

    public static int? EnteroONulo(string? v) =>
        string.IsNullOrWhiteSpace(v) || v == "None" ? null : (int)double.Parse(v, CultureInfo.InvariantCulture);

    /// <summary>Verdadero para 1, "true" o "True"; falso para vacío, 0 o "false".</summary>
    public static bool Logico(string? v) =>
        !string.IsNullOrWhiteSpace(v) && (v == "1" || v.Equals("true", StringComparison.OrdinalIgnoreCase)
            || (double.TryParse(v, NumberStyles.Any, CultureInfo.InvariantCulture, out double n) && n != 0));
}
