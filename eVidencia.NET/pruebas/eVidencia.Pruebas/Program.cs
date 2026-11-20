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

// Una comprobación cuyo texto empieza por "[DM-02]" pertenece a ese caso del
// documento «eVidencia - Plan de Pruebas (PRO 1.15)». Al final se resume por caso.
var porCaso = new SortedDictionary<string, (int Correctas, int Fallidas)>(StringComparer.Ordinal);

void Comprobar(string regla, bool condicion)
{
    if (condicion) { correctas++; Console.WriteLine("  OK    " + regla); }
    else { fallidas++; Console.WriteLine("  FALLO " + regla); }

    int cierre = regla.IndexOf(']');
    if (regla.StartsWith('[') && cierre > 1)
    {
        string caso = regla[1..cierre];
        porCaso.TryGetValue(caso, out var cuenta);
        porCaso[caso] = condicion ? (cuenta.Correctas + 1, cuenta.Fallidas) : (cuenta.Correctas, cuenta.Fallidas + 1);
    }
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
    Comprobar("[DM-17] devuelve el total de procesos no borrados (26)", total == 26);
    Comprobar("[DM-17] página 1 con 20 filas", p1.Count == 20);
    Comprobar("[DM-17] orden inicial por Código ascendente", p1[0].Codigo == "EJ-01" && p1[19].Codigo == "EJ-20");
    var p2 = gp.ConsultarProcesoConFiltro(null, "Codigo asc", 2, 20, out _);
    Comprobar("[DM-17] página 2 con las 6 filas restantes", p2.Count == 6 && p2[0].Codigo == "EJ-21");
    var desc = gp.ConsultarProcesoConFiltro(null, "Codigo desc", 1, 20, out _);
    Comprobar("[DM-17] orden descendente", desc[0].Codigo == "EJ-26");
    var porTipo = gp.ConsultarProcesoConFiltro(null, "TIPO_DE_PROCESO.Nombre asc", 1, 20, out _);
    Comprobar("[DM-17] orden por la columna Tipo de Proceso", porTipo[0].TIPO_DE_PROCESO!.Nombre == "Comercialización y ventas");
    var f1 = gp.ConsultarProcesoConFiltro(new FiltroPROCESO { Codigo = "EJ-1" }, "Codigo asc", 1, 20, out int t1);
    Comprobar("[DM-17] filtro Código: contiene el texto", t1 == 10 && f1.All(p => p.Codigo!.Contains("EJ-1")));
    gp.ConsultarProcesoConFiltro(new FiltroPROCESO { Nombre = "gestión" }, "Codigo asc", 1, 20, out int t2);
    Comprobar("[DM-17] filtro Nombre: contiene el texto, sin distinguir mayúsculas", t2 == 7);
    gp.ConsultarProcesoConFiltro(new FiltroPROCESO { Codigo = "EJ-2", Nombre = "ejemplo" }, "Codigo asc", 1, 20, out int t3);
    Comprobar("[DM-17] los dos filtros se combinan con Y", t3 == 7);
    Comprobar("[DM-17] campo de orden no permitido se rechaza", Captura<ArgumentException>(() => gp.ConsultarProcesoConFiltro(null, "Borrado; DROP asc", 1, 20, out _)) != null);
}

Console.WriteLine("Alta (GestorProcesos.CrearProceso)");
{
    var (gp, almacen) = Crear();
    int id = gp.CrearProceso(new PROCESO { Codigo = "NUEVO", Nombre = "Proceso nuevo", IdTipoDeProceso = 11 });
    PROCESO creado = gp.ConsultarProceso(id);
    Comprobar("[DM-01] crea el proceso con sus datos", creado.Codigo == "NUEVO" && creado.Nombre == "Proceso nuevo" && creado.IdTipoDeProceso == 11);
    SUBPROCESO? otros = almacen.Subprocesos.FirstOrDefault(s => s.IdProceso == id);
    Comprobar("[DM-01] crea el subproceso 'Otros' (código vacío, TipoOtros)", otros != null && otros.Nombre == "Otros" && otros.Codigo == "" && otros.TipoOtros);
    Comprobar("[DM-01] registra dos líneas de auditoría: alta de proceso y alta de subproceso", almacen.Logs.Count == 2
        && almacen.Logs[0].IdTipoDeAccion == 1 && almacen.Logs[0].IdProceso == id && almacen.Logs[0].IdSubproceso == null
        && almacen.Logs[1].IdTipoDeAccion == 1 && almacen.Logs[1].IdProceso == id && almacen.Logs[1].IdSubproceso == otros!.IdSubproceso);
    string esperado = "Se ha creado el Proceso con el nombre: Proceso nuevo" + NL +
        "\n *[Campo Código:]* Valor nuevo: NUEVO " + NL +
        "\n *[Campo Nombre:]* Valor nuevo: Proceso nuevo " + NL +
        "\n *[Campo Tipo de Proceso:]* Valor antiguo:  - Valor nuevo: Cumplimiento ";
    Comprobar("[DM-01] texto de auditoría del alta igual al formato del original", almacen.Logs[0].Descripcion == esperado);
    Comprobar("[DM-01] la auditoría queda a nombre del usuario autenticado", almacen.Logs[0].IdUsuario == 1);
    var ex = Captura<ApplicationException>(() => gp.CrearProceso(new PROCESO { Codigo = "EJ-05", Nombre = "Duplicado", IdTipoDeProceso = 1 }));
    Comprobar("[DM-02] rechaza un código que ya existe: 'Ya existe otro proceso con este mismo código'", ex?.Message == "Ya existe otro proceso con este mismo código");
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
    Comprobar("[DM-02] rechaza cambiar a un código que ya usa otro proceso", Captura<ApplicationException>(() => gp.ActualizarProceso(p))?.Message == "Ya existe otro proceso con este mismo código");
    p = gp.ConsultarProceso(4);
    Comprobar("permite guardar manteniendo su propio código", Captura<Exception>(() => gp.ActualizarProceso(p)) == null);
    Comprobar("proceso inexistente: 'No existe el proceso'", Captura<ApplicationException>(() => gp.ConsultarProceso(9999))?.Message == "No existe el proceso");
}

