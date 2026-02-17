-- Script para añadir segunda línea móvil principal a contratos de telefonía
-- Algunos contratos tienen 2 líneas móviles incluidas en la tarifa principal

-- Añadir columnas para segunda línea móvil principal
ALTER TABLE contratos 
ADD COLUMN linea_movil_principal_2 VARCHAR(50) NULL AFTER codigo_icc_principal;

ALTER TABLE contratos 
ADD COLUMN tipo_linea_movil_principal_2 VARCHAR(20) NULL AFTER linea_movil_principal_2;

ALTER TABLE contratos 
ADD COLUMN codigo_icc_principal_2 VARCHAR(19) NULL AFTER tipo_linea_movil_principal_2;
