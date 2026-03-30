-- Script de depuración para el contrato 324
USE enerfone_crm;

-- Ver datos completos del contrato 324
SELECT 
    '=== DATOS DEL CONTRATO 324 ===' AS seccion;

SELECT 
    id,
    tipo,
    Estado,
    Fecha_Alta,
    fecha_activo,
    DATEDIFF(CURDATE(), fecha_activo) AS dias_desde_activacion,
    en_Tarifa AS nombre_tarifa,
    en_tarifa_id,
    Comision
FROM contratos
WHERE id = 324;

-- Buscar tarifas por nombre
SELECT 
    '=== BUSCAR TARIFA POR NOMBRE ===' AS seccion;

SELECT 
    t.id,
    t.nombre,
    t.dias_penalizacion,
    t.tipo_penalizacion,
    t.activa,
    CASE 
        WHEN t.dias_penalizacion IS NULL THEN 'SIN CONFIGURAR'
        WHEN t.dias_penalizacion = 0 THEN 'SIN PENALIZACIÓN'
        ELSE CONCAT('PENALIZACIÓN: ', t.dias_penalizacion, ' días (', COALESCE(t.tipo_penalizacion, 'sin tipo'), ')')
    END AS estado
FROM tarifasenergia t
WHERE t.nombre = (SELECT en_Tarifa FROM contratos WHERE id = 324);

-- Si hay tarifa_id, buscar por ID
SELECT 
    '=== BUSCAR TARIFA POR ID (si existe) ===' AS seccion;

SELECT 
    t.id,
    t.nombre,
    t.dias_penalizacion,
    t.tipo_penalizacion,
    'Búsqueda por ID' AS metodo
FROM tarifasenergia t
WHERE t.id = (SELECT en_tarifa_id FROM contratos WHERE id = 324 AND en_tarifa_id IS NOT NULL);
