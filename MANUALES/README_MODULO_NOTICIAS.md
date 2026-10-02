# Cambios en el Módulo de Noticias

## Resumen de Cambios

Se han realizado las siguientes modificaciones en el módulo "Mensajes de Bienvenida" (ahora "Noticias"):

### 1. Cambio de Nomenclatura
- **Antes:** "Mensajes de Bienvenida"
- **Ahora:** "Noticias"
- **Ubicación en menú:** Movido de "Configuración" a "Gestiones"
- **Icono:** Cambiado a megáfono (`bi-megaphone`)

### 2. Cambios Funcionales

#### Contenido Opcional
- El campo "Contenido" ya **NO es obligatorio**
- Solo el campo "Título" es obligatorio
- Se pueden crear noticias solo con imágenes

#### Múltiples Imágenes por Noticia
- Se puede agregar **múltiples imágenes** a cada noticia
- Las imágenes se ordenan automáticamente
- Límite: 10 imágenes por carga
- Tamaño máximo por imagen: 50MB
- Formatos soportados: JPG, PNG, GIF

#### Carrusel de Imágenes
- Las noticias con múltiples imágenes se muestran en un **carrusel interactivo** en el popup de inicio
- Controles de navegación (anterior/siguiente)
- Indicadores de posición
- Soporte para descripciones por imagen (opcional)

### 3. Cambios en la Base de Datos

#### Nueva Tabla: `noticias_imagenes`
```sql
CREATE TABLE noticias_imagenes (
    id INT AUTO_INCREMENT PRIMARY KEY,
    mensaje_id INT NOT NULL,
    imagen_url VARCHAR(500) NOT NULL,
    orden INT DEFAULT 0,
    descripcion VARCHAR(255) NULL,
    fecha_creacion DATETIME DEFAULT CURRENT_TIMESTAMP
);
```

**Nota:** La columna `imagen_url` en la tabla `mensajes_bienvenida` se mantiene por compatibilidad pero ya no se usa.

### 4. Archivos Modificados

#### Nuevos Archivos
- `/Models/NoticiaImagen.cs` - Modelo para imágenes de noticias
- `/ADD_NOTICIAS_IMAGENES.sql` - Script de creación de tabla y migración

#### Archivos Modificados
- `/Models/MensajeBienvenida.cs` - Contenido opcional, relación con imágenes
- `/Data/ApplicationDbContext.cs` - Agregado DbSet para imágenes
- `/Services/MensajeBienvenidaService.cs` - Métodos para gestión de imágenes
- `/Components/Pages/MensajesBienvenida.razor` - Formulario con múltiples imágenes
- `/Components/Layout/PopupBienvenida.razor` - Carrusel de imágenes
- `/Components/Layout/NavMenu.razor` - Ubicación y nombre del menú

## Instrucciones de Instalación

### 1. Ejecutar Script SQL
```bash
mysql -u usuario -p nombre_base_datos < ADD_NOTICIAS_IMAGENES.sql
```

O desde MySQL:
```sql
source /ruta/al/archivo/ADD_NOTICIAS_IMAGENES.sql;
```

### 2. Verificar Migración
El script migrará automáticamente las imágenes existentes de la columna `imagen_url` a la nueva tabla `noticias_imagenes`.

### 3. Reiniciar Aplicación
```bash
dotnet run
```

## Uso

### Crear una Nueva Noticia
1. Ir a **Gestiones → Noticias**
2. Click en "Nuevo Mensaje"
3. Llenar el **Título** (obligatorio)
4. Opcionalmente agregar **Contenido**
5. Click en "Seleccionar imágenes" para agregar una o varias imágenes
6. Configurar fechas y prioridad
7. Guardar

### Editar Noticia Existente
1. Click en el icono de edición
2. Las imágenes existentes se muestran en la sección "Imágenes actuales"
3. Puedes eliminar imágenes existentes o agregar nuevas
4. Guardar cambios

### Visualización
- Las noticias se muestran automáticamente en el inicio
- Si hay una sola imagen, se muestra centrada
- Si hay múltiples imágenes, se muestra un carrusel interactivo
- Los usuarios pueden marcar noticias como "No volver a mostrar hoy"

## Notas Técnicas

- Las imágenes se almacenan en `/storage/uploads/mensajes/`
- El orden de las imágenes se mantiene mediante el campo `orden`
- La eliminación de noticias elimina en cascada las imágenes asociadas
- Se mantiene compatibilidad con la URL antigua `/mensajes-bienvenida`
- La nueva URL es `/noticias`

## Compatibilidad
- ✅ Compatible con imágenes existentes (se migran automáticamente)
- ✅ Mantiene URLs antiguas por retrocompatibilidad
- ✅ No requiere cambios en datos existentes
