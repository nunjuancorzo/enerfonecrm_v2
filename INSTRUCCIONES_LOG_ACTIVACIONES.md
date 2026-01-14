# Instrucciones para activar el log de activaciones de contratos

## Scripts SQL a ejecutar (en orden)

1. **agregar_campo_fecha_activo.sql**
   - Agrega el campo `fecha_activo` a la tabla `contratos`
   
2. **crear_tabla_log_activaciones.sql**
   - Crea la tabla `log_activaciones_contratos` para registrar cada activación

## Ejecutar los scripts

```bash
# Desde la raíz del proyecto
mysql -u tu_usuario -p tu_base_de_datos < EnerfoneCRM/Scripts/agregar_campo_fecha_activo.sql
mysql -u tu_usuario -p tu_base_de_datos < EnerfoneCRM/Scripts/crear_tabla_log_activaciones.sql
```

## Funcionalidad implementada

### Cambios en la base de datos
- **Tabla contratos**: Nuevo campo `fecha_activo` (DATE) para almacenar la fecha de activación
- **Nueva tabla log_activaciones_contratos**: Registra cada vez que un contrato cambia a estado "Activo"
  - `id`: ID autoincremental
  - `contrato_id`: ID del contrato activado
  - `fecha_activacion`: Fecha manual de activación (obligatoria)
  - `fecha_registro`: Timestamp automático del registro
  - `usuario`: Usuario que realizó la activación
  - `observaciones`: Observaciones opcionales

### Cambios en el código
1. **Nuevo modelo**: `LogActivacionContrato.cs`
2. **Nuevo servicio**: `LogActivacionContratoService.cs` con métodos:
   - `RegistrarActivacionAsync()`: Registra una nueva activación
   - `ObtenerActivacionesPorContratoAsync()`: Obtiene el historial de activaciones
   - `ObtenerUltimaActivacionAsync()`: Obtiene la última activación

3. **Actualización del modelo Contrato**: Nuevo campo `FechaActivo`
4. **Actualización de la UI** (ContratosEnergia.razor):
   - Campo de fecha obligatorio cuando se selecciona estado "Activo"
   - Validación: No permite guardar sin fecha si el estado es "Activo"
   - Registro automático en el log cuando el contrato pasa a estado "Activo"

### Comportamiento
- Al editar un contrato y cambiar el estado a "Activo", aparece un campo de fecha obligatorio
- Si no se proporciona la fecha, muestra un error y no permite guardar
- Cuando se guarda con éxito, registra:
  - La fecha de activación en el campo `fecha_activo` del contrato
  - Un registro en la tabla `log_activaciones_contratos` con:
    - Fecha de activación
    - Usuario que activó
    - Estado anterior del contrato
    - Fecha/hora del registro

### Próximas mejoras sugeridas
- Agregar vista del historial de activaciones en el modal de detalle del contrato
- Implementar la misma funcionalidad para contratos de telefonía y alarmas
- Agregar reportes de contratos activados por período
