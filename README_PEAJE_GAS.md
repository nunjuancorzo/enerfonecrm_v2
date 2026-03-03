# Añadir Columna Peaje Gas

## 📋 Descripción

Este script añade una nueva columna `peaje_gas` a la tabla `tarifasenergia` para separar los peajes de luz y gas.

## 🔧 Cambios Realizados

### Base de Datos
- **Nueva columna**: `peaje_gas` (VARCHAR(50), NULL)
- Se posiciona después de la columna `peaje`

### Modelo
- Añadido campo `PeajeGas` en `TarifaEnergia.cs`

### Interfaz de Usuario
- Campo **Peaje** renombrado a **Peaje Luz** (desplegable)
  - Opciones: 2.0, 3.0, 6.1, 6.2
- Nuevo campo **Peaje Gas** (desplegable)
  - Opciones: RL1, RL2, RL3, RL4, RL5, RL6

## 🚀 Cómo Ejecutar

### Opción 1: MySQL Workbench
1. Abre MySQL Workbench
2. Conecta a tu base de datos
3. Abre el archivo `ADD_PEAJE_GAS.sql`
4. Ejecuta el script

### Opción 2: Línea de comandos (si tienes mysql instalado)
```bash
mysql -u enerfone -p'Salaiet6680.' energy_crm < ADD_PEAJE_GAS.sql
```

### Opción 3: Cliente MySQL Interactivo
```sql
USE energy_crm;
ALTER TABLE tarifasenergia ADD COLUMN peaje_gas VARCHAR(50) NULL AFTER peaje;
```

## ⚠️ Importante

- **No requiere backup**: Esta operación solo añade una columna, no modifica datos existentes
- Los valores existentes en el campo `peaje` (ahora Peaje Luz) se mantienen intactos
- El nuevo campo `peaje_gas` estará vacío (NULL) en todos los registros existentes

## ✅ Verificación

Después de ejecutar el script, verifica que la columna se ha añadido correctamente:

```sql
DESCRIBE tarifasenergia;
```

Deberías ver la columna `peaje_gas` después de `peaje`.

## 📅 Información

- **Fecha**: 2 de marzo de 2026
- **Archivo**: `ADD_PEAJE_GAS.sql`
- **Tabla afectada**: `tarifasenergia`
