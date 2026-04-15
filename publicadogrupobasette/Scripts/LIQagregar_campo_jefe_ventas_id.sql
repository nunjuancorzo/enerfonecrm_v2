-- Script para agregar el campo jefe_ventas_id a la tabla usuarios
-- Este campo permite establecer la relación jerárquica:
-- - Gestores pueden tener un Jefe de ventas asignado
-- - Colaboradores pueden tener un Jefe de ventas asignado
-- Fecha: 28 de enero de 2026

USE crm_enerfone;

-- Verificar si la columna existe antes de agregarla
SET @col_exists = (
    SELECT COUNT(*) 
    FROM INFORMATION_SCHEMA.COLUMNS 
    WHERE TABLE_SCHEMA = 'crm_enerfone' 
    AND TABLE_NAME = 'usuarios' 
    AND COLUMN_NAME = 'jefe_ventas_id'
);

-- Agregar el campo jefe_ventas_id si no existe
SET @sql_add_column = IF(@col_exists = 0,
    'ALTER TABLE usuarios ADD COLUMN jefe_ventas_id INT NULL COMMENT ''ID del jefe de ventas al que pertenece el usuario (Gestor o Colaborador)''',
    'SELECT ''La columna jefe_ventas_id ya existe'' AS Mensaje'
);

PREPARE stmt FROM @sql_add_column;
EXECUTE stmt;
DEALLOCATE PREPARE stmt;

-- Verificar si la FK existe antes de crearla
SET @fk_exists = (
    SELECT COUNT(*) 
    FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS 
    WHERE CONSTRAINT_SCHEMA = 'crm_enerfone' 
    AND TABLE_NAME = 'usuarios' 
    AND CONSTRAINT_NAME = 'fk_usuarios_jefe_ventas'
);

-- Crear la restricción de clave foránea si no existe
SET @sql_add_fk = IF(@fk_exists = 0,
    'ALTER TABLE usuarios ADD CONSTRAINT fk_usuarios_jefe_ventas FOREIGN KEY (jefe_ventas_id) REFERENCES usuarios(idusuarios) ON DELETE SET NULL ON UPDATE CASCADE',
    'SELECT ''La FK fk_usuarios_jefe_ventas ya existe'' AS Mensaje'
);

PREPARE stmt FROM @sql_add_fk;
EXECUTE stmt;
DEALLOCATE PREPARE stmt;

-- Verificar si el índice existe antes de crearlo
SET @idx_exists = (
    SELECT COUNT(*) 
    FROM INFORMATION_SCHEMA.STATISTICS 
    WHERE TABLE_SCHEMA = 'crm_enerfone' 
    AND TABLE_NAME = 'usuarios' 
    AND INDEX_NAME = 'idx_jefe_ventas_id'
);

-- Crear índice para mejorar el rendimiento de las consultas si no existe
SET @sql_add_idx = IF(@idx_exists = 0,
    'CREATE INDEX idx_jefe_ventas_id ON usuarios(jefe_ventas_id)',
    'SELECT ''El índice idx_jefe_ventas_id ya existe'' AS Mensaje'
);

PREPARE stmt FROM @sql_add_idx;
EXECUTE stmt;
DEALLOCATE PREPARE stmt;

-- Verificar que el campo se ha creado correctamente
SELECT 
    COLUMN_NAME,
    COLUMN_TYPE,
    IS_NULLABLE,
    COLUMN_KEY,
    COLUMN_COMMENT
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_SCHEMA = 'crm_enerfone' 
AND TABLE_NAME = 'usuarios' 
AND COLUMN_NAME = 'jefe_ventas_id';
