-- Script para añadir relación gestor-colaborador
-- Fecha: 28/01/2026
-- Descripción: Permite que los gestores tengan colaboradores asociados

-- Añadir campo gestor_id a la tabla usuarios
ALTER TABLE usuarios
ADD COLUMN gestor_id INT NULL AFTER rol,
ADD CONSTRAINT fk_usuarios_gestor 
    FOREIGN KEY (gestor_id) 
    REFERENCES usuarios(idusuarios) 
    ON DELETE SET NULL;

-- Añadir índice para mejorar las consultas
CREATE INDEX idx_gestor_id ON usuarios(gestor_id);

-- Verificar cambios
SELECT 
    'Usuarios con gestor asignado:' as info, 
    COUNT(*) as cantidad 
FROM usuarios 
WHERE gestor_id IS NOT NULL;
