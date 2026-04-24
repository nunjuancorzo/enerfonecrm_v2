-- Agregar campo procedencia a la tabla clientes_simple
-- Valores posibles: 'BASE DE DATOS', 'RRSS', 'REFERIDOS'
-- Por defecto NULL para clientes existentes (se mostrará con color celeste)

ALTER TABLE clientes_simple 
ADD COLUMN procedencia VARCHAR(50) NULL 
COMMENT 'Origen del cliente: BASE DE DATOS, RRSS, REFERIDOS';
