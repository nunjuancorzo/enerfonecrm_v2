DROP PROCEDURE IF EXISTS MigrarAparienciaConfiguracion;

DELIMITER //
CREATE PROCEDURE MigrarAparienciaConfiguracion()
BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM information_schema.COLUMNS
        WHERE TABLE_SCHEMA = DATABASE()
          AND TABLE_NAME = 'configuracion_empresa'
          AND COLUMN_NAME = 'tema_visual'
    ) THEN
        ALTER TABLE configuracion_empresa
            ADD COLUMN tema_visual VARCHAR(30) NOT NULL DEFAULT 'azul';
    END IF;

    IF NOT EXISTS (
        SELECT 1 FROM information_schema.COLUMNS
        WHERE TABLE_SCHEMA = DATABASE()
          AND TABLE_NAME = 'configuracion_empresa'
          AND COLUMN_NAME = 'tema_visual_anterior'
    ) THEN
        ALTER TABLE configuracion_empresa
            ADD COLUMN tema_visual_anterior VARCHAR(30) NULL;
    END IF;

    IF NOT EXISTS (
        SELECT 1 FROM information_schema.COLUMNS
        WHERE TABLE_SCHEMA = DATABASE()
          AND TABLE_NAME = 'configuracion_empresa'
          AND COLUMN_NAME = 'tema_color_acento'
    ) THEN
        ALTER TABLE configuracion_empresa ADD COLUMN tema_color_acento VARCHAR(7) NULL;
    END IF;

    IF NOT EXISTS (
        SELECT 1 FROM information_schema.COLUMNS
        WHERE TABLE_SCHEMA = DATABASE()
          AND TABLE_NAME = 'configuracion_empresa'
          AND COLUMN_NAME = 'tema_color_barra'
    ) THEN
        ALTER TABLE configuracion_empresa ADD COLUMN tema_color_barra VARCHAR(7) NULL;
    END IF;

    IF NOT EXISTS (
        SELECT 1 FROM information_schema.COLUMNS
        WHERE TABLE_SCHEMA = DATABASE()
          AND TABLE_NAME = 'configuracion_empresa'
          AND COLUMN_NAME = 'tema_color_fondo'
    ) THEN
        ALTER TABLE configuracion_empresa ADD COLUMN tema_color_fondo VARCHAR(7) NULL;
    END IF;

    IF NOT EXISTS (
        SELECT 1 FROM information_schema.COLUMNS
        WHERE TABLE_SCHEMA = DATABASE()
          AND TABLE_NAME = 'configuracion_empresa'
          AND COLUMN_NAME = 'tema_anterior_color_acento'
    ) THEN
        ALTER TABLE configuracion_empresa ADD COLUMN tema_anterior_color_acento VARCHAR(7) NULL;
    END IF;

    IF NOT EXISTS (
        SELECT 1 FROM information_schema.COLUMNS
        WHERE TABLE_SCHEMA = DATABASE()
          AND TABLE_NAME = 'configuracion_empresa'
          AND COLUMN_NAME = 'tema_anterior_color_barra'
    ) THEN
        ALTER TABLE configuracion_empresa ADD COLUMN tema_anterior_color_barra VARCHAR(7) NULL;
    END IF;

    IF NOT EXISTS (
        SELECT 1 FROM information_schema.COLUMNS
        WHERE TABLE_SCHEMA = DATABASE()
          AND TABLE_NAME = 'configuracion_empresa'
          AND COLUMN_NAME = 'tema_anterior_color_fondo'
    ) THEN
        ALTER TABLE configuracion_empresa ADD COLUMN tema_anterior_color_fondo VARCHAR(7) NULL;
    END IF;

    IF NOT EXISTS (
        SELECT 1 FROM information_schema.COLUMNS
        WHERE TABLE_SCHEMA = DATABASE()
          AND TABLE_NAME = 'configuracion_empresa'
          AND COLUMN_NAME = 'tema_actualizado_por'
    ) THEN
        ALTER TABLE configuracion_empresa
            ADD COLUMN tema_actualizado_por VARCHAR(255) NULL;
    END IF;

    IF NOT EXISTS (
        SELECT 1 FROM information_schema.COLUMNS
        WHERE TABLE_SCHEMA = DATABASE()
          AND TABLE_NAME = 'configuracion_empresa'
          AND COLUMN_NAME = 'tema_actualizado_en'
    ) THEN
        ALTER TABLE configuracion_empresa
            ADD COLUMN tema_actualizado_en DATETIME NULL;
    END IF;
END//
DELIMITER ;

CALL MigrarAparienciaConfiguracion();
DROP PROCEDURE MigrarAparienciaConfiguracion;

UPDATE configuracion_empresa
SET tema_visual = 'azul'
WHERE id > 0 AND tema_visual = 'enerfone';

UPDATE configuracion_empresa
SET tema_visual_anterior = 'azul'
WHERE id > 0 AND tema_visual_anterior = 'enerfone';