-- Script para agregar campo de notificaciones por email a la tabla usuarios
-- Fecha: 2026-02-06
-- Descripción: Permite a los administradores activar notificaciones por email para usuarios específicos

USE enerfonecrm;

-- Agregar campo recibir_notificaciones a la tabla usuarios
ALTER TABLE usuarios 
ADD COLUMN recibir_notificaciones BOOLEAN DEFAULT FALSE COMMENT 'Indica si el usuario quiere recibir notificaciones por email cuando se modifiquen sus contratos';

-- Actualizar valor por defecto para usuarios existentes
UPDATE usuarios SET recibir_notificaciones = FALSE WHERE recibir_notificaciones IS NULL;

SELECT 'Campo recibir_notificaciones agregado exitosamente' AS Resultado;
