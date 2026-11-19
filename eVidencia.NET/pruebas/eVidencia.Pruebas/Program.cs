// Pruebas de la lógica de negocio migrada (Administración de Procesos) sobre datos en memoria.
// Programa de consola sin dependencias externas: cada comprobación cita la regla del código original.
// Ejecutar:  dotnet run --project pruebas/eVidencia.Pruebas
using System.Security;
using eVidencia.BLL;
using eVidencia.Comun.Seguridad;
using eVidencia.DAL.Memoria;
using eVidencia.Entidades;
using eVidencia.Entidades.Filtros;

int correctas = 0, fallidas = 0;

void Comprobar(string regla, bool condicion)
{
    if (condicion) { correctas++; Console.WriteLine("  OK    " + regla); }
    else { fallidas++; Console.WriteLine("  FALLO " + regla); }
}

T? Captura<T>(Action accion) where T : Exception
{
    try { accion(); } catch (T ex) { return ex; }
    return null;
}

(GestorProcesos gp, AlmacenMemoria almacen) Crear(string uid = DatosDeEjemplo.UidUsuarioDemo, int? perfil = null)
{
    AlmacenMemoria almacen = AlmacenMemoria.DeEjemplo();
    if (perfil != null) almacen.Usuarios[0].IdPerfilDeUsuario = perfil.Value;
    var rp = new RepositorioProcesosMemoria(almacen);
    var rs = new RepositorioSubprocesosMemoria(almacen);
    var gu = new GestorUsuarios(new RepositorioUsuariosMemoria(almacen), new UsuarioFijo(uid));
    var gl = new GestorLogDeEntidades(new RepositorioLogDeEntidadesMemoria(almacen), gu);
    var gs = new GestorSubprocesos(rs, rp, gu, gl);
    return (new GestorProcesos(rp, rs, gu, gs, gl), almacen);
}

string NL = Environment.NewLine;

Console.WriteLine("Listado (GestorProcesos.ConsultarProcesoConFiltro)");
{
    var (gp, _) = Crear();
    var p1 = gp.ConsultarProcesoConFiltro(null, "Codigo asc", 1, 20, out int total);
    Comprobar("devuelve el total de procesos no borrados (26)", total == 26);
    Comprobar("página 1 con 20 filas", p1.Count == 20);
    Comprobar("orden inicial por Código ascendente", p1[0].Codigo == "EJ-01" && p1[19].Codigo == "EJ-20");
    var p2 = gp.ConsultarProcesoConFiltro(null, "Codigo asc", 2, 20, out _);
    Comprobar("página 2 con las 6 filas restantes", p2.Count == 6 && p2[0].Codigo == "EJ-21");
    var desc = gp.ConsultarProcesoConFiltro(null, "Codigo desc", 1, 20, out _);
    Comprobar("orden descendente", desc[0].Codigo == "EJ-26");
    var porTipo = gp.ConsultarProcesoConFiltro(null, "TIPO_DE_PROCESO.Nombre asc", 1, 20, out _);
    Comprobar("orden por la columna Tipo de Proceso", porTipo[0].TIPO_DE_PROCESO!.Nombre == "Comercialización y ventas");
    var f1 = gp.ConsultarProcesoConFiltro(new FiltroPROCESO { Codigo = "EJ-1" }, "Codigo asc", 1, 20, out int t1);
    Comprobar("filtro Código: contiene el texto", t1 == 10 && f1.All(p => p.Codigo!.Contains("EJ-1")));
    gp.ConsultarProcesoConFiltro(new FiltroPROCESO { Nombre = "gestión" }, "Codigo asc", 1, 20, out int t2);
    Comprobar("filtro Nombre: contiene el texto, sin distinguir mayúsculas", t2 == 7);
    gp.ConsultarProcesoConFiltro(new FiltroPROCESO { Codigo = "EJ-2", Nombre = "ejemplo" }, "Codigo asc", 1, 20, out int t3);
    Comprobar("los dos filtros se combinan con Y", t3 == 7);
    Comprobar("campo de orden no permitido se rechaza", Captura<ArgumentException>(() => gp.ConsultarProcesoConFiltro(null, "Borrado; DROP asc", 1, 20, out _)) != null);
}

