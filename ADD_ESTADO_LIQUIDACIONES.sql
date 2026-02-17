-- Script para añadir campos de estado y fechas a historico_liquidaciones
-- Ejecutar en la base de datos enerfone_crm

USE enerfone_crm;

-- Añadir columna de estado
ALTER TABLE historico_liquidaciones 
ADD COLUMN estado VARCHAR(50) DEFAULT 'Aceptada' AFTER observaciones;

-- Añadir columnas de fechas para cada estado
ALTER TABLE historico_liquidaciones 
ADD COLUMN fecha_en_incidencia DATETIME NULL AFTER estado;

ALTER TABLE historico_liquidaciones 
ADD COLUMN fecha_aceptada DATETIME NULL AFTER fecha_en_incidencia;

ALTER TABLE historico_liquidaciones 
ADD COLUMN fecha_liquidada DATETIME NULL AFTER fecha_aceptada;

-- Añadir columna de total_comisiones
ALTER TABLE historico_liquidaciones 
ADD COLUMN total_comisiones DECIMAL(10,2) NULL AFTER fecha_liquidada;

-- Deshabilitar safe update mode temporalmente
SET SQL_SAFE_UPDATES = 0;

-- Actualizar registros existentes: establecer fecha_aceptada como fecha_aprobacion
-- Ya que los registros históricos ya están aprobados/aceptados
UPDATE historico_liquidaciones 
SET fecha_aceptada = fecha_aprobacion
WHERE fecha_aceptada IS NULL;

-- Volver a habilitar safe update mode
SET SQL_SAFE_UPDATES = 1;

-- Opcional: Actualizar estado de registros muy antiguos a 'Liquidada' si se quiere
-- UPDATE historico_liquidaciones 
-- SET estado = 'Liquidada', fecha_liquidada = fecha_aprobacion
-- WHERE fecha_aprobacion < DATE_SUB(NOW(), INTERVAL 30 DAY);

SELECT 'Script ejecutado correctamente' AS mensaje;
