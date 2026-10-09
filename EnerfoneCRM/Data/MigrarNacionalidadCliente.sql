DROP PROCEDURE IF EXISTS MigrarNacionalidadCliente;

DELIMITER //
CREATE PROCEDURE MigrarNacionalidadCliente()
BEGIN
    IF NOT EXISTS (
        SELECT 1
        FROM information_schema.COLUMNS
        WHERE TABLE_SCHEMA = DATABASE()
          AND TABLE_NAME = 'clientes_simple'
          AND COLUMN_NAME = 'nacionalidad'
    ) THEN
        ALTER TABLE clientes_simple ADD COLUMN nacionalidad VARCHAR(100) NULL;
    END IF;

    IF NOT EXISTS (
        SELECT 1
        FROM information_schema.COLUMNS
        WHERE TABLE_SCHEMA = DATABASE()
          AND TABLE_NAME = 'clientes_simple'
          AND COLUMN_NAME = 'tipo_documento_identidad'
    ) THEN
        ALTER TABLE clientes_simple ADD COLUMN tipo_documento_identidad VARCHAR(10) NOT NULL DEFAULT 'DNI';
    END IF;

    UPDATE clientes_simple
    SET tipo_documento_identidad = CASE
        WHEN tipo_cliente = 'Pyme' THEN 'CIF'
        WHEN UPPER(LEFT(TRIM(COALESCE(dni_cif, '')), 1)) IN ('X', 'Y', 'Z') THEN 'NIE'
        ELSE 'DNI'
    END
    WHERE id > 0
      AND (tipo_documento_identidad IS NULL OR tipo_documento_identidad = '' OR
           (tipo_documento_identidad = 'DNI' AND (tipo_cliente = 'Pyme' OR UPPER(LEFT(TRIM(COALESCE(dni_cif, '')), 1)) IN ('X', 'Y', 'Z'))));
END//
DELIMITER ;

CALL MigrarNacionalidadCliente();
DROP PROCEDURE MigrarNacionalidadCliente;