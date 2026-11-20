using System.Security;
using eVidencia.DAL;
using eVidencia.Entidades;
using eVidencia.Entidades.Filtros;

namespace eVidencia.BLL;

/// <summary>Tipos de las sociedades. Origen: eVidencias.BLL/GestorSociedades.cs:20.</summary>
public enum TipoDeSociedad
{
    Auditora = 1,
    Auditada = 2,
    Ambas = 3
}

/// <summary>
/// Lógica de negocio de las sociedades.
/// Origen: eVidencias.BLL/GestorSociedades.cs (529 líneas). Se migran los métodos que usan las pantallas
/// Sociedades.aspx y Sociedad.aspx.
/// Pendientes: las consultas que usan otras pantallas (selector de sociedad y periodo, usuarios, reportes;
/// líneas 209-300 y 420-497) y la exportación a Excel.
/// </summary>
public class GestorSociedades(
    IRepositorioSociedades sociedades,
    GestorUsuarios gestorUsuarios,
    GestorLogDeEntidades gestorLog)
{
    public enum AccionSociedades
    {
        CrearSociedad,
        ActualizarSociedad,
        EliminarSociedad,
        ConsultarSociedad
    }

    /// <summary>
    /// Solo el Administrador puede crear, actualizar y eliminar; el resto de perfiles solo consulta.
    /// Origen: AccionesPermitidas (línea 391).
    /// </summary>
    public List<AccionSociedades> AccionesPermitidas()
    {
        List<AccionSociedades> acciones = [];
        USUARIO usuario = gestorUsuarios.ConsultarUsuarioAutenticado();
        switch (usuario.IdPerfilDeUsuario)
        {
            case (int)PerfilDeUsuario.Administrador:
                acciones.Add(AccionSociedades.CrearSociedad);
                acciones.Add(AccionSociedades.ActualizarSociedad);
                acciones.Add(AccionSociedades.EliminarSociedad);
                acciones.Add(AccionSociedades.ConsultarSociedad);
                break;
            case (int)PerfilDeUsuario.AdministradorPais:
            case (int)PerfilDeUsuario.ResponsableEmpresa:
            case (int)PerfilDeUsuario.Auditor:
            case (int)PerfilDeUsuario.ResponsableDeAuditoria:
            case (int)PerfilDeUsuario.AuditorExterno:
            case (int)PerfilDeUsuario.ResponsableControl:
            case (int)PerfilDeUsuario.ResponsableSeguimientoArea:
                acciones.Add(AccionSociedades.ConsultarSociedad);
                break;
        }
        return acciones;
    }

    /// <summary>
    /// Listado paginado de sociedades no borradas. Si el usuario tiene el perfil Auditor, solo ve las
    /// sociedades que tiene asignadas; el resto de perfiles ve todas.
    /// Origen: ConsultarSociedadConFiltro paginado (línea 310).
    /// </summary>
    public List<SOCIEDAD> ConsultarSociedadConFiltro(FiltroSOCIEDAD? filtro, string orden, int numeroPagina, int numeroElementosPorPagina, out int numeroElementosTotales)
    {
        ExigirAccion(AccionSociedades.ConsultarSociedad);

        USUARIO usuario = gestorUsuarios.ConsultarUsuarioAutenticado();
        int? idUsuarioAsignado = usuario.IdPerfilDeUsuario == (int)PerfilDeUsuario.Auditor ? usuario.IdUsuario : null;

        List<SOCIEDAD> lista = sociedades.ObtenerFiltrados(filtro, idUsuarioAsignado, orden, numeroPagina, numeroElementosPorPagina, out numeroElementosTotales);
        lista.ForEach(s =>
        {
            if (s.TIPO_DE_EMPRESA != null)
                s.TIPO_DE_EMPRESA.Nombre = GestorDatosMaestros.Traducir("TipoDeEmpresa", s.TIPO_DE_EMPRESA.IdTipoDeEmpresa, s.TIPO_DE_EMPRESA.Nombre);
        });
        return lista;
    }

    /// <summary>Origen: ConsultarSociedad (línea 182).</summary>
    public SOCIEDAD ConsultarSociedad(int idSociedad)
    {
        ExigirAccion(AccionSociedades.ConsultarSociedad);
        return sociedades.ObtenerPorId(idSociedad)
            ?? throw new ApplicationException(DatosMaestros.SociedadesNoExisteSociedad);
    }

    /// <summary>Origen: CrearSociedad (línea 52).</summary>
    public int CrearSociedad(SOCIEDAD sociedad)
    {
        ExigirAccion(AccionSociedades.CrearSociedad);
        ValidarSociedad(sociedad);

        int idSociedad = sociedades.Insertar(new SOCIEDAD
        {
            CodigoEsif = sociedad.CodigoEsif,
            Nombre = sociedad.Nombre,
            IdPais = sociedad.IdPais,
            IdTipoDeEmpresa = sociedad.IdTipoDeEmpresa
        });

        // El registro de auditoría lee los nombres de país y tipo de empresa por las relaciones de la sociedad.
        SOCIEDAD nuevaSociedad = sociedades.ObtenerPorId(idSociedad)!;
        gestorLog.LogDeSociedad(TipoDeAccionLog.Alta, null, nuevaSociedad);

        return idSociedad;
    }

    /// <summary>
    /// Guarda código eSif, nombre, país y tipo de empresa.
    /// Origen: ActualizarSociedad (línea 88). Como en el origen, primero se valida el código y después se comprueba que la sociedad existe.
    /// </summary>
    public void ActualizarSociedad(SOCIEDAD sociedad)
    {
        ExigirAccion(AccionSociedades.ActualizarSociedad);
        ValidarSociedad(sociedad);

        SOCIEDAD sociedadAntigua = sociedades.ObtenerPorId(sociedad.IdSociedad)
            ?? throw new ApplicationException(DatosMaestros.SociedadesNoExisteSociedad);

        sociedades.Actualizar(new SOCIEDAD
        {
            IdSociedad = sociedad.IdSociedad,
            CodigoEsif = sociedad.CodigoEsif,
            Nombre = sociedad.Nombre,
            IdPais = sociedad.IdPais,
            IdTipoDeEmpresa = sociedad.IdTipoDeEmpresa
        });

        SOCIEDAD sociedadActual = sociedades.ObtenerPorId(sociedad.IdSociedad)!;
        gestorLog.LogDeSociedad(TipoDeAccionLog.Modificacion, sociedadAntigua, sociedadActual);
    }

    /// <summary>
    /// Eliminación lógica. No se permite si la sociedad tiene procesos asignados, usuarios o usuarios asignados;
    /// como en el origen, cuentan también los procesos asignados y los usuarios dados de baja.
    /// Origen: EliminarSociedad (línea 131).
    /// </summary>
    public void EliminarSociedad(int idSociedad)
    {
        ExigirAccion(AccionSociedades.EliminarSociedad);

        SOCIEDAD sociedad = sociedades.ObtenerPorId(idSociedad)
            ?? throw new ApplicationException(DatosMaestros.SociedadesNoExisteSociedad);

        UsoDeSociedad uso = sociedades.ObtenerUso(idSociedad);

        if (uso.ProcesosAsociados > 0)
        {
            throw new ApplicationException(DatosMaestros.SociedadesTieneProcesosAsociados);
        }

        if (uso.Usuarios > 0 || uso.UsuariosAuditores > 0)
        {
            throw new ApplicationException(DatosMaestros.SociedadesTieneUsuariosAsociados);
        }

        sociedades.MarcarBorrado(idSociedad);
        gestorLog.LogDeSociedad(TipoDeAccionLog.Baja, sociedad, null);
    }

    /// <summary>
    /// No puede haber dos sociedades no borradas con el mismo código eSif.
    /// Origen: ValidarSociedad (línea 374).
    /// </summary>
    private void ValidarSociedad(SOCIEDAD sociedad)
    {
        if (sociedades.ContarConMismoCodigoEsif(sociedad.CodigoEsif, sociedad.IdSociedad) != 0)
        {
            throw new ApplicationException(DatosMaestros.SociedadesYaExisteSociedadMismoeSif);
        }
    }

    private void ExigirAccion(AccionSociedades accion)
    {
        if (!AccionesPermitidas().Contains(accion))
        {
            throw new SecurityException(string.Format(DatosMaestros.UsuarioSinPermisosParaRealizarAccion, gestorUsuarios.UsuarioActual));
        }
    }
}
