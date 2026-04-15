-- Script para agregar campos detallados de dirección de instalación en contratos de alarma
-- Fecha: 2025-12-29

-- Agregar campos de dirección detallada para instalación de alarmas
ALTER TABLE contratos ADD COLUMN numero_instalacion VARCHAR(20);
ALTER TABLE contratos ADD COLUMN escalera_instalacion VARCHAR(10);
ALTER TABLE contratos ADD COLUMN piso_instalacion VARCHAR(10);
ALTER TABLE contratos ADD COLUMN puerta_instalacion VARCHAR(10);
ALTER TABLE contratos ADD COLUMN codigo_postal_instalacion VARCHAR(10);
ALTER TABLE contratos ADD COLUMN provincia_instalacion VARCHAR(100);
ALTER TABLE contratos ADD COLUMN localidad_instalacion VARCHAR(100);
ALTER TABLE contratos ADD COLUMN aclarador_instalacion VARCHAR(500);

-- Nota: El campo 'direccion_instalacion_alarma' ya existe y se usará para el nombre de la calle
