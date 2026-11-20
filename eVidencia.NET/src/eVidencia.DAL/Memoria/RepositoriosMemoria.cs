using eVidencia.Comun.Excel;
using eVidencia.Comun.Grid;
using eVidencia.Entidades;
using eVidencia.Entidades.Filtros;

namespace eVidencia.DAL.Memoria;

// Implementación en memoria: permite ejecutar la demo sin base de datos y probar la lógica de negocio.
// Los datos se pierden al parar la aplicación. No es parte de la migración: es un apoyo para la demo.

/// <summary>
/// Datos en memoria compartidos por los repositorios. Se cargan de un libro Excel (una hoja = una tabla,
/// con los nombres de tabla y columna del modelo de eVidencia) o, si no hay libro, de los datos de ejemplo.
/// Los cambios se quedan en memoria: no se escriben en el Excel.
/// </summary>
public sealed class AlmacenMemoria
{
    public readonly object Cerrojo = new();
    public List<TIPO_DE_PROCESO> TiposDeProceso { get; private init; } = [];
    public List<PERFIL_DE_USUARIO> Perfiles { get; private init; } = [];
    public List<USUARIO> Usuarios { get; private init; } = [];
    public List<PROCESO> Procesos { get; private init; } = [];
    public List<SUBPROCESO> Subprocesos { get; private init; } = [];
    /// <summary>Filas de PROCESO_ASOCIADO (solo las columnas que usan las pantallas migradas).</summary>
    public List<PROCESO_ASOCIADO> ProcesosAsociados { get; private init; } = [];
    public List<TIPO_DE_AUDITORIA> TiposDeAuditoria { get; private init; } = [];
    /// <summary>True si los tipos de auditoría no vienen del libro Excel y se han generado como ejemplo.</summary>
    public bool TiposDeAuditoriaDeEjemplo { get; private init; }
    public List<PAIS> Paises { get; private init; } = [];
    public List<TIPO_DE_EMPRESA> TiposDeEmpresa { get; private init; } = [];
    public List<SOCIEDAD> Sociedades { get; private init; } = [];
    public List<USUARIO_SOCIEDAD_AUDITORES> UsuariosSociedadAuditores { get; private init; } = [];
    /// <summary>True si las sociedades no vienen del libro Excel y se han generado como ejemplo.</summary>
    public bool SociedadesDeEjemplo { get; private init; }
    /// <summary>Líneas de LOG_DE_ENTIDADES creadas durante la ejecución.</summary>
    public List<LOG_DE_ENTIDADES> Logs { get; } = [];
    /// <summary>Número de líneas de LOG_DE_ENTIDADES que ya había en el origen (no se cargan en memoria).</summary>
    public int LogsEnOrigen { get; private init; }
    /// <summary>Descripción del origen de los datos, para mostrarla en pantalla.</summary>
    public string Origen { get; private init; } = "";

    private AlmacenMemoria() { }

    /// <summary>Datos de ejemplo inventados (ver DatosDeEjemplo).</summary>
    public static AlmacenMemoria DeEjemplo()
    {
        List<PROCESO> procesos = DatosDeEjemplo.Procesos();
        // Cada proceso lleva su subproceso "Otros", como hace la aplicación al crearlo.
        List<SUBPROCESO> subprocesos = procesos
            .Select((p, i) => new SUBPROCESO { IdSubproceso = i + 1, Codigo = "", Nombre = "Otros", IdProceso = p.IdProceso, TipoOtros = true })
            .Concat(DatosDeEjemplo.Subprocesos())
            .ToList();

        return new AlmacenMemoria
        {
            TiposDeProceso = DatosDeEjemplo.TiposDeProceso(),
            Perfiles = DatosDeEjemplo.Perfiles(),
            Usuarios = DatosDeEjemplo.Usuarios(),
            Procesos = procesos,
            Subprocesos = subprocesos,
            ProcesosAsociados = DatosDeEjemplo.ProcesosAsociados(),
            TiposDeAuditoria = DatosDeEjemplo.TiposDeAuditoria(),
            TiposDeAuditoriaDeEjemplo = true,
            Paises = DatosDeEjemplo.Paises(),
            TiposDeEmpresa = DatosDeEjemplo.TiposDeEmpresa(),
            Sociedades = DatosDeEjemplo.Sociedades(),
            UsuariosSociedadAuditores = DatosDeEjemplo.UsuariosSociedadAuditores(),
            SociedadesDeEjemplo = true,
            Origen = "de ejemplo en memoria"
        };
    }

