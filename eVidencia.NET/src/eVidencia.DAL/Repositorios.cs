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
