namespace eVidencia.Web.Servicios;

/// <summary>Datos que la demo muestra en el pie de página. No existe en el original.</summary>
public sealed class InfoDemo(string usuarioFijo, string descripcionDatos)
{
    public string VersionNet { get; } = Environment.Version.Major.ToString();
    public string UsuarioFijo { get; } = usuarioFijo;
    public string DescripcionDatos { get; } = descripcionDatos;
}
