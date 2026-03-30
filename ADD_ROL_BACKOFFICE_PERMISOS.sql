-- Script para agregar rol Backoffice y permisos granulares por entidad
-- Sistema de permisos para usuarios Backoffice
-- Ejecutar en la base de datos enerfone_crm
-- Fecha: Marzo 2026

USE enerfone_crm;

-- Deshabilitar modo de actualización segura temporalmente
SET SQL_SAFE_UPDATES = 0;

-- ========================================
-- VERIFICAR Y AGREGAR COLUMNAS DE PERMISOS
-- ========================================

-- Verificar si la columna puede_ver_clientes existe
SET @col_exists_clientes = (
    SELECT COUNT(*) 
    FROM information_schema.COLUMNS 
    WHERE TABLE_SCHEMA = DATABASE() 
    AND TABLE_NAME = 'usuarios' 
    AND COLUMN_NAME = 'puede_ver_clientes'
);

-- Agregar columna puede_ver_clientes si no existe
SET @sql_clientes = IF(@col_exists_clientes = 0,
    'ALTER TABLE usuarios ADD COLUMN puede_ver_clientes BOOLEAN NOT NULL DEFAULT 1 COMMENT "Permiso para ver módulo de clientes (aplica solo a rol Backoffice)" AFTER recibir_notificaciones',
    'SELECT "La columna puede_ver_clientes ya existe en usuarios" AS mensaje'
);
PREPARE stmt FROM @sql_clientes;
EXECUTE stmt;
DEALLOCATE PREPARE stmt;

-- ========================================

-- Verificar si la columna puede_ver_contratos existe
SET @col_exists_contratos = (
    SELECT COUNT(*) 
    FROM information_schema.COLUMNS 
    WHERE TABLE_SCHEMA = DATABASE() 
    AND TABLE_NAME = 'usuarios' 
    AND COLUMN_NAME = 'puede_ver_contratos'
);

-- Agregar columna puede_ver_contratos si no existe
SET @sql_contratos = IF(@col_exists_contratos = 0,
    'ALTER TABLE usuarios ADD COLUMN puede_ver_contratos BOOLEAN NOT NULL DEFAULT 1 COMMENT "Permiso para ver módulo de contratos (aplica solo a rol Backoffice)" AFTER puede_ver_clientes',
    'SELECT "La columna puede_ver_contratos ya existe en usuarios" AS mensaje'
);
PREPARE stmt FROM @sql_contratos;
EXECUTE stmt;
DEALLOCATE PREPARE stmt;

-- ========================================

-- Verificar si la columna puede_ver_tarifas existe
SET @col_exists_tarifas = (
    SELECT COUNT(*) 
    FROM information_schema.COLUMNS 
    WHERE TABLE_SCHEMA = DATABASE() 
    AND TABLE_NAME = 'usuarios' 
    AND COLUMN_NAME = 'puede_ver_tarifas'
);

-- Agregar columna puede_ver_tarifas si no existe
SET @sql_tarifas = IF(@col_exists_tarifas = 0,
    'ALTER TABLE usuarios ADD COLUMN puede_ver_tarifas BOOLEAN NOT NULL DEFAULT 1 COMMENT "Permiso para ver módulo de tarifas (aplica solo a rol Backoffice)" AFTER puede_ver_contratos',
    'SELECT "La columna puede_ver_tarifas ya existe en usuarios" AS mensaje'
);
PREPARE stmt FROM @sql_tarifas;
EXECUTE stmt;
DEALLOCATE PREPARE stmt;

-- ========================================

-- Verificar si la columna puede_ver_liquidaciones existe
SET @col_exists_liquidaciones = (
    SELECT COUNT(*) 
    FROM information_schema.COLUMNS 
    WHERE TABLE_SCHEMA = DATABASE() 
    AND TABLE_NAME = 'usuarios' 
    AND COLUMN_NAME = 'puede_ver_liquidaciones'
);

-- Agregar columna puede_ver_liquidaciones si no existe
SET @sql_liquidaciones = IF(@col_exists_liquidaciones = 0,
    'ALTER TABLE usuarios ADD COLUMN puede_ver_liquidaciones BOOLEAN NOT NULL DEFAULT 1 COMMENT "Permiso para ver módulo de liquidaciones (aplica solo a rol Backoffice)" AFTER puede_ver_tarifas',
    'SELECT "La columna puede_ver_liquidaciones ya existe en usuarios" AS mensaje'
);
PREPARE stmt FROM @sql_liquidaciones;
EXECUTE stmt;
DEALLOCATE PREPARE stmt;

-- ========================================

-- Verificar si la columna puede_ver_sips existe
SET @col_exists_sips = (
    SELECT COUNT(*) 
    FROM information_schema.COLUMNS 
    WHERE TABLE_SCHEMA = DATABASE() 
    AND TABLE_NAME = 'usuarios' 
    AND COLUMN_NAME = 'puede_ver_sips'
);

-- Agregar columna puede_ver_sips si no existe
SET @sql_sips = IF(@col_exists_sips = 0,
    'ALTER TABLE usuarios ADD COLUMN puede_ver_sips BOOLEAN NOT NULL DEFAULT 1 COMMENT "Permiso para ver módulo SIPS (aplica solo a rol Backoffice)" AFTER puede_ver_liquidaciones',
    'SELECT "La columna puede_ver_sips ya existe en usuarios" AS mensaje'
);
PREPARE stmt FROM @sql_sips;
EXECUTE stmt;
DEALLOCATE PREPARE stmt;

