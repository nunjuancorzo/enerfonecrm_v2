-- ============================================================================
-- SCRIPT DE MIGRACIÓN A PRODUCCIÓN
-- Fecha: 29 de marzo de 2026
-- Versión: 20260329
-- ============================================================================
-- 
-- CAMBIOS INCLUIDOS:
-- ==================
-- 1. ROL BACKOFFICE CON PERMISOS GRANULARES
--    - 8 nuevas columnas en tabla usuarios
--    - Control de acceso por módulo (clientes, contratos, tarifas, etc.)
--    - Sistema de permisos individuales para cada funcionalidad
--
-- 2. SISTEMA DE COMISIONES Y DECOMISIONES
--    - Campos de penalización en 3 tablas de tarifas
--    - Nueva tabla: configuracion_comisiones
--    - Nueva tabla: detalle_comision_liquidacion
--    - Nueva tabla: decomisiones
--    - Sistema completo de distribución jerárquica de comisiones
--    - Gestión de penalizaciones por bajas anticipadas
--
-- 3. NUEVA TABLA: comentarios_incidencias
--    - Sistema de comentarios para incidencias
--    - Notificaciones por email
--    - Tracking de emails enviados
--
-- 4. ESTADO "EN VALIDACIÓN" PARA INCIDENCIAS
--    - No requiere cambios de BD (solo código)
--
-- 5. OTROS CAMBIOS SOLO DE CÓDIGO (no requieren BD):
--    - Tipo tarifa "MovilAdicional"
--    - Mejoras SIPS (API sips3.php, parser CSV)
--    - Visualización bono social
--
-- REQUISITOS PREVIOS:
-- ===================
-- - MySQL 8.0+
-- - Base de datos: enerfone_crm (o crmgrupobasette según corresponda)
-- - Usuario con permisos CREATE, ALTER, INSERT
--
-- INSTRUCCIONES:
-- ==============
-- 1. Hacer backup de la base de datos antes de ejecutar:
--    mysqldump -u root -p enerfone_crm > backup_pre_20260329.sql
--
-- 2. Ejecutar este script:
--    mysql -u root -p enerfone_crm < MIGRACION_PRODUCCION_20260329.sql
--    # O para Grupo Basette:
--    mysql -u usuario -p crmgrupobasette < MIGRACION_PRODUCCION_20260329.sql
--
-- 3. Verificar que no haya errores en la salida
--
-- 4. Desplegar la aplicación versión 20260329
--
-- 5. Configurar SMTP en tabla configuraciones_empresa (para que funcionen emails)
--
-- ============================================================================

USE enerfone_crm;

-- Deshabilitar verificaciones temporalmente para mayor velocidad
SET SQL_SAFE_UPDATES = 0;

-- ============================================================================
-- 1. AGREGAR COLUMNAS DE PERMISOS PARA ROL BACKOFFICE
-- ============================================================================

-- Verificar y agregar columna puede_ver_clientes
SET @col_exists_clientes = (
    SELECT COUNT(*) 
    FROM information_schema.COLUMNS 
    WHERE TABLE_SCHEMA = DATABASE() 
    AND TABLE_NAME = 'usuarios' 
    AND COLUMN_NAME = 'puede_ver_clientes'
);

SET @sql_clientes = IF(@col_exists_clientes = 0,
    'ALTER TABLE usuarios ADD COLUMN puede_ver_clientes BOOLEAN NOT NULL DEFAULT 1 COMMENT "Permiso para ver módulo de clientes (aplica solo a rol Backoffice)" AFTER recibir_notificaciones',
    'SELECT "✓ La columna puede_ver_clientes ya existe" AS mensaje'
);
PREPARE stmt FROM @sql_clientes;
EXECUTE stmt;
DEALLOCATE PREPARE stmt;

-- Verificar y agregar columna puede_ver_contratos
SET @col_exists_contratos = (
    SELECT COUNT(*) 
    FROM information_schema.COLUMNS 
    WHERE TABLE_SCHEMA = DATABASE() 
    AND TABLE_NAME = 'usuarios' 
    AND COLUMN_NAME = 'puede_ver_contratos'
);

SET @sql_contratos = IF(@col_exists_contratos = 0,
    'ALTER TABLE usuarios ADD COLUMN puede_ver_contratos BOOLEAN NOT NULL DEFAULT 1 COMMENT "Permiso para ver módulo de contratos (aplica solo a rol Backoffice)" AFTER puede_ver_clientes',
    'SELECT "✓ La columna puede_ver_contratos ya existe" AS mensaje'
);
PREPARE stmt FROM @sql_contratos;
EXECUTE stmt;
DEALLOCATE PREPARE stmt;

