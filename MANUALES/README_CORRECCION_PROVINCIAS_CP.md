# 🔧 Corrección de Provincias en Códigos Postales

## 📋 Problema Detectado

Los códigos postales en la base de datos tienen **Comunidades Autónomas** en lugar de **Provincias**.

### Ejemplos del error:
- ❌ "Andalucía" → ✅ "Cádiz", "Sevilla", "Granada", etc.
- ❌ "Comunidad de Madrid" → ✅ "Madrid"
- ❌ "País Vasco" → ✅ "Álava", "Bizkaia", "Gipuzkoa"
- ❌ "Cataluña" → ✅ "Barcelona", "Girona", "Lleida", "Tarragona"

## 🔍 Causa Raíz

El script `generar_codigos_postales_sql.py` estaba leyendo el campo incorrecto del archivo de Geonames:

```python
# ❌ ANTES (línea 94)
provincia = campos[3].strip()  # admin name1 = Comunidad Autónoma

# ✅ DESPUÉS (corregido)
provincia = campos[5].strip()  # admin name2 = Provincia
```

## ✅ Solución Aplicada

### 1️⃣ Script Python Corregido
Se actualizó `generar_codigos_postales_sql.py` para leer el campo correcto de provincias.

### 2️⃣ Nuevo SQL Generado
Se regeneró `ADD_MAESTRO_CODIGOS_POSTALES_COMPLETO.sql` con **11,150 códigos postales** y provincias correctas.

## 🚀 Cómo Aplicar la Corrección

### **Opción A: Recargar Completo (RECOMENDADO)**

La forma más limpia es eliminar los datos antiguos y cargar los nuevos:

```bash
# 1. Ejecutar el script de limpieza
mysql -u root -p corcrm_db < FIX_PROVINCIAS_CODIGOS_POSTALES.sql

# 2. Cargar los datos correctos
mysql -u root -p corcrm_db < ADD_MAESTRO_CODIGOS_POSTALES_COMPLETO.sql
```

**Ventajas:**
- ✅ Datos 100% correctos y actualizados
- ✅ Incluye nuevos códigos postales si los hay
- ✅ Limpia posibles inconsistencias

**Desventajas:**
- ⚠️ Elimina datos actuales (pero se recargan inmediatamente)

---

### **Opción B: Actualización Manual**

Si prefieres actualizar sin eliminar, edita `FIX_PROVINCIAS_CODIGOS_POSTALES.sql` y descomenta la sección "OPCIÓN 2" que contiene updates por rangos de códigos postales.

```bash
mysql -u root -p corcrm_db < FIX_PROVINCIAS_CODIGOS_POSTALES.sql
```

**Ventajas:**
- ✅ No elimina datos
- ✅ Más conservador

**Desventajas:**
- ⚠️ Requiere mapeo manual completo
- ⚠️ Más propenso a errores

---

## 📊 Verificación

Después de aplicar la corrección, verifica que las provincias sean correctas:

```sql
-- Ver provincias distintas
SELECT DISTINCT provincia 
FROM codigos_postales 
ORDER BY provincia;

-- Verificar códigos postales específicos
SELECT * FROM codigos_postales WHERE codigo_postal IN ('28001', '08001', '41001');
-- Debería mostrar: Madrid, Barcelona, Sevilla (no comunidades)

-- Contar registros
SELECT COUNT(*) as total, COUNT(DISTINCT provincia) as provincias
FROM codigos_postales;
-- Debería mostrar ~11,150 códigos y ~52 provincias
```

## 📁 Archivos Generados

- ✅ `FIX_PROVINCIAS_CODIGOS_POSTALES.sql` - Script de corrección
- ✅ `ADD_MAESTRO_CODIGOS_POSTALES_COMPLETO.sql` - Datos actualizados con provincias correctas
- ✅ `generar_codigos_postales_sql.py` - Script Python corregido

## 🎯 Resultado Esperado

**ANTES:**
```
28001 | Madrid | Comunidad de Madrid
08001 | Barcelona | Cataluña
41001 | Sevilla | Andalucía
```

**DESPUÉS:**
```
28001 | Madrid | Madrid
08001 | Barcelona | Barcelona
41001 | Sevilla | Sevilla
```

## 🔄 Regenerar en el Futuro

Si necesitas actualizar los códigos postales nuevamente:

```bash
# El script está corregido, solo ejecuta:
python3 generar_codigos_postales_sql.py

# Luego aplica el SQL generado:
mysql -u root -p corcrm_db < ADD_MAESTRO_CODIGOS_POSTALES_COMPLETO.sql
```

## 📝 Notas

- El script descarga datos actualizados de Geonames cada vez que se ejecuta
- Las provincias ahora son las 52 provincias españolas oficiales (50 + Ceuta + Melilla)
- Los datos incluyen ciudad, provincia y código postal únicos
