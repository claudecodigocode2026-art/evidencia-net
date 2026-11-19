namespace eVidencia.Comun.Seguridad;

/// <summary>
/// Identifica al usuario que ejecuta la lógica de negocio.
/// Origen: GestorBase.UsuarioActual (eVidencias.BLL/GestorBase.cs:12,30), que lo tomaba de
/// Thread.CurrentPrincipal.Identity.Name. Aquí se abstrae para que cada aplicación decida
/// de dónde sale (usuario fijo en la demo; autenticación corporativa en el proyecto).
/// </summary>
public interface IUsuarioActual
{
    /// <summary>UID del usuario (columna USUARIO.UID).</summary>
    string Uid { get; }
}

/// <summary>Usuario fijo leído de configuración. Solo para la demo.</summary>
public sealed class UsuarioFijo(string uid) : IUsuarioActual
{
    public string Uid { get; } = uid;
}
