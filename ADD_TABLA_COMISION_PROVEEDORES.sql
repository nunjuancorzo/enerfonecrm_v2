-- Script para crear tabla de comisiones por usuario y proveedor específico
-- Esta tabla almacena el porcentaje de comisión que cada usuario recibe de cada proveedor individual

CREATE TABLE IF NOT EXISTS usuario_comision_proveedores (
    id INT AUTO_INCREMENT PRIMARY KEY,
    usuario_id INT NOT NULL,
    tipo_proveedor ENUM('operadora', 'comercializadora', 'empresa_alarma') NOT NULL,
    proveedor_id INT NOT NULL,
    porcentaje_comision DECIMAL(5,2) NOT NULL DEFAULT 0,
    fecha_creacion TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    fecha_actualizacion TIMESTAMP DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    FOREIGN KEY (usuario_id) REFERENCES usuarios(idusuarios) ON DELETE CASCADE,
    UNIQUE KEY uk_usuario_proveedor (usuario_id, tipo_proveedor, proveedor_id),
    INDEX idx_usuario (usuario_id),
    INDEX idx_proveedor (tipo_proveedor, proveedor_id)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci COMMENT='Comisiones personalizadas por usuario y proveedor especifico';

-- Información sobre la estructura:
-- - usuario_id: ID del usuario (relación con tabla usuarios)
-- - tipo_proveedor: Tipo de proveedor (operadora, comercializadora, empresa_alarma)
-- - proveedor_id: ID específico del proveedor (id de operadoras, comercializadoras o empresas_alarmas)
-- - porcentaje_comision: Porcentaje de comisión (0-99.99)
-- - El UNIQUE KEY asegura que no haya duplicados
-- - El índice en usuario permite búsquedas rápidas por usuario
