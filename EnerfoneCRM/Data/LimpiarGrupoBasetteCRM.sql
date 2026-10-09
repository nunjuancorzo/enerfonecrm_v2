-- DESTRUCTIVO: hacer copia de seguridad y detener el CRM y sus procesos de fondo.
-- Ejecutar en una conexion nueva con permisos sobre todas las tablas del esquema.
-- Conserva la estructura y la cuenta elegida; borra tambien configuracion y catalogos.
-- No elimina archivos externos de storage/uploads ni reinicia AUTO_INCREMENT.
USE grupobasettecrm;

SELECT idusuarios, username, rol, activo
FROM usuarios
WHERE rol = 'Administrador'
ORDER BY idusuarios;

-- Indicar el username exacto del administrador; solo se acepta una coincidencia.
SET @administrador_a_conservar = (
    SELECT CASE WHEN COUNT(*) = 1 THEN MIN(idusuarios) ELSE NULL END
    FROM grupobasettecrm.usuarios
    WHERE username = 'Administrador' AND rol = 'Administrador'
);
-- Para autorizar el borrado, sustituir '' por 'BORRAR grupobasettecrm'.
SET @confirmacion_borrado = 'BORRAR grupobasettecrm';

SELECT @administrador_a_conservar AS id_administrador_a_conservar;

DROP PROCEDURE IF EXISTS LimpiarGrupoBasetteCRM;

