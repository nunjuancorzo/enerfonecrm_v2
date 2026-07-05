-- Añadir campos para indicar si una línea telefónica es un ALTA NUEVA
-- (no tiene número previo porque se está dando de alta por primera vez)

DELIMITER //

CREATE PROCEDURE AddAltaNuevaLineasTelefonia()
BEGIN
    DECLARE CONTINUE HANDLER FOR SQLEXCEPTION BEGIN END;
    
    -- Alta nueva para línea principal
    ALTER TABLE contratos ADD COLUMN alta_nueva_linea_principal TINYINT(1) DEFAULT 0;
    
    -- Alta nueva para segunda línea principal
    ALTER TABLE contratos ADD COLUMN alta_nueva_linea_principal_2 TINYINT(1) DEFAULT 0;
    
    -- Alta nueva para líneas adicionales (1-15)
    ALTER TABLE contratos ADD COLUMN alta_nueva_linea1_tel TINYINT(1) DEFAULT 0;
    ALTER TABLE contratos ADD COLUMN alta_nueva_linea2_tel TINYINT(1) DEFAULT 0;
    ALTER TABLE contratos ADD COLUMN alta_nueva_linea3_tel TINYINT(1) DEFAULT 0;
    ALTER TABLE contratos ADD COLUMN alta_nueva_linea4_tel TINYINT(1) DEFAULT 0;
    ALTER TABLE contratos ADD COLUMN alta_nueva_linea5_tel TINYINT(1) DEFAULT 0;
    ALTER TABLE contratos ADD COLUMN alta_nueva_linea6_tel TINYINT(1) DEFAULT 0;
    ALTER TABLE contratos ADD COLUMN alta_nueva_linea7_tel TINYINT(1) DEFAULT 0;
    ALTER TABLE contratos ADD COLUMN alta_nueva_linea8_tel TINYINT(1) DEFAULT 0;
    ALTER TABLE contratos ADD COLUMN alta_nueva_linea9_tel TINYINT(1) DEFAULT 0;
    ALTER TABLE contratos ADD COLUMN alta_nueva_linea10_tel TINYINT(1) DEFAULT 0;
    ALTER TABLE contratos ADD COLUMN alta_nueva_linea11_tel TINYINT(1) DEFAULT 0;
    ALTER TABLE contratos ADD COLUMN alta_nueva_linea12_tel TINYINT(1) DEFAULT 0;
    ALTER TABLE contratos ADD COLUMN alta_nueva_linea13_tel TINYINT(1) DEFAULT 0;
    ALTER TABLE contratos ADD COLUMN alta_nueva_linea14_tel TINYINT(1) DEFAULT 0;
    ALTER TABLE contratos ADD COLUMN alta_nueva_linea15_tel TINYINT(1) DEFAULT 0;
    
END //

DELIMITER ;

CALL AddAltaNuevaLineasTelefonia();
DROP PROCEDURE AddAltaNuevaLineasTelefonia;
