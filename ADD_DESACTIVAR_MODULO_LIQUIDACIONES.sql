-- Agregar campo para desactivar módulo de liquidaciones
-- Este campo permite desactivar temporalmente el acceso al módulo de liquidaciones
-- Útil para mantenimiento o periodos donde no se desea que los usuarios accedan

ALTER TABLE configuracion_empresa
ADD COLUMN modulo_liquidaciones_activo BOOLEAN DEFAULT TRUE COMMENT 'Indica si el módulo de liquidaciones está activo';

-- Mensaje informativo
SELECT 'Campo modulo_liquidaciones_activo agregado correctamente a configuracion_empresa' as Resultado;
