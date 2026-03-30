-- Script para arreglar la tabla comentarios_incidencias
-- Eliminar foreign key a usuarios y hacer usuario_id nullable
-- Fecha: 27 de marzo de 2026

USE enerfone_crm;

-- ========================================
-- ELIMINAR TODAS LAS FOREIGN KEYS
-- ========================================

-- Desactivar temporalmente las verificaciones de foreign keys
SET FOREIGN_KEY_CHECKS=0;

-- Eliminar la tabla y recrearla sin la foreign key a usuarios
DROP TABLE IF EXISTS comentarios_incidencias;

CREATE TABLE comentarios_incidencias (
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

-- Reactivar las verificaciones de foreign keys
SET FOREIGN_KEY_CHECKS=1;

-- ========================================
-- VERIFICAR CAMBIOS
-- ========================================

DESC comentarios_incidencias;

SELECT 
    'Tabla comentarios_incidencias recreada correctamente' AS resultado,
    'usuario_id es NULL, sin foreign key a usuarios' AS detalle;
