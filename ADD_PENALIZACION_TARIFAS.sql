-- Script para agregar campos de penalización a las tablas de tarifas
-- Sistema de decomisiones por baja anticipada
-- Ejecutar en la base de datos enerfone_crm
-- Fecha: Marzo 2026

USE enerfone_crm;

-- Deshabilitar modo de actualización segura temporalmente
SET SQL_SAFE_UPDATES = 0;

-- ========================================
-- TARIFAS ENERGÍA
-- ========================================

-- Agregar campo de días de penalización
ALTER TABLE tarifasenergia 
ADD COLUMN dias_penalizacion INT NULL 
COMMENT 'Días mínimos antes de permitir baja sin penalización' 
AFTER activa;

-- Agregar campo de tipo de penalización
ALTER TABLE tarifasenergia 
ADD COLUMN tipo_penalizacion VARCHAR(20) NULL 
COMMENT 'Total o Proporcional' 
AFTER dias_penalizacion;

-- ========================================
-- TARIFAS TELEFONÍA
-- ========================================

-- Agregar campo de días de penalización
ALTER TABLE tarifastelefonia 
ADD COLUMN dias_penalizacion INT NULL 
COMMENT 'Días mínimos antes de permitir baja sin penalización' 
AFTER activa;

-- Agregar campo de tipo de penalización
ALTER TABLE tarifastelefonia 
ADD COLUMN tipo_penalizacion VARCHAR(20) NULL 
COMMENT 'Total o Proporcional' 
AFTER dias_penalizacion;

-- ========================================
-- TARIFAS ALARMAS
-- ========================================

-- Agregar campo de días de penalización
ALTER TABLE tarifas_alarmas 
ADD COLUMN dias_penalizacion INT NULL 
COMMENT 'Días mínimos antes de permitir baja sin penalización' 
AFTER activa;

-- Agregar campo de tipo de penalización
ALTER TABLE tarifas_alarmas 
ADD COLUMN tipo_penalizacion VARCHAR(20) NULL 
COMMENT 'Total o Proporcional' 
AFTER dias_penalizacion;

-- ========================================
-- VALORES POR DEFECTO SUGERIDOS
-- ========================================
-- Estos valores son ejemplos y deben configurarse según las políticas de cada comercializadora

-- Para tarifas de energía con permanencia, establecer penalización proporcional a 365 días
UPDATE tarifasenergia 
SET dias_penalizacion = 365, 
    tipo_penalizacion = 'Proporcional' 
WHERE permanencia IS NOT NULL 
  AND permanencia != '' 
  AND permanencia != 'Sin permanencia'
  AND activa = 1;

-- Para tarifas de telefonía, usar el campo permanencia como base (convertir de meses a días)
UPDATE tarifastelefonia 
SET dias_penalizacion = CASE 
        WHEN permanencia = '12 meses' THEN 365
        WHEN permanencia = '24 meses' THEN 730
        WHEN permanencia LIKE '%mes%' THEN CAST(REGEXP_REPLACE(permanencia, '[^0-9]', '') AS UNSIGNED) * 30
        ELSE 365
    END,
    tipo_penalizacion = 'Proporcional' 
WHERE permanencia IS NOT NULL 
  AND permanencia != '' 
  AND permanencia != 'Sin permanencia'
  AND activa = 1;

-- Para tarifas de alarmas, usar el campo permanencia (meses) y convertir a días
UPDATE tarifas_alarmas 
SET dias_penalizacion = permanencia * 30,
    tipo_penalizacion = 'Total'
WHERE permanencia > 0
  AND activa = 1;

-- Mostrar resumen de cambios
SELECT 'Tarifas Energía actualizadas' AS tabla, COUNT(*) AS total
FROM tarifasenergia
WHERE dias_penalizacion IS NOT NULL

UNION ALL

SELECT 'Tarifas Telefonía actualizadas' AS tabla, COUNT(*) AS total
FROM tarifastelefonia
WHERE dias_penalizacion IS NOT NULL

UNION ALL

SELECT 'Tarifas Alarmas actualizadas' AS tabla, COUNT(*) AS total
FROM tarifas_alarmas
WHERE dias_penalizacion IS NOT NULL;

-- Reactivar modo de actualización segura
SET SQL_SAFE_UPDATES = 1;
