# Genera, a partir del libro Excel completo, una copia reducida y sin datos personales para el repositorio.
#
#   python3 generar_copia_sin_datos_personales.py <libro_completo.xlsx> <copia.xlsx>
#
# La copia contiene solo las hojas que usa la funcionalidad migrada (Administración de Procesos):
#   - TIPO_DE_PROCESO, PROCESO, SUBPROCESO, PERFIL_DE_USUARIO, TIPO_DE_ACCION: sin cambios.
#   - PROCESO_ASOCIADO: sin cambios salvo la columna de texto libre "Otros", que se vacía.
#   - USUARIO: se conservan los identificadores, el perfil y las demás claves; nombre, apellidos,
#     correo y UID se sustituyen por valores ficticios.
#   - LOG_DE_ENTIDADES: solo la cabecera (las descripciones contienen nombres de personas).
# El resto de hojas del libro completo no se copia.
import sys
import openpyxl

origen, destino = sys.argv[1], sys.argv[2]
lib = openpyxl.load_workbook(origen, read_only=True, data_only=True)
out = openpyxl.Workbook()
out.remove(out.active)

def filas(hoja):
    return [list(f) for f in lib[hoja].iter_rows(values_only=True)]

def escribir(hoja, datos):
    ws = out.create_sheet(hoja)
    for f in datos:
        ws.append(f)

resumen = []

for hoja in ['TIPO_DE_PROCESO', 'PROCESO', 'SUBPROCESO', 'PERFIL_DE_USUARIO', 'TIPO_DE_ACCION']:
    d = filas(hoja)
    escribir(hoja, d)
    resumen.append((hoja, len(d) - 1, 'Sin cambios'))

d = filas('PROCESO_ASOCIADO')
col = d[0].index('Otros')
for f in d[1:]:
    f[col] = None
escribir('PROCESO_ASOCIADO', d)
resumen.append(('PROCESO_ASOCIADO', len(d) - 1, 'Columna de texto libre "Otros" vaciada'))

d = filas('USUARIO')
cab = d[0]
iNombre, iApellidos, iEmail, iUid = (cab.index(c) for c in ('Nombre', 'Apellidos', 'Email', 'UID'))
iPerfil, iId = cab.index('IdPerfilDeUsuario'), cab.index('IdUsuario')
perfiles = {f[0]: f[1] for f in filas('PERFIL_DE_USUARIO')[1:]}
for f in d[1:]:
    n = int(f[iId])
    f[iNombre] = 'Usuario'
    f[iApellidos] = 'Demo %02d %s' % (n, perfiles.get(f[iPerfil], ''))
    f[iEmail] = 'usuario%02d@ejemplo.invalid' % n
    f[iUid] = 'DEMO%05d' % n
escribir('USUARIO', d)
resumen.append(('USUARIO', len(d) - 1, 'Nombre, Apellidos, Email y UID sustituidos por valores ficticios'))

d = filas('LOG_DE_ENTIDADES')
escribir('LOG_DE_ENTIDADES', d[:1])
resumen.append(('LOG_DE_ENTIDADES', 0, 'Solo cabecera: las %d líneas del origen no se copian' % (len(d) - 1)))

ws = out.create_sheet('_ORIGEN', 0)
ws.append(['Tabla', 'Filas', 'Tratamiento'])
for r in resumen:
    ws.append(list(r))
ws.append([])
ws.append(['Copia reducida y sin datos personales, generada con generar_copia_sin_datos_personales.py.'])
ws.append(['El resto de hojas del libro completo no se incluye.'])

out.save(destino)
for r in resumen:
    print('%-20s %5d  %s' % r)
