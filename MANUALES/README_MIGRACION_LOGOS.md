# Migración Masiva de Logos Antiguos al Nuevo Formato

## Problema Identificado

Después de implementar el sistema de logos estandarizado con extensión `.logo*`, los logos existentes que tenían nombres diferentes (como `comercializadora_logo.png` o su nombre original) dejaron de mostrarse porque el sistema busca específicamente archivos `.logo*`.

## Solución Implementada

Se ha creado una funcionalidad de **migración masiva de logos antiguos** que convierte automáticamente todos los logos existentes al nuevo formato `.logo*`. Esta funcionalidad está disponible **exclusivamente para el superadministrador**.

## Archivos Modificados

### 1. RepositorioService.cs

**Ubicación:** `EnerfoneCRM/Services/RepositorioService.cs`

**Métodos agregados:**

#### `MigrarLogosAntiguosANuevoFormato()`
Método público que ejecuta la migración completa de todos los logos en el repositorio.

**Funcionalidad:**
- Procesa las tres carpetas principales: **Energía** (comercializadoras), **Telefonía** (operadoras) y **Alarmas** (empresas)
- Busca en cada subcarpeta archivos de imagen que NO tengan el formato `.logo*`
- Retorna estadísticas: cantidad de migraciones exitosas, errores y detalles

**Extensiones de imagen reconocidas:**
- `.jpg`, `.jpeg`, `.png`, `.gif`, `.bmp`, `.svg`, `.webp`

#### `MigrarLogoEnCarpeta()`
Método privado que procesa una carpeta individual.

**Lógica:**
1. Verifica si ya existe un logo con formato `.logo*` → Si existe, no hace nada
2. Busca archivos de imagen antiguos (que no empiecen con `.logo`)
3. Renombra el primer archivo encontrado a `.logo` + su extensión
4. Si había múltiples archivos de imagen, elimina los duplicados
5. Retorna resultado de la operación con detalles

**Ejemplo de migración:**
```
Carpeta: Enerfone
Archivo antiguo: enerfone_logo.png
Archivo nuevo: .logo.png
```

### 2. Repositorio.razor

**Ubicación:** `EnerfoneCRM/Components/Pages/Repositorio.razor`

**Cambios realizados:**

#### Inyección de Servicio
```csharp
@inject RepositorioService RepositorioService
```

#### Botón de Migración (Solo para Superadmin)
```html
@if (AuthService.EsSuperAdmin)
{
    <button class="btn btn-warning" @onclick="MigrarLogosAntiguos" disabled="@migrandoLogos">
        @if (migrandoLogos)
        {
            <span class="spinner-border spinner-border-sm me-2"></span>
        }
        else
        {
            <i class="bi bi-arrow-repeat me-2"></i>
        }
        Migrar Logos Antiguos
    </button>
}
```

**Ubicación:** El botón aparece en la barra superior del repositorio, antes de los botones de administrador.

**Visibilidad:** Solo visible para el usuario `superadmin`.

#### Variables de Estado
```csharp
private bool migrandoLogos = false;
private bool mostrarResultadoMigracion = false;
private int migracionesExitosas = 0;
private int migracionesErrores = 0;
private List<string> detallesMigracion = new();
```

#### Método `MigrarLogosAntiguos()`
Método que se ejecuta al hacer clic en el botón:
1. Activa el estado de carga (`migrandoLogos = true`)
2. Llama a `RepositorioService.MigrarLogosAntiguosANuevoFormato()`
3. Guarda los resultados en las variables de estado
4. Muestra el modal con los resultados
5. Recarga el contenido del repositorio para mostrar los logos migrados

#### Modal de Resultados
Modal informativo que muestra:
- **Resumen:** Cantidad de migraciones exitosas y errores
- **Detalles:** Lista detallada de cada migración realizada
  - ✓ en verde para migraciones exitosas
  - ✗ en rojo para errores
  - Formato: `✓ NombreCarpeta: 'archivo_antiguo.png' → '.logo.png'`

### 3. ContratoService.cs

**Ubicación:** `EnerfoneCRM/Services/ContratoService.cs`

**Cambio:** Se agregó la columna `historico_liquidacion_id` a la consulta SQL en el método `ObtenerTodosPorTipoAsync()`.

**Motivo:** Evitar el error "The required column 'historico_liquidacion_id' was not present in the results of a 'FromSql' operation" que aparecía al cargar el dashboard después de agregar el nuevo campo a la BD.

```sql
usuario_comercializadora_id,
servicio_id,
historico_liquidacion_id,  -- <- NUEVO
potencia_contratada_p1,
```

## Cómo Usar la Funcionalidad

### Paso 1: Acceder como Superadmin
1. Iniciar sesión con el usuario **superadmin**
2. Navegar a **Repositorio** (menú lateral o `/repositorio`)

### Paso 2: Ejecutar Migración
1. En la parte superior derecha, verá el botón amarillo **"Migrar Logos Antiguos"**
2. Hacer clic en el botón
3. Esperar mientras se procesa la migración (aparece un spinner)

### Paso 3: Ver Resultados
Al finalizar, aparece automáticamente un modal con:
- **Resumen:** 
  ```
  ✓ Migraciones exitosas: 15
  ✗ Errores: 0
  ```
