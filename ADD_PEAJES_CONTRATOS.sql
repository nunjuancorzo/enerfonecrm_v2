-- Script para añadir las columnas peaje_luz y peaje_gas a la tabla contratos
-- Fecha: 2 marzo 2026
-- Objetivo: Separar peajes de luz y gas en contratos de energía

-- ============================================================
-- Añadir columnas peaje_luz y peaje_gas
-- ============================================================

ALTER TABLE contratos 
ADD COLUMN peaje_luz VARCHAR(50) NULL AFTER consumo_ultimos_12_meses;

ALTER TABLE contratos 
ADD COLUMN peaje_gas VARCHAR(50) NULL AFTER peaje_luz;

-- ============================================================
-- Verificación
-- ============================================================

SELECT 
    'Columnas peaje_luz y peaje_gas añadidas correctamente' AS resultado;

-- Ver estructura de la tabla
DESCRIBE contratos;

-- Ver muestra de registros de contratos de energía
SELECT 
    id,
    tipo,
    en_Comercializadora,
    en_Tarifa,
    peaje_luz,
    peaje_gas,
    estado
FROM contratos
WHERE tipo = 'energia'
LIMIT 10;
