-- =====================================================
-- Script: Corregir Provincias en Códigos Postales
-- Descripción: Actualiza las Comunidades Autónomas a Provincias correctas
-- Fecha: 2026-06-25
-- =====================================================

-- Verificar datos actuales
SELECT 
    'ANTES DE LA CORRECCIÓN' as estado,
    COUNT(*) as total_registros,
    COUNT(DISTINCT provincia) as provincias_distintas
FROM codigos_postales;

-- Mostrar algunas provincias incorrectas (comunidades autónomas)
SELECT DISTINCT provincia 
FROM codigos_postales 
ORDER BY provincia 
LIMIT 20;

-- =====================================================
-- OPCIÓN 1: ELIMINAR Y RECARGAR (RECOMENDADO)
-- =====================================================
-- Esta opción elimina todos los datos y los recarga con las provincias correctas

-- Desactivar verificaciones de claves foráneas temporalmente
SET FOREIGN_KEY_CHECKS = 0;

-- Vaciar la tabla
TRUNCATE TABLE codigos_postales;

-- Reactivar verificaciones
SET FOREIGN_KEY_CHECKS = 1;

-- =====================================================
-- IMPORTANTE: Ahora ejecuta el archivo completo
-- mysql -u usuario -p nombre_db < ADD_MAESTRO_CODIGOS_POSTALES_COMPLETO.sql
-- =====================================================

-- =====================================================
-- OPCIÓN 2: ACTUALIZACIÓN MANUAL POR RANGOS DE CP
-- =====================================================
-- Si prefieres actualizar sin eliminar, usa estas queries
-- (Requiere mapear todos los rangos de CPs a provincias)

