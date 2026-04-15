-- Convertir TODOS los campos TEXT y BLOB a VARCHAR para evitar problemas de casting
-- Ejecutar en base de datos: enerfonecrm

ALTER TABLE contratos
    MODIFY COLUMN direccion VARCHAR(1000),
    MODIFY COLUMN observaciones VARCHAR(2000),
    MODIFY COLUMN en_Tarifa VARCHAR(500),
    MODIFY COLUMN en_CUPS VARCHAR(500),
    MODIFY COLUMN en_Servicios VARCHAR(500),
    MODIFY COLUMN en_IBAN VARCHAR(100),
    MODIFY COLUMN en_Titular VARCHAR(500),
    MODIFY COLUMN en_DNI VARCHAR(50),
    MODIFY COLUMN observaciones_alarma VARCHAR(2000),
    MODIFY COLUMN observaciones_estado VARCHAR(2000),
    MODIFY COLUMN factura VARCHAR(500),
    MODIFY COLUMN cuenta_bancaria VARCHAR(500),
    MODIFY COLUMN en_Factura VARCHAR(500),
    MODIFY COLUMN factura_pdf_tel VARCHAR(500);

SELECT 'Todos los campos TEXT/BLOB convertidos a VARCHAR correctamente' as resultado;
