# eVidencia · demo de migración a .NET 10 · Administración de Procesos

Primera funcionalidad de eVidencia traspasada a .NET: listado de Procesos con filtros, alta, edición y baja.

- **Funcionalidad y reglas:** las del código original (rama `PROD`, commit `86c33f3`, versión web 1.15.0.0).
- **Diseño de pantallas:** el del mockup «eVidencia · Mockup responsive · aplicación completa» (escritorio y móvil).
- **Datos:** el libro Excel `eVidencia_BBDD_Mock_CERT_20261112.xlsx`, hasta nuevo aviso.

## 1. Cómo ejecutarla

**En GitHub Codespaces** (sin instalar nada): botón **Code** > **Codespaces** > **Create codespace on main**.
Compila, pasa las pruebas y arranca sola; la demo se abre en el puerto 5080.

**En un equipo con el SDK de .NET 10:** `ejecutar.cmd`, o abrir `eVidencia.sln` con Visual Studio 2026 y pulsar F5.
Visual Studio 2022 no admite proyectos .NET 10.

## 2. Datos

El origen de datos se elige en `src/eVidencia.Web/appsettings.json`, clave `Datos:Modo`:

| Modo | Qué usa |
|---|---|
| `Excel` (por defecto) | El libro de `datos/`, con una hoja por tabla. Los cambios se quedan en memoria; no se escriben en el libro. |
| `Memoria` | 26 procesos de ejemplo inventados. |
| `SqlServer` | Una base de datos de eVidencia (`Datos:CadenaConexion`). Requiere `Demo:UsuarioFijo`. |

**El libro Excel no está en el repositorio** porque contiene datos personales reales (ver `datos/LEEME.md`).
Hay que copiarlo a mano a la carpeta `datos/`. Si no está, la demo arranca con los datos de ejemplo y lo avisa en el pie de página.

**Usuario de la demo:** no hay pantalla de acceso. Entra el usuario indicado en `Demo:UsuarioFijo` (su UID);
si está vacío, el primer usuario con perfil Administrador de la tabla `USUARIO`.

## 3. Guion de la demo (5 minutos)

1. Listado: cambiar de página, de número de filas y ordenar por una columna.
2. Filtro: escribir un texto en Nombre y pulsar Filtrar. Borrar filtros.
3. Alta: Crear, pulsar Guardar en vacío (tres errores de validación); rellenar y Guardar.
4. Regla de negocio: intentar otra alta con un código que ya exista.
5. Edición: abrir un proceso con **i** o doble clic, cambiar el nombre y Guardar. Volver.
6. Baja: seleccionar el proceso creado, Eliminar y confirmar.
7. Regla de negocio: intentar eliminar un proceso con procesos asociados.
8. Vista móvil: estrechar la ventana del navegador; el listado pasa a tarjetas y el menú a un panel lateral.

Capturas de cada paso, con los datos de ejemplo, en `docs/capturas`.

## 4. Qué se ha migrado y de dónde

| Pieza nueva | Origen |
|---|---|
| `Pages/Administracion/Procesos.cshtml` | Funcionalidad de `eVidencias.Web/Administracion/Procesos.aspx`; diseño del mockup |
| `Pages/Administracion/Proceso.cshtml` | Funcionalidad de `eVidencias.Web/Administracion/Proceso.aspx`; diseño del mockup |
| Servicio del listado en `Program.cs` | `AdministracionService.asmx.cs:154` (`ConsultarGridProcesos`) |
| `wwwroot/css/evidencia.css` | Hoja de estilos del mockup, sin cambios salvo retirar la franja «MOCK» |
| `wwwroot/js/evidencia.js` | Nuevo: menú, diálogos, avisos y componente de listado |
| `eVidencia.BLL` | `GestorProcesos`, `GestorSubprocesos`, `GestorLogDeEntidades` y `GestorUsuarios` del original |
| `eVidencia.DAL` | Repositorios del original; tablas según `eVidencias.edmx` |
| `eVidencia.Comun` | Componentes reutilizables: conexión a datos, orden de listados, respuesta de listado, lector de Excel, usuario actual |

Se conservan del original las direcciones (`/Administracion/Procesos.aspx`, `/Administracion/Proceso.aspx`, la del servicio),
el formato JSON del listado, las reglas de negocio, los permisos por perfil, los mensajes y los textos de auditoría.

## 5. Diferencias con la aplicación actual

Por el nuevo diseño (son las del mockup):
- Estructura con módulos arriba, menú lateral y paneles con barra de título; tipografía Figtree.
- Listado propio en lugar de jqGrid: 10, 20 o 50 filas por página (el original: 10, 20, 30, 60 o 120; 20 por defecto).
- El título del listado muestra el número de registros.
- Errores de validación y de negocio en un diálogo; avisos bajo el rótulo «Notificación».
- El nombre de usuario se muestra como «Nombre Apellidos» (el original: «Apellidos, Nombre»).
- Adaptación a móvil, que la aplicación actual no tiene.

Del mockup que la demo no incluye, por no tener datos o quedar fuera de la funcionalidad migrada:
- Sociedad y periodo activos en el menú lateral, la entrada «Inicio» y el botón «Exportar» (se muestra inactivo).

Propias de la demo:
- Usuario fijo, solo español, resto del menú sin enlace y aviso de demo en el pie.

Técnicas, a decidir en la Línea base del proyecto:
- Acceso a datos con ADO.NET; el original usa Entity Framework 4.
- El estado que Web Forms guardaba en ViewState viaja en campos ocultos sin protección.
- La validación del formulario se hace solo en el servidor.

## 6. Qué está probado y qué no

Probado el 19/11/2026 en Linux con el SDK de .NET 10.0.115:
- Compilación de la solución: 0 errores, 0 avisos.
- 37 pruebas de la lógica de negocio (`pruebas.cmd`): 37 correctas.
- 6 comprobaciones de carga del libro Excel: 6 correctas (49 procesos, 11 tipos, 80 subprocesos, 44 usuarios).
- 40 comprobaciones en Chrome sobre la aplicación en marcha, en escritorio y móvil (`docs/prueba_navegador.py`): 40 correctas.

No probado:
- **El modo SqlServer no se ha ejecutado contra ningún SQL Server.**
- La compilación con el paquete `Microsoft.Data.SqlClient` en el entorno de construcción (sí se hace en Codespaces).
- La ejecución en Windows.
- La comparación con la aplicación actual en marcha.

## 7. Observaciones sobre el libro Excel

- Dos subprocesos (Id 77 y 78) no tienen proceso; en la base de datos ese campo es obligatorio. Se cargan sin proceso.
- El libro no contiene subprocesos de tipo «Otros»; al eliminar un proceso no hay subproceso «Otros» que marcar.
- Las 3.032 líneas de `LOG_DE_ENTIDADES` no se cargan en memoria; las nuevas se numeran a continuación.
