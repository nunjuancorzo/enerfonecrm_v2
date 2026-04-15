-- Script para añadir campos de precios detallados a la tabla tarifasenergia
-- Fecha: 7 de enero de 2026
-- IMPORTANTE: No modifica ningún campo existente, solo añade nuevos campos

-- Añadir columnas para cálculo de comparativa de tarifas
ALTER TABLE tarifasenergia
ADD COLUMN termino_fijo_diario DECIMAL(10,5) DEFAULT 0,
ADD COLUMN precio_potencia_p1 DECIMAL(10,5) DEFAULT 0,
ADD COLUMN precio_potencia_p2 DECIMAL(10,5) DEFAULT 0,
ADD COLUMN precio_potencia_p3 DECIMAL(10,5) DEFAULT 0,
ADD COLUMN precio_energia_p1 DECIMAL(10,5) DEFAULT 0,
ADD COLUMN precio_energia_p2 DECIMAL(10,5) DEFAULT 0,
ADD COLUMN precio_energia_p3 DECIMAL(10,5) DEFAULT 0;

-- Comentarios en las columnas para documentación (solo para MySQL 5.5+)
ALTER TABLE tarifasenergia 
MODIFY COLUMN termino_fijo_diario DECIMAL(10,5) DEFAULT 0 COMMENT 'Término fijo diario en €/día',
MODIFY COLUMN precio_potencia_p1 DECIMAL(10,5) DEFAULT 0 COMMENT 'Precio de potencia periodo 1 (Punta) en €/kW/día',
MODIFY COLUMN precio_potencia_p2 DECIMAL(10,5) DEFAULT 0 COMMENT 'Precio de potencia periodo 2 (Llano/Valle) en €/kW/día',
MODIFY COLUMN precio_potencia_p3 DECIMAL(10,5) DEFAULT 0 COMMENT 'Precio de potencia periodo 3 (Valle) en €/kW/día',
MODIFY COLUMN precio_energia_p1 DECIMAL(10,5) DEFAULT 0 COMMENT 'Precio de energía periodo 1 (Punta) en €/kWh',
MODIFY COLUMN precio_energia_p2 DECIMAL(10,5) DEFAULT 0 COMMENT 'Precio de energía periodo 2 (Llano) en €/kWh',
MODIFY COLUMN precio_energia_p3 DECIMAL(10,5) DEFAULT 0 COMMENT 'Precio de energía periodo 3 (Valle) en €/kWh';

-- Datos de ejemplo para algunas comercializadoras (valores orientativos)
-- Puedes ejecutar estos INSERTs o modificar tarifas existentes con UPDATE

-- Ejemplo: Actualizar una tarifa existente (ajusta el ID según tus datos)
-- UPDATE tarifasenergia 
-- SET termino_fijo_diario = 0.12,
--     precio_potencia_p1 = 0.105,
--     precio_potencia_p2 = 0.046,
--     precio_potencia_p3 = 0.0034,
--     precio_energia_p1 = 0.168,
--     precio_energia_p2 = 0.098,
--     precio_energia_p3 = 0.065
-- WHERE id = 1;
