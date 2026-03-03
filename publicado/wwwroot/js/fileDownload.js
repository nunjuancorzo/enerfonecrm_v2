// Función para descargar archivos desde base64
console.log('fileDownload.js cargado correctamente');

window.downloadFile = function (fileName, dataUrl) {
    const link = document.createElement('a');
    link.href = dataUrl;
    link.download = fileName;
    document.body.appendChild(link);
    link.click();
    document.body.removeChild(link);
};

// Función para descargar archivos desde base64 (alias para compatibilidad)
window.descargarArchivo = function (fileName, base64Data, mimeType = 'application/pdf') {
    // Auto-detectar tipo MIME si no se proporciona
    if (!mimeType) {
        if (fileName.endsWith('.pdf')) {
            mimeType = 'application/pdf';
        } else if (fileName.endsWith('.xlsx') || fileName.endsWith('.xls')) {
            mimeType = 'application/vnd.openxmlformats-officedocument.spreadsheetml.sheet';
        } else if (fileName.endsWith('.csv')) {
            mimeType = 'text/csv';
        } else {
            mimeType = 'application/octet-stream';
        }
    }
    
    const byteCharacters = atob(base64Data);
    const byteNumbers = new Array(byteCharacters.length);
    for (let i = 0; i < byteCharacters.length; i++) {
        byteNumbers[i] = byteCharacters.charCodeAt(i);
    }
    const byteArray = new Uint8Array(byteNumbers);
    const blob = new Blob([byteArray], { type: mimeType });
    const url = URL.createObjectURL(blob);
    const link = document.createElement('a');
    link.href = url;
    link.download = fileName;
    document.body.appendChild(link);
    link.click();
    document.body.removeChild(link);
    URL.revokeObjectURL(url);
};

// Función para hacer scroll a un elemento
window.scrollToElement = function (elementId) {
    const element = document.getElementById(elementId);
    if (element) {
        element.scrollIntoView({ behavior: 'smooth', block: 'center' });
    }
};
