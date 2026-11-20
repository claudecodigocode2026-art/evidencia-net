using System.Data.Common;
using eVidencia.Comun.Datos;
using eVidencia.Comun.Grid;
using eVidencia.Entidades;
using eVidencia.Entidades.Filtros;

namespace eVidencia.DAL.SqlServer;

// Implementación sobre la base de datos de eVidencia (SQL Server). Tablas y columnas según
// eVidencias.DAL/eVidencias.edmx. Se usa ADO.NET (System.Data.Common) con consultas parametrizadas.

public sealed class RepositorioProcesosSql(IFabricaConexiones fabrica) : IRepositorioProcesos
{
    private const string Columnas =
        "p.IdProceso, p.Codigo, p.Nombre, p.IdTipoDeProceso, p.Borrado, t.Nombre AS TipoNombre";

    private const string Desde =
        " FROM PROCESO p INNER JOIN TIPO_DE_PROCESO t ON t.IdTipoDeProceso = p.IdTipoDeProceso";

    private static readonly Dictionary<string, string> ColumnaDeOrden = new()
    {
        [CamposOrdenProceso.IdProceso] = "p.IdProceso",
        [CamposOrdenProceso.Codigo] = "p.Codigo",
        [CamposOrdenProceso.Nombre] = "p.Nombre",
        [CamposOrdenProceso.TipoDeProceso] = "t.Nombre"
    };

    public List<PROCESO> ObtenerFiltrados(FiltroPROCESO? filtro, string orden, int numeroPagina, int numeroElementosPorPagina, out int numeroElementosTotales)
    {
        List<(string, object?)> parametros = [];
        string donde = " WHERE p.Borrado = 0";
        if (filtro != null)
        {
            if (!string.IsNullOrEmpty(filtro.Codigo))
            {
                donde += " AND p.Codigo LIKE @Codigo ESCAPE '~'";
                parametros.Add(("@Codigo", ExtensionesDb.PatronContiene(filtro.Codigo)));
            }
            if (!string.IsNullOrEmpty(filtro.Nombre))
            {
                donde += " AND p.Nombre LIKE @Nombre ESCAPE '~'";
                parametros.Add(("@Nombre", ExtensionesDb.PatronContiene(filtro.Nombre)));
            }
        }

        List<CriterioOrden> criterios = OrdenGrid.Interpretar(orden, CamposOrdenProceso.Todos);
        if (criterios.Count == 0)
            throw new ArgumentException("No se ha indicado el orden del listado.");
        string ordenSql = string.Join(", ", criterios.Select(c => ColumnaDeOrden[c.Campo] + (c.Descendente ? " DESC" : " ASC")));

        using DbConnection conexion = fabrica.Abrir();

        using (DbCommand contar = conexion.Comando("SELECT COUNT(*)" + Desde + donde, [.. parametros]))
        {
            numeroElementosTotales = Convert.ToInt32(contar.ExecuteScalar());
        }

        List<(string, object?)> parametrosPagina = [.. parametros,
            ("@Saltar", numeroElementosPorPagina * (numeroPagina - 1)),
            ("@Tomar", numeroElementosPorPagina)];

        string sql = "SELECT " + Columnas + Desde + donde + " ORDER BY " + ordenSql +
                     " OFFSET @Saltar ROWS FETCH NEXT @Tomar ROWS ONLY";

        List<PROCESO> procesos = [];
        using DbCommand consulta = conexion.Comando(sql, [.. parametrosPagina]);
        using DbDataReader lector = consulta.ExecuteReader();
        while (lector.Read()) procesos.Add(Leer(lector));
        return procesos;
    }

    public PROCESO? ObtenerPorId(int idProceso)
    {
        using DbConnection conexion = fabrica.Abrir();
        using DbCommand consulta = conexion.Comando(
            "SELECT " + Columnas + Desde + " WHERE p.IdProceso = @IdProceso AND p.Borrado = 0",
            ("@IdProceso", idProceso));
        using DbDataReader lector = consulta.ExecuteReader();
        return lector.Read() ? Leer(lector) : null;
    }

