-- Script para crear registros en detalles_comision_liquidacion para liquidaciones existentes
-- que no tienen estos registros y por tanto no muestran contratos

-- Paso 1: Crear registros para liquidaciones de colaboradores
-- (liquidaciones donde los contratos tienen HistoricoLiquidacionId apuntando a esa liquidación)
INSERT INTO detalles_comision_liquidacion (historico_liquidacion_id, contrato_id, comision_colaborador, comision_gestor, comision_jefe_ventas, comision_director_comercial)
SELECT 
    c.historico_liquidacion_id,
    c.id,
    c.comision,
    0,
    0,
    0
FROM contratos c
INNER JOIN historico_liquidaciones hl ON c.historico_liquidacion_id = hl.id
WHERE c.historico_liquidacion_id IS NOT NULL
AND NOT EXISTS (
    SELECT 1 FROM detalles_comision_liquidacion dcl 
    WHERE dcl.historico_liquidacion_id = c.historico_liquidacion_id 
    AND dcl.contrato_id = c.id
);

-- Paso 2: Para liquidaciones de roles adicionales (gestor, jefe, director) que no tienen contratos vinculados,
-- necesitamos identificarlas manualmente o ejecutar la lógica de LiquidacionService.

-- Consulta para verificar liquidaciones sin registros en detalles_comision_liquidacion:
SELECT 
    hl.id,
    hl.usuario_nombre,
    hl.estado,
    hl.cantidad_contratos,
    hl.total_comisiones,
    COUNT(dcl.id) as registros_detalle
FROM historico_liquidaciones hl
LEFT JOIN detalles_comision_liquidacion dcl ON hl.id = dcl.historico_liquidacion_id
GROUP BY hl.id, hl.usuario_nombre, hl.estado, hl.cantidad_contratos, hl.total_comisiones
HAVING COUNT(dcl.id) = 0
ORDER BY hl.fecha_aprobacion DESC;

-- NOTA: Las liquidaciones de roles adicionales (gestor, jefe, director) son más complejas
-- porque necesitan calcular las comisiones basándose en la configuración de comisiones.
-- Para esas liquidaciones, es mejor reactivarlas y volver a aprobarlas desde la aplicación.
