// Fichero generado por herramientas/gen_datos.py. Datos de ejemplo de la demo.
// Los tipos de proceso, perfiles y tipos de acción son los datos maestros del script original
// (eVidencias.DAL/Scripts/bbddCompleta_eVidencias.sql). Los procesos y el usuario son inventados.
using eVidencia.Entidades;

namespace eVidencia.DAL.Memoria;

public static class DatosDeEjemplo
{
    public const string UidUsuarioDemo = "demo.admin";

    public static List<TIPO_DE_PROCESO> TiposDeProceso() =>
    [
        new() { IdTipoDeProceso = 1, Nombre = "Desarrollo de la estrategia corporativa" },
        new() { IdTipoDeProceso = 2, Nombre = "Comercialización y ventas" },
        new() { IdTipoDeProceso = 3, Nombre = "Compra productos y servicios, inversión y cuentas a pagar" },
        new() { IdTipoDeProceso = 4, Nombre = "Producción y almacenaje" },
        new() { IdTipoDeProceso = 5, Nombre = "Facturación y cuentas a cobrar" },
        new() { IdTipoDeProceso = 6, Nombre = "Gestión de RR.HH." },
        new() { IdTipoDeProceso = 7, Nombre = "Procesos de Gestión de la tecnología y la información" },
        new() { IdTipoDeProceso = 8, Nombre = "Gestión de recursos financieros" },
        new() { IdTipoDeProceso = 9, Nombre = "Información financiera-contable y fiscal" },
        new() { IdTipoDeProceso = 10, Nombre = "Gestión de riesgos específicos" },
        new() { IdTipoDeProceso = 11, Nombre = "Cumplimiento" },
        new() { IdTipoDeProceso = 12, Nombre = "SOX Procesos Operativos" },
        new() { IdTipoDeProceso = 13, Nombre = "SOX Procesos No Operativos" },
        new() { IdTipoDeProceso = 14, Nombre = "SOX Procesos Globales" },
    ];

    public static List<PERFIL_DE_USUARIO> Perfiles() =>
    [
        new() { IdPerfilDeUsuario = 1, Nombre = "Administrador" },
        new() { IdPerfilDeUsuario = 2, Nombre = "ResponsableDeAuditoria" },
        new() { IdPerfilDeUsuario = 3, Nombre = "Auditor" },
    ];

    public static List<USUARIO> Usuarios() =>
    [
        new() { IdUsuario = 1, Nombre = "Demo", Apellidos = "Usuario", UID = UidUsuarioDemo, IdPerfilDeUsuario = 1, IdSociedad = 2, IdTipoDeAuditoria = 2 },
        // Usuario dado de baja: su sociedad (la 7) sigue sin poder eliminarse, como en el origen.
        new() { IdUsuario = 2, Nombre = "Baja", Apellidos = "Usuario", UID = "demo.baja", IdPerfilDeUsuario = 6, IdSociedad = 7, IdTipoDeAuditoria = 6, Borrado = true },
    ];

    /// <summary>
    /// Tipos de auditoría inventados. El 1 tiene procesos asignados; el 2 es el del usuario de la demo;
    /// el 3 solo tiene un proceso asignado borrado; el 4 no se usa; el 5 está borrado; el 6 es el de un usuario borrado.
    /// </summary>
    public static List<TIPO_DE_AUDITORIA> TiposDeAuditoria() =>
    [
        new() { IdTipoDeAuditoria = 1, Codigo = "E1", Descripcion = "Auditoría de cuentas (ejemplo)" },
        new() { IdTipoDeAuditoria = 2, Codigo = "E2", Descripcion = "Auditoría de procesos (ejemplo)" },
        new() { IdTipoDeAuditoria = 3, Codigo = "E3", Descripcion = "Auditoría de sistemas (ejemplo)" },
        new() { IdTipoDeAuditoria = 4, Codigo = "E4", Descripcion = "Revisión interna (ejemplo)" },
        new() { IdTipoDeAuditoria = 5, Codigo = "E5", Descripcion = "Auditoría borrada (ejemplo)", Borrado = true },
        new() { IdTipoDeAuditoria = 6, Codigo = "E6", Descripcion = "Revisión externa (ejemplo)" },
    ];

    public static List<PAIS> Paises() =>
    [
        new() { IdPais = 1, Nombre = "España" },
        new() { IdPais = 2, Nombre = "Brasil" },
        new() { IdPais = 3, Nombre = "Argentina" },
        new() { IdPais = 4, Nombre = "Chile" },
    ];

