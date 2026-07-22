(function () {
    var contactos = window.contactosIniciales || [];
    var sucursales = window.sucursalesIniciales || [];
    var tabIdxProspecto = 0;
    var tabIdxSuc = 0;
    var tabsProspecto = ['datos', 'direccion', 'fotos'];
    var tabsSuc = ['contacto', 'direccion', 'fotos'];

    // === Utilidades ===
    function escapeHtml(text) {
        if (!text) return '';
        return text.toString()
            .replace(/&/g, "&amp;")
            .replace(/</g, "&lt;")
            .replace(/>/g, "&gt;")
            .replace(/"/g, "&quot;");
    }

    function readFileBase64(input, callback) {
        if (input.files && input.files[0]) {
            var reader = new FileReader();
            reader.onload = function (e) { callback(e.target.result); };
            reader.readAsDataURL(input.files[0]);
        }
    }

    function esModoDetalle() {
        return window.esModoDetalle === true;
    }

    // === Render contactos ===
    function renderContactos() {
        var $cont = $('#listaContactos').empty();
        if (contactos.length === 0) {
            $cont.html('<div style="color:var(--text3);font-size:13px;padding:8px 0;">Sin contactos registrados.</div>');
        } else {
            contactos.forEach(function (c, idx) {
                if (esModoDetalle()) {
                    $cont.append(
                        '<div style="background:white; border-radius:8px; padding:12px 16px; border:1px solid var(--border-light); display:flex; justify-content:space-between; align-items:center; margin-bottom:8px;">' +
                        '<div style="font-weight:700; font-size:14px; color:var(--text1);">' + escapeHtml(c.NombreContacto) + (c.RepresentanteLegal ? ' <span style="background:#EBF5FB;color:#2874A6;font-size:10px;padding:2px 8px;border-radius:10px;font-weight:600;margin-left:6px;">Rep. Legal</span>' : '') + '</div>' +
                        '<button type="button" class="btn-ver-contacto" data-idx="' + idx + '" title="Ver contacto" style="background:none; border:1px solid var(--border-light); border-radius:6px; padding:4px 12px; color:var(--brown-dark); cursor:pointer;"><i class="fas fa-eye"></i></button>' +
                        '</div>'
                    );
                } else {
                    $cont.append(
                        '<div style="background:var(--bg2,#F8F9FA);border-radius:10px;padding:14px 16px;position:relative;border:1px solid var(--border-light);">' +
                        '<div style="position:absolute;top:10px;right:10px;display:flex;gap:6px;">' +
                        '<button type="button" class="btn-editar-contacto" data-idx="' + idx + '" title="Editar" style="background:none;border:none;color:#3498DB;cursor:pointer;font-size:15px;"><i class="fas fa-pencil-alt"></i></button>' +
                        '<button type="button" class="btn-quitar-contacto" data-idx="' + idx + '" title="Eliminar" style="background:none;border:none;color:#C0392B;cursor:pointer;font-size:15px;"><i class="fas fa-times"></i></button>' +
                        '</div>' +
                        '<div style="font-weight:700;font-size:13.5px;color:var(--text1);margin-bottom:4px;padding-right:30px;">' + escapeHtml(c.NombreContacto) + (c.RepresentanteLegal ? ' <span style="background:#E3F2FD;color:#1565C0;font-size:10px;padding:2px 8px;border-radius:10px;font-weight:600;margin-left:6px;">Rep. Legal</span>' : '') + '</div>' +
                        '<div style="font-size:12px;color:var(--text3);margin-bottom:2px;"><i class="fas fa-envelope" style="width:14px;"></i> ' + escapeHtml(c.Correo) + '</div>' +
                        '<div style="font-size:12px;color:var(--text3);"><i class="fas fa-phone" style="width:14px;"></i> ' + escapeHtml(c.Telefono) + '</div>' +
                        '</div>');
                }
            });
        }
        $('#hdnContactos').val(JSON.stringify(contactos));
    }

    // === Render sucursales ===
    function renderSucursales() {
        var $cont = $('#listaSucursales').empty();
        if (sucursales.length === 0) {
            $cont.html('<div style="color:var(--text3);font-size:13px;padding:8px 0;">Sin sucursales registradas.</div>');
        } else {
            sucursales.forEach(function (s, idx) {
                if (esModoDetalle()) {
                    $cont.append(
                        '<div style="background:white; border-radius:8px; padding:12px 16px; border:1px solid var(--border-light); display:flex; justify-content:space-between; align-items:center; margin-bottom:8px;">' +
                        '<div style="font-weight:700; font-size:14px; color:var(--text1);">' + escapeHtml(s.NombreSucursal) + '</div>' +
                        '<button type="button" class="btn-ver-sucursal" data-idx="' + idx + '" title="Ver sucursal" style="background:none; border:1px solid var(--border-light); border-radius:6px; padding:4px 12px; color:var(--brown-dark); cursor:pointer;"><i class="fas fa-eye"></i></button>' +
                        '</div>'
                    );
                } else {
                    $cont.append(
                        '<div style="background:var(--bg2,#F8F9FA);border-radius:10px;padding:14px 16px;position:relative;border:1px solid var(--border-light);margin-bottom:8px;">' +
                        '<div style="position:absolute;top:10px;right:10px;display:flex;gap:6px;">' +
                        '<button type="button" class="btn-editar-sucursal" data-idx="' + idx + '" title="Editar" style="background:none;border:none;color:#3498DB;cursor:pointer;font-size:15px;"><i class="fas fa-pencil-alt"></i></button>' +
                        '<button type="button" class="btn-quitar-sucursal" data-idx="' + idx + '" title="Eliminar" style="background:none;border:none;color:#C0392B;cursor:pointer;font-size:15px;"><i class="fas fa-times"></i></button>' +
                        '</div>' +
                        '<div style="font-weight:700;font-size:13.5px;color:var(--text1);margin-bottom:4px;padding-right:30px;">' + escapeHtml(s.NombreSucursal) + '</div>' +
                        '<div style="display:grid;grid-template-columns:1fr 1fr;gap:4px;">' +
                        '<div style="font-size:12px;color:var(--text3);"><i class="fas fa-map-marker-alt" style="width:14px;"></i> ' + escapeHtml((s.Calle || '') + ' ' + (s.NumExt || '') + (s.Colonia ? ', ' + s.Colonia : '')) + '</div>' +
                        '<div style="font-size:12px;color:var(--text3);"><i class="fas fa-user" style="width:14px;"></i> ' + escapeHtml(s.NombreResponsable || '') + '</div>' +
                        '<div style="font-size:12px;color:var(--text3);"><i class="fas fa-phone" style="width:14px;"></i> ' + escapeHtml(s.TelefonoSucursal || '') + '</div>' +
                        '<div style="font-size:12px;color:var(--text3);"><i class="fas fa-envelope" style="width:14px;"></i> ' + escapeHtml(s.CorreoElectronico || '') + '</div>' +
                        '</div></div>');
                }
            });
        }
        $('#hdnSucursales').val(JSON.stringify(sucursales));
    }

    // === Tabs del formulario principal ===
    window.showTab = function (tabId, element) {
        document.querySelectorAll('#modalFormularioProspecto .tab-content').forEach(function (t) { t.classList.remove('active'); });
        document.querySelectorAll('#modalFormularioProspecto .tab-item').forEach(function (t) { t.classList.remove('active'); });
        var tab = document.getElementById('tab-' + tabId);
        if (tab) tab.classList.add('active');
        if (element) element.classList.add('active');

        var index = tabsProspecto.indexOf(tabId);
        if (index >= 0) {
            tabIdxProspecto = index;
            actualizarBotonesProspecto();
        }

        // Si se muestra la pestaña dirección y ya hay mapa, ajustar tamaño
        if (tabId === 'direccion' && window.mapasLeaflet && window.mapasLeaflet['form']) {
            setTimeout(function () { window.mapasLeaflet['form'].invalidateSize(); }, 50);
        }
    };

    function actualizarBotonesProspecto() {
        var esUltimo = tabIdxProspecto === tabsProspecto.length - 1;
        var btnAnterior = document.getElementById('btnProspectoAnterior');
        if (btnAnterior) btnAnterior.style.display = tabIdxProspecto > 0 ? 'inline-block' : 'none';

        var btnCancelar = document.getElementById('btnProspectoCancelar');
        if (btnCancelar) btnCancelar.style.display = tabIdxProspecto === 0 ? 'inline-block' : 'none';

        var btnSiguiente = document.getElementById('btnProspectoSiguiente');
        if (btnSiguiente) btnSiguiente.style.display = !esUltimo ? 'inline-block' : 'none';

        var btnGuardar = document.getElementById('btnProspectoGuardar');
        if (btnGuardar) btnGuardar.style.display = esUltimo ? 'inline-block' : 'none';
    }

    function validarTabProspecto(idx) {
        var campos = [];
        if (idx === 0) {
            var razon = document.getElementById('Nombre');
            if (razon && !razon.value.trim()) campos.push('&bull; <b>Raz&oacute;n Social / Nombre completo</b> (en Datos Personales)');
            var tipo = document.getElementById('TipoPersona');
            if (tipo && !tipo.value.trim()) campos.push('&bull; <b>Tipo de Persona</b> (en Datos Personales)');
        }
        if (campos.length > 0) {
            Swal.fire({
                icon: 'warning',
                title: 'Campos requeridos',
                html: 'Por favor completa los siguientes campos antes de continuar:<br><br>' + campos.join('<br>'),
                confirmButtonColor: '#7b3f1a'
            });
            return false;
        }
        return true;
    }

    window.navProspecto = function (dir) {
        if (dir === 1 && !validarTabProspecto(tabIdxProspecto)) return;
        tabIdxProspecto = Math.max(0, Math.min(tabsProspecto.length - 1, tabIdxProspecto + dir));
        var tabItems = document.querySelectorAll('#modalFormularioProspecto .tab-item');
        showTab(tabsProspecto[tabIdxProspecto], tabItems[tabIdxProspecto]);
        actualizarBotonesProspecto();

        if (tabsProspecto[tabIdxProspecto] === 'direccion' && window.mapasLeaflet && window.mapasLeaflet['form']) {
            setTimeout(function () { window.mapasLeaflet['form'].invalidateSize(); }, 50);
        }
    };

    // === Modal principal (Nuevo/Editar/Detalle) ===
    function cargarModal(url, modo) {
        var $container = $('#modalProspectoContainer');
        $container.empty().html('<div style="display:flex;align-items:center;justify-content:center;height:100%;color:white;"><i class="fas fa-spinner fa-spin fa-2x"></i></div>').show();

        $.get(url, function (html) {
            $container.html(html);
            window.esModoDetalle = (modo === 'Detalle');

            // Resetear variables
            tabIdxProspecto = 0;
            contactos = window.contactosIniciales || [];
            sucursales = window.sucursalesIniciales || [];

            // Inicializar UI según modo
            if (window.esModoDetalle) {
                $('#frmProspecto input, #frmProspecto select, #frmProspecto textarea, #frmProspecto button[type="submit"]').prop('disabled', true);
                $('#frmProspecto button[onclick*="verificarDireccion(\'form\')"]').prop('disabled', true);
                $('#btnProspectoGuardar').remove();
                $('#btnAgregarContacto, #btnAgregarSucursal').hide();
                $('[id^=fotoSlot], #docCatastralSlot').css('cursor', 'default').removeAttr('onclick');
                $('#btnAccionesDetalle').css('display', 'flex');
                $('#seccionEstadoDetalle').show();
                $('#seccionCotizacionDetalle').show();
                if ($('#motivoRechazoDetalle').length && $('#hdnEstatusProspecto').val().toLowerCase() === 'rechazado') {
                    $('#motivoRechazoDetalle').show();
                }
                if ($('#hdnFotoFachada').val()) { $('#fotoPreview1').show().attr('src', 'data:image/jpeg;base64,' + $('#hdnFotoFachada').val()); $('#fotoPlaceholder1').hide(); }
                if ($('#hdnFotoAcceso').val()) { $('#fotoPreview2').show().attr('src', 'data:image/jpeg;base64,' + $('#hdnFotoAcceso').val()); $('#fotoPlaceholder2').hide(); }
                if ($('#hdnFotoReferencia').val()) { $('#fotoPreview3').show().attr('src', 'data:image/jpeg;base64,' + $('#hdnFotoReferencia').val()); $('#fotoPlaceholder3').hide(); }

                // Inicializar mapa de matriz en solo lectura si hay coordenadas guardadas
                var latMatriz = parseFloat(($('#Lat').val() || '').replace(',', '.'));
                var lngMatriz = parseFloat(($('#Lng').val() || '').replace(',', '.'));
                if (!isNaN(latMatriz) && !isNaN(lngMatriz)) {
                    setTimeout(function() {
                        window.inicializarMapaLeaflet('form', latMatriz, lngMatriz, '', true);
                    }, 100);
                }
            }

            actualizarBotonesProspecto();
            renderContactos();
            renderSucursales();
            inicializarPreviewsMatriz();
            toggleSucursales();

            // Mostrar modal
            var modal = document.getElementById('modalFormularioProspecto');
            if (modal) modal.style.display = 'flex';
        }).fail(function () {
            Swal.fire('Error', 'No se pudo cargar el formulario.', 'error');
            $container.hide();
        });
    }

    window.abrirModalNuevo = function () {
        cargarModal(window.urlPartialNuevo, 'Nuevo');
    };

    window.abrirModalEditar = function (id) {
        cargarModal(window.urlPartialEditar + '/' + id, 'Editar');
    };

    window.abrirModalDetalle = function (id) {
        cargarModal(window.urlPartialDetalle + '/' + id, 'Detalle');
    };

    window.cerrarModalPrincipal = function () {
        var modal = document.getElementById('modalFormularioProspecto');
        if (modal) modal.style.display = 'none';

        // Limpiar mapas Leaflet antes de destruir el DOM
        ['form', 'suc'].forEach(function (prefix) {
            if (window.mapasLeaflet && window.mapasLeaflet[prefix]) {
                try { window.mapasLeaflet[prefix].remove(); } catch (e) { }
                delete window.mapasLeaflet[prefix];
                delete window.marcadoresLeaflet[prefix];
            }
        });

        $('#modalProspectoContainer').empty().hide();
        window.esModoDetalle = false;
        tabIdxProspecto = 0;
        contactos = [];
        sucursales = [];
    };

    // === Contactos ===
    window.abrirModalContacto = function () {
        window.contactoEditIndex = -1;
        $('#contactoNombre, #contactoCorreo, #contactoTelefono').val('');
        $('#contactoRepresentante').prop('checked', false);
        $('.modal-contacto-titulo').text('AGREGAR CONTACTO');
        $('#modalContactoEdit').show();
        $('#modalContactoView').hide();
        var modal = document.getElementById('modalContacto');
        if (modal) modal.style.display = 'flex';
    };

    window.cerrarModalContacto = function () {
        var modal = document.getElementById('modalContacto');
        if (modal) modal.style.display = 'none';
    };

    window.guardarContactoConValidacion = function () {
        var nombre = $('#contactoNombre').val().trim();
        var correo = $('#contactoCorreo').val().trim();
        var telefono = $('#contactoTelefono').val().trim();
        var campos = [];
        if (!nombre) campos.push('&bull; <b>Nombre completo</b>');
        if (!correo) campos.push('&bull; <b>Correo electr&oacute;nico</b>');
        if (!telefono) campos.push('&bull; <b>Tel&eacute;fono</b>');
        if (campos.length > 0) {
            Swal.fire({
                icon: 'warning', title: 'Campos requeridos',
                html: 'Por favor completa los siguientes campos:<br><br>' + campos.join('<br>'),
                confirmButtonColor: '#7b3f1a'
            });
            return;
        }
        var nuevoContacto = {
            NombreContacto: nombre, Correo: correo, Telefono: telefono,
            RepresentanteLegal: $('#contactoRepresentante').is(':checked')
        };
        if (window.contactoEditIndex >= 0) {
            contactos[window.contactoEditIndex] = nuevoContacto;
            Swal.fire('Actualizado', 'Contacto actualizado correctamente.', 'success');
        } else {
            contactos.push(nuevoContacto);
            Swal.fire('Agregado', 'Contacto agregado correctamente.', 'success');
        }
        renderContactos();
        cerrarModalContacto();
    };

    function verContacto(idx) {
        var c = contactos[idx];
        $('#viewContactoNombre').text(c.NombreContacto || '--');
        $('#viewContactoCorreo').text(c.Correo || '--');
        $('#viewContactoTelefono').text(c.Telefono || '--');
        $('#viewContactoRep').text(c.RepresentanteLegal ? 'Sí' : 'No');
        $('.modal-contacto-titulo').text('VER CONTACTO');
        $('#modalContactoEdit').hide();
        $('#modalContactoView').show();
        var modal = document.getElementById('modalContacto');
        if (modal) modal.style.display = 'flex';
    }

    // === Sucursales ===
    window.showTabSuc = function (tabId, element) {
        document.querySelectorAll('#modalSucursal .tab-content-suc').forEach(function (t) { t.style.display = 'none'; });
        document.querySelectorAll('#modalSucursal .tab-item-suc').forEach(function (t) {
            t.style.color = 'var(--text3)'; t.style.fontWeight = '600'; t.style.borderBottomColor = 'transparent';
        });
        var tab = document.getElementById('tab-suc-' + tabId);
        if (tab) tab.style.display = 'block';
        if (element) { element.style.color = 'var(--brown-dark)'; element.style.fontWeight = '700'; element.style.borderBottomColor = 'var(--brown-dark)'; }

        var index = tabsSuc.indexOf(tabId);
        if (index >= 0) {
            tabIdxSuc = index;
            actualizarBotonesSuc();
        }

        // Inicializar mapa de sucursal en solo lectura si se abre la pestaña dirección y hay coordenadas
        if (tabId === 'direccion') {
            var latSuc = parseFloat((document.getElementById('sucLat')?.value || '').replace(',', '.'));
            var lngSuc = parseFloat((document.getElementById('sucLng')?.value || '').replace(',', '.'));
            if (!isNaN(latSuc) && !isNaN(lngSuc)) {
                setTimeout(function() {
                    window.inicializarMapaLeaflet('suc', latSuc, lngSuc, '', window.sucursalSoloLectura === true);
                }, 100);
            }
        }
    };

    function actualizarBotonesSuc() {
        var esUltimo = tabIdxSuc === tabsSuc.length - 1;
        var btnAnterior = document.getElementById('btnSucAnterior');
        if (btnAnterior) btnAnterior.style.display = tabIdxSuc > 0 ? 'inline-block' : 'none';
        var btnSiguiente = document.getElementById('btnSucSiguiente');
        if (btnSiguiente) btnSiguiente.style.display = !esUltimo ? 'inline-block' : 'none';
        var btnGuardar = document.getElementById('btnGuardarSucursal');
        if (btnGuardar) btnGuardar.style.display = esUltimo ? 'inline-block' : 'none';
    }

    function validarTabSuc(idx) {
        var campos = [];
        if (idx === 0) {
            var nombre = document.getElementById('sucNombre');
            if (nombre && !nombre.value.trim()) campos.push('&bull; <b>Nombre de Sucursal</b>');
            var tel = document.getElementById('sucTelefono');
            if (tel && !tel.value.trim()) campos.push('&bull; <b>Tel&eacute;fono</b>');
        }
        if (campos.length > 0) {
            Swal.fire({
                icon: 'warning',
                title: 'Campos requeridos',
                html: 'Por favor completa:<br><br>' + campos.join('<br>'),
                confirmButtonColor: '#7b3f1a'
            });
            return false;
        }
        return true;
    }

    window.navSuc = function (dir) {
        if (dir === 1 && !validarTabSuc(tabIdxSuc)) return;
        tabIdxSuc = Math.max(0, Math.min(tabsSuc.length - 1, tabIdxSuc + dir));
        var tabItems = document.querySelectorAll('#modalSucursal .tab-item-suc');
        showTabSuc(tabsSuc[tabIdxSuc], tabItems[tabIdxSuc]);
        actualizarBotonesSuc();

        if (tabsSuc[tabIdxSuc] === 'direccion' && window.mapasLeaflet && window.mapasLeaflet['suc']) {
            setTimeout(function () { window.mapasLeaflet['suc'].invalidateSize(); }, 50);
        }
    };

    window.resetModalSucursal = function () {
        window.sucursalSoloLectura = false;
        tabIdxSuc = 0;
        var tabItems = document.querySelectorAll('#modalSucursal .tab-item-suc');
        showTabSuc('contacto', tabItems[0]);
        actualizarBotonesSuc();
        ['sucNombre', 'sucTelefono', 'sucCorreo', 'sucResponsable', 'sucCalle', 'sucNumExt', 'sucNumInt',
            'sucColonia', 'sucCp', 'sucMunicipio', 'sucEstado', 'sucConcesionaria', 'sucReferencias', 'sucFolio']
            .forEach(function (id) {
                var el = document.getElementById(id);
                if (el) el.value = el.id === 'sucEstado' ? 'Yucat&aacute;n' : '';
            });
        $('#hdnSucFotoFachada, #hdnSucFotoAcceso, #hdnSucFotoReferencia, #hdnSucDocCatastral, #hdnSucDocCatastralNombre').val('');
        $('#sucFotoPreview1, #sucFotoPreview2, #sucFotoPreview3').hide().attr('src', '#');
        $('#sucFotoPlaceholder1, #sucFotoPlaceholder2, #sucFotoPlaceholder3').show();
        $('#sucDocCatastralNombre').hide();
        $('#sucDocCatastralPlaceholder').show();
        $('#modalSucursal input, #modalSucursal select, #modalSucursal textarea').prop('readonly', false).prop('disabled', false);
        $('#btnGuardarSucursal').show();
        $('.modal-sucursal-titulo').text('AGREGAR SUCURSAL');
    };

    window.abrirModalSucursal = function () {
        window.sucursalEditIndex = -1;
        resetModalSucursal();
        var modal = document.getElementById('modalSucursal');
        if (modal) modal.style.display = 'flex';
    };

    window.cerrarModalSucursal = function () {
        var modal = document.getElementById('modalSucursal');
        if (modal) modal.style.display = 'none';
    };

    window.guardarSucursalConValidacion = function () {
        var nombre = $('#sucNombre').val().trim();
        var telefono = $('#sucTelefono').val().trim();
        var correo = $('#sucCorreo').val().trim();
        var responsable = $('#sucResponsable').val().trim();
        if (!nombre || !telefono || !correo || !responsable) {
            var faltantes = [];
            if (!nombre) faltantes.push('Nombre de Sucursal');
            if (!telefono) faltantes.push('Tel&eacute;fono de sucursal');
            if (!correo) faltantes.push('Correo electr&oacute;nico');
            if (!responsable) faltantes.push('Nombre del responsable');
            Swal.fire('Atenci&oacute;n', 'Completa los siguientes datos obligatorios:\n- ' + faltantes.join('\n- '), 'warning');
            return;
        }
        var nuevaSucursal = {
            NombreSucursal: nombre,
            TelefonoSucursal: telefono,
            CorreoElectronico: correo,
            NombreResponsable: responsable,
            Calle: $('#sucCalle').val().trim(),
            NumExt: $('#sucNumExt').val().trim(),
            NumInt: $('#sucNumInt').val().trim(),
            Colonia: $('#sucColonia').val().trim(),
            Municipio: $('#sucMunicipio').val().trim(),
            Cp: $('#sucCp').val().trim(),
            Estado: $('#sucEstado').val().trim(),
            Lat: $('#sucLat').val().trim(),
            Lng: $('#sucLng').val().trim(),
            FolioCatastral: $('#sucFolio').val().trim(),
            Concesionaria: $('#sucConcesionaria').val().trim(),
            Referencias: $('#sucReferencias').val().trim(),
            FotoFachada: $('#hdnSucFotoFachada').val(),
            FotoAcceso: $('#hdnSucFotoAcceso').val(),
            FotoReferencia: $('#hdnSucFotoReferencia').val(),
            DocumentoCatastral: $('#hdnSucDocCatastral').val(),
            DocumentoCatastralNombre: $('#hdnSucDocCatastralNombre').val()
        };
        if (window.sucursalEditIndex >= 0) {
            sucursales[window.sucursalEditIndex] = nuevaSucursal;
            Swal.fire('Actualizada', 'Sucursal actualizada correctamente.', 'success');
        } else {
            sucursales.push(nuevaSucursal);
            Swal.fire('Agregada', 'Sucursal agregada correctamente.', 'success');
        }
        renderSucursales();
        cerrarModalSucursal();
    };

    function editarSucursal(idx) {
        window.sucursalEditIndex = idx;
        var s = sucursales[idx];
        if (!s) return;

        $('#sucNombre').val(s.NombreSucursal);
        $('#sucTelefono').val(s.TelefonoSucursal);
        $('#sucCorreo').val(s.CorreoElectronico);
        $('#sucResponsable').val(s.NombreResponsable);
        $('#sucCalle').val(s.Calle);
        $('#sucNumExt').val(s.NumExt);
        $('#sucNumInt').val(s.NumInt);
        $('#sucColonia').val(s.Colonia);
        $('#sucMunicipio').val(s.Municipio);
        $('#sucCp').val(s.Cp);
        $('#sucEstado').val(s.Estado || 'Yucat&aacute;n');
        $('#sucLat').val(s.Lat);
        $('#sucLng').val(s.Lng);
        $('#sucFolio').val(s.FolioCatastral);
        $('#sucConcesionaria').val(s.Concesionaria);
        $('#sucReferencias').val(s.Referencias);

        $('#hdnSucFotoFachada').val(s.FotoFachada || '');
        $('#hdnSucFotoAcceso').val(s.FotoAcceso || '');
        $('#hdnSucFotoReferencia').val(s.FotoReferencia || '');
        $('#hdnSucDocCatastral').val(s.DocumentoCatastral || '');
        $('#hdnSucDocCatastralNombre').val(s.DocumentoCatastralNombre || '');

        setPreviewSuc(s.FotoFachada, 'sucFotoPreview1', 'sucFotoPlaceholder1', 'sucFotoSlot1');
        setPreviewSuc(s.FotoAcceso, 'sucFotoPreview2', 'sucFotoPlaceholder2', 'sucFotoSlot2');
        setPreviewSuc(s.FotoReferencia, 'sucFotoPreview3', 'sucFotoPlaceholder3', 'sucFotoSlot3');

        var docSlot = document.getElementById('sucDocCatastralSlot');
        if (s.DocumentoCatastral) {
            $('#sucDocCatastralNombreTexto').text(s.DocumentoCatastralNombre || 'documento.pdf');
            $('#sucDocCatastralNombre').show();
            $('#sucDocCatastralPlaceholder').hide();
            if (docSlot) { docSlot.style.border = '2px solid #27AE60'; docSlot.style.background = '#F0FFF4'; }
        } else {
            $('#sucDocCatastralNombre').hide();
            $('#sucDocCatastralPlaceholder').show();
            if (docSlot) { docSlot.style.border = '2px dashed #C4A574'; docSlot.style.background = '#FFFAF5'; }
        }

        window.sucursalSoloLectura = false;
        $('#modalSucursal input, #modalSucursal select, #modalSucursal textarea').prop('readonly', false).prop('disabled', false);
        $('#modalSucursal button[onclick*="verificarDireccion(\'suc\')"]').prop('disabled', false);
        $('#btnGuardarSucursal').show();
        $('.modal-sucursal-titulo').text('EDITAR SUCURSAL');

        var modal = document.getElementById('modalSucursal');
        if (modal) modal.style.display = 'flex';
    }

    function verSucursal(idx) {
        window.sucursalSoloLectura = true;
        window.sucursalEditIndex = idx;
        var s = sucursales[idx];
        if (!s) return;

        $('#sucNombre').val(s.NombreSucursal);
        $('#sucTelefono').val(s.TelefonoSucursal);
        $('#sucCorreo').val(s.CorreoElectronico);
        $('#sucResponsable').val(s.NombreResponsable);
        $('#sucCalle').val(s.Calle);
        $('#sucNumExt').val(s.NumExt);
        $('#sucNumInt').val(s.NumInt);
        $('#sucColonia').val(s.Colonia);
        $('#sucMunicipio').val(s.Municipio);
        $('#sucCp').val(s.Cp);
        $('#sucEstado').val(s.Estado || 'Yucat&aacute;n');
        $('#sucLat').val(s.Lat);
        $('#sucLng').val(s.Lng);
        $('#sucFolio').val(s.FolioCatastral);
        $('#sucConcesionaria').val(s.Concesionaria);
        $('#sucReferencias').val(s.Referencias);

        setPreviewSuc(s.FotoFachada, 'sucFotoPreview1', 'sucFotoPlaceholder1', 'sucFotoSlot1');
        setPreviewSuc(s.FotoAcceso, 'sucFotoPreview2', 'sucFotoPlaceholder2', 'sucFotoSlot2');
        setPreviewSuc(s.FotoReferencia, 'sucFotoPreview3', 'sucFotoPlaceholder3', 'sucFotoSlot3');

        var docSlot = document.getElementById('sucDocCatastralSlot');
        if (s.DocumentoCatastral) {
            var displayName = s.DocumentoCatastralNombre || 'documento.pdf';
            var downloadUrl = s.DocumentoCatastral.startsWith('data:') ? s.DocumentoCatastral : 'data:application/pdf;base64,' + s.DocumentoCatastral;
            $('#sucDocCatastralNombreTexto').html('<a href="' + downloadUrl + '" download="' + displayName + '" style="color:#27AE60; text-decoration:underline;">' + displayName + '</a>');
            $('#sucDocCatastralNombre').show();
            $('#sucDocCatastralPlaceholder').hide();
            if (docSlot) { docSlot.style.border = '2px solid #27AE60'; docSlot.style.background = '#F0FFF4'; }
        } else {
            $('#sucDocCatastralNombre').hide();
            $('#sucDocCatastralPlaceholder').show();
            if (docSlot) { docSlot.style.border = '2px dashed #eee'; docSlot.style.background = '#fafafa'; }
        }

        $('#modalSucursal input, #modalSucursal select, #modalSucursal textarea').prop('readonly', true).prop('disabled', true);
        $('#modalSucursal button[onclick*="verificarDireccion(\'suc\')"]').prop('disabled', true);
        $('#btnGuardarSucursal').hide();
        $('.modal-sucursal-titulo').text('VER SUCURSAL');

        var modal = document.getElementById('modalSucursal');
        if (modal) modal.style.display = 'flex';
    }

    function setPreviewSuc(b64, previewId, placeholderId, slotId) {
        var el = document.getElementById(slotId);
        if (el) { el.style.cursor = 'default'; }
        if (!b64) {
            $('#' + previewId).hide().attr('src', '#');
            $('#' + placeholderId).show();
            if (el) { el.style.border = '2px dashed #C4A574'; el.style.background = '#FFFAF5'; }
        } else {
            $('#' + previewId).show().attr('src', b64.startsWith('data:') ? b64 : 'data:image/jpeg;base64,' + b64);
            $('#' + placeholderId).hide();
            if (el) { el.style.border = '2px solid #C4A574'; el.style.background = '#fff'; }
        }
    }

    // === Preview de fotos y documentos ===
    window.previewFoto = function (input, previewId, placeholderId, slotId, hiddenId) {
        if (input.files && input.files[0]) {
            var reader = new FileReader();
            reader.onload = function (e) {
                var preview = document.getElementById(previewId);
                var placeholder = document.getElementById(placeholderId);
                var slot = document.getElementById(slotId);
                if (preview) { preview.src = e.target.result; preview.style.display = 'block'; }
                if (placeholder) placeholder.style.display = 'none';
                if (slot) { slot.style.border = '2px solid #C4A574'; slot.style.background = '#fff'; }
                if (hiddenId) {
                    var hdn = document.getElementById(hiddenId);
                    if (hdn) hdn.value = e.target.result;
                }
            };
            reader.readAsDataURL(input.files[0]);
        }
    };

    window.previewDoc = function (input, nombreId, placeholderId, nombreTextoId, slotId, hiddenDocId, hiddenNameId) {
        if (input.files && input.files[0]) {
            var reader = new FileReader();
            var nombre = input.files[0].name;
            reader.onload = function (e) {
                var nombreTexto = document.getElementById(nombreTextoId);
                var nombreEl = document.getElementById(nombreId);
                var placeholder = document.getElementById(placeholderId);
                var slot = document.getElementById(slotId);
                if (nombreTexto) nombreTexto.innerText = nombre;
                if (nombreEl) nombreEl.style.display = 'block';
                if (placeholder) placeholder.style.display = 'none';
                if (slot) { slot.style.border = '2px solid #27AE60'; slot.style.background = '#F0FFF4'; }
                if (hiddenDocId) {
                    var hdnDoc = document.getElementById(hiddenDocId);
                    if (hdnDoc) hdnDoc.value = e.target.result;
                }
                if (hiddenNameId) {
                    var hdnName = document.getElementById(hiddenNameId);
                    if (hdnName) hdnName.value = nombre;
                }
            };
            reader.readAsDataURL(input.files[0]);
        }
    };

    function inicializarPreviewsMatriz() {
        function initPreview(hdnId, previewId, placeholderId, slotId) {
            var b64 = $('#' + hdnId).val();
            if (b64) {
                $('#' + previewId).show().attr('src', b64.startsWith('data:') ? b64 : 'data:image/jpeg;base64,' + b64);
                $('#' + placeholderId).hide();
                var el = document.getElementById(slotId);
                if (el) { el.style.border = '2px solid #C4A574'; el.style.background = '#fff'; }
            }
        }
        initPreview('hdnFotoFachada', 'fotoPreview1', 'fotoPlaceholder1', 'fotoSlot1');
        initPreview('hdnFotoAcceso', 'fotoPreview2', 'fotoPlaceholder2', 'fotoSlot2');
        initPreview('hdnFotoReferencia', 'fotoPreview3', 'fotoPlaceholder3', 'fotoSlot3');

        var docB64 = $('#hdnDocumentoCatastral').val();
        var docName = $('#hdnDocumentoCatastralNombre').val();
        if (docB64) {
            var displayName = docName || 'Documento.pdf';
            if (esModoDetalle()) {
                var downloadUrl = docB64.startsWith('data:') ? docB64 : 'data:application/pdf;base64,' + docB64;
                $('#docCatastralNombreTexto').html('<a href="' + downloadUrl + '" download="' + displayName + '" style="color:#27AE60; text-decoration:underline;">' + displayName + '</a>');
            } else {
                $('#docCatastralNombreTexto').text(displayName);
            }
            $('#docCatastralNombre').show();
            $('#docCatastralPlaceholder').hide();
            var docSlot = document.getElementById('docCatastralSlot');
            if (docSlot) { docSlot.style.border = '2px solid #27AE60'; docSlot.style.background = '#F0FFF4'; }
        }

        if (esModoDetalle()) {
            $('#fotoSlot1, #fotoSlot2, #fotoSlot3, #docCatastralSlot').removeAttr('onclick').css('cursor', 'default');
        }
    }

    function toggleSucursales() {
        var tieneSuc = ($('#TieneSucursales').val() || '').toString().toLowerCase();
        if (tieneSuc === 's&iacute;' || tieneSuc === 'si' || tieneSuc === 'sí' || tieneSuc === 'yes' || tieneSuc === 's') {
            $('#seccion-sucursales').show();
        } else {
            $('#seccion-sucursales').hide();
        }
    }

    // === Acciones de tabla ===
    window.accionRechazar = function (id) {
        Swal.fire({
            title: 'Motivo de rechazo',
            input: 'textarea',
            inputPlaceholder: 'Escribe el motivo...',
            showCancelButton: true,
            confirmButtonText: 'Rechazar',
            confirmButtonColor: '#C0392B',
            cancelButtonText: 'Cancelar'
        }).then(function (result) {
            if (result.isConfirmed && result.value) {
                $.post(window.urlRechazar, { id: id, motivo: result.value }, function (res) {
                    if (res.success) {
                        Swal.fire('Rechazado', 'El prospecto ha sido rechazado.', 'success').then(function () {
                            window.location.href = window.urlBase;
                        });
                    } else {
                        Swal.fire('Error', res.error || 'Ocurri&oacute; un error', 'error');
                    }
                });
            }
        });
    };

    window.accionAsignarVendedor = function (id) {
        var vendedores = {};
        if (window.vendedores && window.vendedores.length) {
            window.vendedores.forEach(function (v) {
                vendedores[v.id] = (v.nombre || v.Nombre || 'Vendedor');
            });
        }
        if (Object.keys(vendedores).length === 0) {
            Swal.fire('Atenci&oacute;n', 'No hay vendedores disponibles.', 'warning');
            return;
        }
        Swal.fire({
            title: 'Selecciona vendedor',
            input: 'select',
            inputOptions: vendedores,
            inputPlaceholder: 'Seleccione',
            showCancelButton: true,
            confirmButtonText: 'Asignar Vendedor',
            confirmButtonColor: '#E67E22',
            cancelButtonText: 'Cerrar'
        }).then(function (result) {
            if (result.isConfirmed && result.value) {
                $.post(window.urlAsignarVendedor, { id: id, vendedorId: result.value }, function (res) {
                    if (res.success) {
                        Swal.fire('Asignado', 'Vendedor asignado correctamente.', 'success').then(function () {
                            window.location.href = window.urlBase;
                        });
                    } else {
                        Swal.fire('Error', res.error || 'Ocurri&oacute; un error', 'error');
                    }
                });
            }
        });
    };

    window.accionDarDeBaja = function (id) {
        var nombre = $('#Nombre').val() || 'El prospecto';
        Swal.fire({
            icon: 'warning',
            title: '&iquest;Dar de baja?',
            html: '<b>' + nombre + '</b> cambiar&aacute; a estatus Inactivo.',
            showCancelButton: true,
            confirmButtonText: 'S&iacute;, dar de baja',
            confirmButtonColor: '#C0392B',
            cancelButtonText: 'Cancelar'
        }).then(function (result) {
            if (result.isConfirmed) {
                $.post(window.urlCambiarEstatus, { id: id, estatus: 'Inactivo' }, function (res) {
                    if (res.success) {
                        Swal.fire('Dado de baja', 'El prospecto est&aacute; ahora inactivo.', 'success').then(function () {
                            window.location.href = window.urlBase;
                        });
                    } else {
                        Swal.fire('Error', res.error || 'Ocurri&oacute; un error', 'error');
                    }
                });
            }
        });
    };

    window.abrirModalNotificacion = function (id, nombre, correo) {
        Swal.fire({
            title: 'SELECCIONE UN ASUNTO',
            input: 'select',
            inputOptions: {
                'Reenvío de Cotización': 'Reenvío de Cotización',
                'Finalizar datos de registro': 'Finalizar datos de registro'
            },
            inputPlaceholder: '-- Seleccione --',
            showCancelButton: true,
            confirmButtonText: 'Continuar',
            confirmButtonColor: '#5C3819',
            cancelButtonText: 'Cerrar'
        }).then(function (result) {
            if (result.isConfirmed && result.value) {
                enviarNotificacion(id, result.value, nombre, correo);
            }
        });
    };

    function enviarNotificacion(id, asunto, nombre, correo) {
        var payload = {
            tipo_asunto: asunto,
            correo_destino: correo,
            enviado_por: null
        };

        if (asunto === 'Reenvío de Cotización') {
            // Precalcular referencia y vigencia para enviar siempre valores no nulos.
            var now = new Date();
            var yyyy = now.getFullYear();
            var mm = String(now.getMonth() + 1).padStart(2, '0');
            var dd = String(now.getDate()).padStart(2, '0');
            payload.cotizacion_ref = 'COT-' + yyyy + '-' + mm + '-' + dd;
            payload.vigencia_inicio = yyyy + '-' + mm + '-' + dd;
            var fin = new Date(now.getTime() + 7 * 24 * 60 * 60 * 1000);
            payload.vigencia_fin = fin.getFullYear() + '-' + String(fin.getMonth() + 1).padStart(2, '0') + '-' + String(fin.getDate()).padStart(2, '0');
            payload.password_temporal = ''; // no aplica

            Swal.fire({
                icon: 'success',
                title: 'REENVÍO DE COTIZACIÓN',
                html: '<div style="text-align: left; font-size: 14px;">' +
                      'Se enviar&aacute; la cotizaci&oacute;n a <b>' + (nombre || '') + '</b>.<br><br>' +
                      '<b>Correo electr&oacute;nico:</b> ' + (correo || 'No especificado') + '<br>' +
                      '<b>Cotizaci&oacute;n:</b> ' + (payload.cotizacion_ref || 'Se generar&aacute; autom&aacute;ticamente') + '<br>' +
                      '<b>Vigencia:</b> ' + (payload.vigencia_inicio || '') + ' al ' + (payload.vigencia_fin || '') +
                      '</div>',
                showCancelButton: true,
                confirmButtonText: 'ENVIAR',
                confirmButtonColor: '#5C3819',
                cancelButtonText: 'Cancelar'
            }).then(function (res2) {
                if (res2.isConfirmed) {
                    $.post(window.urlNotificacion, { id: id, req: JSON.stringify(payload) }, function (res) {
                        if (res.success) {
                            Swal.fire('Enviado', 'Notificaci&oacute;n enviada correctamente.', 'success');
                        } else {
                            Swal.fire('Error', res.error || 'Ocurri&oacute; un error', 'error');
                        }
                    });
                }
            });
        } else if (asunto === 'Finalizar datos de registro') {
            var randPass = Math.floor(10000000 + Math.random() * 90000000) + 'ABC!';
            payload.password_temporal = randPass;
            payload.cotizacion_ref = ''; // no aplica
            payload.vigencia_inicio = '';
            payload.vigencia_fin = '';

            Swal.fire({
                icon: 'success',
                title: 'FINALIZAR DATOS DE REGISTRO',
                html: '<div style="text-align: left; font-size: 14px;">' +
                      'Estimado prospecto, hemos realizado el proceso de registro de su informaci&oacute;n para realizar su cotizaci&oacute;n, ' +
                      'sin embargo, es necesario que nos termine de brindar la informaci&oacute;n faltante.<br><br>' +
                      '<b>Correo electr&oacute;nico:</b> ' + (correo || 'No especificado') + '<br>' +
                      '<b>Contrase&ntilde;a Temporal:</b> ' + randPass +
                      '</div>',
                showCancelButton: true,
                confirmButtonText: 'ENVIAR',
                confirmButtonColor: '#5C3819',
                cancelButtonText: 'Cancelar'
            }).then(function (res2) {
                if (res2.isConfirmed) {
                    $.post(window.urlNotificacion, { id: id, req: JSON.stringify(payload) }, function (res) {
                        if (res.success) {
                            Swal.fire('Enviado', 'Notificaci&oacute;n enviada correctamente.', 'success');
                        } else {
                            Swal.fire('Error', res.error || 'Ocurri&oacute; un error', 'error');
                        }
                    });
                }
            });
        }
    }

    // === Document ready ===
    $(function () {
        // Llenar select de estatus
        var $selectEstatus = $('#filtroEstatus');
        if ($selectEstatus.length && window.estatusLista && window.estatusLista.length) {
            $selectEstatus.empty().append('<option value="">Todos los estatus</option>');
            window.estatusLista.forEach(function (e) {
                $selectEstatus.append('<option value="' + e + '">' + e + '</option>');
            });
            if (window.estatusFiltro) $selectEstatus.val(window.estatusFiltro);
        }

        // Filtros
        $('#filtroBusqueda').on('keyup', function () {
            var value = $(this).val().toLowerCase();
            $('#tablaProspectos tbody tr').filter(function () {
                $(this).toggle($(this).text().toLowerCase().indexOf(value) > -1);
            });
        });

        $('#filtroEstatus').on('change', function () {
            var value = $(this).val();
            if (value === "") {
                $('#tablaProspectos tbody tr').show();
            } else {
                $('#tablaProspectos tbody tr').filter(function () {
                    var estatus = $(this).find('.badge').text().trim();
                    $(this).toggle(estatus === value);
                });
            }
        });

        // Botón agregar prospecto
        $('#btnAgregarProspecto').on('click', function () {
            abrirModalNuevo();
        });

        // Eventos delegados de la tabla
        $(document).on('click', '.btn-ver-prospecto', function () {
            abrirModalDetalle($(this).data('id'));
        });

        $(document).on('click', '.btn-editar-prospecto', function () {
            abrirModalEditar($(this).data('id'));
        });

        $(document).on('click', '.btn-notificar-prospecto', function () {
            abrirModalNotificacion($(this).data('id'), $(this).data('nombre'), $(this).data('correo'));
        });

        $(document).on('click', '.btn-eliminar-prospecto', function () {
            var id = $(this).data('id');
            var nombre = $(this).data('nombre');
            Swal.fire({
                title: '&iquest;Eliminar prospecto?',
                text: nombre,
                icon: 'warning',
                showCancelButton: true,
                confirmButtonText: 'S&iacute;, eliminar',
                cancelButtonText: 'Cancelar',
                confirmButtonColor: '#7b3f1a'
            }).then(function (result) {
                if (result.isConfirmed) {
                    $.post(window.urlEliminar, { id: id }, function (res) {
                        if (res.success) {
                            Swal.fire('Eliminado', '', 'success').then(function () { location.reload(); });
                        } else {
                            Swal.fire('Error', res.error, 'error');
                        }
                    });
                }
            });
        });

        // Eventos delegados dentro del modal principal
        $(document).on('click', '[data-cerrar="modalPrincipal"]', function () {
            cerrarModalPrincipal();
        });

        $(document).on('click', '[data-cerrar="modalContacto"]', function () {
            cerrarModalContacto();
        });

        $(document).on('click', '[data-cerrar="modalSucursal"]', function () {
            cerrarModalSucursal();
        });

        $(document).on('click', '[data-nav]', function () {
            var nav = $(this).data('nav');
            if ($(this).closest('#modalSucursal').length) {
                navSuc(nav);
            } else {
                navProspecto(nav);
            }
        });

        $(document).on('click', '#btnAgregarContacto', function () {
            abrirModalContacto();
        });

        $(document).on('click', '#btnGuardarContacto', function () {
            guardarContactoConValidacion();
        });

        $(document).on('click', '.btn-editar-contacto', function () {
            var idx = parseInt($(this).data('idx'));
            window.contactoEditIndex = idx;
            var c = contactos[idx];
            $('#contactoNombre').val(c.NombreContacto);
            $('#contactoCorreo').val(c.Correo);
            $('#contactoTelefono').val(c.Telefono);
            $('#contactoRepresentante').prop('checked', c.RepresentanteLegal);
            $('.modal-contacto-titulo').text('EDITAR CONTACTO');
            $('#modalContactoEdit').show();
            $('#modalContactoView').hide();
            var modal = document.getElementById('modalContacto');
            if (modal) modal.style.display = 'flex';
        });

        $(document).on('click', '.btn-quitar-contacto', function () {
            var idx = parseInt($(this).data('idx'));
            Swal.fire({
                title: '&iquest;Eliminar contacto?',
                text: "Esta acci&oacute;n no se puede deshacer.",
                icon: 'warning',
                showCancelButton: true,
                confirmButtonColor: '#d33',
                cancelButtonColor: '#3085d6',
                confirmButtonText: 'S&iacute;, eliminar',
                cancelButtonText: 'Cancelar'
            }).then(function (result) {
                if (result.isConfirmed) {
                    contactos.splice(idx, 1);
                    renderContactos();
                    Swal.fire('Eliminado', 'El contacto ha sido eliminado.', 'success');
                }
            });
        });

        $(document).on('click', '.btn-ver-contacto', function () {
            verContacto(parseInt($(this).data('idx')));
        });

        $(document).on('click', '#btnAgregarSucursal', function () {
            abrirModalSucursal();
        });

        $(document).on('click', '#btnGuardarSucursal', function () {
            guardarSucursalConValidacion();
        });

        $(document).on('click', '.btn-editar-sucursal', function () {
            editarSucursal(parseInt($(this).data('idx')));
        });

        $(document).on('click', '.btn-quitar-sucursal', function () {
            var idx = parseInt($(this).data('idx'));
            Swal.fire({
                title: '&iquest;Eliminar sucursal?',
                text: "Esta acci&oacute;n no se puede deshacer.",
                icon: 'warning',
                showCancelButton: true,
                confirmButtonColor: '#d33',
                cancelButtonColor: '#3085d6',
                confirmButtonText: 'S&iacute;, eliminar',
                cancelButtonText: 'Cancelar'
            }).then(function (result) {
                if (result.isConfirmed) {
                    sucursales.splice(idx, 1);
                    renderSucursales();
                    Swal.fire('Eliminada', 'La sucursal ha sido eliminada.', 'success');
                }
            });
        });

        $(document).on('click', '.btn-ver-sucursal', function () {
            verSucursal(parseInt($(this).data('idx')));
        });

        $(document).on('change', '#TieneSucursales', function () {
            toggleSucursales();
        });

        $(document).on('click', '[data-accion]', function () {
            var accion = $(this).data('accion');
            var id = $(this).data('id');
            if (accion === 'rechazar') accionRechazar(id);
            else if (accion === 'asignar') accionAsignarVendedor(id);
            else if (accion === 'baja') accionDarDeBaja(id);
        });

        // Envío vía AJAX cuando el formulario está dentro del modal (evita recargar el layout completo).
        $(document).on('submit', '#frmProspecto', function (e) {
            var $form = $(this);
            // Solo interceptar si el formulario está dentro del contenedor del modal.
            if ($form.closest('#modalProspectoContainer').length === 0) {
                return true; // permitir envío normal en la página completa
            }

            e.preventDefault();

            var esNuevo = ($form.attr('action') || '').toLowerCase().indexOf('nuevo') !== -1;

            // Validación mínima de campos requeridos
            var camposFaltantes = [];
            if (!$('#Nombre').val().trim()) camposFaltantes.push('Razón Social / Nombre completo');
            if (!$('#TipoPersona').val().trim()) camposFaltantes.push('Tipo de persona');
            if (!$('#Telefono').val().trim()) camposFaltantes.push('Teléfono de contacto');
            if (!$('#Email').val().trim()) camposFaltantes.push('Correo electrónico');
            if (!$('#Contacto').val().trim()) camposFaltantes.push('Nombre completo de contacto');

            if (camposFaltantes.length > 0) {
                Swal.fire({
                    icon: 'warning',
                    title: 'Campos requeridos',
                    html: 'Por favor completa los siguientes campos:<br><br>• ' + camposFaltantes.join('<br>• '),
                    confirmButtonColor: '#7b3f1a'
                });
                return false;
            }

            var formData = new FormData(this);

            $.ajax({
                url: $form.attr('action'),
                type: 'POST',
                data: formData,
                processData: false,
                contentType: false,
                success: function (res) {
                    // Si la respuesta no es JSON (p. ej. redirección seguida por jQuery), recargar.
                    if (typeof res === 'string') {
                        window.location.href = window.urlBase || '/Prospectos';
                        return;
                    }
                    if (res && res.success) {
                        Swal.fire({
                            icon: 'success',
                            title: esNuevo ? 'Prospecto registrado' : 'Prospecto actualizado',
                            text: res.message || '',
                            confirmButtonColor: '#7b3f1a'
                        }).then(function () {
                            cerrarModalPrincipal();
                            if (window.location.pathname.toLowerCase().indexOf('prospectos') !== -1) {
                                location.reload();
                            } else {
                                window.location.href = window.urlBase || '/Prospectos';
                            }
                        });
                    } else {
                        Swal.fire({
                            icon: 'error',
                            title: 'Error',
                            text: (res && res.error) ? res.error : 'No se pudo guardar el prospecto.',
                            confirmButtonColor: '#7b3f1a'
                        });
                    }
                },
                error: function (xhr) {
                    var msg = 'Error de comunicación con el servidor.';
                    if (xhr.responseText) {
                        // Si la respuesta contiene el formulario, reemplazar el modal con la vista de errores.
                        if (xhr.responseText.indexOf('frmProspecto') !== -1) {
                            $('#modalProspectoContainer').html(xhr.responseText);
                            return;
                        }
                        msg = 'Error del servidor: ' + xhr.status + ' ' + xhr.statusText;
                    }
                    Swal.fire({ icon: 'error', title: 'Error', text: msg, confirmButtonColor: '#7b3f1a' });
                }
            });

            return false;
        });
    });
})();

// === Mapa Leaflet (globales) ===
window.mapasLeaflet = {};
window.marcadoresLeaflet = {};

window.inicializarMapaLeaflet = function(prefix, lat, lng, displayName, soloLectura) {
    var containerId = 'mapContainer-' + prefix;
    var container = document.getElementById(containerId);
    var span = document.getElementById('mapText-' + prefix);

    if (!container) return;

    var esSoloLectura = soloLectura === true || window.esModoDetalle === true;

    if (span) span.style.display = 'none';
    container.style.border = '2px solid #C4A574';

    var map = window.mapasLeaflet[prefix];
    var marker = window.marcadoresLeaflet[prefix];

    // Si ya existe un mapa pero el modo de solo lectura cambió, recreamos el marcador para reflejar draggable correcto
    if (map && marker) {
        map.setView([lat, lng], 16);
        marker.setLatLng([lat, lng]);
        marker.dragging[esSoloLectura ? 'disable' : 'enable']();
        if (esSoloLectura) {
            marker.off('dragend');
        }
        return;
    }

    var leafletDiv = document.getElementById('leaflet-' + prefix);
    if (!leafletDiv) {
        leafletDiv = document.createElement('div');
        leafletDiv.id = 'leaflet-' + prefix;
        leafletDiv.style.width = '100%';
        leafletDiv.style.height = '100%';
        container.appendChild(leafletDiv);
    }

    var iframe = document.getElementById('mapFrame-' + prefix);
    if (iframe) iframe.style.display = 'none';

    map = L.map('leaflet-' + prefix).setView([lat, lng], 16);
    window.mapasLeaflet[prefix] = map;

    L.tileLayer('https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png', {
        maxZoom: 19,
        attribution: '&copy; OpenStreetMap'
    }).addTo(map);

    marker = L.marker([lat, lng], { draggable: !esSoloLectura }).addTo(map);
    window.marcadoresLeaflet[prefix] = marker;

    if (!esSoloLectura) {
        marker.on('dragend', function(e) {
            var position = marker.getLatLng();

            var latInput = prefix === 'form' ? document.getElementById('Lat') : document.getElementById('sucLat');
            var lngInput = prefix === 'form' ? document.getElementById('Lng') : document.getElementById('sucLng');

            if (latInput) latInput.value = position.lat.toFixed(6);
            if (lngInput) lngInput.value = position.lng.toFixed(6);

            fetch("https://nominatim.openstreetmap.org/reverse?lat=" + position.lat + "&lon=" + position.lng + "&format=json&addressdetails=1")
                .then(function(res) { return res.json(); })
                .then(function(data) {
                    if (data && data.address) {
                        var ad = data.address;

                        var calleInput = prefix === 'form' ? document.getElementById('Calle') : document.getElementById('sucCalle');
                        var numInput = prefix === 'form' ? document.getElementById('NumExt') : document.getElementById('sucNumExt');
                        var colInput = prefix === 'form' ? document.getElementById('Colonia') : document.getElementById('sucColonia');
                        var munInput = prefix === 'form' ? document.getElementById('Municipio') : document.getElementById('sucMunicipio');
                        var estInput = prefix === 'form' ? document.getElementById('Estado') : document.getElementById('sucEstado');
                        var cpInput = prefix === 'form' ? document.getElementById('Cp') : document.getElementById('sucCp');

                        if (calleInput && ad.road) calleInput.value = ad.road;
                        if (numInput && ad.house_number) numInput.value = ad.house_number;
                        if (colInput && (ad.neighbourhood || ad.suburb || ad.village || ad.town)) colInput.value = ad.neighbourhood || ad.suburb || ad.village || ad.town;
                        if (munInput && (ad.city || ad.county || ad.municipality)) munInput.value = ad.city || ad.county || ad.municipality;
                        if (estInput && ad.state) estInput.value = ad.state;
                        if (cpInput && ad.postcode) cpInput.value = ad.postcode;
                    }
                })
                .catch(function(err) { console.error(err); });
        });
    }
};

window.buscarCP = function(cp, prefix) {
    if (!cp || cp.length !== 5) return;

    var span = document.getElementById('mapText-' + prefix);
    if (span) {
        span.style.display = 'block';
        span.innerText = "Buscando código postal...";
    }

    fetch("https://nominatim.openstreetmap.org/search?postalcode=" + cp + "&country=mx&format=json&addressdetails=1&limit=1")
        .then(function(res) { return res.json(); })
        .then(function(data) {
            if (data && data.length > 0) {
                var ad = data[0].address;

                var colInput = prefix === 'form' ? document.getElementById('Colonia') : document.getElementById('sucColonia');
                var munInput = prefix === 'form' ? document.getElementById('Municipio') : document.getElementById('sucMunicipio');
                var estInput = prefix === 'form' ? document.getElementById('Estado') : document.getElementById('sucEstado');
                var latInput = prefix === 'form' ? document.getElementById('Lat') : document.getElementById('sucLat');
                var lngInput = prefix === 'form' ? document.getElementById('Lng') : document.getElementById('sucLng');

                if (colInput && !colInput.value && (ad.neighbourhood || ad.suburb || ad.village || ad.town)) {
                    colInput.value = ad.neighbourhood || ad.suburb || ad.village || ad.town;
                }
                if (munInput && (ad.city || ad.county || ad.municipality)) {
                    munInput.value = ad.city || ad.county || ad.municipality;
                }
                if (estInput && ad.state) {
                    estInput.value = ad.state;
                }

                var lat = parseFloat(data[0].lat);
                var lng = parseFloat(data[0].lon);

                if (latInput) latInput.value = lat.toFixed(6);
                if (lngInput) lngInput.value = lng.toFixed(6);

                window.inicializarMapaLeaflet(prefix, lat, lng, data[0].display_name);
            } else {
                if (span) span.innerText = "No se encontró el código postal. Ingresa los datos manualmente.";
            }
        })
        .catch(function(err) {
            console.error("Error buscando CP:", err);
            if (span) span.innerText = "Error al conectar con el servidor de mapas.";
        });
};

window.limpiarCalle = function(texto) {
    if (!texto) return '';
    return texto
        .replace(/\bC\.\s*/gi, 'Calle ')
        .replace(/\bAv\.\s*/gi, 'Avenida ')
        .replace(/\bBlvd\.\s*/gi, 'Boulevard ')
        .replace(/\bYuc\.?\s*/gi, 'Yucatán')
        .replace(/\s+/g, ' ')
        .trim();
};

window.verificarDireccion = function(prefix) {
    var calle   = (prefix === 'form' ? document.getElementById('Calle')    : document.getElementById('sucCalle'))?.value    || '';
    var num     = (prefix === 'form' ? document.getElementById('NumExt')   : document.getElementById('sucNumExt'))?.value   || '';
    var col     = (prefix === 'form' ? document.getElementById('Colonia')  : document.getElementById('sucColonia'))?.value  || '';
    var mun     = (prefix === 'form' ? document.getElementById('Municipio'): document.getElementById('sucMunicipio'))?.value|| '';
    var est     = (prefix === 'form' ? document.getElementById('Estado')   : document.getElementById('sucEstado'))?.value   || 'Yucatán';
    var cp      = (prefix === 'form' ? document.getElementById('Cp')       : document.getElementById('sucCp'))?.value       || '';
    var latInput = prefix === 'form' ? document.getElementById('Lat') : document.getElementById('sucLat');
    var lngInput = prefix === 'form' ? document.getElementById('Lng') : document.getElementById('sucLng');

    var span = document.getElementById('mapText-' + prefix);
    if (span) { span.style.display = 'block'; span.innerText = 'Buscando dirección en el mapa...'; }

    var calleLimpia = window.limpiarCalle(calle);

    var qParts = function() { return Array.prototype.slice.call(arguments).filter(function(p){ return p && p.trim(); }).join(', '); };

    var intentos = [
        qParts(calleLimpia + ' ' + num, col, cp + ' ' + mun, est, 'México'),
        qParts(calleLimpia + ' ' + num, col, mun, est, 'México'),
        qParts(calle + ' ' + num, col, mun, est, 'México'),
        qParts(col, mun, cp, est, 'México'),
        qParts(mun, est, 'México')
    ].filter(function(q) { return q.replace(/,\s*/g, '').trim().length > 0; });

    function intentarSiguiente(index) {
        if (index >= intentos.length) {
            if (span) span.innerText = 'No se encontró la dirección exacta. Mueve el marcador para ajustar la ubicación.';
            window.inicializarMapaLeaflet(prefix, 20.9674, -89.6237, 'Mérida');
            return;
        }

        var q = intentos[index];
        var url = 'https://nominatim.openstreetmap.org/search?q=' + encodeURIComponent(q) +
                  '&format=json&limit=1&countrycodes=mx' +
                  '&viewbox=-90.5,21.7,-87.5,19.5&bounded=0';

        fetch(url)
            .then(function(res) { return res.json(); })
            .then(function(data) {
                if (data && data.length > 0) {
                    var lat = parseFloat(data[0].lat);
                    var lng = parseFloat(data[0].lon);
                    if (latInput) latInput.value = lat.toFixed(6);
                    if (lngInput) lngInput.value = lng.toFixed(6);
                    if (span) span.style.display = 'none';
                    window.inicializarMapaLeaflet(prefix, lat, lng, data[0].display_name);
                } else {
                    intentarSiguiente(index + 1);
                }
            })
            .catch(function() { intentarSiguiente(index + 1); });
    }

    intentarSiguiente(0);
};
