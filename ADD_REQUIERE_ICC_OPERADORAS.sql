-- Script para añadir campo RequiereICC a la tabla operadoras
-- Este campo indica si la operadora requiere código ICC para las líneas móviles

-- Añadir columna requiere_icc
ALTER TABLE operadoras 
ADD COLUMN requiere_icc TINYINT NOT NULL DEFAULT 0 AFTER activo;

-- Actualizar operadoras existentes que requieren ICC (ajustar según necesidad)
-- Ejemplo: Marcar O2 y Symio como operadoras que requieren ICC
-- UPDATE operadoras SET requiere_icc = 1 WHERE nombre IN ('O2', 'Symio');
