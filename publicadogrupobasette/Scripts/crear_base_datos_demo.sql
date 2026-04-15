-- Script para crear y configurar la base de datos de demo
-- Fecha: 30 de diciembre de 2025
-- Descripción: Crea la base de datos demoenerfone como copia de enerfone_pre

-- Crear la base de datos de demo si no existe
CREATE DATABASE IF NOT EXISTS demoenerfone
CHARACTER SET utf8mb4
COLLATE utf8mb4_general_ci;

-- Usar la base de datos de demo
USE demoenerfone;

-- Opcional: Si deseas copiar la estructura y datos de enerfone_pre a demoenerfone
-- Descomentar las siguientes líneas y ejecutar después de este script:

/*
-- Exportar estructura y datos de enerfone_pre:
-- Desde terminal ejecutar:
-- mysqldump -u enerfone -p enerfone_pre > backup_enerfone_pre.sql
-- mysql -u enerfone -p demoenerfone < backup_enerfone_pre.sql

-- O copiar tabla por tabla (ejemplo):
CREATE TABLE demoenerfone.usuarios LIKE enerfone_pre.usuarios;
INSERT INTO demoenerfone.usuarios SELECT * FROM enerfone_pre.usuarios;

CREATE TABLE demoenerfone.contratos LIKE enerfone_pre.contratos;
INSERT INTO demoenerfone.contratos SELECT * FROM enerfone_pre.contratos;

-- Repetir para todas las tablas necesarias...
*/

-- Verificar que la base de datos fue creada
SHOW DATABASES LIKE 'demoenerfone';

-- Mostrar las tablas (estará vacía si es nueva)
SHOW TABLES;
