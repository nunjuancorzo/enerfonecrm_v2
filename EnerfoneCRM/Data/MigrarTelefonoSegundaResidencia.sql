DROP PROCEDURE IF EXISTS MigrarTelefonoSegundaResidencia;

DELIMITER //
CREATE PROCEDURE MigrarTelefonoSegundaResidencia()
BEGIN
    IF NOT EXISTS (
        SELECT 1
        FROM information_schema.COLUMNS
        WHERE TABLE_SCHEMA = DATABASE()
          AND TABLE_NAME = 'contratos'
          AND COLUMN_NAME = 'telefono_segunda_residencia'
    ) THEN
        ALTER TABLE contratos ADD COLUMN telefono_segunda_residencia VARCHAR(50) NULL;
    END IF;
END//
DELIMITER ;

CALL MigrarTelefonoSegundaResidencia();
DROP PROCEDURE MigrarTelefonoSegundaResidencia;