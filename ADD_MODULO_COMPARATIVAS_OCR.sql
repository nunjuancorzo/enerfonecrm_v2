-- Agregar tablas para módulo de comparativas con OCR y PreCarga
-- Fecha: 2026-06-10

-- =====================================================
-- Tabla: historico_comparativas
-- Descripción: Almacena el historial de comparativas realizadas
-- =====================================================

DELIMITER //

CREATE PROCEDURE AddHistoricoComparativasTable()
BEGIN
    DECLARE CONTINUE HANDLER FOR SQLEXCEPTION BEGIN END;
    
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
        FOREIGN KEY (usuario_id) REFERENCES usuarios(id) ON DELETE SET NULL
    ) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
    
END //

DELIMITER ;

CALL AddHistoricoComparativasTable();
DROP PROCEDURE AddHistoricoComparativasTable;

-- =====================================================
-- Tabla: plantillas_precarga
-- Descripción: Almacena plantillas de mapeo visual de facturas por comercializadora
-- =====================================================

DELIMITER //

CREATE PROCEDURE AddPlantillasPreCargaTable()
BEGIN
    DECLARE CONTINUE HANDLER FOR SQLEXCEPTION BEGIN END;
    
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
        FOREIGN KEY (usuario_creador_id) REFERENCES usuarios(id) ON DELETE SET NULL
    ) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
    
END //

DELIMITER ;

CALL AddPlantillasPreCargaTable();
DROP PROCEDURE AddPlantillasPreCargaTable;

-- =====================================================
-- Configuración SMTP para envío de emails
-- (Ampliar tabla configuracion_empresa si no existe)
-- =====================================================

DELIMITER //

CREATE PROCEDURE AddSmtpConfigToConfiguracionEmpresa()
BEGIN
    DECLARE CONTINUE HANDLER FOR SQLEXCEPTION BEGIN END;
    
    -- Verificar y agregar campos SMTP
    ALTER TABLE configuracion_empresa ADD COLUMN smtp_servidor VARCHAR(255);
    ALTER TABLE configuracion_empresa ADD COLUMN smtp_puerto INT DEFAULT 587;
    ALTER TABLE configuracion_empresa ADD COLUMN smtp_usuario VARCHAR(255);
    ALTER TABLE configuracion_empresa ADD COLUMN smtp_password VARCHAR(500);
    ALTER TABLE configuracion_empresa ADD COLUMN smtp_ssl BOOLEAN DEFAULT TRUE;
    ALTER TABLE configuracion_empresa ADD COLUMN smtp_email_desde VARCHAR(255);
    ALTER TABLE configuracion_empresa ADD COLUMN smtp_nombre_desde VARCHAR(255);
    
END //

DELIMITER ;

CALL AddSmtpConfigToConfiguracionEmpresa();
DROP PROCEDURE AddSmtpConfigToConfiguracionEmpresa;

-- =====================================================
-- Agregar configuración para OCR
-- =====================================================

DELIMITER //

CREATE PROCEDURE AddOcrConfigToConfiguracionEmpresa()
BEGIN
    DECLARE CONTINUE HANDLER FOR SQLEXCEPTION BEGIN END;
    
    -- Configuración OCR
    ALTER TABLE configuracion_empresa ADD COLUMN ocr_proveedor VARCHAR(100) DEFAULT 'tesseract' COMMENT 'azure, openai, google, tesseract';
    ALTER TABLE configuracion_empresa ADD COLUMN ocr_api_key VARCHAR(500);
    ALTER TABLE configuracion_empresa ADD COLUMN ocr_endpoint VARCHAR(500);
    ALTER TABLE configuracion_empresa ADD COLUMN ocr_modelo VARCHAR(100);
    ALTER TABLE configuracion_empresa ADD COLUMN ocr_timeout INT DEFAULT 30;
    ALTER TABLE configuracion_empresa ADD COLUMN ocr_proveedor_secundario VARCHAR(100);
    ALTER TABLE configuracion_empresa ADD COLUMN ocr_fallback_automatico BOOLEAN DEFAULT TRUE;
    
END //

DELIMITER ;

CALL AddOcrConfigToConfiguracionEmpresa();
DROP PROCEDURE AddOcrConfigToConfiguracionEmpresa;

-- =====================================================
-- Configuración por defecto: Tesseract (gratuito)
-- =====================================================

UPDATE configuracion_empresa SET
    ocr_proveedor = 'tesseract',
    ocr_proveedor_secundario = NULL,
    ocr_fallback_automatico = TRUE
WHERE id = 1;

SELECT 'Tablas para módulo de comparativas con OCR y PreCarga creadas correctamente' AS Resultado;
