-- Script para agregar el campo DNI Representante a la tabla clientes_simple
-- Ejecutar este script en la base de datos de producción

ALTER TABLE clientes_simple
ADD COLUMN dni_representante VARCHAR(50) NULL;
