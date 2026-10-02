// Sistema de Mapeo Visual de Facturas con PDF.js
let pdfDoc = null;
let pageNum = 1;
let pageRendering = false;
let pageNumPending = null;
let scale = 1.5;
let canvas = null;
let ctx = null;
let originalImage = null; // Para guardar la imagen original

// Variables para dibujar rectángulos
let isDrawing = false;
let startX = 0;
let startY = 0;
let currentField = null;
let rectangles = [];

/**
 * Inicializar y cargar el documento (PDF o imagen)
 */
window.initPdfMapeo = async function (fileUrl, canvasId) {
    console.log('[Mapeo] ========================================');
    console.log('[Mapeo] Iniciando con archivo:', fileUrl);
    console.log('[Mapeo] Canvas ID:', canvasId);
    
    canvas = document.getElementById(canvasId);
    if (!canvas) {
        console.error('[Mapeo] ❌ Canvas no encontrado:', canvasId);
        alert('Error: No se encontró el elemento canvas. ID: ' + canvasId);
        return;
    }
    
    console.log('[Mapeo] ✓ Canvas encontrado:', canvas);

    ctx = canvas.getContext('2d');
    console.log('[Mapeo] ✓ Contexto 2D obtenido');

    try {
        // Detectar si es PDF o imagen por la extensión
        const extension = fileUrl.toLowerCase().split('.').pop().split('?')[0];
        console.log('[Mapeo] Extensión detectada:', extension);
        
        if (extension === 'pdf') {
            console.log('[Mapeo] 📄 Cargando como PDF');
            await cargarPDF(fileUrl);
        } else if (['jpg', 'jpeg', 'png', 'gif', 'webp'].includes(extension)) {
            console.log('[Mapeo] 🖼️ Cargando como imagen');
            await cargarImagen(fileUrl);
        } else {
            console.error('[Mapeo] ❌ Formato de archivo no soportado:', extension);
            alert('Formato de archivo no soportado: ' + extension + '\nUse PDF, JPG, PNG o GIF.');
            return;
        }

        // Configurar eventos del canvas
        canvas.addEventListener('mousedown', onMouseDown);
        canvas.addEventListener('mousemove', onMouseMove);
        canvas.addEventListener('mouseup', onMouseUp);
        
        console.log('[Mapeo] ✓ Eventos configurados');
        console.log('[Mapeo] ✅ Inicialización completada exitosamente');
        console.log('[Mapeo] ========================================');
    } catch (error) {
        console.error('[Mapeo] ❌ Error al cargar archivo:', error);
        console.error('[Mapeo] Error stack:', error.stack);
        alert('Error al cargar el archivo:\n' + error.message + '\n\nRevisa la consola para más detalles.');
    }
};

/**
 * Cargar PDF con PDF.js
 */
async function cargarPDF(pdfUrl) {
    console.log('[PDF] Iniciando carga de PDF:', pdfUrl);
    
    if (typeof pdfjsLib === 'undefined') {
        console.error('[PDF] ❌ PDF.js no está cargado');
        throw new Error('PDF.js no está cargado. Asegúrate de incluir la librería en la página.');
    }
    
    console.log('[PDF] ✓ PDF.js disponible');
    
    const loadingTask = pdfjsLib.getDocument(pdfUrl);
    console.log('[PDF] Tarea de carga creada');
    
    pdfDoc = await loadingTask.promise;
    
    console.log('[PDF] ✓ Documento PDF cargado exitosamente');
    console.log('[PDF] Número de páginas:', pdfDoc.numPages);
    
    // Renderizar primera página
    await renderizarPagina(1);
    console.log('[PDF] ✓ Primera página renderizada');
}

/**
 * Cargar imagen
 */
async function cargarImagen(imageUrl) {
    console.log('[Imagen] Iniciando carga de imagen:', imageUrl);
    return new Promise((resolve, reject) => {
        const img = new Image();
        img.crossOrigin = 'anonymous';
        
        img.onload = function() {
            console.log('[Imagen] ✓ Imagen cargada exitosamente');
            console.log('[Imagen] Dimensiones originales:', img.width, 'x', img.height);
            
            // Guardar la imagen original
            originalImage = img;
            
            // Ajustar canvas al tamaño de la imagen (con escala si es necesario)
            let displayWidth = img.width;
            let displayHeight = img.height;
            
            // Si la imagen es muy grande, escalarla
            const maxWidth = 1200;
            if (displayWidth > maxWidth) {
                const ratio = maxWidth / displayWidth;
                displayWidth = maxWidth;
                displayHeight = img.height * ratio;
                console.log('[Imagen] Imagen escalada a:', displayWidth, 'x', displayHeight);
            }
            
            canvas.width = displayWidth;
            canvas.height = displayHeight;
            console.log('[Imagen] Canvas ajustado a:', canvas.width, 'x', canvas.height);
            
            // Dibujar imagen en el canvas
            ctx.drawImage(img, 0, 0, displayWidth, displayHeight);
            console.log('[Imagen] ✓ Imagen dibujada en canvas');
            
            // Para imágenes, no hay múltiples páginas
            pdfDoc = null; // No es PDF
            pageNum = 1;
            
            resolve();
        };
        
        img.onerror = function(error) {
            console.error('[Imagen] ❌ Error al cargar imagen:', error);
            console.error('[Imagen] URL intentada:', imageUrl);
            reject(new Error('No se pudo cargar la imagen. Verifica que el archivo existe y es accesible.'));
        };
        
        img.src = imageUrl;
        console.log('[Imagen] Solicitud de carga iniciada');
    });
}

