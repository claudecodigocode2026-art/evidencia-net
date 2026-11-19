/* =====================================================================
   eVidencia - demo .NET - base de datos local de pruebas
   Crea la base de datos eVidenciaDemo con las tablas que usa la demo
   (Administración de Procesos) y datos de ejemplo.

   Tablas y columnas: según eVidencias.DAL/eVidencias.edmx (rama PROD).
   Se crean solo las 8 tablas que toca la funcionalidad; no se crean las
   claves ajenas hacia tablas que no forman parte de la demo.
   Datos maestros (tipos de proceso, perfiles, tipos de acción): los del
   script original bbddCompleta_eVidencias.sql.
   Procesos y usuario: INVENTADOS para la demo. No son datos de eVidencia.
   ===================================================================== */
IF DB_ID('eVidenciaDemo') IS NULL CREATE DATABASE eVidenciaDemo;
GO
USE eVidenciaDemo;
GO
IF OBJECT_ID('LOG_DE_ENTIDADES') IS NOT NULL DROP TABLE LOG_DE_ENTIDADES;
IF OBJECT_ID('PROCESO_ASOCIADO') IS NOT NULL DROP TABLE PROCESO_ASOCIADO;
IF OBJECT_ID('SUBPROCESO') IS NOT NULL DROP TABLE SUBPROCESO;
IF OBJECT_ID('PROCESO') IS NOT NULL DROP TABLE PROCESO;
IF OBJECT_ID('TIPO_DE_PROCESO') IS NOT NULL DROP TABLE TIPO_DE_PROCESO;
IF OBJECT_ID('USUARIO') IS NOT NULL DROP TABLE USUARIO;
IF OBJECT_ID('PERFIL_DE_USUARIO') IS NOT NULL DROP TABLE PERFIL_DE_USUARIO;
IF OBJECT_ID('TIPO_DE_ACCION') IS NOT NULL DROP TABLE TIPO_DE_ACCION;
GO
CREATE TABLE TIPO_DE_PROCESO (
    IdTipoDeProceso int NOT NULL PRIMARY KEY,
    Nombre varchar(250) NOT NULL);
CREATE TABLE PROCESO (
    IdProceso int IDENTITY(1,1) NOT NULL PRIMARY KEY,
    Codigo varchar(10) NOT NULL,
    Nombre varchar(250) NOT NULL,
    IdTipoDeProceso int NOT NULL REFERENCES TIPO_DE_PROCESO(IdTipoDeProceso),
    Borrado bit NOT NULL);
CREATE TABLE SUBPROCESO (
    IdSubproceso int IDENTITY(1,1) NOT NULL PRIMARY KEY,
    Codigo varchar(10) NOT NULL,
    Nombre varchar(250) NOT NULL,
    IdProceso int NOT NULL REFERENCES PROCESO(IdProceso),
    Borrado bit NOT NULL,
    TipoOtros bit NOT NULL);
CREATE TABLE PROCESO_ASOCIADO (
    IdProcesoAsociado int IDENTITY(1,1) NOT NULL PRIMARY KEY,
    IdProceso int NOT NULL REFERENCES PROCESO(IdProceso),
    IdTipoDeAuditoria int NOT NULL,
    IdPeriodo int NOT NULL,
    IdSociedad int NOT NULL,
    IdUsuarioResponsable int NOT NULL,
    Borrado bit NOT NULL,
    IdEnPeriodoAnterior int NULL,
    Otros varchar(250) NULL,
    IdSubproceso int NOT NULL);
CREATE TABLE PERFIL_DE_USUARIO (
    IdPerfilDeUsuario int NOT NULL PRIMARY KEY,
    Nombre varchar(50) NOT NULL);
