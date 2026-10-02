// Manejar submenús multinivel en la barra de navegación
(function() {
    'use strict';

    // Esperar a que el DOM esté completamente cargado
    function init() {
        console.log('NavMenu script cargado y listo');
        
        // Función para cerrar el menú móvil
        function cerrarMenuMovil() {
            const navbarToggler = document.querySelector('.navbar-toggler');
            const navbarCollapse = document.querySelector('.navbar-collapse');
            
            if (navbarCollapse && navbarCollapse.classList.contains('show')) {
                // Cerrar el menú móvil
                navbarCollapse.classList.remove('show');
                if (navbarToggler) {
                    navbarToggler.classList.add('collapsed');
                    navbarToggler.setAttribute('aria-expanded', 'false');
                }
            }
        }
        
        // Capturar eventos en fase de captura para tener prioridad sobre Bootstrap
        document.addEventListener('click', function(e) {
            let target = e.target;
            
            // Verificar si el clic fue en un nav-link directo (como "Inicio") o en el botón de incidencias
            const clickedNavLink = target.closest('.nav-link');
            const clickedIncidencias = target.closest('.btn-incidencia');
            
            if ((clickedNavLink && !clickedNavLink.classList.contains('dropdown-toggle')) || clickedIncidencias) {
                // Es un enlace directo de navegación (como "Inicio" o "Incidencias")
                console.log('Click en nav-link directo o incidencias, cerrar menú móvil');
                setTimeout(cerrarMenuMovil, 100);
                return;
            }
            
            // Primero verificar si el clic fue en un enlace dentro de un dropdown-menu del submenú
            // Si es así, dejar que funcione normalmente (no capturar el evento)
            const clickedLink = target.closest('.dropdown-submenu > .dropdown-menu a.dropdown-item');
            if (clickedLink && !clickedLink.classList.contains('dropdown-toggle')) {
                // Es un enlace normal dentro del submenú, dejar que navegue
                console.log('Click en enlace del submenú, permitir navegación y cerrar menú móvil');
                // Cerrar el menú móvil después de un pequeño delay para permitir la navegación
                setTimeout(cerrarMenuMovil, 100);
                return;
            }
            
            // Verificar si el clic fue en un enlace normal del dropdown (no submenú)
            const clickedDropdownLink = target.closest('.dropdown-menu > li > a.dropdown-item');
            if (clickedDropdownLink && !clickedDropdownLink.classList.contains('dropdown-toggle')) {
                // Es un enlace normal del dropdown, cerrar el menú móvil
                console.log('Click en enlace del dropdown, cerrar menú móvil');
                setTimeout(cerrarMenuMovil, 100);
                return;
            }
            
            // Buscar si el clic fue en un dropdown-toggle dentro de un dropdown-submenu
            // Subir en el DOM hasta encontrar un dropdown-toggle
            while (target && target !== document) {
                if (target.classList && target.classList.contains('dropdown-toggle')) {
                    const parent = target.parentElement;
                    if (parent && parent.classList.contains('dropdown-submenu')) {
                        // Este clic es para el toggle del submenú
                        e.preventDefault();
                        e.stopPropagation();
                        e.stopImmediatePropagation();
                        
                        const submenu = target.nextElementSibling;
                        if (submenu && submenu.classList.contains('dropdown-menu')) {
                            // Cerrar otros submenús hermanos
                            const grandparent = parent.parentElement;
                            const siblings = grandparent.querySelectorAll('.dropdown-submenu > .dropdown-menu.show');
                            siblings.forEach(function(sibling) {
                                if (sibling !== submenu) {
                                    sibling.classList.remove('show');
                                }
                            });
                            
                            // Alternar el submenú actual
                            const isOpen = submenu.classList.contains('show');
                            submenu.classList.toggle('show');
                            console.log('Submenú toggled, ahora está:', !isOpen ? 'abierto' : 'cerrado');
                        }
                        return false;
                    }
                }
                target = target.parentElement;
            }
            
            // Si el clic fue fuera de cualquier dropdown-submenu, cerrar todos los submenús
            if (!e.target.closest('.dropdown-submenu')) {
                const openSubmenus = document.querySelectorAll('.dropdown-submenu > .dropdown-menu.show');
                openSubmenus.forEach(function(menu) {
                    menu.classList.remove('show');
                });
            }
        }, true); // true = fase de captura
    }
    
    // Ejecutar cuando el DOM esté listo
    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', init);
    } else {
        init();
    }
})();
