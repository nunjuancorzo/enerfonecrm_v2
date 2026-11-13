DROP PROCEDURE IF EXISTS MigrarFormularioOptime;

DELIMITER //
CREATE PROCEDURE MigrarFormularioOptime()
BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM information_schema.COLUMNS
        WHERE TABLE_SCHEMA = DATABASE()
          AND TABLE_NAME = 'configuracion_empresa'
          AND COLUMN_NAME = 'formulario_optime_habilitado'
    ) THEN
        ALTER TABLE configuracion_empresa
            ADD COLUMN formulario_optime_habilitado TINYINT(1) NOT NULL DEFAULT 0;
    END IF;
END//
DELIMITER ;

CALL MigrarFormularioOptime();
DROP PROCEDURE MigrarFormularioOptime;
