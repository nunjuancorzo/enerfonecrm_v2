-- Script para agregar el campo empresa_alarma a la tabla contratos
-- Fecha de creación: 19/01/2026
-- Descripción: Almacena la empresa de alarma seleccionada en contratos de tipo alarma

ALTER TABLE contratos 
ADD COLUMN empresa_alarma VARCHAR(255) NULL 
AFTER campana_alarma;

-- Crear índice para mejorar las consultas
CREATE INDEX idx_contratos_empresa_alarma ON contratos(empresa_alarma);
