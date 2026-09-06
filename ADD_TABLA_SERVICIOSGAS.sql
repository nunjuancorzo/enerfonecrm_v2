-- =====================================================
-- Servicios de gas
-- Misma estructura que la tabla `servicios` (que pasa a ser solo de luz)
-- =====================================================

CREATE TABLE IF NOT EXISTS serviciosgas (
    id INT NOT NULL AUTO_INCREMENT,
    tipo VARCHAR(50) NOT NULL,                  -- Residencial / Pyme
    nombreServicio VARCHAR(100) NOT NULL,
    precio VARCHAR(50) NOT NULL,
    empresa VARCHAR(100) NULL,                  -- Comercializadora

    PRIMARY KEY (id),
    INDEX idx_serviciosgas_tipo (tipo),
    INDEX idx_serviciosgas_empresa (empresa)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
