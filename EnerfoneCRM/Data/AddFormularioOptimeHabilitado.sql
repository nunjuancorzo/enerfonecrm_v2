-- ============================================================================
-- Add: Añadir columna formulario_optime_habilitado a configuracion_empresa
-- Descripción: Columna para controlar si el formulario de Optime está habilitado
-- ============================================================================

ALTER TABLE configuracion_empresa
ADD COLUMN formulario_optime_habilitado TINYINT(1) NOT NULL DEFAULT 0
AFTER ocr_fallback_automatico;

-- Verificar que la columna se añadió
SELECT COLUMN_NAME, COLUMN_TYPE, IS_NULLABLE, COLUMN_DEFAULT 
FROM INFORMATION_SCHEMA.COLUMNS 
WHERE TABLE_SCHEMA = 'enerfonecrm' AND TABLE_NAME = 'configuracion_empresa'
AND COLUMN_NAME = 'formulario_optime_habilitado';
