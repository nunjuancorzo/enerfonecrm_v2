-- =====================================================
-- Contratos de energía: tarifa y servicio separados para GAS
-- Los campos existentes (en_Tarifa, en_tarifa_id, servicio_id,
-- en_Servicios) pasan a ser exclusivamente de LUZ.
-- =====================================================

DELIMITER //

CREATE PROCEDURE AddTarifaServicioGasContratos()
BEGIN
    DECLARE CONTINUE HANDLER FOR SQLEXCEPTION BEGIN END;

    ALTER TABLE contratos ADD COLUMN en_tarifa_gas VARCHAR(255) NULL;
    ALTER TABLE contratos ADD COLUMN en_tarifa_gas_id INT NULL;
    ALTER TABLE contratos ADD COLUMN servicio_gas_id INT NULL;
    ALTER TABLE contratos ADD COLUMN en_servicios_gas VARCHAR(255) NULL;
END //

DELIMITER ;

CALL AddTarifaServicioGasContratos();
DROP PROCEDURE AddTarifaServicioGasContratos;
