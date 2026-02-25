-- Script para crear la tabla de solicitudes de ofertas

CREATE TABLE IF NOT EXISTS solicitudes_ofertas (
    id INT AUTO_INCREMENT PRIMARY KEY,
    usuario_id INT NOT NULL,
    nombre_comercial VARCHAR(100) NOT NULL,
    email_comercial VARCHAR(100) NOT NULL,
    
    -- Tipos de oferta seleccionados
    tipo_luz BOOLEAN DEFAULT FALSE,
    tipo_gas BOOLEAN DEFAULT FALSE,
    tipo_fotovoltaica BOOLEAN DEFAULT FALSE,
    tipo_fibra BOOLEAN DEFAULT FALSE,
    tipo_movil BOOLEAN DEFAULT FALSE,
    tipo_fibra_movil BOOLEAN DEFAULT FALSE,
    tipo_fibra_movil_tv BOOLEAN DEFAULT FALSE,
    tipo_alarma BOOLEAN DEFAULT FALSE,
    
    -- Campos para Luz y Gas
    luz_gas_ruta_factura VARCHAR(500),
    luz_gas_observaciones VARCHAR(2000),
    
    -- Campos para Fotovoltaica
    fotovoltaica_ruta_factura VARCHAR(500),
    fotovoltaica_enlace_maps VARCHAR(500),
    fotovoltaica_observaciones VARCHAR(2000),
    
    -- Campos para Telefonía
    telefonia_tipo_solicitud VARCHAR(50), -- 'Alta Nueva' o 'Portabilidad'
    telefonia_ruta_factura VARCHAR(500),
    telefonia_contrato_actual VARCHAR(1000),
    telefonia_observaciones VARCHAR(2000),
    
    -- Campos para Alarma
    alarma_tipo VARCHAR(50), -- 'Negocio' o 'Residencial'
    alarma_tiene_actual BOOLEAN,
    alarma_observaciones VARCHAR(2000),
    
    -- Campos de control
    fecha_creacion DATETIME DEFAULT CURRENT_TIMESTAMP,
    estado VARCHAR(50) DEFAULT 'Pendiente',
    observaciones_admin VARCHAR(2000),
    fecha_procesado DATETIME,
    
    FOREIGN KEY (usuario_id) REFERENCES usuarios(idusuarios) ON DELETE CASCADE,
    INDEX idx_usuario (usuario_id),
    INDEX idx_fecha (fecha_creacion),
    INDEX idx_estado (estado)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
