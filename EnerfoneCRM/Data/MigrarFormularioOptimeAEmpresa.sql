-- Migración: Agregar formulario_optime_habilitado a tabla empresas

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
        SELECT 'Columna formulario_optime_habilitado agregada a tabla empresas' AS resultado;
    ELSE
        SELECT 'Columna formulario_optime_habilitado ya existe en tabla empresas' AS resultado;
    END IF;
    
    -- Eliminar columna de configuracion_empresa si existe
    IF EXISTS (
        SELECT 1 FROM information_schema.COLUMNS
        WHERE TABLE_SCHEMA = DATABASE()
          AND TABLE_NAME = 'configuracion_empresa'
          AND COLUMN_NAME = 'formulario_optime_habilitado'
    ) THEN
        ALTER TABLE configuracion_empresa
            DROP COLUMN formulario_optime_habilitado;
        SELECT 'Columna formulario_optime_habilitado eliminada de tabla configuracion_empresa' AS resultado;
    END IF;
END//
DELIMITER ;

CALL MigrarFormularioOptimeAEmpresa();
DROP PROCEDURE IF EXISTS MigrarFormularioOptimeAEmpresa;
