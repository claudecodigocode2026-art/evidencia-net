using eVidencia.Comun.Grid;
using eVidencia.Entidades;
using eVidencia.Entidades.Filtros;

namespace eVidencia.DAL.Memoria;

// Implementación en memoria: permite ejecutar la demo sin base de datos y probar la lógica de negocio.
// Los datos se pierden al parar la aplicación. No es parte de la migración: es un apoyo para la demo.

/// <summary>Datos en memoria compartidos por los repositorios.</summary>
public sealed class AlmacenMemoria
{
    public readonly object Cerrojo = new();
    public List<TIPO_DE_PROCESO> TiposDeProceso { get; } = DatosDeEjemplo.TiposDeProceso();
    public List<PERFIL_DE_USUARIO> Perfiles { get; } = DatosDeEjemplo.Perfiles();
    public List<USUARIO> Usuarios { get; } = DatosDeEjemplo.Usuarios();
    public List<PROCESO> Procesos { get; } = DatosDeEjemplo.Procesos();
    public List<SUBPROCESO> Subprocesos { get; } = [];
    /// <summary>IdProceso de cada fila de PROCESO_ASOCIADO.</summary>
    public List<int> ProcesosAsociados { get; } = DatosDeEjemplo.ProcesosConAsociado();
    public List<LOG_DE_ENTIDADES> Logs { get; } = [];

    public AlmacenMemoria()
    {
        // Cada proceso lleva su subproceso "Otros", como hace la aplicación al crearlo.
        foreach (PROCESO p in Procesos)
            Subprocesos.Add(new SUBPROCESO { IdSubproceso = Subprocesos.Count + 1, Codigo = "", Nombre = "Otros", IdProceso = p.IdProceso, TipoOtros = true });
    }
}

public sealed class RepositorioProcesosMemoria(AlmacenMemoria almacen) : IRepositorioProcesos
{
    // Las búsquedas y comparaciones no distinguen mayúsculas, como la intercalación habitual de SQL Server.
    private const StringComparison Comparacion = StringComparison.OrdinalIgnoreCase;

    public List<PROCESO> ObtenerFiltrados(FiltroPROCESO? filtro, string orden, int numeroPagina, int numeroElementosPorPagina, out int numeroElementosTotales)
    {
        List<CriterioOrden> criterios = OrdenGrid.Interpretar(orden, CamposOrdenProceso.Todos);
        if (criterios.Count == 0)
            throw new ArgumentException("No se ha indicado el orden del listado.");

        lock (almacen.Cerrojo)
        {
            IEnumerable<PROCESO> consulta = almacen.Procesos.Where(p => !p.Borrado).Select(Copiar);
            if (filtro != null)
            {
                if (!string.IsNullOrEmpty(filtro.Codigo))
                    consulta = consulta.Where(p => (p.Codigo ?? "").Contains(filtro.Codigo, Comparacion));
                if (!string.IsNullOrEmpty(filtro.Nombre))
                    consulta = consulta.Where(p => (p.Nombre ?? "").Contains(filtro.Nombre, Comparacion));
            }

            List<PROCESO> filtrados = consulta.ToList();
            numeroElementosTotales = filtrados.Count;

            IOrderedEnumerable<PROCESO>? ordenados = null;
            foreach (CriterioOrden c in criterios)
            {
                Func<PROCESO, object?> clave = c.Campo switch
                {
                    CamposOrdenProceso.IdProceso => p => p.IdProceso,
                    CamposOrdenProceso.Codigo => p => p.Codigo,
                    CamposOrdenProceso.Nombre => p => p.Nombre,
                    _ => p => p.TIPO_DE_PROCESO!.Nombre
                };
                ordenados = ordenados == null
                    ? (c.Descendente ? filtrados.OrderByDescending(clave, ComparadorClave.Instancia) : filtrados.OrderBy(clave, ComparadorClave.Instancia))
                    : (c.Descendente ? ordenados.ThenByDescending(clave, ComparadorClave.Instancia) : ordenados.ThenBy(clave, ComparadorClave.Instancia));
            }

            return ordenados!.Skip(numeroElementosPorPagina * (numeroPagina - 1)).Take(numeroElementosPorPagina).ToList();
        }
    }

    public PROCESO? ObtenerPorId(int idProceso)
    {
        lock (almacen.Cerrojo)
        {
            PROCESO? p = almacen.Procesos.FirstOrDefault(x => x.IdProceso == idProceso && !x.Borrado);
            return p == null ? null : Copiar(p);
        }
    }

    public int ContarConMismoCodigo(string? codigo, int idProcesoExcluido)
    {
        lock (almacen.Cerrojo)
            return almacen.Procesos.Count(p => string.Equals(p.Codigo, codigo, Comparacion) && p.IdProceso != idProcesoExcluido && !p.Borrado);
    }

    public int Insertar(PROCESO proceso)
    {
        lock (almacen.Cerrojo)
        {
            int id = almacen.Procesos.Count == 0 ? 1 : almacen.Procesos.Max(p => p.IdProceso) + 1;
            almacen.Procesos.Add(new PROCESO { IdProceso = id, Codigo = proceso.Codigo, Nombre = proceso.Nombre, IdTipoDeProceso = proceso.IdTipoDeProceso });
            return id;
        }
    }

