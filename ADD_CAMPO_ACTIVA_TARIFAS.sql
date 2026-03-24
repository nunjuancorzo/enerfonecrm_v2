-- Agregar campo 'activa' a las tablas de tarifas de energía, telefonía y alarmas
-- Fecha: 18 de marzo de 2026
-- Nota: Si alguna columna ya existe, el comando dará error. Ejecutar solo las líneas necesarias.

-- Agregar columna 'activa' a tarifasenergia
ALTER TABLE tarifasenergia 
ADD COLUMN activa BOOLEAN NOT NULL DEFAULT TRUE;

-- Agregar columna 'activa' a tarifastelefonia
ALTER TABLE tarifastelefonia 
ADD COLUMN activa BOOLEAN NOT NULL DEFAULT TRUE;


-- Verificar las columnas agregadas
SHOW COLUMNS FROM tarifasenergia LIKE 'activa';
SHOW COLUMNS FROM tarifastelefonia LIKE 'activa';
SHOW COLUMNS FROM tarifas_alarmas LIKE 'activa';
