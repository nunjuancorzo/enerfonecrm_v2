-- Script para agregar columnas de porcentajes de comisión por proveedor en la tabla usuarios
-- NOTA: Este script ya NO se usa. Las comisiones se almacenan en la tabla usuario_comision_proveedores
-- Se mantiene como referencia histórica

-- Las columnas se agregaron previamente:
-- ALTER TABLE usuarios ADD COLUMN porcentaje_comision_operadora DECIMAL(5,2) DEFAULT 0;
-- ALTER TABLE usuarios ADD COLUMN porcentaje_comision_comercializadora DECIMAL(5,2) DEFAULT 0;
-- ALTER TABLE usuarios ADD COLUMN porcentaje_comision_empresa_alarma DECIMAL(5,2) DEFAULT 0;
