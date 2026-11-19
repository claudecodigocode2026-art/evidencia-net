# eVidencia · demo de migración a .NET 10 · Administración de Procesos

Primera funcionalidad de eVidencia traspasada a .NET: listado de Procesos con filtros, alta, edición y baja.
Origen: rama `PROD` del repositorio (commit `86c33f3`, 22/10/2026, versión web 1.15.0.0).

## 1. Qué necesitas

- **Visual Studio 2026** con la carga de trabajo "Desarrollo de ASP.NET y web", o
- el **SDK de .NET 10** y la línea de comandos (`ejecutar.cmd`).

Visual Studio 2022 no sirve para abrir esta solución: no admite proyectos .NET 10 (error NETSDK1209).
Con Visual Studio 2022 instalado puedes usar igualmente `ejecutar.cmd` si instalas el SDK de .NET 10.

La primera compilación descarga un paquete de nuget.org (`Microsoft.Data.SqlClient`). Hace falta salida a internet o un repositorio NuGet corporativo.

## 2. Cómo ejecutarla

**Opción A · sin base de datos (así viene configurada)**
1. Abre `eVidencia.sln`.
2. Comprueba que el proyecto de inicio es `eVidencia.Web` y pulsa F5.
3. Se abre `http://localhost:5080/Administracion/Procesos.aspx` con 26 procesos de ejemplo.

Los datos están en memoria y se pierden al parar la aplicación.

**Opción B · base de datos local de pruebas**
1. Ejecuta `bd\01_crear_bd_local.sql` en LocalDB (`(localdb)\MSSQLLocalDB`): desde Visual Studio (Ver > Explorador de objetos de SQL Server > Nueva consulta) o con `bd\crear_bd_local.cmd`.
2. En `src\eVidencia.Web\appsettings.json` cambia `"Modo": "Memoria"` por `"Modo": "SqlServer"`.
3. F5.

**Opción C · base de datos real de Desarrollo o CERT**
1. En `appsettings.json`: `"Modo": "SqlServer"` y en `CadenaConexion` la cadena de esa base de datos.
2. En `Demo:UsuarioFijo` pon el UID de un usuario que exista en la tabla `USUARIO` con perfil Administrador.
3. F5.

Atención: en la opción C las altas, cambios y bajas se graban en esa base de datos y en su `LOG_DE_ENTIDADES`.
No guardes la cadena de conexión real en ningún ZIP ni repositorio.

## 3. Guion de la demo (5 minutos)

1. Listado: 26 procesos, 20 por página; cambiar de página y ordenar por una columna.
2. Filtro: escribir `gestión` en Nombre y pulsar Filtrar (7 resultados). Borrar Filtros.
3. Alta: Crear, pulsar Guardar en vacío (tres errores de validación); rellenar y Guardar (aviso "Registro creado con éxito").
4. Regla de negocio: intentar otra alta con el código `EJ-05` ("Ya existe otro proceso con este mismo código").
5. Edición: cambiar el nombre y Guardar. Volver.
6. Baja: seleccionar el proceso creado, Eliminar y confirmar.
7. Regla de negocio: intentar eliminar `EJ-01` ("El Proceso tiene procesos asociados").

Capturas de cada paso en `docs\capturas`.

## 4. Qué se ha migrado y de dónde

| Pieza nueva | Origen en PROD |
|---|---|
| `Pages/Administracion/Procesos.cshtml` | `eVidencias.Web/Administracion/Procesos.aspx` y `.aspx.cs` |
| `Pages/Administracion/Proceso.cshtml` | `eVidencias.Web/Administracion/Proceso.aspx` y `.aspx.cs` |
| Servicio del listado en `Program.cs` | `AdministracionService.asmx.cs:154` (`ConsultarGridProcesos`) |
| `Pages/Shared/_Layout.cshtml`, `_Menu.cshtml` | `eVidencias.Master`, `Controles/Menu.ascx` |
| `eVidencia.BLL/GestorProcesos.cs` | `eVidencias.BLL/GestorProcesos.cs` (8 de sus métodos) |
| `eVidencia.BLL/GestorSubprocesos.cs` | `GestorSubprocesos.cs` (permisos y alta del subproceso "Otros") |
| `eVidencia.BLL/GestorLogDeEntidades.cs` | `GestorLogDeEntidades.cs` (auditoría de Proceso y Subproceso) |
| `eVidencia.BLL/GestorUsuarios.cs` | `GestorUsuarios.cs` (`ConsultarUsuarioAutenticado`) |
| `eVidencia.DAL/SqlServer` | `PROCESORepository.cs` y resto de repositorios; tablas según `eVidencias.edmx` |
| `wwwroot/Scripts`, `Styles`, `Images` | Copia sin cambios de las carpetas del original |

Se conservan las direcciones (`/Administracion/Procesos.aspx`, `/Administracion/Proceso.aspx`, la del servicio),
el formato JSON del listado, las reglas de negocio, los permisos por perfil, los mensajes y los textos de auditoría.
Cada clase y método cita en su comentario el fichero y la línea de origen.

Componentes comunes reutilizables por otras aplicaciones (`eVidencia.Comun`): conexión a base de datos,
ordenación de listados con lista de campos permitidos, respuesta de jqGrid y usuario actual.

## 5. Diferencias con el original

Propias de la demo (no son "tal cual"):
- Usuario fijo en lugar de la pantalla de acceso.
- Solo en español (el original tiene además inglés y portugués).
- Sin el botón "Exportar a Excel".
- El resto del menú se muestra sin enlace.
- Aviso de demo en el pie de página.
- Modo de datos en memoria, que no existe en el original.

Técnicas, a decidir en la Línea base del proyecto:
- Acceso a datos con ADO.NET y SQL escrito a mano; el original usa Entity Framework 4. No prejuzga la decisión del proyecto.
- El estado que Web Forms guardaba en ViewState viaja en campos ocultos sin protección.
- La validación del formulario se hace en el servidor; el original valida además en el navegador.
- Los identificadores HTML de los controles no llevan el prefijo de Web Forms.
- El orden del listado solo admite las cuatro columnas de la pantalla.

Inferencias pendientes de contrastar con la aplicación real:
- Que el texto de auditoría del alta incluye la línea "Tipo de Proceso" y, en el subproceso, la línea "Proceso"
  (depende de la carga diferida de Entity Framework en el original).
- Que las búsquedas no distinguen mayúsculas (depende de la intercalación de la base de datos).

## 6. Qué está probado y qué no

Probado el 19/11/2026 en Linux con el SDK de .NET 10.0.115:
- Compilación de la solución: 0 errores, 0 avisos.
- 37 pruebas de la lógica de negocio (`pruebas.cmd`): 37 correctas.
- 32 comprobaciones en navegador Chrome sobre la aplicación en marcha (`docs\prueba_navegador.py`): 32 correctas.

No probado:
- **El modo SqlServer no se ha ejecutado contra ningún SQL Server.** El código compila salvo la línea que registra
  el proveedor, y la sintaxis del SQL está analizada, pero no se ha ejecutado. Prueba las opciones B o C antes de la demo.
- La compilación con el paquete `Microsoft.Data.SqlClient` (el entorno de construcción no tenía acceso a nuget.org).
- La ejecución en Windows y en Visual Studio.
- La comparación con la aplicación actual en marcha.
