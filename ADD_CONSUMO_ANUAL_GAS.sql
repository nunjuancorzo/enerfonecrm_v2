-- Script para separar consumo anual de luz y gas en contratos de energía
-- Fecha: 2026-03-20

-- Agregar campo para consumo anual de gas (si falla es porque ya existe, se puede ignorar)
ALTER TABLE contratos 
ADD COLUMN consumo_anual_gas DECIMAL(18, 2) NULL
COMMENT 'Consumo anual de gas en kWh (separado del consumo de luz)';

-- Renombrar conceptualmente el campo existente (mantiene compatibilidad)
-- consumo_ultimos_12_meses ahora representa solo el consumo de luz
ALTER TABLE contratos 
MODIFY COLUMN consumo_ultimos_12_meses DECIMAL(18, 2) NULL
COMMENT 'Consumo anual de luz en kWh';
