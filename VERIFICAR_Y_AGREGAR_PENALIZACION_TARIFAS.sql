-- Script para verificar y agregar campos de penalización a las tablas de tarifas
-- Sistema de decomisiones por baja anticipada
-- Ejecutar en la base de datos enerfone_crm
-- Fecha: Marzo 2026

USE enerfone_crm;

-- Deshabilitar modo de actualización segura temporalmente
SET SQL_SAFE_UPDATES = 0;

-- ========================================
-- VERIFICAR Y AGREGAR COLUMNAS
-- ========================================

-- Verificar si la columna dias_penalizacion existe en tarifasenergia
SET @col_exists_energia_dias = (
    SELECT COUNT(*) 
    FROM information_schema.COLUMNS 
    WHERE TABLE_SCHEMA = DATABASE() 
    AND TABLE_NAME = 'tarifasenergia' 
    AND COLUMN_NAME = 'dias_penalizacion'
);

-- Verificar si la columna tipo_penalizacion existe en tarifasenergia
SET @col_exists_energia_tipo = (
    SELECT COUNT(*) 
    FROM information_schema.COLUMNS 
    WHERE TABLE_SCHEMA = DATABASE() 
    AND TABLE_NAME = 'tarifasenergia' 
    AND COLUMN_NAME = 'tipo_penalizacion'
);

-- Agregar columnas a tarifasenergia si no existen
SET @sql_energia_dias = IF(@col_exists_energia_dias = 0,
    'ALTER TABLE tarifasenergia ADD COLUMN dias_penalizacion INT NULL COMMENT "Días mínimos antes de permitir baja sin penalización" AFTER activa',
    'SELECT "La columna dias_penalizacion ya existe en tarifasenergia" AS mensaje'
);
PREPARE stmt FROM @sql_energia_dias;
EXECUTE stmt;
DEALLOCATE PREPARE stmt;

SET @sql_energia_tipo = IF(@col_exists_energia_tipo = 0,
    'ALTER TABLE tarifasenergia ADD COLUMN tipo_penalizacion VARCHAR(20) NULL COMMENT "Total o Proporcional" AFTER dias_penalizacion',
    'SELECT "La columna tipo_penalizacion ya existe en tarifasenergia" AS mensaje'
);
PREPARE stmt FROM @sql_energia_tipo;
EXECUTE stmt;
DEALLOCATE PREPARE stmt;

-- ========================================

-- Verificar si la columna dias_penalizacion existe en tarifastelefonia
SET @col_exists_tel_dias = (
    SELECT COUNT(*) 
    FROM information_schema.COLUMNS 
    WHERE TABLE_SCHEMA = DATABASE() 
    AND TABLE_NAME = 'tarifastelefonia' 
    AND COLUMN_NAME = 'dias_penalizacion'
);

-- Verificar si la columna tipo_penalizacion existe en tarifastelefonia
SET @col_exists_tel_tipo = (
    SELECT COUNT(*) 
    FROM information_schema.COLUMNS 
    WHERE TABLE_SCHEMA = DATABASE() 
    AND TABLE_NAME = 'tarifastelefonia' 
    AND COLUMN_NAME = 'tipo_penalizacion'
);

-- Agregar columnas a tarifastelefonia si no existen
SET @sql_tel_dias = IF(@col_exists_tel_dias = 0,
    'ALTER TABLE tarifastelefonia ADD COLUMN dias_penalizacion INT NULL COMMENT "Días mínimos antes de permitir baja sin penalización" AFTER activa',
    'SELECT "La columna dias_penalizacion ya existe en tarifastelefonia" AS mensaje'
);
PREPARE stmt FROM @sql_tel_dias;
EXECUTE stmt;
DEALLOCATE PREPARE stmt;

SET @sql_tel_tipo = IF(@col_exists_tel_tipo = 0,
    'ALTER TABLE tarifastelefonia ADD COLUMN tipo_penalizacion VARCHAR(20) NULL COMMENT "Total o Proporcional" AFTER dias_penalizacion',
    'SELECT "La columna tipo_penalizacion ya existe en tarifastelefonia" AS mensaje'
);
PREPARE stmt FROM @sql_tel_tipo;
EXECUTE stmt;
DEALLOCATE PREPARE stmt;

-- ========================================

-- Verificar si la columna dias_penalizacion existe en tarifas_alarmas
SET @col_exists_alar_dias = (
    SELECT COUNT(*) 
    FROM information_schema.COLUMNS 
    WHERE TABLE_SCHEMA = DATABASE() 
    AND TABLE_NAME = 'tarifas_alarmas' 
    AND COLUMN_NAME = 'dias_penalizacion'
);

-- Verificar si la columna tipo_penalizacion existe en tarifas_alarmas
SET @col_exists_alar_tipo = (
    SELECT COUNT(*) 
    FROM information_schema.COLUMNS 
    WHERE TABLE_SCHEMA = DATABASE() 
    AND TABLE_NAME = 'tarifas_alarmas' 
    AND COLUMN_NAME = 'tipo_penalizacion'
);

-- Agregar columnas a tarifas_alarmas si no existen
SET @sql_alar_dias = IF(@col_exists_alar_dias = 0,
    'ALTER TABLE tarifas_alarmas ADD COLUMN dias_penalizacion INT NULL COMMENT "Días mínimos antes de permitir baja sin penalización" AFTER activa',
    'SELECT "La columna dias_penalizacion ya existe en tarifas_alarmas" AS mensaje'
);
PREPARE stmt FROM @sql_alar_dias;
EXECUTE stmt;
DEALLOCATE PREPARE stmt;

SET @sql_alar_tipo = IF(@col_exists_alar_tipo = 0,
    'ALTER TABLE tarifas_alarmas ADD COLUMN tipo_penalizacion VARCHAR(20) NULL COMMENT "Total o Proporcional" AFTER dias_penalizacion',
    'SELECT "La columna tipo_penalizacion ya existe en tarifas_alarmas" AS mensaje'
);
PREPARE stmt FROM @sql_alar_tipo;
EXECUTE stmt;
DEALLOCATE PREPARE stmt;

-- ========================================
-- VERIFICACIÓN FINAL
-- ========================================

SELECT 'VERIFICACIÓN DE COLUMNAS' AS titulo;

SELECT 
    'tarifasenergia' AS tabla,
    COUNT(*) AS columnas_penalizacion
FROM information_schema.COLUMNS 
WHERE TABLE_SCHEMA = DATABASE() 
  AND TABLE_NAME = 'tarifasenergia' 
  AND COLUMN_NAME IN ('dias_penalizacion', 'tipo_penalizacion')

UNION ALL

SELECT 
    'tarifastelefonia' AS tabla,
    COUNT(*) AS columnas_penalizacion
FROM information_schema.COLUMNS 
WHERE TABLE_SCHEMA = DATABASE() 
  AND TABLE_NAME = 'tarifastelefonia' 
  AND COLUMN_NAME IN ('dias_penalizacion', 'tipo_penalizacion')

UNION ALL

SELECT 
    'tarifas_alarmas' AS tabla,
    COUNT(*) AS columnas_penalizacion
FROM information_schema.COLUMNS 
WHERE TABLE_SCHEMA = DATABASE() 
  AND TABLE_NAME = 'tarifas_alarmas' 
  AND COLUMN_NAME IN ('dias_penalizacion', 'tipo_penalizacion');

-- Reactivar modo de actualización segura
SET SQL_SAFE_UPDATES = 1;

SELECT 'Script ejecutado correctamente. Cada tabla debe tener 2 columnas de penalización.' AS resultado;
