-- Script para agregar el campo 'activo' a la tabla usuarios
-- Ejecutar este script en la base de datos MySQL

ALTER TABLE usuarios 
ADD COLUMN activo BOOLEAN DEFAULT FALSE AFTER comision;

-- Actualizar usuarios existentes como activos
SET SQL_SAFE_UPDATES = 0;
UPDATE usuarios SET activo = TRUE WHERE idusuarios > 0;
SET SQL_SAFE_UPDATES = 1;
