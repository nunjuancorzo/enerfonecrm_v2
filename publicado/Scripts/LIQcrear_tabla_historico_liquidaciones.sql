-- Script para crear la tabla de histórico de liquidaciones

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
