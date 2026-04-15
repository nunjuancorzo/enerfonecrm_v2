-- Script para inicializar la base de datos demo con datos de prueba
-- Fecha: 30 de diciembre de 2025
-- Descripción: Borra todos los datos y crea datos de prueba para el entorno demo

USE demoenerfone;

-- Desactivar verificaciones de claves foráneas temporalmente
SET FOREIGN_KEY_CHECKS = 0;

-- ============================================
-- PASO 1: BORRAR TODOS LOS DATOS
-- ============================================

TRUNCATE TABLE observaciones_contratos;
TRUNCATE TABLE ficheroscontratos;
TRUNCATE TABLE ficherosclientes;
TRUNCATE TABLE contratos;
TRUNCATE TABLE clientes_simple;
TRUNCATE TABLE tareas_pendientes;
TRUNCATE TABLE servicios;
TRUNCATE TABLE tarifas_alarmas;
TRUNCATE TABLE tarifastelefonia;
TRUNCATE TABLE tarifasenergia;
TRUNCATE TABLE operadoras;
TRUNCATE TABLE comercializadoras;
TRUNCATE TABLE log_accesos;
TRUNCATE TABLE usuarios;
TRUNCATE TABLE configuracion_empresa;

-- ============================================
-- PASO 2: CREAR DATOS DE PRUEBA
-- ============================================

-- Usuario Administrador (contraseña: Demo2025!)
INSERT INTO usuarios (idusuarios, username, nombre, apellidos, direccion, codigo_postal, localidad, email, password, rol, comercializadora, comision, activo, fecha_creacion)
VALUES (1, 'administrador', 'Admin', 'Sistema', 'Calle Demo 123', '28001', 'Madrid', 'admin@demo.com', 'Demo2025!', 'Administrador', NULL, 0, 1, NOW());

-- Configuración de la empresa
INSERT INTO configuracion_empresa (id, nombre_empresa, cif, direccion, codigo_postal, telefono, email, web, ciudad, provincia, pais)
VALUES (1, 'Enerfone Demo', 'B12345678', 'Av. Demo 100', '28001', '900123456', 'contacto@demo.com', 'www.demo.com', 'Madrid', 'Madrid', 'España');

-- Comercializadoras de energía
INSERT INTO comercializadoras (id, nombre, activo) VALUES
(1, 'Iberdrola', 1),
(2, 'Endesa', 1),
(3, 'Naturgy', 1),
(4, 'Repsol', 1);

-- Operadoras de telefonía
INSERT INTO operadoras (id, nombre, activo) VALUES
(1, 'Movistar', 1),
(2, 'Vodafone', 1),
(3, 'Orange', 1),
(4, 'MásMóvil', 1);

-- Tarifas de energía
INSERT INTO tarifasenergia (id, empresa, nombre, tipo, precio, precioNew, comision, potencia1, potencia2, potencia3, potencia4, potencia5, potencia6, energia1, energia2, energia3, energia4, energia5, energia6) VALUES
(1, 'Iberdrola', 'Tarifa 2.0TD Estable', '2.0TD', '45.50', 42.00, 35.00, '0.120', '0.090', NULL, NULL, NULL, NULL, '0.145', '0.098', NULL, NULL, NULL, NULL),
(2, 'Endesa', 'Tarifa One Luz', '2.0TD', '48.00', 44.50, 38.00, '0.125', '0.095', NULL, NULL, NULL, NULL, '0.150', '0.102', NULL, NULL, NULL, NULL),
(3, 'Naturgy', 'Tarifa Dual', '2.0TD', '46.50', 43.00, 36.00, '0.118', '0.088', NULL, NULL, NULL, NULL, '0.142', '0.095', NULL, NULL, NULL, NULL);

-- Tarifas de telefonía  
INSERT INTO tarifastelefonia (id, compania, tipo, fibra, gbmovil, precio, comision, precioNew, comisionNew, TV) VALUES
(1, 'Movistar', 'Fibra+Móvil', '600Mb', 'Ilimitados', '89.90', '45.00', 89.90, 45.00, 'Incluido'),
(2, 'Vodafone', 'Fibra+Móvil', '1Gb', 'Ilimitados', '84.90', '42.00', 84.90, 42.00, 'Sin TV'),
(3, 'Orange', 'Fibra+Móvil', '500Mb', '100GB', '79.90', '40.00', 79.90, 40.00, 'Sin TV');

