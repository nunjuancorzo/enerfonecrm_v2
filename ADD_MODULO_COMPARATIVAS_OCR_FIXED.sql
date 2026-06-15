-- Agregar tablas para módulo de comparativas con OCR y PreCarga
-- Fecha: 2026-06-12
-- VERSIÓN CORREGIDA - Sin silenciar errores

-- =====================================================
-- Tabla: historico_comparativas
-- Descripción: Almacena el historial de comparativas realizadas
-- =====================================================

CREATE TABLE IF NOT EXISTS historico_comparativas (
    id INT AUTO_INCREMENT PRIMARY KEY,
    fecha_comparativa DATETIME NOT NULL,
    origen VARCHAR(50) NOT NULL COMMENT 'frontend o backend',
    email_cliente VARCHAR(255),
    tipo_energia VARCHAR(50) NOT NULL COMMENT 'LUZ, GAS o LUZ+GAS',
    cups VARCHAR(255),
    comercializadora_actual VARCHAR(255),
    tarifa_actual VARCHAR(255),
    total_factura_actual DECIMAL(10,2),
    mejor_tarifa_id INT,
    mejor_tarifa_nombre VARCHAR(255),
    mejor_tarifa_empresa VARCHAR(255),
    ahorro_mensual DECIMAL(10,2),
    ahorro_anual DECIMAL(10,2),
    porcentaje_ahorro DECIMAL(5,2),
    datos_utilizados TEXT COMMENT 'JSON con los datos usados',
    resultado_ranking TEXT COMMENT 'JSON con el ranking completo',
    proveedor_ocr VARCHAR(100),
    email_enviado BOOLEAN DEFAULT FALSE,
    fecha_envio_email DATETIME,
    advertencias TEXT,
    usuario_id INT,
    nombre_archivo_factura VARCHAR(500),
    INDEX idx_fecha_comparativa (fecha_comparativa),
    INDEX idx_origen (origen),
    INDEX idx_email_cliente (email_cliente),
    INDEX idx_usuario_id (usuario_id),
    FOREIGN KEY (usuario_id) REFERENCES usuarios(idusuarios) ON DELETE SET NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

SELECT 'Tabla historico_comparativas creada/verificada correctamente' AS Resultado;

-- =====================================================
-- Tabla: plantillas_precarga
-- Descripción: Almacena plantillas de mapeo visual de facturas por comercializadora
-- =====================================================

CREATE TABLE IF NOT EXISTS plantillas_precarga (
    id INT AUTO_INCREMENT PRIMARY KEY,
    nombre VARCHAR(255) NOT NULL,
    comercializadora VARCHAR(255) NOT NULL,
    alias_comercializadora VARCHAR(500) COMMENT 'Alias separados por comas',
    tipo_energia VARCHAR(50) NOT NULL COMMENT 'LUZ, GAS o LUZ+GAS',
    variante_factura VARCHAR(255),
    prioridad INT DEFAULT 0,
    activa BOOLEAN DEFAULT TRUE,
    campos_mapeados TEXT COMMENT 'JSON con los campos mapeados',
    notas_internas TEXT,
    archivo_factura_ejemplo VARCHAR(500),
    fecha_creacion DATETIME DEFAULT CURRENT_TIMESTAMP,
    fecha_modificacion DATETIME,
    usuario_creador_id INT,
    INDEX idx_comercializadora (comercializadora),
    INDEX idx_tipo_energia (tipo_energia),
    INDEX idx_activa (activa),
    INDEX idx_prioridad (prioridad),
    FOREIGN KEY (usuario_creador_id) REFERENCES usuarios(idusuarios) ON DELETE SET NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

SELECT 'Tabla plantillas_precarga creada/verificada correctamente' AS Resultado;

-- =====================================================
-- Configuración SMTP para envío de emails
-- Agregar columnas a configuracion_empresa solo si no existen
-- =====================================================

DELIMITER //

CREATE PROCEDURE AddSmtpConfigIfNotExists()
BEGIN
    -- smtp_servidor
    IF NOT EXISTS (SELECT * FROM information_schema.COLUMNS 
                   WHERE TABLE_SCHEMA = DATABASE() 
                   AND TABLE_NAME = 'configuracion_empresa' 
                   AND COLUMN_NAME = 'smtp_servidor') THEN
        ALTER TABLE configuracion_empresa ADD COLUMN smtp_servidor VARCHAR(255);
    END IF;
    
    -- smtp_puerto
    IF NOT EXISTS (SELECT * FROM information_schema.COLUMNS 
                   WHERE TABLE_SCHEMA = DATABASE() 
                   AND TABLE_NAME = 'configuracion_empresa' 
                   AND COLUMN_NAME = 'smtp_puerto') THEN
        ALTER TABLE configuracion_empresa ADD COLUMN smtp_puerto INT DEFAULT 587;
    END IF;
    
    -- smtp_usuario
    IF NOT EXISTS (SELECT * FROM information_schema.COLUMNS 
                   WHERE TABLE_SCHEMA = DATABASE() 
                   AND TABLE_NAME = 'configuracion_empresa' 
                   AND COLUMN_NAME = 'smtp_usuario') THEN
        ALTER TABLE configuracion_empresa ADD COLUMN smtp_usuario VARCHAR(255);
    END IF;
    
    -- smtp_password
    IF NOT EXISTS (SELECT * FROM information_schema.COLUMNS 
                   WHERE TABLE_SCHEMA = DATABASE() 
                   AND TABLE_NAME = 'configuracion_empresa' 
                   AND COLUMN_NAME = 'smtp_password') THEN
        ALTER TABLE configuracion_empresa ADD COLUMN smtp_password VARCHAR(500);
    END IF;
    
    -- smtp_ssl
    IF NOT EXISTS (SELECT * FROM information_schema.COLUMNS 
                   WHERE TABLE_SCHEMA = DATABASE() 
                   AND TABLE_NAME = 'configuracion_empresa' 
                   AND COLUMN_NAME = 'smtp_ssl') THEN
        ALTER TABLE configuracion_empresa ADD COLUMN smtp_ssl BOOLEAN DEFAULT TRUE;
    END IF;
    
    -- smtp_email_desde
    IF NOT EXISTS (SELECT * FROM information_schema.COLUMNS 
                   WHERE TABLE_SCHEMA = DATABASE() 
                   AND TABLE_NAME = 'configuracion_empresa' 
                   AND COLUMN_NAME = 'smtp_email_desde') THEN
        ALTER TABLE configuracion_empresa ADD COLUMN smtp_email_desde VARCHAR(255);
    END IF;
    
    -- smtp_nombre_desde
    IF NOT EXISTS (SELECT * FROM information_schema.COLUMNS 
                   WHERE TABLE_SCHEMA = DATABASE() 
                   AND TABLE_NAME = 'configuracion_empresa' 
                   AND COLUMN_NAME = 'smtp_nombre_desde') THEN
        ALTER TABLE configuracion_empresa ADD COLUMN smtp_nombre_desde VARCHAR(255);
    END IF;
END//

DELIMITER ;

CALL AddSmtpConfigIfNotExists();
DROP PROCEDURE AddSmtpConfigIfNotExists;

SELECT 'Configuración SMTP agregada correctamente' AS Resultado;

-- =====================================================
-- Agregar configuración para OCR
-- =====================================================

DELIMITER //

CREATE PROCEDURE AddOcrConfigIfNotExists()
BEGIN
    -- ocr_proveedor
    IF NOT EXISTS (SELECT * FROM information_schema.COLUMNS 
                   WHERE TABLE_SCHEMA = DATABASE() 
                   AND TABLE_NAME = 'configuracion_empresa' 
                   AND COLUMN_NAME = 'ocr_proveedor') THEN
        ALTER TABLE configuracion_empresa ADD COLUMN ocr_proveedor VARCHAR(100) DEFAULT 'tesseract' COMMENT 'azure, openai, google, tesseract';
    END IF;
    
    -- ocr_api_key
    IF NOT EXISTS (SELECT * FROM information_schema.COLUMNS 
                   WHERE TABLE_SCHEMA = DATABASE() 
                   AND TABLE_NAME = 'configuracion_empresa' 
                   AND COLUMN_NAME = 'ocr_api_key') THEN
        ALTER TABLE configuracion_empresa ADD COLUMN ocr_api_key VARCHAR(500);
    END IF;
    
    -- ocr_endpoint
    IF NOT EXISTS (SELECT * FROM information_schema.COLUMNS 
                   WHERE TABLE_SCHEMA = DATABASE() 
                   AND TABLE_NAME = 'configuracion_empresa' 
                   AND COLUMN_NAME = 'ocr_endpoint') THEN
        ALTER TABLE configuracion_empresa ADD COLUMN ocr_endpoint VARCHAR(500);
    END IF;
    
    -- ocr_modelo
    IF NOT EXISTS (SELECT * FROM information_schema.COLUMNS 
                   WHERE TABLE_SCHEMA = DATABASE() 
                   AND TABLE_NAME = 'configuracion_empresa' 
                   AND COLUMN_NAME = 'ocr_modelo') THEN
        ALTER TABLE configuracion_empresa ADD COLUMN ocr_modelo VARCHAR(100);
    END IF;
    
    -- ocr_timeout
    IF NOT EXISTS (SELECT * FROM information_schema.COLUMNS 
                   WHERE TABLE_SCHEMA = DATABASE() 
                   AND TABLE_NAME = 'configuracion_empresa' 
                   AND COLUMN_NAME = 'ocr_timeout') THEN
        ALTER TABLE configuracion_empresa ADD COLUMN ocr_timeout INT DEFAULT 30;
    END IF;
    
    -- ocr_proveedor_secundario
    IF NOT EXISTS (SELECT * FROM information_schema.COLUMNS 
                   WHERE TABLE_SCHEMA = DATABASE() 
                   AND TABLE_NAME = 'configuracion_empresa' 
                   AND COLUMN_NAME = 'ocr_proveedor_secundario') THEN
        ALTER TABLE configuracion_empresa ADD COLUMN ocr_proveedor_secundario VARCHAR(100);
    END IF;
    
    -- ocr_fallback_automatico
    IF NOT EXISTS (SELECT * FROM information_schema.COLUMNS 
                   WHERE TABLE_SCHEMA = DATABASE() 
                   AND TABLE_NAME = 'configuracion_empresa' 
                   AND COLUMN_NAME = 'ocr_fallback_automatico') THEN
        ALTER TABLE configuracion_empresa ADD COLUMN ocr_fallback_automatico BOOLEAN DEFAULT TRUE;
    END IF;
END//

DELIMITER ;

CALL AddOcrConfigIfNotExists();
DROP PROCEDURE AddOcrConfigIfNotExists;

SELECT 'Configuración OCR agregada correctamente' AS Resultado;

-- =====================================================
-- Configuración por defecto: Tesseract (gratuito)
-- =====================================================

UPDATE configuracion_empresa SET
    ocr_proveedor = 'tesseract',
    ocr_proveedor_secundario = NULL,
    ocr_fallback_automatico = TRUE
WHERE id = 1;

SELECT '✅ Script completado exitosamente - Todas las tablas y configuraciones han sido creadas/actualizadas' AS Resultado;
