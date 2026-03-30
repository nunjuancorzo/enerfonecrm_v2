-- Script para agregar campos de ID de tarifas en la tabla contratos
-- Esto mejora la integridad referencial y hace más seguras las búsquedas
-- Ejecutar en la base de datos enerfone_crm
-- Fecha: Marzo 2026

USE enerfone_crm;

-- Deshabilitar modo de actualización segura temporalmente
SET SQL_SAFE_UPDATES = 0;

-- ========================================
-- AGREGAR CAMPOS DE ID DE TARIFAS
-- ========================================

-- Agregar ID de tarifa de energía
ALTER TABLE contratos 
ADD COLUMN en_tarifa_id INT NULL 
COMMENT 'ID de la tarifa de energía (FK a tarifasenergia)' 
AFTER en_Tarifa;

-- Agregar ID de tarifa de telefonía
ALTER TABLE contratos 
ADD COLUMN tarifa_tel_id INT NULL 
COMMENT 'ID de la tarifa de telefonía (FK a tarifastelefonia)' 
AFTER Tarifa_tel;

-- Agregar ID de tarifa de alarma
ALTER TABLE contratos 
ADD COLUMN kit_alarma_id INT NULL 
COMMENT 'ID de la tarifa de alarma (FK a tarifas_alarmas)' 
AFTER kit_alarma;

-- ========================================
-- POPULAR LOS IDs DESDE LOS NOMBRES EXISTENTES
-- ========================================

-- Actualizar IDs de tarifas de energía basándose en el nombre
UPDATE contratos c
INNER JOIN tarifasenergia t ON c.en_Tarifa = t.nombre
SET c.en_tarifa_id = t.id
WHERE c.tipo = 'Energia' 
  AND c.en_Tarifa IS NOT NULL
  AND c.en_Tarifa != '';

-- Actualizar IDs de tarifas de telefonía basándose en el nombre
UPDATE contratos c
INNER JOIN tarifastelefonia t ON c.Tarifa_tel = t.tarifa
SET c.tarifa_tel_id = t.id
WHERE c.tipo = 'Telefonia' 
  AND c.Tarifa_tel IS NOT NULL
  AND c.Tarifa_tel != '';

-- Actualizar IDs de tarifas de alarmas basándose en el nombre
UPDATE contratos c
INNER JOIN tarifas_alarmas t ON c.kit_alarma = t.nombre_tarifa
SET c.kit_alarma_id = t.id
WHERE c.tipo = 'Alarma' 
  AND c.kit_alarma IS NOT NULL
  AND c.kit_alarma != '';

-- ========================================
-- CREAR ÍNDICES PARA MEJORAR EL RENDIMIENTO
-- ========================================

CREATE INDEX idx_contratos_en_tarifa_id ON contratos(en_tarifa_id);
CREATE INDEX idx_contratos_tarifa_tel_id ON contratos(tarifa_tel_id);
CREATE INDEX idx_contratos_kit_alarma_id ON contratos(kit_alarma_id);

-- ========================================
-- VERIFICACIÓN
-- ========================================

SELECT 'VERIFICACIÓN DE MIGRACIÓN' AS titulo;

-- Contratos de energía con ID de tarifa
SELECT 
    'Energía' AS tipo,
    COUNT(*) AS total_contratos,
    SUM(CASE WHEN en_tarifa_id IS NOT NULL THEN 1 ELSE 0 END) AS con_id_tarifa,
    SUM(CASE WHEN en_tarifa_id IS NULL AND en_Tarifa IS NOT NULL THEN 1 ELSE 0 END) AS sin_id_pero_con_nombre
FROM contratos
WHERE tipo = 'Energia'

UNION ALL

-- Contratos de telefonía con ID de tarifa
SELECT 
    'Telefonía' AS tipo,
    COUNT(*) AS total_contratos,
    SUM(CASE WHEN tarifa_tel_id IS NOT NULL THEN 1 ELSE 0 END) AS con_id_tarifa,
    SUM(CASE WHEN tarifa_tel_id IS NULL AND Tarifa_tel IS NOT NULL THEN 1 ELSE 0 END) AS sin_id_pero_con_nombre
FROM contratos
WHERE tipo = 'Telefonia'

UNION ALL

-- Contratos de alarmas con ID de tarifa
SELECT 
    'Alarma' AS tipo,
    COUNT(*) AS total_contratos,
    SUM(CASE WHEN kit_alarma_id IS NOT NULL THEN 1 ELSE 0 END) AS con_id_tarifa,
    SUM(CASE WHEN kit_alarma_id IS NULL AND kit_alarma IS NOT NULL THEN 1 ELSE 0 END) AS sin_id_pero_con_nombre
FROM contratos
WHERE tipo = 'Alarma';

-- Listar contratos que no pudieron ser actualizados (problemas de integridad)
SELECT 
    'Contratos sin ID asignado' AS problema,
    id,
    tipo,
    CASE 
        WHEN tipo = 'Energia' THEN en_Tarifa
        WHEN tipo = 'Telefonia' THEN Tarifa_tel
        WHEN tipo = 'Alarma' THEN kit_alarma
    END AS nombre_tarifa
FROM contratos
WHERE (tipo = 'Energia' AND en_tarifa_id IS NULL AND en_Tarifa IS NOT NULL AND en_Tarifa != '')
   OR (tipo = 'Telefonia' AND tarifa_tel_id IS NULL AND Tarifa_tel IS NOT NULL AND Tarifa_tel != '')
   OR (tipo = 'Alarma' AND kit_alarma_id IS NULL AND kit_alarma IS NOT NULL AND kit_alarma != '')
ORDER BY tipo, id
LIMIT 50;

-- Reactivar modo de actualización segura
SET SQL_SAFE_UPDATES = 1;

SELECT 'Script ejecutado correctamente. Revisa los resultados de verificación arriba.' AS resultado;
