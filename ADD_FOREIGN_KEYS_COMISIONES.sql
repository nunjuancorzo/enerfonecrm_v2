-- Script para agregar claves foráneas a las tablas de comisiones
-- Ejecutar DESPUÉS de crear todas las tablas del sistema de comisiones
-- Ejecutar en la base de datos enerfone_crm
-- Fecha: Marzo 2026

-- ================================================================
-- ⚠️ IMPORTANTE: NO EJECUTAR ESTE SCRIPT AUTOMÁTICAMENTE
-- ================================================================
-- Este script requiere ajustar los nombres de las columnas según
-- la estructura REAL de tus tablas usuarios, contratos, etc.
-- 
-- PASOS:
-- 1. Ejecutar: DESCRIBE usuarios;
-- 2. Verificar el nombre de la columna ID (puede ser 'Id', 'id', 'usuario_id', etc.)
-- 3. Ajustar TODAS las referencias en este script
-- 4. Repetir para tablas: contratos, historico_liquidaciones
-- 5. Ejecutar bloque por bloque
--
-- Entity Framework manejará las relaciones sin necesidad de claves foráneas,
-- así que este script es OPCIONAL.
-- ================================================================

USE enerfone_crm;

/*
-- ========================================
-- CLAVES FORÁNEAS: configuracion_comisiones
-- ========================================

ALTER TABLE configuracion_comisiones 
ADD CONSTRAINT fk_config_comision_usuario 
FOREIGN KEY (usuario_id) REFERENCES usuarios(id) ON DELETE CASCADE;

-- ========================================
-- CLAVES FORÁNEAS: detalle_comision_liquidacion
-- ========================================

ALTER TABLE detalle_comision_liquidacion 
ADD CONSTRAINT fk_detalle_liquidacion 
FOREIGN KEY (historico_liquidacion_id) REFERENCES historico_liquidaciones(id) ON DELETE CASCADE;

ALTER TABLE detalle_comision_liquidacion 
ADD CONSTRAINT fk_detalle_contrato 
FOREIGN KEY (contrato_id) REFERENCES contratos(id) ON DELETE CASCADE;

ALTER TABLE detalle_comision_liquidacion 
ADD CONSTRAINT fk_detalle_colaborador 
FOREIGN KEY (colaborador_id) REFERENCES usuarios(id);

ALTER TABLE detalle_comision_liquidacion 
ADD CONSTRAINT fk_detalle_gestor 
FOREIGN KEY (gestor_id) REFERENCES usuarios(id);

ALTER TABLE detalle_comision_liquidacion 
ADD CONSTRAINT fk_detalle_jefe_ventas 
FOREIGN KEY (jefe_ventas_id) REFERENCES usuarios(id);

ALTER TABLE detalle_comision_liquidacion 
ADD CONSTRAINT fk_detalle_director 
FOREIGN KEY (director_comercial_id) REFERENCES usuarios(id);

ALTER TABLE detalle_comision_liquidacion 
ADD CONSTRAINT fk_detalle_administrador 
FOREIGN KEY (administrador_id) REFERENCES usuarios(id);

-- ========================================
-- CLAVES FORÁNEAS: decomisiones
-- ========================================

ALTER TABLE decomisiones 
ADD CONSTRAINT fk_decomision_contrato 
FOREIGN KEY (contrato_id) REFERENCES contratos(id) ON DELETE CASCADE;

ALTER TABLE decomisiones 
ADD CONSTRAINT fk_decomision_usuario 
FOREIGN KEY (usuario_id) REFERENCES usuarios(id);

ALTER TABLE decomisiones 
ADD CONSTRAINT fk_decomision_liquidacion_original 
FOREIGN KEY (liquidacion_original_id) REFERENCES historico_liquidaciones(id);

ALTER TABLE decomisiones 
ADD CONSTRAINT fk_decomision_liquidacion_decomision 
FOREIGN KEY (liquidacion_decomision_id) REFERENCES historico_liquidaciones(id);

ALTER TABLE decomisiones 
ADD CONSTRAINT fk_decomision_creado_por 
FOREIGN KEY (creado_por_usuario_id) REFERENCES usuarios(id);

-- ========================================
-- VERIFICACIÓN
-- ========================================

-- Verificar que se crearon las claves foráneas
SELECT 
    TABLE_NAME,
    CONSTRAINT_NAME,
    REFERENCED_TABLE_NAME,
    REFERENCED_COLUMN_NAME
FROM information_schema.KEY_COLUMN_USAGE
WHERE TABLE_SCHEMA = 'enerfone_crm'
  AND TABLE_NAME IN ('configuracion_comisiones', 'detalle_comision_liquidacion', 'decomisiones')
  AND REFERENCED_TABLE_NAME IS NOT NULL
ORDER BY TABLE_NAME, CONSTRAINT_NAME;

SELECT 'Claves foráneas agregadas correctamente' AS resultado;
*/

-- ========================================
-- CONSULTA PARA VERIFICAR ESTRUCTURA
-- ========================================
-- Ejecutar estas consultas para ver la estructura real:

SELECT 'Estructura de usuarios:' AS info;
DESCRIBE usuarios;

SELECT 'Estructura de contratos:' AS info;
DESCRIBE contratos;

SELECT 'Estructura de historico_liquidaciones:' AS info;
DESCRIBE historico_liquidaciones;

-- Con esta información, ajusta las referencias arriba antes de ejecutar las FK