-- Verificar y agregar columna puede_ver_tarifas
SET @col_exists_tarifas = (
    SELECT COUNT(*) 
    FROM information_schema.COLUMNS 
    WHERE TABLE_SCHEMA = DATABASE() 
    AND TABLE_NAME = 'usuarios' 
    AND COLUMN_NAME = 'puede_ver_tarifas'
);

SET @sql_tarifas = IF(@col_exists_tarifas = 0,
    'ALTER TABLE usuarios ADD COLUMN puede_ver_tarifas BOOLEAN NOT NULL DEFAULT 1 COMMENT "Permiso para ver módulo de tarifas (aplica solo a rol Backoffice)" AFTER puede_ver_contratos',
    'SELECT "✓ La columna puede_ver_tarifas ya existe" AS mensaje'
);
PREPARE stmt FROM @sql_tarifas;
EXECUTE stmt;
DEALLOCATE PREPARE stmt;

-- Verificar y agregar columna puede_ver_liquidaciones
SET @col_exists_liquidaciones = (
    SELECT COUNT(*) 
    FROM information_schema.COLUMNS 
    WHERE TABLE_SCHEMA = DATABASE() 
    AND TABLE_NAME = 'usuarios' 
    AND COLUMN_NAME = 'puede_ver_liquidaciones'
);

SET @sql_liquidaciones = IF(@col_exists_liquidaciones = 0,
    'ALTER TABLE usuarios ADD COLUMN puede_ver_liquidaciones BOOLEAN NOT NULL DEFAULT 1 COMMENT "Permiso para ver módulo de liquidaciones (aplica solo a rol Backoffice)" AFTER puede_ver_tarifas',
    'SELECT "✓ La columna puede_ver_liquidaciones ya existe" AS mensaje'
);
PREPARE stmt FROM @sql_liquidaciones;
EXECUTE stmt;
DEALLOCATE PREPARE stmt;

-- Verificar y agregar columna puede_ver_sips
SET @col_exists_sips = (
    SELECT COUNT(*) 
    FROM information_schema.COLUMNS 
    WHERE TABLE_SCHEMA = DATABASE() 
    AND TABLE_NAME = 'usuarios' 
    AND COLUMN_NAME = 'puede_ver_sips'
);

SET @sql_sips = IF(@col_exists_sips = 0,
    'ALTER TABLE usuarios ADD COLUMN puede_ver_sips BOOLEAN NOT NULL DEFAULT 1 COMMENT "Permiso para ver módulo SIPS (aplica solo a rol Backoffice)" AFTER puede_ver_liquidaciones',
    'SELECT "✓ La columna puede_ver_sips ya existe" AS mensaje'
);
PREPARE stmt FROM @sql_sips;
EXECUTE stmt;
DEALLOCATE PREPARE stmt;

-- Verificar y agregar columna puede_ver_incidencias
SET @col_exists_incidencias = (
    SELECT COUNT(*) 
    FROM information_schema.COLUMNS 
    WHERE TABLE_SCHEMA = DATABASE() 
    AND TABLE_NAME = 'usuarios' 
    AND COLUMN_NAME = 'puede_ver_incidencias'
);

SET @sql_incidencias = IF(@col_exists_incidencias = 0,
    'ALTER TABLE usuarios ADD COLUMN puede_ver_incidencias BOOLEAN NOT NULL DEFAULT 1 COMMENT "Permiso para ver módulo de incidencias (aplica solo a rol Backoffice)" AFTER puede_ver_sips',
    'SELECT "✓ La columna puede_ver_incidencias ya existe" AS mensaje'
);
PREPARE stmt FROM @sql_incidencias;
EXECUTE stmt;
DEALLOCATE PREPARE stmt;

-- Verificar y agregar columna puede_ver_ofertas
SET @col_exists_ofertas = (
    SELECT COUNT(*) 
    FROM information_schema.COLUMNS 
    WHERE TABLE_SCHEMA = DATABASE() 
    AND TABLE_NAME = 'usuarios' 
    AND COLUMN_NAME = 'puede_ver_ofertas'
);