    /// <summary>
    /// Carga las tablas que usa la Administración de Procesos desde un libro Excel con una hoja por tabla
    /// (TIPO_DE_PROCESO, PERFIL_DE_USUARIO, USUARIO, PROCESO, SUBPROCESO, PROCESO_ASOCIADO, LOG_DE_ENTIDADES).
    /// </summary>
    public static AlmacenMemoria DesdeExcel(string ruta)
    {
        using LectorXlsx libro = new(ruta);

        List<USUARIO> usuarios = libro.LeerTabla("USUARIO").Select(f => new USUARIO
        {
            IdUsuario = ValorXlsx.Entero(f["IdUsuario"]),
            Nombre = f["Nombre"] ?? "",
            Apellidos = f["Apellidos"] ?? "",
            UID = f["UID"],
            IdPerfilDeUsuario = ValorXlsx.Entero(f["IdPerfilDeUsuario"]),
            IdSociedad = f.ContainsKey("IdSociedad") ? ValorXlsx.EnteroONulo(f["IdSociedad"]) : null,
            IdTipoDeAuditoria = f.ContainsKey("IdTipoDeAuditoria") ? ValorXlsx.EnteroONulo(f["IdTipoDeAuditoria"]) : null,
            Borrado = ValorXlsx.Logico(f["Borrado"])
        }).ToList();

        List<PROCESO_ASOCIADO> procesosAsociados = libro.LeerTabla("PROCESO_ASOCIADO").Select(f => new PROCESO_ASOCIADO
        {
            IdProcesoAsociado = ValorXlsx.Entero(f["IdProcesoAsociado"]),
            IdProceso = ValorXlsx.Entero(f["IdProceso"]),
            IdSubproceso = ValorXlsx.EnteroONulo(f["IdSubproceso"]),
            IdSociedad = f.ContainsKey("IdSociedad") ? ValorXlsx.EnteroONulo(f["IdSociedad"]) ?? 0 : 0,
            IdTipoDeAuditoria = f.ContainsKey("IdTipoDeAuditoria") ? ValorXlsx.EnteroONulo(f["IdTipoDeAuditoria"]) ?? 0 : 0,
            Borrado = ValorXlsx.Logico(f["Borrado"])
        }).ToList();

        // Sociedades, países y tipos de empresa: si el libro no trae la hoja SOCIEDAD (el libro de demostración
        // sin datos personales no la incluye), se generan sociedades de ejemplo con los identificadores que
        // usan los usuarios y los procesos asignados del libro, para que las relaciones sigan siendo coherentes.
        bool hayHojaSociedad = libro.ExisteHoja("SOCIEDAD");
        List<PAIS> paises = libro.ExisteHoja("PAIS")
            ? libro.LeerTabla("PAIS").Select(f => new PAIS { IdPais = ValorXlsx.Entero(f["IdPais"]), Nombre = f["Nombre"] ?? "" }).ToList()
            : DatosDeEjemplo.Paises();
        List<TIPO_DE_EMPRESA> tiposDeEmpresa = libro.ExisteHoja("TIPO_DE_EMPRESA")
            ? libro.LeerTabla("TIPO_DE_EMPRESA").Select(f => new TIPO_DE_EMPRESA { IdTipoDeEmpresa = ValorXlsx.Entero(f["IdTipoDeEmpresa"]), Nombre = f["Nombre"] ?? "" }).ToList()
            : DatosDeEjemplo.TiposDeEmpresa();
        List<SOCIEDAD> sociedades = hayHojaSociedad
            ? libro.LeerTabla("SOCIEDAD").Select(f => new SOCIEDAD
              {
                  IdSociedad = ValorXlsx.Entero(f["IdSociedad"]),
                  CodigoEsif = f["CodigoEsif"],
                  Nombre = f["Nombre"],
                  IdTipoDeEmpresa = ValorXlsx.Entero(f["IdTipoDeEmpresa"]),
                  IdPais = ValorXlsx.Entero(f["IdPais"]),
                  Borrado = ValorXlsx.Logico(f["Borrado"])
              }).ToList()
            : usuarios.Where(u => u.IdSociedad != null).Select(u => u.IdSociedad!.Value)
                .Concat(procesosAsociados.Select(pa => pa.IdSociedad))
                .Where(id => id > 0).Distinct().Order()
                .Select((id, i) => new SOCIEDAD
                {
                    IdSociedad = id,
                    CodigoEsif = "E" + id.ToString("000"),
                    Nombre = "Sociedad " + id + " (ejemplo)",
                    IdTipoDeEmpresa = tiposDeEmpresa[i % tiposDeEmpresa.Count].IdTipoDeEmpresa,
                    IdPais = paises[i % paises.Count].IdPais
                }).ToList();
        List<USUARIO_SOCIEDAD_AUDITORES> usuariosSociedad = libro.ExisteHoja("USUARIO_SOCIEDAD_AUDITORES")
            ? libro.LeerTabla("USUARIO_SOCIEDAD_AUDITORES").Select(f => new USUARIO_SOCIEDAD_AUDITORES
              {
                  IdUsuario = ValorXlsx.Entero(f["IdUsuario"]),
                  IdSociedad = ValorXlsx.Entero(f["IdSociedad"])
              }).ToList()
            : [];

        // Tipos de auditoría: igual que las sociedades, si el libro no trae la hoja se generan de ejemplo
        // con los identificadores que usan los usuarios y los procesos asignados del libro.
        bool hayHojaTipoDeAuditoria = libro.ExisteHoja("TIPO_DE_AUDITORIA");
        List<TIPO_DE_AUDITORIA> tiposDeAuditoria = hayHojaTipoDeAuditoria
            ? libro.LeerTabla("TIPO_DE_AUDITORIA").Select(f => new TIPO_DE_AUDITORIA
              {
                  IdTipoDeAuditoria = ValorXlsx.Entero(f["IdTipoDeAuditoria"]),
                  Codigo = f["Codigo"],
                  Descripcion = f["Descripcion"],
                  Borrado = ValorXlsx.Logico(f["Borrado"])
              }).ToList()
            : usuarios.Where(u => u.IdTipoDeAuditoria != null).Select(u => u.IdTipoDeAuditoria!.Value)
                .Concat(procesosAsociados.Select(pa => pa.IdTipoDeAuditoria))
                .Where(id => id > 0).Distinct().Order()
                .Select(id => new TIPO_DE_AUDITORIA
                {
                    IdTipoDeAuditoria = id,
                    Codigo = (id % 100).ToString("00"),
                    Descripcion = "Tipo de auditoría " + id + " (ejemplo)"
                }).ToList();

        return new AlmacenMemoria
        {
            TiposDeAuditoria = tiposDeAuditoria,
            TiposDeAuditoriaDeEjemplo = !hayHojaTipoDeAuditoria,
            Paises = paises,
            TiposDeEmpresa = tiposDeEmpresa,
            Sociedades = sociedades,
            UsuariosSociedadAuditores = usuariosSociedad,
            SociedadesDeEjemplo = !hayHojaSociedad,

            TiposDeProceso = libro.LeerTabla("TIPO_DE_PROCESO").Select(f => new TIPO_DE_PROCESO
            {
                IdTipoDeProceso = ValorXlsx.Entero(f["IdTipoDeProceso"]),
                Nombre = f["Nombre"] ?? ""
            }).ToList(),

            Perfiles = libro.LeerTabla("PERFIL_DE_USUARIO").Select(f => new PERFIL_DE_USUARIO
            {
                IdPerfilDeUsuario = ValorXlsx.Entero(f["IdPerfilDeUsuario"]),
                Nombre = f["Nombre"] ?? ""
            }).ToList(),

            Usuarios = usuarios,

            Procesos = libro.LeerTabla("PROCESO").Select(f => new PROCESO
            {
                IdProceso = ValorXlsx.Entero(f["IdProceso"]),
                Codigo = f["Codigo"],
                Nombre = f["Nombre"],
                IdTipoDeProceso = ValorXlsx.Entero(f["IdTipoDeProceso"]),
                Borrado = ValorXlsx.Logico(f["Borrado"])
            }).ToList(),

            Subprocesos = libro.LeerTabla("SUBPROCESO").Select(f => new SUBPROCESO
            {
                IdSubproceso = ValorXlsx.Entero(f["IdSubproceso"]),
                Codigo = f["Codigo"],
                Nombre = f["Nombre"],
                // En la base de datos IdProceso es obligatorio; si el libro lo trae vacío se carga como 0 (sin proceso).
                IdProceso = ValorXlsx.EnteroONulo(f["IdProceso"]) ?? 0,
                Borrado = ValorXlsx.Logico(f["Borrado"]),
                TipoOtros = ValorXlsx.Logico(f["TipoOtros"])
            }).ToList(),

            ProcesosAsociados = procesosAsociados,

            LogsEnOrigen = libro.ExisteHoja("LOG_DE_ENTIDADES") ? libro.LeerTabla("LOG_DE_ENTIDADES").Count : 0,

            Origen = "libro Excel " + Path.GetFileName(ruta)
        };
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
            return almacen.ProcesosAsociados.Count(pa => pa.IdProceso == idProceso);
    }

