-- Script de diagnóstico para el sistema de liquidaciones

-- 1. Verificar jerarquía del colaborador
SELECT 
    u.id,
    u.nombre_usuario,
    u.rol,
    u.gestor_id,
    u.jefe_ventas_id,
    u.director_comercial_id,
    g.nombre_usuario as gestor_nombre,
    j.nombre_usuario as jefe_nombre,
    d.nombre_usuario as director_nombre
FROM usuarios u
LEFT JOIN usuarios g ON u.gestor_id = g.id
LEFT JOIN usuarios j ON u.jefe_ventas_id = j.id
LEFT JOIN usuarios d ON u.director_comercial_id = d.id
WHERE u.rol = 'Colaborador' AND u.activo = 1
ORDER BY u.nombre_usuario;

-- 2. Verificar configuración de comisiones del colaborador
SELECT 
    cc.id,
    cc.usuario_id,
    u.nombre_usuario,
    cc.tipo_proveedor,
    cc.nombre_proveedor,
    cc.porcentaje_colaborador,
    cc.porcentaje_gestor,
    cc.porcentaje_jefe_ventas,
    cc.porcentaje_director_comercial,
    (100 - cc.porcentaje_colaborador - IFNULL(cc.porcentaje_gestor, 0) - IFNULL(cc.porcentaje_jefe_ventas, 0) - IFNULL(cc.porcentaje_director_comercial, 0)) as porcentaje_administrador
FROM configuracion_comisiones cc
INNER JOIN usuarios u ON cc.usuario_id = u.id
WHERE cc.activa = 1
ORDER BY u.nombre_usuario, cc.tipo_proveedor;

-- 3. Ver contratos facturables recientes
SELECT 
    c.id,
    c.tipo,
    c.comercial,
    c.estado,
    c.comision,
    c.fecha_creacion
FROM contratos c
WHERE c.estado = 'Act/Facturable'
ORDER BY c.fecha_creacion DESC
LIMIT 20;

-- 4. Ver liquidaciones históricas creadas
SELECT 
    hl.id,
    hl.usuario_id,
    hl.usuario_nombre,
    u.rol,
    hl.cantidad_contratos,
    hl.total_comisiones,
    hl.fecha_aprobacion,
    hl.estado
FROM historico_liquidaciones hl
INNER JOIN usuarios u ON hl.usuario_id = u.id
ORDER BY hl.fecha_aprobacion DESC
LIMIT 20;

-- 5. Ver detalles de comisión de las últimas liquidaciones
SELECT 
    dcl.id,
    dcl.historico_liquidacion_id,
    dcl.contrato_id,
    dcl.comision_base,
    dcl.colaborador_id,
    dcl.comision_colaborador,
    dcl.porcentaje_colaborador,
    dcl.gestor_id,
    dcl.comision_gestor,
    dcl.porcentaje_gestor,
    dcl.jefe_ventas_id,
    dcl.comision_jefe_ventas,
    dcl.porcentaje_jefe_ventas,
    dcl.director_comercial_id,
    dcl.comision_director_comercial,
    dcl.porcentaje_director_comercial,
    dcl.administrador_id,
    dcl.comision_administrador,
    dcl.porcentaje_administrador
FROM detalle_comision_liquidacion dcl
ORDER BY dcl.id DESC
LIMIT 20;
