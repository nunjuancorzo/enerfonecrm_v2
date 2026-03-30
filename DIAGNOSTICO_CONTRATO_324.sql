-- Script de diagnóstico para el contrato 324
-- Verifica la configuración de penalización

USE enerfone_crm;

-- Información del contrato 324
SELECT 
    '=== CONTRATO 324 ===' AS seccion,
    c.id,
    c.tipo,
    c.Estado,
    c.Fecha_Alta,
    DATEDIFF(NOW(), c.Fecha_Alta) AS dias_desde_alta,
    c.en_Tarifa,
    c.en_tarifa_id,
    c.Tarifa_tel,
    c.tarifa_tel_id,
    c.kit_alarma,
    c.kit_alarma_id
FROM contratos c
WHERE c.id = 324;

-- Si es energía, verificar la tarifa
SELECT 
    '=== TARIFA ENERGÍA (si aplica) ===' AS seccion,
    t.id AS tarifa_id,
    t.nombre AS tarifa_nombre,
    t.dias_penalizacion,
    t.tipo_penalizacion,
    CASE 
        WHEN t.dias_penalizacion IS NULL THEN '⚠️  NO CONFIGURADA - Necesita editar la tarifa y agregar días de penalización'
        WHEN t.dias_penalizacion > 0 THEN CONCAT('✓ Configurada: ', t.dias_penalizacion, ' días (', COALESCE(t.tipo_penalizacion, 'sin tipo'), ')')
        ELSE 'Sin penalización'
    END AS estado_penalizacion
FROM tarifasenergia t
INNER JOIN contratos c ON c.en_tarifa_id = t.id
WHERE c.id = 324 AND c.tipo = 'Energia';

-- Si es telefonía, verificar la tarifa
SELECT 
    '=== TARIFA TELEFONÍA (si aplica) ===' AS seccion,
    t.id AS tarifa_id,
    t.tarifa AS tarifa_nombre,
    t.dias_penalizacion,
    t.tipo_penalizacion,
    CASE 
        WHEN t.dias_penalizacion IS NULL THEN '⚠️  NO CONFIGURADA - Necesita editar la tarifa y agregar días de penalización'
        WHEN t.dias_penalizacion > 0 THEN CONCAT('✓ Configurada: ', t.dias_penalizacion, ' días (', COALESCE(t.tipo_penalizacion, 'sin tipo'), ')')
        ELSE 'Sin penalización'
    END AS estado_penalizacion
FROM tarifastelefonia t
INNER JOIN contratos c ON c.tarifa_tel_id = t.id
WHERE c.id = 324 AND c.tipo = 'Telefonia';

-- Si es alarma, verificar la tarifa
SELECT 
    '=== TARIFA ALARMA (si aplica) ===' AS seccion,
    t.id AS tarifa_id,
    t.nombre_tarifa AS tarifa_nombre,
    t.dias_penalizacion,
    t.tipo_penalizacion,
    CASE 
        WHEN t.dias_penalizacion IS NULL THEN '⚠️  NO CONFIGURADA - Necesita editar la tarifa y agregar días de penalización'
        WHEN t.dias_penalizacion > 0 THEN CONCAT('✓ Configurada: ', t.dias_penalizacion, ' días (', COALESCE(t.tipo_penalizacion, 'sin tipo'), ')')
        ELSE 'Sin penalización'
    END AS estado_penalizacion
FROM tarifas_alarmas t
INNER JOIN contratos c ON c.kit_alarma_id = t.id
WHERE c.id = 324 AND c.tipo = 'Alarma';

-- Resumen de todas las tarifas con penalización configurada
SELECT 
    '=== TARIFAS CON PENALIZACIÓN CONFIGURADA ===' AS seccion;

SELECT 
    'Energía' AS tipo_tarifa,
    COUNT(*) AS total_tarifas,
    SUM(CASE WHEN dias_penalizacion IS NOT NULL AND dias_penalizacion > 0 THEN 1 ELSE 0 END) AS con_penalizacion,
    SUM(CASE WHEN dias_penalizacion IS NULL THEN 1 ELSE 0 END) AS sin_configurar
FROM tarifasenergia
WHERE activa = 1

UNION ALL

SELECT 
    'Telefonía' AS tipo_tarifa,
    COUNT(*) AS total_tarifas,
    SUM(CASE WHEN dias_penalizacion IS NOT NULL AND dias_penalizacion > 0 THEN 1 ELSE 0 END) AS con_penalizacion,
    SUM(CASE WHEN dias_penalizacion IS NULL THEN 1 ELSE 0 END) AS sin_configurar
FROM tarifastelefonia
WHERE activa = 1

UNION ALL

SELECT 
    'Alarma' AS tipo_tarifa,
    COUNT(*) AS total_tarifas,
    SUM(CASE WHEN dias_penalizacion IS NOT NULL AND dias_penalizacion > 0 THEN 1 ELSE 0 END) AS con_penalizacion,
    SUM(CASE WHEN dias_penalizacion IS NULL THEN 1 ELSE 0 END) AS sin_configurar
FROM tarifas_alarmas
WHERE activa = 1;
