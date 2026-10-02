# Instrucciones para actualizar la base de datos

## Nueva columna: observaciones_estado

Se ha agregado una nueva funcionalidad para los contratos que requiere actualizar la base de datos.

### Pasos para aplicar la migración:

1. **Conectarse a la base de datos PostgreSQL:**
   ```bash
   psql -U postgres -d enerfonecrm
   ```

2. **Ejecutar el script SQL:**
   ```bash
   \i /Users/juanmariacorzo/Documents/EnerfoneCRMv2/EnerfoneCRM/Scripts/agregar_observaciones_estado.sql
   ```

   O alternativamente, ejecutar directamente:
   ```sql
   ALTER TABLE contratos ADD COLUMN observaciones_estado TEXT;
   ```

3. **Verificar que la columna se creó correctamente:**
   ```sql
   \d contratos
   ```

### Cambios realizados:

- **Modelo de datos:** Se agregó la propiedad `ObservacionesEstado` al modelo `Contrato.cs`
- **Base de datos:** Nueva columna `observaciones_estado` de tipo TEXT en la tabla `contratos`
- **Interfaz de usuario:** 
  - Se muestra un icono con tooltip en el listado cuando hay observaciones
  - Campo de texto en el formulario de edición (obligatorio para cancelados, opcional para otros estados)
- **Validación:** El sistema valida que se introduzcan observaciones cuando el estado es "Cancelado"

### Funcionalidad:

- **Para contratos CANCELADOS:** El campo de observaciones es obligatorio y debe indicar el motivo de la cancelación
- **Para otros estados:** El campo es opcional y puede usarse para cualquier observación adicional
- **En el listado:** Aparece un icono 💬 (globo de chat) junto al estado cuando hay observaciones. Al pasar el mouse sobre el icono, se muestra el contenido de las observaciones