DELIMITER //
CREATE PROCEDURE LimpiarGrupoBasetteCRM(IN administrador_id INT, IN confirmacion VARCHAR(100))
BEGIN
    DECLARE fin BOOLEAN DEFAULT FALSE;
    DECLARE tabla_actual VARCHAR(64);
    DECLARE administrador_encontrado BIGINT DEFAULT 0;
    DECLARE filas_restantes BIGINT DEFAULT 0;
    DECLARE foreign_keys_original INT DEFAULT @@SESSION.FOREIGN_KEY_CHECKS;
    DECLARE safe_updates_original INT DEFAULT @@SESSION.SQL_SAFE_UPDATES;
    DECLARE sentencia_preparada BOOLEAN DEFAULT FALSE;

    DECLARE tablas CURSOR FOR
        SELECT TABLE_NAME
        FROM information_schema.TABLES
        WHERE TABLE_SCHEMA = 'grupobasettecrm'
          AND TABLE_TYPE = 'BASE TABLE'
        ORDER BY TABLE_NAME;

    DECLARE CONTINUE HANDLER FOR NOT FOUND SET fin = TRUE;
    DECLARE EXIT HANDLER FOR SQLEXCEPTION
    BEGIN
        ROLLBACK;
        IF sentencia_preparada THEN
            DEALLOCATE PREPARE limpieza_stmt;
        END IF;
        SET SESSION FOREIGN_KEY_CHECKS = foreign_keys_original;
        SET SESSION SQL_SAFE_UPDATES = safe_updates_original;
        RESIGNAL;
    END;

    IF DATABASE() IS NULL OR BINARY DATABASE() <> BINARY 'grupobasettecrm' THEN
        SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'Base de datos incorrecta: se requiere grupobasettecrm.';
    END IF;

    IF confirmacion IS NULL OR BINARY confirmacion <> BINARY 'BORRAR grupobasettecrm' THEN
        SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'Borrado no autorizado: configurar la confirmacion explicita.';
    END IF;

    IF administrador_id IS NULL OR administrador_id <= 0 THEN
        SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'Indicar el idusuarios del administrador que se conservara.';
    END IF;

    IF EXISTS (
        SELECT 1 FROM information_schema.TABLES
        WHERE TABLE_SCHEMA = 'grupobasettecrm' AND TABLE_TYPE = 'BASE TABLE'
          AND (ENGINE IS NULL OR ENGINE <> 'InnoDB')
    ) THEN
        SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'Borrado cancelado: todas las tablas deben ser InnoDB para permitir rollback.';
    END IF;

    IF EXISTS (
        SELECT 1 FROM information_schema.TRIGGERS
        WHERE TRIGGER_SCHEMA = 'grupobasettecrm'
    ) THEN
        SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'Borrado cancelado: revisar los triggers antes de limpiar la base de datos.';
    END IF;

    IF EXISTS (
        SELECT 1 FROM information_schema.KEY_COLUMN_USAGE
        WHERE REFERENCED_TABLE_NAME IS NOT NULL
          AND ((TABLE_SCHEMA = 'grupobasettecrm' AND REFERENCED_TABLE_SCHEMA <> 'grupobasettecrm')
            OR (TABLE_SCHEMA <> 'grupobasettecrm' AND REFERENCED_TABLE_SCHEMA = 'grupobasettecrm'))
    ) THEN
        SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'Borrado cancelado: existen claves foraneas con otra base de datos.';
    END IF;

    START TRANSACTION;

    SELECT COUNT(*) INTO administrador_encontrado
    FROM grupobasettecrm.usuarios
    WHERE idusuarios = administrador_id AND rol = 'Administrador';

    IF administrador_encontrado <> 1 THEN
        SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'El usuario indicado no existe o no tiene rol Administrador.';
    END IF;

    SET SESSION FOREIGN_KEY_CHECKS = 0;
    SET SESSION SQL_SAFE_UPDATES = 0;

    UPDATE grupobasettecrm.usuarios
    SET gestor_id = NULL, jefe_ventas_id = NULL, director_comercial_id = NULL
    WHERE idusuarios = administrador_id;

    OPEN tablas;
    bucle_borrado: LOOP
        FETCH tablas INTO tabla_actual;
        IF fin THEN
            LEAVE bucle_borrado;
        END IF;

        SET @limpieza_sql = CONCAT('DELETE FROM `grupobasettecrm`.`', REPLACE(tabla_actual, '`', '``'), '`');
        IF tabla_actual = 'usuarios' THEN
            SET @limpieza_sql = CONCAT(@limpieza_sql, ' WHERE `idusuarios` <> ', administrador_id);
        END IF;
        PREPARE limpieza_stmt FROM @limpieza_sql;
        SET sentencia_preparada = TRUE;
        EXECUTE limpieza_stmt;
        DEALLOCATE PREPARE limpieza_stmt;
        SET sentencia_preparada = FALSE;
    END LOOP;
    CLOSE tablas;

    SET fin = FALSE;
    OPEN tablas;
    bucle_verificacion: LOOP
        FETCH tablas INTO tabla_actual;
        IF fin THEN
            LEAVE bucle_verificacion;
        END IF;

        SET @limpieza_sql = CONCAT('SELECT COUNT(*) INTO @limpieza_filas FROM `grupobasettecrm`.`',
            REPLACE(tabla_actual, '`', '``'), '`');
        PREPARE limpieza_stmt FROM @limpieza_sql;
        SET sentencia_preparada = TRUE;
        EXECUTE limpieza_stmt;
        DEALLOCATE PREPARE limpieza_stmt;
        SET sentencia_preparada = FALSE;
        SET filas_restantes = @limpieza_filas;

        IF (tabla_actual = 'usuarios' AND filas_restantes <> 1)
            OR (tabla_actual <> 'usuarios' AND filas_restantes <> 0) THEN
            SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'Verificacion fallida: quedan datos inesperados. Borrado revertido.';
        END IF;
    END LOOP;
    CLOSE tablas;

    SELECT COUNT(*) INTO administrador_encontrado
    FROM grupobasettecrm.usuarios
    WHERE idusuarios = administrador_id AND rol = 'Administrador';
    IF administrador_encontrado <> 1 THEN
        SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'Verificacion fallida: no se conserva el administrador. Borrado revertido.';
    END IF;

    SET SESSION FOREIGN_KEY_CHECKS = foreign_keys_original;
    SET SESSION SQL_SAFE_UPDATES = safe_updates_original;
    COMMIT;

    SELECT 'Limpieza completada: solo queda el usuario administrador indicado.' AS resultado;
    SELECT idusuarios, username, rol, activo FROM grupobasettecrm.usuarios;
END//
DELIMITER ;

CALL LimpiarGrupoBasetteCRM(@administrador_a_conservar, @confirmacion_borrado);
DROP PROCEDURE LimpiarGrupoBasetteCRM;