    public List<TIPO_DE_PROCESO> ObtenerTiposDeProceso()
    {
        lock (almacen.Cerrojo)
            return almacen.TiposDeProceso
                .OrderBy(t => t.Nombre, StringComparer.Create(new System.Globalization.CultureInfo("es-ES"), ignoreCase: true))
                .Select(t => new TIPO_DE_PROCESO { IdTipoDeProceso = t.IdTipoDeProceso, Nombre = t.Nombre })
                .ToList();
    }

    public List<PROCESO> ObtenerNoBorradosPorCodigo()
    {
        lock (almacen.Cerrojo)
            return almacen.Procesos.Where(p => !p.Borrado)
                .OrderBy(p => p.Codigo, ComparadorClave.Instancia)
                .Select(Copiar).ToList();
    }

    public List<PROCESO> ObtenerConSubprocesos()
    {
        lock (almacen.Cerrojo)
            return almacen.Procesos
                .Where(p => !p.Borrado && almacen.Subprocesos.Any(s => s.IdProceso == p.IdProceso))
                .OrderBy(p => p.IdProceso)
                .Select(Copiar).ToList();
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
    internal sealed class ComparadorClave : IComparer<object?>
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

    public List<SUBPROCESO> ObtenerFiltrados(FiltroSUBPROCESO? filtro, string orden, int numeroPagina, int numeroElementosPorPagina, out int numeroElementosTotales)
    {
        List<CriterioOrden> criterios = OrdenGrid.Interpretar(orden, CamposOrdenSubproceso.Todos);
        if (criterios.Count == 0)
            throw new ArgumentException("No se ha indicado el orden del listado.");

        lock (almacen.Cerrojo)
        {
            IEnumerable<SUBPROCESO> consulta = almacen.Subprocesos.Where(s => !s.Borrado && !s.TipoOtros).Select(Copiar);
            if (filtro != null)
            {
                if (!string.IsNullOrEmpty(filtro.Codigo))
                    consulta = consulta.Where(s => (s.Codigo ?? "").Contains(filtro.Codigo, Comparacion));
                if (!string.IsNullOrEmpty(filtro.Nombre))
                    consulta = consulta.Where(s => (s.Nombre ?? "").Contains(filtro.Nombre, Comparacion));
                if (filtro.ListaIdProceso != null)
                    consulta = consulta.Where(s => filtro.ListaIdProceso.Contains(s.IdProceso));
            }

            List<SUBPROCESO> filtrados = consulta.ToList();
            numeroElementosTotales = filtrados.Count;

            var comparador = RepositorioProcesosMemoria.ComparadorClave.Instancia;
            IOrderedEnumerable<SUBPROCESO>? ordenados = null;
            foreach (CriterioOrden c in criterios)
            {
                Func<SUBPROCESO, object?> clave = c.Campo switch
                {
                    CamposOrdenSubproceso.IdSubproceso => s => s.IdSubproceso,
                    CamposOrdenSubproceso.Codigo => s => s.Codigo,
                    CamposOrdenSubproceso.Nombre => s => s.Nombre,
                    _ => s => s.PROCESO?.Nombre
                };
                ordenados = ordenados == null
                    ? (c.Descendente ? filtrados.OrderByDescending(clave, comparador) : filtrados.OrderBy(clave, comparador))
                    : (c.Descendente ? ordenados.ThenByDescending(clave, comparador) : ordenados.ThenBy(clave, comparador));
            }

            return ordenados!.Skip(numeroElementosPorPagina * (numeroPagina - 1)).Take(numeroElementosPorPagina).ToList();
        }
    }

    public SUBPROCESO? ObtenerPorId(int idSubproceso)
    {
        lock (almacen.Cerrojo)
        {
            SUBPROCESO? s = almacen.Subprocesos.FirstOrDefault(x => x.IdSubproceso == idSubproceso && !x.Borrado);
            return s == null ? null : Copiar(s);
        }
    }

    public int ContarConMismoCodigo(string? codigo, int idSubprocesoExcluido)
    {
        lock (almacen.Cerrojo)
            return almacen.Subprocesos.Count(s => string.Equals(s.Codigo, codigo, Comparacion) && s.IdSubproceso != idSubprocesoExcluido && !s.Borrado);
    }

    public void Actualizar(SUBPROCESO subproceso)
    {
        lock (almacen.Cerrojo)
        {
            SUBPROCESO s = almacen.Subprocesos.First(x => x.IdSubproceso == subproceso.IdSubproceso);
            s.Codigo = subproceso.Codigo;
            s.Nombre = subproceso.Nombre;
            s.IdProceso = subproceso.IdProceso;
        }
    }

    public int ContarProcesosAsociadosNoBorrados(int idSubproceso)
    {
        lock (almacen.Cerrojo)
            return almacen.ProcesosAsociados.Count(pa => pa.IdSubproceso == idSubproceso && !pa.Borrado);
    }

    // Las búsquedas y comparaciones no distinguen mayúsculas, como la intercalación habitual de SQL Server.
    private const StringComparison Comparacion = StringComparison.OrdinalIgnoreCase;

    /// <summary>Copia con su PROCESO, esté o no borrado (la relación del modelo original no filtra por Borrado).</summary>
    private SUBPROCESO Copiar(SUBPROCESO s)
    {
        PROCESO? p = almacen.Procesos.FirstOrDefault(x => x.IdProceso == s.IdProceso);
        return new SUBPROCESO
        {
            IdSubproceso = s.IdSubproceso, Codigo = s.Codigo, Nombre = s.Nombre,
            IdProceso = s.IdProceso, Borrado = s.Borrado, TipoOtros = s.TipoOtros,
            PROCESO = p == null ? null : new PROCESO { IdProceso = p.IdProceso, Codigo = p.Codigo, Nombre = p.Nombre, IdTipoDeProceso = p.IdTipoDeProceso, Borrado = p.Borrado }
        };
    }
}

public sealed class RepositorioSociedadesMemoria(AlmacenMemoria almacen) : IRepositorioSociedades
{
    // Las búsquedas y comparaciones no distinguen mayúsculas, como la intercalación habitual de SQL Server.
    private const StringComparison Comparacion = StringComparison.OrdinalIgnoreCase;

