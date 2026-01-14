-- Script para crear la tabla operadoras
-- Ejecutar este script en la base de datos

CREATE TABLE IF NOT EXISTS operadoras (
    id INT AUTO_INCREMENT PRIMARY KEY,
    nombre VARCHAR(100) NOT NULL UNIQUE,
    logo_archivo VARCHAR(255) NULL,
    logo_contenido LONGBLOB NULL,
    activo TINYINT(1) DEFAULT 1,
    fecha_creacion DATETIME DEFAULT CURRENT_TIMESTAMP,
    INDEX idx_nombre (nombre),
    INDEX idx_activo (activo)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- Insertar operadoras principales si no existen
INSERT IGNORE INTO operadoras (nombre, activo, fecha_creacion)
VALUES 
    ('O2', 1, NOW()),
    ('Lowi', 1, NOW()),
    ('Symio', 1, NOW()),
    ('Jazztel', 1, NOW()),
    ('Masmovil', 1, NOW()),
    ('Pepephone', 1, NOW()),
    ('Vodafone', 1, NOW());
