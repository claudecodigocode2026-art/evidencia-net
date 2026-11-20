/*
  eVidencia · comportamiento común de la interfaz (sin dependencias externas).
  Diseño según el mockup "eVidencia · Mockup responsive": menú por módulos, diálogos, avisos y listados.
  EV.listado es un componente reutilizable por cualquier pantalla de listado.
*/
(function () {
  'use strict';
  var EV = (window.EV = window.EV || {});

  function el(tag, attrs, hijos) {
    var e = document.createElement(tag);
    Object.keys(attrs || {}).forEach(function (k) {
      if (k === 'text') e.textContent = attrs[k];
      else if (k === 'html') e.innerHTML = attrs[k];
      else if (k.indexOf('on') === 0) e.addEventListener(k.substring(2), attrs[k]);
      else e.setAttribute(k, attrs[k]);
    });
    (hijos || []).forEach(function (h) { if (h) e.appendChild(h); });
    return e;
  }
  function icono(d, extra) {
    return '<svg class="ev-i' + (extra ? ' ' + extra : '') + '" viewBox="0 0 24 24" aria-hidden="true"><path d="' + d + '"></path></svg>';
  }
  var app = function () { return document.querySelector('.ev-app') || document.body; };

  // Los servicios devuelven los textos codificados en HTML (igual que el servicio original): se decodifican a texto.
  var _dec = document.createElement('textarea');
  EV.texto = function (v) { if (v == null) return ''; _dec.innerHTML = String(v); return _dec.value; };

  /* ---------- Diálogos (mockup: .ev-scrim / .ev-dialog) ---------- */
  function dialogo(titulo, mensaje, lista, botones) {
    cerrarDialogo();
    var cuerpo = el('div', { class: 'ev-dialog__body' }, [el('p', { text: mensaje })]);
    if (lista && lista.length) cuerpo.appendChild(el('ul', {}, lista.map(function (t) { return el('li', { text: t }); })));
    var dlg = el('div', { class: 'ev-dialog', role: 'dialog', 'aria-modal': 'true', 'aria-labelledby': 'ev-dlg-t', onclick: function (e) { e.stopPropagation(); } }, [
      el('div', { class: 'ev-dialog__head' }, [
        el('h2', { class: 'ev-dialog__title', id: 'ev-dlg-t', text: titulo }),
        el('button', { type: 'button', class: 'ev-btn ev-btn--ghost ev-btn--icon', 'aria-label': 'Cerrar', html: icono('M6 6l12 12M18 6 6 18'), onclick: cerrarDialogo })
      ]),
      cuerpo,
      el('div', { class: 'ev-actions' }, botones.map(function (b) {
        return el('button', { type: 'button', class: 'ev-btn ' + b.cls, text: b.l, onclick: function () { cerrarDialogo(); if (b.go) b.go(); } });
      }))
    ]);
    var scrim = el('div', { class: 'ev-scrim', id: 'ev-scrim', onclick: cerrarDialogo }, [dlg]);
    app().appendChild(scrim);
    var primero = dlg.querySelector('.ev-actions .ev-btn');
    if (primero) primero.focus();
  }
  function cerrarDialogo() { var s = document.getElementById('ev-scrim'); if (s) s.remove(); }
  document.addEventListener('keydown', function (e) { if (e.key === 'Escape') cerrarDialogo(); });

  EV.alerta = function (mensaje, lista, titulo) {
    dialogo(titulo || 'Error', mensaje, lista || [], [{ l: 'Aceptar', cls: 'ev-btn--primary' }]);
  };
  EV.errores = function (lista) {
    EV.alerta('Hay errores en el formulario, por favor, revise los siguientes campos:', lista);
  };
  EV.confirmar = function (mensaje, alAceptar) {
    dialogo('Se necesita confirmación', mensaje, [], [
      { l: 'Sí', cls: 'ev-btn--primary', go: alAceptar },
      { l: 'No', cls: 'ev-btn--secondary' }
    ]);
  };

  /* ---------- Aviso (mockup: .ev-toast) ---------- */
  var _t;
  EV.notificar = function (mensaje) {
    var previo = document.getElementById('ev-toast'); if (previo) previo.remove();
    var cuerpo = el('div', {}, [el('b', { text: 'Notificación' })]);
    cuerpo.appendChild(document.createTextNode(mensaje));
    app().appendChild(el('div', { class: 'ev-toast', id: 'ev-toast', role: 'status' }, [
      el('span', { class: 'ev-toast__icon', html: icono('m5 12 5 5L20 7') }), cuerpo
    ]));
    clearTimeout(_t);
    _t = setTimeout(function () { var t = document.getElementById('ev-toast'); if (t) t.remove(); }, 3500);
  };

  /* ---------- Estructura: menú lateral y pestañas de módulo ---------- */
  document.addEventListener('DOMContentLoaded', function () {
    var shell = document.querySelector('.ev-shell');
    if (!shell) return;
    var abrir = shell.querySelector('.ev-menu-btn');
    function menu(abierto) {
      shell.classList.toggle('is-nav-open', abierto);
      if (abrir) abrir.setAttribute('aria-expanded', abierto ? 'true' : 'false');
      var s = shell.querySelector('.ev-nav-scrim');
      if (abierto && !s) shell.appendChild(el('button', { type: 'button', class: 'ev-nav-scrim', 'aria-label': 'Cerrar menú', onclick: function () { menu(false); } }));
      if (!abierto && s) s.remove();
    }
    if (abrir) abrir.addEventListener('click', function () { menu(true); });
    var cerrar = shell.querySelector('.ev-nav__close');
    if (cerrar) cerrar.addEventListener('click', function () { menu(false); });

    // Pestañas superiores: muestran en el lateral la sección del módulo elegido
    shell.querySelectorAll('.ev-tab').forEach(function (tab) {
      tab.addEventListener('click', function () {
        shell.querySelectorAll('.ev-tab').forEach(function (t) {
          var on = t === tab; t.classList.toggle('is-on', on); t.setAttribute('aria-current', on ? 'page' : 'false');
        });
        shell.querySelectorAll('.ev-sec').forEach(function (sec) {
          sec.classList.toggle('is-active', sec.getAttribute('data-sec') === tab.getAttribute('data-sec'));
        });
      });
    });
    // Secciones plegables (vista móvil)
    shell.querySelectorAll('.ev-sec__head').forEach(function (h) {
      h.addEventListener('click', function () {
        var cerrada = h.parentElement.classList.toggle('is-closed');
        h.setAttribute('aria-expanded', cerrada ? 'false' : 'true');
      });
    });
  });

  /* ---------- Listado con filtros, orden, paginación y selección ----------
     opciones: {
       id            identificador de la pantalla (para recordar filtros y página al volver de la ficha)
       url           servicio que devuelve { page, total, records, rows }
       tabla, paginador, titulo   selectores de los elementos de la página
       columnas      [{ campo, titulo, orden }]  (orden = nombre del campo de ordenación en el servidor)
       clave         campo identificador de la fila
       ordenInicial  { campo, dir }
       filtros       { nombreEnElFiltro: selectorDelInput }
       alEnviarFiltro (opcional) función que recibe los filtros leídos y devuelve el objeto que se envía al servicio
       esVolver      true si se llega desde la propia pantalla o su ficha
       alAbrir(fila) acción al pulsar "i" o hacer doble clic
     } */
  EV.listado = function (o) {
    var tabla = document.querySelector(o.tabla), pag = document.querySelector(o.paginador);
    var titulo = o.titulo ? document.querySelector(o.titulo) : null;
    var clave = 'ev_listado_' + o.id;
    var st = { page: 1, rows: 10, sidx: o.ordenInicial.campo, sord: o.ordenInicial.dir, filtros: {}, sel: null, filas: [], total: 0, records: 0 };

    var guardado = null;
    try { guardado = JSON.parse(sessionStorage.getItem(clave)); } catch (e) { guardado = null; }
    if (o.esVolver && guardado) {
      st.page = guardado.page; st.rows = guardado.rows; st.sidx = guardado.sidx; st.sord = guardado.sord;
      Object.keys(o.filtros).forEach(function (k) { var i = document.querySelector(o.filtros[k]); if (i) i.value = guardado.filtros[k] || ''; });
    } else {
      try { sessionStorage.removeItem(clave); } catch (e) { /* sin almacenamiento */ }
    }

    function leerFiltros() {
      var f = {};
      Object.keys(o.filtros).forEach(function (k) { var v = document.querySelector(o.filtros[k]).value; f[k] = v === '' ? null : v; });
      return f;
    }
    st.filtros = leerFiltros();

    var thead = el('thead'), tbody = el('tbody');
    tabla.appendChild(thead); tabla.appendChild(tbody);

    function pintarCabecera() {
      var tr = el('tr', {}, [el('th', {}, [el('span', { class: 'ev-sr', text: 'Abrir' })])]);
      o.columnas.forEach(function (c) {
        var marca = st.sidx === c.orden ? (st.sord === 'asc' ? ' ▲' : ' ▼') : '';
        tr.appendChild(el('th', {}, [el('button', { type: 'button', class: 'ev-th-btn', text: c.titulo + marca, onclick: function () {
          st.sord = (st.sidx === c.orden && st.sord === 'asc') ? 'desc' : 'asc'; st.sidx = c.orden; cargar();
        } })]));
      });
      thead.innerHTML = ''; thead.appendChild(tr);
    }

    function pintarFilas() {
      tbody.innerHTML = '';
      if (!st.filas.length) {
        tbody.appendChild(el('tr', {}, [el('td', { colspan: String(o.columnas.length + 1), class: 'ev-empty', text: 'No hay registros que mostrar' })]));
      }
      st.filas.forEach(function (f) {
        var tr = el('tr', { class: 'ev-row' + (st.sel && st.sel[o.clave] === f[o.clave] ? ' is-selected' : '') });
        tr.appendChild(el('td', { class: 'ev-td-open' }, [el('button', { type: 'button', class: 'ev-btn ev-btn--ghost ev-btn--icon ev-btn--sm', 'aria-label': 'Abrir', title: 'Abrir',
          html: icono('M12 21a9 9 0 1 0 0-18 9 9 0 0 0 0 18zM12 11v6M12 7.5v.5'), onclick: function (e) { e.stopPropagation(); o.alAbrir(f); } })]));
        o.columnas.forEach(function (c, i) {
          var v = f[c.campo];
          tr.appendChild(el('td', { class: (i === 0 ? 'is-first' : '') + (v === '' ? ' is-empty' : ''), 'data-label': c.titulo, title: v, text: v }));
        });
        tr.addEventListener('click', function () { st.sel = f; pintarFilas(); });
        tr.addEventListener('dblclick', function () { o.alAbrir(f); });
        tbody.appendChild(tr);
      });
    }

    function boton(etiqueta, d, inactivo, accion) {
      var b = el('button', { type: 'button', class: 'ev-btn ev-btn--ghost ev-btn--icon ev-btn--sm', 'aria-label': etiqueta, title: etiqueta, html: icono(d), onclick: accion });
      if (inactivo) b.disabled = true;
      return b;
    }

    function pintarPaginador() {
      var desde = st.records === 0 ? 0 : (st.page - 1) * st.rows + 1;
      var hasta = Math.min(st.page * st.rows, st.records);
      var paginas = Math.max(st.total, 1);
      var sel = el('select', { class: 'ev-select', 'aria-label': 'Filas por página' }, [10, 20, 50].map(function (n) {
        var op = el('option', { value: String(n), text: String(n) }); if (n === st.rows) op.selected = true; return op;
      }));
      sel.addEventListener('change', function () { st.rows = parseInt(sel.value, 10); st.page = 1; cargar(); });
      var etiqueta = el('label', { class: 'ev-inline', style: 'gap:6px' }); etiqueta.appendChild(document.createTextNode('Filas')); etiqueta.appendChild(sel);
      pag.innerHTML = '';
      pag.appendChild(el('span', { text: 'Mostrando ' + desde + ' - ' + hasta + ' de ' + st.records }));
      pag.appendChild(el('div', { class: 'ev-pager__nav' }, [
        boton('Primera página', 'm17 6-6 6 6 6M7 6v12', st.page <= 1, function () { st.page = 1; cargar(); }),
        boton('Página anterior', 'm15 6-6 6 6 6', st.page <= 1, function () { st.page--; cargar(); }),
        el('span', { text: 'Página ' + st.page + ' de ' + paginas }),
        boton('Página siguiente', 'm9 6 6 6-6 6', st.page >= paginas, function () { st.page++; cargar(); }),
        boton('Última página', 'm7 6 6 6-6 6M17 6v12', st.page >= paginas, function () { st.page = paginas; cargar(); })
      ]));
      pag.appendChild(etiqueta);
    }

    function cargar() {
      var datos = new URLSearchParams();
      datos.set('page', st.page); datos.set('rows', st.rows); datos.set('sidx', st.sidx); datos.set('sord', st.sord);
      datos.set('filtro', JSON.stringify(o.alEnviarFiltro ? o.alEnviarFiltro(st.filtros) : st.filtros));
      return fetch(o.url, { method: 'POST', headers: { 'Content-Type': 'application/x-www-form-urlencoded' }, body: datos.toString() })
        .then(function (r) { if (!r.ok) throw new Error('El servicio ha respondido ' + r.status); return r.json(); })
        .then(function (d) {
          st.total = d.total; st.records = d.records;
          // Si la página pedida ya no existe (por ejemplo, tras eliminar), se vuelve a la última
          if (st.page > 1 && d.rows.length === 0 && d.records > 0) { st.page = Math.max(d.total, 1); return cargar(); }
          st.filas = d.rows.map(function (f) { var x = {}; Object.keys(f).forEach(function (k) { x[k] = typeof f[k] === 'string' ? EV.texto(f[k]) : f[k]; }); return x; });
          st.sel = null;
          if (titulo) titulo.textContent = o.nombre + ' ( ' + d.records + ' )';
          pintarCabecera(); pintarFilas(); pintarPaginador();
        })
        .catch(function (e) { EV.alerta('No se ha podido cargar el listado. ' + e.message); });
    }

    cargar();

    return {
      seleccion: function () { return st.sel; },
      filtrar: function () { st.filtros = leerFiltros(); st.page = 1; cargar(); },
      borrarFiltros: function () {
        Object.keys(o.filtros).forEach(function (k) { document.querySelector(o.filtros[k]).value = ''; });
        st.filtros = leerFiltros(); st.page = 1; cargar();
      },
      // Guarda filtros, página y orden antes de ir a la ficha, para recuperarlos al volver
      recordar: function () {
        try { sessionStorage.setItem(clave, JSON.stringify({ page: st.page, rows: st.rows, sidx: st.sidx, sord: st.sord, filtros: leerFiltros() })); } catch (e) { /* sin almacenamiento */ }
      }
    };
  };
})();