    /// <summary>Valores de la tabla TIPO_DE_EMPRESA (los mismos de eVidencias.BLL/DatosMaestros.resx).</summary>
    public static List<TIPO_DE_EMPRESA> TiposDeEmpresa() =>
    [
        new() { IdTipoDeEmpresa = 1, Nombre = "Auditora" },
        new() { IdTipoDeEmpresa = 2, Nombre = "Auditada" },
        new() { IdTipoDeEmpresa = 3, Nombre = "Ambas" },
    ];

    /// <summary>
    /// Sociedades inventadas. La 1 tiene procesos asignados; la 2 es la del usuario de la demo; la 3 está asignada
    /// al usuario de la demo; la 4 solo tiene un proceso asignado borrado; la 5 no se usa; la 6 está borrada;
    /// la 7 es la de un usuario borrado.
    /// </summary>
    public static List<SOCIEDAD> Sociedades() =>
    [
        new() { IdSociedad = 1, CodigoEsif = "E001", Nombre = "Sociedad Uno (ejemplo)", IdTipoDeEmpresa = 1, IdPais = 1 },
        new() { IdSociedad = 2, CodigoEsif = "E002", Nombre = "Sociedad Dos (ejemplo)", IdTipoDeEmpresa = 2, IdPais = 2 },
        new() { IdSociedad = 3, CodigoEsif = "E003", Nombre = "Sociedad Tres (ejemplo)", IdTipoDeEmpresa = 3, IdPais = 3 },
        new() { IdSociedad = 4, CodigoEsif = "E004", Nombre = "Sociedad Cuatro (ejemplo)", IdTipoDeEmpresa = 2, IdPais = 4 },
        new() { IdSociedad = 5, CodigoEsif = "E005", Nombre = "Filial Cinco (ejemplo)", IdTipoDeEmpresa = 3, IdPais = 1 },
        new() { IdSociedad = 6, CodigoEsif = "E006", Nombre = "Sociedad Seis, borrada (ejemplo)", IdTipoDeEmpresa = 2, IdPais = 1, Borrado = true },
        new() { IdSociedad = 7, CodigoEsif = "E007", Nombre = "Filial Siete (ejemplo)", IdTipoDeEmpresa = 2, IdPais = 2 },
    ];

    public static List<USUARIO_SOCIEDAD_AUDITORES> UsuariosSociedadAuditores() =>
    [
        new() { IdUsuario = 1, IdSociedad = 3 },
    ];