SET @sql_ofertas = IF(@col_exists_ofertas = 0,
    'ALTER TABLE usuarios ADD COLUMN puede_ver_ofertas BOOLEAN NOT NULL DEFAULT 1 COMMENT "Permiso para ver módulo de ofertas (aplica solo a rol Backoffice)" AFTER puede_ver_incidencias',
    'SELECT "✓ La columna puede_ver_ofertas ya existe" AS mensaje'
);
PREPARE stmt FROM @sql_ofertas;
EXECUTE stmt;
DEALLOCATE PREPARE stmt;

-- Verificar y agregar columna puede_ver_usuarios
SET @col_exists_usuarios = (
    SELECT COUNT(*) 
    FROM information_schema.COLUMNS 
    WHERE TABLE_SCHEMA = DATABASE() 
    AND TABLE_NAME = 'usuarios' 
    AND COLUMN_NAME = 'puede_ver_usuarios'
);

SET @sql_usuarios = IF(@col_exists_usuarios = 0,
    'ALTER TABLE usuarios ADD COLUMN puede_ver_usuarios BOOLEAN NOT NULL DEFAULT 1 COMMENT "Permiso para ver gestión de usuarios (aplica solo a rol Backoffice)" AFTER puede_ver_ofertas',
    'SELECT "✓ La columna puede_ver_usuarios ya existe" AS mensaje'
);
PREPARE stmt FROM @sql_usuarios;
EXECUTE stmt;
DEALLOCATE PREPARE stmt;

SELECT '✓ Permisos de Backoffice agregados/verificados correctamente' AS resultado;

-- ============================================================================
-- 2. SISTEMA DE COMISIONES Y DECOMISIONES
-- ============================================================================

-- ----------------------------------------------------------------------------
-- 2.1. Agregar campos de penalización a tarifas
-- ----------------------------------------------------------------------------

-- TARIFAS ENERGÍA
SET @col_dias_pen_energia = (
    SELECT COUNT(*) 
    FROM information_schema.COLUMNS 
    WHERE TABLE_SCHEMA = DATABASE() 
    AND TABLE_NAME = 'tarifasenergia' 
    AND COLUMN_NAME = 'dias_penalizacion'
);

SET @sql_dias_pen_energia = IF(@col_dias_pen_energia = 0,
    'ALTER TABLE tarifasenergia ADD COLUMN dias_penalizacion INT NULL COMMENT "Días mínimos antes de permitir baja sin penalización" AFTER activa',
    'SELECT "✓ dias_penalizacion ya existe en tarifasenergia" AS mensaje'
);
PREPARE stmt FROM @sql_dias_pen_energia;
EXECUTE stmt;
DEALLOCATE PREPARE stmt;

SET @col_tipo_pen_energia = (
    SELECT COUNT(*) 
    FROM information_schema.COLUMNS 
    WHERE TABLE_SCHEMA = DATABASE() 
    AND TABLE_NAME = 'tarifasenergia' 
    AND COLUMN_NAME = 'tipo_penalizacion'
);

SET @sql_tipo_pen_energia = IF(@col_tipo_pen_energia = 0,
    'ALTER TABLE tarifasenergia ADD COLUMN tipo_penalizacion VARCHAR(20) NULL COMMENT "Total o Proporcional" AFTER dias_penalizacion',
    'SELECT "✓ tipo_penalizacion ya existe en tarifasenergia" AS mensaje'
);
PREPARE stmt FROM @sql_tipo_pen_energia;
EXECUTE stmt;
DEALLOCATE PREPARE stmt;

-- TARIFAS TELEFONÍA
SET @col_dias_pen_tel = (
    SELECT COUNT(*) 
    FROM information_schema.COLUMNS 
    WHERE TABLE_SCHEMA = DATABASE() 
    AND TABLE_NAME = 'tarifastelefonia' 
    AND COLUMN_NAME = 'dias_penalizacion'
);

SET @sql_dias_pen_tel = IF(@col_dias_pen_tel = 0,
    'ALTER TABLE tarifastelefonia ADD COLUMN dias_penalizacion INT NULL COMMENT "Días mínimos antes de permitir baja sin penalización" AFTER activa',
    'SELECT "✓ dias_penalizacion ya existe en tarifastelefonia" AS mensaje'
);
PREPARE stmt FROM @sql_dias_pen_tel;
EXECUTE stmt;
DEALLOCATE PREPARE stmt;

SET @col_tipo_pen_tel = (
    SELECT COUNT(*) 
    FROM information_schema.COLUMNS 
    WHERE TABLE_SCHEMA = DATABASE() 
    AND TABLE_NAME = 'tarifastelefonia' 
    AND COLUMN_NAME = 'tipo_penalizacion'
);

