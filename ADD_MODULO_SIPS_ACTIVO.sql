-- Agregar campo para activar/desactivar módulo SIPS
-- Solo SuperAdmin puede modificar esta configuración
-- Útil para controlar el acceso a consultas SIPS y su cuota mensual

ALTER TABLE configuracion_empresa
ADD COLUMN modulo_sips_activo BOOLEAN DEFAULT TRUE COMMENT 'Indica si el módulo SIPS está activo (solo SuperAdmin puede modificar)';

-- Mensaje informativo
SELECT 'Campo modulo_sips_activo agregado correctamente a configuracion_empresa' as Resultado;
