-- Script para agregar el campo 'servicio_id' a la tabla contratos
-- Ejecutar este script en la base de datos MySQL

ALTER TABLE contratos 
ADD COLUMN servicio_id INT NULL AFTER usuario_comercializadora_id,
ADD FOREIGN KEY (servicio_id) REFERENCES servicios(id) ON DELETE SET NULL;
