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

    // Toggle sidebar en móvil
    document.addEventListener('DOMContentLoaded', function () {
        var btn = document.getElementById('btnToggleSidebar');
        var sidebar = document.getElementById('sidebar');
        var overlay = document.getElementById('sidebarOverlay');

        if (btn && sidebar) {
            btn.addEventListener('click', function () {
                sidebar.classList.add('sidebar-open');
                if (overlay) overlay.classList.add('active');
            });
        }

        if (overlay && sidebar) {
            overlay.addEventListener('click', function () {
                sidebar.classList.remove('sidebar-open');
                overlay.classList.remove('active');
            });
        }
    });
})();
