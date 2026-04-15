-- Agregar columna observaciones_estado a la tabla contratos
-- Esta columna almacena las observaciones sobre el estado del contrato
-- Es obligatoria para contratos con estado "Cancelado" y opcional para otros estados

ALTER TABLE contratos 
ADD COLUMN observaciones_estado TEXT;

-- Comentario de la columna
COMMENT ON COLUMN contratos.observaciones_estado IS 'Observaciones del estado del contrato. Obligatorio para estado Cancelado, opcional para otros estados';
