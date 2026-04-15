-- Script para migrar datos del campo 'direccion' a 'direccion_instalacion_alarma' en contratos de energía
-- Fecha: 2024
-- Descripción: Mueve la dirección general a la dirección de suministro para evitar duplicados

-- Verificar si existe el campo direccion_instalacion_alarma
SELECT COUNT(*) as campo_existe 
FROM information_schema.COLUMNS 
WHERE TABLE_SCHEMA = 'enerfoneCRM' 
AND TABLE_NAME = 'contratos' 
AND COLUMN_NAME = 'direccion_instalacion_alarma';

-- Verificar cuántos registros se actualizarán
SELECT COUNT(*) as registros_a_actualizar
FROM contratos
WHERE tipo = 'energia'
AND direccion IS NOT NULL
AND direccion != ''
AND (direccion_instalacion_alarma IS NULL OR direccion_instalacion_alarma = '');

-- Mostrar algunos ejemplos de lo que se va a migrar
SELECT id, nombre_cliente, direccion, direccion_instalacion_alarma
FROM contratos
WHERE tipo = 'energia'
AND direccion IS NOT NULL
AND direccion != ''
AND (direccion_instalacion_alarma IS NULL OR direccion_instalacion_alarma = '')
LIMIT 10;

-- Desactivar el modo de actualización segura temporalmente
SET SQL_SAFE_UPDATES = 0;

-- Realizar la migración: copiar direccion a direccion_instalacion_alarma
-- Solo para contratos de energía donde direccion_instalacion_alarma esté vacía
UPDATE contratos
SET direccion_instalacion_alarma = direccion
WHERE tipo = 'energia'
AND direccion IS NOT NULL
AND direccion != ''
AND (direccion_instalacion_alarma IS NULL OR direccion_instalacion_alarma = '');

-- Reactivar el modo de actualización segura
SET SQL_SAFE_UPDATES = 1;

-- Verificar los resultados
SELECT COUNT(*) as registros_migrados
FROM contratos
WHERE tipo = 'energia'
AND direccion_instalacion_alarma IS NOT NULL
AND direccion_instalacion_alarma != '';

-- Mostrar algunos ejemplos después de la migración
SELECT id, nombre_cliente, direccion, direccion_instalacion_alarma
FROM contratos
WHERE tipo = 'energia'
AND direccion_instalacion_alarma IS NOT NULL
AND direccion_instalacion_alarma != ''
LIMIT 10;
