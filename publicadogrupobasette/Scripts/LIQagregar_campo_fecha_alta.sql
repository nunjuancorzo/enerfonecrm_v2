-- Script para agregar el campo fecha_alta a la tabla contratos
-- Fecha: 28 de enero de 2026
-- Descripción: Añade el campo fecha_alta que por defecto será igual a fecha_creacion

USE crm_enerfone;

-- Agregar la columna fecha_alta
ALTER TABLE contratos 
ADD COLUMN fecha_alta DATETIME NULL AFTER fecha_activo;

-- Desactivar el modo de actualización segura temporalmente
SET SQL_SAFE_UPDATES = 0;

-- Inicializar con la fecha de creación para contratos existentes
UPDATE contratos 
SET fecha_alta = fecha_creacion 
WHERE fecha_alta IS NULL;

-- Reactivar el modo de actualización segura
SET SQL_SAFE_UPDATES = 1;

SELECT 'Campo fecha_alta agregado correctamente a la tabla contratos' AS Resultado;
