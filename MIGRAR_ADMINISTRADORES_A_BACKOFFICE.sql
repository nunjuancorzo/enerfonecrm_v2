-- Script para migrar administradores extra a rol Backoffice
-- Solo puede haber UN administrador: el usuario "administrador"
-- Todos los demás administradores se convierten en Backoffice
-- Ejecutar en la base de datos enerfone_crm
-- Fecha: Marzo 2026

USE enerfone_crm;

-- Deshabilitar modo de actualización segura temporalmente
SET SQL_SAFE_UPDATES = 0;

-- ========================================
-- IDENTIFICAR ADMINISTRADORES A MIGRAR
-- ========================================

SELECT 'ADMINISTRADORES ACTUALES EN LA BASE DE DATOS' AS titulo;

SELECT 
    idusuarios AS id,
    username AS nombre_usuario,
    nombre,
    apellidos,
    email,
    rol,
    activo
FROM usuarios
WHERE rol = 'Administrador'
ORDER BY username;

-- ========================================
-- MIGRACIÓN A BACKOFFICE
-- ========================================

-- Cambiar todos los administradores EXCEPTO el usuario "administrador" a rol Backoffice
UPDATE usuarios
SET rol = 'Backoffice'
WHERE rol = 'Administrador'
  AND username != 'administrador';

-- Verificar el resultado
SELECT 'RESULTADO DE LA MIGRACIÓN' AS titulo;

SELECT 
    COUNT(*) AS total_administradores
FROM usuarios
WHERE rol = 'Administrador';

SELECT 
    COUNT(*) AS total_backoffice
FROM usuarios
WHERE rol = 'Backoffice';

-- Listar el único administrador que debe quedar
SELECT 
    idusuarios AS id,
    username AS nombre_usuario,
    nombre,
    apellidos,
    email,
    rol
FROM usuarios
WHERE rol = 'Administrador';

-- Listar los usuarios migrados a Backoffice
SELECT 
    idusuarios AS id,
    username AS nombre_usuario,
    nombre,
    apellidos,
    email,
    rol
FROM usuarios
WHERE rol = 'Backoffice';

-- Reactivar modo de actualización segura
SET SQL_SAFE_UPDATES = 1;

-- ========================================
-- INFORMACIÓN IMPORTANTE
-- ========================================
/*
POLÍTICA DE ADMINISTRADOR ÚNICO:
- Solo puede haber UN administrador por base de datos
- El administrador único debe ser el usuario con username = "administrador"
- Todos los demás usuarios con rol "Administrador" se convierten en "Backoffice"
- Los usuarios Backoffice mantienen todos los permisos activados por defecto
- El Administrador puede configurar los permisos de cada usuario Backoffice

IMPORTANTE:
- Este script NO afecta a otros roles (Colaborador, Gestor, etc.)
- Los usuarios migrados a Backoffice mantienen todos sus datos
- Los permisos de Backoffice se pueden configurar desde la ficha del usuario
- Si necesitas que un usuario Backoffice tenga acceso a todo, simplemente deja todos los permisos activados
*/

SELECT '✓ Migración completada. Solo debe haber 1 administrador: "administrador"' AS resultado;