    public List<SOCIEDAD> ObtenerFiltrados(FiltroSOCIEDAD? filtro, int? idUsuarioAsignado, string orden, int numeroPagina, int numeroElementosPorPagina, out int numeroElementosTotales)
    {
        List<CriterioOrden> criterios = OrdenGrid.Interpretar(orden, CamposOrdenSociedad.Todos);
        if (criterios.Count == 0)
            throw new ArgumentException("No se ha indicado el orden del listado.");

        lock (almacen.Cerrojo)
        {
            IEnumerable<SOCIEDAD> consulta = almacen.Sociedades.Where(s => !s.Borrado);
            if (filtro != null)
            {
                if (!string.IsNullOrEmpty(filtro.Nombre))
                    consulta = consulta.Where(s => (s.Nombre ?? "").Contains(filtro.Nombre, Comparacion));
                if (filtro.ListaIdPais != null)
                    consulta = consulta.Where(s => filtro.ListaIdPais.Contains(s.IdPais));
                if (filtro.ListaIdTipoEmpresa != null)
                    consulta = consulta.Where(s => filtro.ListaIdTipoEmpresa.Contains(s.IdTipoDeEmpresa));
            }
            if (idUsuarioAsignado != null)
            {
                HashSet<int> asignadas = almacen.UsuariosSociedadAuditores
                    .Where(us => us.IdUsuario == idUsuarioAsignado).Select(us => us.IdSociedad).ToHashSet();
                consulta = consulta.Where(s => asignadas.Contains(s.IdSociedad));
            }

            List<SOCIEDAD> filtrados = consulta.Select(Copiar).ToList();
            numeroElementosTotales = filtrados.Count;

            var comparador = RepositorioProcesosMemoria.ComparadorClave.Instancia;
            IOrderedEnumerable<SOCIEDAD>? ordenados = null;
            foreach (CriterioOrden c in criterios)
            {
                Func<SOCIEDAD, object?> clave = c.Campo switch
                {
                    CamposOrdenSociedad.IdSociedad => s => s.IdSociedad,
                    CamposOrdenSociedad.CodigoEsif => s => s.CodigoEsif,
                    CamposOrdenSociedad.Nombre => s => s.Nombre,
                    CamposOrdenSociedad.TipoDeEmpresa => s => s.TIPO_DE_EMPRESA?.Nombre,
                    _ => s => s.PAIS?.Nombre
                };
                ordenados = ordenados == null
                    ? (c.Descendente ? filtrados.OrderByDescending(clave, comparador) : filtrados.OrderBy(clave, comparador))
                    : (c.Descendente ? ordenados.ThenByDescending(clave, comparador) : ordenados.ThenBy(clave, comparador));
            }

            return ordenados!.Skip(numeroElementosPorPagina * (numeroPagina - 1)).Take(numeroElementosPorPagina).ToList();
        }
    }

