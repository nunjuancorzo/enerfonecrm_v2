-- Script para crear la tabla empresas_alarmas
-- Fecha de creación: 19/01/2026
-- Descripción: Maestro de empresas de alarmas, similar a comercializadoras y operadoras

CREATE TABLE IF NOT EXISTS empresas_alarmas (
    id INT AUTO_INCREMENT PRIMARY KEY,
    nombre VARCHAR(255) NOT NULL,
    logo_archivo VARCHAR(255) NULL,
    logo_contenido LONGBLOB NULL,
    activo TINYINT(1) NOT NULL DEFAULT 1,
    fecha_creacion DATETIME NULL,
    CONSTRAINT uk_empresas_alarmas_nombre UNIQUE (nombre)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- Índices
CREATE INDEX idx_empresas_alarmas_activo ON empresas_alarmas(activo);
CREATE INDEX idx_empresas_alarmas_nombre ON empresas_alarmas(nombre);

-- Datos de ejemplo (opcional)
-- INSERT INTO empresas_alarmas (nombre, activo, fecha_creacion) VALUES
-- ('Securitas Direct', 1, NOW()),
-- ('Prosegur', 1, NOW()),
-- ('Tyco', 1, NOW()),
-- ('ADT', 1, NOW());
