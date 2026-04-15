-- Script COMPLETO para convertir TODOS los campos string NULL a cadenas vacías
-- Ejecutar en base de datos: enerfonecrm

SET SQL_SAFE_UPDATES = 0;

UPDATE contratos 
SET 
    -- Campos básicos
    comercial = COALESCE(comercial, ''),
    nombre_cliente = COALESCE(nombre_cliente, ''),
    dni = COALESCE(dni, ''),
    direccion = COALESCE(direccion, ''),
    iban = COALESCE(iban, ''),
    
    -- Campos de energía
    estadoServicio = COALESCE(estadoServicio, ''),
    en_Comercializadora = COALESCE(en_Comercializadora, ''),
    en_Tarifa = COALESCE(en_Tarifa, ''),
    en_CUPS = COALESCE(en_CUPS, ''),
    en_CUPSGas = COALESCE(en_CUPSGas, ''),
    en_Servicios = COALESCE(en_Servicios, ''),
    en_IBAN = COALESCE(en_IBAN, ''),
    tipoOperacion = COALESCE(tipoOperacion, ''),
    
    -- Campos de telefonía
    operadora_tel = COALESCE(operadora_tel, ''),
    Tarifa_tel = COALESCE(Tarifa_tel, ''),
    TipoTarifa_tel = COALESCE(TipoTarifa_tel, ''),
    fijo_tel = COALESCE(fijo_tel, ''),
    LineaMovilPrincipal = COALESCE(LineaMovilPrincipal, ''),
    tipo_linea_movil_principal = COALESCE(tipo_linea_movil_principal, ''),
    codigo_icc_principal = COALESCE(codigo_icc_principal, ''),
    telefono_linea1_tel = COALESCE(telefono_linea1_tel, ''),
    tarifa_linea1_tel = COALESCE(tarifa_linea1_tel, ''),
    tipo_linea1_tel = COALESCE(tipo_linea1_tel, ''),
    codigo_icc_linea1_tel = COALESCE(codigo_icc_linea1_tel, ''),
    telefono_linea2_tel = COALESCE(telefono_linea2_tel, ''),
    tarifa_linea2_tel = COALESCE(tarifa_linea2_tel, ''),
    tipo_linea2_tel = COALESCE(tipo_linea2_tel, ''),
    codigo_icc_linea2_tel = COALESCE(codigo_icc_linea2_tel, ''),
    telefono_linea3_tel = COALESCE(telefono_linea3_tel, ''),
    tarifa_linea3_tel = COALESCE(tarifa_linea3_tel, ''),
    tipo_linea3_tel = COALESCE(tipo_linea3_tel, ''),
    codigo_icc_linea3_tel = COALESCE(codigo_icc_linea3_tel, ''),
    telefono_linea4_tel = COALESCE(telefono_linea4_tel, ''),
    tarifa_linea4_tel = COALESCE(tarifa_linea4_tel, ''),
    tipo_linea4_tel = COALESCE(tipo_linea4_tel, ''),
    codigo_icc_linea4_tel = COALESCE(codigo_icc_linea4_tel, ''),
    telefono_linea5_tel = COALESCE(telefono_linea5_tel, ''),
    tarifa_linea5_tel = COALESCE(tarifa_linea5_tel, ''),
    tipo_linea5_tel = COALESCE(tipo_linea5_tel, ''),
    codigo_icc_linea5_tel = COALESCE(codigo_icc_linea5_tel, ''),
    horario_instalacion_tel = COALESCE(horario_instalacion_tel, ''),
    contratar = COALESCE(contratar, ''),
    cuenta_bancaria = COALESCE(cuenta_bancaria, ''),
    TV = COALESCE(TV, ''),
    
    -- Campos de alarmas
    tipo_alarma = COALESCE(tipo_alarma, ''),
    subtipo_inmueble = COALESCE(subtipo_inmueble, ''),
    compania_anterior = COALESCE(compania_anterior, ''),
    numero_contrato_anterior = COALESCE(numero_contrato_anterior, ''),
    kit_alarma = COALESCE(kit_alarma, ''),
    opcionales_alarma = COALESCE(opcionales_alarma, ''),
    campana_alarma = COALESCE(campana_alarma, ''),
    direccion_instalacion_alarma = COALESCE(direccion_instalacion_alarma, ''),
    numero_instalacion = COALESCE(numero_instalacion, ''),
    escalera_instalacion = COALESCE(escalera_instalacion, ''),
    piso_instalacion = COALESCE(piso_instalacion, ''),
    puerta_instalacion = COALESCE(puerta_instalacion, ''),
    codigo_postal_instalacion = COALESCE(codigo_postal_instalacion, ''),
    provincia_instalacion = COALESCE(provincia_instalacion, ''),
    localidad_instalacion = COALESCE(localidad_instalacion, ''),
    aclarador_instalacion = COALESCE(aclarador_instalacion, ''),
    observaciones_alarma = COALESCE(observaciones_alarma, ''),
    observaciones_estado = COALESCE(observaciones_estado, ''),
    
    -- Campos de titular IBAN
    titular_iban_dni = COALESCE(titular_iban_dni, ''),
    titular_iban_nombre = COALESCE(titular_iban_nombre, ''),
    titular_iban_numero = COALESCE(titular_iban_numero, ''),
    
    -- PDF
    pdf_contrato_url = COALESCE(pdf_contrato_url, '')
