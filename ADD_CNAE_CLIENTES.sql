-- Script para añadir el campo CNAE a los clientes PYME
-- Fecha: 11 de marzo de 2026
-- Descripción: Añade el campo cnae a la tabla clientes_simple
--              para registrar el Código Nacional de Actividades Económicas de las empresas PYME

ALTER TABLE clientes_simple 
ADD COLUMN cnae VARCHAR(10) NULL
COMMENT 'Código Nacional de Actividades Económicas (CNAE) para clientes PYME';
