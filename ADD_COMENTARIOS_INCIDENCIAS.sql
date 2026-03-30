-- Script para agregar tabla de comentarios de incidencias
-- y actualizar estados de incidencias
-- Fecha: 27 de marzo de 2026

USE enerfone_crm;

-- ========================================
-- CREAR TABLA DE COMENTARIOS
-- ========================================

CREATE TABLE IF NOT EXISTS comentarios_incidencias (
    id INT AUTO_INCREMENT PRIMARY KEY,
    incidencia_id INT NOT NULL,
    usuario_id INT NULL,
    nombre_usuario VARCHAR(100) NOT NULL,
    comentario VARCHAR(2000) NOT NULL,
    fecha_creacion DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    email_enviado TINYINT(1) NOT NULL DEFAULT 0,
    FOREIGN KEY (incidencia_id) REFERENCES incidencias(id) ON DELETE CASCADE,
    INDEX idx_incidencia (incidencia_id),
    INDEX idx_fecha (fecha_creacion)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- ========================================
-- VERIFICAR TABLA CREADA
-- ========================================

SELECT 
    'Tabla comentarios_incidencias creada correctamente' AS resultado,
    COUNT(*) AS total_comentarios
FROM comentarios_incidencias;

-- ========================================
-- INFORMACIÓN SOBRE EL ESTADO "EN VALIDACIÓN"
-- ========================================

/*
NUEVO ESTADO DISPONIBLE: "En validación"

Estados de incidencias:
- Pendiente
- Pendiente subir PRO
- En validación (NUEVO)
- En Proceso  
- Resuelta
- Cerrada

FUNCIONALIDAD DE COMENTARIOS:
- Los administradores pueden agregar comentarios a cualquier incidencia
- Cada comentario queda registrado con fecha y usuario
- Al agregar un comentario, se puede enviar un email automático al usuario que creó la incidencia
- El historial de comentarios se mantiene completo para seguimiento

CAMPOS DE LA TABLA:
- id: Identificador único del comentario
- incidencia_id: ID de la incidencia a la que pertenece
- usuario_id: ID del usuario que hizo el comentario (normalmente admin)
- nombre_usuario: Nombre del usuario para mostrar
- comentario: Texto del comentario (hasta 2000 caracteres)
- fecha_creacion: Cuándo se creó el comentario
- email_enviado: Si se envió email de notificación al usuario
*/