/*
-- Ejemplos de actualización por rangos de código postal

-- Álava (01xxx)
UPDATE codigos_postales SET provincia = 'Álava' WHERE codigo_postal BETWEEN '01000' AND '01999';

-- Albacete (02xxx)
UPDATE codigos_postales SET provincia = 'Albacete' WHERE codigo_postal BETWEEN '02000' AND '02999';

-- Alicante (03xxx)
UPDATE codigos_postales SET provincia = 'Alicante' WHERE codigo_postal BETWEEN '03000' AND '03999';

-- Almería (04xxx)
UPDATE codigos_postales SET provincia = 'Almería' WHERE codigo_postal BETWEEN '04000' AND '04999';

-- Ávila (05xxx)
UPDATE codigos_postales SET provincia = 'Ávila' WHERE codigo_postal BETWEEN '05000' AND '05999';

-- Badajoz (06xxx)
UPDATE codigos_postales SET provincia = 'Badajoz' WHERE codigo_postal BETWEEN '06000' AND '06999';

-- Baleares (07xxx)
UPDATE codigos_postales SET provincia = 'Baleares' WHERE codigo_postal BETWEEN '07000' AND '07999';

-- Barcelona (08xxx)
UPDATE codigos_postales SET provincia = 'Barcelona' WHERE codigo_postal BETWEEN '08000' AND '08999';

-- Burgos (09xxx)
UPDATE codigos_postales SET provincia = 'Burgos' WHERE codigo_postal BETWEEN '09000' AND '09999';

-- Cáceres (10xxx)
UPDATE codigos_postales SET provincia = 'Cáceres' WHERE codigo_postal BETWEEN '10000' AND '10999';

-- Cádiz (11xxx)
UPDATE codigos_postales SET provincia = 'Cádiz' WHERE codigo_postal BETWEEN '11000' AND '11999';

-- Castellón (12xxx)
UPDATE codigos_postales SET provincia = 'Castellón' WHERE codigo_postal BETWEEN '12000' AND '12999';

-- Ciudad Real (13xxx)
UPDATE codigos_postales SET provincia = 'Ciudad Real' WHERE codigo_postal BETWEEN '13000' AND '13999';

-- Córdoba (14xxx)
UPDATE codigos_postales SET provincia = 'Córdoba' WHERE codigo_postal BETWEEN '14000' AND '14999';

-- A Coruña (15xxx)
UPDATE codigos_postales SET provincia = 'A Coruña' WHERE codigo_postal BETWEEN '15000' AND '15999';

-- Cuenca (16xxx)
UPDATE codigos_postales SET provincia = 'Cuenca' WHERE codigo_postal BETWEEN '16000' AND '16999';

-- Girona (17xxx)
UPDATE codigos_postales SET provincia = 'Girona' WHERE codigo_postal BETWEEN '17000' AND '17999';

-- Granada (18xxx)
UPDATE codigos_postales SET provincia = 'Granada' WHERE codigo_postal BETWEEN '18000' AND '18999';

-- Guadalajara (19xxx)
UPDATE codigos_postales SET provincia = 'Guadalajara' WHERE codigo_postal BETWEEN '19000' AND '19999';

-- Gipuzkoa (20xxx)
UPDATE codigos_postales SET provincia = 'Gipuzkoa' WHERE codigo_postal BETWEEN '20000' AND '20999';

-- Huelva (21xxx)
UPDATE codigos_postales SET provincia = 'Huelva' WHERE codigo_postal BETWEEN '21000' AND '21999';

-- Huesca (22xxx)
UPDATE codigos_postales SET provincia = 'Huesca' WHERE codigo_postal BETWEEN '22000' AND '22999';

-- Jaén (23xxx)
UPDATE codigos_postales SET provincia = 'Jaén' WHERE codigo_postal BETWEEN '23000' AND '23999';

-- León (24xxx)
UPDATE codigos_postales SET provincia = 'León' WHERE codigo_postal BETWEEN '24000' AND '24999';

-- Lleida (25xxx)
UPDATE codigos_postales SET provincia = 'Lleida' WHERE codigo_postal BETWEEN '25000' AND '25999';

-- La Rioja (26xxx)
UPDATE codigos_postales SET provincia = 'La Rioja' WHERE codigo_postal BETWEEN '26000' AND '26999';

-- Lugo (27xxx)
UPDATE codigos_postales SET provincia = 'Lugo' WHERE codigo_postal BETWEEN '27000' AND '27999';

-- Madrid (28xxx)
UPDATE codigos_postales SET provincia = 'Madrid' WHERE codigo_postal BETWEEN '28000' AND '28999';

-- Málaga (29xxx)
UPDATE codigos_postales SET provincia = 'Málaga' WHERE codigo_postal BETWEEN '29000' AND '29999';

-- Murcia (30xxx)
UPDATE codigos_postales SET provincia = 'Murcia' WHERE codigo_postal BETWEEN '30000' AND '30999';

-- Navarra (31xxx)
UPDATE codigos_postales SET provincia = 'Navarra' WHERE codigo_postal BETWEEN '31000' AND '31999';

-- Ourense (32xxx)
UPDATE codigos_postales SET provincia = 'Ourense' WHERE codigo_postal BETWEEN '32000' AND '32999';

-- Asturias (33xxx)
UPDATE codigos_postales SET provincia = 'Asturias' WHERE codigo_postal BETWEEN '33000' AND '33999';

-- Palencia (34xxx)
UPDATE codigos_postales SET provincia = 'Palencia' WHERE codigo_postal BETWEEN '34000' AND '34999';

-- Las Palmas (35xxx)
UPDATE codigos_postales SET provincia = 'Las Palmas' WHERE codigo_postal BETWEEN '35000' AND '35999';

-- Pontevedra (36xxx)
UPDATE codigos_postales SET provincia = 'Pontevedra' WHERE codigo_postal BETWEEN '36000' AND '36999';

-- Salamanca (37xxx)
UPDATE codigos_postales SET provincia = 'Salamanca' WHERE codigo_postal BETWEEN '37000' AND '37999';

-- Santa Cruz de Tenerife (38xxx)
UPDATE codigos_postales SET provincia = 'Santa Cruz de Tenerife' WHERE codigo_postal BETWEEN '38000' AND '38999';

-- Cantabria (39xxx)
UPDATE codigos_postales SET provincia = 'Cantabria' WHERE codigo_postal BETWEEN '39000' AND '39999';

-- Segovia (40xxx)
UPDATE codigos_postales SET provincia = 'Segovia' WHERE codigo_postal BETWEEN '40000' AND '40999';

-- Sevilla (41xxx)
UPDATE codigos_postales SET provincia = 'Sevilla' WHERE codigo_postal BETWEEN '41000' AND '41999';

-- Soria (42xxx)
UPDATE codigos_postales SET provincia = 'Soria' WHERE codigo_postal BETWEEN '42000' AND '42999';

-- Tarragona (43xxx)
UPDATE codigos_postales SET provincia = 'Tarragona' WHERE codigo_postal BETWEEN '43000' AND '43999';

-- Teruel (44xxx)
UPDATE codigos_postales SET provincia = 'Teruel' WHERE codigo_postal BETWEEN '44000' AND '44999';

-- Toledo (45xxx)
UPDATE codigos_postales SET provincia = 'Toledo' WHERE codigo_postal BETWEEN '45000' AND '45999';

-- Valencia (46xxx)
UPDATE codigos_postales SET provincia = 'Valencia' WHERE codigo_postal BETWEEN '46000' AND '46999';

-- Valladolid (47xxx)
UPDATE codigos_postales SET provincia = 'Valladolid' WHERE codigo_postal BETWEEN '47000' AND '47999';

-- Bizkaia (48xxx)
UPDATE codigos_postales SET provincia = 'Bizkaia' WHERE codigo_postal BETWEEN '48000' AND '48999';

-- Zamora (49xxx)
UPDATE codigos_postales SET provincia = 'Zamora' WHERE codigo_postal BETWEEN '49000' AND '49999';

-- Zaragoza (50xxx)
UPDATE codigos_postales SET provincia = 'Zaragoza' WHERE codigo_postal BETWEEN '50000' AND '50999';

-- Ceuta (51xxx)
UPDATE codigos_postales SET provincia = 'Ceuta' WHERE codigo_postal BETWEEN '51000' AND '51999';

-- Melilla (52xxx)
UPDATE codigos_postales SET provincia = 'Melilla' WHERE codigo_postal BETWEEN '52000' AND '52999';
*/

-- =====================================================
-- Verificación final
-- =====================================================
SELECT 
    'DESPUÉS DE LA CORRECCIÓN' as estado,
    COUNT(*) as total_registros,
    COUNT(DISTINCT provincia) as provincias_distintas
FROM codigos_postales;

-- Mostrar provincias corregidas
SELECT DISTINCT provincia 
FROM codigos_postales 
ORDER BY provincia;

-- Mostrar ejemplos de cada provincia
SELECT codigo_postal, ciudad, provincia 
FROM codigos_postales 
WHERE codigo_postal IN ('28001', '08001', '41001', '46001', '03001', '50001', '36001', '48001')
ORDER BY codigo_postal;
