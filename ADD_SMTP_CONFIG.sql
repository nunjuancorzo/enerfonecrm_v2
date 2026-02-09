-- Script para añadir configuración SMTP a la tabla configuracion_empresa
-- Fecha: 6 de febrero de 2026

USE enerfonecrm;

-- Añadir columnas de configuración SMTP
ALTER TABLE configuracion_empresa 
ADD COLUMN smtp_servidor VARCHAR(255) NULL COMMENT 'Servidor SMTP para envío de emails',
ADD COLUMN smtp_puerto INT NULL DEFAULT 587 COMMENT 'Puerto del servidor SMTP',
ADD COLUMN smtp_usuario VARCHAR(255) NULL COMMENT 'Usuario para autenticación SMTP',
ADD COLUMN smtp_password VARCHAR(255) NULL COMMENT 'Contraseña para autenticación SMTP',
ADD COLUMN smtp_usar_ssl BOOLEAN DEFAULT TRUE COMMENT 'Indica si se debe usar SSL/TLS',
ADD COLUMN smtp_email_desde VARCHAR(255) NULL COMMENT 'Email que aparecerá como remitente',
ADD COLUMN smtp_nombre_desde VARCHAR(255) NULL COMMENT 'Nombre que aparecerá como remitente';

-- Verificar las columnas añadidas
SELECT COLUMN_NAME, DATA_TYPE, IS_NULLABLE, COLUMN_DEFAULT, COLUMN_COMMENT
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_SCHEMA = 'enerfonecrm'
  AND TABLE_NAME = 'configuracion_empresa'
  AND COLUMN_NAME LIKE 'smtp_%'
ORDER BY ORDINAL_POSITION;