CREATE TABLE USUARIO (
    IdUsuario int IDENTITY(1,1) NOT NULL PRIMARY KEY,
    Nombre varchar(250) NOT NULL,
    Apellidos varchar(250) NOT NULL,
    Email varchar(250) NULL,
    UID varchar(250) NULL,
    IdPerfilDeUsuario int NOT NULL REFERENCES PERFIL_DE_USUARIO(IdPerfilDeUsuario),
    IdPais int NOT NULL,
    IdSociedad int NULL,
    IdTipoDeAuditoria int NULL,
    Borrado bit NOT NULL,
    FechaFinVigencia date NULL,
    NumeroIntentos int NULL,
    Captcha int NULL);
CREATE TABLE TIPO_DE_ACCION (
    IdTipoDeAccion int NOT NULL PRIMARY KEY,
    Nombre varchar(50) NOT NULL);
CREATE TABLE LOG_DE_ENTIDADES (
    IdLogDeEntidades int IDENTITY(1,1) NOT NULL PRIMARY KEY,
    Fecha datetime NOT NULL,
    IdUsuario int NOT NULL,
    IdTipoDeAccion int NULL,
    IdSociedad int NULL,
    IdPeriodo int NULL,
    Descripcion varchar(max) NOT NULL,
    IdProceso int NULL,
    IdPlanificadorDeEvidencias int NULL,
    IdActividadDeControl int NULL,
    IdSubproceso int NULL,
    IdAreaResponsable int NULL);
