-- Script para añadir campos del Excel de tarifas de energía
-- Fecha: 24 de febrero de 2026
-- IMPORTANTE: Añade campos para soportar importación masiva desde Excel
-- Test: USE enerfonecrm; | Producción: USE enerfone_pre;

-- Añadir nuevas columnas a la tabla tarifasenergia
ALTER TABLE tarifasenergia
ADD COLUMN tipo_cliente VARCHAR(50),
ADD COLUMN peaje VARCHAR(50),
ADD COLUMN termino_fijo_gas VARCHAR(255),
ADD COLUMN pvd_sva VARCHAR(255),
ADD COLUMN termino_variable_gas VARCHAR(255),
ADD COLUMN descuento VARCHAR(255),
ADD COLUMN observaciones_descuentos VARCHAR(500),
ADD COLUMN permanencia VARCHAR(100),
ADD COLUMN excedentes VARCHAR(100),
ADD COLUMN bateria_virtual VARCHAR(50),
ADD COLUMN fecha_carga DATETIME;