    public int ContarConMismoCodigo(string? codigo, int idProcesoExcluido)
    {
        using DbConnection conexion = fabrica.Abrir();
        using DbCommand consulta = conexion.Comando(
            "SELECT COUNT(*) FROM PROCESO WHERE Codigo = @Codigo AND IdProceso <> @IdProceso AND Borrado = 0",
            ("@Codigo", codigo), ("@IdProceso", idProcesoExcluido));
        return Convert.ToInt32(consulta.ExecuteScalar());
    }

    public int Insertar(PROCESO proceso)
    {
        using DbConnection conexion = fabrica.Abrir();
        using DbCommand comando = conexion.Comando(
            "INSERT INTO PROCESO (Codigo, Nombre, IdTipoDeProceso, Borrado) VALUES (@Codigo, @Nombre, @IdTipoDeProceso, 0); " +
            "SELECT CAST(SCOPE_IDENTITY() AS int);",
            ("@Codigo", proceso.Codigo), ("@Nombre", proceso.Nombre), ("@IdTipoDeProceso", proceso.IdTipoDeProceso));
        return Convert.ToInt32(comando.ExecuteScalar());
    }

    public void Actualizar(PROCESO proceso)
    {
        using DbConnection conexion = fabrica.Abrir();
        using DbCommand comando = conexion.Comando(
            "UPDATE PROCESO SET Codigo = @Codigo, Nombre = @Nombre, IdTipoDeProceso = @IdTipoDeProceso WHERE IdProceso = @IdProceso",
            ("@Codigo", proceso.Codigo), ("@Nombre", proceso.Nombre),
            ("@IdTipoDeProceso", proceso.IdTipoDeProceso), ("@IdProceso", proceso.IdProceso));
        comando.ExecuteNonQuery();
    }

    public void MarcarBorrado(int idProceso)
    {
        using DbConnection conexion = fabrica.Abrir();
        using DbCommand comando = conexion.Comando(
            "UPDATE PROCESO SET Borrado = 1 WHERE IdProceso = @IdProceso", ("@IdProceso", idProceso));
        comando.ExecuteNonQuery();
    }

    public int ContarProcesosAsociados(int idProceso)
    {
        using DbConnection conexion = fabrica.Abrir();
        using DbCommand consulta = conexion.Comando(
            "SELECT COUNT(*) FROM PROCESO_ASOCIADO WHERE IdProceso = @IdProceso", ("@IdProceso", idProceso));
        return Convert.ToInt32(consulta.ExecuteScalar());
    }

    public List<TIPO_DE_PROCESO> ObtenerTiposDeProceso()
    {
        List<TIPO_DE_PROCESO> tipos = [];
        using DbConnection conexion = fabrica.Abrir();
        using DbCommand consulta = conexion.Comando("SELECT IdTipoDeProceso, Nombre FROM TIPO_DE_PROCESO ORDER BY Nombre");
        using DbDataReader lector = consulta.ExecuteReader();
        while (lector.Read())
            tipos.Add(new TIPO_DE_PROCESO { IdTipoDeProceso = lector.GetInt32(0), Nombre = lector.GetString(1) });
        return tipos;
    }

    public List<PROCESO> ObtenerNoBorradosPorCodigo()
    {
        List<PROCESO> procesos = [];
        using DbConnection conexion = fabrica.Abrir();
        using DbCommand consulta = conexion.Comando("SELECT " + Columnas + Desde + " WHERE p.Borrado = 0 ORDER BY p.Codigo");
        using DbDataReader lector = consulta.ExecuteReader();
        while (lector.Read()) procesos.Add(Leer(lector));
        return procesos;
    }

    public List<PROCESO> ObtenerConSubprocesos()
    {
        List<PROCESO> procesos = [];
        using DbConnection conexion = fabrica.Abrir();
        using DbCommand consulta = conexion.Comando(
            "SELECT " + Columnas + Desde + " WHERE p.Borrado = 0 AND EXISTS (SELECT 1 FROM SUBPROCESO s WHERE s.IdProceso = p.IdProceso) ORDER BY p.IdProceso");
        using DbDataReader lector = consulta.ExecuteReader();
        while (lector.Read()) procesos.Add(Leer(lector));
        return procesos;
    }

