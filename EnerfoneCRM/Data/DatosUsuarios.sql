-- Script para añadir campos de información bancaria y documentación a la tabla usuarios

USE enerfonecrm;

-- Añadir campos de datos bancarios y tipo de entidad
ALTER TABLE usuarios 
ADD COLUMN numero_cuenta VARCHAR(34) NULL COMMENT 'IBAN del usuario',
ADD COLUMN tipo_entidad ENUM('Autónomo', 'PYME', 'N/A') DEFAULT 'N/A' COMMENT 'Tipo de entidad del usuario';

-- Añadir campos para documentación adjunta
ALTER TABLE usuarios
ADD COLUMN archivo_dni VARCHAR(255) NULL COMMENT 'Ruta del archivo DNI',
ADD COLUMN archivo_cif VARCHAR(255) NULL COMMENT 'Ruta del archivo CIF',
ADD COLUMN archivo_poder VARCHAR(255) NULL COMMENT 'Ruta del archivo Poder/Escrituras',
ADD COLUMN archivo_contrato VARCHAR(255) NULL COMMENT 'Ruta del archivo Contrato de Colaboración';
