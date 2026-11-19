-- ============================================================================
-- Add: Datos de Pyme en la tabla usuarios (tipo de usuario Autónomo / Pyme)
-- Descripción:
--   1. Agregar columna razon_social_pyme a usuarios (si no existe)
--   2. Agregar columna cif_pyme a usuarios (si no existe)
-- Es idempotente: se puede ejecutar varias veces sin error.
-- Ejecutar en la base de datos del CRM antes de publicar la aplicación.
-- ============================================================================

SET @dbname = DATABASE();

SET @tablename = "usuarios";
SET @columnname = "razon_social_pyme";
SET @preparedstatement = (SELECT IF(
  (
    SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS
    WHERE (table_name = @tablename)
    AND (table_schema = @dbname)
    AND (column_name = @columnname)
  ) > 0,
  "SELECT 1",
  CONCAT("ALTER TABLE ", @tablename, " ADD COLUMN ", @columnname, " VARCHAR(200) NULL")
));
PREPARE alterIfNotExists FROM @preparedstatement;
EXECUTE alterIfNotExists;
DEALLOCATE PREPARE alterIfNotExists;

SET @columnname = "cif_pyme";
SET @preparedstatement = (SELECT IF(
  (
    SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS
    WHERE (table_name = @tablename)
    AND (table_schema = @dbname)
    AND (column_name = @columnname)
  ) > 0,
  "SELECT 1",
  CONCAT("ALTER TABLE ", @tablename, " ADD COLUMN ", @columnname, " VARCHAR(20) NULL")
));
PREPARE alterIfNotExists FROM @preparedstatement;
EXECUTE alterIfNotExists;
DEALLOCATE PREPARE alterIfNotExists;

-- Verificación: debe devolver 2 filas
SELECT COLUMN_NAME, COLUMN_TYPE, IS_NULLABLE
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'usuarios'
  AND COLUMN_NAME IN ('razon_social_pyme', 'cif_pyme');
