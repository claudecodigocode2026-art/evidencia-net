# Carpeta de datos

La demo usa como base de datos un libro Excel con una hoja por tabla. Busca, por este orden:

1. `eVidencia_BBDD_Mock_CERT_20261112.xlsx`: el libro completo. **No está en el repositorio** (los `.xlsx` de esta
   carpeta están en `.gitignore`) porque contiene nombres, correos e identificadores de usuarios reales.
   Si se copia aquí a mano, la demo lo usa.
2. `eVidencia_BBDD_Demo_sin_datos_personales.xlsx`: copia reducida y sin datos personales. **Sí está en el repositorio.**

Si no hay ninguno, la demo arranca con datos de ejemplo inventados y lo avisa en el pie de página.
Los cambios hechos en la demo se quedan en memoria: no se escriben en el libro.

## Qué contiene la copia sin datos personales

Se genera con `generar_copia_sin_datos_personales.py` a partir del libro completo. Su hoja `_ORIGEN` lo resume:

| Hoja | Tratamiento |
|---|---|
| TIPO_DE_PROCESO, PROCESO, SUBPROCESO, PERFIL_DE_USUARIO, TIPO_DE_ACCION | Sin cambios |
| PROCESO_ASOCIADO | Sin cambios, salvo la columna de texto libre «Otros», que se vacía |
| USUARIO | Se conservan identificadores, perfil y claves; nombre, apellidos, correo y UID son ficticios |
| LOG_DE_ENTIDADES | Solo la cabecera |

El resto de hojas del libro completo (evidencias, planificadores, comentarios, registros, parámetros…) no se incluye:
contienen textos libres, correos y nombres que habrá que revisar tabla a tabla cuando se migre la funcionalidad que las usa.

Las rutas se configuran en `src/eVidencia.Web/appsettings.json` (`Datos:FicheroExcel`).
