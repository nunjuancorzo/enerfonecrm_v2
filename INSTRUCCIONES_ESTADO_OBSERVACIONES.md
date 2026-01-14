# Instrucciones para Agregar Estado a Observaciones

## Descripción
Se ha añadido la columna `estado_contrato` a la tabla `observaciones_contratos` para registrar el estado del contrato en el momento en que se creó cada observación.

## Cambios realizados

### 1. Modelo de datos
- Se agregó la propiedad `EstadoContrato` al modelo `ObservacionContrato.cs`
- Tipo: `string?` (nullable)
- Columna: `estado_contrato`
- Longitud máxima: 100 caracteres

### 2. Base de datos
- Nueva columna: `estado_contrato VARCHAR(100) NULL`
- Ubicación: después de la columna `fecha_hora`
- Las observaciones existentes se actualizan automáticamente con el estado actual del contrato

### 3. Funcionalidad
- Al crear una nueva observación, se guarda automáticamente el estado actual del contrato
- El estado se muestra como un badge (etiqueta) en el histórico de observaciones
- Aparece junto al nombre del usuario y la fecha/hora

## Pasos para la implementación

### 1. Ejecutar el script SQL

**IMPORTANTE:** Debes ejecutar el script SQL para agregar la nueva columna:

```bash
mysql -u root -p nombre_base_datos < Scripts/agregar_estado_observaciones.sql
```

O bien, conectarte a MySQL y ejecutar:

```sql
source /ruta/completa/Scripts/agregar_estado_observaciones.sql;
```

El script realizará:
1. Agregar la columna `estado_contrato` a la tabla `observaciones_contratos`
2. Actualizar las observaciones existentes con el estado actual del contrato relacionado

### 2. Verificar la creación de la columna

Conecta a MySQL y verifica que la columna se creó correctamente:

```sql
USE nombre_base_datos;
DESCRIBE observaciones_contratos;
SELECT id, id_contrato, usuario, estado_contrato, fecha_hora 
FROM observaciones_contratos 
LIMIT 10;
```

### 3. Reiniciar la aplicación

Después de ejecutar el script SQL, reinicia la aplicación para que los cambios surtan efecto:

```bash
# Detener la aplicación (Ctrl+C en el terminal donde corre)
# Luego ejecutar:
dotnet run
```

## Visualización en la interfaz

### En el modal de edición de contratos
- Las observaciones muestran el estado como un badge gris junto al nombre del usuario
- Ejemplo: "Juan Pérez [Activo] - 30/12/2025 14:30"

### En el modal de solo lectura (ícono de reloj)
- También muestra el estado como badge
- Permite ver en qué estado estaba el contrato cuando se hizo cada observación

## Archivos modificados

1. **Modelo**: `Models/ObservacionContrato.cs`
2. **Script SQL**: `Scripts/agregar_estado_observaciones.sql`
3. **Vistas**:
   - `Components/Pages/ContratosEnergia.razor`
   - `Components/Pages/ContratosTelefonia.razor`
   - `Components/Pages/ContratosAlarmas.razor`

## Notas importantes

- El estado se guarda en el momento de crear la observación
- Si se cambia el estado del contrato después, las observaciones anteriores mantienen el estado original
- Esto permite tener un histórico completo de la evolución del contrato
- El campo es opcional (nullable), por lo que observaciones antiguas pueden no tener estado
