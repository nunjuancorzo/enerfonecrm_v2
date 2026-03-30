USE enerfone_crm;

-- Ver datos completos del contrato 324
SELECT 
    id,
    tipo,
    en_Tarifa,
    en_tarifa_id,
    fecha_activo,
    Estado,
    Comision
FROM contratos 
WHERE id = 324;

-- Ver la tarifa que debería corresponder
SELECT 
    id,
    nombre,
    dias_penalizacion,
    tipo_penalizacion,
    'Búsqueda por nombre' as metodo
FROM tarifasenergia
WHERE nombre = (SELECT en_Tarifa FROM contratos WHERE id = 324);

-- Verificar si hay coincidencia exacta de nombre
SELECT 
    'Verificación de coincidencia' as titulo,
    c.en_Tarifa as nombre_en_contrato,
    t.nombre as nombre_en_tarifa,
    CASE 
        WHEN c.en_Tarifa = t.nombre THEN 'COINCIDE EXACTAMENTE'
        ELSE 'NO COINCIDE'
    END as estado_coincidencia
FROM contratos c
LEFT JOIN tarifasenergia t ON c.en_Tarifa = t.nombre
WHERE c.id = 324;
