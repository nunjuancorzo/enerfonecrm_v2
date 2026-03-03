-- Script para normalizar tarifas de energía existentes
-- Fecha: 27 febrero 2026
-- Actualizado: 2 marzo 2026
-- Objetivo: 
--   1. Normalizar nombres de comercializadoras a Title Case
--   2. Formatear potencias y energías con 6 decimales y coma como separador
--   3. Corregir Tipo Energía: solo LUZ, GAS o LUZ+GAS
--   4. Corregir Tipo Cliente: solo Residencial o Pyme

-- ============================================================
-- PASO 1: Normalizar nombres de comercializadoras a Title Case
-- ============================================================

UPDATE tarifasenergia 
SET empresa = CONCAT(
    UPPER(SUBSTRING(empresa, 1, 1)),
    LOWER(SUBSTRING(empresa, 2))
)
WHERE empresa IS NOT NULL
  AND id > 0;

-- ============================================================
-- PASO 1B: Corregir Tipo Energía (mover Residencial/Pyme a tipo_cliente)
-- ============================================================

-- Si tipo='Residencial' o 'Pyme', moverlo a tipo_cliente y cambiar tipo a 'LUZ'
UPDATE tarifasenergia 
SET tipo_cliente = CASE 
    WHEN LOWER(tipo) = 'residencial' THEN 'Residencial'
    WHEN LOWER(tipo) = 'pyme' THEN 'Pyme'
    ELSE tipo_cliente
END,
tipo = 'LUZ'
WHERE (LOWER(tipo) = 'residencial' OR LOWER(tipo) = 'pyme')
  AND id > 0;

-- Normalizar valores de Tipo Energía
UPDATE tarifasenergia 
SET tipo = CASE 
    WHEN UPPER(tipo) = 'LUZ' THEN 'LUZ'
    WHEN UPPER(tipo) = 'GAS' THEN 'GAS'
    WHEN UPPER(tipo) LIKE '%LUZ%' AND UPPER(tipo) LIKE '%GAS%' THEN 'LUZ+GAS'
    ELSE tipo
END
WHERE id > 0;

-- ============================================================
-- PASO 1C: Corregir Tipo Cliente (normalizar valores)
-- ============================================================

UPDATE tarifasenergia 
SET tipo_cliente = CASE 
    WHEN LOWER(tipo_cliente) = 'residencial' OR LOWER(tipo_cliente) = 'residencial' THEN 'Residencial'
    WHEN LOWER(tipo_cliente) IN ('pyme', 'pymes', 'empresa') THEN 'Pyme'
    ELSE tipo_cliente
END
WHERE tipo_cliente IS NOT NULL
  AND id > 0;

-- ============================================================
-- PASO 2: Formatear Potencias con 6 decimales y coma
-- ============================================================

-- Potencia 1
UPDATE tarifasenergia 
SET potencia1 = REPLACE(FORMAT(CAST(REPLACE(potencia1, ',', '.') AS DECIMAL(20,6)), 6, 'es_ES'), '.', ',')
WHERE potencia1 IS NOT NULL 
  AND potencia1 != ''
  AND CAST(REPLACE(potencia1, ',', '.') AS DECIMAL(20,6)) IS NOT NULL
  AND id > 0;

-- Potencia 2
UPDATE tarifasenergia 
SET potencia2 = REPLACE(FORMAT(CAST(REPLACE(potencia2, ',', '.') AS DECIMAL(20,6)), 6, 'es_ES'), '.', ',')
WHERE potencia2 IS NOT NULL 
  AND potencia2 != ''
  AND CAST(REPLACE(potencia2, ',', '.') AS DECIMAL(20,6)) IS NOT NULL
  AND id > 0;

-- Potencia 3
UPDATE tarifasenergia 
SET potencia3 = REPLACE(FORMAT(CAST(REPLACE(potencia3, ',', '.') AS DECIMAL(20,6)), 6, 'es_ES'), '.', ',')
WHERE potencia3 IS NOT NULL 
  AND potencia3 != ''
  AND CAST(REPLACE(potencia3, ',', '.') AS DECIMAL(20,6)) IS NOT NULL
  AND id > 0;

-- Potencia 4
UPDATE tarifasenergia 
SET potencia4 = REPLACE(FORMAT(CAST(REPLACE(potencia4, ',', '.') AS DECIMAL(20,6)), 6, 'es_ES'), '.', ',')
WHERE potencia4 IS NOT NULL 
  AND potencia4 != ''
  AND CAST(REPLACE(potencia4, ',', '.') AS DECIMAL(20,6)) IS NOT NULL
  AND id > 0;

