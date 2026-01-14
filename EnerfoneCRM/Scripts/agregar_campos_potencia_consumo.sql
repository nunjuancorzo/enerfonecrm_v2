-- Script para agregar los campos de Potencia Contratada P1, P2 y Consumo Últimos 12 Meses
-- Ejecutar este script en la base de datos de producción

ALTER TABLE contratos
ADD COLUMN potencia_contratada_p1 DECIMAL(10,2) NULL,
ADD COLUMN potencia_contratada_p2 DECIMAL(10,2) NULL,
ADD COLUMN consumo_ultimos_12_meses DECIMAL(10,2) NULL;