-- ========================================

-- Verificar si la columna puede_ver_incidencias existe
SET @col_exists_incidencias = (
    SELECT COUNT(*) 
    FROM information_schema.COLUMNS 
    WHERE TABLE_SCHEMA = DATABASE() 
    AND TABLE_NAME = 'usuarios' 
    AND COLUMN_NAME = 'puede_ver_incidencias'
);

-- Agregar columna puede_ver_incidencias si no existe
SET @sql_incidencias = IF(@col_exists_incidencias = 0,
    'ALTER TABLE usuarios ADD COLUMN puede_ver_incidencias BOOLEAN NOT NULL DEFAULT 1 COMMENT "Permiso para ver módulo de incidencias (aplica solo a rol Backoffice)" AFTER puede_ver_sips',
    'SELECT "La columna puede_ver_incidencias ya existe en usuarios" AS mensaje'
);
PREPARE stmt FROM @sql_incidencias;
EXECUTE stmt;
DEALLOCATE PREPARE stmt;

-- ========================================

-- Verificar si la columna puede_ver_ofertas existe
SET @col_exists_ofertas = (
    SELECT COUNT(*) 
    FROM information_schema.COLUMNS 
    WHERE TABLE_SCHEMA = DATABASE() 
    AND TABLE_NAME = 'usuarios' 
    AND COLUMN_NAME = 'puede_ver_ofertas'
);

-- Agregar columna puede_ver_ofertas si no existe
SET @sql_ofertas = IF(@col_exists_ofertas = 0,
    'ALTER TABLE usuarios ADD COLUMN puede_ver_ofertas BOOLEAN NOT NULL DEFAULT 1 COMMENT "Permiso para ver módulo de ofertas (aplica solo a rol Backoffice)" AFTER puede_ver_incidencias',
    'SELECT "La columna puede_ver_ofertas ya existe en usuarios" AS mensaje'
);
PREPARE stmt FROM @sql_ofertas;
EXECUTE stmt;
DEALLOCATE PREPARE stmt;

-- ========================================

-- Verificar si la columna puede_ver_usuarios existe
SET @col_exists_usuarios = (
    SELECT COUNT(*) 
    FROM information_schema.COLUMNS 
    WHERE TABLE_SCHEMA = DATABASE() 
    AND TABLE_NAME = 'usuarios' 
    AND COLUMN_NAME = 'puede_ver_usuarios'
);

-- Agregar columna puede_ver_usuarios si no existe
SET @sql_usuarios = IF(@col_exists_usuarios = 0,
    'ALTER TABLE usuarios ADD COLUMN puede_ver_usuarios BOOLEAN NOT NULL DEFAULT 1 COMMENT "Permiso para ver módulo de gestión de usuarios (aplica solo a rol Backoffice)" AFTER puede_ver_ofertas',
    'SELECT "La columna puede_ver_usuarios ya existe en usuarios" AS mensaje'
);
PREPARE stmt FROM @sql_usuarios;
EXECUTE stmt;
DEALLOCATE PREPARE stmt;

-- ========================================
-- VERIFICACIÓN FINAL
-- ========================================

SELECT 'VERIFICACIÓN DE COLUMNAS DE PERMISOS' AS titulo;

SELECT 
    COLUMN_NAME AS columna,
    DATA_TYPE AS tipo,
    COLUMN_DEFAULT AS valor_defecto,
    COLUMN_COMMENT AS comentario
FROM information_schema.COLUMNS 
WHERE TABLE_SCHEMA = DATABASE() 
  AND TABLE_NAME = 'usuarios' 
  AND COLUMN_NAME IN (
      'puede_ver_clientes',
      'puede_ver_contratos',
      'puede_ver_tarifas',
      'puede_ver_liquidaciones',
      'puede_ver_sips',
      'puede_ver_incidencias',
      'puede_ver_ofertas',
      'puede_ver_usuarios'
  )
ORDER BY ORDINAL_POSITION;

-- Reactivar modo de actualización segura
SET SQL_SAFE_UPDATES = 1;

SELECT 'Script ejecutado correctamente. Se deben tener 8 columnas de permisos.' AS resultado;

-- ========================================
-- INFORMACIÓN IMPORTANTE
-- ========================================
/*
ROL BACKOFFICE:
- Nuevo rol disponible: "Backoffice"
- NO recibe comisiones (solo el Administrador las recibe)
- El Administrador puede controlar qué módulos puede ver cada usuario Backoffice
- Permisos disponibles:
  * puede_ver_clientes: Acceso al módulo de clientes
  * puede_ver_contratos: Acceso al módulo de contratos
  * puede_ver_tarifas: Acceso al módulo de tarifas
  * puede_ver_liquidaciones: Acceso al módulo de liquidaciones
  * puede_ver_sips: Acceso al módulo SIPS
  * puede_ver_incidencias: Acceso al módulo de incidencias
  * puede_ver_ofertas: Acceso al módulo de ofertas
  * puede_ver_usuarios: Acceso al módulo de gestión de usuarios

IMPORTANTE:
- Estos permisos SOLO aplican a usuarios con rol "Backoffice"
- Para otros roles (Administrador, Colaborador, Gestor, etc.) el comportamiento es el habitual
- Por defecto, todos los permisos están activados (valor 1/true)
- El Administrador puede desactivarlos individualmente desde la ficha del usuario
*/