-- Potencia 5
UPDATE tarifasenergia 
SET potencia5 = REPLACE(FORMAT(CAST(REPLACE(potencia5, ',', '.') AS DECIMAL(20,6)), 6, 'es_ES'), '.', ',')
WHERE potencia5 IS NOT NULL 
  AND potencia5 != ''
  AND CAST(REPLACE(potencia5, ',', '.') AS DECIMAL(20,6)) IS NOT NULL
  AND id > 0;

-- Potencia 6
UPDATE tarifasenergia 
SET potencia6 = REPLACE(FORMAT(CAST(REPLACE(potencia6, ',', '.') AS DECIMAL(20,6)), 6, 'es_ES'), '.', ',')
WHERE potencia6 IS NOT NULL 
  AND potencia6 != ''
  AND CAST(REPLACE(potencia6, ',', '.') AS DECIMAL(20,6)) IS NOT NULL
  AND id > 0;

-- ============================================================
-- PASO 3: Formatear Energías con 6 decimales y coma
-- ============================================================

-- Energía 1
UPDATE tarifasenergia 
SET energia1 = REPLACE(FORMAT(CAST(REPLACE(energia1, ',', '.') AS DECIMAL(20,6)), 6, 'es_ES'), '.', ',')
WHERE energia1 IS NOT NULL 
  AND energia1 != ''
  AND CAST(REPLACE(energia1, ',', '.') AS DECIMAL(20,6)) IS NOT NULL
  AND id > 0;

-- Energía 2
UPDATE tarifasenergia 
SET energia2 = REPLACE(FORMAT(CAST(REPLACE(energia2, ',', '.') AS DECIMAL(20,6)), 6, 'es_ES'), '.', ',')
WHERE energia2 IS NOT NULL 
  AND energia2 != ''
  AND CAST(REPLACE(energia2, ',', '.') AS DECIMAL(20,6)) IS NOT NULL
  AND id > 0;

-- Energía 3
UPDATE tarifasenergia 
SET energia3 = REPLACE(FORMAT(CAST(REPLACE(energia3, ',', '.') AS DECIMAL(20,6)), 6, 'es_ES'), '.', ',')
WHERE energia3 IS NOT NULL 
  AND energia3 != ''
  AND CAST(REPLACE(energia3, ',', '.') AS DECIMAL(20,6)) IS NOT NULL
  AND id > 0;

-- Energía 4
UPDATE tarifasenergia 
SET energia4 = REPLACE(FORMAT(CAST(REPLACE(energia4, ',', '.') AS DECIMAL(20,6)), 6, 'es_ES'), '.', ',')
WHERE energia4 IS NOT NULL 
  AND energia4 != ''
  AND CAST(REPLACE(energia4, ',', '.') AS DECIMAL(20,6)) IS NOT NULL
  AND id > 0;

-- Energía 5
UPDATE tarifasenergia 
SET energia5 = REPLACE(FORMAT(CAST(REPLACE(energia5, ',', '.') AS DECIMAL(20,6)), 6, 'es_ES'), '.', ',')
WHERE energia5 IS NOT NULL 
  AND energia5 != ''
  AND CAST(REPLACE(energia5, ',', '.') AS DECIMAL(20,6)) IS NOT NULL
  AND id > 0;

-- Energía 6
UPDATE tarifasenergia 
SET energia6 = REPLACE(FORMAT(CAST(REPLACE(energia6, ',', '.') AS DECIMAL(20,6)), 6, 'es_ES'), '.', ',')
WHERE energia6 IS NOT NULL 
  AND energia6 != ''
  AND CAST(REPLACE(energia6, ',', '.') AS DECIMAL(20,6)) IS NOT NULL
  AND id > 0;

-- ============================================================
-- Verificación
-- ============================================================

SELECT 
    'Normalización completada' AS resultado,
    COUNT(*) AS total_tarifas,
    COUNT(DISTINCT empresa) AS total_empresas
FROM tarifasenergia;

-- Ver muestra de 5 registros actualizados
SELECT 
    empresa,
    tipo,
    nombre,
    potencia1,
    potencia2,
    energia1,
    energia2
FROM tarifasenergia
LIMIT 5;