Console.WriteLine("Baja (GestorProcesos.EliminarProceso)");
{
    var (gp, almacen) = Crear();
    gp.EliminarProceso(5);
    Comprobar("[DM-04] baja lógica: el registro sigue en la tabla, marcado como borrado", almacen.Procesos.Single(p => p.IdProceso == 5).Borrado);
    Comprobar("[DM-04] el proceso deja de poder consultarse", Captura<ApplicationException>(() => gp.ConsultarProceso(5))?.Message == "No existe el proceso");
    gp.ConsultarProcesoConFiltro(null, "Codigo asc", 1, 20, out int total);
    Comprobar("[DM-04] el listado deja de mostrarlo", total == 25);
    Comprobar("[DM-04] marca como borrado su subproceso 'Otros'", almacen.Subprocesos.Single(s => s.IdProceso == 5).Borrado);
    Comprobar("[DM-04] auditoría de la baja", almacen.Logs.Count == 1 && almacen.Logs[0].IdTipoDeAccion == 2
        && almacen.Logs[0].Descripcion == "Se ha eliminado el Proceso con el nombre: Homologación de proveedores (ejemplo)");
    Comprobar("[DM-03] no elimina un proceso con procesos asociados", Captura<ApplicationException>(() => gp.EliminarProceso(1))?.Message == "El Proceso tiene procesos asociados"
        && !almacen.Procesos.Single(p => p.IdProceso == 1).Borrado);
    Comprobar("[DM-04] el código de un proceso borrado se puede reutilizar", Captura<Exception>(() => gp.CrearProceso(new PROCESO { Codigo = "EJ-05", Nombre = "Reutilizado", IdTipoDeProceso = 1 })) == null);
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

// ---------------------------------------------------------------------------------------------
// Subprocesos (GestorSubprocesos). Datos de ejemplo: 6 subprocesos que no son "Otros" (101 a 106);
// el 101 se usa en un proceso asignado y el 103 solo en uno ya borrado.
// ---------------------------------------------------------------------------------------------
(GestorSubprocesos gs, GestorProcesos gp, AlmacenMemoria almacen) CrearSub(int? perfil = null)
{
    AlmacenMemoria almacen = AlmacenMemoria.DeEjemplo();
    if (perfil != null) almacen.Usuarios[0].IdPerfilDeUsuario = perfil.Value;
    var rp = new RepositorioProcesosMemoria(almacen);
    var rs = new RepositorioSubprocesosMemoria(almacen);
    var gu = new GestorUsuarios(new RepositorioUsuariosMemoria(almacen), new UsuarioFijo(DatosDeEjemplo.UidUsuarioDemo));
    var gl = new GestorLogDeEntidades(new RepositorioLogDeEntidadesMemoria(almacen), gu);
    var gs = new GestorSubprocesos(rs, rp, gu, gl);
    return (gs, new GestorProcesos(rp, rs, gu, gs, gl), almacen);
}

Console.WriteLine();
Console.WriteLine("Subprocesos: listado (GestorSubprocesos.ConsultarSubprocesoConFiltro)");
{
    var (gs, gp, _) = CrearSub();
    var p1 = gs.ConsultarSubprocesoConFiltro(null, "Codigo asc", 1, 20, out int total);
    Comprobar("no incluye los subprocesos de tipo 'Otros' (6 de 32)", total == 6 && p1.All(s => !s.TipoOtros));
    Comprobar("orden inicial por Código ascendente", p1[0].Codigo == "EJ-01.1" && p1[5].Codigo == "EJ-09.1");
    Comprobar("cada fila trae el nombre de su proceso", p1.All(s => !string.IsNullOrEmpty(s.PROCESO?.Nombre)));
    gs.ConsultarSubprocesoConFiltro(new FiltroSUBPROCESO { Codigo = "ej-02" }, "Codigo asc", 1, 20, out int t1);
    Comprobar("filtro Código: contiene el texto, sin distinguir mayúsculas", t1 == 2);
    gs.ConsultarSubprocesoConFiltro(new FiltroSUBPROCESO { Nombre = "ofertas" }, "Codigo asc", 1, 20, out int t2);
    Comprobar("filtro Nombre: contiene el texto", t2 == 2);
    var f3 = gs.ConsultarSubprocesoConFiltro(new FiltroSUBPROCESO { ListaIdProceso = [1] }, "Codigo asc", 1, 20, out int t3);
    Comprobar("filtro Proceso: solo los subprocesos de ese proceso", t3 == 2 && f3.All(s => s.IdProceso == 1));
    gs.ConsultarSubprocesoConFiltro(new FiltroSUBPROCESO { Codigo = "EJ-01", ListaIdProceso = [2] }, "Codigo asc", 1, 20, out int t4);
    Comprobar("los filtros se combinan con Y", t4 == 0);
    var porProceso = gs.ConsultarSubprocesoConFiltro(null, "PROCESO.Nombre desc", 1, 20, out _);
    Comprobar("orden por la columna Proceso", porProceso[0].IdProceso == 1);
    var pag2 = gs.ConsultarSubprocesoConFiltro(null, "Codigo asc", 2, 4, out _);
    Comprobar("paginación: página 2 de 4 filas con las 2 restantes", pag2.Count == 2 && pag2[0].Codigo == "EJ-03.1");
    Comprobar("campo de orden no permitido se rechaza", Captura<ArgumentException>(() => gs.ConsultarSubprocesoConFiltro(null, "Borrado; DROP asc", 1, 20, out _)) != null);
    Comprobar("desplegable de la ficha: procesos no borrados por código", gp.ConsultarProcesos().Select(p => p.Codigo).SequenceEqual(gp.ConsultarProcesos().Select(p => p.Codigo).OrderBy(c => c, StringComparer.OrdinalIgnoreCase)) && gp.ConsultarProcesos().Count == 26);
    Comprobar("desplegable del filtro: procesos con algún subproceso", gp.ConsultarProcesosParaSubprocesos().Count == 26);
}

Console.WriteLine();
Console.WriteLine("Subprocesos: alta (GestorSubprocesos.CrearSubproceso)");
{
    var (gs, _, almacen) = CrearSub();
    int id = gs.CrearSubproceso(new SUBPROCESO { Codigo = "EJ-04.1", Nombre = "Solicitud de compra", IdProceso = 4, TipoOtros = true });
    SUBPROCESO creado = almacen.Subprocesos.Single(s => s.IdSubproceso == id);
    Comprobar("crea el subproceso con sus datos", creado.Codigo == "EJ-04.1" && creado.Nombre == "Solicitud de compra" && creado.IdProceso == 4);
    Comprobar("nunca lo crea como tipo 'Otros', aunque se pida", !creado.TipoOtros);
    string esperado = "Se ha creado el Subproceso con el nombre: Solicitud de compra" + NL +
                      "\n *[Campo Código:]* Valor nuevo: EJ-04.1 " + NL +
                      "\n *[Campo Nombre:]* Valor nuevo: Solicitud de compra " + NL +
                      "\n *[Campo Proceso:]* Valor antiguo:  - Valor nuevo: Compras de bienes y servicios (ejemplo) ";
    Comprobar("texto de auditoría del alta igual al formato del original", almacen.Logs.Count == 1 && almacen.Logs[0].Descripcion == esperado);
    Comprobar("la auditoría apunta al proceso y al subproceso", almacen.Logs[0].IdProceso == 4 && almacen.Logs[0].IdSubproceso == id && almacen.Logs[0].IdTipoDeAccion == 1);
    Comprobar("[DM-05] rechaza un código que ya existe: 'Ya existe otro subproceso con este mismo código'",
        Captura<ApplicationException>(() => gs.CrearSubproceso(new SUBPROCESO { Codigo = "EJ-01.1", Nombre = "Repetido", IdProceso = 2 }))?.Message == "Ya existe otro subproceso con este mismo código");
    Comprobar("[DM-05] el rechazo no distingue mayúsculas ni el proceso",
        Captura<ApplicationException>(() => gs.CrearSubproceso(new SUBPROCESO { Codigo = "ej-09.1", Nombre = "Repetido", IdProceso = 3 })) != null);
    Comprobar("un código vacío choca con los subprocesos 'Otros', que tienen código vacío",
        Captura<ApplicationException>(() => gs.CrearSubproceso(new SUBPROCESO { Codigo = "", Nombre = "Sin código", IdProceso = 2 }))?.Message == "Ya existe otro subproceso con este mismo código");
}

Console.WriteLine();
Console.WriteLine("Subprocesos: edición (GestorSubprocesos.ActualizarSubproceso)");
{
    var (gs, _, almacen) = CrearSub();
    SUBPROCESO s = gs.ConsultarSubproceso(105);
    s.Nombre = "Registro de pedidos (modificado)";
    s.IdProceso = 2;
    gs.ActualizarSubproceso(s);
    SUBPROCESO leido = gs.ConsultarSubproceso(105);
    Comprobar("guarda nombre y proceso", leido.Nombre == "Registro de pedidos (modificado)" && leido.IdProceso == 2 && leido.Codigo == "EJ-03.1");
    string esperado = "Se ha modificado el Subproceso con el nombre: Registro de pedidos (ejemplo)" + NL +
                      "\n *[Campo Nombre:]* Valor antiguo: Registro de pedidos (ejemplo) - Valor nuevo: Registro de pedidos (modificado) " + NL +
                      "\n *[Campo Proceso:]* Valor antiguo: Gestión de pedidos de clientes (ejemplo) - Valor nuevo: Gestión de ofertas comerciales (ejemplo) ";
    Comprobar("auditoría: solo los campos cambiados, con valor antiguo y nuevo", almacen.Logs.Count == 1 && almacen.Logs[0].IdTipoDeAccion == 3 && almacen.Logs[0].Descripcion == esperado);
    Comprobar("la auditoría de la modificación apunta al proceso anterior", almacen.Logs[0].IdProceso == 3 && almacen.Logs[0].IdSubproceso == 105);
    s.Codigo = "EJ-01.2";
    Comprobar("[DM-05] rechaza cambiar a un código que ya usa otro subproceso", Captura<ApplicationException>(() => gs.ActualizarSubproceso(s))?.Message == "Ya existe otro subproceso con este mismo código");
    s.Codigo = "EJ-03.1";
    Comprobar("permite guardar manteniendo su propio código", Captura<Exception>(() => gs.ActualizarSubproceso(s)) == null);
    Comprobar("subproceso inexistente: 'No existe el subproceso'", Captura<ApplicationException>(() => gs.ConsultarSubproceso(9999))?.Message == "No existe el subproceso");
    Comprobar("actualizar un subproceso inexistente: 'No existe el subproceso'", Captura<ApplicationException>(() => gs.ActualizarSubproceso(new SUBPROCESO { IdSubproceso = 9999, Codigo = "NO-HAY", Nombre = "x", IdProceso = 1 }))?.Message == "No existe el subproceso");
}

Console.WriteLine();
Console.WriteLine("Subprocesos: baja (GestorSubprocesos.EliminarSubproceso)");
{
    var (gs, _, almacen) = CrearSub();
    Comprobar("[DM-06] no elimina un subproceso usado en un proceso asignado",
        Captura<ApplicationException>(() => gs.EliminarSubproceso(101))?.Message == "No se puede eliminar el Subproceso porque esta asociado a Procesos Asignados."
        && !almacen.Subprocesos.Single(x => x.IdSubproceso == 101).Borrado && almacen.Logs.Count == 0);
    Comprobar("[DM-06] sí elimina si su único proceso asignado está borrado", Captura<Exception>(() => gs.EliminarSubproceso(103)) == null);
    Comprobar("baja lógica: el registro sigue en la tabla, marcado como borrado", almacen.Subprocesos.Single(x => x.IdSubproceso == 103).Borrado);
    Comprobar("auditoría de la baja", almacen.Logs.Count == 1 && almacen.Logs[0].IdTipoDeAccion == 2
        && almacen.Logs[0].Descripcion == "Se ha eliminado el Subproceso con el nombre: Elaboración de ofertas (ejemplo)" && almacen.Logs[0].IdSubproceso == 103);
    Comprobar("el subproceso deja de poder consultarse", Captura<ApplicationException>(() => gs.ConsultarSubproceso(103))?.Message == "No existe el subproceso");
    gs.ConsultarSubprocesoConFiltro(null, "Codigo asc", 1, 20, out int total);
    Comprobar("el listado deja de mostrarlo", total == 5);
    Comprobar("eliminar dos veces: 'No existe el subproceso'", Captura<ApplicationException>(() => gs.EliminarSubproceso(103))?.Message == "No existe el subproceso");
    Comprobar("el código de un subproceso borrado se puede reutilizar", Captura<Exception>(() => gs.CrearSubproceso(new SUBPROCESO { Codigo = "EJ-02.1", Nombre = "Reutilizado", IdProceso = 2 })) == null);
}

Console.WriteLine();
Console.WriteLine("Subprocesos: permisos por perfil (GestorSubprocesos.AccionesPermitidas)");
{
    var (gsAdmin, _, _) = CrearSub();
    Comprobar("Administrador: consultar, crear, actualizar y eliminar", gsAdmin.AccionesPermitidas().Count == 4);
    foreach (PerfilDeUsuario perfil in new[] { PerfilDeUsuario.ResponsableDeAuditoria, PerfilDeUsuario.AdministradorPais, PerfilDeUsuario.ResponsableEmpresa })
    {
        var (gs, _, almacen) = CrearSub((int)perfil);
        bool crea = Captura<Exception>(() => gs.CrearSubproceso(new SUBPROCESO { Codigo = "P-" + (int)perfil, Nombre = "x", IdProceso = 4 })) == null;
        SUBPROCESO s = gs.ConsultarSubproceso(105);
        bool noActualiza = Captura<SecurityException>(() => gs.ActualizarSubproceso(s))?.Message == "El usuario demo.admin no tiene permisos para realizar esta acción.";
        bool elimina = Captura<Exception>(() => gs.EliminarSubproceso(105)) == null;
        Comprobar($"{perfil}: consulta, crea y elimina, pero no puede actualizar", crea && noActualiza && elimina);
    }
    foreach (PerfilDeUsuario perfil in new[] { PerfilDeUsuario.Auditor, PerfilDeUsuario.AuditorExterno, PerfilDeUsuario.ResponsableControl, PerfilDeUsuario.ResponsableSeguimientoArea })
    {
        var (gs, _, _) = CrearSub((int)perfil);
        bool soloConsulta = gs.AccionesPermitidas().SequenceEqual([GestorSubprocesos.AccionSubproceso.ConsultarSubproceso]);
        bool sinAlta = Captura<SecurityException>(() => gs.CrearSubproceso(new SUBPROCESO { Codigo = "X", Nombre = "X", IdProceso = 4 })) != null;
        bool sinBaja = Captura<SecurityException>(() => gs.EliminarSubproceso(105)) != null;
        Comprobar($"{perfil}: solo consulta; alta y baja rechazadas", soloConsulta && sinAlta && sinBaja);
    }
}

// ---------------------------------------------------------------------------------------------
// Sociedades (GestorSociedades). Datos de ejemplo: 7 sociedades, la 6 borrada.
//   1: con procesos asignados · 2: sociedad del usuario de la demo · 3: asignada al usuario de la demo
//   4: solo un proceso asignado borrado · 5: sin uso · 7: sociedad de un usuario dado de baja
// ---------------------------------------------------------------------------------------------
(GestorSociedades gso, GestorDatosMaestros gdm, AlmacenMemoria almacen) CrearSoc(int? perfil = null)
{
    AlmacenMemoria almacen = AlmacenMemoria.DeEjemplo();
    if (perfil != null) almacen.Usuarios[0].IdPerfilDeUsuario = perfil.Value;
    var rso = new RepositorioSociedadesMemoria(almacen);
    var gu = new GestorUsuarios(new RepositorioUsuariosMemoria(almacen), new UsuarioFijo(DatosDeEjemplo.UidUsuarioDemo));
    var gl = new GestorLogDeEntidades(new RepositorioLogDeEntidadesMemoria(almacen), gu);
    return (new GestorSociedades(rso, gu, gl), new GestorDatosMaestros(rso, gu), almacen);
}

Console.WriteLine();
Console.WriteLine("Sociedades: listado (GestorSociedades.ConsultarSociedadConFiltro)");
{
    var (gso, gdm, _) = CrearSoc();
    var p1 = gso.ConsultarSociedadConFiltro(null, "CodigoEsif asc", 1, 20, out int total);
    Comprobar("no incluye las sociedades borradas (6 de 7)", total == 6 && p1.All(s => s.IdSociedad != 6));
    Comprobar("orden inicial por Código eSif ascendente", p1[0].CodigoEsif == "E001" && p1[5].CodigoEsif == "E007");
    Comprobar("cada fila trae el tipo de empresa y el país", p1.All(s => !string.IsNullOrEmpty(s.TIPO_DE_EMPRESA?.Nombre) && !string.IsNullOrEmpty(s.PAIS?.Nombre)));
    gso.ConsultarSociedadConFiltro(new FiltroSOCIEDAD { Nombre = "filial" }, "CodigoEsif asc", 1, 20, out int t1);
    Comprobar("filtro Nombre: contiene el texto, sin distinguir mayúsculas", t1 == 2);
    var f2 = gso.ConsultarSociedadConFiltro(new FiltroSOCIEDAD { ListaIdTipoEmpresa = [2] }, "CodigoEsif asc", 1, 20, out int t2);
    Comprobar("filtro Tipo de Empresa", t2 == 3 && f2.All(s => s.IdTipoDeEmpresa == 2));
    var f3 = gso.ConsultarSociedadConFiltro(new FiltroSOCIEDAD { ListaIdPais = [1] }, "CodigoEsif asc", 1, 20, out int t3);
    Comprobar("filtro País", t3 == 2 && f3.All(s => s.IdPais == 1));
    gso.ConsultarSociedadConFiltro(new FiltroSOCIEDAD { Nombre = "Sociedad", ListaIdTipoEmpresa = [2], ListaIdPais = [4] }, "CodigoEsif asc", 1, 20, out int t4);
    Comprobar("los filtros se combinan con Y", t4 == 1);
    var porPais = gso.ConsultarSociedadConFiltro(null, "PAIS.Nombre asc", 1, 20, out _);
    Comprobar("orden por la columna País", porPais[0].PAIS!.Nombre == "Argentina" && porPais[5].PAIS!.Nombre == "España");
    var porTipo = gso.ConsultarSociedadConFiltro(null, "TIPO_DE_EMPRESA.Nombre desc", 1, 20, out _);
    Comprobar("orden por la columna Tipo de Empresa", porTipo[0].TIPO_DE_EMPRESA!.Nombre == "Auditora" && porTipo[5].TIPO_DE_EMPRESA!.Nombre == "Ambas");
    var pag2 = gso.ConsultarSociedadConFiltro(null, "CodigoEsif asc", 2, 4, out _);
    Comprobar("paginación: página 2 de 4 filas con las 2 restantes", pag2.Count == 2 && pag2[0].CodigoEsif == "E005");
    Comprobar("campo de orden no permitido se rechaza", Captura<ArgumentException>(() => gso.ConsultarSociedadConFiltro(null, "Borrado asc", 1, 20, out _)) != null);
    Comprobar("desplegable Tipo de Empresa: los tres tipos por orden alfabético", gdm.ConsultarTiposDeEmpresa().Select(t => t.Nombre).SequenceEqual(["Ambas", "Auditada", "Auditora"]));
    Comprobar("desplegable País: por orden alfabético", gdm.ConsultarPaises().Select(p => p.Nombre).SequenceEqual(["Argentina", "Brasil", "Chile", "España"]));

    var (gsoAuditor, _, _) = CrearSoc((int)PerfilDeUsuario.Auditor);
    var soloAsignadas = gsoAuditor.ConsultarSociedadConFiltro(null, "CodigoEsif asc", 1, 20, out int tAud);
    Comprobar("perfil Auditor: solo ve las sociedades que tiene asignadas", tAud == 1 && soloAsignadas[0].IdSociedad == 3);
    var (gsoExterno, _, _) = CrearSoc((int)PerfilDeUsuario.AuditorExterno);
    gsoExterno.ConsultarSociedadConFiltro(null, "CodigoEsif asc", 1, 20, out int tExt);
    Comprobar("resto de perfiles (p. ej. Auditor Externo): ven todas las sociedades", tExt == 6);
}

Console.WriteLine();
Console.WriteLine("Sociedades: alta (GestorSociedades.CrearSociedad)");
{
    var (gso, _, almacen) = CrearSoc();
    int id = gso.CrearSociedad(new SOCIEDAD { CodigoEsif = "N001", Nombre = "Sociedad nueva", IdTipoDeEmpresa = 3, IdPais = 4 });
    SOCIEDAD creada = gso.ConsultarSociedad(id);
    Comprobar("crea la sociedad con sus datos", creada.CodigoEsif == "N001" && creada.Nombre == "Sociedad nueva" && creada.IdTipoDeEmpresa == 3 && creada.IdPais == 4 && !creada.Borrado);
    string esperado = "Se ha creado la Sociedad: Sociedad nueva" + NL +
                      "\n *[Campo CodigoEsif:]* Valor nuevo: N001 " + NL +
                      "\n *[Campo Nombre:]* Valor nuevo: Sociedad nueva " + NL +
                      "\n *[Campo País:]* Valor antiguo:  - Valor nuevo: Chile " + NL +
                      "\n *[Campo Tipo de Empresa:]* Valor antiguo:  - Valor nuevo: Ambas ";
    Comprobar("texto de auditoría del alta igual al formato del original", almacen.Logs.Count == 1 && almacen.Logs[0].Descripcion == esperado);
    Comprobar("la auditoría apunta a la sociedad", almacen.Logs[0].IdSociedad == id && almacen.Logs[0].IdProceso == null && almacen.Logs[0].IdTipoDeAccion == 1 && almacen.Logs[0].IdUsuario == 1);
    Comprobar("[DM-07] rechaza un código eSif que ya existe: 'Ya existe otra sociedad con este mismo código eSif.'",
        Captura<ApplicationException>(() => gso.CrearSociedad(new SOCIEDAD { CodigoEsif = "E001", Nombre = "Repetida", IdTipoDeEmpresa = 1, IdPais = 1 }))?.Message == "Ya existe otra sociedad con este mismo código eSif.");
    Comprobar("[DM-07] el rechazo no distingue mayúsculas", Captura<ApplicationException>(() => gso.CrearSociedad(new SOCIEDAD { CodigoEsif = "e002", Nombre = "Repetida", IdTipoDeEmpresa = 1, IdPais = 1 })) != null);
    Comprobar("el código eSif de una sociedad borrada se puede reutilizar", Captura<Exception>(() => gso.CrearSociedad(new SOCIEDAD { CodigoEsif = "E006", Nombre = "Reutiliza código", IdTipoDeEmpresa = 1, IdPais = 1 })) == null);
    Comprobar("sí admite dos sociedades con el mismo nombre", Captura<Exception>(() => gso.CrearSociedad(new SOCIEDAD { CodigoEsif = "N002", Nombre = "Sociedad nueva", IdTipoDeEmpresa = 1, IdPais = 1 })) == null);
}

Console.WriteLine();
Console.WriteLine("Sociedades: edición (GestorSociedades.ActualizarSociedad)");
{
    var (gso, _, almacen) = CrearSoc();
    SOCIEDAD s = gso.ConsultarSociedad(5);
    s.Nombre = "Filial Cinco (modificada)";
    s.IdPais = 3;
    gso.ActualizarSociedad(s);
    SOCIEDAD leida = gso.ConsultarSociedad(5);
    Comprobar("guarda nombre y país y conserva el resto", leida.Nombre == "Filial Cinco (modificada)" && leida.IdPais == 3 && leida.CodigoEsif == "E005" && leida.IdTipoDeEmpresa == 3);
    string esperado = "Se ha modificado  la Sociedad con el nombre: Filial Cinco (ejemplo)" + NL +
                      "\n *[Campo Nombre:]* Valor antiguo: Filial Cinco (ejemplo) - Valor nuevo: Filial Cinco (modificada) " + NL +
                      "\n *[Campo País:]* Valor antiguo: España - Valor nuevo: Argentina ";
    Comprobar("auditoría: solo los campos cambiados, con valor antiguo y nuevo", almacen.Logs.Count == 1 && almacen.Logs[0].IdTipoDeAccion == 3 && almacen.Logs[0].Descripcion == esperado && almacen.Logs[0].IdSociedad == 5);
    s.CodigoEsif = "E001";
    Comprobar("[DM-07] rechaza cambiar a un código eSif que ya usa otra sociedad", Captura<ApplicationException>(() => gso.ActualizarSociedad(s))?.Message == "Ya existe otra sociedad con este mismo código eSif.");
    s.CodigoEsif = "E005";
    Comprobar("permite guardar manteniendo su propio código eSif", Captura<Exception>(() => gso.ActualizarSociedad(s)) == null);
    Comprobar("sociedad inexistente: 'No existe la sociedad'", Captura<ApplicationException>(() => gso.ConsultarSociedad(9999))?.Message == "No existe la sociedad");
    Comprobar("sociedad borrada: 'No existe la sociedad'", Captura<ApplicationException>(() => gso.ConsultarSociedad(6))?.Message == "No existe la sociedad");
    Comprobar("actualizar una sociedad inexistente: 'No existe la sociedad'", Captura<ApplicationException>(() => gso.ActualizarSociedad(new SOCIEDAD { IdSociedad = 9999, CodigoEsif = "ZZZZ", Nombre = "x", IdTipoDeEmpresa = 1, IdPais = 1 }))?.Message == "No existe la sociedad");
}

Console.WriteLine();
Console.WriteLine("Sociedades: baja (GestorSociedades.EliminarSociedad)");
{
    var (gso, _, almacen) = CrearSoc();
    Comprobar("[DM-08] no elimina una sociedad con procesos asignados: 'La sociedad tiene procesos asociados.'",
        Captura<ApplicationException>(() => gso.EliminarSociedad(1))?.Message == "La sociedad tiene procesos asociados.");
    Comprobar("[DM-08] tampoco si su único proceso asignado está borrado (como el original)",
        Captura<ApplicationException>(() => gso.EliminarSociedad(4))?.Message == "La sociedad tiene procesos asociados.");
    Comprobar("[DM-08] no elimina la sociedad de un usuario: 'La sociedad tiene usuarios asociados'",
        Captura<ApplicationException>(() => gso.EliminarSociedad(2))?.Message == "La sociedad tiene usuarios asociados");
    Comprobar("[DM-08] tampoco si ese usuario está dado de baja (como el original)",
        Captura<ApplicationException>(() => gso.EliminarSociedad(7))?.Message == "La sociedad tiene usuarios asociados");
    Comprobar("[DM-08] no elimina una sociedad asignada a un usuario",
        Captura<ApplicationException>(() => gso.EliminarSociedad(3))?.Message == "La sociedad tiene usuarios asociados");
    Comprobar("[DM-08] los rechazos no borran ni dejan auditoría", almacen.Sociedades.Count(x => x.Borrado) == 1 && almacen.Logs.Count == 0);
    Comprobar("sí elimina una sociedad sin uso", Captura<Exception>(() => gso.EliminarSociedad(5)) == null);
    Comprobar("baja lógica: el registro sigue en la tabla, marcado como borrado", almacen.Sociedades.Single(x => x.IdSociedad == 5).Borrado);
    Comprobar("auditoría de la baja", almacen.Logs.Count == 1 && almacen.Logs[0].IdTipoDeAccion == 2
        && almacen.Logs[0].Descripcion == "Se ha eliminado  la Sociedad con el nombre: Filial Cinco (ejemplo)" && almacen.Logs[0].IdSociedad == 5);
    gso.ConsultarSociedadConFiltro(null, "CodigoEsif asc", 1, 20, out int total);
    Comprobar("el listado deja de mostrarla", total == 5);
    Comprobar("eliminar dos veces: 'No existe la sociedad'", Captura<ApplicationException>(() => gso.EliminarSociedad(5))?.Message == "No existe la sociedad");
}

Console.WriteLine();
Console.WriteLine("Sociedades: permisos por perfil (GestorSociedades.AccionesPermitidas)");
{
    var (gsoAdmin, _, _) = CrearSoc();
    Comprobar("Administrador: consultar, crear, actualizar y eliminar", gsoAdmin.AccionesPermitidas().Count == 4);
    foreach (PerfilDeUsuario perfil in Enum.GetValues<PerfilDeUsuario>().Where(x => x != PerfilDeUsuario.Administrador))
    {
        var (gso, _, _) = CrearSoc((int)perfil);
        bool soloConsulta = gso.AccionesPermitidas().SequenceEqual([GestorSociedades.AccionSociedades.ConsultarSociedad]);
        SOCIEDAD s = gso.ConsultarSociedad(5);
        string sinPermiso = "El usuario demo.admin no tiene permisos para realizar esta acción.";
        bool sinAlta = Captura<SecurityException>(() => gso.CrearSociedad(new SOCIEDAD { CodigoEsif = "X", Nombre = "X", IdTipoDeEmpresa = 1, IdPais = 1 }))?.Message == sinPermiso;
        bool sinEdicion = Captura<SecurityException>(() => gso.ActualizarSociedad(s))?.Message == sinPermiso;
        bool sinBaja = Captura<SecurityException>(() => gso.EliminarSociedad(5))?.Message == sinPermiso;
        Comprobar($"{perfil}: solo consulta; alta, edición y baja rechazadas", soloConsulta && sinAlta && sinEdicion && sinBaja);
    }
}

// ---------------------------------------------------------------------------------------------
// Tipos de Auditoría (GestorTiposDeAuditoria). Datos de ejemplo: 6 tipos, el 5 borrado.
//   1: con procesos asignados · 2: tipo del usuario de la demo · 3: solo un proceso asignado borrado
//   4: sin uso · 6: tipo de un usuario dado de baja
// ---------------------------------------------------------------------------------------------
(GestorTiposDeAuditoria gta, AlmacenMemoria almacen) CrearTip(int? perfil = null)
{
    AlmacenMemoria almacen = AlmacenMemoria.DeEjemplo();
    if (perfil != null) almacen.Usuarios[0].IdPerfilDeUsuario = perfil.Value;
    var gu = new GestorUsuarios(new RepositorioUsuariosMemoria(almacen), new UsuarioFijo(DatosDeEjemplo.UidUsuarioDemo));
    var gl = new GestorLogDeEntidades(new RepositorioLogDeEntidadesMemoria(almacen), gu);
    return (new GestorTiposDeAuditoria(new RepositorioTiposDeAuditoriaMemoria(almacen), gu, gl), almacen);
}

Console.WriteLine();
Console.WriteLine("Tipos de Auditoría: listado (GestorTiposDeAuditoria.ConsultarTipoAuditoriaConFiltro)");
{
    var (gta, _) = CrearTip();
    var p1 = gta.ConsultarTipoAuditoriaConFiltro(null, "Codigo asc", 1, 20, out int total);
    Comprobar("no incluye los tipos borrados (5 de 6)", total == 5 && p1.All(t => t.IdTipoDeAuditoria != 5));
    Comprobar("orden inicial por Código ascendente", p1[0].Codigo == "E1" && p1[4].Codigo == "E6");
    gta.ConsultarTipoAuditoriaConFiltro(new FiltroTIPO_DE_AUDITORIA { Codigo = "e3" }, "Codigo asc", 1, 20, out int t1);
    Comprobar("filtro Código: contiene el texto, sin distinguir mayúsculas", t1 == 1);
    gta.ConsultarTipoAuditoriaConFiltro(new FiltroTIPO_DE_AUDITORIA { Descripcion = "revisión" }, "Codigo asc", 1, 20, out int t2);
    Comprobar("filtro Descripción: contiene el texto", t2 == 2);
    gta.ConsultarTipoAuditoriaConFiltro(new FiltroTIPO_DE_AUDITORIA { Codigo = "E1", Descripcion = "revisión" }, "Codigo asc", 1, 20, out int t3);
    Comprobar("los filtros se combinan con Y", t3 == 0);
    var porDesc = gta.ConsultarTipoAuditoriaConFiltro(null, "Descripcion desc", 1, 20, out _);
    Comprobar("orden por la columna Descripción", porDesc[0].Codigo == "E4" && porDesc[4].Codigo == "E1");
    var pag2 = gta.ConsultarTipoAuditoriaConFiltro(null, "Codigo asc", 2, 3, out _);
    Comprobar("paginación: página 2 de 3 filas con las 2 restantes", pag2.Count == 2 && pag2[0].Codigo == "E4");
    Comprobar("campo de orden no permitido se rechaza", Captura<ArgumentException>(() => gta.ConsultarTipoAuditoriaConFiltro(null, "Borrado asc", 1, 20, out _)) != null);
}

Console.WriteLine();
Console.WriteLine("Tipos de Auditoría: alta (GestorTiposDeAuditoria.CrearTipoDeAuditoria)");
{
    var (gta, almacen) = CrearTip();
    int id = gta.CrearTipoDeAuditoria(new TIPO_DE_AUDITORIA { Codigo = "N1", Descripcion = "Tipo nuevo" });
    TIPO_DE_AUDITORIA creado = gta.ConsultarTipoDeAuditoria(id);
    Comprobar("crea el tipo con sus datos", creado.Codigo == "N1" && creado.Descripcion == "Tipo nuevo" && !creado.Borrado);
    string esperado = "Se ha creado el Tipo de Auditoría con el codigo: N1" + NL +
                      "\n *[Campo Código:]* Valor nuevo: N1 " + NL +
                      "\n *[Campo Descripción:]* Valor nuevo: Tipo nuevo ";
    Comprobar("texto de auditoría del alta igual al formato del original", almacen.Logs.Count == 1 && almacen.Logs[0].Descripcion == esperado && almacen.Logs[0].IdTipoDeAccion == 1);
    Comprobar("la línea de auditoría no queda relacionada con ninguna entidad (como el original)",
        almacen.Logs[0].IdSociedad == null && almacen.Logs[0].IdProceso == null && almacen.Logs[0].IdSubproceso == null);
    Comprobar("[DM-11] rechaza un código que ya existe: 'Ya existe otro tipo de auditoría con este mismo código'",
        Captura<ApplicationException>(() => gta.CrearTipoDeAuditoria(new TIPO_DE_AUDITORIA { Codigo = "E1", Descripcion = "Repetido" }))?.Message == "Ya existe otro tipo de auditoría con este mismo código");
    Comprobar("[DM-11] el rechazo no distingue mayúsculas", Captura<ApplicationException>(() => gta.CrearTipoDeAuditoria(new TIPO_DE_AUDITORIA { Codigo = "e2", Descripcion = "Repetido" })) != null);
    Comprobar("el código de un tipo borrado se puede reutilizar", Captura<Exception>(() => gta.CrearTipoDeAuditoria(new TIPO_DE_AUDITORIA { Codigo = "E5", Descripcion = "Reutiliza código" })) == null);
}

Console.WriteLine();
Console.WriteLine("Tipos de Auditoría: edición (GestorTiposDeAuditoria.ActualizarTipoDeAuditoria)");
{
    var (gta, almacen) = CrearTip();
    TIPO_DE_AUDITORIA t = gta.ConsultarTipoDeAuditoria(4);
    t.Descripcion = "Revisión interna (modificada)";
    gta.ActualizarTipoDeAuditoria(t);
    TIPO_DE_AUDITORIA leido = gta.ConsultarTipoDeAuditoria(4);
    Comprobar("guarda la descripción y conserva el código", leido.Descripcion == "Revisión interna (modificada)" && leido.Codigo == "E4");
    string esperado = "Se ha modificado el Tipo de Auditoría con el codigo: E4" + NL +
                      "\n *[Campo Descripción:]* Valor antiguo: Revisión interna (ejemplo) - Valor nuevo: Revisión interna (modificada) ";
    Comprobar("auditoría: solo los campos cambiados, con valor antiguo y nuevo", almacen.Logs.Count == 1 && almacen.Logs[0].IdTipoDeAccion == 3 && almacen.Logs[0].Descripcion == esperado);
    t.Codigo = "E1";
    Comprobar("[DM-11] rechaza cambiar a un código que ya usa otro tipo", Captura<ApplicationException>(() => gta.ActualizarTipoDeAuditoria(t))?.Message == "Ya existe otro tipo de auditoría con este mismo código");
    t.Codigo = "E4";
    Comprobar("permite guardar manteniendo su propio código", Captura<Exception>(() => gta.ActualizarTipoDeAuditoria(t)) == null);
    Comprobar("consultar un tipo inexistente: 'No existe el tipo de auditoría'", Captura<ApplicationException>(() => gta.ConsultarTipoDeAuditoria(9999))?.Message == "No existe el tipo de auditoría");
    Comprobar("actualizar un tipo inexistente: 'No existe el tipo de auditoria' (sin tilde, como el original)",
        Captura<ApplicationException>(() => gta.ActualizarTipoDeAuditoria(new TIPO_DE_AUDITORIA { IdTipoDeAuditoria = 9999, Codigo = "ZZ", Descripcion = "x" }))?.Message == "No existe el tipo de auditoria");
}

Console.WriteLine();
Console.WriteLine("Tipos de Auditoría: baja (GestorTiposDeAuditoria.EliminarTipoDeAuditoria)");
{
    var (gta, almacen) = CrearTip();
    Comprobar("[DM-11] no elimina un tipo con procesos asignados: 'El tipo de auditorías tiene procesos asociados'",
        Captura<ApplicationException>(() => gta.EliminarTipoDeAuditoria(1))?.Message == "El tipo de auditorías tiene procesos asociados");
    Comprobar("[DM-11] tampoco si su único proceso asignado está borrado (como el original)",
        Captura<ApplicationException>(() => gta.EliminarTipoDeAuditoria(3))?.Message == "El tipo de auditorías tiene procesos asociados");
    Comprobar("[DM-11] no elimina el tipo de un usuario: 'El tipo de auditorías tiene usuarios asociados'",
        Captura<ApplicationException>(() => gta.EliminarTipoDeAuditoria(2))?.Message == "El tipo de auditorías tiene usuarios asociados");
    Comprobar("[DM-11] tampoco si ese usuario está dado de baja (como el original)",
        Captura<ApplicationException>(() => gta.EliminarTipoDeAuditoria(6))?.Message == "El tipo de auditorías tiene usuarios asociados");
    Comprobar("[DM-11] los rechazos no borran ni dejan auditoría", almacen.TiposDeAuditoria.Count(x => x.Borrado) == 1 && almacen.Logs.Count == 0);
    Comprobar("sí elimina un tipo sin uso", Captura<Exception>(() => gta.EliminarTipoDeAuditoria(4)) == null);
    Comprobar("baja lógica: el registro sigue en la tabla, marcado como borrado", almacen.TiposDeAuditoria.Single(x => x.IdTipoDeAuditoria == 4).Borrado);
    Comprobar("auditoría de la baja", almacen.Logs.Count == 1 && almacen.Logs[0].IdTipoDeAccion == 2
        && almacen.Logs[0].Descripcion == "Se ha eliminado el Tipo de Auditoría con el codigo: E4");
    gta.ConsultarTipoAuditoriaConFiltro(null, "Codigo asc", 1, 20, out int total);
    Comprobar("el listado deja de mostrarlo", total == 4);
    Comprobar("eliminar dos veces: 'No existe el tipo de auditoría'", Captura<ApplicationException>(() => gta.EliminarTipoDeAuditoria(4))?.Message == "No existe el tipo de auditoría");
}

Console.WriteLine();
Console.WriteLine("Tipos de Auditoría: permisos por perfil (GestorTiposDeAuditoria.AccionesPermitidas)");
{
    string sinPermiso = "El usuario demo.admin no tiene permisos para realizar esta acción.";
    var (gtaAdmin, _) = CrearTip();
    Comprobar("Administrador: consultar, crear, actualizar y eliminar", gtaAdmin.AccionesPermitidas().Count == 4);
    foreach (PerfilDeUsuario perfil in new[] { PerfilDeUsuario.Auditor, PerfilDeUsuario.ResponsableDeAuditoria, PerfilDeUsuario.ResponsableEmpresa, PerfilDeUsuario.AdministradorPais })
    {
        var (gta, _) = CrearTip((int)perfil);
        bool soloConsulta = gta.AccionesPermitidas().SequenceEqual([GestorTiposDeAuditoria.AccionTiposDeAuditoria.ConsultarTipoDeAuditoria]);
        TIPO_DE_AUDITORIA t = gta.ConsultarTipoDeAuditoria(4);
        bool sinAlta = Captura<SecurityException>(() => gta.CrearTipoDeAuditoria(new TIPO_DE_AUDITORIA { Codigo = "XX", Descripcion = "X" }))?.Message == sinPermiso;
        bool sinEdicion = Captura<SecurityException>(() => gta.ActualizarTipoDeAuditoria(t))?.Message == sinPermiso;
        bool sinBaja = Captura<SecurityException>(() => gta.EliminarTipoDeAuditoria(4))?.Message == sinPermiso;
        Comprobar($"{perfil}: solo consulta; alta, edición y baja rechazadas", soloConsulta && sinAlta && sinEdicion && sinBaja);
    }
    foreach (PerfilDeUsuario perfil in new[] { PerfilDeUsuario.AuditorExterno, PerfilDeUsuario.ResponsableControl, PerfilDeUsuario.ResponsableSeguimientoArea })
    {
        var (gta, _) = CrearTip((int)perfil);
        bool nada = gta.AccionesPermitidas().Count == 0;
        bool sinListado = Captura<SecurityException>(() => gta.ConsultarTipoAuditoriaConFiltro(null, "Codigo asc", 1, 20, out _))?.Message == sinPermiso;
        bool sinFicha = Captura<SecurityException>(() => gta.ConsultarTipoDeAuditoria(4))?.Message == sinPermiso;
        Comprobar($"{perfil}: ni siquiera puede consultar", nada && sinListado && sinFicha);
    }
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
    int conAsociado = excel.ProcesosAsociados.First().IdProceso;
    Comprobar("[DM-03] un proceso con procesos asociados no se puede eliminar", Captura<ApplicationException>(() => gp.EliminarProceso(conAsociado))?.Message == "El Proceso tiene procesos asociados");
    int nuevo = gp.CrearProceso(new PROCESO { Codigo = "ZZ-PRB", Nombre = "Prueba de carga", IdTipoDeProceso = excel.TiposDeProceso[0].IdTipoDeProceso });
    Comprobar("alta sobre los datos del libro y numeración del log a continuación del origen", excel.Logs.Count == 2 && excel.Logs[0].IdLogDeEntidades == excel.LogsEnOrigen + 1);
    gp.EliminarProceso(nuevo);
    Comprobar("baja del proceso recién creado", excel.Procesos.Single(p => p.IdProceso == nuevo).Borrado);

    var gsExcel = new GestorSubprocesos(rs, rp, gu, gl);
    var subs = gsExcel.ConsultarSubprocesoConFiltro(null, "Codigo asc", 1, 1000, out int totalSubs);
    Comprobar("lee los subprocesos que no son 'Otros' ni están borrados", totalSubs == excel.Subprocesos.Count(x => !x.Borrado && !x.TipoOtros) && totalSubs > 0);
    int? enUso = excel.ProcesosAsociados.Where(pa => !pa.Borrado && pa.IdSubproceso != null).Select(pa => pa.IdSubproceso).FirstOrDefault();
    if (enUso != null)
        Comprobar("[DM-06] un subproceso usado en un proceso asignado del libro no se puede eliminar",
            Captura<ApplicationException>(() => gsExcel.EliminarSubproceso(enUso.Value))?.Message == "No se puede eliminar el Subproceso porque esta asociado a Procesos Asignados.");
}

Console.WriteLine();
Console.WriteLine("Casos del Plan de Pruebas (PRO 1.15) con comprobación automática");
foreach (var (caso, cuenta) in porCaso)
    Console.WriteLine($"  {(cuenta.Fallidas == 0 ? "OK   " : "FALLO")} {caso}: {cuenta.Correctas + cuenta.Fallidas} comprobaciones");
Console.WriteLine($"  {porCaso.Count} casos cubiertos; {correctas + fallidas - porCaso.Values.Sum(c => c.Correctas + c.Fallidas)} comprobaciones sin caso en el plan");

Console.WriteLine();
Console.WriteLine($"Resultado: {correctas} correctas, {fallidas} fallidas");
return fallidas == 0 ? 0 : 1;
