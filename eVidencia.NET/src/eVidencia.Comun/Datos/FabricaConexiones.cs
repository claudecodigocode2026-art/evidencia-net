using System.Data.Common;

namespace eVidencia.Comun.Datos;

/// <summary>
/// Crea conexiones abiertas a la base de datos. Componente común: no depende de eVidencia
/// ni de un proveedor concreto (el proveedor se inyecta como DbProviderFactory).
/// </summary>
public interface IFabricaConexiones
{
    DbConnection Abrir();
}

public sealed class FabricaConexiones(DbProviderFactory proveedor, string cadenaConexion) : IFabricaConexiones
{
    public DbConnection Abrir()
    {
        DbConnection conexion = proveedor.CreateConnection()
            ?? throw new InvalidOperationException("El proveedor de datos no ha creado la conexión.");
        conexion.ConnectionString = cadenaConexion;
        conexion.Open();
        return conexion;
    }
}

public static class ExtensionesDb
{
    /// <summary>Crea un comando con su texto y parámetros (nombre, valor).</summary>
    public static DbCommand Comando(this DbConnection conexion, string sql, params (string Nombre, object? Valor)[] parametros)
    {
        DbCommand comando = conexion.CreateCommand();
        comando.CommandText = sql;
        foreach ((string nombre, object? valor) in parametros)
        {
            DbParameter p = comando.CreateParameter();
            p.ParameterName = nombre;
            p.Value = valor ?? DBNull.Value;
            comando.Parameters.Add(p);
        }
        return comando;
    }

    /// <summary>
    /// Prepara un valor para una búsqueda "contiene" con LIKE ... ESCAPE '~':
    /// los comodines del texto del usuario se tratan como literales.
    /// </summary>
    public static string PatronContiene(string texto)
    {
        string escapado = texto.Replace("~", "~~").Replace("%", "~%").Replace("_", "~_").Replace("[", "~[");
        return "%" + escapado + "%";
    }
}