    public void Actualizar(PROCESO proceso)
    {
        lock (almacen.Cerrojo)
        {
            PROCESO p = almacen.Procesos.First(x => x.IdProceso == proceso.IdProceso);
            p.Codigo = proceso.Codigo;
            p.Nombre = proceso.Nombre;
            p.IdTipoDeProceso = proceso.IdTipoDeProceso;
        }
    }

    public void MarcarBorrado(int idProceso)
    {
        lock (almacen.Cerrojo)
            almacen.Procesos.First(x => x.IdProceso == idProceso).Borrado = true;
    }

    public int ContarProcesosAsociados(int idProceso)
    {
        lock (almacen.Cerrojo)
            return almacen.ProcesosAsociados.Count(id => id == idProceso);
    }

    public List<TIPO_DE_PROCESO> ObtenerTiposDeProceso()
    {
        lock (almacen.Cerrojo)
            return almacen.TiposDeProceso
                .OrderBy(t => t.Nombre, StringComparer.Create(new System.Globalization.CultureInfo("es-ES"), ignoreCase: true))
                .Select(t => new TIPO_DE_PROCESO { IdTipoDeProceso = t.IdTipoDeProceso, Nombre = t.Nombre })
                .ToList();
    }

    private PROCESO Copiar(PROCESO p)
    {
        TIPO_DE_PROCESO? tipo = almacen.TiposDeProceso.FirstOrDefault(t => t.IdTipoDeProceso == p.IdTipoDeProceso);
        return new PROCESO
        {
            IdProceso = p.IdProceso,
            Codigo = p.Codigo,
            Nombre = p.Nombre,
            IdTipoDeProceso = p.IdTipoDeProceso,
            Borrado = p.Borrado,
            TIPO_DE_PROCESO = tipo == null ? null : new TIPO_DE_PROCESO { IdTipoDeProceso = tipo.IdTipoDeProceso, Nombre = tipo.Nombre }
        };
    }

    /// <summary>Compara claves de orden: números como números y textos en español sin distinguir mayúsculas.</summary>
    private sealed class ComparadorClave : IComparer<object?>
    {
        public static readonly ComparadorClave Instancia = new();
        private static readonly StringComparer Texto = StringComparer.Create(new System.Globalization.CultureInfo("es-ES"), ignoreCase: true);

        public int Compare(object? x, object? y) => (x, y) switch
        {
            (int a, int b) => a.CompareTo(b),
            _ => Texto.Compare(x?.ToString(), y?.ToString())
        };
    }
}

public sealed class RepositorioSubprocesosMemoria(AlmacenMemoria almacen) : IRepositorioSubprocesos
{
    public int Insertar(SUBPROCESO subproceso)
    {
        lock (almacen.Cerrojo)
        {
            int id = almacen.Subprocesos.Count == 0 ? 1 : almacen.Subprocesos.Max(s => s.IdSubproceso) + 1;
            almacen.Subprocesos.Add(new SUBPROCESO
            {
                IdSubproceso = id, Codigo = subproceso.Codigo, Nombre = subproceso.Nombre,
                IdProceso = subproceso.IdProceso, TipoOtros = subproceso.TipoOtros
            });
            return id;
        }
    }

    public SUBPROCESO? ObtenerOtrosDeProceso(int idProceso)
    {
        lock (almacen.Cerrojo)
        {
            SUBPROCESO? s = almacen.Subprocesos.FirstOrDefault(x => x.IdProceso == idProceso && !x.Borrado && x.TipoOtros);
            return s == null ? null : new SUBPROCESO
            {
                IdSubproceso = s.IdSubproceso, Codigo = s.Codigo, Nombre = s.Nombre,
                IdProceso = s.IdProceso, Borrado = s.Borrado, TipoOtros = s.TipoOtros
            };
        }
    }

    public void MarcarBorrado(int idSubproceso)
    {
        lock (almacen.Cerrojo)
            almacen.Subprocesos.First(x => x.IdSubproceso == idSubproceso).Borrado = true;
    }
}

public sealed class RepositorioUsuariosMemoria(AlmacenMemoria almacen) : IRepositorioUsuarios
{
    public List<USUARIO> ObtenerPorUid(string uid)
    {
        lock (almacen.Cerrojo)
            return almacen.Usuarios
                .Where(u => string.Equals(u.UID, uid, StringComparison.OrdinalIgnoreCase) && !u.Borrado)
                .Select(u => new USUARIO
                {
                    IdUsuario = u.IdUsuario, Nombre = u.Nombre, Apellidos = u.Apellidos, UID = u.UID,
                    IdPerfilDeUsuario = u.IdPerfilDeUsuario, Borrado = u.Borrado,
                    PERFIL_DE_USUARIO = almacen.Perfiles.FirstOrDefault(p => p.IdPerfilDeUsuario == u.IdPerfilDeUsuario)
                })
                .ToList();
    }
}

public sealed class RepositorioLogDeEntidadesMemoria(AlmacenMemoria almacen) : IRepositorioLogDeEntidades
{
    public void Insertar(LOG_DE_ENTIDADES log)
    {
        lock (almacen.Cerrojo)
        {
            log.IdLogDeEntidades = almacen.Logs.Count + 1;
            almacen.Logs.Add(log);
        }
    }
}
