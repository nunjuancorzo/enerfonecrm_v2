-- ============================================================================
-- Fix: Normalizar nombre de columna empresa_id a id_empresa en optime_solicitudes
-- Descripción: Renombra empresa_id a id_empresa para consistencia con otras tablas
-- ============================================================================

DROP PROCEDURE IF EXISTS FixOptimeEmpresaIdColumn;

DELIMITER //
CREATE PROCEDURE FixOptimeEmpresaIdColumn()
BEGIN
    DECLARE EXIT HANDLER FOR SQLEXCEPTION
    BEGIN
        GET DIAGNOSTICS CONDITION 1 @message = MESSAGE_TEXT;
        SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = @message;
    END;
    
    -- Verificar si existe empresa_id pero no id_empresa
    IF EXISTS (
        SELECT 1 FROM information_schema.COLUMNS
        WHERE TABLE_SCHEMA = DATABASE()
          AND TABLE_NAME = 'optime_solicitudes'
          AND COLUMN_NAME = 'empresa_id'
    ) AND NOT EXISTS (
        SELECT 1 FROM information_schema.COLUMNS
        WHERE TABLE_SCHEMA = DATABASE()
          AND TABLE_NAME = 'optime_solicitudes'
          AND COLUMN_NAME = 'id_empresa'
    ) THEN
        -- Eliminar claves foráneas que apunten a empresa_id
        SET @fk_name = (
            SELECT CONSTRAINT_NAME 
            FROM information_schema.REFERENTIAL_CONSTRAINTS
            WHERE TABLE_NAME = 'optime_solicitudes'
              AND CONSTRAINT_SCHEMA = DATABASE()
            LIMIT 1
        );
        
        IF @fk_name IS NOT NULL THEN
            SET @sql = CONCAT('ALTER TABLE optime_solicitudes DROP FOREIGN KEY ', @fk_name);
            PREPARE stmt FROM @sql;
            EXECUTE stmt;
            DEALLOCATE PREPARE stmt;
            SELECT CONCAT('Clave foránea ', @fk_name, ' eliminada') AS paso;
        END IF;
        
        -- Eliminar índice si existe
        IF EXISTS (
            SELECT 1 FROM information_schema.STATISTICS
            WHERE TABLE_SCHEMA = DATABASE()
              AND TABLE_NAME = 'optime_solicitudes'
              AND INDEX_NAME = 'idx_optime_empresa'
        ) THEN
            ALTER TABLE optime_solicitudes DROP INDEX idx_optime_empresa;
            SELECT 'Índice idx_optime_empresa eliminado' AS paso;
        END IF;
        
        -- Renombrar columna
        ALTER TABLE optime_solicitudes CHANGE COLUMN empresa_id id_empresa INT NOT NULL;
        SELECT 'Columna empresa_id renombrada a id_empresa' AS paso;
        
        -- Recrear índice
        CREATE INDEX idx_optime_empresa ON optime_solicitudes(id_empresa);
        SELECT 'Índice idx_optime_empresa recreado' AS paso;
        
        -- Recrear clave foránea
        ALTER TABLE optime_solicitudes
            ADD CONSTRAINT fk_optime_solicitudes_empresa
            FOREIGN KEY (id_empresa) REFERENCES empresas(id_empresa)
            ON DELETE RESTRICT ON UPDATE CASCADE;
        SELECT 'Clave foránea fk_optime_solicitudes_empresa recreada' AS paso;
        
        SELECT '✓ Fix completado exitosamente' AS resultado;
    ELSE
        SELECT '✓ Columna ya está normalizada correctamente' AS resultado;
    END IF;
END//
DELIMITER ;

CALL FixOptimeEmpresaIdColumn();
DROP PROCEDURE IF EXISTS FixOptimeEmpresaIdColumn;