SET @sql_tipo_pen_tel = IF(@col_tipo_pen_tel = 0,
    'ALTER TABLE tarifastelefonia ADD COLUMN tipo_penalizacion VARCHAR(20) NULL COMMENT "Total o Proporcional" AFTER dias_penalizacion',
    'SELECT "✓ tipo_penalizacion ya existe en tarifastelefonia" AS mensaje'
);
PREPARE stmt FROM @sql_tipo_pen_tel;
EXECUTE stmt;
DEALLOCATE PREPARE stmt;

-- TARIFAS ALARMAS
SET @col_dias_pen_alarma = (
    SELECT COUNT(*) 
    FROM information_schema.COLUMNS 
    WHERE TABLE_SCHEMA = DATABASE() 
    AND TABLE_NAME = 'tarifas_alarmas' 
    AND COLUMN_NAME = 'dias_penalizacion'
);

SET @sql_dias_pen_alarma = IF(@col_dias_pen_alarma = 0,
    'ALTER TABLE tarifas_alarmas ADD COLUMN dias_penalizacion INT NULL COMMENT "Días mínimos antes de permitir baja sin penalización" AFTER activa',
    'SELECT "✓ dias_penalizacion ya existe en tarifas_alarmas" AS mensaje'
);
PREPARE stmt FROM @sql_dias_pen_alarma;
EXECUTE stmt;
DEALLOCATE PREPARE stmt;

SET @col_tipo_pen_alarma = (
    SELECT COUNT(*) 
    FROM information_schema.COLUMNS 
    WHERE TABLE_SCHEMA = DATABASE() 
    AND TABLE_NAME = 'tarifas_alarmas' 
    AND COLUMN_NAME = 'tipo_penalizacion'
);

SET @sql_tipo_pen_alarma = IF(@col_tipo_pen_alarma = 0,
    'ALTER TABLE tarifas_alarmas ADD COLUMN tipo_penalizacion VARCHAR(20) NULL COMMENT "Total o Proporcional" AFTER dias_penalizacion',
    'SELECT "✓ tipo_penalizacion ya existe en tarifas_alarmas" AS mensaje'
);
PREPARE stmt FROM @sql_tipo_pen_alarma;
EXECUTE stmt;
DEALLOCATE PREPARE stmt;

SELECT '✓ Campos de penalización agregados a tarifas' AS resultado;

-- ----------------------------------------------------------------------------
-- 2.2. Tabla: configuracion_comisiones
-- ----------------------------------------------------------------------------

