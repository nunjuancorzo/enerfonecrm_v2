-- Script para añadir el campo director_comercial_id a la tabla usuarios
-- Este campo permite asignar un Director Comercial a un Jefe de Ventas

USE enerfonecrm;

-- Añadir columna director_comercial_id después de jefe_ventas_id
ALTER TABLE usuarios 
ADD COLUMN director_comercial_id INT NULL AFTER jefe_ventas_id;

-- Añadir índice para mejorar el rendimiento de las consultas
ALTER TABLE usuarios
ADD INDEX idx_director_comercial (director_comercial_id);

-- Opcional: Añadir comentario a la columna
ALTER TABLE usuarios 
MODIFY COLUMN director_comercial_id INT NULL COMMENT 'ID del Director Comercial al que está asignado este usuario (solo para Jefes de Ventas y usuarios sin Jefe de Ventas)';