/**
 * Renderizar una página específica del PDF (o imagen estática)
 */
window.renderizarPagina = async function (num) {
    // Si es una imagen, redibujarla
    if (!pdfDoc && originalImage) {
        ctx.clearRect(0, 0, canvas.width, canvas.height);
        ctx.drawImage(originalImage, 0, 0, canvas.width, canvas.height);
        redibujarRectangulos();
        return;
    }
    
    // Si es PDF, renderizar la página
    if (!pdfDoc) {
        return;
    }

    pageRendering = true;
    pageNum = num;

    try {
        const page = await pdfDoc.getPage(num);
        const viewport = page.getViewport({ scale: scale });

        canvas.height = viewport.height;
        canvas.width = viewport.width;

        const renderContext = {
            canvasContext: ctx,
            viewport: viewport
        };

        await page.render(renderContext).promise;
        pageRendering = false;

        // Redibujar rectángulos guardados para esta página
        redibujarRectangulos();

        if (pageNumPending !== null) {
            renderizarPagina(pageNumPending);
            pageNumPending = null;
        }
    } catch (error) {
        console.error('[PDF.js] Error al renderizar página:', error);
        pageRendering = false;
    }
};

/**
 * Activar modo selección para un campo
 */
window.activarModoSeleccion = function (field) {
    currentField = field;
    canvas.style.cursor = 'crosshair';
    console.log('[Mapeo] Campo activo:', field);
};

/**
 * Eventos del mouse para dibujar rectángulos
 */
function onMouseDown(e) {
    if (!currentField) return;

    isDrawing = true;
    const rect = canvas.getBoundingClientRect();
    startX = e.clientX - rect.left;
    startY = e.clientY - rect.top;
}

function onMouseMove(e) {
    if (!isDrawing) return;

    const rect = canvas.getBoundingClientRect();
    const currentX = e.clientX - rect.left;
    const currentY = e.clientY - rect.top;

    // Redibujar página
    renderizarPagina(pageNum);

    // Dibujar rectángulo temporal
    ctx.strokeStyle = '#3b82f6';
    ctx.lineWidth = 2;
    ctx.strokeRect(startX, startY, currentX - startX, currentY - startY);
    ctx.fillStyle = 'rgba(59, 130, 246, 0.1)';
    ctx.fillRect(startX, startY, currentX - startX, currentY - startY);
}

function onMouseUp(e) {
    if (!isDrawing || !currentField) return;

    isDrawing = false;

    const rect = canvas.getBoundingClientRect();
    const endX = e.clientX - rect.left;
    const endY = e.clientY - rect.top;

    const width = endX - startX;
    const height = endY - startY;

    // Solo guardar si el rectángulo tiene tamaño mínimo
    if (Math.abs(width) > 10 && Math.abs(height) > 10) {
        // Convertir a coordenadas relativas (0-1)
        const relX = startX / canvas.width;
        const relY = startY / canvas.height;
        const relW = width / canvas.width;
        const relH = height / canvas.height;

        // Eliminar mapeo anterior del mismo campo en esta página
        rectangles = rectangles.filter(r => !(r.field === currentField && r.page === pageNum));

        // Guardar nuevo rectángulo
        rectangles.push({
            field: currentField,
            page: pageNum,
            x: relX,
            y: relY,
            w: relW,
            h: relH
        });

        console.log('[Mapeo] Campo mapeado:', currentField, 'Página:', pageNum);
        console.log('[Mapeo] Coordenadas relativas:', { x: relX, y: relY, w: relW, h: relH });

        // Notificar a Blazor
        if (window.DotNet) {
            DotNet.invokeMethodAsync('EnerfoneCRM', 'OnCampoMapeado', relX, relY, relW, relH);
        }

        // Redibujar con el nuevo rectángulo
        redibujarRectangulos();
    }

    canvas.style.cursor = 'default';
    currentField = null;
}

/**
 * Redibujar todos los rectángulos guardados para la página actual
 */
function redibujarRectangulos() {
    const pageRects = rectangles.filter(r => r.page === pageNum);

    pageRects.forEach(rect => {
        const absX = rect.x * canvas.width;
        const absY = rect.y * canvas.height;
        const absW = rect.w * canvas.width;
        const absH = rect.h * canvas.height;

        ctx.strokeStyle = '#10b981';
        ctx.lineWidth = 2;
        ctx.strokeRect(absX, absY, absW, absH);
        ctx.fillStyle = 'rgba(16, 185, 129, 0.15)';
        ctx.fillRect(absX, absY, absW, absH);

        // Etiqueta del campo
        ctx.fillStyle = '#10b981';
        ctx.font = 'bold 12px Arial';
        ctx.fillText(rect.field.toUpperCase(), absX + 5, absY + 15);
    });
}

/**
 * Limpiar rectángulos de un campo específico
 */
window.limpiarCampo = function (field) {
    rectangles = rectangles.filter(r => r.field !== field);
    redibujarRectangulos();
};

/**
 * Limpiar todos los rectángulos
 */
window.limpiarTodo = function () {
    rectangles = [];
    if (pdfDoc) {
        renderizarPagina(pageNum);
    }
};

/**
 * Obtener total de páginas
 */
window.getTotalPaginas = function () {
    return pdfDoc ? pdfDoc.numPages : 1;
};
