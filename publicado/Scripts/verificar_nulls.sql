-- Verificar si todavía existen valores NULL en los campos
SELECT 
    COUNT(*) as total_contratos,
    SUM(CASE WHEN titular_iban_dni IS NULL THEN 1 ELSE 0 END) as nulls_titular_dni,
    SUM(CASE WHEN titular_iban_nombre IS NULL THEN 1 ELSE 0 END) as nulls_titular_nombre,
    SUM(CASE WHEN titular_iban_numero IS NULL THEN 1 ELSE 0 END) as nulls_titular_numero,
    SUM(CASE WHEN pdf_contrato_url IS NULL THEN 1 ELSE 0 END) as nulls_pdf,
    SUM(CASE WHEN numero_instalacion IS NULL THEN 1 ELSE 0 END) as nulls_numero_instalacion,
    SUM(CASE WHEN observaciones_estado IS NULL THEN 1 ELSE 0 END) as nulls_observaciones_estado,
    SUM(CASE WHEN tipo_linea_movil_principal IS NULL THEN 1 ELSE 0 END) as nulls_tipo_linea_principal
FROM contratos;