    public SOCIEDAD? ObtenerPorId(int idSociedad)
    {
        lock (almacen.Cerrojo)
        {
            SOCIEDAD? s = almacen.Sociedades.FirstOrDefault(x => x.IdSociedad == idSociedad && !x.Borrado);
            return s == null ? null : Copiar(s);
        }
    }

    public int ContarConMismoCodigoEsif(string? codigoEsif, int idSociedadExcluida)
    {
        lock (almacen.Cerrojo)
            return almacen.Sociedades.Count(s => string.Equals(s.CodigoEsif, codigoEsif, Comparacion) && s.IdSociedad != idSociedadExcluida && !s.Borrado);
    }

    public int Insertar(SOCIEDAD sociedad)
    {
        lock (almacen.Cerrojo)
        {
            int id = almacen.Sociedades.Count == 0 ? 1 : almacen.Sociedades.Max(s => s.IdSociedad) + 1;
            almacen.Sociedades.Add(new SOCIEDAD
            {
                IdSociedad = id, CodigoEsif = sociedad.CodigoEsif, Nombre = sociedad.Nombre,
                IdTipoDeEmpresa = sociedad.IdTipoDeEmpresa, IdPais = sociedad.IdPais
            });
            return id;
        }
    }

    public void Actualizar(SOCIEDAD sociedad)
    {
        lock (almacen.Cerrojo)
        {
            SOCIEDAD s = almacen.Sociedades.First(x => x.IdSociedad == sociedad.IdSociedad);
            s.CodigoEsif = sociedad.CodigoEsif;
            s.Nombre = sociedad.Nombre;
            s.IdPais = sociedad.IdPais;
            s.IdTipoDeEmpresa = sociedad.IdTipoDeEmpresa;
        }
    }

