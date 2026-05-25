-- Verificar jerarquía de AgustinMendoza
SELECT 
    u.idusuarios,
    u.username,
    u.rol,
    u.gestor_id,
    u.jefe_ventas_id,
    u.director_comercial_id,
    u.activo,
    g.username AS gestor_nombre,
    g.rol AS gestor_rol,
    g.activo AS gestor_activo,
    jv.username AS jefe_nombre,
    jv.rol AS jefe_rol,
    jv.activo AS jefe_activo,
    dc.username AS director_nombre,
    dc.rol AS director_rol,
    dc.activo AS director_activo
FROM Usuarios u
LEFT JOIN Usuarios g ON u.gestor_id = g.idusuarios
LEFT JOIN Usuarios jv ON g.jefe_ventas_id = jv.idusuarios
LEFT JOIN Usuarios dc ON jv.director_comercial_id = dc.idusuarios
WHERE u.username = 'AgustinMendoza';

-- Ver contratos vinculados a la liquidación aceptada (ID 41 basado en la imagen)
SELECT 
    c.id,
    c.tipo,
    c.estado,
    c.comercial,
    c.comision,
    c.historico_liquidacion_id,
    c.en_tarifa_id,
    c.tarifa_tel_id
FROM contratos c
WHERE c.historico_liquidacion_id = (
    SELECT id FROM historico_liquidaciones 
    WHERE usuario_nombre = 'AgustinMendoza' 
    AND estado = 'Aceptada' 
    ORDER BY id DESC 
    LIMIT 1
);

-- Ver última liquidación aceptada de AgustinMendoza
SELECT * FROM historico_liquidaciones 
WHERE usuario_nombre = 'AgustinMendoza' 
AND estado = 'Aceptada' 
ORDER BY id DESC 
LIMIT 1;

-- Ver si hay configuración de comisiones activa para AgustinMendoza
SELECT 
    cc.*,
    u.username
FROM configuracion_comisiones cc
INNER JOIN usuarios u ON cc.usuario_id = u.idusuarios
WHERE u.username = 'AgustinMendoza'
AND cc.activa = 1;
