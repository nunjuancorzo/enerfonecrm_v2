-- Script para agregar el campo de teléfono a la tabla de usuarios
-- Ejecutar este script en la base de datos

USE enerfonecrm;

-- Agregar columna de teléfono si no existe
-- Verificar primero si ya existe la columna
SET @dbname = DATABASE();
SET @tablename = 'usuarios';
SET @columnname = 'telefono';
SET @preparedStatement = (SELECT IF(
  (
    SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS
    WHERE
      (table_name = @tablename)
      AND (table_schema = @dbname)
      AND (column_name = @columnname)
  ) > 0,
  'SELECT 1',
  'ALTER TABLE usuarios ADD COLUMN telefono VARCHAR(20) NULL AFTER email'
));

PREPARE alterIfNotExists FROM @preparedStatement;
EXECUTE alterIfNotExists;
DEALLOCATE PREPARE alterIfNotExists;

-- Verificar que la columna existe
DESCRIBE usuarios;