    private static PROCESO Leer(DbDataReader lector) => new()
    {
        IdProceso = lector.GetInt32(0),
        Codigo = lector.GetString(1),
        Nombre = lector.GetString(2),
        IdTipoDeProceso = lector.GetInt32(3),
        Borrado = lector.GetBoolean(4),
        TIPO_DE_PROCESO = new TIPO_DE_PROCESO { IdTipoDeProceso = lector.GetInt32(3), Nombre = lector.GetString(5) }
    };
}

public sealed class RepositorioSubprocesosSql(IFabricaConexiones fabrica) : IRepositorioSubprocesos
{
    public int Insertar(SUBPROCESO subproceso)
    {
        using DbConnection conexion = fabrica.Abrir();
        using DbCommand comando = conexion.Comando(
            "INSERT INTO SUBPROCESO (Codigo, Nombre, IdProceso, Borrado, TipoOtros) VALUES (@Codigo, @Nombre, @IdProceso, 0, @TipoOtros); " +
            "SELECT CAST(SCOPE_IDENTITY() AS int);",
            ("@Codigo", subproceso.Codigo), ("@Nombre", subproceso.Nombre),
            ("@IdProceso", subproceso.IdProceso), ("@TipoOtros", subproceso.TipoOtros));
        return Convert.ToInt32(comando.ExecuteScalar());
    }

    public SUBPROCESO? ObtenerOtrosDeProceso(int idProceso)
    {
        using DbConnection conexion = fabrica.Abrir();
        using DbCommand consulta = conexion.Comando(
            "SELECT TOP (1) IdSubproceso, Codigo, Nombre, IdProceso, Borrado, TipoOtros FROM SUBPROCESO " +
            "WHERE IdProceso = @IdProceso AND Borrado = 0 AND TipoOtros = 1",
            ("@IdProceso", idProceso));
        using DbDataReader lector = consulta.ExecuteReader();
        if (!lector.Read()) return null;
        return new SUBPROCESO
        {
            IdSubproceso = lector.GetInt32(0),
            Codigo = lector.GetString(1),
            Nombre = lector.GetString(2),
            IdProceso = lector.GetInt32(3),
            Borrado = lector.GetBoolean(4),
            TipoOtros = lector.GetBoolean(5)
        };
    }

    public void MarcarBorrado(int idSubproceso)
    {
        using DbConnection conexion = fabrica.Abrir();
        using DbCommand comando = conexion.Comando(
            "UPDATE SUBPROCESO SET Borrado = 1 WHERE IdSubproceso = @IdSubproceso", ("@IdSubproceso", idSubproceso));
        comando.ExecuteNonQuery();
    }

    private const string ColumnasConProceso =
        "s.IdSubproceso, s.Codigo, s.Nombre, s.IdProceso, s.Borrado, s.TipoOtros, p.Nombre AS ProcesoNombre, p.Codigo AS ProcesoCodigo";

    // LEFT JOIN: la relación SUBPROCESO.PROCESO del modelo original no filtra por PROCESO.Borrado.
    private const string DesdeConProceso =
        " FROM SUBPROCESO s LEFT JOIN PROCESO p ON p.IdProceso = s.IdProceso";

    private static readonly Dictionary<string, string> ColumnaDeOrden = new()
    {
        [CamposOrdenSubproceso.IdSubproceso] = "s.IdSubproceso",
        [CamposOrdenSubproceso.Codigo] = "s.Codigo",
        [CamposOrdenSubproceso.Nombre] = "s.Nombre",
        [CamposOrdenSubproceso.Proceso] = "p.Nombre"
    };

