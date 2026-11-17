# Scripts para Producción - Versión 17112026

## Resumen de Cambios
Cambios para Optime y Gestión de Empresas que necesitan ejecutarse en producción.

---

## ORDEN DE EJECUCIÓN EN PRODUCCIÓN

### 1️⃣ **Script 1: Migración de Optime (PRINCIPAL)**
**Archivo:** `EnerfoneCRM/Data/MigrarOptime.sql`

**Qué hace:**
- Agrega columna `formulario_optime_habilitado` a tabla `empresas`
- Normaliza nombre de columna `empresa_id` → `id_empresa` en `optime_solicitudes`
- Crea índices y claves foráneas para Optime
- Crea índices para tablas relacionadas (`optime_documentos`, `optime_firmas`)

**Riesgos:** Bajo (incluye validaciones y manejo de errores)
**Tiempo estimado:** 2-5 segundos

---

### 2️⃣ **Script 2: Migración de es_empresa_prueba (IMPORTANTE)**
**Archivo:** `EnerfoneCRM/Data/MigrateEsEmpresaPruebaToEmpresas.sql`

**Qué hace:**
- Agrega columna `es_empresa_prueba` a tabla `empresas` (si no existe)
- Migra datos de `configuracion_empresa` a `empresas`
- Elimina columna `es_empresa_prueba` de `configuracion_empresa`

**Riesgos:** Bajo (incluye validaciones dinámicas)
**Tiempo estimado:** 2-5 segundos

---

## SCRIPTS ALTERNATIVOS (Si aplican específicamente)

### ⚠️ Script 3A: Fix de Optime EmpresaId (SI SOLO SI FALLA Script 1)
**Archivo:** `EnerfoneCRM/Data/FixOptimeEmpresaId.sql`

**Uso:** Solo ejecutar si el Script 1 falla al renombrar `empresa_id` a `id_empresa`

---

### ⚠️ Script 3B: Agregar formulario_optime_habilitado (SI SOLO SI FALLA Script 1)
**Archivo:** `EnerfoneCRM/Data/AddFormularioOptimeHabilitado.sql`

**Uso:** Solo ejecutar si el Script 1 no agregó la columna a `empresas`

---

## COMANDO SQL PARA EJECUTAR EN PRODUCCIÓN

```sql
-- Ejecutar en este orden:

-- 1. Migración de Optime
SOURCE 'ruta/a/EnerfoneCRM/Data/MigrarOptime.sql';

-- 2. Migración de es_empresa_prueba
SOURCE 'ruta/a/EnerfoneCRM/Data/MigrateEsEmpresaPruebaToEmpresas.sql';

-- Verificar que todo funcionó:
SHOW TABLES;
DESCRIBE empresas; -- Verificar columnas: formulario_optime_habilitado, es_empresa_prueba
DESCRIBE configuracion_empresa; -- Verificar que NO exista es_empresa_prueba aquí
```

---

## VERIFICACIÓN POST-MIGRACIÓN

Después de ejecutar los scripts, verifica:

### ✅ Verificación 1: Columnas en tabla `empresas`
```sql
SELECT COLUMN_NAME, COLUMN_TYPE, IS_NULLABLE, COLUMN_DEFAULT
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_SCHEMA = 'enerfonecrm' AND TABLE_NAME = 'empresas'
ORDER BY COLUMN_NAME;

-- Debe tener:
-- - formulario_optime_habilitado (TINYINT(1), NOT NULL, DEFAULT 0)
-- - es_empresa_prueba (TINYINT(1), NOT NULL, DEFAULT 0)
```

### ✅ Verificación 2: Tabla `optime_solicitudes`
```sql
SELECT COLUMN_NAME, COLUMN_TYPE, IS_NULLABLE
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_SCHEMA = 'enerfonecrm' AND TABLE_NAME = 'optime_solicitudes'
ORDER BY COLUMN_NAME;

-- Debe tener: id_empresa (INT, NOT NULL)
-- NO debe tener: empresa_id
```

### ✅ Verificación 3: Claves foráneas
```sql
SELECT CONSTRAINT_NAME, TABLE_NAME, REFERENCED_TABLE_NAME
FROM INFORMATION_SCHEMA.REFERENTIAL_CONSTRAINTS
WHERE CONSTRAINT_SCHEMA = 'enerfonecrm'
AND TABLE_NAME IN ('optime_solicitudes', 'optime_documentos', 'optime_firmas');

-- Debe mostrar:
-- - fk_optime_solicitudes_empresa
```

### ✅ Verificación 4: Índices
```sql
SHOW INDEXES FROM optime_solicitudes;

-- Debe tener:
-- - idx_optime_empresa
```

---

## ❌ ROLLBACK (Si algo falla)

Si necesitas deshacer cambios:

```sql
-- Rollback es_empresa_prueba (si es necesario)
-- Volver a agregar columna a configuracion_empresa
ALTER TABLE configuracion_empresa
ADD COLUMN es_empresa_prueba TINYINT(1) NOT NULL DEFAULT 0;

-- Restaurar datos desde empresas
UPDATE configuracion_empresa ce
SET ce.es_empresa_prueba = (
    SELECT e.es_empresa_prueba FROM empresas e
    WHERE e.id_empresa = ce.id
);
```

---

## RESUMEN FINAL

| Script | Tabla Destino | Cambio | Prioridad |
|--------|--------------|--------|-----------|
| MigrarOptime.sql | empresas, optime_solicitudes | Agregar columnas, índices, FK | 🔴 CRÍTICA |
| MigrateEsEmpresaPruebaToEmpresas.sql | empresas, configuracion_empresa | Migrar y eliminar columna | 🟠 IMPORTANTE |

---

**Versión:** 17112026  
**Fecha:** 17/11/2026  
**Estado:** Listo para producción
