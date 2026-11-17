-- ============================================================================
-- Migración: Optime - Crear y configurar tablas y columnas de Optime
-- Fecha: 2026-11-13
-- Descripción: Script unificado para la migración completa de Optime
-- ============================================================================

-- Paso 1: Agregar columna formulario_optime_habilitado a tabla empresas
DROP PROCEDURE IF EXISTS MigrarFormularioOptimeAEmpresa;

DELIMITER //
CREATE PROCEDURE MigrarFormularioOptimeAEmpresa()
BEGIN
    DECLARE EXIT HANDLER FOR SQLEXCEPTION
    BEGIN
        GET DIAGNOSTICS CONDITION 1 @message = MESSAGE_TEXT;
        SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = @message;
    END;
    
    -- Agregar columna a tabla empresas si no existe
    IF NOT EXISTS (
        SELECT 1 FROM information_schema.COLUMNS
        WHERE TABLE_SCHEMA = DATABASE()
          AND TABLE_NAME = 'empresas'
          AND COLUMN_NAME = 'formulario_optime_habilitado'
    ) THEN
        ALTER TABLE empresas
            ADD COLUMN formulario_optime_habilitado TINYINT(1) NOT NULL DEFAULT 0;
        SELECT 'Paso 1: Columna formulario_optime_habilitado agregada a tabla empresas' AS resultado;
    ELSE
        SELECT 'Paso 1: Columna formulario_optime_habilitado ya existe en tabla empresas' AS resultado;
    END IF;
END//
DELIMITER ;

CALL MigrarFormularioOptimeAEmpresa();
DROP PROCEDURE IF EXISTS MigrarFormularioOptimeAEmpresa;

-- Paso 2: Normalizar la columna empresa_id en tabla optime_solicitudes
-- Renombrar empresa_id a id_empresa si existe con ese nombre
DROP PROCEDURE IF EXISTS NormalizarOptimeSolicitudes;

DELIMITER //
CREATE PROCEDURE NormalizarOptimeSolicitudes()
BEGIN
    DECLARE EXIT HANDLER FOR SQLEXCEPTION
    BEGIN
        GET DIAGNOSTICS CONDITION 1 @message = MESSAGE_TEXT;
        SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = @message;
    END;
    
    -- Verificar si existe la columna empresa_id (nombre incorrecto)
    IF EXISTS (
        SELECT 1 FROM information_schema.COLUMNS
        WHERE TABLE_SCHEMA = DATABASE()
          AND TABLE_NAME = 'optime_solicitudes'
          AND COLUMN_NAME = 'empresa_id'
    ) THEN
        -- Eliminar clave foránea si existe (asociada a empresa_id)
        IF EXISTS (
            SELECT CONSTRAINT_NAME FROM information_schema.REFERENTIAL_CONSTRAINTS
            WHERE TABLE_NAME = 'optime_solicitudes'
              AND CONSTRAINT_SCHEMA = DATABASE()
              AND CONSTRAINT_NAME LIKE '%empresa%'
        ) THEN
            SET @sql = CONCAT('ALTER TABLE optime_solicitudes DROP FOREIGN KEY ', 
                (SELECT CONSTRAINT_NAME FROM information_schema.REFERENTIAL_CONSTRAINTS
                 WHERE TABLE_NAME = 'optime_solicitudes' AND CONSTRAINT_SCHEMA = DATABASE() LIMIT 1));
            PREPARE stmt FROM @sql;
            EXECUTE stmt;
            DEALLOCATE PREPARE stmt;
        END IF;
        
        -- Renombrar empresa_id a id_empresa
        ALTER TABLE optime_solicitudes CHANGE COLUMN empresa_id id_empresa INT NOT NULL;
        SELECT 'Paso 2a: Columna empresa_id renombrada a id_empresa en tabla optime_solicitudes' AS resultado;
    END IF;
    
    -- Verificar si NO existe la columna id_empresa y crearla
    IF NOT EXISTS (
        SELECT 1 FROM information_schema.COLUMNS
        WHERE TABLE_SCHEMA = DATABASE()
          AND TABLE_NAME = 'optime_solicitudes'
          AND COLUMN_NAME = 'id_empresa'
    ) THEN
        -- Primero agregar como nullable
        ALTER TABLE optime_solicitudes
            ADD COLUMN id_empresa INT NULL;
        
        -- Obtener el ID de la primera empresa (si existe)
        SET @empresa_id = (SELECT id_empresa FROM empresas LIMIT 1);
        
        -- Actualizar registros existentes con la primera empresa
        IF @empresa_id IS NOT NULL THEN
            UPDATE optime_solicitudes SET id_empresa = @empresa_id WHERE id_empresa IS NULL;
        END IF;
        
        -- Cambiar a NOT NULL
        ALTER TABLE optime_solicitudes
            MODIFY COLUMN id_empresa INT NOT NULL;
        
        SELECT 'Paso 2b: Columna id_empresa creada en tabla optime_solicitudes' AS resultado;
    END IF;
