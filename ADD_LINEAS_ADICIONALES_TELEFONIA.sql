-- Script para agregar columnas de líneas adicionales 6-15 en contratos de telefonía
-- Permite hasta 15 líneas móviles adicionales en lugar de 5

USE enerfone_pre;

-- Línea 6
ALTER TABLE contratos ADD COLUMN telefono_linea6_tel VARCHAR(50) NULL;
ALTER TABLE contratos ADD COLUMN tarifa_linea6_tel VARCHAR(255) NULL;
ALTER TABLE contratos ADD COLUMN tipo_linea6_tel VARCHAR(20) NULL;
ALTER TABLE contratos ADD COLUMN codigo_icc_linea6_tel VARCHAR(19) NULL;

-- Línea 7
ALTER TABLE contratos ADD COLUMN telefono_linea7_tel VARCHAR(50) NULL;
ALTER TABLE contratos ADD COLUMN tarifa_linea7_tel VARCHAR(255) NULL;
ALTER TABLE contratos ADD COLUMN tipo_linea7_tel VARCHAR(20) NULL;
ALTER TABLE contratos ADD COLUMN codigo_icc_linea7_tel VARCHAR(19) NULL;

-- Línea 8
ALTER TABLE contratos ADD COLUMN telefono_linea8_tel VARCHAR(50) NULL;
ALTER TABLE contratos ADD COLUMN tarifa_linea8_tel VARCHAR(255) NULL;
ALTER TABLE contratos ADD COLUMN tipo_linea8_tel VARCHAR(20) NULL;
ALTER TABLE contratos ADD COLUMN codigo_icc_linea8_tel VARCHAR(19) NULL;

-- Línea 9
ALTER TABLE contratos ADD COLUMN telefono_linea9_tel VARCHAR(50) NULL;
ALTER TABLE contratos ADD COLUMN tarifa_linea9_tel VARCHAR(255) NULL;
ALTER TABLE contratos ADD COLUMN tipo_linea9_tel VARCHAR(20) NULL;
ALTER TABLE contratos ADD COLUMN codigo_icc_linea9_tel VARCHAR(19) NULL;

-- Línea 10
ALTER TABLE contratos ADD COLUMN telefono_linea10_tel VARCHAR(50) NULL;
ALTER TABLE contratos ADD COLUMN tarifa_linea10_tel VARCHAR(255) NULL;
ALTER TABLE contratos ADD COLUMN tipo_linea10_tel VARCHAR(20) NULL;
ALTER TABLE contratos ADD COLUMN codigo_icc_linea10_tel VARCHAR(19) NULL;

-- Línea 11
ALTER TABLE contratos ADD COLUMN telefono_linea11_tel VARCHAR(50) NULL;
ALTER TABLE contratos ADD COLUMN tarifa_linea11_tel VARCHAR(255) NULL;
ALTER TABLE contratos ADD COLUMN tipo_linea11_tel VARCHAR(20) NULL;
ALTER TABLE contratos ADD COLUMN codigo_icc_linea11_tel VARCHAR(19) NULL;

-- Línea 12
ALTER TABLE contratos ADD COLUMN telefono_linea12_tel VARCHAR(50) NULL;
ALTER TABLE contratos ADD COLUMN tarifa_linea12_tel VARCHAR(255) NULL;
ALTER TABLE contratos ADD COLUMN tipo_linea12_tel VARCHAR(20) NULL;
ALTER TABLE contratos ADD COLUMN codigo_icc_linea12_tel VARCHAR(19) NULL;

-- Línea 13
ALTER TABLE contratos ADD COLUMN telefono_linea13_tel VARCHAR(50) NULL;
ALTER TABLE contratos ADD COLUMN tarifa_linea13_tel VARCHAR(255) NULL;
ALTER TABLE contratos ADD COLUMN tipo_linea13_tel VARCHAR(20) NULL;
ALTER TABLE contratos ADD COLUMN codigo_icc_linea13_tel VARCHAR(19) NULL;

-- Línea 14
ALTER TABLE contratos ADD COLUMN telefono_linea14_tel VARCHAR(50) NULL;
ALTER TABLE contratos ADD COLUMN tarifa_linea14_tel VARCHAR(255) NULL;
ALTER TABLE contratos ADD COLUMN tipo_linea14_tel VARCHAR(20) NULL;
ALTER TABLE contratos ADD COLUMN codigo_icc_linea14_tel VARCHAR(19) NULL;

-- Línea 15
ALTER TABLE contratos ADD COLUMN telefono_linea15_tel VARCHAR(50) NULL;
ALTER TABLE contratos ADD COLUMN tarifa_linea15_tel VARCHAR(255) NULL;
ALTER TABLE contratos ADD COLUMN tipo_linea15_tel VARCHAR(20) NULL;
ALTER TABLE contratos ADD COLUMN codigo_icc_linea15_tel VARCHAR(19) NULL;
