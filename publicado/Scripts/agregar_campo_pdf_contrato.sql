-- Agregar campo para almacenar la URL del PDF del contrato

ALTER TABLE contratos 
ADD COLUMN pdf_contrato_url VARCHAR(500) NULL AFTER comision;

-- Comentario: Este campo almacena la ruta del archivo PDF del contrato cargado
-- Formato: /uploads/contratos/[nombre_archivo].pdf
