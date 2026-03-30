-- Script para crear tabla de detalles de comisión por liquidación
-- Sistema de distribución jerárquica de comisiones
-- Ejecutar en la base de datos enerfone_crm
-- Fecha: Marzo 2026

USE enerfone_crm;

-- ========================================
-- TABLA: detalle_comision_liquidacion
-- ========================================
-- Registra la distribución de comisiones entre todos los niveles jerárquicos
-- para cada contrato incluido en una liquidación

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

-- Agregar claves foráneas (descomentar después de verificar estructura de tablas)
-- Si da error, verificar que las tablas y columnas referenciadas existan
/*
ALTER TABLE detalle_comision_liquidacion 
ADD CONSTRAINT fk_detalle_liquidacion 
FOREIGN KEY (historico_liquidacion_id) REFERENCES historico_liquidaciones(id) ON DELETE CASCADE;

ALTER TABLE detalle_comision_liquidacion 
ADD CONSTRAINT fk_detalle_contrato 
FOREIGN KEY (contrato_id) REFERENCES contratos(id) ON DELETE CASCADE;

ALTER TABLE detalle_comision_liquidacion 
ADD CONSTRAINT fk_detalle_colaborador 
FOREIGN KEY (colaborador_id) REFERENCES usuarios(id);

ALTER TABLE detalle_comision_liquidacion 
ADD CONSTRAINT fk_detalle_gestor 
FOREIGN KEY (gestor_id) REFERENCES usuarios(id);

ALTER TABLE detalle_comision_liquidacion 
ADD CONSTRAINT fk_detalle_jefe_ventas 
FOREIGN KEY (jefe_ventas_id) REFERENCES usuarios(id);

ALTER TABLE detalle_comision_liquidacion 
ADD CONSTRAINT fk_detalle_director 
FOREIGN KEY (director_comercial_id) REFERENCES usuarios(id);

ALTER TABLE detalle_comision_liquidacion 
ADD CONSTRAINT fk_detalle_administrador 
FOREIGN KEY (administrador_id) REFERENCES usuarios(id);
*/

-- ========================================
-- VISTA AUXILIAR: Resumen de comisiones por usuario y liquidación
-- ========================================
-- NOTA: Descomentar y ajustar según estructura real de tabla usuarios
-- Esta vista requiere conocer los nombres exactos de las columnas de usuarios

/*
CREATE OR REPLACE VIEW v_comisiones_por_usuario AS
SELECT 
    dcl.historico_liquidacion_id,
    u.id AS usuario_id,
    u.nombre_usuario,
    u.nombre,
    u.apellidos,
    u.rol,
    SUM(CASE 
        WHEN dcl.colaborador_id = u.id THEN dcl.comision_colaborador
        WHEN dcl.gestor_id = u.id THEN dcl.comision_gestor
        WHEN dcl.jefe_ventas_id = u.id THEN dcl.comision_jefe_ventas
        WHEN dcl.director_comercial_id = u.id THEN dcl.comision_director_comercial
        WHEN dcl.administrador_id = u.id THEN dcl.comision_administrador
        ELSE 0
    END) AS total_comisiones,
    COUNT(DISTINCT dcl.contrato_id) AS total_contratos
FROM detalle_comision_liquidacion dcl
CROSS JOIN usuarios u
WHERE u.id IN (
    dcl.colaborador_id, 
    dcl.gestor_id, 
    dcl.jefe_ventas_id, 
    dcl.director_comercial_id, 
    dcl.administrador_id
)
GROUP BY dcl.historico_liquidacion_id, u.id, u.nombre_usuario, u.nombre, u.apellidos, u.rol;
*/

-- ========================================
-- STORED PROCEDURE: Calcular totales por liquidación
-- ========================================
-- NOTA: Descomentar después de verificar que funciona correctamente

/*
DELIMITER //

CREATE PROCEDURE sp_calcular_totales_liquidacion(
    IN p_liquidacion_id INT
)
BEGIN
    -- Actualiza el total_comisiones en historico_liquidaciones
    UPDATE historico_liquidaciones hl
    SET hl.total_comisiones = (
        SELECT SUM(comision_base)
        FROM detalle_comision_liquidacion
        WHERE historico_liquidacion_id = p_liquidacion_id
    )
    WHERE hl.id = p_liquidacion_id;
    
    -- Devuelve resumen
    SELECT 
        'Total contratos' AS concepto, 
        COUNT(*) AS valor
    FROM detalle_comision_liquidacion
    WHERE historico_liquidacion_id = p_liquidacion_id
    
    UNION ALL
    
    SELECT 
        'Total comisiones' AS concepto, 
        SUM(comision_base) AS valor
    FROM detalle_comision_liquidacion
    WHERE historico_liquidacion_id = p_liquidacion_id;
END //

DELIMITER ;
*/

-- Verificar creación
SELECT 'Tabla detalle_comision_liquidacion creada correctamente' AS resultado;
SELECT TABLE_NAME, TABLE_ROWS 
FROM information_schema.TABLES 
WHERE TABLE_SCHEMA = 'enerfone_crm' 
  AND TABLE_NAME = 'detalle_comision_liquidacion';
