-- Script para crear tabla de configuración de comisiones por usuario y proveedor
-- Sistema de comisiones jerárquicas personalizadas
-- Ejecutar en la base de datos enerfone_crm
-- Fecha: Marzo 2026

USE enerfone_crm;

-- ========================================
-- TABLA: configuracion_comisiones
-- ========================================
-- Almacena configuraciones de comisión específicas por usuario y proveedor
-- Permite definir porcentajes diferenciados por comercializadora/operadora/empresa de alarmas

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

-- Agregar clave foránea si la tabla usuarios existe y tiene el campo id correcto
-- Si da error, ejecutar manualmente después de verificar la estructura de usuarios
-- ALTER TABLE configuracion_comisiones 
-- ADD CONSTRAINT fk_config_comision_usuario 
-- FOREIGN KEY (usuario_id) REFERENCES usuarios(id) ON DELETE CASCADE;

-- ========================================
-- CONFIGURACIONES POR DEFECTO (EJEMPLOS)
-- ========================================
-- Estos valores son ejemplos y deben ajustarse según la política de cada empresa

-- Ejemplo: Configuración por defecto para colaboradores
-- 70% colaborador, 10% gestor, 10% jefe ventas, 10% administrador
-- DESCOMENTAR Y AJUSTAR SEGÚN NECESIDADES:

/*
INSERT INTO configuracion_comisiones 
    (usuario_id, tipo_proveedor, proveedor_id, nombre_proveedor, 
     porcentaje_colaborador, porcentaje_gestor, porcentaje_jefe_ventas, 
     porcentaje_director_comercial, activa)
SELECT 
    u.id,
    'Comercializadora',
    c.id,
    c.nombre,
    70.00,  -- Colaborador
    10.00,  -- Gestor
    10.00,  -- Jefe de ventas
    NULL,   -- Director comercial (no aplica por defecto)
    TRUE
FROM usuarios u
CROSS JOIN comercializadoras c
WHERE u.rol = 'Colaborador' 
  AND u.activo = 1;
*/

-- Verificar creación
SELECT 'Tabla configuracion_comisiones creada correctamente' AS resultado;
SELECT TABLE_NAME, TABLE_ROWS 
FROM information_schema.TABLES 
WHERE TABLE_SCHEMA = 'enerfone_crm' 
  AND TABLE_NAME = 'configuracion_comisiones';
