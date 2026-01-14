-- Script para corregir valores NULL en campos requeridos (no nullables)
-- Ejecutar en base de datos: enerfonecrm

SET SQL_SAFE_UPDATES = 0;

-- Actualizar campos Tipo y Estado que no pueden ser NULL
UPDATE contratos 
SET 
    tipo = COALESCE(tipo, 'energia'),
    estado = COALESCE(estado, 'Pendiente')
WHERE 
    tipo IS NULL 
    OR estado IS NULL;

SET SQL_SAFE_UPDATES = 1;

-- Verificación: ver si quedan NULLs
SELECT 
    COUNT(*) as total,
    SUM(CASE WHEN tipo IS NULL THEN 1 ELSE 0 END) as nulls_tipo,
    SUM(CASE WHEN estado IS NULL THEN 1 ELSE 0 END) as nulls_estado
FROM contratos;