CREATE TABLE IF NOT EXISTS configuracion_comisiones (
    id INT AUTO_INCREMENT PRIMARY KEY,
    usuario_id INT NOT NULL,
    tipo_proveedor VARCHAR(50) NOT NULL COMMENT 'Comercializadora, Operadora, EmpresaAlarma',
    proveedor_id INT NOT NULL COMMENT 'ID del proveedor específico',
    nombre_proveedor VARCHAR(200) NULL COMMENT 'Nombre del proveedor para referencia',
    
    -- Porcentajes de distribución jerárquica (0-100)
    porcentaje_colaborador DECIMAL(5,2) NOT NULL DEFAULT 70.00 COMMENT 'Porcentaje para el colaborador',
    porcentaje_gestor DECIMAL(5,2) NULL COMMENT 'Porcentaje para el gestor (si aplica)',
    porcentaje_jefe_ventas DECIMAL(5,2) NULL COMMENT 'Porcentaje para el jefe de ventas (si aplica)',
    porcentaje_director_comercial DECIMAL(5,2) NULL COMMENT 'Porcentaje para el director comercial (si aplica)',
    -- El porcentaje restante (100% - suma) se asigna automáticamente al Administrador
    
    fecha_creacion DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    activa BOOLEAN NOT NULL DEFAULT TRUE,
    
    -- Índice único para evitar duplicados de configuración por usuario-proveedor
    UNIQUE KEY uk_usuario_proveedor (usuario_id, tipo_proveedor, proveedor_id),
    
    -- Índices de búsqueda
    INDEX idx_usuario (usuario_id),
    INDEX idx_proveedor (tipo_proveedor, proveedor_id),
    INDEX idx_activa (activa)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci
COMMENT='Configuración de comisiones personalizadas por usuario y proveedor';

SELECT '✓ Tabla configuracion_comisiones creada/verificada' AS resultado;

-- ----------------------------------------------------------------------------
-- 2.3. Tabla: detalle_comision_liquidacion
-- ----------------------------------------------------------------------------

CREATE TABLE IF NOT EXISTS detalle_comision_liquidacion (
    id INT AUTO_INCREMENT PRIMARY KEY,
    historico_liquidacion_id INT NOT NULL,
    contrato_id INT NOT NULL,
    tipo_contrato VARCHAR(20) NULL COMMENT 'energia, telefonia, alarma',
    
    -- Comisión base del contrato (100%)
    comision_base DECIMAL(10,2) NOT NULL,
    
    -- COLABORADOR
    colaborador_id INT NOT NULL,
    comision_colaborador DECIMAL(10,2) NOT NULL,
    porcentaje_colaborador DECIMAL(5,2) NOT NULL,
    
    -- GESTOR (opcional)
    gestor_id INT NULL,
    comision_gestor DECIMAL(10,2) NULL,
    porcentaje_gestor DECIMAL(5,2) NULL,
    
    -- JEFE DE VENTAS (opcional)
    jefe_ventas_id INT NULL,
    comision_jefe_ventas DECIMAL(10,2) NULL,
    porcentaje_jefe_ventas DECIMAL(5,2) NULL,
    
    -- DIRECTOR COMERCIAL (opcional)
    director_comercial_id INT NULL,
    comision_director_comercial DECIMAL(10,2) NULL,
    porcentaje_director_comercial DECIMAL(5,2) NULL,
    
    -- ADMINISTRADOR (siempre recibe el restante)
    administrador_id INT NOT NULL,
    comision_administrador DECIMAL(10,2) NOT NULL,
    porcentaje_administrador DECIMAL(5,2) NOT NULL,
    
    -- Información del proveedor
    nombre_proveedor VARCHAR(200) NULL,
    tipo_proveedor VARCHAR(50) NULL COMMENT 'Comercializadora, Operadora, EmpresaAlarma',
    
    fecha_creacion DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    
    -- Índices de búsqueda
    INDEX idx_liquidacion (historico_liquidacion_id),
    INDEX idx_contrato (contrato_id),
    INDEX idx_colaborador (colaborador_id),
    INDEX idx_fecha_creacion (fecha_creacion),
    
    -- Índice único para evitar duplicados
    UNIQUE KEY uk_liquidacion_contrato (historico_liquidacion_id, contrato_id)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci
COMMENT='Detalle de distribución jerárquica de comisiones por liquidación';

SELECT '✓ Tabla detalle_comision_liquidacion creada/verificada' AS resultado;

-- ----------------------------------------------------------------------------
-- 2.4. Tabla: decomisiones
-- ----------------------------------------------------------------------------

CREATE TABLE IF NOT EXISTS decomisiones (
    id INT AUTO_INCREMENT PRIMARY KEY,
    contrato_id INT NOT NULL,
    usuario_id INT NOT NULL COMMENT 'Usuario que recibió la comisión original',
    nombre_usuario VARCHAR(200) NULL,
    
    -- Liquidaciones relacionadas
    liquidacion_original_id INT NULL COMMENT 'Liquidación donde se pagó la comisión',
    liquidacion_decomision_id INT NULL COMMENT 'Liquidación donde se registra la decomisión',
    
    -- Tipo de decomisión
    tipo_decomision VARCHAR(20) NOT NULL DEFAULT 'Total' COMMENT 'Total o Proporcional',
    
    -- Importes
    comision_original DECIMAL(10,2) NOT NULL COMMENT 'Comisión que se pagó originalmente',
    importe_decomision DECIMAL(10,2) NOT NULL COMMENT 'Importe a descontar (positivo)',
    
    -- Cálculo de penalización
    dias_penalizacion INT NOT NULL COMMENT 'Periodo de penalización en días (de la tarifa)',
    dias_activo INT NOT NULL COMMENT 'Días que estuvo activo el contrato',
    dias_pendientes INT NOT NULL COMMENT 'Días que faltaban por cumplir',
    
    -- Fechas
    fecha_alta DATETIME NULL COMMENT 'Fecha de alta/activación del contrato',
    fecha_baja DATETIME NOT NULL COMMENT 'Fecha de baja del contrato',
    fecha_creacion DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    
    -- Control
    creado_por_usuario_id INT NULL,
    observaciones VARCHAR(1000) NULL,
    estado VARCHAR(20) NOT NULL DEFAULT 'Pendiente' COMMENT 'Pendiente, Aplicada, Cancelada',
    
    -- Información adicional
    tipo_contrato VARCHAR(20) NULL COMMENT 'energia, telefonia, alarma',
    nombre_proveedor VARCHAR(200) NULL,
    
    -- Índices de búsqueda
    INDEX idx_contrato (contrato_id),
    INDEX idx_usuario (usuario_id),
    INDEX idx_estado (estado),
    INDEX idx_fecha_baja (fecha_baja),
    INDEX idx_liquidacion_decomision (liquidacion_decomision_id)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci
COMMENT='Decomisiones por bajas anticipadas de contratos';

SELECT '✓ Tabla decomisiones creada/verificada' AS resultado;

-- ============================================================================
-- 3. CREAR TABLA DE COMENTARIOS DE INCIDENCIAS
-- ============================================================================

-- Verificar si la tabla ya existe
SET @tabla_existe = (
    SELECT COUNT(*)
    FROM information_schema.TABLES 
    WHERE TABLE_SCHEMA = DATABASE() 
    AND TABLE_NAME = 'comentarios_incidencias'
);

-- Crear tabla solo si no existe
CREATE TABLE IF NOT EXISTS comentarios_incidencias (
    id INT AUTO_INCREMENT PRIMARY KEY,
    incidencia_id INT NOT NULL,
    usuario_id INT NULL COMMENT 'ID del usuario que creó el comentario (nullable)',
    nombre_usuario VARCHAR(100) NOT NULL COMMENT 'Nombre del usuario para mostrar',
    comentario VARCHAR(2000) NOT NULL COMMENT 'Texto del comentario',
    fecha_creacion DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP COMMENT 'Fecha de creación del comentario',
    email_enviado TINYINT(1) NOT NULL DEFAULT 0 COMMENT 'Indica si se envió email al usuario',
    
    -- Foreign Key solo a incidencias (CASCADE para eliminar comentarios si se elimina la incidencia)
    FOREIGN KEY (incidencia_id) REFERENCES incidencias(id) ON DELETE CASCADE,
    
    -- Índices para optimizar consultas
    INDEX idx_incidencia (incidencia_id),
    INDEX idx_fecha (fecha_creacion),
    INDEX idx_email_enviado (email_enviado)
    
) ENGINE=InnoDB 
  DEFAULT CHARSET=utf8mb4 
  COLLATE=utf8mb4_unicode_ci
  COMMENT='Comentarios de incidencias con sistema de notificaciones';

SELECT '✓ Tabla comentarios_incidencias creada/verificada correctamente' AS resultado;

-- ============================================================================
-- 3. VERIFICACIÓN POST-INSTALACIÓN
-- ============================================================================

-- ----------------------------------------------------------------------------
-- 3.1. Verificar columnas de permisos en tabla usuarios
-- ----------------------------------------------------------------------------
SELECT '=== VERIFICACIÓN: Columnas de permisos en usuarios ===' AS paso;
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

-- ----------------------------------------------------------------------------
-- 3.2. Verificar campos de penalización en tarifas
-- ----------------------------------------------------------------------------
SELECT '=== VERIFICACIÓN: Campos de penalización en tarifas ===' AS paso;

SELECT 'tarifasenergia' AS tabla, COUNT(*) AS campos_agregados
FROM information_schema.COLUMNS
WHERE TABLE_SCHEMA = DATABASE()
  AND TABLE_NAME = 'tarifasenergia'
  AND COLUMN_NAME IN ('dias_penalizacion', 'tipo_penalizacion')

UNION ALL

SELECT 'tarifastelefonia' AS tabla, COUNT(*) AS campos_agregados
FROM information_schema.COLUMNS
WHERE TABLE_SCHEMA = DATABASE()
  AND TABLE_NAME = 'tarifastelefonia'
  AND COLUMN_NAME IN ('dias_penalizacion', 'tipo_penalizacion')

UNION ALL

SELECT 'tarifas_alarmas' AS tabla, COUNT(*) AS campos_agregados
FROM information_schema.COLUMNS
WHERE TABLE_SCHEMA = DATABASE()
  AND TABLE_NAME = 'tarifas_alarmas'
  AND COLUMN_NAME IN ('dias_penalizacion', 'tipo_penalizacion');

-- ----------------------------------------------------------------------------
-- 3.3. Verificar tablas de comisiones
-- ----------------------------------------------------------------------------
SELECT '=== VERIFICACIÓN: Tablas del sistema de comisiones ===' AS paso;

SELECT 
    TABLE_NAME AS tabla,
    TABLE_ROWS AS filas_aproximadas,
    ROUND((DATA_LENGTH + INDEX_LENGTH) / 1024, 2) AS 'tamaño_kb',
    TABLE_COMMENT AS comentario
FROM information_schema.TABLES
WHERE TABLE_SCHEMA = DATABASE()
  AND TABLE_NAME IN (
      'configuracion_comisiones',
      'detalle_comision_liquidacion',
      'decomisiones'
  )
ORDER BY TABLE_NAME;

-- ----------------------------------------------------------------------------
-- 3.4. Verificar estructura de tabla comentarios_incidencias
-- ----------------------------------------------------------------------------
SELECT '=== VERIFICACIÓN: Estructura de comentarios_incidencias ===' AS paso;
DESC comentarios_incidencias;

-- Contar registros actuales
SELECT 
    'Tabla comentarios_incidencias creada/verificada correctamente' AS resultado,
    COUNT(*) AS total_comentarios
FROM comentarios_incidencias;

-- Verificar foreign keys
SELECT 
    'Foreign Keys de comentarios_incidencias' AS info,
    CONSTRAINT_NAME AS nombre_constraint,
    COLUMN_NAME AS columna,
    REFERENCED_TABLE_NAME AS tabla_referenciada,
    REFERENCED_COLUMN_NAME AS columna_referenciada
FROM information_schema.KEY_COLUMN_USAGE
WHERE TABLE_SCHEMA = DATABASE()
  AND TABLE_NAME = 'comentarios_incidencias'
  AND REFERENCED_TABLE_NAME IS NOT NULL;

-- ============================================================================
-- 4. NOTAS DE LA MIGRACIÓN
-- ============================================================================

SELECT '
============================================================================
MIGRACIÓN COMPLETADA EXITOSAMENTE - VERSIÓN 20260329
============================================================================

NUEVAS FUNCIONALIDADES DISPONIBLES:

────────────────────────────────────────────────────────────────────────────
1. ROL BACKOFFICE CON PERMISOS GRANULARES:
────────────────────────────────────────────────────────────────────────────
   ✓ Nuevo rol "Backoffice" disponible en gestión de usuarios
   ✓ 8 permisos individuales configurables:
     • puede_ver_clientes - Acceso al módulo de Clientes
     • puede_ver_contratos - Acceso al módulo de Contratos
     • puede_ver_tarifas - Acceso al módulo de Tarifas
     • puede_ver_liquidaciones - Acceso al módulo de Liquidaciones
     • puede_ver_sips - Acceso al módulo SIPS
     • puede_ver_incidencias - Acceso al módulo de Incidencias
     • puede_ver_ofertas - Acceso al módulo de Ofertas
     • puede_ver_usuarios - Acceso a gestión de Usuarios
   
   ✓ Configuración por usuario en modal de edición
   ✓ Todos los permisos activados por defecto
   ✓ En comisiones: Backoffice recibe 0%, Administrador 100%
   
   ORDEN JERÁRQUICO DE ROLES (de mayor a menor):
   1. Administrador
   2. Backoffice
   3. Director Comercial
   4. Jefe de Ventas
   5. Gestor
   6. Colaborador
   7. Comercializadora

────────────────────────────────────────────────────────────────────────────
2. SISTEMA DE COMISIONES Y DECOMISIONES:
────────────────────────────────────────────────────────────────────────────
   ✓ CAMPOS DE PENALIZACIÓN EN TARIFAS:
     • dias_penalizacion (INT) - Días mínimos antes de baja sin penalización
     • tipo_penalizacion (VARCHAR) - "Total" o "Proporcional"
     • Aplicado a: tarifasenergia, tarifastelefonia, tarifas_alarmas
   
   ✓ TABLA: configuracion_comisiones
     • Configuraciones personalizadas por usuario y proveedor
     • Porcentajes jerárquicos (colaborador, gestor, jefe ventas, director)
     • Control por comercializadora/operadora/empresa alarma
   
   ✓ TABLA: detalle_comision_liquidacion
     • Distribución detallada de comisiones por liquidación
     • Tracking de comisiones por cada nivel jerárquico
     • Vinculación con contratos y liquidaciones
   
   ✓ TABLA: decomisiones
     • Registro de penalizaciones por bajas anticipadas
     • Cálculo automático: Total o Proporcional
     • Estados: Pendiente, Aplicada, Cancelada
     • Vinculación con liquidaciones original y de decomisión
   
   FUNCIONALIDADES:
   • Distribución jerárquica automática de comisiones
   • Configuración personalizada por proveedor
   • Penalización por permanencia no cumplida
   • Cálculo proporcional de decomisiones
   • Trazabilidad completa de comisiones y penalizaciones

────────────────────────────────────────────────────────────────────────────
3. SISTEMA DE COMENTARIOS EN INCIDENCIAS:
────────────────────────────────────────────────────────────────────────────
   ✓ Administradores y Backoffice pueden agregar comentarios
   ✓ Cada comentario registra: fecha, usuario, texto
   ✓ Opción de enviar email automático al creador de la incidencia
   ✓ Historial completo de comentarios por incidencia
   ✓ Botón para enviar email de comentarios ya guardados
   
   ESTRUCTURA DE LA TABLA:
   • id - Identificador único del comentario
   • incidencia_id - Relación con incidencia (CASCADE DELETE)
   • usuario_id - ID del usuario que comentó (NULL permitido)
   • nombre_usuario - Nombre a mostrar en historial
   • comentario - Texto del comentario (máx 2000 caracteres)
   • fecha_creacion - Timestamp de creación
   • email_enviado - Flag indicador de envío de email

────────────────────────────────────────────────────────────────────────────
4. NUEVO ESTADO "EN VALIDACIÓN":
────────────────────────────────────────────────────────────────────────────
   ✓ Disponible en selector de estados de incidencias
   ✓ Útil para marcar incidencias en revisión
   ✓ Badge color azul (bg-info)
   ✓ No requiere cambios de BD (solo código)

────────────────────────────────────────────────────────────────────────────
5. OTROS CAMBIOS SOLO DE CÓDIGO:
────────────────────────────────────────────────────────────────────────────
   ✓ Tipo tarifa "MovilAdicional" (líneas móviles adicionales)
   ✓ Mejoras SIPS: API sips3.php para electricidad
   ✓ Parser CSV robusto RFC 4180 (maneja comillas y comas)
   ✓ Visualización bono social en consultas SIPS
   ✓ Doble llamada API electricidad (id=1 cliente + id=2 consumos)
   ✓ Gráficos de consumo histórico

────────────────────────────────────────────────────────────────────────────
PRÓXIMOS PASOS:
────────────────────────────────────────────────────────────────────────────
1. ✅ Reiniciar la aplicación web para cargar los nuevos cambios
   
2. ⚙️ Configurar SMTP en tabla "configuraciones_empresa":
   UPDATE configuraciones_empresa SET
       smtp_servidor = "smtp.ejemplo.com",
       smtp_puerto = 587,
       smtp_usuario = "usuario@ejemplo.com",
       smtp_password = "password",
       smtp_usar_ssl = TRUE,
       smtp_email_desde = "noreply@empresa.com",
       smtp_nombre_desde = "CRM Enerfone"
   WHERE id = 1;
   
3. 💰 Configurar penalizaciones en tarifas existentes:
   -- Ejemplo: Tarifas con permanencia de 12 meses
   UPDATE tarifasenergia SET
       dias_penalizacion = 365,
       tipo_penalizacion = "Proporcional"
   WHERE permanencia LIKE "%12%" AND activa = 1;
   
4. 🧪 Probar funcionalidades nuevas:
   • Crear usuario con rol Backoffice
   • Configurar permisos individuales
   • Configurar comisiones personalizadas por proveedor
   • Agregar comentario en incidencia de prueba
   • Verificar envío de emails
   • Probar cálculo de decomisiones
   
5. 📋 Verificar que Footer muestre "Versión 20260329"

────────────────────────────────────────────────────────────────────────────
TABLAS CREADAS/MODIFICADAS:
────────────────────────────────────────────────────────────────────────────
• usuarios (8 columnas nuevas de permisos)
• tarifasenergia (2 columnas nuevas de penalización)
• tarifastelefonia (2 columnas nuevas de penalización)
• tarifas_alarmas (2 columnas nuevas de penalización)
• configuracion_comisiones (NUEVA - comisiones personalizadas)
• detalle_comision_liquidacion (NUEVA - distribución comisiones)
• decomisiones (NUEVA - penalizaciones por bajas)
• comentarios_incidencias (NUEVA - sistema de comentarios)

============================================================================
' AS info;

-- ============================================================================
-- FIN DEL SCRIPT DE MIGRACIÓN
-- ============================================================================
