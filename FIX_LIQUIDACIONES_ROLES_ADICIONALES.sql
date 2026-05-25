-- Script para corregir los DetallesComisionLiquidacion de liquidaciones existentes
-- Asigna los IDs de usuario (GestorId, JefeVentasId, DirectorComercialId) basándose en el UsuarioId del HistoricoLiquidacion

-- Actualizar GestorId para liquidaciones de Gestor
UPDATE detalles_comision_liquidacion dcl
INNER JOIN historico_liquidaciones hl ON dcl.historico_liquidacion_id = hl.id
SET dcl.gestor_id = hl.usuario_id
WHERE hl.rol_nombre = 'Gestor' 
  AND dcl.comision_gestor > 0
  AND dcl.gestor_id IS NULL;

-- Actualizar JefeVentasId para liquidaciones de Jefe de Ventas
UPDATE detalles_comision_liquidacion dcl
INNER JOIN historico_liquidaciones hl ON dcl.historico_liquidacion_id = hl.id
SET dcl.jefe_ventas_id = hl.usuario_id
WHERE hl.rol_nombre = 'Jefe de Ventas' 
  AND dcl.comision_jefe_ventas > 0
  AND dcl.jefe_ventas_id IS NULL;

-- Actualizar DirectorComercialId para liquidaciones de Director Comercial
UPDATE detalles_comision_liquidacion dcl
INNER JOIN historico_liquidaciones hl ON dcl.historico_liquidacion_id = hl.id
SET dcl.director_comercial_id = hl.usuario_id
WHERE hl.rol_nombre = 'Director Comercial' 
  AND dcl.comision_director_comercial > 0
  AND dcl.director_comercial_id IS NULL;

-- Mostrar resultados
SELECT 
    'Gestores corregidos' as Tipo,
    COUNT(*) as Cantidad
FROM detalles_comision_liquidacion dcl
INNER JOIN historico_liquidaciones hl ON dcl.historico_liquidacion_id = hl.id
WHERE hl.rol_nombre = 'Gestor' 
  AND dcl.comision_gestor > 0
  AND dcl.gestor_id IS NOT NULL

UNION ALL

SELECT 
    'Jefes de Ventas corregidos' as Tipo,
    COUNT(*) as Cantidad
FROM detalles_comision_liquidacion dcl
INNER JOIN historico_liquidaciones hl ON dcl.historico_liquidacion_id = hl.id
WHERE hl.rol_nombre = 'Jefe de Ventas' 
  AND dcl.comision_jefe_ventas > 0
  AND dcl.jefe_ventas_id IS NOT NULL

UNION ALL

SELECT 
    'Directores Comerciales corregidos' as Tipo,
    COUNT(*) as Cantidad
FROM detalles_comision_liquidacion dcl
INNER JOIN historico_liquidaciones hl ON dcl.historico_liquidacion_id = hl.id
WHERE hl.rol_nombre = 'Director Comercial' 
  AND dcl.comision_director_comercial > 0
  AND dcl.director_comercial_id IS NOT NULL;
