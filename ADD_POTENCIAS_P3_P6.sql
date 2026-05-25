-- Script para agregar campos de potencia contratada P3, P4, P5, P6
-- Para contratos de energía con tarifas que requieren hasta 6 periodos
-- NOTA: Si las columnas ya existen, el script fallará. Esto es normal.

ALTER TABLE contratos
ADD COLUMN potencia_contratada_p3 DECIMAL(10,2) NULL COMMENT 'Potencia contratada periodo 3 (kW)',
ADD COLUMN potencia_contratada_p4 DECIMAL(10,2) NULL COMMENT 'Potencia contratada periodo 4 (kW)',
ADD COLUMN potencia_contratada_p5 DECIMAL(10,2) NULL COMMENT 'Potencia contratada periodo 5 (kW)',
ADD COLUMN potencia_contratada_p6 DECIMAL(10,2) NULL COMMENT 'Potencia contratada periodo 6 (kW)';

-- Mensaje informativo
SELECT 'Campos de potencia contratada P3, P4, P5, P6 agregados correctamente a contratos' as Resultado;
