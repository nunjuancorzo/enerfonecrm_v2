-- =====================================================
-- Separación de tarifas de energía en LUZ y GAS
-- Paso 2: traspaso de datos desde tarifasenergia
--
-- Requisito previo: ejecutar ADD_TABLAS_TARIFASLUZ_TARIFASGAS.sql
--
-- Solo se migran las tarifas con tipo = 'LUZ' o tipo = 'GAS'.
-- Las tarifas 'LUZ+GAS' se ignoran deliberadamente.
-- Se conserva el id original para poder mapear contratos existentes.
-- El script es re-ejecutable: no duplica filas ya migradas.
-- =====================================================

-- -----------------------------------------------------
-- 1. TARIFAS DE LUZ
-- -----------------------------------------------------
INSERT INTO tarifasluz (
    id, empresa, tipo_cliente, nombre, peaje,
    potencia1, potencia2, potencia3, potencia4, potencia5, potencia6,
    energia1, energia2, energia3, energia4, energia5, energia6,
    termino_fijo_diario,
    precio_potencia_p1, precio_potencia_p2, precio_potencia_p3,
    precio_energia_p1, precio_energia_p2, precio_energia_p3,
    descuento, observaciones_descuentos, excedentes, bateria_virtual,
    permanencia, dias_penalizacion, tipo_penalizacion,
    comision, precioNew, activa, fecha_carga
)
SELECT
    t.id,
    t.empresa,
    COALESCE(NULLIF(TRIM(t.tipo_cliente), ''), 'Residencial'),
    t.nombre,
    t.peaje,
    t.potencia1, t.potencia2, t.potencia3, t.potencia4, t.potencia5, t.potencia6,
    t.energia1, t.energia2, t.energia3, t.energia4, t.energia5, t.energia6,
    t.termino_fijo_diario,
    t.precio_potencia_p1, t.precio_potencia_p2, t.precio_potencia_p3,
    t.precio_energia_p1, t.precio_energia_p2, t.precio_energia_p3,
    t.descuento, t.observaciones_descuentos, t.excedentes, t.bateria_virtual,
    t.permanencia, t.dias_penalizacion, t.tipo_penalizacion,
    COALESCE(t.comision, 0), COALESCE(t.precioNew, 0), t.activa, t.fecha_carga
FROM tarifasenergia t
WHERE UPPER(TRIM(t.tipo)) = 'LUZ'
  AND NOT EXISTS (SELECT 1 FROM tarifasluz l WHERE l.id = t.id);

-- -----------------------------------------------------
-- 2. TARIFAS DE GAS
-- -----------------------------------------------------
INSERT INTO tarifasgas (
    id, empresa, tipo_cliente, nombre, peaje_gas,
    termino_fijo_gas, termino_variable_gas, pvd_sva,
    descuento, observaciones_descuentos,
    permanencia, dias_penalizacion, tipo_penalizacion,
    comision, precioNew, activa, fecha_carga
)
SELECT
    t.id,
    t.empresa,
    COALESCE(NULLIF(TRIM(t.tipo_cliente), ''), 'Residencial'),
    t.nombre,
    t.peaje_gas,
    t.termino_fijo_gas, t.termino_variable_gas, t.pvd_sva,
    t.descuento, t.observaciones_descuentos,
    t.permanencia, t.dias_penalizacion, t.tipo_penalizacion,
    COALESCE(t.comision, 0), COALESCE(t.precioNew, 0), t.activa, t.fecha_carga
FROM tarifasenergia t
WHERE UPPER(TRIM(t.tipo)) = 'GAS'
  AND NOT EXISTS (SELECT 1 FROM tarifasgas g WHERE g.id = t.id);

-- -----------------------------------------------------
-- 3. Reajuste del AUTO_INCREMENT tras insertar ids explícitos
-- -----------------------------------------------------
SET @sql_luz = CONCAT('ALTER TABLE tarifasluz AUTO_INCREMENT = ',
                      (SELECT COALESCE(MAX(id), 0) + 1 FROM tarifasluz));
PREPARE stmt FROM @sql_luz; EXECUTE stmt; DEALLOCATE PREPARE stmt;

SET @sql_gas = CONCAT('ALTER TABLE tarifasgas AUTO_INCREMENT = ',
                      (SELECT COALESCE(MAX(id), 0) + 1 FROM tarifasgas));
PREPARE stmt FROM @sql_gas; EXECUTE stmt; DEALLOCATE PREPARE stmt;

-- -----------------------------------------------------
-- 4. Verificación
-- -----------------------------------------------------
SELECT 'Origen LUZ' AS concepto, COUNT(*) AS total FROM tarifasenergia WHERE UPPER(TRIM(tipo)) = 'LUZ'
UNION ALL
SELECT 'Destino tarifasluz', COUNT(*) FROM tarifasluz
UNION ALL
SELECT 'Origen GAS', COUNT(*) FROM tarifasenergia WHERE UPPER(TRIM(tipo)) = 'GAS'
UNION ALL
SELECT 'Destino tarifasgas', COUNT(*) FROM tarifasgas
UNION ALL
SELECT 'No migradas (LUZ+GAS)', COUNT(*) FROM tarifasenergia WHERE UPPER(TRIM(tipo)) NOT IN ('LUZ', 'GAS');

-- Listado de las tarifas que quedan fuera de la migración
SELECT id, empresa, nombre, tipo
FROM tarifasenergia
WHERE UPPER(TRIM(tipo)) NOT IN ('LUZ', 'GAS')
ORDER BY empresa, nombre;
