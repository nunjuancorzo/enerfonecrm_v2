-- Ejecutar en la base de datos del CRM antes de publicar la aplicacion.
-- Es global: no se vincula a ninguna empresa ni se crea un usuario de empresa.
CREATE TABLE IF NOT EXISTS perfil_superadmin (
    id INT NOT NULL,
    nombre VARCHAR(100) NOT NULL,
    apellidos VARCHAR(100) NOT NULL,
    email VARCHAR(254) NOT NULL,
    telefono VARCHAR(20) NULL,
    direccion VARCHAR(255) NULL,
    codigo_postal VARCHAR(10) NULL,
    localidad VARCHAR(100) NULL,
    PRIMARY KEY (id),
    CONSTRAINT chk_perfil_superadmin_unico CHECK (id = 1)
) CHARACTER SET utf8mb4;
