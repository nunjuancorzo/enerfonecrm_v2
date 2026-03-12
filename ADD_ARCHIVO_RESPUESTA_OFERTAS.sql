-- Script para añadir el campo de archivo adjunto en la respuesta de ofertas
-- Fecha: 10 de marzo de 2026
-- Descripción: Añade el campo ruta_archivo_respuesta a la tabla solicitudes_ofertas
--              para permitir adjuntar archivos en la respuesta al usuario

ALTER TABLE solicitudes_ofertas 
ADD COLUMN ruta_archivo_respuesta VARCHAR(500) NULL
COMMENT 'Ruta del archivo adjunto enviado como respuesta al usuario';
