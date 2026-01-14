-- Script para agregar la columna estado_contrato a la tabla observaciones_contratos
-- Esta columna almacena el estado del contrato en el momento en que se creó la observación

-- Agregar la columna solo si no existe
SET @column_exists = (
    SELECT COUNT(*) 
    FROM INFORMATION_SCHEMA.COLUMNS 
    WHERE TABLE_SCHEMA = 'enerfoneCRM' 
    AND TABLE_NAME = 'observaciones_contratos' 
    AND COLUMN_NAME = 'estado_contrato'
);

SET @sql = IF(@column_exists = 0,
    'ALTER TABLE observaciones_contratos ADD COLUMN estado_contrato VARCHAR(100) NULL AFTER fecha_hora',
    'SELECT "La columna estado_contrato ya existe" AS mensaje'
);

PREPARE stmt FROM @sql;
EXECUTE stmt;
DEALLOCATE PREPARE stmt;

-- Comentario de la columna
ALTER TABLE observaciones_contratos 
MODIFY COLUMN estado_contrato VARCHAR(100) NULL 
COMMENT 'Estado del contrato en el momento de crear la observación';

-- Desactivar modo de actualización segura temporalmente
SET SQL_SAFE_UPDATES = 0;

-- Actualizar observaciones existentes con el estado actual del contrato
UPDATE observaciones_contratos oc
INNER JOIN contratos c ON oc.id_contrato = c.id
SET oc.estado_contrato = c.estado
WHERE oc.estado_contrato IS NULL;

-- Reactivar modo de actualización segura
SET SQL_SAFE_UPDATES = 1;
