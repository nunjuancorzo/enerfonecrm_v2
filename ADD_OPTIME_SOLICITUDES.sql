-- Módulo Optime: fichas de alta de PdV enviadas desde el formulario público (/optime/formulario)
CREATE TABLE IF NOT EXISTS optime_solicitudes (
    id INT AUTO_INCREMENT PRIMARY KEY,
    razon_social VARCHAR(200) NOT NULL,
    cif_nif VARCHAR(20) NOT NULL,
    direccion_fiscal VARCHAR(250) NOT NULL,
    poblacion VARCHAR(100) NOT NULL,
    provincia VARCHAR(100) NOT NULL,
    codigo_postal VARCHAR(10) NOT NULL,
    persona_contacto VARCHAR(150) NOT NULL,
    nif_persona_contacto VARCHAR(20) NOT NULL,
    telefono VARCHAR(20) NOT NULL,
    email VARCHAR(150) NOT NULL,
    cnae VARCHAR(10) NULL,
    iban VARCHAR(4) NULL,
    ccc VARCHAR(20) NULL,
    swift VARCHAR(11) NULL,
    titular_cuenta VARCHAR(200) NULL,
    interes_lowi TINYINT(1) NOT NULL DEFAULT 0,
    interes_vodafone TINYINT(1) NOT NULL DEFAULT 0,
    interes_rentik TINYINT(1) NOT NULL DEFAULT 0,
    interes_3d_seguridad TINYINT(1) NOT NULL DEFAULT 0,
    acepta_codigo_buenas_practicas TINYINT(1) NOT NULL DEFAULT 0,
    acepta_adhesion_pdv TINYINT(1) NOT NULL DEFAULT 0,
    fecha_firma DATE NOT NULL,
    fecha_creacion DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    fecha_modificacion DATETIME NULL,
    INDEX idx_optime_cif_nif (cif_nif),
    INDEX idx_optime_fecha_creacion (fecha_creacion)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- Documentos adjuntos (copias CIF/Escrituras/DNI) y firma
CREATE TABLE IF NOT EXISTS optime_documentos (
    id INT AUTO_INCREMENT PRIMARY KEY,
    solicitud_id INT NOT NULL,
    tipo VARCHAR(50) NOT NULL,
    nombre_archivo VARCHAR(255) NOT NULL,
    content_type VARCHAR(100) NOT NULL,
    contenido LONGBLOB NOT NULL,
    fecha_creacion DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    CONSTRAINT fk_optime_documentos_solicitud
        FOREIGN KEY (solicitud_id) REFERENCES optime_solicitudes(id) ON DELETE CASCADE,
    INDEX idx_optime_documentos_solicitud (solicitud_id)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- Documentos enviados a firma tras el alta (Contrato, Adenda y Código de Buenas Prácticas)
CREATE TABLE IF NOT EXISTS optime_firmas (
    id INT AUTO_INCREMENT PRIMARY KEY,
    solicitud_id INT NOT NULL,
    tipo_documento VARCHAR(20) NOT NULL,
    proceso_id VARCHAR(36) NOT NULL,
    token_hash CHAR(64) NOT NULL,
    fecha_creacion_utc DATETIME NOT NULL,
    fecha_caducidad_utc DATETIME NOT NULL,
    fecha_firma_utc DATETIME NULL,
    estado VARCHAR(30) NOT NULL,
    email_destinatario VARCHAR(255) NOT NULL,
    nombre_destinatario VARCHAR(255) NULL,
    documento_original LONGBLOB NOT NULL,
    documento_firmado LONGBLOB NULL,
    firma_imagen MEDIUMBLOB NULL,
    hash_documento_original CHAR(64) NULL,
    hash_documento_firmado CHAR(64) NULL,
    ip_firma VARCHAR(64) NULL,
    user_agent_firma VARCHAR(1000) NULL,
    error_envio VARCHAR(500) NULL,
    CONSTRAINT fk_optime_firmas_solicitud
        FOREIGN KEY (solicitud_id) REFERENCES optime_solicitudes(id) ON DELETE CASCADE,
    UNIQUE INDEX ux_optime_firmas_token (token_hash),
    INDEX idx_optime_firmas_solicitud (solicitud_id, tipo_documento)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
