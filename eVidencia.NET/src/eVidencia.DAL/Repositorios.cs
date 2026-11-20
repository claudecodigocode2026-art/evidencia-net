using eVidencia.Entidades;
using eVidencia.Entidades.Filtros;

namespace eVidencia.DAL;

// Contratos de acceso a datos. La lógica de negocio (BLL) solo conoce estas interfaces;
// hay dos implementaciones: SqlServer (base de datos de eVidencia) y Memoria (demo sin base de datos).

public interface IRepositorioProcesos
{
    /// <summary>Procesos no borrados que cumplen el filtro, ordenados y paginados (página desde 1).</summary>
    List<PROCESO> ObtenerFiltrados(FiltroPROCESO? filtro, string orden, int numeroPagina, int numeroElementosPorPagina, out int numeroElementosTotales);

    /// <summary>El proceso no borrado con ese identificador, con su TIPO_DE_PROCESO; null si no existe.</summary>
    PROCESO? ObtenerPorId(int idProceso);

    /// <summary>Número de procesos no borrados con ese código, sin contar el indicado.</summary>
    int ContarConMismoCodigo(string? codigo, int idProcesoExcluido);

    /// <summary>Inserta el proceso y devuelve su identificador.</summary>
    int Insertar(PROCESO proceso);

    void Actualizar(PROCESO proceso);

    void MarcarBorrado(int idProceso);

    /// <summary>Número de filas de PROCESO_ASOCIADO del proceso (borradas o no, como en el origen).</summary>
    int ContarProcesosAsociados(int idProceso);

    List<TIPO_DE_PROCESO> ObtenerTiposDeProceso();

    /// <summary>Procesos no borrados ordenados por código (desplegable de la ficha de Subproceso).</summary>
    List<PROCESO> ObtenerNoBorradosPorCodigo();

    /// <summary>Procesos no borrados que tienen alguna fila en SUBPROCESO, borrada o no (filtro del listado de Subprocesos).</summary>
    List<PROCESO> ObtenerConSubprocesos();
}

public interface IRepositorioSubprocesos
{
    int Insertar(SUBPROCESO subproceso);

    /// <summary>El primer subproceso "Otros" no borrado del proceso; null si no hay.</summary>
    SUBPROCESO? ObtenerOtrosDeProceso(int idProceso);

    void MarcarBorrado(int idSubproceso);

    /// <summary>
    /// Subprocesos no borrados y que no son de tipo "Otros" que cumplen el filtro, con su PROCESO,
    /// ordenados y paginados (página desde 1).
    /// </summary>
    List<SUBPROCESO> ObtenerFiltrados(FiltroSUBPROCESO? filtro, string orden, int numeroPagina, int numeroElementosPorPagina, out int numeroElementosTotales);

    /// <summary>El subproceso no borrado con ese identificador, con su PROCESO (esté o no borrado); null si no existe.</summary>
    SUBPROCESO? ObtenerPorId(int idSubproceso);

    /// <summary>Número de subprocesos no borrados con ese código, sin contar el indicado.</summary>
    int ContarConMismoCodigo(string? codigo, int idSubprocesoExcluido);

    /// <summary>Guarda código, nombre y proceso.</summary>
    void Actualizar(SUBPROCESO subproceso);

    /// <summary>Número de filas no borradas de PROCESO_ASOCIADO que usan el subproceso.</summary>
    int ContarProcesosAsociadosNoBorrados(int idSubproceso);
}

/// <summary>Número de filas de otras tablas que apuntan a una sociedad.</summary>
public readonly record struct UsoDeSociedad(int ProcesosAsociados, int Usuarios, int UsuariosAuditores);

public interface IRepositorioSociedades
{
    /// <summary>
    /// Sociedades no borradas que cumplen el filtro, con su TIPO_DE_EMPRESA y su PAIS, ordenadas y paginadas (página desde 1).
    /// Si se indica idUsuarioAsignado, solo las sociedades asignadas a ese usuario en USUARIO_SOCIEDAD_AUDITORES.
    /// </summary>
    List<SOCIEDAD> ObtenerFiltrados(FiltroSOCIEDAD? filtro, int? idUsuarioAsignado, string orden, int numeroPagina, int numeroElementosPorPagina, out int numeroElementosTotales);

    /// <summary>La sociedad no borrada con ese identificador, con su TIPO_DE_EMPRESA y su PAIS; null si no existe.</summary>
    SOCIEDAD? ObtenerPorId(int idSociedad);

    /// <summary>Número de sociedades no borradas con ese código eSif, sin contar la indicada.</summary>
    int ContarConMismoCodigoEsif(string? codigoEsif, int idSociedadExcluida);

    int Insertar(SOCIEDAD sociedad);

