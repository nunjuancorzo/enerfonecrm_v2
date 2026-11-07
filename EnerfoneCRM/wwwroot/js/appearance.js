const empresaActivaKey = "volti.apariencia.empresa";
const modoSesionKey = "volti.apariencia.modo";
const clavePaletaEmpresa = empresaId => `volti.apariencia.${empresaId}`;

function aplicarPaletaEnDocumento(paleta) {
        if (!paleta) return;

        const variables = {
            "--crm-accent": paleta.acento,
            "--crm-accent-rgb": paleta.acentoRgb,
            "--crm-sidebar": paleta.barraLateral,
            "--crm-sidebar-text": paleta.textoBarra,
            "--crm-canvas": paleta.fondo,
            "--crm-on-canvas": paleta.textoFondo,
            "--crm-on-accent": paleta.textoAccion
        };
        const contenedores = [
            document.documentElement,
            document.body,
            ...document.querySelectorAll(".app-shell, .app-empty-shell")
        ];

        for (const contenedor of contenedores) {
            contenedor.dataset.theme = paleta.id;
            for (const [nombre, valor] of Object.entries(variables)) {
                contenedor.style.setProperty(nombre, valor);
            }
        }
}

function limpiarEstilosPaleta() {
    const nombres = ["--crm-accent", "--crm-accent-rgb", "--crm-sidebar", "--crm-sidebar-text", "--crm-canvas", "--crm-on-canvas", "--crm-on-accent"];
    const contenedores = [document.documentElement, document.body, ...document.querySelectorAll(".app-shell, .app-empty-shell")].filter(Boolean);
    for (const contenedor of contenedores) {
        delete contenedor.dataset.theme;
        for (const nombre of nombres) contenedor.style.removeProperty(nombre);
    }
}

window.voltiaApariencia = {
    aplicar: function (paleta, empresaId, persistir) {
        aplicarPaletaEnDocumento(paleta);
        const id = Number(empresaId);
        if (!persistir || !Number.isInteger(id) || id <= 0) return;
        try {
            sessionStorage.setItem(modoSesionKey, "empresa");
            sessionStorage.setItem(empresaActivaKey, String(id));
            sessionStorage.setItem(clavePaletaEmpresa(id), JSON.stringify(paleta));
        } catch { }
    },
    inicializar: function () {
        try {
            const modo = sessionStorage.getItem(modoSesionKey);
            if (modo !== "empresa") {
                limpiarEstilosPaleta();
                return;
            }
            const empresaId = Number(sessionStorage.getItem(empresaActivaKey));
            if (!Number.isInteger(empresaId) || empresaId <= 0) return;
            const paleta = JSON.parse(sessionStorage.getItem(clavePaletaEmpresa(empresaId)) || "null");
            aplicarPaletaEnDocumento(paleta);
        } catch { }
    },
    seleccionarEmpresa: function (empresaId) {
        const id = Number(empresaId);
        if (!Number.isInteger(id) || id <= 0) return;
        try {
            sessionStorage.setItem(empresaActivaKey, String(id));
            const paleta = JSON.parse(sessionStorage.getItem(clavePaletaEmpresa(id)) || "null");
            if (paleta) aplicarPaletaEnDocumento(paleta);
            else limpiarEstilosPaleta();
        } catch { }
    },
    establecerModo: function (modo) {
        if (modo !== "empresa" && modo !== "superadmin") return;
        try {
            sessionStorage.setItem(modoSesionKey, modo);
            if (modo === "superadmin") sessionStorage.removeItem(empresaActivaKey);
        } catch { }
        if (modo === "superadmin") limpiarEstilosPaleta();
    },
    limpiar: function () {
        try {
            sessionStorage.removeItem(empresaActivaKey);
            sessionStorage.removeItem(modoSesionKey);
        } catch { }
        limpiarEstilosPaleta();
    }
};

window.voltiaApariencia.inicializar();
document.addEventListener("enhancedload", () => window.voltiaApariencia.inicializar());

function shellNecesitaTemaCacheado() {
    try {
        const empresaId = Number(sessionStorage.getItem(empresaActivaKey));
        if (!Number.isInteger(empresaId) || empresaId <= 0) return false;
        const paleta = JSON.parse(sessionStorage.getItem(clavePaletaEmpresa(empresaId)) || "null");
        const shell = document.querySelector(".app-shell, .app-empty-shell");
        return !!paleta && !!shell &&
            (shell.dataset.theme !== paleta.id || shell.style.getPropertyValue("--crm-sidebar") !== paleta.barraLateral);
    } catch {
        return false;
    }
}

const observadorShell = new MutationObserver(() => {
    if (!shellNecesitaTemaCacheado()) return;
    requestAnimationFrame(() => {
        if (shellNecesitaTemaCacheado()) window.voltiaApariencia.inicializar();
    });
});

observadorShell.observe(document.documentElement, { childList: true, subtree: true });