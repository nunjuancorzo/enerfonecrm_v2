-- Script para crear la tabla de log de accesos de usuarios
-- Fecha: 29/12/2025

CREATE TABLE IF NOT EXISTS log_accesos (
    id INT AUTO_INCREMENT PRIMARY KEY,
    id_usuario INT NOT NULL,
    nombre_usuario VARCHAR(45) NOT NULL,
    rol VARCHAR(45) NOT NULL,
    fecha_acceso DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    INDEX idx_usuario (id_usuario),
    INDEX idx_fecha (fecha_acceso),
    INDEX idx_nombre_usuario (nombre_usuario),
    FOREIGN KEY (id_usuario) REFERENCES usuarios(idusuarios) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