    /// <summary>Guarda código eSif, nombre, país y tipo de empresa.</summary>
    void Actualizar(SOCIEDAD sociedad);

    void MarcarBorrado(int idSociedad);

    /// <summary>
    /// Filas que apuntan a la sociedad en PROCESO_ASOCIADO, USUARIO y USUARIO_SOCIEDAD_AUDITORES,
    /// borradas o no (como en el origen). Una sola consulta en lugar de las tres colecciones que carga el original.
    /// </summary>
    UsoDeSociedad ObtenerUso(int idSociedad);

    List<TIPO_DE_EMPRESA> ObtenerTiposDeEmpresa();

    /// <summary>Todos los países, ordenados por nombre.</summary>
    List<PAIS> ObtenerPaises();
}

/// <summary>Número de filas de otras tablas que apuntan a un tipo de auditoría.</summary>
public readonly record struct UsoDeTipoDeAuditoria(int ProcesosAsociados, int Usuarios);

public interface IRepositorioTiposDeAuditoria
{
    /// <summary>Tipos de auditoría no borrados que cumplen el filtro, ordenados y paginados (página desde 1).</summary>
    List<TIPO_DE_AUDITORIA> ObtenerFiltrados(FiltroTIPO_DE_AUDITORIA? filtro, string orden, int numeroPagina, int numeroElementosPorPagina, out int numeroElementosTotales);

    /// <summary>El tipo de auditoría no borrado con ese identificador; null si no existe.</summary>
    TIPO_DE_AUDITORIA? ObtenerPorId(int idTipoDeAuditoria);

    /// <summary>Número de tipos de auditoría no borrados con ese código, sin contar el indicado.</summary>
    int ContarConMismoCodigo(string? codigo, int idTipoDeAuditoriaExcluido);

    int Insertar(TIPO_DE_AUDITORIA tipoDeAuditoria);

    /// <summary>Guarda código y descripción.</summary>
    void Actualizar(TIPO_DE_AUDITORIA tipoDeAuditoria);

    void MarcarBorrado(int idTipoDeAuditoria);

    /// <summary>
    /// Filas que apuntan al tipo de auditoría en PROCESO_ASOCIADO y USUARIO, borradas o no (como en el origen).
    /// Una sola consulta en lugar de las dos colecciones que carga el original.
    /// </summary>
    UsoDeTipoDeAuditoria ObtenerUso(int idTipoDeAuditoria);
}

public interface IRepositorioUsuarios
{
    /// <summary>Usuarios no borrados con ese UID, con su PERFIL_DE_USUARIO.</summary>
    List<USUARIO> ObtenerPorUid(string uid);
}

public interface IRepositorioLogDeEntidades
{
    void Insertar(LOG_DE_ENTIDADES log);
}

/// <summary>Campos por los que se puede ordenar el listado de Procesos (colModel de Procesos.aspx).</summary>
public static class CamposOrdenProceso
{
    public const string IdProceso = "IdProceso";
    public const string Codigo = "Codigo";
    public const string Nombre = "Nombre";
    public const string TipoDeProceso = "TIPO_DE_PROCESO.Nombre";

    public static readonly string[] Todos = [IdProceso, Codigo, Nombre, TipoDeProceso];
}

/// <summary>Campos por los que se puede ordenar el listado de Subprocesos (colModel de Subprocesos.aspx).</summary>
public static class CamposOrdenSubproceso
{
    public const string IdSubproceso = "IdSubproceso";
    public const string Codigo = "Codigo";
    public const string Nombre = "Nombre";
    public const string Proceso = "PROCESO.Nombre";

    public static readonly string[] Todos = [IdSubproceso, Codigo, Nombre, Proceso];
}

/// <summary>Campos por los que se puede ordenar el listado de Sociedades (colModel de Sociedades.aspx).</summary>
public static class CamposOrdenSociedad
{
    public const string IdSociedad = "IdSociedad";
    public const string CodigoEsif = "CodigoEsif";
    public const string Nombre = "Nombre";
    public const string TipoDeEmpresa = "TIPO_DE_EMPRESA.Nombre";
    public const string Pais = "PAIS.Nombre";

    public static readonly string[] Todos = [IdSociedad, CodigoEsif, Nombre, TipoDeEmpresa, Pais];
}

/// <summary>Campos por los que se puede ordenar el listado de Tipos de Auditoría (colModel de TiposDeAuditoria.aspx).</summary>
public static class CamposOrdenTipoDeAuditoria
{
    public const string IdTipoDeAuditoria = "IdTipoDeAuditoria";
    public const string Codigo = "Codigo";
    public const string Descripcion = "Descripcion";

    public static readonly string[] Todos = [IdTipoDeAuditoria, Codigo, Descripcion];
}
