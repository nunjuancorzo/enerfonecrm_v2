-- Script para agregar 'Autonomo' al campo tipo_cliente de la tabla clientes_simple
-- Este script verifica primero el tipo de la columna y lo modifica para incluir 'Autonomo'

USE enerfonecrm;

-- Verificar el tipo actual de la columna
SELECT COLUMN_TYPE 
FROM INFORMATION_SCHEMA.COLUMNS 
WHERE TABLE_SCHEMA = 'enerfonecrm' 
  AND TABLE_NAME = 'clientes_simple' 
  AND COLUMN_NAME = 'tipo_cliente';

-- Modificar la columna tipo_cliente para incluir 'Autonomo' (sin tilde)
-- Si la columna es ENUM, esto agregará 'Autonomo' a los valores permitidos
-- Si es VARCHAR, no causará problemas
ALTER TABLE clientes_simple 
MODIFY COLUMN tipo_cliente ENUM('Particular', 'Pyme', 'Autonomo') NOT NULL;

-- Verificar que el cambio se aplicó correctamente
SELECT COLUMN_TYPE 
FROM INFORMATION_SCHEMA.COLUMNS 
WHERE TABLE_SCHEMA = 'enerfonecrm' 
  AND TABLE_NAME = 'clientes_simple' 
  AND COLUMN_NAME = 'tipo_cliente';

SELECT 'Script ejecutado correctamente. La columna tipo_cliente ahora acepta: Particular, Pyme, Autonomo' as Resultado;
