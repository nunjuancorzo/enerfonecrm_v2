-- =====================================================
-- Script: Crear Maestro de Códigos Postales
-- Descripción: Tabla maestra para gestionar códigos postales españoles
--              con ciudad y provincia asociada
-- Autor: Sistema CorCRM
-- Fecha: 2026-06-15
-- =====================================================

-- Crear tabla de códigos postales
CREATE TABLE IF NOT EXISTS codigos_postales (
    id INT AUTO_INCREMENT PRIMARY KEY,
    codigo_postal VARCHAR(5) NOT NULL,
    ciudad VARCHAR(100) NOT NULL,
    provincia VARCHAR(50) NOT NULL,
    activo BOOLEAN DEFAULT TRUE,
    fecha_creacion DATETIME DEFAULT CURRENT_TIMESTAMP,
    fecha_modificacion DATETIME DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    
    UNIQUE KEY unique_codigo_postal (codigo_postal),
    INDEX idx_ciudad (ciudad),
    INDEX idx_provincia (provincia),
    INDEX idx_activo (activo)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- Insertar códigos postales más comunes de España (ejemplo inicial)
INSERT INTO codigos_postales (codigo_postal, ciudad, provincia) VALUES
-- Madrid
('28001', 'Madrid', 'Madrid'),
('28002', 'Madrid', 'Madrid'),
('28003', 'Madrid', 'Madrid'),
('28004', 'Madrid', 'Madrid'),
('28005', 'Madrid', 'Madrid'),
('28006', 'Madrid', 'Madrid'),
('28007', 'Madrid', 'Madrid'),
('28008', 'Madrid', 'Madrid'),
('28009', 'Madrid', 'Madrid'),
('28010', 'Madrid', 'Madrid'),
('28015', 'Madrid', 'Madrid'),
('28020', 'Madrid', 'Madrid'),
('28034', 'Madrid', 'Madrid'),
('28050', 'Madrid', 'Madrid'),

-- Barcelona
('08001', 'Barcelona', 'Barcelona'),
('08002', 'Barcelona', 'Barcelona'),
('08003', 'Barcelona', 'Barcelona'),
('08004', 'Barcelona', 'Barcelona'),
('08005', 'Barcelona', 'Barcelona'),
('08006', 'Barcelona', 'Barcelona'),
('08007', 'Barcelona', 'Barcelona'),
('08008', 'Barcelona', 'Barcelona'),
('08009', 'Barcelona', 'Barcelona'),
('08010', 'Barcelona', 'Barcelona'),

-- Valencia
('46001', 'Valencia', 'Valencia'),
('46002', 'Valencia', 'Valencia'),
('46003', 'Valencia', 'Valencia'),
('46004', 'Valencia', 'Valencia'),
('46005', 'Valencia', 'Valencia'),
('46010', 'Valencia', 'Valencia'),
('46015', 'Valencia', 'Valencia'),
('46020', 'Valencia', 'Valencia'),

-- Sevilla
('41001', 'Sevilla', 'Sevilla'),
('41002', 'Sevilla', 'Sevilla'),
('41003', 'Sevilla', 'Sevilla'),
('41004', 'Sevilla', 'Sevilla'),
('41005', 'Sevilla', 'Sevilla'),
('41010', 'Sevilla', 'Sevilla'),

-- Zaragoza
('50001', 'Zaragoza', 'Zaragoza'),
('50002', 'Zaragoza', 'Zaragoza'),
('50003', 'Zaragoza', 'Zaragoza'),
('50004', 'Zaragoza', 'Zaragoza'),
('50005', 'Zaragoza', 'Zaragoza'),

-- Málaga
('29001', 'Málaga', 'Málaga'),
('29002', 'Málaga', 'Málaga'),
('29003', 'Málaga', 'Málaga'),
('29004', 'Málaga', 'Málaga'),
('29005', 'Málaga', 'Málaga'),

-- Bilbao
('48001', 'Bilbao', 'Vizcaya'),
('48002', 'Bilbao', 'Vizcaya'),
('48003', 'Bilbao', 'Vizcaya'),
('48004', 'Bilbao', 'Vizcaya'),
('48005', 'Bilbao', 'Vizcaya')

ON DUPLICATE KEY UPDATE
    ciudad = VALUES(ciudad),
    provincia = VALUES(provincia);

-- =====================================================
-- Verificación
-- =====================================================
SELECT 
    COUNT(*) as total_codigos_postales,
    COUNT(DISTINCT provincia) as total_provincias
FROM codigos_postales;

SELECT 'Tabla codigos_postales creada correctamente' AS resultado;
