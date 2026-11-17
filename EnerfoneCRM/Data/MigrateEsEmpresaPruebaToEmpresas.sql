-- ============================================================================
-- Migration: Mover es_empresa_prueba de configuracion_empresa a empresas
-- Descripción: 
--   1. Agregar columna es_empresa_prueba a tabla empresas (si no existe)
--   2. Migrar datos de configuracion_empresa a empresas
--   3. Eliminar columna de configuracion_empresa
-- ============================================================================

-- Paso 1: Agregar columna a empresas solo si no existe
SET @dbname = DATABASE();
SET @tablename = "empresas";
SET @columnname = "es_empresa_prueba";
SET @preparedstatement = (SELECT IF(
  (
    SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS
    WHERE (table_name = @tablename)
    AND (table_schema = @dbname)
    AND (column_name = @columnname)
  ) > 0,
  "SELECT 1",
  CONCAT("ALTER TABLE ", @tablename, " ADD COLUMN ", @columnname, " TINYINT(1) NOT NULL DEFAULT 0")
));
PREPARE alterIfNotExists FROM @preparedstatement;
EXECUTE alterIfNotExists;
DEALLOCATE PREPARE alterIfNotExists;

-- Paso 2: Migrar datos de configuracion_empresa a empresas
-- La relación es: configuracion_empresa.id = empresas.id_empresa
UPDATE empresas e
INNER JOIN configuracion_empresa ce ON ce.id = e.id_empresa
SET e.es_empresa_prueba = ce.es_empresa_prueba
WHERE e.id_empresa > 0;

-- Paso 3: Eliminar columna de configuracion_empresa si existe
SET @columnname = "es_empresa_prueba";
SET @preparedstatement = (SELECT IF(
  (
    SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS
    WHERE (table_name = "configuracion_empresa")
    AND (table_schema = @dbname)
    AND (column_name = @columnname)
  ) > 0,
  "ALTER TABLE configuracion_empresa DROP COLUMN es_empresa_prueba",
  "SELECT 1"
));
PREPARE dropIfExists FROM @preparedstatement;
EXECUTE dropIfExists;
DEALLOCATE PREPARE dropIfExists;

-- Verificar que la migración fue correcta
SELECT 'Migración completada. Empresas con es_empresa_prueba = 1:' as estado;
SELECT id_empresa, nombre, es_empresa_prueba FROM empresas WHERE es_empresa_prueba = 1;
