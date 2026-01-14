-- Script para otorgar permisos al usuario 'enerfone' sobre la base de datos 'demoenerfone'
-- Ejecutar este script como root o con un usuario con privilegios GRANT

-- Otorgar todos los permisos sobre la base de datos demoenerfone al usuario enerfone
GRANT ALL PRIVILEGES ON demoenerfone.* TO 'enerfone'@'localhost';

-- Si el usuario se conecta desde otra IP (por ejemplo '%'), también otorgar permisos:
-- GRANT ALL PRIVILEGES ON demoenerfone.* TO 'enerfone'@'%';

-- Aplicar los cambios
FLUSH PRIVILEGES;

-- Verificar los permisos otorgados
SHOW GRANTS FOR 'enerfone'@'localhost';
