-- Añade la opción para activar o desactivar el envío automático de documentos a firma
-- Ejecutar en la base de datos correspondiente si la columna no existe

ALTER TABLE configuracion_empresa
ADD COLUMN envio_documentos_firma_automatico BOOLEAN NOT NULL DEFAULT TRUE COMMENT 'Indica si el envío de documentos para firmar se hace automáticamente al crear contratos';

UPDATE configuracion_empresa
SET envio_documentos_firma_automatico = TRUE
WHERE envio_documentos_firma_automatico IS NULL;

SELECT 'Campo envio_documentos_firma_automatico agregado correctamente a configuracion_empresa' as Resultado;
