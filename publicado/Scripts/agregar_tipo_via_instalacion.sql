-- Script para agregar el campo tipo_via a las tablas clientes y contratos
-- Fecha: 3 de febrero de 2026
-- Descripción: Añade el campo para almacenar el tipo de vía (CALLE, AVENIDA, etc.)

USE enerfonecrm;

-- Agregar columna tipo_via a la tabla clientes después de telefono
ALTER TABLE clientes 
ADD COLUMN tipo_via VARCHAR(50) NULL;

-- Agregar columna tipo_via_instalacion a la tabla contratos después de empresa_alarma
ALTER TABLE contratos 
ADD COLUMN tipo_via_instalacion VARCHAR(50) NULL;