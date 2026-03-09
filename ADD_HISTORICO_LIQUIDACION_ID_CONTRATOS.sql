-- Script para vincular contratos a liquidaciones específicas
-- Agrega campo historico_liquidacion_id a la tabla contratos

USE corcrmv2;

-- Agregar columna para vincular contratos a liquidaciones
ALTER TABLE contratos
ADD COLUMN historico_liquidacion_id INT NULL
COMMENT 'ID de la liquidación a la que pertenece este contrato';

-- Crear índice para mejorar el rendimiento de las consultas
CREATE INDEX idx_contratos_historico_liquidacion 
ON contratos(historico_liquidacion_id);

-- Agregar foreign key constraint (opcional, comentado por si hay datos legacy)
-- ALTER TABLE contratos
-- ADD CONSTRAINT fk_contratos_historico_liquidacion
-- FOREIGN KEY (historico_liquidacion_id) 
-- REFERENCES historico_liquidaciones(id)
-- ON DELETE SET NULL;

SELECT 'Campo historico_liquidacion_id agregado exitosamente a la tabla contratos' AS resultado;