-- Tarifas de alarmas
INSERT INTO tarifas_alarmas (id, tipo, tipo_inmueble, nombre_tarifa, cuota_mensual, permanencia, empresa, comision, descripcion, activa) VALUES
(1, 'Hogar', 'Vivienda', 'Alarma Hogar Básica', 39.90, 24, 'Securitas Direct', 120.00, 'Sistema de alarma completo para el hogar', 1),
(2, 'Negocio', 'Local Comercial', 'Alarma Negocio', 49.90, 36, 'Prosegur', 150.00, 'Sistema de alarma para comercios', 1),
(3, 'Hogar', 'Vivienda', 'Alarma Premium', 44.90, 24, 'ADT', 135.00, 'Sistema avanzado con cámaras', 1);

-- Servicios
INSERT INTO servicios (id, tipo, empresa, nombreServicio, precio) VALUES
(1, 'energia', 'Iberdrola', 'Mantenimiento Caldera', '5.90'),
(2, 'energia', 'Endesa', 'Seguro Hogar Básico', '8.50'),
(3, 'telefonia', 'Movistar', 'Netflix Básico', '7.99'),
(4, 'telefonia', 'Vodafone', 'Amazon Prime', '4.99'),
(5, 'alarmas', 'Securitas Direct', 'Mantenimiento Premium', '12.00');

-- Clientes de prueba
INSERT INTO clientes_simple (id, nombre, dni_cif, telefono, email, direccion, numero, escalera, piso, puerta, codigo_postal, provincia, poblacion, tipo_cliente, representante, dni_representante, iban, copia_recibo_bancario, observaciones, comercial, fecha_alta, id_usuario, aclarador) VALUES
(1, 'María García López', '12345678A', '666111222', 'maria.garcia@email.com', 'Calle Mayor', '25', 'A', '3', 'B', '28013', 'Madrid', 'Madrid', 'Particular', NULL, NULL, 'ES7921000813610123456789', NULL, 'Cliente habitual', 'administrador', NOW(), 1, NULL),
(2, 'Comercial Los Olivos SL', 'B87654321', '911222333', 'info@losolivos.com', 'Av. Constitución', '100', NULL, 'Bajo', NULL, '28015', 'Madrid', 'Madrid', 'Pyme', 'Juan Martín', '87654321B', 'ES1420805801101234567891', NULL, 'Centro comercial', 'administrador', NOW(), 1, NULL),
(3, 'Pedro Sánchez Ruiz', '23456789B', '677333444', 'pedro.sanchez@email.com', 'Plaza España', '8', 'B', '1', 'A', '28020', 'Madrid', 'Madrid', 'Particular', NULL, NULL, 'ES9000491500051234567892', NULL, NULL, 'administrador', NOW(), 1, NULL);

-- Contratos de energía
INSERT INTO contratos (id, tipo, estado, comercial, fecha_creacion, fecha_modificacion, idCliente, nombre_cliente, dni, iban, en_Comercializadora, en_CUPS, en_Tarifa, direccion_instalacion_alarma, numero_instalacion, escalera_instalacion, piso_instalacion, puerta_instalacion, codigo_postal_instalacion, provincia_instalacion, localidad_instalacion, comision) VALUES
(1, 'energia', 'Activo', 'administrador', NOW(), NOW(), 1, 'María García López', '12345678A', 'ES7921000813610123456789', 'Iberdrola', 'ES0021000000000001JN0F', 'Tarifa 2.0TD Estable', 'Calle Mayor', '25', 'A', '3', 'B', '28013', 'Madrid', 'Madrid', 35.00),
(2, 'energia', 'Pendiente', 'administrador', NOW(), NOW(), 2, 'Comercial Los Olivos SL', 'B87654321', 'ES1420805801101234567891', 'Endesa', 'ES0031000000000002KP0G', 'Tarifa One Luz', 'Av. Constitución', '100', NULL, 'Bajo', NULL, '28015', 'Madrid', 'Madrid', 38.00);

