-- Script para crear la tabla de relación entre usuarios y comercializadoras
-- Esta tabla permitirá que los administradores seleccionen qué comercializadoras puede visualizar cada usuario

CREATE TABLE IF NOT EXISTS usuario_comercializadoras (
    id INT AUTO_INCREMENT PRIMARY KEY,
    usuario_id INT NOT NULL,
    comercializadora_id INT NOT NULL,
    fecha_asignacion DATETIME DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (usuario_id) REFERENCES usuarios(idusuarios) ON DELETE CASCADE,
    FOREIGN KEY (comercializadora_id) REFERENCES comercializadoras(id) ON DELETE CASCADE,
    UNIQUE KEY unique_usuario_comercializadora (usuario_id, comercializadora_id)
);

-- Índices para mejorar el rendimiento de consultas
CREATE INDEX idx_usuario_id ON usuario_comercializadoras(usuario_id);
CREATE INDEX idx_comercializadora_id ON usuario_comercializadoras(comercializadora_id);