    public static List<PROCESO> Procesos() =>
    [
        new() { IdProceso = 1, Codigo = "EJ-01", Nombre = "Planificación estratégica (ejemplo)", IdTipoDeProceso = 1 },
        new() { IdProceso = 2, Codigo = "EJ-02", Nombre = "Gestión de ofertas comerciales (ejemplo)", IdTipoDeProceso = 2 },
        new() { IdProceso = 3, Codigo = "EJ-03", Nombre = "Gestión de pedidos de clientes (ejemplo)", IdTipoDeProceso = 2 },
        new() { IdProceso = 4, Codigo = "EJ-04", Nombre = "Compras de bienes y servicios (ejemplo)", IdTipoDeProceso = 3 },
        new() { IdProceso = 5, Codigo = "EJ-05", Nombre = "Homologación de proveedores (ejemplo)", IdTipoDeProceso = 3 },
        new() { IdProceso = 6, Codigo = "EJ-06", Nombre = "Cuentas a pagar (ejemplo)", IdTipoDeProceso = 3 },
        new() { IdProceso = 7, Codigo = "EJ-07", Nombre = "Gestión de inventario (ejemplo)", IdTipoDeProceso = 4 },
        new() { IdProceso = 8, Codigo = "EJ-08", Nombre = "Logística y distribución (ejemplo)", IdTipoDeProceso = 4 },
        new() { IdProceso = 9, Codigo = "EJ-09", Nombre = "Facturación a clientes (ejemplo)", IdTipoDeProceso = 5 },
        new() { IdProceso = 10, Codigo = "EJ-10", Nombre = "Gestión de cobros (ejemplo)", IdTipoDeProceso = 5 },
        new() { IdProceso = 11, Codigo = "EJ-11", Nombre = "Nómina (ejemplo)", IdTipoDeProceso = 6 },
        new() { IdProceso = 12, Codigo = "EJ-12", Nombre = "Selección y contratación (ejemplo)", IdTipoDeProceso = 6 },
        new() { IdProceso = 13, Codigo = "EJ-13", Nombre = "Gestión de accesos a sistemas (ejemplo)", IdTipoDeProceso = 7 },
        new() { IdProceso = 14, Codigo = "EJ-14", Nombre = "Gestión de cambios en sistemas (ejemplo)", IdTipoDeProceso = 7 },
        new() { IdProceso = 15, Codigo = "EJ-15", Nombre = "Copias de seguridad (ejemplo)", IdTipoDeProceso = 7 },
        new() { IdProceso = 16, Codigo = "EJ-16", Nombre = "Tesorería (ejemplo)", IdTipoDeProceso = 8 },
        new() { IdProceso = 17, Codigo = "EJ-17", Nombre = "Financiación (ejemplo)", IdTipoDeProceso = 8 },
        new() { IdProceso = 18, Codigo = "EJ-18", Nombre = "Cierre contable (ejemplo)", IdTipoDeProceso = 9 },
        new() { IdProceso = 19, Codigo = "EJ-19", Nombre = "Consolidación (ejemplo)", IdTipoDeProceso = 9 },
        new() { IdProceso = 20, Codigo = "EJ-20", Nombre = "Impuestos (ejemplo)", IdTipoDeProceso = 9 },
        new() { IdProceso = 21, Codigo = "EJ-21", Nombre = "Gestión de seguros (ejemplo)", IdTipoDeProceso = 10 },
        new() { IdProceso = 22, Codigo = "EJ-22", Nombre = "Canal de denuncias (ejemplo)", IdTipoDeProceso = 11 },
        new() { IdProceso = 23, Codigo = "EJ-23", Nombre = "Protección de datos (ejemplo)", IdTipoDeProceso = 11 },
        new() { IdProceso = 24, Codigo = "EJ-24", Nombre = "Ingresos de servicios (ejemplo)", IdTipoDeProceso = 12 },
        new() { IdProceso = 25, Codigo = "EJ-25", Nombre = "Activos fijos (ejemplo)", IdTipoDeProceso = 13 },
        new() { IdProceso = 26, Codigo = "EJ-26", Nombre = "Controles a nivel de entidad (ejemplo)", IdTipoDeProceso = 14 },
    ];

    /// <summary>
    /// Subprocesos de ejemplo que no son de tipo "Otros". Los identificadores empiezan en 101 para no
    /// coincidir con los subprocesos "Otros" (uno por proceso, identificadores 1 a 26).
    /// </summary>
    public static List<SUBPROCESO> Subprocesos() =>
    [
        new() { IdSubproceso = 101, Codigo = "EJ-01.1", Nombre = "Definición de objetivos (ejemplo)", IdProceso = 1 },
        new() { IdSubproceso = 102, Codigo = "EJ-01.2", Nombre = "Seguimiento del plan (ejemplo)", IdProceso = 1 },
        new() { IdSubproceso = 103, Codigo = "EJ-02.1", Nombre = "Elaboración de ofertas (ejemplo)", IdProceso = 2 },
        new() { IdSubproceso = 104, Codigo = "EJ-02.2", Nombre = "Aprobación de ofertas (ejemplo)", IdProceso = 2 },
        new() { IdSubproceso = 105, Codigo = "EJ-03.1", Nombre = "Registro de pedidos (ejemplo)", IdProceso = 3 },
        new() { IdSubproceso = 106, Codigo = "EJ-09.1", Nombre = "Emisión de facturas (ejemplo)", IdProceso = 9 },
    ];

    /// <summary>
    /// Filas de PROCESO_ASOCIADO: los procesos 1 y 9 tienen un proceso asociado; el del proceso 1 usa el
    /// subproceso 101 y el subproceso 103 solo tiene un proceso asociado ya borrado.
    /// </summary>
    public static List<PROCESO_ASOCIADO> ProcesosAsociados() =>
    [
        new() { IdProcesoAsociado = 1, IdProceso = 1, IdSubproceso = 101, IdSociedad = 1, IdTipoDeAuditoria = 1 },
        new() { IdProcesoAsociado = 2, IdProceso = 9, IdSubproceso = null, IdSociedad = 1, IdTipoDeAuditoria = 1 },
        new() { IdProcesoAsociado = 3, IdProceso = 2, IdSubproceso = 103, IdSociedad = 4, IdTipoDeAuditoria = 3, Borrado = true },
    ];
}
