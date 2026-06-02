-- Agregar campo porcentaje_comision a operadoras
ALTER TABLE operadoras ADD COLUMN porcentaje_comision DECIMAL(5,2) DEFAULT 0;

-- Agregar campo porcentaje_comision a comercializadoras
ALTER TABLE comercializadoras ADD COLUMN porcentaje_comision DECIMAL(5,2) DEFAULT 0;

-- Agregar campo porcentaje_comision a empresas_alarmas
ALTER TABLE empresas_alarmas ADD COLUMN porcentaje_comision DECIMAL(5,2) DEFAULT 0;
