-- Añade el NIF/CIF del usuario o colaborador.
-- El campo es opcional para no afectar a los usuarios existentes.

USE enerfonecrm;

DELIMITER //

CREATE PROCEDURE AddNifCifUsuarios()
BEGIN
    IF NOT EXISTS (
        SELECT 1
        FROM information_schema.COLUMNS
        WHERE TABLE_SCHEMA = DATABASE()
          AND TABLE_NAME = 'usuarios'
          AND COLUMN_NAME = 'nif_cif'
    ) THEN
        ALTER TABLE usuarios
        ADD COLUMN nif_cif VARCHAR(20) NULL
        COMMENT 'NIF o CIF del usuario/colaborador'
        AFTER apellidos;
    END IF;
END//

DELIMITER ;

CALL AddNifCifUsuarios();
DROP PROCEDURE AddNifCifUsuarios;