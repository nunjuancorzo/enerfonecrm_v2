-- Script para agregar campos detallados de dirección a la tabla clientes_simple
-- Fecha: 29/12/2025

-- Agregar columnas nuevas para dirección detallada
-- Nota: Si la columna ya existe, se producirá un error que puedes ignorar
ALTER TABLE clientes_simple
ADD COLUMN numero VARCHAR(20) AFTER direccion;

ALTER TABLE clientes_simple
ADD COLUMN escalera VARCHAR(10) AFTER numero;

ALTER TABLE clientes_simple
ADD COLUMN piso VARCHAR(10) AFTER escalera;

ALTER TABLE clientes_simple
ADD COLUMN puerta VARCHAR(10) AFTER piso;

ALTER TABLE clientes_simple
ADD COLUMN aclarador VARCHAR(255) AFTER puerta;

-- Actualizar comentarios para documentación
ALTER TABLE clientes_simple 
MODIFY COLUMN direccion VARCHAR(500) COMMENT 'Dirección de suministro (obligatorio)',
MODIFY COLUMN numero VARCHAR(20) COMMENT 'Número de la dirección (obligatorio)',
MODIFY COLUMN escalera VARCHAR(10) COMMENT 'Escalera (opcional)',
MODIFY COLUMN piso VARCHAR(10) COMMENT 'Piso (opcional)',
MODIFY COLUMN puerta VARCHAR(10) COMMENT 'Puerta (opcional)',
MODIFY COLUMN aclarador VARCHAR(255) COMMENT 'Aclarador de dirección (opcional)',
MODIFY COLUMN codigo_postal VARCHAR(10) COMMENT 'Código postal (obligatorio)',
MODIFY COLUMN provincia VARCHAR(100) COMMENT 'Provincia (obligatorio)',
MODIFY COLUMN poblacion VARCHAR(100) COMMENT 'Localidad (obligatorio)';
