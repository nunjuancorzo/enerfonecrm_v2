-- Script para actualizar la tabla comercializadoras: cambiar logo_url por logo_archivo y logo_contenido
-- Ejecutar este script en la base de datos

-- Agregar nuevas columnas para almacenar logos en base de datos
ALTER TABLE comercializadoras 
ADD COLUMN logo_archivo VARCHAR(255) NULL AFTER nombre,
ADD COLUMN logo_contenido LONGBLOB NULL AFTER logo_archivo;

-- Eliminar columna antigua si existe (en MySQL no se puede usar IF EXISTS con DROP COLUMN)
ALTER TABLE comercializadoras 
DROP COLUMN logo_url;

-- Eliminar comercializadoras que no estén en la lista oficial
DELETE FROM comercializadoras 
WHERE nombre NOT IN ('Axpo', 'Eleia', 'Endesa', 'Iberdrola', 'Mega', 'Naturgy', 'Niba', 'Totalenergies');

-- Insertar las comercializadoras oficiales si no existen
INSERT IGNORE INTO comercializadoras (nombre, activo, fecha_creacion)
VALUES 
    ('Axpo', 1, NOW()),
    ('Eleia', 1, NOW()),
    ('Endesa', 1, NOW()),
    ('Iberdrola', 1, NOW()),
    ('Mega', 1, NOW()),
    ('Naturgy', 1, NOW()),
    ('Niba', 1, NOW()),
    ('Totalenergies', 1, NOW());
