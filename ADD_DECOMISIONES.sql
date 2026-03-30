-- Script para crear tabla de decomisiones (penalizaciones por bajas anticipadas)
-- Sistema de gestión de penalizaciones
-- Ejecutar en la base de datos enerfone_crm
-- Fecha: Marzo 2026

USE enerfone_crm;

-- ========================================
-- TABLA: decomisiones
-- ========================================
-- Registra decomisiones por bajas anticipadas de contratos
-- antes de cumplir el periodo de penalización establecido en la tarifa

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

-- Agregar claves foráneas (descomentar después de verificar estructura de tablas)
-- Si da error, verificar que las tablas y columnas referenciadas existan
/*
ALTER TABLE decomisiones 
ADD CONSTRAINT fk_decomision_contrato 
FOREIGN KEY (contrato_id) REFERENCES contratos(id) ON DELETE CASCADE;

ALTER TABLE decomisiones 
ADD CONSTRAINT fk_decomision_usuario 
FOREIGN KEY (usuario_id) REFERENCES usuarios(id);

ALTER TABLE decomisiones 
ADD CONSTRAINT fk_decomision_liquidacion_original 
FOREIGN KEY (liquidacion_original_id) REFERENCES historico_liquidaciones(id);

ALTER TABLE decomisiones 
ADD CONSTRAINT fk_decomision_liquidacion_decomision 
FOREIGN KEY (liquidacion_decomision_id) REFERENCES historico_liquidaciones(id);

ALTER TABLE decomisiones 
ADD CONSTRAINT fk_decomision_creado_por 
FOREIGN KEY (creado_por_usuario_id) REFERENCES usuarios(id);
*/

-- ========================================
-- VISTA: Resumen de decomisiones por usuario
-- ========================================
-- NOTA: Descomentar y ajustar según estructura real de tabla usuarios
/*
CREATE OR REPLACE VIEW v_decomisiones_por_usuario AS
SELECT 
    d.usuario_id,
    u.nombre_usuario,
    u.nombre,
    u.apellidos,
    COUNT(*) AS total_decomisiones,
    SUM(d.importe_decomision) AS total_importe,
    SUM(CASE WHEN d.estado = 'Pendiente' THEN d.importe_decomision ELSE 0 END) AS pendiente,
    SUM(CASE WHEN d.estado = 'Aplicada' THEN d.importe_decomision ELSE 0 END) AS aplicada,
    SUM(CASE WHEN d.estado = 'Cancelada' THEN 0 ELSE d.importe_decomision END) AS efectivo
FROM decomisiones d
INNER JOIN usuarios u ON d.usuario_id = u.id
GROUP BY d.usuario_id, u.nombre_usuario, u.nombre, u.apellidos;
*/

-- ========================================
-- VISTA: Decomisiones pendientes de aplicar
-- ========================================
-- NOTA: Descomentar y ajustar según estructura real de tablas usuarios y contratos
/*
CREATE OR REPLACE VIEW v_decomisiones_pendientes AS
SELECT 
    d.id,
    d.contrato_id,
    c.tipo AS tipo_contrato,
    c.comercial,
    d.usuario_id,
    u.nombre_usuario,
    d.comision_original,
    d.importe_decomision,
    d.tipo_decomision,
    d.dias_penalizacion,
    d.dias_activo,
    d.dias_pendientes,
    d.fecha_baja,
    d.fecha_creacion,
    d.nombre_proveedor,
    ROUND((d.importe_decomision / d.comision_original) * 100, 2) AS porcentaje_penalizacion
FROM decomisiones d
INNER JOIN contratos c ON d.contrato_id = c.id
INNER JOIN usuarios u ON d.usuario_id = u.id
WHERE d.estado = 'Pendiente'
ORDER BY d.fecha_creacion DESC;
*/

-- ========================================
-- STORED PROCEDURE: Crear decomisión automática
-- ========================================
-- NOTA: Este stored procedure requiere conocer los nombres exactos de las columnas
-- Descomentar y ajustar después de verificar estructura de tablas
/*
DELIMITER //

CREATE PROCEDURE sp_crear_decomision(
    IN p_contrato_id INT,
    IN p_usuario_id INT,
    IN p_liquidacion_original_id INT,
    IN p_comision_original DECIMAL(10,2),
    IN p_dias_penalizacion INT,
    IN p_tipo_penalizacion VARCHAR(20),
    IN p_fecha_alta DATETIME,
    IN p_fecha_baja DATETIME,
    IN p_creado_por INT,
    IN p_observaciones VARCHAR(1000),
    OUT p_decomision_id INT,
    OUT p_importe_decomision DECIMAL(10,2)
)
BEGIN
    DECLARE v_dias_activo INT;
    DECLARE v_dias_pendientes INT;
    DECLARE v_importe DECIMAL(10,2);
    DECLARE v_nombre_usuario VARCHAR(200);
    DECLARE v_tipo_contrato VARCHAR(20);
    DECLARE v_nombre_proveedor VARCHAR(200);
    
    -- Calcular días activo
    SET v_dias_activo = DATEDIFF(p_fecha_baja, p_fecha_alta);
    SET v_dias_pendientes = p_dias_penalizacion - v_dias_activo;
    
    -- Si ya cumplió el periodo, no hay decomisión
    IF v_dias_pendientes <= 0 THEN
        SET p_decomision_id = NULL;
        SET p_importe_decomision = 0;
    ELSE
        -- Calcular importe según tipo de penalización
        IF p_tipo_penalizacion = 'Total' THEN
            SET v_importe = p_comision_original;
        ELSEIF p_tipo_penalizacion = 'Proporcional' THEN
            SET v_importe = ROUND((p_comision_original * v_dias_pendientes) / p_dias_penalizacion, 2);
        ELSE
            SET v_importe = p_comision_original;
        END IF;
        
        -- Obtener datos adicionales
        SELECT 
            u.nombre_usuario,
            c.tipo,
            COALESCE(c.en_comercializadora, c.operadora_tel, c.empresa_alarma) 
        INTO 
            v_nombre_usuario,
            v_tipo_contrato,
            v_nombre_proveedor
        FROM contratos c
        INNER JOIN usuarios u ON u.id = p_usuario_id
        WHERE c.id = p_contrato_id;
        
        -- Insertar decomisión
        INSERT INTO decomisiones (
            contrato_id, usuario_id, nombre_usuario,
            liquidacion_original_id, tipo_decomision,
            comision_original, importe_decomision,
            dias_penalizacion, dias_activo, dias_pendientes,
            fecha_alta, fecha_baja, fecha_creacion,
            creado_por_usuario_id, observaciones, estado,
            tipo_contrato, nombre_proveedor
        ) VALUES (
            p_contrato_id, p_usuario_id, v_nombre_usuario,
            p_liquidacion_original_id, p_tipo_penalizacion,
            p_comision_original, v_importe,
            p_dias_penalizacion, v_dias_activo, v_dias_pendientes,
            p_fecha_alta, p_fecha_baja, NOW(),
            p_creado_por, p_observaciones, 'Pendiente',
            v_tipo_contrato, v_nombre_proveedor
        );
        
        SET p_decomision_id = LAST_INSERT_ID();
        SET p_importe_decomision = v_importe;
    END IF;
END //

DELIMITER ;
*/

-- Verificar creación
SELECT 'Tabla decomisiones creada correctamente' AS resultado;
SELECT TABLE_NAME, TABLE_ROWS 
FROM information_schema.TABLES 
WHERE TABLE_SCHEMA = 'enerfone_crm' 
  AND TABLE_NAME = 'decomisiones';
