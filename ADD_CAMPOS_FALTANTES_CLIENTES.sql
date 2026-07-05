-- Script para agregar campos faltantes a la tabla clientes_simple
-- Ejecutar este script si los campos no existen en tu base de datos

USE [nombre_de_tu_base_de_datos];
GO

-- Procedimiento para agregar columnas solo si no existen
DELIMITER //

CREATE PROCEDURE AddClientesFieldsIfNotExists()
BEGIN
    DECLARE CONTINUE HANDLER FOR SQLEXCEPTION BEGIN END;
    
    -- Agregar campo apellidos si no existe
    IF NOT EXISTS (SELECT * FROM information_schema.COLUMNS 
                   WHERE TABLE_SCHEMA = DATABASE() 
                   AND TABLE_NAME = 'clientes_simple' 
                   AND COLUMN_NAME = 'apellidos') THEN
        ALTER TABLE clientes_simple ADD COLUMN apellidos VARCHAR(255) NULL;
    END IF;
    
    -- Agregar campo empresa si no existe
    IF NOT EXISTS (SELECT * FROM information_schema.COLUMNS 
                   WHERE TABLE_SCHEMA = DATABASE() 
                   AND TABLE_NAME = 'clientes_simple' 
                   AND COLUMN_NAME = 'empresa') THEN
        ALTER TABLE clientes_simple ADD COLUMN empresa VARCHAR(255) NULL;
    END IF;
    
    -- Agregar campo cif si no existe
    IF NOT EXISTS (SELECT * FROM information_schema.COLUMNS 
                   WHERE TABLE_SCHEMA = DATABASE() 
                   AND TABLE_NAME = 'clientes_simple' 
                   AND COLUMN_NAME = 'cif') THEN
        ALTER TABLE clientes_simple ADD COLUMN cif VARCHAR(20) NULL;
    END IF;
    
    -- Agregar campo ciudad si no existe
    IF NOT EXISTS (SELECT * FROM information_schema.COLUMNS 
                   WHERE TABLE_SCHEMA = DATABASE() 
                   AND TABLE_NAME = 'clientes_simple' 
                   AND COLUMN_NAME = 'ciudad') THEN
        ALTER TABLE clientes_simple ADD COLUMN ciudad VARCHAR(100) NULL;
    END IF;
    
    -- Agregar campo pais si no existe
    IF NOT EXISTS (SELECT * FROM information_schema.COLUMNS 
                   WHERE TABLE_SCHEMA = DATABASE() 
                   AND TABLE_NAME = 'clientes_simple' 
                   AND COLUMN_NAME = 'pais') THEN
        ALTER TABLE clientes_simple ADD COLUMN pais VARCHAR(100) NULL DEFAULT 'España';
    END IF;
    
END //

DELIMITER ;

-- Ejecutar el procedimiento
CALL AddClientesFieldsIfNotExists();

-- Eliminar el procedimiento
DROP PROCEDURE AddClientesFieldsIfNotExists;

-- Verificar que se agregaron correctamente
DESCRIBE clientes_simple;
