-- Script para agregar el campo 'fecha_creacion' a la tabla usuarios
-- Ejecutar este script en la base de datos MySQL

ALTER TABLE usuarios 
ADD COLUMN fecha_creacion DATETIME DEFAULT CURRENT_TIMESTAMP AFTER activo;

-- Actualizar usuarios existentes con la fecha actual
UPDATE usuarios SET fecha_creacion = CURRENT_TIMESTAMP WHERE idusuarios > 0;
