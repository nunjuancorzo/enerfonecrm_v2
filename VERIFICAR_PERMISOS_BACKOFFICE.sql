-- Script para verificar y configurar permisos de usuario Backoffice
-- Ejecutar estas consultas para diagnosticar y corregir problemas de permisos

-- 1. Ver todos los usuarios Backoffice y sus permisos
SELECT 
    id,
    nombre_usuario,
    nombre,
    apellidos,
    rol,
    activo,
    puede_ver_clientes,
    puede_ver_contratos,
    puede_ver_tarifas,
    puede_ver_liquidaciones,
    puede_ver_sips,
    puede_ver_incidencias,
    puede_ver_ofertas,
    puede_ver_usuarios
FROM usuarios
WHERE rol = 'Backoffice'
ORDER BY id;

-- 2. Dar todos los permisos a un usuario Backoffice específico
-- IMPORTANTE: Cambia el ID 'XXX' por el ID del usuario que necesitas configurar
-- NOTA: puede_ver_usuarios NO se incluye - la gestión de usuarios es exclusiva de Administrador
/*
UPDATE usuarios SET
    puede_ver_clientes = 1,
    puede_ver_contratos = 1,
    puede_ver_tarifas = 1,
    puede_ver_liquidaciones = 1,
    puede_ver_sips = 1,
    puede_ver_incidencias = 1,
    puede_ver_ofertas = 1,
    puede_ver_usuarios = 0  -- SIEMPRE debe ser 0 para Backoffice (seguridad)
WHERE id = XXX AND rol = 'Backoffice';
*/

-- 3. Verificar que el usuario tiene los permisos correctos después de la actualización
/*
SELECT 
    id,
    nombre_usuario,
    rol,
    puede_ver_clientes AS 'Clientes',
    puede_ver_contratos AS 'Contratos',
    puede_ver_tarifas AS 'Tarifas',
    puede_ver_liquidaciones AS 'Liquidaciones',
    puede_ver_sips AS 'SIPS',
    puede_ver_incidencias AS 'Incidencias',
    puede_ver_ofertas AS 'Ofertas',
    puede_ver_usuarios AS 'Usuarios'
FROM usuarios
WHERE id = XXX;
*/

-- 4. Ver qué puede hacer cada rol
SELECT 
    rol,
    COUNT(*) as cantidad,
    CASE 
        WHEN rol = 'Administrador' THEN 'Puede hacer TODO sin restricciones'
        WHEN rol = 'Backoffice' THEN 'Puede hacer TODO en los módulos que tenga permisos (granular) - EXCEPTO gestionar usuarios'
        WHEN rol = 'Gestor' THEN 'Solo ve sus propios datos'
        WHEN rol = 'Colaborador' THEN 'Solo ve sus propios datos'
        ELSE 'Otros permisos limitados'
    END as descripcion_permisos
FROM usuarios
WHERE activo = 1
GROUP BY rol;
