# Corrección del Campo Comercial en Contratos

## Problema Identificado

El campo `comercial` (texto) en la tabla `contratos` se estaba guardando incorrectamente con el nombre del **usuario comercializadora** en lugar del **comercial asociado**.

## Cambios Realizados en el Código (Versión 20260813)

### 1. ContratosEnergia.razor
- ✅ Eliminada la asignación incorrecta de `contratoSeleccionado.Comercial` en `OnUsuarioComercializadoraChanged`
- ✅ El campo `Comercial` ahora solo se modifica en `OnUsuarioComercialChanged`

### 2. ContratosAlarmas.razor
- ✅ Corregida la función `OnUsuarioComercialChanged` para usar `UsuarioComercialId` en lugar de `UsuarioComercializadoraId`

### 3. ContratosTelefonia.razor
- ✅ Corregida la función `OnUsuarioComercialChanged` para usar `UsuarioComercialId` en lugar de `UsuarioComercializadoraId`

### 4. Wizards de Creación
- ✅ Ya corregidos anteriormente para usar `UsuarioComercialId`

## Corrección de Datos Existentes

⚠️ **IMPORTANTE**: Los contratos existentes en la base de datos tienen el campo `comercial` con datos incorrectos.

### Pasos para corregir los datos:

1. **Hacer backup de la tabla contratos**:
   ```sql
   CREATE TABLE contratos_backup_20260813 AS SELECT * FROM contratos;
   ```

2. **Ejecutar el script de corrección**:
   ```bash
   # Desde la carpeta del proyecto
   mysql -u [usuario] -p [nombre_base_datos] < FIX_COMERCIAL_FIELD.sql
   ```

3. **Verificar los cambios**:
   - El script incluye consultas de verificación al final
   - Comprueba que el campo `comercial` coincida con el nombre del usuario en `usuario_comercial_id`

## Verificación en la Aplicación

Después de ejecutar el script SQL:

1. Reinicia la aplicación
2. Ve al listado de contratos de energía
3. La columna "Comercial" debe mostrar ahora el nombre del comercial asociado
4. Ejemplo: Si el contrato tiene `usuario_comercial_id = 123` (Jorge), debe mostrar "Jorge", no "A-Iberdrola"

## Comportamiento Esperado

| Campo Base de Datos | Campo en Aplicación | Valor Mostrado |
|---------------------|---------------------|----------------|
| `usuario_comercial_id` | Comercial | Nombre del comercial asociado |
| `usuario_comercializadora_id` | Usuario Comercializadora | Nombre del usuario comercializadora (solo para permisos/comisiones) |

## Notas Adicionales

- El campo `usuario_comercializadora_id` sigue existiendo y funcionando para:
  - Cálculo de comisiones específicas del usuario comercializadora
  - Control de permisos en casos especiales
  - Pero **NO** determina qué se muestra en la columna "Comercial" del listado

- El campo `comercial` (texto) es redundante con `usuario_comercial_id` pero se mantiene por compatibilidad y para no hacer JOIN en cada consulta del listado.

## Fecha de Cambio

13 de agosto de 2026 - Versión 20260813
