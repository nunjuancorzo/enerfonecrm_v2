-- Script para añadir la columna peaje_gas a la tabla tarifasenergia
-- Fecha: 2 marzo 2026
-- Objetivo: Separar peajes de luz y gas en columnas distintas

-- ============================================================
-- Añadir columna peaje_gas
-- ============================================================

ALTER TABLE tarifasenergia 
ADD COLUMN peaje_gas VARCHAR(50) NULL AFTER peaje;

-- ============================================================
-- Verificación
-- ============================================================

SELECT 
    'Columna peaje_gas añadida correctamente' AS resultado;

-- Ver estructura de la tabla
DESCRIBE tarifasenergia;

-- Ver muestra de registros
SELECT 
    id,
    empresa,
    nombre,
    peaje,
    peaje_gas,
    tipo
FROM tarifasenergia
LIMIT 10;
