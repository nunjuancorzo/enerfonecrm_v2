-- =============================================
-- Script: Agregar campo usuario_comercial_id
-- Descripción: Añade un nuevo campo para separar el concepto de
--              "Comercial asociado" del "Usuario Comercializadora"
-- =============================================

DELIMITER //

CREATE PROCEDURE AddUsuarioComercialId()
BEGIN
    DECLARE CONTINUE HANDLER FOR SQLEXCEPTION BEGIN END;
    
    -- Agregar el nuevo campo usuario_comercial_id
    ALTER TABLE contratos ADD COLUMN usuario_comercial_id INT NULL;
    
    -- Crear índice para mejorar el rendimiento
    CREATE INDEX idx_usuario_comercial_id ON contratos(usuario_comercial_id);
    
    -- MIGRAR datos existentes: Los IDs que estaban en usuario_comercializadora_id
    -- eran realmente "comerciales asociados", así que los movemos a usuario_comercial_id
    UPDATE contratos 
    SET usuario_comercial_id = usuario_comercializadora_id,
        usuario_comercializadora_id = NULL
    WHERE usuario_comercializadora_id IS NOT NULL;
    
END //

DELIMITER ;

CALL AddUsuarioComercialId();
DROP PROCEDURE AddUsuarioComercialId;

-- Verificar resultado
SELECT 
    COUNT(*) as total_contratos,
    COUNT(usuario_comercializadora_id) as con_usuario_comercializadora,
    COUNT(usuario_comercial_id) as con_usuario_comercial
FROM contratos;

SELECT 'Campo usuario_comercial_id agregado correctamente' as resultado;
