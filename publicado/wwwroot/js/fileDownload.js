// Función para descargar archivos desde base64
window.downloadFile = function (fileName, dataUrl) {
    const link = document.createElement('a');
    link.href = dataUrl;
    link.download = fileName;
    document.body.appendChild(link);
    link.click();
    document.body.removeChild(link);
};

// Función para hacer scroll a un elemento
window.scrollToElement = function (elementId) {
    const element = document.getElementById(elementId);
    if (element) {
        element.scrollIntoView({ behavior: 'smooth', block: 'center' });
    }
};
