-- Migración: Agregar empresa_id a tabla optime_solicitudes

DROP PROCEDURE IF EXISTS MigrarEmpresaIdOptime;

DELIMITER //
CREATE PROCEDURE MigrarEmpresaIdOptime()
BEGIN
    DECLARE EXIT HANDLER FOR SQLEXCEPTION
    BEGIN
        GET DIAGNOSTICS CONDITION 1 @message = MESSAGE_TEXT;
        SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = @message;
    END;
    
    -- Agregar columna a tabla optime_solicitudes si no existe
    IF NOT EXISTS (
        SELECT 1 FROM information_schema.COLUMNS
        WHERE TABLE_SCHEMA = DATABASE()
          AND TABLE_NAME = 'optime_solicitudes'
          AND COLUMN_NAME = 'empresa_id'
    ) THEN
        -- Primero agregar como nullable
        ALTER TABLE optime_solicitudes
            ADD COLUMN empresa_id INT NULL;
        
        -- Obtener el ID de la primera empresa (si existe)
        SET @empresa_id = (SELECT id_empresa FROM empresas LIMIT 1);
        
        -- Actualizar registros existentes con la primera empresa
        IF @empresa_id IS NOT NULL THEN
            UPDATE optime_solicitudes SET empresa_id = @empresa_id WHERE empresa_id IS NULL;
        END IF;
        
        -- Cambiar a NOT NULL
        ALTER TABLE optime_solicitudes
            MODIFY COLUMN empresa_id INT NOT NULL;
        
        -- Agregar índice
        CREATE INDEX idx_optime_empresa ON optime_solicitudes(empresa_id);
        
        SELECT 'Columna empresa_id agregada a tabla optime_solicitudes' AS resultado;
    ELSE
        SELECT 'Columna empresa_id ya existe en tabla optime_solicitudes' AS resultado;
    END IF;
END//
DELIMITER ;

CALL MigrarEmpresaIdOptime();
DROP PROCEDURE IF EXISTS MigrarEmpresaIdOptime;