    public void MarcarBorrado(int idSociedad)
    {
        lock (almacen.Cerrojo)
            almacen.Sociedades.First(x => x.IdSociedad == idSociedad).Borrado = true;
    }

    public UsoDeSociedad ObtenerUso(int idSociedad)
    {
        lock (almacen.Cerrojo)
            return new UsoDeSociedad(
                almacen.ProcesosAsociados.Count(pa => pa.IdSociedad == idSociedad),
                almacen.Usuarios.Count(u => u.IdSociedad == idSociedad),
                almacen.UsuariosSociedadAuditores.Count(us => us.IdSociedad == idSociedad));
    }

    public List<TIPO_DE_EMPRESA> ObtenerTiposDeEmpresa()
    {
        lock (almacen.Cerrojo)
            return almacen.TiposDeEmpresa
                .Select(t => new TIPO_DE_EMPRESA { IdTipoDeEmpresa = t.IdTipoDeEmpresa, Nombre = t.Nombre })
                .ToList();
    }

    public List<PAIS> ObtenerPaises()
    {
        lock (almacen.Cerrojo)
            return almacen.Paises
                .OrderBy(p => p.Nombre, StringComparer.Create(new System.Globalization.CultureInfo("es-ES"), ignoreCase: true))
                .Select(p => new PAIS { IdPais = p.IdPais, Nombre = p.Nombre })
                .ToList();
    }

