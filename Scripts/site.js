// Utilidades JavaScript compartidas del CRM
(function () {
    'use strict';

    window.CRMSistema = window.CRMSistema || {};

    // Abrir un modal de Bootstrap 5 por ID
    CRMSistema.abrirModal = function (id) {
        var el = document.getElementById(id);
        if (!el) return;
        var modal = bootstrap.Modal.getInstance(el) || new bootstrap.Modal(el);
        modal.show();
    };

    // Cerrar un modal de Bootstrap 5 por ID
    CRMSistema.cerrarModal = function (id) {
        var el = document.getElementById(id);
        if (!el) return;
        var modal = bootstrap.Modal.getInstance(el);
        if (modal) modal.hide();
    };

})();
