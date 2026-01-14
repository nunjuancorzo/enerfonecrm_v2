-- Script para crear la tabla de observaciones de contratos
-- Este script debe ejecutarse en la base de datos MySQL

CREATE TABLE IF NOT EXISTS `observaciones_contratos` (
  `id` INT NOT NULL AUTO_INCREMENT,
  `id_contrato` INT NOT NULL,
  `observacion` TEXT NOT NULL,
  `usuario` VARCHAR(100) NOT NULL,
  `fecha_hora` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
  PRIMARY KEY (`id`),
  INDEX `idx_id_contrato` (`id_contrato`),
  INDEX `idx_fecha_hora` (`fecha_hora` DESC),
  CONSTRAINT `fk_observaciones_contratos_contrato`
    FOREIGN KEY (`id_contrato`)
    REFERENCES `contratos` (`id`)
    ON DELETE CASCADE
    ON UPDATE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

-- Comentarios sobre la tabla
ALTER TABLE `observaciones_contratos` 
  COMMENT = 'Histórico de observaciones realizadas en los contratos';

-- Migrar observaciones existentes del campo observaciones_estado al histórico
INSERT INTO `observaciones_contratos` (`id_contrato`, `observacion`, `usuario`, `fecha_hora`)
SELECT 
  `id`,
  `observaciones_estado`,
  COALESCE(`comercial`, 'Sistema'),
  COALESCE(`fecha_creacion`, NOW())
FROM `contratos`
WHERE `observaciones_estado` IS NOT NULL 
  AND TRIM(`observaciones_estado`) != ''
  AND NOT EXISTS (
    SELECT 1 FROM `observaciones_contratos` 
    WHERE `id_contrato` = `contratos`.`id` 
      AND `observacion` COLLATE utf8mb4_general_ci = `contratos`.`observaciones_estado` COLLATE utf8mb4_general_ci
  );