    private SOCIEDAD Copiar(SOCIEDAD s)
    {
        TIPO_DE_EMPRESA? tipo = almacen.TiposDeEmpresa.FirstOrDefault(t => t.IdTipoDeEmpresa == s.IdTipoDeEmpresa);
        PAIS? pais = almacen.Paises.FirstOrDefault(p => p.IdPais == s.IdPais);
        return new SOCIEDAD
        {
            IdSociedad = s.IdSociedad, CodigoEsif = s.CodigoEsif, Nombre = s.Nombre,
            IdTipoDeEmpresa = s.IdTipoDeEmpresa, IdPais = s.IdPais, Borrado = s.Borrado,
            TIPO_DE_EMPRESA = tipo == null ? null : new TIPO_DE_EMPRESA { IdTipoDeEmpresa = tipo.IdTipoDeEmpresa, Nombre = tipo.Nombre },
            PAIS = pais == null ? null : new PAIS { IdPais = pais.IdPais, Nombre = pais.Nombre }
        };
    }
}

public sealed class RepositorioTiposDeAuditoriaMemoria(AlmacenMemoria almacen) : IRepositorioTiposDeAuditoria
{
    // Las búsquedas y comparaciones no distinguen mayúsculas, como la intercalación habitual de SQL Server.
    private const StringComparison Comparacion = StringComparison.OrdinalIgnoreCase;

    public List<TIPO_DE_AUDITORIA> ObtenerFiltrados(FiltroTIPO_DE_AUDITORIA? filtro, string orden, int numeroPagina, int numeroElementosPorPagina, out int numeroElementosTotales)
    {
        List<CriterioOrden> criterios = OrdenGrid.Interpretar(orden, CamposOrdenTipoDeAuditoria.Todos);
        if (criterios.Count == 0)
            throw new ArgumentException("No se ha indicado el orden del listado.");

        lock (almacen.Cerrojo)
        {
            IEnumerable<TIPO_DE_AUDITORIA> consulta = almacen.TiposDeAuditoria.Where(t => !t.Borrado);
            if (filtro != null)
            {
                if (!string.IsNullOrEmpty(filtro.Codigo))
                    consulta = consulta.Where(t => (t.Codigo ?? "").Contains(filtro.Codigo, Comparacion));
                if (!string.IsNullOrEmpty(filtro.Descripcion))
                    consulta = consulta.Where(t => (t.Descripcion ?? "").Contains(filtro.Descripcion, Comparacion));
            }

            List<TIPO_DE_AUDITORIA> filtrados = consulta.Select(Copiar).ToList();
            numeroElementosTotales = filtrados.Count;

            var comparador = RepositorioProcesosMemoria.ComparadorClave.Instancia;
            IOrderedEnumerable<TIPO_DE_AUDITORIA>? ordenados = null;
            foreach (CriterioOrden c in criterios)
            {
                Func<TIPO_DE_AUDITORIA, object?> clave = c.Campo switch
                {
                    CamposOrdenTipoDeAuditoria.IdTipoDeAuditoria => t => t.IdTipoDeAuditoria,
                    CamposOrdenTipoDeAuditoria.Codigo => t => t.Codigo,
                    _ => t => t.Descripcion
                };
                ordenados = ordenados == null
                    ? (c.Descendente ? filtrados.OrderByDescending(clave, comparador) : filtrados.OrderBy(clave, comparador))
                    : (c.Descendente ? ordenados.ThenByDescending(clave, comparador) : ordenados.ThenBy(clave, comparador));
            }

            return ordenados!.Skip(numeroElementosPorPagina * (numeroPagina - 1)).Take(numeroElementosPorPagina).ToList();
        }
    }

