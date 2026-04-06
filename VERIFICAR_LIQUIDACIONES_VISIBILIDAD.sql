-- Script para verificar la visibilidad de liquidaciones
-- 
-- REGLAS DE VISIBILIDAD:
-- - Administrador y Backoffice: Ven TODAS las liquidaciones de todos los usuarios
-- - Colaboradores y Gestores: Solo ven SUS PROPIAS liquidaciones
-- 
-- Ejecutar este script para diagnosticar problemas de visibilidad

-- 1. Ver todas las liquidaciones existentes
SELECT 
    id,
    usuario_id,
    usuario_nombre,
    estado,
    fecha_aprobacion,
    fecha_aceptada,
    fecha_liquidada,
    fecha_en_incidencia,
    cantidad_contratos
FROM historico_liquidaciones
ORDER BY fecha_aprobacion DESC;

-- 2. Contar liquidaciones por estado
SELECT 
    estado,
    COUNT(*) as cantidad
FROM historico_liquidaciones
GROUP BY estado;

-- 3. Ver liquidaciones por usuario
SELECT 
    usuario_id,
    usuario_nombre,
    estado,
    COUNT(*) as cantidad_liquidaciones
FROM historico_liquidaciones
GROUP BY usuario_id, usuario_nombre, estado
ORDER BY usuario_id, estado;

-- 4. Verificar usuarios colaboradores activos
SELECT 
    id,
    nombre_usuario,
    rol,
    activo,
    puede_ver_liquidaciones
FROM usuarios
WHERE rol = 'Colaborador' AND activo = 1;

-- 5. Ver liquidaciones con detalles completos
SELECT 
    h.id,
    h.usuario_id,
    h.usuario_nombre,
    h.estado,
    h.fecha_aprobacion,
    h.cantidad_contratos,
    h.total_comisiones,
    h.aprobado_por_nombre,
    u.rol as rol_usuario
FROM historico_liquidaciones h
LEFT JOIN usuarios u ON h.usuario_id = u.id
ORDER BY h.fecha_aprobacion DESC;

-- 6. Simular visibilidad para un colaborador específico (cambiar el ID según sea necesario)
-- Por ejemplo, para el usuario con ID = 5
SELECT 
    h.id,
    h.usuario_id,
    h.usuario_nombre,
    h.estado,
    h.fecha_aprobacion,
    h.cantidad_contratos,
    'VISIBLE PARA COLABORADOR ID 5' as visibilidad
FROM historico_liquidaciones h
WHERE h.usuario_id = 5  -- Cambiar este ID por el del colaborador que quieres verificar
ORDER BY h.fecha_aprobacion DESC;

-- 7. Verificar roles y permisos de usuarios
SELECT 
    id,
    nombre_usuario,
    rol,
    activo,
    puede_ver_liquidaciones,
    CASE 
        WHEN rol IN ('Administrador', 'Backoffice') THEN 'VE TODAS LAS LIQUIDACIONES'
        WHEN rol IN ('Colaborador', 'Gestor') THEN 'SOLO VE SUS PROPIAS LIQUIDACIONES'
        ELSE 'SIN ACCESO'
    END as nivel_visibilidad
FROM usuarios
WHERE activo = 1
ORDER BY rol, nombre_usuario;