-- Contratos de telefonía
INSERT INTO contratos (id, tipo, estado, comercial, fecha_creacion, fecha_modificacion, idCliente, nombre_cliente, dni, direccion, iban, operadora_tel, Tarifa_tel, LineaMovilPrincipal, tipo_linea_movil_principal, codigo_icc_principal, comision) VALUES
(3, 'telefonia', 'Activo', 'administrador', NOW(), NOW(), 3, 'Pedro Sánchez Ruiz', '23456789B', 'Plaza España, 8', 'ES9000491500051234567892', 'Movistar', 'Fusión Total', '666555444', 'Física', '8934071234567890123', 45.00);

-- Contratos de alarmas
INSERT INTO contratos (id, tipo, estado, comercial, fecha_creacion, fecha_modificacion, idCliente, nombre_cliente, dni, iban, tipo_alarma, subtipo_inmueble, kit_alarma, direccion_instalacion_alarma, numero_instalacion, escalera_instalacion, piso_instalacion, puerta_instalacion, codigo_postal_instalacion, provincia_instalacion, localidad_instalacion, comision) VALUES
(4, 'alarmas', 'Activo', 'administrador', NOW(), NOW(), 1, 'María García López', '12345678A', 'ES7921000813610123456789', 'Hogar', 'Vivienda', 'Alarma Hogar Básica', 'Calle Mayor', '25', 'A', '3', 'B', '28013', 'Madrid', 'Madrid', 120.00);

-- Observaciones de contratos
INSERT INTO observaciones_contratos (id, id_contrato, usuario, observacion, fecha_hora, estado_contrato) VALUES
(1, 1, 'administrador', 'Contrato activado correctamente. Cliente satisfecho con el cambio.', NOW(), 'Activo'),
(2, 2, 'administrador', 'Pendiente de documentación. Esperando CIF del comercio.', NOW(), 'Pendiente'),
(3, 3, 'administrador', 'Instalación completada. Todo funcionando correctamente.', NOW(), 'Activo'),
(4, 4, 'administrador', 'Sistema de alarma instalado y probado. Cliente formado en su uso.', NOW(), 'Activo');

-- Tareas pendientes
INSERT INTO tareas_pendientes (id, descripcion, id_usuario_asignado, fecha_creacion, fecha_vencimiento, prioridad, estado, notas) VALUES
(1, 'Llamar a María García - Hacer seguimiento del primer mes con la nueva tarifa', 1, NOW(), DATE_ADD(NOW(), INTERVAL 7 DAY), 'Media', 'Activa', 'Cliente: María García López'),
(2, 'Recoger documentación Los Olivos - Pendiente CIF y poderes del representante', 1, NOW(), DATE_ADD(NOW(), INTERVAL 3 DAY), 'Alta', 'Activa', 'Cliente: Comercial Los Olivos SL'),
(3, 'Revisión anual alarma - Programar revisión técnica anual del sistema', 1, NOW(), DATE_ADD(NOW(), INTERVAL 30 DAY), 'Baja', 'Activa', 'Cliente: María García López');

-- Log de accesos (ejemplo)
INSERT INTO log_accesos (id, id_usuario, nombre_usuario, rol, fecha_acceso) VALUES
(1, 1, 'administrador', 'Administrador', NOW());

-- Reactivar verificaciones de claves foráneas
SET FOREIGN_KEY_CHECKS = 1;

-- ============================================
-- VERIFICACIÓN
-- ============================================

SELECT 'Usuarios creados:' as Tabla, COUNT(*) as Total FROM usuarios
UNION ALL
SELECT 'Clientes creados:', COUNT(*) FROM clientes_simple
UNION ALL
SELECT 'Contratos creados:', COUNT(*) FROM contratos
UNION ALL
SELECT 'Comercializadoras:', COUNT(*) FROM comercializadoras
UNION ALL
SELECT 'Operadoras:', COUNT(*) FROM operadoras
UNION ALL
SELECT 'Tarifas energía:', COUNT(*) FROM tarifasenergia
UNION ALL
SELECT 'Tarifas telefonía:', COUNT(*) FROM tarifastelefonia
UNION ALL
SELECT 'Tarifas alarmas:', COUNT(*) FROM tarifas_alarmas
UNION ALL
SELECT 'Observaciones:', COUNT(*) FROM observaciones_contratos
UNION ALL
SELECT 'Tareas pendientes:', COUNT(*) FROM tareas_pendientes;

-- Mostrar información del usuario creado
SELECT 
    username as Usuario, 
    CONCAT(nombre, ' ', apellidos) as Nombre,
    email as Email,
    password as Contraseña,
    rol as Rol
FROM usuarios;
