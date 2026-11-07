export function guardar(token, recordar) {
    const secure = location.protocol === "https:" ? "; Secure" : "";
    const duration = recordar ? "; Max-Age=2592000" : "";
    document.cookie = `crm_empresa_session=${encodeURIComponent(token)}; Path=/; SameSite=Strict${secure}${duration}`;
}

export function eliminar() {
    document.cookie = "crm_empresa_session=; Path=/; SameSite=Strict; Max-Age=0";
}