- **Detalles:** Lista de todas las carpetas procesadas
  ```
  ✓ Enerfone: 'enerfone_logo.png' → '.logo.png'
  ✓ Movistar: 'movistar_logo.jpeg' → '.logo.jpeg'
  ✓ Securitas: 'logo.png' → '.logo.png'
  ...
  ```

### Paso 4: Verificar Logos
- Los logos migrados aparecerán inmediatamente en las carpetas del repositorio
- El contenido se recarga automáticamente después de la migración

## Casos de Uso

### Caso 1: Logo con Nombre Estándar
**Antes:**
```
/Energía/Enerfone/
  - enerfone_logo.png
  - documento.pdf
```

**Después:**
```
/Energía/Enerfone/
  - .logo.png  (renombrado)
  - documento.pdf
```

### Caso 2: Logo con Nombre Personalizado
**Antes:**
```
/Telefonía/Movistar/
  - logo_empresa.jpg
  - contrato.pdf
```

**Después:**
```
/Telefonía/Movistar/
  - .logo.jpg  (renombrado)
  - contrato.pdf
```

### Caso 3: Múltiples Archivos de Imagen (Duplicados)
**Antes:**
```
/Alarmas/Securitas/
  - logo1.png
  - logo2.jpg
  - logo_viejo.png
  - info.pdf
```

**Después:**
```
/Alarmas/Securitas/
  - .logo.png  (renombrado desde logo1.png)
  - info.pdf
```
**Nota:** Los archivos `logo2.jpg` y `logo_viejo.png` se eliminan automáticamente.

### Caso 4: Ya Tiene Logo en Nuevo Formato
**Antes:**
```
/Energía/Naturgy/
  - .logo.png
  - documento.pdf
```

**Después:**
```
/Energía/Naturgy/
  - .logo.png  (sin cambios)
  - documento.pdf
```
**Nota:** No se realiza ningún cambio si ya existe un logo `.logo*`.

## Comportamiento Especial

### Carpetas Sin Logo
Si una carpeta no tiene ningún archivo de imagen, simplemente se omite sin generar error.

### Selección de Logo Cuando Hay Múltiples
Si hay varios archivos de imagen (sin formato `.logo*`):
1. Se toma el **primer archivo encontrado** alfabéticamente
2. Se renombra al formato `.logo*`
3. Los demás archivos de imagen se **eliminan automáticamente**

### Mantenimiento de Extensión Original
El sistema preserva la extensión original del archivo:
- `logo.png` → `.logo.png`
- `imagen.jpg` → `.logo.jpg`
- `icono.svg` → `.logo.svg`

## Seguridad

### Restricción de Acceso
- ✅ **Superadmin:** Puede ver y ejecutar la migración
- ❌ **Administrador:** NO puede ver el botón
- ❌ **Otros roles:** NO pueden ver el botón

### Validación
El método `MigrarLogosAntiguosANuevoFormato()` es público, pero el botón solo está visible para superadmin en la interfaz.

## Logs del Sistema

Durante la migración, el sistema registra en consola:

```
[RepositorioService] Iniciando migración de logos antiguos...
[RepositorioService] ✓ Enerfone: 'enerfone_logo.png' → '.logo.png'
[RepositorioService] Logo duplicado eliminado: logo2.jpg
[RepositorioService] ✓ Movistar: 'movistar_logo.jpeg' → '.logo.jpeg'
[RepositorioService] Migración completada: 15 exitosas, 0 errores
```

## Recomendaciones

### Cuándo Ejecutar la Migración
- **Una sola vez:** Después de actualizar a esta versión
- **Backup previo:** Recomendable hacer respaldo del directorio `storage/repositorio` antes de ejecutar
- **Fuera de horario laboral:** Para evitar interferencias con usuarios activos

### Después de la Migración
1. Verificar que todos los logos se muestren correctamente
2. Revisar el log de detalles para identificar posibles errores
3. No es necesario volver a ejecutar la migración a menos que se agreguen nuevos logos antiguos manualmente

## Reversión

Si necesitas revertir la migración:
1. Restaurar el backup del directorio `storage/repositorio`
2. Los archivos `.logo*` volverán a sus nombres originales

**Nota:** La reversión manual requiere renombrar los archivos `.logo*` a sus nombres originales.

## Pruebas Realizadas

✅ Migración de logos de comercializadoras  
✅ Migración de logos de operadoras  
✅ Migración de logos de empresas de alarma  
✅ Manejo de carpetas sin logos  
✅ Manejo de carpetas con logos ya migrados  
✅ Eliminación de logos duplicados  
✅ Preservación de extensiones originales  
✅ Restricción de acceso solo a superadmin  
✅ Visualización de resultados en modal  
✅ Recarga automática del repositorio  

## Próximos Pasos

Después de ejecutar la migración exitosamente:
1. Los logos antiguos estarán en formato `.logo*`
2. El sistema de visualización funcionará correctamente
3. Futuras subidas de logos ya usarán el formato estandarizado automáticamente
4. No será necesaria otra migración

---

**Fecha de implementación:** 6 de marzo de 2026  
**Versión:** 2.0  
**Desarrollador:** Sistema de migración automática