GO
INSERT INTO TIPO_DE_PROCESO (IdTipoDeProceso, Nombre) VALUES (1, 'Desarrollo de la estrategia corporativa');
INSERT INTO TIPO_DE_PROCESO (IdTipoDeProceso, Nombre) VALUES (2, 'Comercialización y ventas');
INSERT INTO TIPO_DE_PROCESO (IdTipoDeProceso, Nombre) VALUES (3, 'Compra productos y servicios, inversión y cuentas a pagar');
INSERT INTO TIPO_DE_PROCESO (IdTipoDeProceso, Nombre) VALUES (4, 'Producción y almacenaje');
INSERT INTO TIPO_DE_PROCESO (IdTipoDeProceso, Nombre) VALUES (5, 'Facturación y cuentas a cobrar');
INSERT INTO TIPO_DE_PROCESO (IdTipoDeProceso, Nombre) VALUES (6, 'Gestión de RR.HH.');
INSERT INTO TIPO_DE_PROCESO (IdTipoDeProceso, Nombre) VALUES (7, 'Procesos de Gestión de la tecnología y la información');
INSERT INTO TIPO_DE_PROCESO (IdTipoDeProceso, Nombre) VALUES (8, 'Gestión de recursos financieros');
INSERT INTO TIPO_DE_PROCESO (IdTipoDeProceso, Nombre) VALUES (9, 'Información financiera-contable y fiscal');
INSERT INTO TIPO_DE_PROCESO (IdTipoDeProceso, Nombre) VALUES (10, 'Gestión de riesgos específicos');
INSERT INTO TIPO_DE_PROCESO (IdTipoDeProceso, Nombre) VALUES (11, 'Cumplimiento');
INSERT INTO TIPO_DE_PROCESO (IdTipoDeProceso, Nombre) VALUES (12, 'SOX Procesos Operativos');
INSERT INTO TIPO_DE_PROCESO (IdTipoDeProceso, Nombre) VALUES (13, 'SOX Procesos No Operativos');
INSERT INTO TIPO_DE_PROCESO (IdTipoDeProceso, Nombre) VALUES (14, 'SOX Procesos Globales');
INSERT INTO PERFIL_DE_USUARIO (IdPerfilDeUsuario, Nombre) VALUES (1, 'Administrador');
INSERT INTO PERFIL_DE_USUARIO (IdPerfilDeUsuario, Nombre) VALUES (2, 'ResponsableDeAuditoria');
INSERT INTO PERFIL_DE_USUARIO (IdPerfilDeUsuario, Nombre) VALUES (3, 'Auditor');
INSERT INTO TIPO_DE_ACCION (IdTipoDeAccion, Nombre) VALUES (1, 'Alta');
INSERT INTO TIPO_DE_ACCION (IdTipoDeAccion, Nombre) VALUES (2, 'Baja');
INSERT INTO TIPO_DE_ACCION (IdTipoDeAccion, Nombre) VALUES (3, 'Modificación');
INSERT INTO USUARIO (Nombre, Apellidos, UID, IdPerfilDeUsuario, IdPais, Borrado) VALUES ('Demo', 'Usuario', 'demo.admin', 1, 1, 0);
GO
-- Procesos de ejemplo (inventados). Cada proceso lleva su subproceso "Otros", como hace la aplicación al crearlo.
INSERT INTO PROCESO (Codigo, Nombre, IdTipoDeProceso, Borrado) VALUES ('EJ-01', 'Planificación estratégica (ejemplo)', 1, 0);
INSERT INTO SUBPROCESO (Codigo, Nombre, IdProceso, Borrado, TipoOtros) VALUES ('', 'Otros', SCOPE_IDENTITY(), 0, 1);
INSERT INTO PROCESO (Codigo, Nombre, IdTipoDeProceso, Borrado) VALUES ('EJ-02', 'Gestión de ofertas comerciales (ejemplo)', 2, 0);
INSERT INTO SUBPROCESO (Codigo, Nombre, IdProceso, Borrado, TipoOtros) VALUES ('', 'Otros', SCOPE_IDENTITY(), 0, 1);
INSERT INTO PROCESO (Codigo, Nombre, IdTipoDeProceso, Borrado) VALUES ('EJ-03', 'Gestión de pedidos de clientes (ejemplo)', 2, 0);
INSERT INTO SUBPROCESO (Codigo, Nombre, IdProceso, Borrado, TipoOtros) VALUES ('', 'Otros', SCOPE_IDENTITY(), 0, 1);
INSERT INTO PROCESO (Codigo, Nombre, IdTipoDeProceso, Borrado) VALUES ('EJ-04', 'Compras de bienes y servicios (ejemplo)', 3, 0);
INSERT INTO SUBPROCESO (Codigo, Nombre, IdProceso, Borrado, TipoOtros) VALUES ('', 'Otros', SCOPE_IDENTITY(), 0, 1);
INSERT INTO PROCESO (Codigo, Nombre, IdTipoDeProceso, Borrado) VALUES ('EJ-05', 'Homologación de proveedores (ejemplo)', 3, 0);
INSERT INTO SUBPROCESO (Codigo, Nombre, IdProceso, Borrado, TipoOtros) VALUES ('', 'Otros', SCOPE_IDENTITY(), 0, 1);
INSERT INTO PROCESO (Codigo, Nombre, IdTipoDeProceso, Borrado) VALUES ('EJ-06', 'Cuentas a pagar (ejemplo)', 3, 0);
INSERT INTO SUBPROCESO (Codigo, Nombre, IdProceso, Borrado, TipoOtros) VALUES ('', 'Otros', SCOPE_IDENTITY(), 0, 1);
INSERT INTO PROCESO (Codigo, Nombre, IdTipoDeProceso, Borrado) VALUES ('EJ-07', 'Gestión de inventario (ejemplo)', 4, 0);
INSERT INTO SUBPROCESO (Codigo, Nombre, IdProceso, Borrado, TipoOtros) VALUES ('', 'Otros', SCOPE_IDENTITY(), 0, 1);
INSERT INTO PROCESO (Codigo, Nombre, IdTipoDeProceso, Borrado) VALUES ('EJ-08', 'Logística y distribución (ejemplo)', 4, 0);
INSERT INTO SUBPROCESO (Codigo, Nombre, IdProceso, Borrado, TipoOtros) VALUES ('', 'Otros', SCOPE_IDENTITY(), 0, 1);
INSERT INTO PROCESO (Codigo, Nombre, IdTipoDeProceso, Borrado) VALUES ('EJ-09', 'Facturación a clientes (ejemplo)', 5, 0);
INSERT INTO SUBPROCESO (Codigo, Nombre, IdProceso, Borrado, TipoOtros) VALUES ('', 'Otros', SCOPE_IDENTITY(), 0, 1);
INSERT INTO PROCESO (Codigo, Nombre, IdTipoDeProceso, Borrado) VALUES ('EJ-10', 'Gestión de cobros (ejemplo)', 5, 0);
INSERT INTO SUBPROCESO (Codigo, Nombre, IdProceso, Borrado, TipoOtros) VALUES ('', 'Otros', SCOPE_IDENTITY(), 0, 1);
INSERT INTO PROCESO (Codigo, Nombre, IdTipoDeProceso, Borrado) VALUES ('EJ-11', 'Nómina (ejemplo)', 6, 0);
INSERT INTO SUBPROCESO (Codigo, Nombre, IdProceso, Borrado, TipoOtros) VALUES ('', 'Otros', SCOPE_IDENTITY(), 0, 1);
INSERT INTO PROCESO (Codigo, Nombre, IdTipoDeProceso, Borrado) VALUES ('EJ-12', 'Selección y contratación (ejemplo)', 6, 0);
INSERT INTO SUBPROCESO (Codigo, Nombre, IdProceso, Borrado, TipoOtros) VALUES ('', 'Otros', SCOPE_IDENTITY(), 0, 1);
INSERT INTO PROCESO (Codigo, Nombre, IdTipoDeProceso, Borrado) VALUES ('EJ-13', 'Gestión de accesos a sistemas (ejemplo)', 7, 0);
INSERT INTO SUBPROCESO (Codigo, Nombre, IdProceso, Borrado, TipoOtros) VALUES ('', 'Otros', SCOPE_IDENTITY(), 0, 1);
INSERT INTO PROCESO (Codigo, Nombre, IdTipoDeProceso, Borrado) VALUES ('EJ-14', 'Gestión de cambios en sistemas (ejemplo)', 7, 0);
INSERT INTO SUBPROCESO (Codigo, Nombre, IdProceso, Borrado, TipoOtros) VALUES ('', 'Otros', SCOPE_IDENTITY(), 0, 1);
INSERT INTO PROCESO (Codigo, Nombre, IdTipoDeProceso, Borrado) VALUES ('EJ-15', 'Copias de seguridad (ejemplo)', 7, 0);
INSERT INTO SUBPROCESO (Codigo, Nombre, IdProceso, Borrado, TipoOtros) VALUES ('', 'Otros', SCOPE_IDENTITY(), 0, 1);
INSERT INTO PROCESO (Codigo, Nombre, IdTipoDeProceso, Borrado) VALUES ('EJ-16', 'Tesorería (ejemplo)', 8, 0);
INSERT INTO SUBPROCESO (Codigo, Nombre, IdProceso, Borrado, TipoOtros) VALUES ('', 'Otros', SCOPE_IDENTITY(), 0, 1);
INSERT INTO PROCESO (Codigo, Nombre, IdTipoDeProceso, Borrado) VALUES ('EJ-17', 'Financiación (ejemplo)', 8, 0);
INSERT INTO SUBPROCESO (Codigo, Nombre, IdProceso, Borrado, TipoOtros) VALUES ('', 'Otros', SCOPE_IDENTITY(), 0, 1);
INSERT INTO PROCESO (Codigo, Nombre, IdTipoDeProceso, Borrado) VALUES ('EJ-18', 'Cierre contable (ejemplo)', 9, 0);
INSERT INTO SUBPROCESO (Codigo, Nombre, IdProceso, Borrado, TipoOtros) VALUES ('', 'Otros', SCOPE_IDENTITY(), 0, 1);
INSERT INTO PROCESO (Codigo, Nombre, IdTipoDeProceso, Borrado) VALUES ('EJ-19', 'Consolidación (ejemplo)', 9, 0);
INSERT INTO SUBPROCESO (Codigo, Nombre, IdProceso, Borrado, TipoOtros) VALUES ('', 'Otros', SCOPE_IDENTITY(), 0, 1);
INSERT INTO PROCESO (Codigo, Nombre, IdTipoDeProceso, Borrado) VALUES ('EJ-20', 'Impuestos (ejemplo)', 9, 0);
INSERT INTO SUBPROCESO (Codigo, Nombre, IdProceso, Borrado, TipoOtros) VALUES ('', 'Otros', SCOPE_IDENTITY(), 0, 1);
INSERT INTO PROCESO (Codigo, Nombre, IdTipoDeProceso, Borrado) VALUES ('EJ-21', 'Gestión de seguros (ejemplo)', 10, 0);
INSERT INTO SUBPROCESO (Codigo, Nombre, IdProceso, Borrado, TipoOtros) VALUES ('', 'Otros', SCOPE_IDENTITY(), 0, 1);
INSERT INTO PROCESO (Codigo, Nombre, IdTipoDeProceso, Borrado) VALUES ('EJ-22', 'Canal de denuncias (ejemplo)', 11, 0);
INSERT INTO SUBPROCESO (Codigo, Nombre, IdProceso, Borrado, TipoOtros) VALUES ('', 'Otros', SCOPE_IDENTITY(), 0, 1);
INSERT INTO PROCESO (Codigo, Nombre, IdTipoDeProceso, Borrado) VALUES ('EJ-23', 'Protección de datos (ejemplo)', 11, 0);
INSERT INTO SUBPROCESO (Codigo, Nombre, IdProceso, Borrado, TipoOtros) VALUES ('', 'Otros', SCOPE_IDENTITY(), 0, 1);
INSERT INTO PROCESO (Codigo, Nombre, IdTipoDeProceso, Borrado) VALUES ('EJ-24', 'Ingresos de servicios (ejemplo)', 12, 0);
INSERT INTO SUBPROCESO (Codigo, Nombre, IdProceso, Borrado, TipoOtros) VALUES ('', 'Otros', SCOPE_IDENTITY(), 0, 1);
INSERT INTO PROCESO (Codigo, Nombre, IdTipoDeProceso, Borrado) VALUES ('EJ-25', 'Activos fijos (ejemplo)', 13, 0);
INSERT INTO SUBPROCESO (Codigo, Nombre, IdProceso, Borrado, TipoOtros) VALUES ('', 'Otros', SCOPE_IDENTITY(), 0, 1);
INSERT INTO PROCESO (Codigo, Nombre, IdTipoDeProceso, Borrado) VALUES ('EJ-26', 'Controles a nivel de entidad (ejemplo)', 14, 0);
INSERT INTO SUBPROCESO (Codigo, Nombre, IdProceso, Borrado, TipoOtros) VALUES ('', 'Otros', SCOPE_IDENTITY(), 0, 1);
GO
-- Dos procesos con un proceso asociado: sirven para enseñar que no se pueden eliminar.
INSERT INTO PROCESO_ASOCIADO (IdProceso, IdTipoDeAuditoria, IdPeriodo, IdSociedad, IdUsuarioResponsable, Borrado, IdSubproceso) SELECT p.IdProceso, 1, 1, 1, 1, 0, s.IdSubproceso FROM PROCESO p INNER JOIN SUBPROCESO s ON s.IdProceso = p.IdProceso WHERE p.Codigo = 'EJ-01';
INSERT INTO PROCESO_ASOCIADO (IdProceso, IdTipoDeAuditoria, IdPeriodo, IdSociedad, IdUsuarioResponsable, Borrado, IdSubproceso) SELECT p.IdProceso, 1, 1, 1, 1, 0, s.IdSubproceso FROM PROCESO p INNER JOIN SUBPROCESO s ON s.IdProceso = p.IdProceso WHERE p.Codigo = 'EJ-09';
GO
SELECT COUNT(*) AS ProcesosCreados FROM PROCESO;
GO
