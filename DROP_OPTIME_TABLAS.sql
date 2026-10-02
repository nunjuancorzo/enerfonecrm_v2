-- Elimina las tablas del módulo Optime (se pierden todos sus datos).
-- Después se puede ejecutar ADD_OPTIME_SOLICITUDES.sql para recrearlas.
DROP TABLE IF EXISTS optime_firmas;
DROP TABLE IF EXISTS optime_documentos;
DROP TABLE IF EXISTS optime_solicitudes;
