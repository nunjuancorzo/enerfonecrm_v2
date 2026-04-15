-- Script para crear la tabla de log de activaciones de contratos
-- Esta tabla registra cada vez que un contrato cambia a estado Activo

CREATE TABLE IF NOT EXISTS log_activaciones_contratos (
    id INT AUTO_INCREMENT PRIMARY KEY,
    contrato_id INT NOT NULL,
    fecha_activacion DATE NOT NULL,
    fecha_registro DATETIME DEFAULT CURRENT_TIMESTAMP,
    usuario VARCHAR(255) NULL,
    observaciones TEXT NULL,
    FOREIGN KEY (contrato_id) REFERENCES contratos(id) ON DELETE CASCADE,
    INDEX idx_contrato_id (contrato_id),
    INDEX idx_fecha_activacion (fecha_activacion)
) COMMENT='Log de activaciones de contratos';
