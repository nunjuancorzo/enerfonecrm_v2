-- Elimina de tarifasluz los campos que ya no forman parte de la ficha de tarifas.
-- Revisar/respaldar la tabla antes de ejecutar: la eliminacion es destructiva.

DELIMITER //

CREATE PROCEDURE EliminarColumnasExtraTarifasLuz()
BEGIN
    DECLARE columnas_a_eliminar TEXT;

    SELECT GROUP_CONCAT(
        CONCAT('DROP COLUMN `', COLUMN_NAME, '`')
        ORDER BY ORDINAL_POSITION
        SEPARATOR ', '
    )
    INTO columnas_a_eliminar
    FROM information_schema.COLUMNS
    WHERE TABLE_SCHEMA = DATABASE()
      AND TABLE_NAME = 'tarifasluz'
      AND COLUMN_NAME IN (
          'termino_fijo_diario',
          'precio_potencia_p1',
          'precio_potencia_p2',
          'precio_potencia_p3',
          'precio_energia_p1',
          'precio_energia_p2',
          'precio_energia_p3',
          'precioNew',
          'fecha_carga'
      );

    IF columnas_a_eliminar IS NOT NULL THEN
        SET @sql_eliminar_columnas = CONCAT('ALTER TABLE `tarifasluz` ', columnas_a_eliminar);
        PREPARE sentencia_eliminar_columnas FROM @sql_eliminar_columnas;
        EXECUTE sentencia_eliminar_columnas;
        DEALLOCATE PREPARE sentencia_eliminar_columnas;
    END IF;
END//

DELIMITER ;

CALL EliminarColumnasExtraTarifasLuz();
DROP PROCEDURE EliminarColumnasExtraTarifasLuz;

-- Columnas finales esperadas en tarifasluz:
-- id, empresa, tipo_cliente, nombre, peaje, comision,
-- potencia1..potencia6, energia1..energia6, descuento,
-- observaciones_descuentos, permanencia, excedentes, bateria_virtual,
-- dias_penalizacion, tipo_penalizacion, activa.
