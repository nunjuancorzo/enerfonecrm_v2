-- Script para corregir valores NULL en campos nuevos que causan error DBNull
-- Ejecutar en base de datos: enerfonecrm (y demoenerfone si existe)

-- Desactivar temporalmente el modo seguro
SET SQL_SAFE_UPDATES = 0;

UPDATE contratos 
SET 
    -- Campos de titular IBAN
    titular_iban_dni = COALESCE(titular_iban_dni, ''),
    titular_iban_nombre = COALESCE(titular_iban_nombre, ''),
    titular_iban_numero = COALESCE(titular_iban_numero, ''),
    
    -- Campos de dirección instalación
    numero_instalacion = COALESCE(numero_instalacion, ''),
    escalera_instalacion = COALESCE(escalera_instalacion, ''),
    piso_instalacion = COALESCE(piso_instalacion, ''),
    puerta_instalacion = COALESCE(puerta_instalacion, ''),
    codigo_postal_instalacion = COALESCE(codigo_postal_instalacion, ''),
    provincia_instalacion = COALESCE(provincia_instalacion, ''),
    localidad_instalacion = COALESCE(localidad_instalacion, ''),
    aclarador_instalacion = COALESCE(aclarador_instalacion, ''),
    
    -- PDF contrato
    pdf_contrato_url = COALESCE(pdf_contrato_url, ''),
    
    -- Campos de alarmas
    subtipo_inmueble = COALESCE(subtipo_inmueble, ''),
    compania_anterior = COALESCE(compania_anterior, ''),
    numero_contrato_anterior = COALESCE(numero_contrato_anterior, ''),
    kit_alarma = COALESCE(kit_alarma, ''),
    opcionales_alarma = COALESCE(opcionales_alarma, ''),
    campana_alarma = COALESCE(campana_alarma, ''),
    observaciones_alarma = COALESCE(observaciones_alarma, ''),
    
    -- Observaciones estado
    observaciones_estado = COALESCE(observaciones_estado, ''),
    
    -- Campos tipo ICC móviles
    tipo_linea_movil_principal = COALESCE(tipo_linea_movil_principal, ''),
    tipo_linea1_tel = COALESCE(tipo_linea1_tel, ''),
    tipo_linea2_tel = COALESCE(tipo_linea2_tel, ''),
    tipo_linea3_tel = COALESCE(tipo_linea3_tel, ''),
    tipo_linea4_tel = COALESCE(tipo_linea4_tel, ''),
    tipo_linea5_tel = COALESCE(tipo_linea5_tel, '')
WHERE 
    titular_iban_dni IS NULL 
    OR titular_iban_nombre IS NULL 
    OR titular_iban_numero IS NULL 
    OR numero_instalacion IS NULL 
    OR escalera_instalacion IS NULL 
    OR piso_instalacion IS NULL 
    OR puerta_instalacion IS NULL 
    OR codigo_postal_instalacion IS NULL 
    OR provincia_instalacion IS NULL 
    OR localidad_instalacion IS NULL 
    OR aclarador_instalacion IS NULL
    OR pdf_contrato_url IS NULL
    OR subtipo_inmueble IS NULL
    OR compania_anterior IS NULL
    OR numero_contrato_anterior IS NULL
    OR kit_alarma IS NULL
    OR opcionales_alarma IS NULL
    OR campana_alarma IS NULL
    OR observaciones_alarma IS NULL
    OR observaciones_estado IS NULL
    OR tipo_linea_movil_principal IS NULL
    OR tipo_linea1_tel IS NULL
    OR tipo_linea2_tel IS NULL
    OR tipo_linea3_tel IS NULL
    OR tipo_linea4_tel IS NULL
    OR tipo_linea5_tel IS NULL;

-- Reactivar el modo seguro
SET SQL_SAFE_UPDATES = 1;
