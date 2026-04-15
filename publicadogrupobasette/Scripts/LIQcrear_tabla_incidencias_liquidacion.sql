-- Script para crear la tabla de incidencias de liquidación
-- Fecha: 28/01/2026
-- IMPORTANTE: Ejecutar DROP TABLE si ya existe la tabla anterior

DROP TABLE IF EXISTS incidencias_liquidacion;

CREATE TABLE incidencias_liquidacion (
    id INT AUTO_INCREMENT PRIMARY KEY,
    usuario_colaborador_id INT NOT NULL,
    mensaje_colaborador TEXT NOT NULL,
    fecha_creacion DATETIME NOT NULL,
    respuesta_administrador TEXT NULL,
    usuario_administrador_id INT NULL,
    fecha_respuesta DATETIME NULL,
    estado VARCHAR(50) NOT NULL DEFAULT 'Pendiente',
    
    FOREIGN KEY (usuario_colaborador_id) REFERENCES usuarios(idusuarios) ON DELETE CASCADE,
    FOREIGN KEY (usuario_administrador_id) REFERENCES usuarios(idusuarios) ON DELETE SET NULL,
    
    INDEX idx_usuario_colaborador_id (usuario_colaborador_id),
    INDEX idx_estado (estado)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
