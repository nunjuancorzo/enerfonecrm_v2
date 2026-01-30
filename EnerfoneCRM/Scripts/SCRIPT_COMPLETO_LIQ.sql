-- =====================================================================
-- SCRIPT CONSOLIDADO - MIGRACIONES LIQ
-- Base de datos: crmgrupobasette
-- Fecha de consolidación: 28 de enero de 2026
-- =====================================================================
-- IMPORTANTE: Este script ejecuta todos los cambios relacionados con 
-- liquidaciones en el orden correcto
-- =====================================================================

USE crmgrupobasette;

-- =====================================================================
-- 1. CREAR TABLA HISTÓRICO DE LIQUIDACIONES
-- =====================================================================
SELECT '--- 1/8: Creando tabla historico_liquidaciones ---' AS Paso;

DROP TABLE IF EXISTS historico_liquidaciones;

CREATE TABLE historico_liquidaciones (
    id INT AUTO_INCREMENT PRIMARY KEY,
    usuario_id INT NOT NULL,
    usuario_nombre VARCHAR(100) NOT NULL,
    usuario_email VARCHAR(255),
    cantidad_contratos INT NOT NULL DEFAULT 0,
    contratos_energia INT NOT NULL DEFAULT 0,
    contratos_telefonia INT NOT NULL DEFAULT 0,
    contratos_alarmas INT NOT NULL DEFAULT 0,
    fecha_aprobacion DATETIME NOT NULL,
    aprobado_por_id INT NOT NULL,
    aprobado_por_nombre VARCHAR(100) NOT NULL,
    observaciones VARCHAR(500),
    INDEX idx_usuario_id (usuario_id),
    INDEX idx_fecha_aprobacion (fecha_aprobacion),
    INDEX idx_aprobado_por_id (aprobado_por_id),
    FOREIGN KEY (usuario_id) REFERENCES usuarios(idusuarios) ON DELETE CASCADE,
    FOREIGN KEY (aprobado_por_id) REFERENCES usuarios(idusuarios) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

SELECT 'Tabla historico_liquidaciones creada correctamente' AS Resultado;

-- =====================================================================
-- 2. CREAR TABLA INCIDENCIAS DE LIQUIDACIÓN
-- =====================================================================
SELECT '--- 2/8: Creando tabla incidencias_liquidacion ---' AS Paso;

DROP TABLE IF EXISTS incidencias_liquidacion;

CREATE TABLE incidencias_liquidacion (
    id INT AUTO_INCREMENT PRIMARY KEY,
    usuario_colaborador_id INT NOT NULL,
    mensaje_colaborador TEXT NOT NULL,
    fecha_creacion DATETIME NOT NULL,
    respuesta_administrador TEXT NULL,
    usuario_administrador_id INT NULL,
    fecha_respuesta DATETIME NULL,
    estado VARCHAR(50) NOT NULL DEFAULT 'Pendiente',
    
    FOREIGN KEY (usuario_colaborador_id) REFERENCES usuarios(idusuarios) ON DELETE CASCADE,
    FOREIGN KEY (usuario_administrador_id) REFERENCES usuarios(idusuarios) ON DELETE SET NULL,
    
    INDEX idx_usuario_colaborador_id (usuario_colaborador_id),
    INDEX idx_estado (estado)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

SELECT 'Tabla incidencias_liquidacion creada correctamente' AS Resultado;

-- =====================================================================
-- 3. CREAR TABLAS USUARIO-OPERADORAS Y USUARIO-EMPRESAS ALARMAS
-- =====================================================================
SELECT '--- 3/8: Creando tablas de relación usuario_operadoras y usuario_empresas_alarmas ---' AS Paso;

-- Tabla para relacionar usuarios con operadoras permitidas
CREATE TABLE IF NOT EXISTS usuario_operadoras (
    idusuario_operadoras INT AUTO_INCREMENT PRIMARY KEY,
    usuario_id INT NOT NULL,
    operadora_id INT NOT NULL,
    fecha_asignacion DATETIME DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (usuario_id) REFERENCES usuarios(idusuarios) ON DELETE CASCADE,
    FOREIGN KEY (operadora_id) REFERENCES operadoras(id) ON DELETE CASCADE,
    UNIQUE KEY unique_usuario_operadora (usuario_id, operadora_id)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci
COMMENT='Relación entre usuarios y las operadoras que pueden visualizar';

-- Tabla para relacionar usuarios con empresas de alarmas permitidas
CREATE TABLE IF NOT EXISTS usuario_empresas_alarmas (
    idusuario_empresa_alarma INT AUTO_INCREMENT PRIMARY KEY,
    usuario_id INT NOT NULL,
    empresa_alarma_id INT NOT NULL,
    fecha_asignacion DATETIME DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (usuario_id) REFERENCES usuarios(idusuarios) ON DELETE CASCADE,
    FOREIGN KEY (empresa_alarma_id) REFERENCES empresas_alarmas(id) ON DELETE CASCADE,
    UNIQUE KEY unique_usuario_empresa_alarma (usuario_id, empresa_alarma_id)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci
COMMENT='Relación entre usuarios y las empresas de alarmas que pueden visualizar';

SELECT 'Tablas usuario_operadoras y usuario_empresas_alarmas creadas correctamente' AS Resultado;

-- =====================================================================
-- 4. AGREGAR CAMPO GESTOR_ID
-- =====================================================================
SELECT '--- 4/8: Agregando campo gestor_id ---' AS Paso;

-- Verificar si la columna existe
SET @col_exists_gestor = (
    SELECT COUNT(*) 
    FROM INFORMATION_SCHEMA.COLUMNS 
    WHERE TABLE_SCHEMA = 'crmgrupobasette' 
    AND TABLE_NAME = 'usuarios' 
    AND COLUMN_NAME = 'gestor_id'
);

-- Agregar campo gestor_id si no existe
SET @sql_gestor = IF(@col_exists_gestor = 0,
    'ALTER TABLE usuarios ADD COLUMN gestor_id INT NULL AFTER rol',
    'SELECT ''Campo gestor_id ya existe'' AS Mensaje'
);

PREPARE stmt FROM @sql_gestor;
EXECUTE stmt;
DEALLOCATE PREPARE stmt;

-- Agregar FK si no existe
SET @fk_exists_gestor = (
    SELECT COUNT(*) 
    FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS 
    WHERE CONSTRAINT_SCHEMA = 'crmgrupobasette' 
    AND TABLE_NAME = 'usuarios' 
    AND CONSTRAINT_NAME = 'fk_usuarios_gestor'
);

SET @sql_fk_gestor = IF(@fk_exists_gestor = 0,
    'ALTER TABLE usuarios ADD CONSTRAINT fk_usuarios_gestor FOREIGN KEY (gestor_id) REFERENCES usuarios(idusuarios) ON DELETE SET NULL',
    'SELECT ''FK fk_usuarios_gestor ya existe'' AS Mensaje'
);

PREPARE stmt FROM @sql_fk_gestor;
EXECUTE stmt;
DEALLOCATE PREPARE stmt;

-- Agregar índice si no existe
SET @idx_exists_gestor = (
    SELECT COUNT(*) 
    FROM INFORMATION_SCHEMA.STATISTICS 
    WHERE TABLE_SCHEMA = 'crmgrupobasette' 
    AND TABLE_NAME = 'usuarios' 
    AND INDEX_NAME = 'idx_gestor_id'
);

SET @sql_idx_gestor = IF(@idx_exists_gestor = 0,
    'CREATE INDEX idx_gestor_id ON usuarios(gestor_id)',
    'SELECT ''Índice idx_gestor_id ya existe'' AS Mensaje'
);

PREPARE stmt FROM @sql_idx_gestor;
EXECUTE stmt;
DEALLOCATE PREPARE stmt;

SELECT 'Campo gestor_id agregado correctamente' AS Resultado;

-- =====================================================================
-- 5. AGREGAR CAMPO JEFE_VENTAS_ID
-- =====================================================================
SELECT '--- 5/8: Agregando campo jefe_ventas_id ---' AS Paso;

-- Verificar si la columna existe
SET @col_exists_jefe = (
    SELECT COUNT(*) 
    FROM INFORMATION_SCHEMA.COLUMNS 
    WHERE TABLE_SCHEMA = 'crmgrupobasette' 
    AND TABLE_NAME = 'usuarios' 
    AND COLUMN_NAME = 'jefe_ventas_id'
);

-- Agregar el campo jefe_ventas_id si no existe
SET @sql_add_column_jefe = IF(@col_exists_jefe = 0,
    'ALTER TABLE usuarios ADD COLUMN jefe_ventas_id INT NULL COMMENT ''ID del jefe de ventas al que pertenece el usuario (Gestor o Colaborador)''',
    'SELECT ''La columna jefe_ventas_id ya existe'' AS Mensaje'
);

PREPARE stmt FROM @sql_add_column_jefe;
EXECUTE stmt;
DEALLOCATE PREPARE stmt;

-- Verificar si la FK existe antes de crearla
SET @fk_exists_jefe = (
    SELECT COUNT(*) 
    FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS 
    WHERE CONSTRAINT_SCHEMA = 'crmgrupobasette' 
    AND TABLE_NAME = 'usuarios' 
    AND CONSTRAINT_NAME = 'fk_usuarios_jefe_ventas'
);

-- Crear la restricción de clave foránea si no existe
SET @sql_add_fk_jefe = IF(@fk_exists_jefe = 0,
    'ALTER TABLE usuarios ADD CONSTRAINT fk_usuarios_jefe_ventas FOREIGN KEY (jefe_ventas_id) REFERENCES usuarios(idusuarios) ON DELETE SET NULL ON UPDATE CASCADE',
    'SELECT ''La FK fk_usuarios_jefe_ventas ya existe'' AS Mensaje'
);

PREPARE stmt FROM @sql_add_fk_jefe;
EXECUTE stmt;
DEALLOCATE PREPARE stmt;

-- Verificar si el índice existe antes de crearlo
SET @idx_exists_jefe = (
    SELECT COUNT(*) 
    FROM INFORMATION_SCHEMA.STATISTICS 
    WHERE TABLE_SCHEMA = 'crmgrupobasette' 
    AND TABLE_NAME = 'usuarios' 
    AND INDEX_NAME = 'idx_jefe_ventas_id'
);

-- Crear índice para mejorar el rendimiento de las consultas si no existe
SET @sql_add_idx_jefe = IF(@idx_exists_jefe = 0,
    'CREATE INDEX idx_jefe_ventas_id ON usuarios(jefe_ventas_id)',
    'SELECT ''El índice idx_jefe_ventas_id ya existe'' AS Mensaje'
);

PREPARE stmt FROM @sql_add_idx_jefe;
EXECUTE stmt;
DEALLOCATE PREPARE stmt;

SELECT 'Campo jefe_ventas_id agregado correctamente' AS Resultado;

-- =====================================================================
-- 6. AGREGAR CAMPO FECHA_ALTA
-- =====================================================================
SELECT '--- 6/8: Agregando campo fecha_alta ---' AS Paso;

-- Verificar si la columna existe
SET @col_exists_fecha_alta = (
    SELECT COUNT(*) 
    FROM INFORMATION_SCHEMA.COLUMNS 
    WHERE TABLE_SCHEMA = 'crmgrupobasette' 
    AND TABLE_NAME = 'contratos' 
    AND COLUMN_NAME = 'fecha_alta'
);

-- Agregar la columna fecha_alta si no existe
SET @sql_fecha_alta = IF(@col_exists_fecha_alta = 0,
    'ALTER TABLE contratos ADD COLUMN fecha_alta DATETIME NULL AFTER fecha_activo',
    'SELECT ''Campo fecha_alta ya existe'' AS Mensaje'
);

PREPARE stmt FROM @sql_fecha_alta;
EXECUTE stmt;
DEALLOCATE PREPARE stmt;

-- Actualizar fecha_alta con fecha_creacion para registros que no tienen fecha_alta
SET @sql_update_fecha_alta = IF(@col_exists_fecha_alta = 0,
    'UPDATE contratos SET fecha_alta = fecha_creacion WHERE fecha_creacion IS NOT NULL',
    'SELECT ''No se requiere actualizar fecha_alta'' AS Mensaje'
);

PREPARE stmt FROM @sql_update_fecha_alta;
EXECUTE stmt;
DEALLOCATE PREPARE stmt;

SELECT 'Campo fecha_alta agregado y actualizado correctamente' AS Resultado;

-- =====================================================================
-- 7. CAMBIAR ROL USUARIO A COLABORADOR
-- =====================================================================
SELECT '--- 7/8: Cambiando rol Usuario a Colaborador ---' AS Paso;

-- Desactivar temporalmente el modo safe update
SET SQL_SAFE_UPDATES = 0;

-- Actualizar todos los usuarios con rol "Usuario" a "Colaborador"
UPDATE usuarios
SET rol = 'Colaborador'
WHERE rol = 'Usuario';

-- Reactivar el modo safe update
SET SQL_SAFE_UPDATES = 1;

SELECT 'Roles actualizados de Usuario a Colaborador' AS Resultado;

-- =====================================================================
-- 8. MIGRAR URLs DE STORAGE
-- =====================================================================
SELECT '--- 8/8: Migrando URLs a storage ---' AS Paso;

-- Desactivar modo seguro temporalmente
SET SQL_SAFE_UPDATES = 0;

-- Actualizar URLs de mensajes de bienvenida
UPDATE mensajes_bienvenida 
SET imagen_url = REPLACE(imagen_url, '/uploads/mensajes/', '/storage/uploads/mensajes/')
WHERE imagen_url LIKE '/uploads/mensajes/%';

-- Actualizar URLs de PDFs de contratos
UPDATE contratos 
SET pdf_contrato_url = REPLACE(pdf_contrato_url, '/uploads/contratos/', '/storage/uploads/contratos/')
WHERE pdf_contrato_url LIKE '/uploads/contratos/%';

-- Reactivar el modo seguro
SET SQL_SAFE_UPDATES = 1;

SELECT 'URLs migradas correctamente a storage' AS Resultado;

-- =====================================================================
-- RESUMEN FINAL
-- =====================================================================
SELECT '========================================' AS '';
SELECT 'SCRIPT COMPLETADO EXITOSAMENTE' AS '';
SELECT '========================================' AS '';

-- Verificar tablas creadas
SELECT 'Tablas creadas:' AS info;
SHOW TABLES LIKE 'historico_liquidaciones';
SHOW TABLES LIKE 'incidencias_liquidacion';
SHOW TABLES LIKE 'usuario_operadoras';
SHOW TABLES LIKE 'usuario_empresas_alarmas';

-- Mostrar resumen de roles
SELECT '========================================' AS '';
SELECT 'Resumen de roles actuales:' AS info;
SELECT rol, COUNT(*) as cantidad
FROM usuarios
GROUP BY rol
ORDER BY rol;

SELECT '========================================' AS '';
SELECT 'Mensajes de bienvenida con storage:' as info, COUNT(*) as cantidad 
FROM mensajes_bienvenida 
WHERE imagen_url LIKE '/storage/uploads/mensajes/%';

SELECT 'Contratos con PDF en storage:' as info, COUNT(*) as cantidad 
FROM contratos 
WHERE pdf_contrato_url LIKE '/storage/uploads/contratos/%';

SELECT '========================================' AS '';
SELECT 'TODAS LAS MIGRACIONES COMPLETADAS' AS Resultado;
SELECT '========================================' AS '';
