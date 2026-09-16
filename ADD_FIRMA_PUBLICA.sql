CREATE TABLE IF NOT EXISTS solicitudes_firma (
    id INT NOT NULL AUTO_INCREMENT,
    contrato_id INT NOT NULL,
    proceso_id VARCHAR(36) NOT NULL,
    token_hash CHAR(64) NOT NULL,
    fecha_creacion_utc DATETIME NOT NULL,
    fecha_caducidad_utc DATETIME NOT NULL,
    fecha_aceptacion_utc DATETIME NULL,
    fecha_firma_utc DATETIME NULL,
    estado VARCHAR(30) NOT NULL,
    email_destinatario VARCHAR(255) NOT NULL,
    nombre_destinatario VARCHAR(255) NULL,
    documento_original_id INT NOT NULL,
    documento_firmado_id INT NULL,
    firma_imagen LONGBLOB NULL,
    hash_documento_original CHAR(64) NULL,
    hash_documento_firmado CHAR(64) NULL,
    ip_firma VARCHAR(64) NULL,
    user_agent_firma VARCHAR(1000) NULL,
    PRIMARY KEY (id),
    UNIQUE KEY uk_solicitudes_firma_token_hash (token_hash),
    KEY ix_solicitudes_firma_contrato_estado (contrato_id, estado),
    CONSTRAINT fk_solicitudes_firma_contrato FOREIGN KEY (contrato_id) REFERENCES contratos(id)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

CREATE TABLE IF NOT EXISTS solicitudes_firma_eventos (
    id INT NOT NULL AUTO_INCREMENT,
    solicitud_firma_id INT NOT NULL,
    tipo_evento VARCHAR(50) NOT NULL,
    fecha_hora_utc DATETIME NOT NULL,
    ip VARCHAR(64) NULL,
    user_agent VARCHAR(1000) NULL,
    documento_id INT NULL,
    resultado VARCHAR(50) NULL,
    datos TEXT NULL,
    PRIMARY KEY (id),
    KEY ix_solicitudes_firma_eventos_solicitud (solicitud_firma_id),
    CONSTRAINT fk_solicitudes_firma_eventos_solicitud FOREIGN KEY (solicitud_firma_id) REFERENCES solicitudes_firma(id) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;