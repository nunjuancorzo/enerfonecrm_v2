-- Script para crear la tabla de comercializadoras y poblarla con datos iniciales
-- Ejecutar este script en la base de datos de producción

CREATE TABLE IF NOT EXISTS comercializadoras (
    id INT AUTO_INCREMENT PRIMARY KEY,
    nombre VARCHAR(255) NOT NULL UNIQUE,
    logo_archivo VARCHAR(255) NULL,
    logo_contenido LONGBLOB NULL,
    activo BOOLEAN DEFAULT TRUE,
    fecha_creacion DATETIME NULL,
    INDEX idx_nombre (nombre),
    INDEX idx_activo (activo)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- Insertar comercializadoras existentes
INSERT INTO comercializadoras (nombre, activo, fecha_creacion) VALUES
('Iberdrola', TRUE, NOW()),
('Endesa', TRUE, NOW()),
('Naturgy', TRUE, NOW()),
('TotalEnergies', TRUE, NOW()),
('Repsol', TRUE, NOW()),
('Podo', TRUE, NOW()),
('Ignis', TRUE, NOW()),
('Audax', TRUE, NOW()),
('Factorenergia', TRUE, NOW()),
('Holaluz', TRUE, NOW()),
('Conecta', TRUE, NOW()),
('Enara', TRUE, NOW()),
('Lucera', TRUE, NOW()),
('Som Energia', TRUE, NOW()),
('Curenergía', TRUE, NOW()),
('EDP', TRUE, NOW()),
('Gesternova', TRUE, NOW()),
('Octopus Energy', TRUE, NOW()),
('Nexus Energía', TRUE, NOW()),
('Energía XXI', TRUE, NOW()),
('Alcanzia', TRUE, NOW()),
('Cide', TRUE, NOW());
