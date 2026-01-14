-- Script para agregar campos adicionales a la tabla usuarios
-- Ejecutar este script en la base de datos MySQL

ALTER TABLE usuarios 
ADD COLUMN nombre VARCHAR(100) NULL AFTER username,
ADD COLUMN apellidos VARCHAR(100) NULL AFTER nombre,
ADD COLUMN direccion VARCHAR(255) NULL AFTER apellidos,
ADD COLUMN codigo_postal VARCHAR(10) NULL AFTER direccion,
ADD COLUMN localidad VARCHAR(100) NULL AFTER codigo_postal;
