-- ================================================================
-- Script para corregir el campo 'comercial' en la tabla contratos
-- El campo 'comercial' debe contener el nombre del usuario comercial 
-- asociado (usuario_comercial_id), NO el usuario comercializadora
-- ================================================================

-- Fecha: 2026-08-13
-- Versión: 1.0

-- IMPORTANTE: Hacer backup de la tabla antes de ejecutar este script
-- Comando para backup: 
-- CREATE TABLE contratos_backup_20260813 AS SELECT * FROM contratos;

-- Actualizar el campo 'comercial' con el nombre del usuario comercial asociado
UPDATE contratos c
INNER JOIN usuarios u ON c.usuario_comercial_id = u.id
SET c.comercial = u.nombre_usuario
WHERE c.usuario_comercial_id IS NOT NULL;

-- Limpiar el campo comercial si no hay usuario comercial asociado
UPDATE contratos 
SET comercial = ''
WHERE usuario_comercial_id IS NULL;

-- Verificación: Mostrar contratos actualizados
SELECT 
    c.id,
    c.comercial AS nombre_comercial_texto,
    c.usuario_comercial_id,
    uc.nombre_usuario AS nombre_comercial_real,
    c.usuario_comercializadora_id,
    ucom.nombre_usuario AS nombre_comercializadora
FROM contratos c
LEFT JOIN usuarios uc ON c.usuario_comercial_id = uc.id
LEFT JOIN usuarios ucom ON c.usuario_comercializadora_id = ucom.id
WHERE c.id IN (578, 576, 575, 574)
ORDER BY c.id DESC;

-- Verificar que coincidan
SELECT 
    COUNT(*) as total_contratos,
    SUM(CASE WHEN c.comercial = uc.nombre_usuario THEN 1 ELSE 0 END) as contratos_correctos,
    SUM(CASE WHEN c.comercial != uc.nombre_usuario OR c.comercial IS NULL THEN 1 ELSE 0 END) as contratos_incorrectos
FROM contratos c
LEFT JOIN usuarios uc ON c.usuario_comercial_id = uc.id
WHERE c.usuario_comercial_id IS NOT NULL;
