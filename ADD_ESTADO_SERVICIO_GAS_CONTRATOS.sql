-- =====================================================
-- Contratos de energía: estado del servicio de gas
-- El campo existente estadoServicio pasa a ser el del servicio de LUZ.
-- =====================================================

DELIMITER //

CREATE PROCEDURE AddEstadoServicioGasContratos()
BEGIN
    DECLARE CONTINUE HANDLER FOR SQLEXCEPTION BEGIN END;

    ALTER TABLE contratos ADD COLUMN estadoServicioGas VARCHAR(100) NULL;
END //

DELIMITER ;

CALL AddEstadoServicioGasContratos();
DROP PROCEDURE AddEstadoServicioGasContratos;
