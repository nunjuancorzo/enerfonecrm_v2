-- Script para migrar URLs de uploads a storage
-- Fecha: 28/01/2026
-- Descripción: Actualiza las URLs de archivos para usar la nueva estructura storage/ en lugar de wwwroot/

-- Desactivar modo seguro temporalmente
SET SQL_SAFE_UPDATES = 0;

-- Actualizar URLs de mensajes de bienvenida
UPDATE mensajes_bienvenida 
SET imagen_url = REPLACE(imagen_url, '/uploads/mensajes/', '/storage/uploads/mensajes/')
WHERE imagen_url LIKE '/uploads/mensajes/%';

-- Actualizar URLs de PDFs de contratos
UPDATE contratos 
SET pdf_contrato_url = REPLACE(pdf_contrato_url, '/uploads/contratos/', '/storage/uploads/contratos/')
WHERE pdf_contrato_url LIKE '/uploads/contratos/%';

-- Reactivar el modo seguro
SET SQL_SAFE_UPDATES = 1;

-- Verificar cambios
SELECT 'Mensajes de bienvenida actualizados:' as info, COUNT(*) as cantidad 
FROM mensajes_bienvenida 
WHERE imagen_url LIKE '/storage/uploads/mensajes/%';

SELECT 'Contratos con PDF actualizados:' as info, COUNT(*) as cantidad 
FROM contratos 
WHERE pdf_contrato_url LIKE '/storage/uploads/contratos/%';