    public List<SUBPROCESO> ObtenerFiltrados(FiltroSUBPROCESO? filtro, string orden, int numeroPagina, int numeroElementosPorPagina, out int numeroElementosTotales)
    {
        List<(string, object?)> parametros = [];
        string donde = " WHERE s.Borrado = 0 AND s.TipoOtros = 0";

        if (filtro != null)
        {
            if (!string.IsNullOrEmpty(filtro.Codigo))
            {
                donde += " AND s.Codigo LIKE @Codigo ESCAPE '~'";
                parametros.Add(("@Codigo", ExtensionesDb.PatronContiene(filtro.Codigo)));
            }
            if (!string.IsNullOrEmpty(filtro.Nombre))
            {
                donde += " AND s.Nombre LIKE @Nombre ESCAPE '~'";
                parametros.Add(("@Nombre", ExtensionesDb.PatronContiene(filtro.Nombre)));
            }
            if (filtro.ListaIdProceso != null)
            {
                if (filtro.ListaIdProceso.Count == 0)
                {
                    donde += " AND 1 = 0";
                }
                else
                {
                    // Un parámetro por identificador: nunca se concatena el valor en la sentencia.
                    List<string> nombres = [];
                    for (int i = 0; i < filtro.ListaIdProceso.Count; i++)
                    {
                        nombres.Add("@IdProceso" + i);
                        parametros.Add(("@IdProceso" + i, filtro.ListaIdProceso[i]));
                    }
                    donde += " AND s.IdProceso IN (" + string.Join(", ", nombres) + ")";
                }
            }
        }

        List<CriterioOrden> criterios = OrdenGrid.Interpretar(orden, CamposOrdenSubproceso.Todos);
        if (criterios.Count == 0)
            throw new ArgumentException("No se ha indicado el orden del listado.");
        string ordenSql = string.Join(", ", criterios.Select(c => ColumnaDeOrden[c.Campo] + (c.Descendente ? " DESC" : " ASC")));

        using DbConnection conexion = fabrica.Abrir();
        using (DbCommand contar = conexion.Comando("SELECT COUNT(*)" + DesdeConProceso + donde, [.. parametros]))
        {
            numeroElementosTotales = Convert.ToInt32(contar.ExecuteScalar());
        }

        List<(string, object?)> parametrosPagina = [.. parametros,
            ("@Saltar", numeroElementosPorPagina * (numeroPagina - 1)),
            ("@Tomar", numeroElementosPorPagina)];
        string sql = "SELECT " + ColumnasConProceso + DesdeConProceso + donde + " ORDER BY " + ordenSql +
                     " OFFSET @Saltar ROWS FETCH NEXT @Tomar ROWS ONLY";

        List<SUBPROCESO> subprocesos = [];
        using DbCommand consulta = conexion.Comando(sql, [.. parametrosPagina]);
        using DbDataReader lector = consulta.ExecuteReader();
        while (lector.Read()) subprocesos.Add(LeerConProceso(lector));
        return subprocesos;
    }

    public SUBPROCESO? ObtenerPorId(int idSubproceso)
    {
        using DbConnection conexion = fabrica.Abrir();
        using DbCommand consulta = conexion.Comando(
            "SELECT " + ColumnasConProceso + DesdeConProceso + " WHERE s.IdSubproceso = @IdSubproceso AND s.Borrado = 0",
            ("@IdSubproceso", idSubproceso));
        using DbDataReader lector = consulta.ExecuteReader();
        return lector.Read() ? LeerConProceso(lector) : null;
    }

    public int ContarConMismoCodigo(string? codigo, int idSubprocesoExcluido)
    {
        using DbConnection conexion = fabrica.Abrir();
        using DbCommand consulta = conexion.Comando(
            "SELECT COUNT(*) FROM SUBPROCESO WHERE Codigo = @Codigo AND IdSubproceso <> @IdSubproceso AND Borrado = 0",
            ("@Codigo", codigo), ("@IdSubproceso", idSubprocesoExcluido));
        return Convert.ToInt32(consulta.ExecuteScalar());
    }

    public void Actualizar(SUBPROCESO subproceso)
    {
        using DbConnection conexion = fabrica.Abrir();
        using DbCommand comando = conexion.Comando(
            "UPDATE SUBPROCESO SET Codigo = @Codigo, Nombre = @Nombre, IdProceso = @IdProceso WHERE IdSubproceso = @IdSubproceso",
            ("@Codigo", subproceso.Codigo), ("@Nombre", subproceso.Nombre),
            ("@IdProceso", subproceso.IdProceso), ("@IdSubproceso", subproceso.IdSubproceso));
        comando.ExecuteNonQuery();
    }

