# Módulo de Servicios Energía - Inicio Rápido

## Descripción

Se ha añadido un nuevo apartado en **Inicio Rápido** llamado **"Servicios Energía"** que permite la importación y exportación masiva de servicios desde/hacia archivos Excel.

## Ubicación

**Menú** → Inicio Rápido → **Servicios Energía** (Tarjeta con fondo gris/secondary)

## Funcionalidades

### 1. Exportar Servicios Actuales
- Exporta todos los servicios existentes en la base de datos a un archivo Excel
- El archivo incluye el **ID** de cada servicio para poder actualizarlos posteriormente
- Formato del archivo: `servicios_exportados_YYYYMMDD_HHMMSS.xlsx`

**Cómo usar:**
1. Ir a Inicio Rápido
2. Buscar la tarjeta "Servicios Energía"
3. Hacer clic en **"Exportar Servicios Actuales (con ID)"**
4. El archivo se descargará automáticamente

### 2. Descargar Plantilla Vacía
- Genera una plantilla Excel con las columnas necesarias y ejemplos
- Útil para crear nuevos servicios desde cero

**Columnas de la plantilla:**
- **ID**: Campo opcional. Dejar vacío para nuevos servicios, incluir ID para actualizar existentes
- **TIPO**: Tipo de servicio (Energía, Telefonía, Alarmas, etc.)
- **NOMBRE_SERVICIO**: Nombre descriptivo del servicio
- **PRECIO**: Precio del servicio (formato: 15,00)
- **EMPRESA**: Empresa que ofrece el servicio (Naturgy, Iberdrola, etc.)

**Cómo usar:**
1. Hacer clic en **"Descargar Plantilla Vacía"**
2. Completar las filas con los datos de los servicios
3. Guardar el archivo

### 3. Importar Servicios
- Permite importar servicios desde un archivo Excel
- Puede crear nuevos servicios o actualizar existentes (si se incluye el ID)
- Muestra resultado detallado: número de importados y errores

**Cómo usar:**
1. Preparar archivo Excel (usando plantilla o exportación)
2. Hacer clic en **"Seleccionar archivo Excel"**
3. Seleccionar el archivo .xlsx
4. Hacer clic en **"Importar Servicios"**
5. Esperar el procesamiento (puede tardar algunos segundos)
6. Revisar el resultado mostrado

## Archivos Creados/Modificados

### Backend
1. `/exportar_servicios.py` - Script Python para exportar servicios
   - Lee servicios de la base de datos
   - Genera archivo Excel con formato
   - Incluye ID para actualizaciones

### Frontend
1. `/EnerfoneCRM/Components/Pages/InicioRapido.razor`
   - Agregadas variables: `archivoServicios`, `resultadoServicios`
   - Métodos nuevos:
     - `CargarArchivoServicios()`
     - `DescargarPlantillaServicios()`
     - `ImportarServicios()`
     - `ExportarServicios()`
   - Sección HTML nueva: Tarjeta "Servicios Energía"

2. `/EnerfoneCRM/wwwroot/js/descargarPlantillas.js`
   - Función `descargarPlantillaServicios()`
   - Genera plantilla con ejemplos de servicios

## Script de Importación

El script `importar_tarifas_servicios.py` ya existente soporta la importación de servicios con el parámetro `servicios`:

```bash
python3 importar_tarifas_servicios.py servicios plantilla_servicios.xlsx
```

## Estructura de Datos

### Modelo: Servicio
```csharp
public class Servicio
{
    public int Id { get; set; }
    public string Tipo { get; set; }
    public string NombreServicio { get; set; }
    public string Precio { get; set; }
    public string? Empresa { get; set; }
}
```

### Tabla: servicios
- `id` (int, PK)
- `tipo` (varchar 50, requerido)
- `nombreServicio` (varchar 100, requerido)
- `precio` (varchar 50, requerido)
- `empresa` (varchar 100, opcional)

## Ejemplos de Uso

### Ejemplo 1: Importar Nuevos Servicios
1. Descargar plantilla vacía
2. Completar (sin ID):
   ```
   | ID | TIPO     | NOMBRE_SERVICIO          | PRECIO | EMPRESA  |
   |----|----------|--------------------------|--------|----------|
   |    | Energía  | Mantenimiento Caldera    | 15,00  | Naturgy  |
   |    | Energía  | Seguro Hogar Eléctrico   | 8,50   | Iberdrola|
   ```
3. Importar archivo
4. Resultado: 2 servicios creados

### Ejemplo 2: Actualizar Servicios Existentes
1. Exportar servicios actuales
2. Modificar precios o datos en el Excel:
   ```
   | ID  | TIPO     | NOMBRE_SERVICIO          | PRECIO | EMPRESA  |
   |-----|----------|--------------------------|--------|----------|
   | 1   | Energía  | Mantenimiento Caldera    | 18,00  | Naturgy  |
   | 2   | Energía  | Seguro Hogar Eléctrico   | 10,00  | Iberdrola|
   ```
3. Importar archivo
4. Resultado: 2 servicios actualizados

### Ejemplo 3: Mezclar Nuevos y Actualizaciones
```
| ID  | TIPO      | NOMBRE_SERVICIO           | PRECIO | EMPRESA          |
|-----|-----------|---------------------------|--------|------------------|
| 1   | Energía   | Mantenimiento Caldera     | 20,00  | Naturgy          |
|     | Telefonía | Servicio Técnico Premium  | 5,00   | Movistar         |
|     | Alarmas   | Mantenimiento Anual       | 25,00  | Securitas Direct |
```
- Servicio ID 1: se actualiza
- Otros 2: se crean como nuevos

## Validaciones

- Los campos **TIPO**, **NOMBRE_SERVICIO** y **PRECIO** son obligatorios
- El archivo debe ser .xlsx o .xls
- Tamaño máximo: 10 MB
- Si hay errores, se muestran en detalle (hasta 10 primeros)

## Notas Técnicas

- El script de exportación usa el mismo formato que la plantilla
- La importación usa el script existente `importar_tarifas_servicios.py` con parámetro `servicios`
- Los archivos temporales se eliminan automáticamente después del procesamiento
- La aplicación lee la configuración de BD automáticamente de `appsettings.Production.json`

## Fecha de Implementación

13 de marzo de 2026