END//
DELIMITER ;

CALL NormalizarOptimeSolicitudes();
DROP PROCEDURE IF EXISTS NormalizarOptimeSolicitudes;

-- Paso 3: Crear índices y claves foráneas para Optime
DROP PROCEDURE IF EXISTS FinalizarOptimeMigracion;

DELIMITER //
CREATE PROCEDURE FinalizarOptimeMigracion()
BEGIN
    DECLARE EXIT HANDLER FOR SQLEXCEPTION
    BEGIN
        GET DIAGNOSTICS CONDITION 1 @message = MESSAGE_TEXT;
        SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = @message;
    END;
    
    -- Crear índice si no existe
    IF NOT EXISTS (
        SELECT 1 FROM information_schema.STATISTICS
        WHERE TABLE_SCHEMA = DATABASE()
          AND TABLE_NAME = 'optime_solicitudes'
          AND INDEX_NAME = 'idx_optime_empresa'
    ) THEN
        CREATE INDEX idx_optime_empresa ON optime_solicitudes(id_empresa);
        SELECT 'Paso 3a: Índice idx_optime_empresa creado en tabla optime_solicitudes' AS resultado;
    ELSE
        SELECT 'Paso 3a: Índice idx_optime_empresa ya existe en tabla optime_solicitudes' AS resultado;
    END IF;
    
    -- Crear clave foránea si no existe
    IF NOT EXISTS (
        SELECT CONSTRAINT_NAME FROM information_schema.REFERENTIAL_CONSTRAINTS
        WHERE TABLE_NAME = 'optime_solicitudes'
          AND CONSTRAINT_SCHEMA = DATABASE()
          AND CONSTRAINT_NAME = 'fk_optime_solicitudes_empresa'
    ) THEN
        ALTER TABLE optime_solicitudes
            ADD CONSTRAINT fk_optime_solicitudes_empresa
            FOREIGN KEY (id_empresa) REFERENCES empresas(id_empresa)
            ON DELETE RESTRICT ON UPDATE CASCADE;
        SELECT 'Paso 3b: Clave foránea fk_optime_solicitudes_empresa creada' AS resultado;
    ELSE
        SELECT 'Paso 3b: Clave foránea fk_optime_solicitudes_empresa ya existe' AS resultado;
    END IF;
    
    -- Verificar y crear índices para optime_documentos
    IF NOT EXISTS (
        SELECT 1 FROM information_schema.STATISTICS
        WHERE TABLE_SCHEMA = DATABASE()
          AND TABLE_NAME = 'optime_documentos'
          AND INDEX_NAME = 'idx_documento_solicitud'
    ) THEN
        CREATE INDEX idx_documento_solicitud ON optime_documentos(solicitud_id);
        SELECT 'Paso 3c: Índice idx_documento_solicitud creado en tabla optime_documentos' AS resultado;
    END IF;
    
    -- Verificar y crear índices para optime_firmas
    IF NOT EXISTS (
        SELECT 1 FROM information_schema.STATISTICS
        WHERE TABLE_SCHEMA = DATABASE()
          AND TABLE_NAME = 'optime_firmas'
          AND INDEX_NAME = 'idx_firma_solicitud'
    ) THEN
        CREATE INDEX idx_firma_solicitud ON optime_firmas(solicitud_id);
        SELECT 'Paso 3d: Índice idx_firma_solicitud creado en tabla optime_firmas' AS resultado;
    END IF;
    
    SELECT '✓ Migración de Optime completada exitosamente' AS resultado;
END//
DELIMITER ;

CALL FinalizarOptimeMigracion();
DROP PROCEDURE IF EXISTS FinalizarOptimeMigracion;

-- ============================================================================
-- FIN DE LA MIGRACIÓN
-- ============================================================================
