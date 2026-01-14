-- Script de creación de base de datos para Enerfone CRM
-- Ejecutar este script en tu servidor MySQL

CREATE DATABASE IF NOT EXISTS EnerfoneCRM CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci;

USE EnerfoneCRM;

-- Tabla de Usuarios
CREATE TABLE IF NOT EXISTS Usuarios (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    Nombre VARCHAR(100) NOT NULL,
    Apellidos VARCHAR(100) NOT NULL,
    Email VARCHAR(150) NOT NULL UNIQUE,
    PasswordHash VARCHAR(255) NOT NULL,
    Activo TINYINT(1) NOT NULL DEFAULT 1,
    FechaCreacion DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    UltimoAcceso DATETIME NULL,
    Rol VARCHAR(50) NOT NULL DEFAULT 'Usuario',
    INDEX idx_email (Email),
    INDEX idx_activo (Activo)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- Insertar un usuario de prueba (password: 123456)
-- Hash SHA256 de "123456": 8d969eef6ecad3c29a3a629280e686cf0c3f5d5a86aff3ca12020c923adc6c92
INSERT INTO Usuarios (Nombre, Apellidos, Email, PasswordHash, Activo, FechaCreacion, Rol)
VALUES ('Admin', 'Sistema', 'admin@enerfone.com', 'jGl25bVBBBW96Qi9Te4V37Fnqchz/Eu4qB9vKrRIqRg=', 1, NOW(), 'Administrador')
ON DUPLICATE KEY UPDATE Id=Id;
