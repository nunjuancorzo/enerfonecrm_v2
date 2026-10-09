const enlaces = new Map();

export function enlazar(elemento, referencia, limiteBytes) {
    liberar(elemento.id);
    const contenedor = elemento.closest('[data-imagen-incidencia]') || elemento;
    const entrada = elemento.querySelector('input[type="file"]');
    const estado = { contenedor, entrada, disabled: false, url: null };
    estado.pegar = async evento => {
        if (estado.disabled) return;
        const imagenes = Array.from(evento.clipboardData?.items || [])
            .filter(item => item.kind === 'file' && item.type.startsWith('image/'));
        if (imagenes.length === 0) return;
        evento.preventDefault();
        if (imagenes.length !== 1) {
            await referencia.invokeMethodAsync('ErrorPegado', 'Solo se puede adjuntar una imagen a la incidencia.');
            return;
        }
        const archivo = imagenes[0].getAsFile();
        if (!archivo || archivo.size === 0 || archivo.size > limiteBytes) {
            await referencia.invokeMethodAsync('ErrorPegado', 'La imagen está vacía o supera los 20 MB.');
            return;
        }
        estado.disabled = true;
        try {
            const transferencia = new DataTransfer();
            transferencia.items.add(archivo);
            entrada.files = transferencia.files;
            entrada.dispatchEvent(new Event('change', { bubbles: true }));
        } catch {
            estado.disabled = false;
            await referencia.invokeMethodAsync('ErrorPegado', 'El navegador no permite pegar esta imagen. Cárgala como archivo.');
        }
    };
    contenedor.addEventListener('paste', estado.pegar);
    enlaces.set(elemento.id, estado);
}

export function deshabilitar(id, disabled) {
    const estado = enlaces.get(id);
    if (estado) estado.disabled = disabled;
}

export function vistaPrevia(id) {
    const estado = enlaces.get(id);
    if (!estado) return null;
    if (estado.url) URL.revokeObjectURL(estado.url);
    const archivo = estado.entrada.files[0];
    estado.url = archivo ? URL.createObjectURL(archivo) : null;
    return estado.url;
}

export function limpiar(id) {
    const estado = enlaces.get(id);
    if (!estado) return;
    if (estado.url) URL.revokeObjectURL(estado.url);
    estado.url = null;
    estado.entrada.value = '';
}

export function liberar(id) {
    const estado = enlaces.get(id);
    if (!estado) return;
    estado.contenedor.removeEventListener('paste', estado.pegar);
    if (estado.url) URL.revokeObjectURL(estado.url);
    enlaces.delete(id);
}