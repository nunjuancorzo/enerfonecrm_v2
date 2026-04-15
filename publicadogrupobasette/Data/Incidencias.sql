-- Script para crear la tabla de incidencias
-- Ejecutar este script en la base de datos de EnerfoneCRM

CREATE TABLE IF NOT EXISTS incidencias (
    id INT AUTO_INCREMENT PRIMARY KEY,
    asunto VARCHAR(200) NOT NULL,
    tipo_incidencia VARCHAR(50) NOT NULL,
    prioridad VARCHAR(20) NOT NULL,
    descripcion VARCHAR(2000) NOT NULL,
    usuario_id INT NOT NULL,
    nombre_usuario VARCHAR(100) NOT NULL,
    email_usuario VARCHAR(100) NOT NULL,
    estado VARCHAR(20) NOT NULL DEFAULT 'Pendiente',
    fecha_creacion DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    fecha_actualizacion DATETIME NULL,
    observaciones_admin VARCHAR(1000) NULL,
    tiene_imagen BOOLEAN NOT NULL DEFAULT FALSE,
    nombre_imagen VARCHAR(200) NULL,
    INDEX idx_usuario_id (usuario_id),
    INDEX idx_estado (estado),
    INDEX idx_prioridad (prioridad),
    INDEX idx_fecha_creacion (fecha_creacion)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
