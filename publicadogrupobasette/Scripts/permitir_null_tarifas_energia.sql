-- Script para permitir valores NULL en columnas de tarifas de energía
-- Fecha: 24 de febrero de 2026
-- IMPORTANTE: Permite valores NULL en potencia1 y energia1 (las demás ya permiten NULL)
-- Test: USE enerfonecrm; | Producción: USE enerfone_pre;

-- Modificar potencia1 y energia1 para permitir NULL (mantienen VARCHAR)
ALTER TABLE tarifasenergia
MODIFY COLUMN potencia1 VARCHAR(255) NULL,
MODIFY COLUMN energia1 VARCHAR(255) NULL;
