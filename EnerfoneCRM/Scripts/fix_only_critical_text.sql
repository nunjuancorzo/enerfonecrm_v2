-- Convertir solo los campos TEXT que se usan como string en el modelo, manteniendo tamaños razonables
-- Ejecutar en base de datos: enerfonecrm

ALTER TABLE contratos
    MODIFY COLUMN observaciones_alarma VARCHAR(2000),
    MODIFY COLUMN observaciones_estado VARCHAR(2000);

-- Los campos BLOB deben permanecer como están ya que probablemente no se mapean en el modelo C#
-- factura, cuenta_bancaria, en_Factura, factura_pdf_tel permanecen como BLOB

SELECT 'Campos críticos TEXT convertidos a VARCHAR' as resultado;
