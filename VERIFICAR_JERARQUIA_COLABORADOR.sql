-- Script para verificar la jerarquía de un colaborador específico

-- 1. Ver todos los colaboradores con sus relaciones directas
SELECT 
    u.idusuarios as ID,
    u.username as Colaborador,
    u.rol as Rol,
    u.gestor_id as GestorID,
    g.username as GestorNombre,
    u.jefe_ventas_id as JefeVentasID_Directo,
    jv_directo.username as JefeVentasNombre_Directo,
    u.director_comercial_id as DirectorID_Directo,
    d_directo.username as DirectorNombre_Directo
FROM usuarios u
LEFT JOIN usuarios g ON u.gestor_id = g.idusuarios
LEFT JOIN usuarios jv_directo ON u.jefe_ventas_id = jv_directo.idusuarios
LEFT JOIN usuarios d_directo ON u.director_comercial_id = d_directo.idusuarios
WHERE u.activo = 1 AND u.rol = 'Colaborador'
ORDER BY u.username;

-- 2. Ver la jerarquía EN CASCADA para un colaborador específico (cambiar 'NombreColaborador')
SELECT 
    'COLABORADOR' as Nivel,
    colab.idusuarios as ID,
    colab.username as Nombre,
    colab.rol as Rol,
    colab.gestor_id as ReferenciaID
FROM usuarios colab
WHERE colab.username = 'AgustinMendoza' AND colab.activo = 1

UNION ALL

SELECT 
    'GESTOR' as Nivel,
    gestor.idusuarios as ID,
    gestor.username as Nombre,
    gestor.rol as Rol,
    gestor.jefe_ventas_id as ReferenciaID
FROM usuarios colab
INNER JOIN usuarios gestor ON colab.gestor_id = gestor.idusuarios
WHERE colab.username = 'AgustinMendoza' AND colab.activo = 1 AND gestor.activo = 1

UNION ALL

SELECT 
    'JEFE DE VENTAS' as Nivel,
    jefe.idusuarios as ID,
    jefe.username as Nombre,
    jefe.rol as Rol,
    jefe.director_comercial_id as ReferenciaID
FROM usuarios colab
INNER JOIN usuarios gestor ON colab.gestor_id = gestor.idusuarios
INNER JOIN usuarios jefe ON gestor.jefe_ventas_id = jefe.idusuarios
WHERE colab.username = 'AgustinMendoza' AND colab.activo = 1 AND gestor.activo = 1 AND jefe.activo = 1

UNION ALL

SELECT 
    'DIRECTOR COMERCIAL' as Nivel,
    director.idusuarios as ID,
    director.username as Nombre,
    director.rol as Rol,
    NULL as ReferenciaID
FROM usuarios colab
INNER JOIN usuarios gestor ON colab.gestor_id = gestor.idusuarios
INNER JOIN usuarios jefe ON gestor.jefe_ventas_id = jefe.idusuarios
INNER JOIN usuarios director ON jefe.director_comercial_id = director.idusuarios
WHERE colab.username = 'AgustinMendoza' AND colab.activo = 1 AND gestor.activo = 1 AND jefe.activo = 1 AND director.activo = 1;

-- 3. Ver las liquidaciones creadas para un colaborador específico y su jerarquía
SELECT 
    hl.id as LiquidacionID,
    hl.usuario_id as UsuarioID,
    hl.usuario_nombre as Usuario,
    u.rol as Rol,
    hl.fecha_creacion as Fecha,
    hl.comision_total as Comision,
    hl.cantidad_contratos as Contratos,
    hl.estado as Estado
FROM historico_liquidacion hl
INNER JOIN usuarios u ON hl.usuario_id = u.idusuarios
WHERE hl.fecha_creacion >= DATE_SUB(NOW(), INTERVAL 1 DAY)
ORDER BY hl.fecha_creacion DESC, u.rol;

-- 4. Ver la configuración de comisiones para cada usuario
SELECT 
    cc.id as ConfigID,
    cc.usuario_id as UsuarioID,
    u.username as Usuario,
    u.rol as Rol,
    cc.tipo_proveedor as TipoProveedor,
    cc.proveedor_id as ProveedorID,
    cc.nombre_proveedor as NombreProveedor,
    cc.porcentaje_colaborador as PctColaborador,
    cc.porcentaje_gestor as PctGestor,
    cc.porcentaje_jefe_ventas as PctJefeVentas,
    cc.porcentaje_director_comercial as PctDirector,
    cc.activa as Activa
FROM configuracion_comision cc
INNER JOIN usuarios u ON cc.usuario_id = u.idusuarios
WHERE u.activo = 1
ORDER BY u.rol, u.username;
