-- Crea tabla de histórico de consultas SIPS (para reutilizar respuestas y ahorrar llamadas a la API)
-- Ejecutar en las BDs: enerfone_pre y demoenerfone (si aplica)

CREATE TABLE IF NOT EXISTS `historico_sips_consultas` (
  `id` INT NOT NULL AUTO_INCREMENT,
  `cups` VARCHAR(32) NOT NULL,
  `usuario_id` INT NULL,
  `usuario_nombre` VARCHAR(100) NULL,
  `usuario_email` VARCHAR(255) NULL,
  `fecha_consulta` DATETIME NOT NULL,
  `success` TINYINT(1) NOT NULL DEFAULT 1,
  `http_status_code` INT NULL,
  `error_message` VARCHAR(500) NULL,
  `response_json` LONGTEXT NULL,
  `response_size` INT NULL,
  PRIMARY KEY (`id`),
  INDEX `idx_historico_sips_cups_fecha` (`cups`, `fecha_consulta`),
  INDEX `idx_historico_sips_fecha` (`fecha_consulta`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;