    public TIPO_DE_AUDITORIA? ObtenerPorId(int idTipoDeAuditoria)
    {
        lock (almacen.Cerrojo)
        {
            TIPO_DE_AUDITORIA? t = almacen.TiposDeAuditoria.FirstOrDefault(x => x.IdTipoDeAuditoria == idTipoDeAuditoria && !x.Borrado);
            return t == null ? null : Copiar(t);
        }
    }

    public int ContarConMismoCodigo(string? codigo, int idTipoDeAuditoriaExcluido)
    {
        lock (almacen.Cerrojo)
            return almacen.TiposDeAuditoria.Count(t => string.Equals(t.Codigo, codigo, Comparacion) && t.IdTipoDeAuditoria != idTipoDeAuditoriaExcluido && !t.Borrado);
    }

    public int Insertar(TIPO_DE_AUDITORIA tipoDeAuditoria)
    {
        lock (almacen.Cerrojo)
        {
            int id = almacen.TiposDeAuditoria.Count == 0 ? 1 : almacen.TiposDeAuditoria.Max(t => t.IdTipoDeAuditoria) + 1;
            almacen.TiposDeAuditoria.Add(new TIPO_DE_AUDITORIA { IdTipoDeAuditoria = id, Codigo = tipoDeAuditoria.Codigo, Descripcion = tipoDeAuditoria.Descripcion });
            return id;
        }
    }

    public void Actualizar(TIPO_DE_AUDITORIA tipoDeAuditoria)
    {
        lock (almacen.Cerrojo)
        {
            TIPO_DE_AUDITORIA t = almacen.TiposDeAuditoria.First(x => x.IdTipoDeAuditoria == tipoDeAuditoria.IdTipoDeAuditoria);
            t.Codigo = tipoDeAuditoria.Codigo;
            t.Descripcion = tipoDeAuditoria.Descripcion;
        }
    }

    public void MarcarBorrado(int idTipoDeAuditoria)
    {
        lock (almacen.Cerrojo)
            almacen.TiposDeAuditoria.First(x => x.IdTipoDeAuditoria == idTipoDeAuditoria).Borrado = true;
    }

    public UsoDeTipoDeAuditoria ObtenerUso(int idTipoDeAuditoria)
    {
        lock (almacen.Cerrojo)
            return new UsoDeTipoDeAuditoria(
                almacen.ProcesosAsociados.Count(pa => pa.IdTipoDeAuditoria == idTipoDeAuditoria),
                almacen.Usuarios.Count(u => u.IdTipoDeAuditoria == idTipoDeAuditoria));
    }

    private static TIPO_DE_AUDITORIA Copiar(TIPO_DE_AUDITORIA t) => new()
    {
        IdTipoDeAuditoria = t.IdTipoDeAuditoria, Codigo = t.Codigo, Descripcion = t.Descripcion, Borrado = t.Borrado
    };
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
                    IdPerfilDeUsuario = u.IdPerfilDeUsuario, IdSociedad = u.IdSociedad, IdTipoDeAuditoria = u.IdTipoDeAuditoria, Borrado = u.Borrado,
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
            log.IdLogDeEntidades = almacen.LogsEnOrigen + almacen.Logs.Count + 1;
            almacen.Logs.Add(log);
        }
    }
}
