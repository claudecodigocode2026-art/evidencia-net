# Carpeta de datos

Aquí se coloca el libro Excel que la demo usa como base de datos:

    eVidencia_BBDD_Mock_CERT_20261112.xlsx

**El libro no se sube al repositorio** (los ficheros `.xlsx` de esta carpeta están en `.gitignore`):
contiene nombres, correos e identificadores de usuarios reales de Telefónica.

- Si el libro está en esta carpeta, la demo carga de él las tablas TIPO_DE_PROCESO, PERFIL_DE_USUARIO,
  USUARIO, PROCESO, SUBPROCESO y PROCESO_ASOCIADO. Los cambios hechos en la demo se quedan en memoria:
  no se escriben en el libro.
- Si el libro no está, la demo arranca con datos de ejemplo inventados y lo avisa en el pie de página.

El nombre y la ruta del libro se configuran en `src/eVidencia.Web/appsettings.json` (`Datos:FicheroExcel`).
