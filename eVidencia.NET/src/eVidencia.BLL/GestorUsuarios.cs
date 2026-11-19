using eVidencia.Comun.Seguridad;
using eVidencia.DAL;
using eVidencia.Entidades;

namespace eVidencia.BLL;

/// <summary>Origen: eVidencias.BLL/GestorUsuarios.cs:18-28.</summary>
public enum PerfilDeUsuario
{
    Administrador = 1,
    ResponsableDeAuditoria = 2,
    Auditor = 3,
    AdministradorPais = 4,
    ResponsableEmpresa = 5,
    AuditorExterno = 6,
    ResponsableControl = 7,
    ResponsableSeguimientoArea = 8
}

/// <summary>
/// Parte de GestorUsuarios que necesita la Administración de Procesos.
/// Origen: eVidencias.BLL/GestorUsuarios.cs (1.046 líneas); aquí solo ConsultarUsuarioAutenticado.
/// </summary>
public class GestorUsuarios(IRepositorioUsuarios usuarios, IUsuarioActual usuarioActual)
{
    private USUARIO? _usuarioAutenticado;

    /// <summary>UID del usuario que ejecuta la lógica de negocio.</summary>
    public string UsuarioActual => usuarioActual.Uid;

    /// <summary>Origen: GestorUsuarios.ConsultarUsuarioAutenticado (línea 393).</summary>
    public USUARIO ConsultarUsuarioAutenticado()
    {
        if (_usuarioAutenticado != null && _usuarioAutenticado.UID == UsuarioActual)
        {
            return _usuarioAutenticado;
        }

        List<USUARIO> listaUsuarios = usuarios.ObtenerPorUid(UsuarioActual);

        if (listaUsuarios.Count == 1)
        {
            _usuarioAutenticado = listaUsuarios.First();
            return _usuarioAutenticado;
        }

        throw new ApplicationException(DatosMaestros.UsuariosNoExisteElUsuario);
    }
}