    public int ContarProcesosAsociadosNoBorrados(int idSubproceso)
    {
        using DbConnection conexion = fabrica.Abrir();
        using DbCommand consulta = conexion.Comando(
            "SELECT COUNT(*) FROM PROCESO_ASOCIADO WHERE IdSubproceso = @IdSubproceso AND Borrado = 0",
            ("@IdSubproceso", idSubproceso));
        return Convert.ToInt32(consulta.ExecuteScalar());
    }

    private static SUBPROCESO LeerConProceso(DbDataReader lector) => new()
    {
        IdSubproceso = lector.GetInt32(0),
        Codigo = lector.IsDBNull(1) ? null : lector.GetString(1),
        Nombre = lector.IsDBNull(2) ? null : lector.GetString(2),
        IdProceso = lector.GetInt32(3),
        Borrado = lector.GetBoolean(4),
        TipoOtros = lector.GetBoolean(5),
        PROCESO = lector.IsDBNull(6) ? null : new PROCESO
        {
            IdProceso = lector.GetInt32(3),
            Nombre = lector.GetString(6),
            Codigo = lector.IsDBNull(7) ? null : lector.GetString(7)
        }
    };
}

public sealed class RepositorioUsuariosSql(IFabricaConexiones fabrica) : IRepositorioUsuarios
{
    public List<USUARIO> ObtenerPorUid(string uid)
    {
        List<USUARIO> usuarios = [];
        using DbConnection conexion = fabrica.Abrir();
        using DbCommand consulta = conexion.Comando(
            "SELECT u.IdUsuario, u.Nombre, u.Apellidos, u.UID, u.IdPerfilDeUsuario, u.Borrado, pf.Nombre AS PerfilNombre " +
            "FROM USUARIO u INNER JOIN PERFIL_DE_USUARIO pf ON pf.IdPerfilDeUsuario = u.IdPerfilDeUsuario " +
            "WHERE u.UID = @Uid AND u.Borrado = 0",
            ("@Uid", uid));
        using DbDataReader lector = consulta.ExecuteReader();
        while (lector.Read())
        {
            usuarios.Add(new USUARIO
            {
                IdUsuario = lector.GetInt32(0),
                Nombre = lector.GetString(1),
                Apellidos = lector.GetString(2),
                UID = lector.IsDBNull(3) ? null : lector.GetString(3),
                IdPerfilDeUsuario = lector.GetInt32(4),
                Borrado = lector.GetBoolean(5),
                PERFIL_DE_USUARIO = new PERFIL_DE_USUARIO { IdPerfilDeUsuario = lector.GetInt32(4), Nombre = lector.GetString(6) }
            });
        }
        return usuarios;
    }
}

public sealed class RepositorioLogDeEntidadesSql(IFabricaConexiones fabrica) : IRepositorioLogDeEntidades
{
    public void Insertar(LOG_DE_ENTIDADES log)
    {
        using DbConnection conexion = fabrica.Abrir();
        using DbCommand comando = conexion.Comando(
            "INSERT INTO LOG_DE_ENTIDADES (Fecha, IdUsuario, IdTipoDeAccion, IdSociedad, IdPeriodo, Descripcion, IdProceso, " +
            "IdPlanificadorDeEvidencias, IdActividadDeControl, IdSubproceso, IdAreaResponsable) VALUES (@Fecha, @IdUsuario, " +
            "@IdTipoDeAccion, @IdSociedad, @IdPeriodo, @Descripcion, @IdProceso, @IdPlanificadorDeEvidencias, " +
            "@IdActividadDeControl, @IdSubproceso, @IdAreaResponsable)",
            ("@Fecha", log.Fecha), ("@IdUsuario", log.IdUsuario), ("@IdTipoDeAccion", log.IdTipoDeAccion),
            ("@IdSociedad", log.IdSociedad), ("@IdPeriodo", log.IdPeriodo), ("@Descripcion", log.Descripcion),
            ("@IdProceso", log.IdProceso), ("@IdPlanificadorDeEvidencias", log.IdPlanificadorDeEvidencias),
            ("@IdActividadDeControl", log.IdActividadDeControl), ("@IdSubproceso", log.IdSubproceso),
            ("@IdAreaResponsable", log.IdAreaResponsable));
        comando.ExecuteNonQuery();
    }
}
