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

    function esCorreoValido(email) {
        if (!email) return false;
        var re = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
        return re.test(email);
    }

    function limpiarErroresEn(contenedor) {
        if (contenedor) {
            $(contenedor).find('.form-input-error').removeClass('form-input-error');
        } else {
            $('.form-input-error').removeClass('form-input-error');
        }
    }

    function marcarCampoError(selector) {
        $(selector).addClass('form-input-error');
    }

    window.actualizarMaxLengthRFC = function () {
        var tipo = $('#TipoPersona').val();
        var rfc = $('#Rfc');
        if (!tipo) {
            rfc.prop('readonly', true).attr('placeholder', 'Seleccione tipo de persona');
            $('#rfcHelp').text('Seleccione el tipo de persona para habilitar el RFC.');
            rfc.val('');
            return;
        }
        rfc.prop('readonly', false).attr('placeholder', 'XAXX010101000');
        var longitud = (tipo === 'Física') ? 13 : 12;
        rfc.attr('maxlength', longitud);
        var val = rfc.val();
        if (val && val.length > longitud) {
            rfc.val(val.substring(0, longitud));
        }
        $('#rfcHelp').text('Persona ' + tipo + ': ' + longitud + ' caracteres.');
    };

    function validarRFC(tipoPersona, rfc) {
        if (!rfc) return false;
        var longitudEsperada = (tipoPersona === 'Física') ? 13 : 12;
        return rfc.length === longitudEsperada;
    }

    function obtenerMensajeRFC(tipoPersona) {
        var longitud = (tipoPersona === 'Física') ? 13 : 12;
        return 'RFC debe tener ' + longitud + ' caracteres para persona ' + ((tipoPersona || 'Moral').toLowerCase());
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

    function obtenerErroresSeccionContactos() {
        var errores = [];
        if (contactos.length === 0) {
            errores.push({ campo: '#btnAgregarContacto', mensaje: 'Al menos un contacto en DATOS DE CONTACTO' });
            return errores;
        }
        for (var i = 0; i < contactos.length; i++) {
            var c = contactos[i];
            if (!c.NombreContacto || !c.NombreContacto.trim())
                errores.push({ campo: '#btnAgregarContacto', mensaje: 'Contacto #' + (i + 1) + ': Nombre completo' });
            if (!c.Correo || !c.Correo.trim())
                errores.push({ campo: '#btnAgregarContacto', mensaje: 'Contacto #' + (i + 1) + ': Correo electrónico (requerido)' });
            else if (!esCorreoValido(c.Correo))
                errores.push({ campo: '#btnAgregarContacto', mensaje: 'Contacto #' + (i + 1) + ': Correo electrónico no válido' });
            if (!c.Telefono || !c.Telefono.trim())
                errores.push({ campo: '#btnAgregarContacto', mensaje: 'Contacto #' + (i + 1) + ': Teléfono' });
        }
        return errores;
    }

    function obtenerErroresSeccionSucursales() {
        var errores = [];
        if (sucursales.length === 0) {
            errores.push({ campo: '#btnAgregarSucursal', mensaje: 'Al menos una sucursal en GESTIÓN DE SUCURSALES' });
            return errores;
        }
        for (var i = 0; i < sucursales.length; i++) {
            var s = sucursales[i];
            var base = 'Sucursal #' + (i + 1) + ': ';
            if (!s.NombreSucursal || !s.NombreSucursal.trim()) errores.push({ campo: '#btnAgregarSucursal', mensaje: base + 'Nombre de Sucursal' });
            if (!s.TelefonoSucursal || !s.TelefonoSucursal.trim()) errores.push({ campo: '#btnAgregarSucursal', mensaje: base + 'Teléfono' });
            else if (s.TelefonoSucursal.replace(/\D/g, '').length !== 10) errores.push({ campo: '#btnAgregarSucursal', mensaje: base + 'El teléfono debe tener exactamente 10 dígitos' });
            if (!s.CorreoElectronico || !s.CorreoElectronico.trim()) errores.push({ campo: '#btnAgregarSucursal', mensaje: base + 'Correo electrónico (requerido)' });
            else if (!esCorreoValido(s.CorreoElectronico)) errores.push({ campo: '#btnAgregarSucursal', mensaje: base + 'Correo electrónico no válido' });
            if (!s.NombreResponsable || !s.NombreResponsable.trim()) errores.push({ campo: '#btnAgregarSucursal', mensaje: base + 'Nombre del responsable' });
            if (!s.Calle || !s.Calle.trim()) errores.push({ campo: '#btnAgregarSucursal', mensaje: base + 'Calle' });
            if (!s.NumExt || !s.NumExt.trim()) errores.push({ campo: '#btnAgregarSucursal', mensaje: base + 'Número Exterior' });
            if (!s.Colonia || !s.Colonia.trim()) errores.push({ campo: '#btnAgregarSucursal', mensaje: base + 'Colonia' });
            if (!s.Municipio || !s.Municipio.trim()) errores.push({ campo: '#btnAgregarSucursal', mensaje: base + 'Municipio' });
            if (!s.Cp || !s.Cp.trim()) errores.push({ campo: '#btnAgregarSucursal', mensaje: base + 'Código Postal' });
            if (!s.Estado || !s.Estado.trim()) errores.push({ campo: '#btnAgregarSucursal', mensaje: base + 'Estado' });
            if (!s.FotoFachada || !s.FotoFachada.trim()) errores.push({ campo: '#btnAgregarSucursal', mensaje: base + 'Foto 1 - Fachada' });
            if (!s.FotoAcceso || !s.FotoAcceso.trim()) errores.push({ campo: '#btnAgregarSucursal', mensaje: base + 'Foto 2 - Acceso' });
            if (!s.FotoReferencia || !s.FotoReferencia.trim()) errores.push({ campo: '#btnAgregarSucursal', mensaje: base + 'Foto 3 - Referencia' });
        }
        return errores;
    }

    function obtenerErroresTabProspecto(idx) {
        var errores = [];
        if (idx === 0) {
            var nombre = $('#Nombre').val().trim();
            var tipoPersona = $('#TipoPersona').val();
            var rfc = $('#Rfc').val().trim();
            var nombreComercial = $('#NombreComercial').val().trim();
            var contacto = $('#Contacto').val().trim();
            var telefono = $('#Telefono').val().trim();
            var email = $('#Email').val().trim();

            if (!tipoPersona) errores.push({ campo: '#TipoPersona', mensaje: 'Tipo de persona' });
            if (!nombre) errores.push({ campo: '#Nombre', mensaje: 'Razón Social / Nombre completo' });
            if (!rfc) errores.push({ campo: '#Rfc', mensaje: 'RFC (requerido)' });
            else if (tipoPersona && !validarRFC(tipoPersona, rfc)) errores.push({ campo: '#Rfc', mensaje: obtenerMensajeRFC(tipoPersona) });
            if (!nombreComercial) errores.push({ campo: '#NombreComercial', mensaje: 'Nombre Comercial' });

            var tieneSucursales = $('#TieneSucursales').val();
            if (!tieneSucursales) errores.push({ campo: '#TieneSucursales', mensaje: '¿Tiene sucursales?' });

            if (!contacto) errores.push({ campo: '#Contacto', mensaje: 'Nombre completo de contacto' });
            if (!telefono) errores.push({ campo: '#Telefono', mensaje: 'Teléfono de contacto' });
            else if (telefono.length !== 10) errores.push({ campo: '#Telefono', mensaje: 'El teléfono debe tener exactamente 10 dígitos' });
            if (!email) errores.push({ campo: '#Email', mensaje: 'Correo electrónico (requerido)' });
            else if (!esCorreoValido(email)) errores.push({ campo: '#Email', mensaje: 'Correo electrónico no válido' });

            if (window.requiereVendedor) {
                var vendedorId = $('#VendedorId').val();
                if (!vendedorId || vendedorId === '' || vendedorId === '0') {
                    errores.push({ campo: '#VendedorId', mensaje: 'Asignar vendedor' });
                }
            }
        } else if (idx === 1) {
            if (!$('#Calle').val().trim()) errores.push({ campo: '#Calle', mensaje: 'Calle' });
            if (!$('#NumExt').val().trim()) errores.push({ campo: '#NumExt', mensaje: 'Número Exterior' });
            if (!$('#Colonia').val().trim()) errores.push({ campo: '#Colonia', mensaje: 'Colonia' });
            if (!$('#Municipio').val().trim()) errores.push({ campo: '#Municipio', mensaje: 'Municipio' });
            if (!$('#Cp').val().trim()) errores.push({ campo: '#Cp', mensaje: 'Código Postal' });
            if (!$('#Estado').val().trim()) errores.push({ campo: '#Estado', mensaje: 'Estado' });
        } else if (idx === 2) {
            if (!$('#hdnFotoFachada').val().trim()) errores.push({ campo: '#fotoSlot1', mensaje: 'Foto 1 - Fachada' });
            if (!$('#hdnFotoAcceso').val().trim()) errores.push({ campo: '#fotoSlot2', mensaje: 'Foto 2 - Acceso' });
            if (!$('#hdnFotoReferencia').val().trim()) errores.push({ campo: '#fotoSlot3', mensaje: 'Foto 3 - Referencia' });
            // Folio y documento catastral son OPCIONALES en matriz.
        }
        return errores;
    }

    function validarTabProspecto(idx) {
        limpiarErroresEn('#tab-' + tabsProspecto[idx]);
        var errores = obtenerErroresTabProspecto(idx);
        if (errores.length > 0) {
            errores.forEach(function (e) { marcarCampoError(e.campo); });
            var nombresTabs = { 0: 'Datos Personales', 1: 'Dirección', 2: 'Fotos' };
            var mensajes = errores.map(function (e) { return '&bull; <b>' + e.mensaje + '</b>'; });
            Swal.fire({
                icon: 'warning',
                title: 'Campos requeridos',
                html: 'Por favor completa los siguientes campos en <b>' + nombresTabs[idx] + '</b> antes de continuar:<br><br>' + mensajes.join('<br>'),
                confirmButtonColor: '#7b3f1a',
                didClose: function () {
                    errores.forEach(function (e) { marcarCampoError(e.campo); });
                }
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
                $('#seccionVendedorDetalle').show();
                $('#seccionEstadoDetalle').show();
                // La sección de COTIZACIÓN se oculta; su información se muestra dentro de ESTATUS.
                $('#seccionCotizacionDetalle').hide();
                var estatusProspecto = ($('#hdnEstatusProspecto').val() || '').toLowerCase();
                if ($('#motivoRechazoDetalle').length && (estatusProspecto === 'rechazado' || estatusProspecto === 'rechazada')) {
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
            actualizarMaxLengthRFC();

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
        limpiarErroresEn('#modalContacto');
        var nombre = $('#contactoNombre').val().trim();
        var correo = $('#contactoCorreo').val().trim();
        var telefono = $('#contactoTelefono').val().trim();
        var errores = [];
        if (!nombre) errores.push({ campo: '#contactoNombre', mensaje: 'Nombre completo' });
        if (!correo) errores.push({ campo: '#contactoCorreo', mensaje: 'Correo electrónico (requerido)' });
        else if (!esCorreoValido(correo)) errores.push({ campo: '#contactoCorreo', mensaje: 'Correo electrónico no válido' });
        if (!telefono) errores.push({ campo: '#contactoTelefono', mensaje: 'Teléfono' });
        else if (telefono.replace(/\D/g, '').length !== 10) errores.push({ campo: '#contactoTelefono', mensaje: 'El teléfono debe tener exactamente 10 dígitos' });
        if (errores.length > 0) {
            errores.forEach(function (e) { marcarCampoError(e.campo); });
            var mensajes = errores.map(function (e) { return '&bull; <b>' + e.mensaje + '</b>'; });
            Swal.fire({
                icon: 'warning', title: 'Campos requeridos',
                html: 'Por favor completa los siguientes campos en el contacto:<br><br>' + mensajes.join('<br>'),
                confirmButtonColor: '#7b3f1a',
                didClose: function () {
                    errores.forEach(function (e) { marcarCampoError(e.campo); });
                }
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
        // El botón Guardar solo se muestra en FOTOS Y ARCHIVOS (último tab).
        var btnGuardar = document.getElementById('btnGuardarSucursal');
        var footerGuardar = btnGuardar ? btnGuardar.closest('#modalSucursal > div > div:last-child > div:last-child') : null;
        if (btnGuardar) {
            var estaEnFotos = tabIdxSuc === tabsSuc.length - 1;
            // El botón físico está ahora dentro del tab fotos; el del footer ya no existe.
            btnGuardar.style.display = estaEnFotos ? 'inline-block' : 'none';
        }
    }

    function obtenerErroresTabSuc(idx) {
        var errores = [];
        if (idx === 0) {
            var nombre = document.getElementById('sucNombre');
            if (nombre && !nombre.value.trim()) errores.push({ campo: '#sucNombre', mensaje: 'Nombre de Sucursal' });
            var tel = document.getElementById('sucTelefono');
            if (tel && !tel.value.trim()) errores.push({ campo: '#sucTelefono', mensaje: 'Teléfono' });
            else if (tel && tel.value.replace(/\D/g, '').length !== 10) errores.push({ campo: '#sucTelefono', mensaje: 'El teléfono debe tener exactamente 10 dígitos' });
            var correo = $('#sucCorreo').val().trim();
            if (!correo) errores.push({ campo: '#sucCorreo', mensaje: 'Correo electrónico (requerido)' });
            else if (!esCorreoValido(correo)) errores.push({ campo: '#sucCorreo', mensaje: 'Correo electrónico no válido' });
            var responsable = document.getElementById('sucResponsable');
            if (responsable && !responsable.value.trim()) errores.push({ campo: '#sucResponsable', mensaje: 'Nombre del responsable' });
        } else if (idx === 1) {
            if (!$('#sucCalle').val().trim()) errores.push({ campo: '#sucCalle', mensaje: 'Calle' });
            if (!$('#sucNumExt').val().trim()) errores.push({ campo: '#sucNumExt', mensaje: 'Número Exterior' });
            if (!$('#sucColonia').val().trim()) errores.push({ campo: '#sucColonia', mensaje: 'Colonia' });
            if (!$('#sucMunicipio').val().trim()) errores.push({ campo: '#sucMunicipio', mensaje: 'Municipio' });
            if (!$('#sucCp').val().trim()) errores.push({ campo: '#sucCp', mensaje: 'Código Postal' });
            if (!$('#sucEstado').val().trim()) errores.push({ campo: '#sucEstado', mensaje: 'Estado' });
        } else if (idx === 2) {
            if (!$('#hdnSucFotoFachada').val().trim()) errores.push({ campo: '#sucFotoSlot1', mensaje: 'Foto 1 - Fachada' });
            if (!$('#hdnSucFotoAcceso').val().trim()) errores.push({ campo: '#sucFotoSlot2', mensaje: 'Foto 2 - Acceso' });
            if (!$('#hdnSucFotoReferencia').val().trim()) errores.push({ campo: '#sucFotoSlot3', mensaje: 'Foto 3 - Referencia' });
            // Folio y documento catastral son OPCIONALES en sucursal.
        }
        return errores;
    }

    function validarTabSuc(idx) {
        limpiarErroresEn('#tab-suc-' + tabsSuc[idx]);
        var errores = obtenerErroresTabSuc(idx);
        if (errores.length > 0) {
            errores.forEach(function (e) { marcarCampoError(e.campo); });
            var mensajes = errores.map(function (e) { return '&bull; <b>' + e.mensaje + '</b>'; });
            Swal.fire({
                icon: 'warning',
                title: 'Campos requeridos',
                html: 'Por favor completa:<br><br>' + mensajes.join('<br>'),
                confirmButtonColor: '#7b3f1a',
                didClose: function () {
                    errores.forEach(function (e) { marcarCampoError(e.campo); });
                }
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
            'sucColonia', 'sucCp', 'sucMunicipio', 'sucEstado', 'sucReferencias', 'sucFolio']
            .forEach(function (id) {
                var el = document.getElementById(id);
                if (el) el.value = el.id === 'sucEstado' ? 'Yucatán' : '';
            });
        $('#hdnSucFotoFachada, #hdnSucFotoAcceso, #hdnSucFotoReferencia, #hdnSucDocCatastral, #hdnSucDocCatastralNombre').val('');
        $('#sucFotoPreview1, #sucFotoPreview2, #sucFotoPreview3').hide().attr('src', '#');
        $('#sucFotoPlaceholder1, #sucFotoPlaceholder2, #sucFotoPlaceholder3').show();
        $('#sucDocCatastralNombre').hide();
        $('#sucDocCatastralPlaceholder').show();
        $('#modalSucursal input, #modalSucursal select, #modalSucursal textarea').prop('readonly', false).prop('disabled', false);
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
        limpiarErroresEn('#modalSucursal');
        var nombresTabsSuc = { 0: 'DATOS DE CONTACTO', 1: 'DIRECCIÓN', 2: 'FOTOS Y ARCHIVOS' };
        var todosErrores = [];
        for (var i = 0; i < tabsSuc.length; i++) {
            var err = obtenerErroresTabSuc(i);
            if (err.length > 0) {
                err.forEach(function (e) {
                    e.tab = i;
                    marcarCampoError(e.campo);
                    todosErrores.push(e);
                });
            }
        }

        if (todosErrores.length > 0) {
            var primeraTab = Math.min.apply(null, todosErrores.map(function (e) { return e.tab; }));
            var tabItems = document.querySelectorAll('#modalSucursal .tab-item-suc');
            showTabSuc(tabsSuc[primeraTab], tabItems[primeraTab]);

            var mensajes = todosErrores.map(function (e) {
                return '&bull; <b>' + e.mensaje + '</b> <span style="color:var(--text3);font-size:12px;">(' + nombresTabsSuc[e.tab] + ')</span>';
            });
            Swal.fire({
                icon: 'warning',
                title: 'Campos requeridos',
                html: 'Completa los siguientes datos obligatorios:<br><br>' + mensajes.join('<br>'),
                confirmButtonColor: '#7b3f1a',
                didClose: function () {
                    todosErrores.forEach(function (e) { marcarCampoError(e.campo); });
                }
            });
            return;
        }
        var nombre = $('#sucNombre').val().trim();
        var telefono = $('#sucTelefono').val().trim();
        var correo = $('#sucCorreo').val().trim();
        var responsable = $('#sucResponsable').val().trim();

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
        $('#sucEstado').val(s.Estado || 'Yucatán');
        $('#sucLat').val(s.Lat);
        $('#sucLng').val(s.Lng);
        $('#sucFolio').val(s.FolioCatastral);
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
        $('.modal-sucursal-titulo').text('EDITAR SUCURSAL');

        tabIdxSuc = 0;
        var tabItemsEdit = document.querySelectorAll('#modalSucursal .tab-item-suc');
        if (tabItemsEdit.length > 0) {
            tabItemsEdit.forEach(function (t) {
                t.classList.remove('active');
                t.style.color = 'var(--text3)';
                t.style.fontWeight = '600';
                t.style.borderBottomColor = 'transparent';
            });
            tabItemsEdit[0].classList.add('active');
        }
        showTabSuc('contacto', tabItemsEdit.length > 0 ? tabItemsEdit[0] : null);

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
        $('#sucEstado').val(s.Estado || 'Yucatán');
        $('#sucLat').val(s.Lat);
        $('#sucLng').val(s.Lng);
        $('#sucFolio').val(s.FolioCatastral);
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

    function generarNombreUnico(nombreOriginal) {
        var ts = Date.now();
        var ext = '';
        var base = nombreOriginal || 'archivo';
        var idx = base.lastIndexOf('.');
        if (idx > -1) {
            ext = base.substring(idx);
            base = base.substring(0, idx);
        }
        return base + '_' + ts + ext;
    }

    window.previewDoc = function (input, nombreId, placeholderId, nombreTextoId, slotId, hiddenDocId, hiddenNameId) {
        if (input.files && input.files[0]) {
            var reader = new FileReader();
            var nombre = generarNombreUnico(input.files[0].name);
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

        // Ajustar RFC según tipo de persona
        $(document).on('change', '#TipoPersona', function () {
            actualizarMaxLengthRFC();
        });

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

        // Quitar marca de error cuando el usuario empieza a corregir un campo
        $(document).on('input change', '.form-input-error', function () {
            $(this).removeClass('form-input-error');
        });

        $(document).on('click', '[data-accion]', function () {
            var accion = $(this).data('accion');
            var id = $(this).data('id');
            if (accion === 'rechazar') accionRechazar(id);
            else if (accion === 'asignar') accionAsignarVendedor(id);
            else if (accion === 'baja') accionDarDeBaja(id);
        });

        // Bandera para evitar doble submit del formulario de prospecto.
        var enviandoProspecto = false;

        function setEnviandoProspecto(activo) {
            enviandoProspecto = activo;
            var $btn = $('#btnProspectoGuardar');
            var $form = $('#frmProspecto');
            if (activo) {
                $btn.prop('disabled', true).html('<i class="fas fa-spinner fa-spin"></i> Guardando...');
                $form.addClass('enviando-prospecto');
            } else {
                $btn.prop('disabled', false).html('Guardar');
                $form.removeClass('enviando-prospecto');
            }
        }

        // Envío del formulario de prospecto (AJAX en modal, normal en página completa).
        $(document).on('submit', '#frmProspecto', function (e) {
            var $form = $(this);
            var esModal = $form.closest('#modalProspectoContainer').length > 0;
            var esNuevo = ($form.attr('action') || '').toLowerCase().indexOf('nuevo') !== -1;

            // Evitar doble submit: si ya se está enviando, no hacer nada.
            if (enviandoProspecto) {
                e.preventDefault();
                return false;
            }

            // Validación completa de todas las pestañas y secciones
            var nombresTabs = { 0: 'Datos Personales', 1: 'Dirección', 2: 'Fotos' };
            limpiarErroresEn('#modalFormularioProspecto');
            var todosErrores = [];
            for (var i = 0; i < tabsProspecto.length; i++) {
                var err = obtenerErroresTabProspecto(i);
                if (err.length > 0) {
                    err.forEach(function (e) {
                        e.tab = i;
                        marcarCampoError(e.campo);
                        todosErrores.push(e);
                    });
                }
            }

            // Validar DATOS DE CONTACTO (sección de contactos)
            var errContactos = obtenerErroresSeccionContactos();
            if (errContactos.length > 0) {
                errContactos.forEach(function (e) {
                    e.tab = 0;
                    marcarCampoError(e.campo);
                    todosErrores.push(e);
                });
            }

            // Validar GESTIÓN DE SUCURSALES solo si el prospecto indica que tiene sucursales
            var tieneSuc = ($('#TieneSucursales').val() || '').toString().toLowerCase();
            if (tieneSuc === 's&iacute;' || tieneSuc === 'si' || tieneSuc === 'sí' || tieneSuc === 'yes' || tieneSuc === 's') {
                var errSuc = obtenerErroresSeccionSucursales();
                if (errSuc.length > 0) {
                    errSuc.forEach(function (e) {
                        e.tab = 2;
                        marcarCampoError(e.campo);
                        todosErrores.push(e);
                    });
                }
            }

            if (todosErrores.length > 0) {
                var primeraTab = Math.min.apply(null, todosErrores.map(function (e) { return e.tab; }));
                var tabItems = document.querySelectorAll('#modalFormularioProspecto .tab-item');
                showTab(tabsProspecto[primeraTab], tabItems[primeraTab]);

                var mensajes = todosErrores.map(function (e) {
                    return '&bull; <b>' + e.mensaje + '</b> <span style="color:var(--text3);font-size:12px;">(' + nombresTabs[e.tab] + ')</span>';
                });

                Swal.fire({
                    icon: 'warning',
                    title: 'Campos requeridos',
                    html: 'Por favor corrige los siguientes errores antes de guardar:<br><br>' + mensajes.join('<br>'),
                    confirmButtonColor: '#7b3f1a',
                    didClose: function () {
                        todosErrores.forEach(function (e) { marcarCampoError(e.campo); });
                    }
                });
                return false;
            }

            if (!esModal) {
                setEnviandoProspecto(true); // protección también en página completa
                return true; // permitir envío normal en la página completa
            }

            e.preventDefault();
            setEnviandoProspecto(true);

            var formData = new FormData(this);

            $.ajax({
                url: $form.attr('action'),
                type: 'POST',
                data: formData,
                processData: false,
                contentType: false,
                success: function (res) {
                    setEnviandoProspecto(false);
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
                    setEnviandoProspecto(false);
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
