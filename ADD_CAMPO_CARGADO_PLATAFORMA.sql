-- Añadir campo para indicar si el contrato ya está cargado en la plataforma de la comercializadora
-- Fecha: 05/05/2026

ALTER TABLE contratos 
ADD COLUMN cargado_en_plataforma TINYINT(1) NOT NULL DEFAULT 0 
COMMENT 'Indica si el contrato ya está cargado directamente en la plataforma de la comercializadora';

-- Verificar el cambio
SELECT 
    COUNT(*) as total_contratos,
    SUM(CASE WHEN cargado_en_plataforma = 1 THEN 1 ELSE 0 END) as cargados_en_plataforma,
    SUM(CASE WHEN cargado_en_plataforma = 0 THEN 1 ELSE 0 END) as pendientes_cargar
FROM contratos;
