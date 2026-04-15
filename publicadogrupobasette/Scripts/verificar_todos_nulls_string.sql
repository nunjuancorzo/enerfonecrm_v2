-- Verificar TODOS los campos string que pueden tener NULL
SELECT 
    id,
    tipo,
    estado,
    comercial,
    nombre_cliente,
    dni,
    direccion,
    iban,
    estadoServicio,
    en_Comercializadora,
    en_Tarifa,
    en_CUPS,
    en_CUPSGas,
    en_Servicios,
    en_IBAN,
    tipoOperacion,
    operadora_tel,
    Tarifa_tel,
    TipoTarifa_tel
FROM contratos
WHERE 
    tipo IS NULL 
    OR estado IS NULL
LIMIT 10;
