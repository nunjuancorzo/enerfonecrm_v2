-- Agregar campo usuario_comercializadora_id a la tabla contratos
ALTER TABLE contratos 
ADD COLUMN usuario_comercializadora_id INT NULL;

-- Agregar comentario descriptivo
ALTER TABLE contratos 
MODIFY COLUMN usuario_comercializadora_id INT NULL COMMENT 'ID del usuario comercializadora asignado al contrato';