Console.WriteLine("Alta (GestorProcesos.CrearProceso)");
{
    var (gp, almacen) = Crear();
    int id = gp.CrearProceso(new PROCESO { Codigo = "NUEVO", Nombre = "Proceso nuevo", IdTipoDeProceso = 11 });
    PROCESO creado = gp.ConsultarProceso(id);
    Comprobar("crea el proceso con sus datos", creado.Codigo == "NUEVO" && creado.Nombre == "Proceso nuevo" && creado.IdTipoDeProceso == 11);
    SUBPROCESO? otros = almacen.Subprocesos.FirstOrDefault(s => s.IdProceso == id);
    Comprobar("crea el subproceso 'Otros' (código vacío, TipoOtros)", otros != null && otros.Nombre == "Otros" && otros.Codigo == "" && otros.TipoOtros);
    Comprobar("registra dos líneas de auditoría: alta de proceso y alta de subproceso", almacen.Logs.Count == 2
        && almacen.Logs[0].IdTipoDeAccion == 1 && almacen.Logs[0].IdProceso == id && almacen.Logs[0].IdSubproceso == null
        && almacen.Logs[1].IdTipoDeAccion == 1 && almacen.Logs[1].IdProceso == id && almacen.Logs[1].IdSubproceso == otros!.IdSubproceso);
    string esperado = "Se ha creado el Proceso con el nombre: Proceso nuevo" + NL +
        "\n *[Campo Código:]* Valor nuevo: NUEVO " + NL +
        "\n *[Campo Nombre:]* Valor nuevo: Proceso nuevo " + NL +
        "\n *[Campo Tipo de Proceso:]* Valor antiguo:  - Valor nuevo: Cumplimiento ";
    Comprobar("texto de auditoría del alta igual al formato del original", almacen.Logs[0].Descripcion == esperado);
    Comprobar("la auditoría queda a nombre del usuario autenticado", almacen.Logs[0].IdUsuario == 1);
    var ex = Captura<ApplicationException>(() => gp.CrearProceso(new PROCESO { Codigo = "EJ-05", Nombre = "Duplicado", IdTipoDeProceso = 1 }));
    Comprobar("rechaza un código que ya existe: 'Ya existe otro proceso con este mismo código'", ex?.Message == "Ya existe otro proceso con este mismo código");
}

Console.WriteLine("Edición (GestorProcesos.ActualizarProceso)");
{
    var (gp, almacen) = Crear();
    PROCESO p = gp.ConsultarProceso(4);
    p.Nombre = "Compras (modificado)";
    p.IdTipoDeProceso = 8;
    gp.ActualizarProceso(p);
    PROCESO leido = gp.ConsultarProceso(4);
    Comprobar("guarda los cambios", leido.Nombre == "Compras (modificado)" && leido.IdTipoDeProceso == 8 && leido.Codigo == "EJ-04");
    string esperado = "Se ha modificado el Proceso con el nombre: Compras de bienes y servicios (ejemplo)" + NL +
        "\n *[Campo Nombre:]* Valor antiguo: Compras de bienes y servicios (ejemplo) - Valor nuevo: Compras (modificado) " + NL +
        "\n *[Campo Tipo de Proceso:]* Valor antiguo: Compra productos y servicios, inversión y cuentas a pagar - Valor nuevo: Gestión de recursos financieros ";
    Comprobar("auditoría: solo los campos cambiados, con valor antiguo y nuevo", almacen.Logs.Count == 1 && almacen.Logs[0].IdTipoDeAccion == 3 && almacen.Logs[0].Descripcion == esperado);
    p = gp.ConsultarProceso(4);
    p.Codigo = "EJ-05";
    Comprobar("rechaza cambiar a un código que ya usa otro proceso", Captura<ApplicationException>(() => gp.ActualizarProceso(p))?.Message == "Ya existe otro proceso con este mismo código");
    p = gp.ConsultarProceso(4);
    Comprobar("permite guardar manteniendo su propio código", Captura<Exception>(() => gp.ActualizarProceso(p)) == null);
    Comprobar("proceso inexistente: 'No existe el proceso'", Captura<ApplicationException>(() => gp.ConsultarProceso(9999))?.Message == "No existe el proceso");
}

Console.WriteLine("Baja (GestorProcesos.EliminarProceso)");
{
    var (gp, almacen) = Crear();
    gp.EliminarProceso(5);
    Comprobar("baja lógica: el registro sigue en la tabla, marcado como borrado", almacen.Procesos.Single(p => p.IdProceso == 5).Borrado);
    Comprobar("el proceso deja de poder consultarse", Captura<ApplicationException>(() => gp.ConsultarProceso(5))?.Message == "No existe el proceso");
    gp.ConsultarProcesoConFiltro(null, "Codigo asc", 1, 20, out int total);
    Comprobar("el listado deja de mostrarlo", total == 25);
    Comprobar("marca como borrado su subproceso 'Otros'", almacen.Subprocesos.Single(s => s.IdProceso == 5).Borrado);
    Comprobar("auditoría de la baja", almacen.Logs.Count == 1 && almacen.Logs[0].IdTipoDeAccion == 2
        && almacen.Logs[0].Descripcion == "Se ha eliminado el Proceso con el nombre: Homologación de proveedores (ejemplo)");
    Comprobar("no elimina un proceso con procesos asociados", Captura<ApplicationException>(() => gp.EliminarProceso(1))?.Message == "El Proceso tiene procesos asociados"
        && !almacen.Procesos.Single(p => p.IdProceso == 1).Borrado);
    Comprobar("el código de un proceso borrado se puede reutilizar", Captura<Exception>(() => gp.CrearProceso(new PROCESO { Codigo = "EJ-05", Nombre = "Reutilizado", IdTipoDeProceso = 1 })) == null);
}

