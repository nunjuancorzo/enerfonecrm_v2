-- ================================================================
-- SCRIPT: Migrar relación Usuario-Contrato de nombre a ID
-- FECHA: 2026-07-02
-- DESCRIPCIÓN: 
--   Actualiza todos los contratos que tienen el nombre del comercial
--   pero no tienen usuario_comercializadora_id, estableciendo la FK
--   correcta buscando el usuario por su nombre.
-- ================================================================

-- PASO 1: Verificar estado actual
SELECT 
    COUNT(*) as total_contratos,
    SUM(CASE WHEN comercial IS NOT NULL AND comercial != '' THEN 1 ELSE 0 END) as con_nombre_comercial,
    SUM(CASE WHEN usuario_comercializadora_id IS NOT NULL THEN 1 ELSE 0 END) as con_id_usuario,
    SUM(CASE WHEN comercial IS NOT NULL AND comercial != '' AND usuario_comercializadora_id IS NULL THEN 1 ELSE 0 END) as necesitan_migracion
FROM contratos;

-- PASO 2: Ver qué usuarios se encuentran en contratos
SELECT 
    c.comercial,
    COUNT(*) as num_contratos,
    MAX(u.idusuarios) as usuario_id,
    MAX(u.username) as username,
    CASE WHEN MAX(u.idusuarios) IS NULL THEN 'NO EXISTE' ELSE 'OK' END as estado
FROM contratos c
LEFT JOIN usuarios u ON c.comercial = u.username
WHERE c.comercial IS NOT NULL AND c.comercial != ''
  AND c.usuario_comercializadora_id IS NULL
GROUP BY c.comercial
ORDER BY num_contratos DESC;

-- PASO 3: ACTUALIZAR los contratos que tienen nombre pero no ID
-- Esto relaciona el contrato con el usuario por ID en lugar de por nombre

-- Deshabilitar modo seguro temporalmente
SET SQL_SAFE_UPDATES = 0;

UPDATE contratos c
INNER JOIN usuarios u ON c.comercial = u.username
SET c.usuario_comercializadora_id = u.idusuarios,
    c.fecha_modificacion = NOW()
WHERE c.comercial IS NOT NULL 
  AND c.comercial != ''
  AND c.usuario_comercializadora_id IS NULL
  AND u.idusuarios IS NOT NULL;

-- Rehabilitar modo seguro
SET SQL_SAFE_UPDATES = 1;

-- PASO 4: Verificar resultado
SELECT 
    COUNT(*) as total_contratos,
    SUM(CASE WHEN comercial IS NOT NULL AND comercial != '' THEN 1 ELSE 0 END) as con_nombre_comercial,
    SUM(CASE WHEN usuario_comercializadora_id IS NOT NULL THEN 1 ELSE 0 END) as con_id_usuario,
    SUM(CASE WHEN comercial IS NOT NULL AND comercial != '' AND usuario_comercializadora_id IS NULL THEN 1 ELSE 0 END) as sin_usuario_valido
FROM contratos;

-- PASO 5: Ver contratos que tienen nombre pero no se pudo encontrar el usuario
SELECT 
    id,
    tipo,
    comercial,
    nombre_cliente,
    fecha_creacion,
    estado
FROM contratos
WHERE comercial IS NOT NULL 
  AND comercial != ''
  AND usuario_comercializadora_id IS NULL
ORDER BY fecha_creacion DESC
LIMIT 20;

-- ================================================================
-- NOTAS:
-- - Los contratos que tienen un nombre de comercial que no existe
--   en la tabla usuarios quedarán con usuario_comercializadora_id = NULL
-- - Esos contratos deben ser revisados manualmente
-- - A partir de ahora, SIEMPRE se debe guardar usuario_comercializadora_id
--   cuando se crea o edita un contrato
-- ================================================================
