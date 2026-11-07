DELIMITER //

DROP PROCEDURE IF EXISTS MigrarEmpresasCRM//
CREATE PROCEDURE MigrarEmpresasCRM()
BEGIN
    DECLARE terminado INT DEFAULT 0;
    DECLARE tabla VARCHAR(64);
    DECLARE empresa_enerfone INT;
    DECLARE nombre_fk VARCHAR(64);
    DECLARE actualizaciones_seguras_originales INT;
    DECLARE tablas CURSOR FOR
        SELECT TABLE_NAME FROM information_schema.TABLES
        WHERE TABLE_SCHEMA = DATABASE() AND TABLE_TYPE = 'BASE TABLE'
          AND TABLE_NAME <> 'empresas';
    DECLARE CONTINUE HANDLER FOR NOT FOUND SET terminado = 1;
    DECLARE EXIT HANDLER FOR SQLEXCEPTION
    BEGIN
        IF actualizaciones_seguras_originales IS NOT NULL THEN
            SET SESSION SQL_SAFE_UPDATES = actualizaciones_seguras_originales;
        END IF;
        RESIGNAL;
    END;

    SET actualizaciones_seguras_originales = @@SESSION.SQL_SAFE_UPDATES;

    IF DATABASE() IS NULL OR NOT EXISTS (
        SELECT 1 FROM information_schema.TABLES
        WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'usuarios'
    ) THEN
        SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'Seleccione la base de datos del CRM antes de migrar.';
    END IF;

    SET SESSION SQL_SAFE_UPDATES = 0;

    CREATE TABLE IF NOT EXISTS empresas (
        id_empresa INT NOT NULL AUTO_INCREMENT,
        nombre VARCHAR(500) NOT NULL,
        activa TINYINT(1) NOT NULL DEFAULT 1,
        PRIMARY KEY (id_empresa),
        UNIQUE KEY ux_empresas_nombre (nombre)
    ) ENGINE=InnoDB;

    ALTER TABLE empresas MODIFY COLUMN nombre VARCHAR(500) NOT NULL;

    IF EXISTS (
        SELECT 1 FROM information_schema.COLUMNS
        WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'configuracion_empresa'
          AND COLUMN_NAME = 'nombre_empresa'
    ) THEN
        ALTER TABLE configuracion_empresa MODIFY COLUMN nombre_empresa VARCHAR(500) NOT NULL;
    END IF;

    INSERT INTO empresas (nombre, activa)
        SELECT 'Enerfone', 1 WHERE NOT EXISTS (SELECT 1 FROM empresas WHERE nombre = 'Enerfone');
    SELECT id_empresa INTO empresa_enerfone FROM empresas WHERE nombre = 'Enerfone';

    OPEN tablas;
    bucle: LOOP
        FETCH tablas INTO tabla;
        IF terminado = 1 THEN LEAVE bucle; END IF;

        IF NOT EXISTS (
            SELECT 1 FROM information_schema.COLUMNS
            WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = tabla AND COLUMN_NAME = 'id_empresa'
        ) THEN
            SET @sql_empresa = CONCAT('ALTER TABLE `', REPLACE(tabla, '`', '``'), '` ADD COLUMN id_empresa INT NULL');
            PREPARE consulta_empresa FROM @sql_empresa;
            EXECUTE consulta_empresa;
            DEALLOCATE PREPARE consulta_empresa;
        END IF;

        SET @sql_empresa = CONCAT('UPDATE `', REPLACE(tabla, '`', '``'), '` SET id_empresa = ', empresa_enerfone, ' WHERE id_empresa IS NULL OR id_empresa = 0');
        PREPARE consulta_empresa FROM @sql_empresa;
        EXECUTE consulta_empresa;
        DEALLOCATE PREPARE consulta_empresa;

        SET @sql_empresa = CONCAT('ALTER TABLE `', REPLACE(tabla, '`', '``'), '` MODIFY COLUMN id_empresa INT NOT NULL');
        PREPARE consulta_empresa FROM @sql_empresa;
        EXECUTE consulta_empresa;
        DEALLOCATE PREPARE consulta_empresa;

        IF NOT EXISTS (
            SELECT 1 FROM information_schema.STATISTICS
            WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = tabla AND COLUMN_NAME = 'id_empresa' AND SEQ_IN_INDEX = 1
        ) THEN
            SET @sql_empresa = CONCAT('ALTER TABLE `', REPLACE(tabla, '`', '``'), '` ADD INDEX ix_empresa (id_empresa)');
            PREPARE consulta_empresa FROM @sql_empresa;
            EXECUTE consulta_empresa;
            DEALLOCATE PREPARE consulta_empresa;
        END IF;

        SET nombre_fk = CONCAT('fk_empresa_', LEFT(tabla, 40), '_', HEX(CRC32(tabla)));
        IF NOT EXISTS (
            SELECT 1 FROM information_schema.KEY_COLUMN_USAGE
            WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = tabla
              AND COLUMN_NAME = 'id_empresa' AND REFERENCED_TABLE_NAME = 'empresas'
        ) THEN
            SET @sql_empresa = CONCAT('ALTER TABLE `', REPLACE(tabla, '`', '``'), '` ADD CONSTRAINT `', nombre_fk,
                '` FOREIGN KEY (id_empresa) REFERENCES empresas (id_empresa) ON DELETE RESTRICT ON UPDATE RESTRICT');
            PREPARE consulta_empresa FROM @sql_empresa;
            EXECUTE consulta_empresa;
            DEALLOCATE PREPARE consulta_empresa;
        END IF;
    END LOOP;
    CLOSE tablas;

    IF NOT EXISTS (
        SELECT 1 FROM information_schema.STATISTICS
        WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'usuarios' AND NON_UNIQUE = 0
        GROUP BY INDEX_NAME HAVING COUNT(*) = 1 AND MAX(COLUMN_NAME) = 'username'
    ) THEN
        ALTER TABLE usuarios ADD UNIQUE INDEX ux_usuarios_username (username);
    END IF;
    SET SESSION SQL_SAFE_UPDATES = actualizaciones_seguras_originales;
END//

CALL MigrarEmpresasCRM()//
DROP PROCEDURE MigrarEmpresasCRM//
DELIMITER ;

SELECT id_empresa, nombre, activa FROM empresas ORDER BY id_empresa;
SELECT TABLE_NAME AS tabla_sin_empresa FROM information_schema.TABLES AS tablas
WHERE TABLE_SCHEMA = DATABASE() AND TABLE_TYPE = 'BASE TABLE'
  AND NOT EXISTS (
      SELECT 1 FROM information_schema.COLUMNS AS columnas
      WHERE columnas.TABLE_SCHEMA = tablas.TABLE_SCHEMA AND columnas.TABLE_NAME = tablas.TABLE_NAME
        AND columnas.COLUMN_NAME = 'id_empresa'
  );