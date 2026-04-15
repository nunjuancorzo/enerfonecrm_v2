-- Script para añadir campos de titular IBAN diferente a la tabla contratos
-- Fecha: 7 de enero de 2026

ALTER TABLE contratos
ADD COLUMN titular_iban_diferente BOOLEAN DEFAULT FALSE,
ADD COLUMN titular_iban_dni VARCHAR(50),
ADD COLUMN titular_iban_nombre VARCHAR(255),
ADD COLUMN titular_iban_numero VARCHAR(100);
