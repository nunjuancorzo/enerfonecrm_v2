-- Script para verificar que las columnas tipo_via se agregaron correctamente
-- Fecha: 3 de febrero de 2026

USE enerfonecrm;

-- Verificar columnas en la tabla clientes
SELECT 
    'CLIENTES' as TABLA,
    COLUMN_NAME, 
    DATA_TYPE, 
    IS_NULLABLE, 
    COLUMN_DEFAULT
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_SCHEMA = 'enerfonecrm' 
    AND TABLE_NAME = 'clientes' 
    AND COLUMN_NAME = 'tipo_via';

-- Verificar columnas en la tabla contratos
SELECT 
    'CONTRATOS' as TABLA,
    COLUMN_NAME, 
    DATA_TYPE, 
    IS_NULLABLE, 
    COLUMN_DEFAULT
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_SCHEMA = 'enerfonecrm' 
    AND TABLE_NAME = 'contratos' 
    AND COLUMN_NAME = 'tipo_via_instalacion';

-- Contar registros en contratos para verificar que hay datos
SELECT 
    'TOTAL CONTRATOS' as INFO,
    COUNT(*) as CANTIDAD
FROM contratos;

-- Contar contratos por tipo
SELECT 
    'CONTRATOS POR TIPO' as INFO,
    tipo,
    COUNT(*) as CANTIDAD
FROM contratos
GROUP BY tipo;
