-- Script para agregar el campo fecha_activo a la tabla contratos
-- Este campo almacena la fecha en que el contrato se puso en estado Activo

ALTER TABLE contratos 
ADD COLUMN fecha_activo DATE NULL 
COMMENT 'Fecha en que el contrato se activó';
