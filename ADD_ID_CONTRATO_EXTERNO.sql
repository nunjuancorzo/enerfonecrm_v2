-- Script para añadir el campo ID Contrato Externo a los contratos
-- Fecha: 11 de marzo de 2026
-- Descripción: Añade el campo id_contrato_externo a la tabla contratos
--              para registrar el identificador del contrato proporcionado por la comercializadora/operadora

ALTER TABLE contratos 
ADD COLUMN id_contrato_externo VARCHAR(100) NULL
COMMENT 'Identificador del contrato proporcionado por la comercializadora/operadora externa';
