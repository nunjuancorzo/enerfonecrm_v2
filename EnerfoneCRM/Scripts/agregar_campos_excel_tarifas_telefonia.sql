-- Script para añadir campos del Excel de tarifas de telefonía
-- Fecha: 24 de febrero de 2026
-- IMPORTANTE: Añade campos para soportar importación masiva desde Excel
-- Test: USE enerfonecrm; | Producción: USE enerfone_pre;

-- Renombrar TV a tv1 para tener dos campos de TV
ALTER TABLE tarifastelefonia
CHANGE COLUMN TV tv1 VARCHAR(255);

-- Añadir nuevas columnas a la tabla tarifastelefonia
ALTER TABLE tarifastelefonia
ADD COLUMN tarifa VARCHAR(255),
ADD COLUMN movil2 VARCHAR(255),
ADD COLUMN tv2 VARCHAR(255),
ADD COLUMN permanencia VARCHAR(100),
ADD COLUMN fecha_carga DATETIME;
