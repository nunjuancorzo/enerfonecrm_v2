-- Script para crear la tabla mensajes_bienvenida
-- Autor: Sistema EnerfoneCRM
-- Fecha: 13/01/2026
-- Descripción: Tabla para almacenar mensajes de bienvenida que se muestran a los usuarios al iniciar la aplicación

CREATE TABLE IF NOT EXISTS mensajes_bienvenida (
    id INT AUTO_INCREMENT PRIMARY KEY,
    titulo VARCHAR(255) NOT NULL,
    contenido TEXT NOT NULL,
    imagen_url VARCHAR(500),
    fecha_inicio DATETIME NOT NULL,
    fecha_fin DATETIME,
    activo BOOLEAN DEFAULT TRUE,
    prioridad INT DEFAULT 0,
    fecha_creacion DATETIME DEFAULT CURRENT_TIMESTAMP,
    usuario_creacion_id INT,
    FOREIGN KEY (usuario_creacion_id) REFERENCES usuarios(idusuarios)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- Índices para mejorar el rendimiento
CREATE INDEX idx_mensajes_bienvenida_activo ON mensajes_bienvenida(activo);
CREATE INDEX idx_mensajes_bienvenida_fecha_inicio ON mensajes_bienvenida(fecha_inicio);
CREATE INDEX idx_mensajes_bienvenida_fecha_fin ON mensajes_bienvenida(fecha_fin);
CREATE INDEX idx_mensajes_bienvenida_prioridad ON mensajes_bienvenida(prioridad);

-- Insertar un mensaje de ejemplo
INSERT INTO mensajes_bienvenida (titulo, contenido, fecha_inicio, activo, prioridad) 
VALUES (
    '¡Bienvenido a EnerfoneCRM!',
    'Sistema de gestión de clientes y contratos. Revisa las novedades y mejoras en esta nueva versión.',
    NOW(),
    TRUE,
    1
);
