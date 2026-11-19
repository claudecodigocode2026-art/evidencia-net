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
        new() { IdUsuario = 1, Nombre = "Demo", Apellidos = "Usuario", UID = UidUsuarioDemo, IdPerfilDeUsuario = 1 },
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

    /// <summary>Identificadores de proceso que tienen un PROCESO_ASOCIADO.</summary>
    public static List<int> ProcesosConAsociado() => [1, 9];
}
