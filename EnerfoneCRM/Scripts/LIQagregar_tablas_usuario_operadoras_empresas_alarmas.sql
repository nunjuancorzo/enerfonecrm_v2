-- Script para crear las tablas de relación entre usuarios y operadoras/empresas de alarmas
-- Esto permite que cada usuario tenga permisos específicos sobre qué operadoras y empresas de alarmas puede visualizar
-- Fecha: 28 de enero de 2026

USE crm_enerfone;

-- Tabla para relacionar usuarios con operadoras permitidas
CREATE TABLE IF NOT EXISTS usuario_operadoras (
    idusuario_operadoras INT AUTO_INCREMENT PRIMARY KEY,
    usuario_id INT NOT NULL,
    operadora_id INT NOT NULL,
    fecha_asignacion DATETIME DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (usuario_id) REFERENCES usuarios(idusuarios) ON DELETE CASCADE,
    FOREIGN KEY (operadora_id) REFERENCES operadoras(id) ON DELETE CASCADE,
    UNIQUE KEY unique_usuario_operadora (usuario_id, operadora_id)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci
COMMENT='Relación entre usuarios y las operadoras que pueden visualizar';

-- Tabla para relacionar usuarios con empresas de alarmas permitidas
CREATE TABLE IF NOT EXISTS usuario_empresas_alarmas (
    idusuario_empresa_alarma INT AUTO_INCREMENT PRIMARY KEY,
    usuario_id INT NOT NULL,
    empresa_alarma_id INT NOT NULL,
    fecha_asignacion DATETIME DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (usuario_id) REFERENCES usuarios(idusuarios) ON DELETE CASCADE,
    FOREIGN KEY (empresa_alarma_id) REFERENCES empresas_alarmas(id) ON DELETE CASCADE,
    UNIQUE KEY unique_usuario_empresa_alarma (usuario_id, empresa_alarma_id)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci
COMMENT='Relación entre usuarios y las empresas de alarmas que pueden visualizar';

-- Crear índices para mejorar el rendimiento de las consultas
-- Nota: Las claves foráneas ya crean índices automáticamente en la columna referenciante
-- pero añadimos índices explícitos para optimizar consultas específicas
CREATE INDEX idx_usuario_operadoras_usuario ON usuario_operadoras(usuario_id);
CREATE INDEX idx_usuario_operadoras_operadora ON usuario_operadoras(operadora_id);
CREATE INDEX idx_usuario_empresas_alarmas_usuario ON usuario_empresas_alarmas(usuario_id);
CREATE INDEX idx_usuario_empresas_alarmas_empresa ON usuario_empresas_alarmas(empresa_alarma_id);

-- Verificar las tablas creadas
SHOW TABLES LIKE 'usuario_%';

SELECT 'Tablas creadas exitosamente' AS Resultado;