Console.WriteLine("Permisos (GestorProcesos.AccionesPermitidas)");
{
    var (gpAdmin, _) = Crear();
    Comprobar("Administrador: consultar, crear, actualizar y eliminar", gpAdmin.AccionesPermitidas().Count == 4);
    foreach (PerfilDeUsuario perfil in Enum.GetValues<PerfilDeUsuario>().Where(x => x != PerfilDeUsuario.Administrador))
    {
        var (gp, _) = Crear(perfil: (int)perfil);
        bool soloConsulta = gp.AccionesPermitidas().SequenceEqual([GestorProcesos.AccionProceso.ConsultarProceso]);
        var ex = Captura<SecurityException>(() => gp.CrearProceso(new PROCESO { Codigo = "X", Nombre = "X", IdTipoDeProceso = 1 }));
        bool sinEliminar = Captura<SecurityException>(() => gp.EliminarProceso(5)) != null;
        Comprobar($"{perfil}: solo consulta; alta y baja rechazadas", soloConsulta && sinEliminar
            && ex?.Message == "El usuario demo.admin no tiene permisos para realizar esta acción.");
    }
    var (gpDesconocido, _) = Crear(uid: "no.existe");
    Comprobar("usuario que no está en la tabla USUARIO: 'No existe el usuario'", Captura<ApplicationException>(() => gpDesconocido.ConsultarTiposProcesos())?.Message == "No existe el usuario");
}

// Comprobaciones opcionales sobre un libro Excel de datos: dotnet run --project pruebas/eVidencia.Pruebas -- <ruta.xlsx>
if (args.Length > 0 && File.Exists(args[0]))
{
    Console.WriteLine("Carga desde Excel (" + Path.GetFileName(args[0]) + ")");
    AlmacenMemoria excel = AlmacenMemoria.DesdeExcel(args[0]);
    var rp = new RepositorioProcesosMemoria(excel);
    var rs = new RepositorioSubprocesosMemoria(excel);
    USUARIO? admin = excel.Usuarios.FirstOrDefault(u => !u.Borrado && u.IdPerfilDeUsuario == (int)PerfilDeUsuario.Administrador);
    Comprobar("el libro tiene al menos un usuario Administrador", admin != null);
    var gu = new GestorUsuarios(new RepositorioUsuariosMemoria(excel), new UsuarioFijo(admin?.UID ?? ""));
    var gl = new GestorLogDeEntidades(new RepositorioLogDeEntidadesMemoria(excel), gu);
    var gp = new GestorProcesos(rp, rs, gu, new GestorSubprocesos(rs, rp, gu, gl), gl);
    var lista = gp.ConsultarProcesoConFiltro(null, "Codigo asc", 1, 1000, out int totalExcel);
    Console.WriteLine($"        procesos={totalExcel} tipos={excel.TiposDeProceso.Count} subprocesos={excel.Subprocesos.Count} usuarios={excel.Usuarios.Count} asociados={excel.ProcesosAsociados.Count} logs en origen={excel.LogsEnOrigen}");
    Comprobar("lee los procesos y todos tienen tipo de proceso", totalExcel > 0 && lista.All(p => p.TIPO_DE_PROCESO != null));
    Comprobar("lee los tipos de proceso para el desplegable", gp.ConsultarTiposProcesos().Count == excel.TiposDeProceso.Count);
    int conAsociado = excel.ProcesosAsociados.First();
    Comprobar("un proceso con procesos asociados no se puede eliminar", Captura<ApplicationException>(() => gp.EliminarProceso(conAsociado))?.Message == "El Proceso tiene procesos asociados");
    int nuevo = gp.CrearProceso(new PROCESO { Codigo = "ZZ-PRB", Nombre = "Prueba de carga", IdTipoDeProceso = excel.TiposDeProceso[0].IdTipoDeProceso });
    Comprobar("alta sobre los datos del libro y numeración del log a continuación del origen", excel.Logs.Count == 2 && excel.Logs[0].IdLogDeEntidades == excel.LogsEnOrigen + 1);
    gp.EliminarProceso(nuevo);
    Comprobar("baja del proceso recién creado", excel.Procesos.Single(p => p.IdProceso == nuevo).Borrado);
}

Console.WriteLine();
Console.WriteLine($"Resultado: {correctas} correctas, {fallidas} fallidas");
return fallidas == 0 ? 0 : 1;
