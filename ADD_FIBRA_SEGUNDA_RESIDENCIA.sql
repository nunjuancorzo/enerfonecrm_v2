-- Script para añadir campos de Fibra Segunda Residencia a contratos de telefonía
-- Ejecutar este script en la base de datos MySQL

USE enerfonecrm;

-- Añadir columna para dirección de segunda residencia
ALTER TABLE contratos 
ADD COLUMN direccion_segunda_residencia VARCHAR(500) NULL AFTER Factura;

-- Añadir columna para tarifa de fibra segunda residencia
ALTER TABLE contratos 
ADD COLUMN tarifa_fibra_segunda_residencia VARCHAR(255) NULL AFTER direccion_segunda_residencia;
