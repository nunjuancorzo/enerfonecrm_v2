-- =====================================================
-- Histórico de cambios de contratos
-- Registra cada campo modificado al guardar un contrato
-- =====================================================

CREATE TABLE IF NOT EXISTS historicocambioscontratos (
    id INT NOT NULL AUTO_INCREMENT,
    idcontrato INT NOT NULL,
    campo VARCHAR(100) NOT NULL,
    dato VARCHAR(500) NULL,
    usuario VARCHAR(100) NULL,
    fecha_cambio DATETIME NOT NULL,

    PRIMARY KEY (id),
    INDEX idx_historicocambios_contrato (idcontrato),
    INDEX idx_historicocambios_fecha (fecha_cambio)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
