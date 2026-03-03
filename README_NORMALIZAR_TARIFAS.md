# Normalización de Tarifas de Energía Existentes

## 📋 Descripción

Este script SQL actualiza todas las tarifas de energía existentes en la base de datos para:

1. **Normalizar nombres de comercializadoras**: Convierte a Title Case (Ej: "AXPO" → "Axpo")
2. **Formatear precios**: Asegura que potencias y energías tengan exactamente 6 decimales con coma como separador

## 🚀 Cómo Usar

### Opción 1: Usando MySQL desde terminal

```bash
# Para base de datos de desarrollo
mysql -u enerfone -p"Salaiet6680." energy_crm < NORMALIZAR_TARIFAS_ENERGIA.sql

# Para base de datos de producción Enerfone
mysql -u enerfone -p"Salaiet6680." energy_crm_enerfone < NORMALIZAR_TARIFAS_ENERGIA.sql

# Para base de datos de producción Grupo Basette
mysql -u enerfone -p"Salaiet6680." energy_crm_grupobasette < NORMALIZAR_TARIFAS_ENERGIA.sql
```

### Opción 2: Usando MySQL Workbench o phpMyAdmin

1. Abre tu cliente MySQL
2. Selecciona la base de datos correspondiente
3. Abre el archivo `NORMALIZAR_TARIFAS_ENERGIA.sql`
4. Ejecuta el script completo

### Opción 3: Usando el cliente MySQL interactivo

```bash
# Conectar a MySQL
mysql -u enerfone -p

# Seleccionar base de datos
USE energy_crm;

# Copiar y pegar el contenido del script SQL
```

## ⚠️ Advertencias

- **BACKUP**: Haz un backup de la tabla `tarifasenergia` antes de ejecutar el script:
  ```sql
  CREATE TABLE tarifasenergia_backup AS SELECT * FROM tarifasenergia;
  ```

- **REVERSIÓN**: Si necesitas revertir:
  ```sql
  DELETE FROM tarifasenergia;
  INSERT INTO tarifasenergia SELECT * FROM tarifasenergia_backup;
  ```

## 🔍 Verificación

El script incluye consultas de verificación al final que muestran:

1. Total de tarifas actualizadas
2. Total de empresas únicas
3. Muestra de 5 registros con los nuevos formatos

## 📝 Ejemplos de Transformaciones

### Nombres de Comercializadoras
- `AXPO` → `Axpo`
- `IBERDROLA` → `Iberdrola`
- `endesa` → `Endesa`

### Valores de Potencia/Energía
- `0.1087792136988` → `0,108779`
- `0.148929` → `0,148929`
- `0.06` → `0,060000`

## 📅 Información

- **Creado**: 27 de febrero de 2026
- **Archivo**: `NORMALIZAR_TARIFAS_ENERGIA.sql`
- **Tabla afectada**: `tarifasenergia`
