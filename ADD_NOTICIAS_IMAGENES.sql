-- Script para crear la tabla de imágenes de noticias
-- Permite múltiples imágenes por noticia

CREATE TABLE IF NOT EXISTS noticias_imagenes (
    id INT AUTO_INCREMENT PRIMARY KEY,
    mensaje_id INT NOT NULL,
    imagen_url VARCHAR(500) NOT NULL,
    orden INT DEFAULT 0,
    descripcion VARCHAR(255) NULL,
    fecha_creacion DATETIME DEFAULT CURRENT_TIMESTAMP,
    CONSTRAINT fk_noticias_imagenes_mensaje 
        FOREIGN KEY (mensaje_id) 
        REFERENCES mensajes_bienvenida(id) 
        ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- Índices para mejorar el rendimiento
CREATE INDEX idx_noticias_imagenes_mensaje_id ON noticias_imagenes(mensaje_id);
CREATE INDEX idx_noticias_imagenes_orden ON noticias_imagenes(mensaje_id, orden);

-- Migrar imágenes existentes de la columna imagen_url
INSERT INTO noticias_imagenes (mensaje_id, imagen_url, orden, fecha_creacion)
SELECT id, imagen_url, 0, fecha_creacion
FROM mensajes_bienvenida
WHERE imagen_url IS NOT NULL AND imagen_url != '';

-- La columna imagen_url del mensaje se mantiene por compatibilidad pero ya no se usa
-- Se puede eliminar en el futuro:
-- ALTER TABLE mensajes_bienvenida DROP COLUMN imagen_url;
