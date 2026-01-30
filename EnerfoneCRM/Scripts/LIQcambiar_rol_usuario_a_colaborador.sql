-- Script para cambiar el rol "Usuario" a "Colaborador" en todos los usuarios existentes
-- Fecha: 28 de enero de 2026

-- Desactivar temporalmente el modo safe update
SET SQL_SAFE_UPDATES = 0;

-- Actualizar todos los usuarios con rol "Usuario" a "Colaborador"
UPDATE usuarios
SET rol = 'Colaborador'
WHERE rol = 'Usuario';

-- Reactivar el modo safe update
SET SQL_SAFE_UPDATES = 1;

-- Verificar los cambios
SELECT id, nombre_usuario, rol, activo
FROM usuarios
WHERE rol = 'Colaborador'
ORDER BY id;

-- Mostrar resumen de roles
SELECT rol, COUNT(*) as cantidad
FROM usuarios
GROUP BY rol
ORDER BY rol;
