-- Script para agregar campos de datos del interesado y tipo de cliente en solicitudes de ofertas
-- Ejecutar en la base de datos CorCRM

USE corcrmv2;

-- Agregar campos de datos del interesado
ALTER TABLE solicitudes_ofertas
ADD COLUMN nombre_interesado VARCHAR(200) NULL COMMENT 'Nombre completo del interesado' AFTER email_comercial,
ADD COLUMN telefono_interesado VARCHAR(20) NULL COMMENT 'Teléfono del interesado' AFTER nombre_interesado,
ADD COLUMN email_interesado VARCHAR(100) NULL COMMENT 'Email del interesado' AFTER telefono_interesado;

-- Agregar campo de tipo de cliente para Luz/Gas
ALTER TABLE solicitudes_ofertas
ADD COLUMN luz_gas_tipo_cliente VARCHAR(50) NULL COMMENT 'Tipo de cliente: Residencial o PYME' AFTER tipo_alarma;

-- Verificar los cambios
DESCRIBE solicitudes_ofertas;

SELECT 
    'Se han agregado correctamente los campos:' as Mensaje,
    '- nombre_interesado' as Campo1,
    '- telefono_interesado' as Campo2,
    '- email_interesado' as Campo3,
    '- luz_gas_tipo_cliente' as Campo4;
