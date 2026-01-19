-- Script para agregar el campo excluir_log_acceso en la tabla usuarios
-- Este campo permite marcar usuarios que no deben aparecer en el log de accesos

-- Desactivar temporalmente el modo seguro
SET SQL_SAFE_UPDATES = 0;

ALTER TABLE usuarios 
ADD COLUMN excluir_log_acceso TINYINT(1) DEFAULT 0 AFTER activo;

-- Actualizar el campo para que tenga valor por defecto 0 (false) en registros existentes
UPDATE usuarios SET excluir_log_acceso = 0 WHERE excluir_log_acceso IS NULL;

-- Reactivar el modo seguro
SET SQL_SAFE_UPDATES = 1;
