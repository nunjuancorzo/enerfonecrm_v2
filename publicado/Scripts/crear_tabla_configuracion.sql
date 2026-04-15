-- Script para crear la tabla de configuración de empresa
USE enerfonecrm;

CREATE TABLE IF NOT EXISTS `configuracion_empresa` (
  `id` INT NOT NULL AUTO_INCREMENT,
  `nombre_empresa` VARCHAR(255) NOT NULL,
  `cif` VARCHAR(20) NULL,
  `direccion` VARCHAR(255) NULL,
  `codigo_postal` VARCHAR(10) NULL,
  `ciudad` VARCHAR(100) NULL,
  `provincia` VARCHAR(100) NULL,
  `pais` VARCHAR(100) NULL DEFAULT 'España',
  `telefono` VARCHAR(20) NULL,
  `email` VARCHAR(100) NULL,
  `web` VARCHAR(255) NULL,
  `logo_url` VARCHAR(500) NULL,
  `fecha_actualizacion` TIMESTAMP NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
  PRIMARY KEY (`id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- Insertar registro inicial (solo si no existe)
INSERT INTO `configuracion_empresa` 
(`nombre_empresa`, `cif`, `direccion`, `ciudad`, `provincia`, `telefono`, `email`)
SELECT 'Enerfone', '', '', '', '', '', ''
WHERE NOT EXISTS (SELECT 1 FROM `configuracion_empresa` LIMIT 1);