WHERE 
    comercial IS NULL OR nombre_cliente IS NULL OR dni IS NULL OR direccion IS NULL OR iban IS NULL
    OR estadoServicio IS NULL OR en_Comercializadora IS NULL OR en_Tarifa IS NULL 
    OR en_CUPS IS NULL OR en_CUPSGas IS NULL OR en_Servicios IS NULL OR en_IBAN IS NULL 
    OR tipoOperacion IS NULL OR operadora_tel IS NULL OR Tarifa_tel IS NULL 
    OR TipoTarifa_tel IS NULL OR fijo_tel IS NULL OR LineaMovilPrincipal IS NULL
    OR tipo_linea_movil_principal IS NULL OR codigo_icc_principal IS NULL
    OR telefono_linea1_tel IS NULL OR tarifa_linea1_tel IS NULL OR tipo_linea1_tel IS NULL OR codigo_icc_linea1_tel IS NULL
    OR telefono_linea2_tel IS NULL OR tarifa_linea2_tel IS NULL OR tipo_linea2_tel IS NULL OR codigo_icc_linea2_tel IS NULL
    OR telefono_linea3_tel IS NULL OR tarifa_linea3_tel IS NULL OR tipo_linea3_tel IS NULL OR codigo_icc_linea3_tel IS NULL
    OR telefono_linea4_tel IS NULL OR tarifa_linea4_tel IS NULL OR tipo_linea4_tel IS NULL OR codigo_icc_linea4_tel IS NULL
    OR telefono_linea5_tel IS NULL OR tarifa_linea5_tel IS NULL OR tipo_linea5_tel IS NULL OR codigo_icc_linea5_tel IS NULL
    OR horario_instalacion_tel IS NULL OR contratar IS NULL OR cuenta_bancaria IS NULL OR TV IS NULL
    OR tipo_alarma IS NULL OR subtipo_inmueble IS NULL OR compania_anterior IS NULL 
    OR numero_contrato_anterior IS NULL OR kit_alarma IS NULL OR opcionales_alarma IS NULL 
    OR campana_alarma IS NULL OR direccion_instalacion_alarma IS NULL
    OR numero_instalacion IS NULL OR escalera_instalacion IS NULL OR piso_instalacion IS NULL 
    OR puerta_instalacion IS NULL OR codigo_postal_instalacion IS NULL OR provincia_instalacion IS NULL 
    OR localidad_instalacion IS NULL OR aclarador_instalacion IS NULL
    OR observaciones_alarma IS NULL OR observaciones_estado IS NULL
    OR titular_iban_dni IS NULL OR titular_iban_nombre IS NULL OR titular_iban_numero IS NULL
    OR pdf_contrato_url IS NULL;

SET SQL_SAFE_UPDATES = 1;

-- Verificar cuántos registros se actualizaron
SELECT 'Actualización completada' as resultado;
