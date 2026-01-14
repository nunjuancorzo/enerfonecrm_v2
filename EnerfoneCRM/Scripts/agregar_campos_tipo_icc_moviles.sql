-- Script para agregar campos de tipo de línea móvil y código ICC
-- para contratos de telefonía

-- Agregar campos para la línea móvil principal
ALTER TABLE contratos ADD COLUMN tipo_linea_movil_principal VARCHAR(20) NULL COMMENT 'Tipo de línea: Contrato o Prepago';
ALTER TABLE contratos ADD COLUMN codigo_icc_principal VARCHAR(19) NULL COMMENT 'Código ICC de 19 dígitos (89 34 XX...)';

-- Agregar campos para línea adicional 1
ALTER TABLE contratos ADD COLUMN tipo_linea1_tel VARCHAR(20) NULL COMMENT 'Tipo de línea adicional 1';
ALTER TABLE contratos ADD COLUMN codigo_icc_linea1_tel VARCHAR(19) NULL COMMENT 'Código ICC línea adicional 1';

-- Agregar campos para línea adicional 2
ALTER TABLE contratos ADD COLUMN tipo_linea2_tel VARCHAR(20) NULL COMMENT 'Tipo de línea adicional 2';
ALTER TABLE contratos ADD COLUMN codigo_icc_linea2_tel VARCHAR(19) NULL COMMENT 'Código ICC línea adicional 2';

-- Agregar campos para línea adicional 3
ALTER TABLE contratos ADD COLUMN tipo_linea3_tel VARCHAR(20) NULL COMMENT 'Tipo de línea adicional 3';
ALTER TABLE contratos ADD COLUMN codigo_icc_linea3_tel VARCHAR(19) NULL COMMENT 'Código ICC línea adicional 3';

-- Agregar campos para línea adicional 4
ALTER TABLE contratos ADD COLUMN tipo_linea4_tel VARCHAR(20) NULL COMMENT 'Tipo de línea adicional 4';
ALTER TABLE contratos ADD COLUMN codigo_icc_linea4_tel VARCHAR(19) NULL COMMENT 'Código ICC línea adicional 4';

-- Agregar campos para línea adicional 5
ALTER TABLE contratos ADD COLUMN tipo_linea5_tel VARCHAR(20) NULL COMMENT 'Tipo de línea adicional 5';
ALTER TABLE contratos ADD COLUMN codigo_icc_linea5_tel VARCHAR(19) NULL COMMENT 'Código ICC línea adicional 5';
