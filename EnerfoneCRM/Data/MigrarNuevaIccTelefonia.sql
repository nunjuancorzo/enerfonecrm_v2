DROP PROCEDURE IF EXISTS MigrarNuevaIccTelefonia;

DELIMITER //
CREATE PROCEDURE MigrarNuevaIccTelefonia()
BEGIN
    IF NOT EXISTS (SELECT 1 FROM information_schema.COLUMNS WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'contratos' AND COLUMN_NAME = 'nueva_icc_principal') THEN
        ALTER TABLE contratos ADD COLUMN nueva_icc_principal TINYINT(1) NOT NULL DEFAULT 0;
    END IF;
    IF NOT EXISTS (SELECT 1 FROM information_schema.COLUMNS WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'contratos' AND COLUMN_NAME = 'nueva_icc_principal_2') THEN
        ALTER TABLE contratos ADD COLUMN nueva_icc_principal_2 TINYINT(1) NOT NULL DEFAULT 0;
    END IF;
    IF NOT EXISTS (SELECT 1 FROM information_schema.COLUMNS WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'contratos' AND COLUMN_NAME = 'nueva_icc_linea1_tel') THEN
        ALTER TABLE contratos ADD COLUMN nueva_icc_linea1_tel TINYINT(1) NOT NULL DEFAULT 0;
    END IF;
    IF NOT EXISTS (SELECT 1 FROM information_schema.COLUMNS WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'contratos' AND COLUMN_NAME = 'nueva_icc_linea2_tel') THEN
        ALTER TABLE contratos ADD COLUMN nueva_icc_linea2_tel TINYINT(1) NOT NULL DEFAULT 0;
    END IF;
    IF NOT EXISTS (SELECT 1 FROM information_schema.COLUMNS WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'contratos' AND COLUMN_NAME = 'nueva_icc_linea3_tel') THEN
        ALTER TABLE contratos ADD COLUMN nueva_icc_linea3_tel TINYINT(1) NOT NULL DEFAULT 0;
    END IF;
    IF NOT EXISTS (SELECT 1 FROM information_schema.COLUMNS WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'contratos' AND COLUMN_NAME = 'nueva_icc_linea4_tel') THEN
        ALTER TABLE contratos ADD COLUMN nueva_icc_linea4_tel TINYINT(1) NOT NULL DEFAULT 0;
    END IF;
    IF NOT EXISTS (SELECT 1 FROM information_schema.COLUMNS WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'contratos' AND COLUMN_NAME = 'nueva_icc_linea5_tel') THEN
        ALTER TABLE contratos ADD COLUMN nueva_icc_linea5_tel TINYINT(1) NOT NULL DEFAULT 0;
    END IF;
    IF NOT EXISTS (SELECT 1 FROM information_schema.COLUMNS WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'contratos' AND COLUMN_NAME = 'nueva_icc_linea6_tel') THEN
        ALTER TABLE contratos ADD COLUMN nueva_icc_linea6_tel TINYINT(1) NOT NULL DEFAULT 0;
    END IF;
    IF NOT EXISTS (SELECT 1 FROM information_schema.COLUMNS WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'contratos' AND COLUMN_NAME = 'nueva_icc_linea7_tel') THEN
        ALTER TABLE contratos ADD COLUMN nueva_icc_linea7_tel TINYINT(1) NOT NULL DEFAULT 0;
    END IF;
    IF NOT EXISTS (SELECT 1 FROM information_schema.COLUMNS WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'contratos' AND COLUMN_NAME = 'nueva_icc_linea8_tel') THEN
        ALTER TABLE contratos ADD COLUMN nueva_icc_linea8_tel TINYINT(1) NOT NULL DEFAULT 0;
    END IF;
    IF NOT EXISTS (SELECT 1 FROM information_schema.COLUMNS WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'contratos' AND COLUMN_NAME = 'nueva_icc_linea9_tel') THEN
        ALTER TABLE contratos ADD COLUMN nueva_icc_linea9_tel TINYINT(1) NOT NULL DEFAULT 0;
    END IF;
    IF NOT EXISTS (SELECT 1 FROM information_schema.COLUMNS WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'contratos' AND COLUMN_NAME = 'nueva_icc_linea10_tel') THEN
        ALTER TABLE contratos ADD COLUMN nueva_icc_linea10_tel TINYINT(1) NOT NULL DEFAULT 0;
    END IF;
    IF NOT EXISTS (SELECT 1 FROM information_schema.COLUMNS WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'contratos' AND COLUMN_NAME = 'nueva_icc_linea11_tel') THEN
        ALTER TABLE contratos ADD COLUMN nueva_icc_linea11_tel TINYINT(1) NOT NULL DEFAULT 0;
    END IF;
    IF NOT EXISTS (SELECT 1 FROM information_schema.COLUMNS WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'contratos' AND COLUMN_NAME = 'nueva_icc_linea12_tel') THEN
        ALTER TABLE contratos ADD COLUMN nueva_icc_linea12_tel TINYINT(1) NOT NULL DEFAULT 0;
    END IF;
    IF NOT EXISTS (SELECT 1 FROM information_schema.COLUMNS WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'contratos' AND COLUMN_NAME = 'nueva_icc_linea13_tel') THEN
        ALTER TABLE contratos ADD COLUMN nueva_icc_linea13_tel TINYINT(1) NOT NULL DEFAULT 0;
    END IF;
    IF NOT EXISTS (SELECT 1 FROM information_schema.COLUMNS WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'contratos' AND COLUMN_NAME = 'nueva_icc_linea14_tel') THEN
        ALTER TABLE contratos ADD COLUMN nueva_icc_linea14_tel TINYINT(1) NOT NULL DEFAULT 0;
    END IF;
    IF NOT EXISTS (SELECT 1 FROM information_schema.COLUMNS WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'contratos' AND COLUMN_NAME = 'nueva_icc_linea15_tel') THEN
        ALTER TABLE contratos ADD COLUMN nueva_icc_linea15_tel TINYINT(1) NOT NULL DEFAULT 0;
    END IF;
END//
DELIMITER ;

CALL MigrarNuevaIccTelefonia();
DROP PROCEDURE MigrarNuevaIccTelefonia;