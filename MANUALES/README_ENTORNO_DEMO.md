# Configuración de Entorno Demo

## Descripción

El sistema ahora soporta múltiples entornos basados en el hostname:

- **enerfonecrm.com** → Base de datos: `enerfone_pre` (Producción)
- **demo.enerfonecrm.com** → Base de datos: `demoenerfone` (Demo)

## Características

1. **Selección Automática de Base de Datos**: El sistema detecta el hostname y selecciona automáticamente la base de datos correspondiente.

2. **Indicador Visual**: Cuando se accede al entorno demo, se muestra un banner amarillo en la parte superior con el texto "🧪 Entorno Demo".

3. **Mismo Código**: Ambos entornos usan el mismo código publicado.

## Configuración del Servidor

### Paso 1: Crear la Base de Datos Demo

Ejecutar el script SQL:
```bash
mysql -u enerfone -p < Scripts/crear_base_datos_demo.sql
```

### Paso 2: Copiar Datos de Producción a Demo (Opcional)

Si deseas que demo tenga los mismos datos que producción:

```bash
# Exportar datos de producción
mysqldump -u enerfone -p enerfone_pre > backup_enerfone_pre.sql

# Importar a demo
mysql -u enerfone -p demoenerfone < backup_enerfone_pre.sql
```

### Paso 3: Configurar web.config

El web.config ya está configurado correctamente. No requiere cambios adicionales.

### Paso 4: Configurar IIS/Web Server

Crear dos sitios o bindings:

**Sitio 1: Producción**
- Hostname: `enerfonecrm.com`
- Path: `/ruta/a/publicado`

**Sitio 2: Demo**
- Hostname: `demo.enerfonecrm.com`
- Path: `/ruta/a/publicado` (mismo directorio)

### Paso 5: Configurar DNS

Crear un registro CNAME o A para el subdominio demo:

```
demo.enerfonecrm.com → [IP del servidor]
```

## Archivos Modificados

1. **Services/EnvironmentService.cs** (nuevo): Servicio para detectar el entorno
2. **Program.cs**: Configuración dinámica de base de datos según hostname
3. **Components/Layout/MainLayout.razor**: Banner de entorno demo
4. **Scripts/crear_base_datos_demo.sql** (nuevo): Script de creación de BD demo

## Funcionamiento Técnico

### Detección de Entorno

```csharp
public bool IsDemo()
{
    var host = _httpContextAccessor.HttpContext?.Request.Host.Host;
    return host != null && host.StartsWith("demo.", StringComparison.OrdinalIgnoreCase);
}
```

### Selección de Base de Datos

```csharp
var databaseName = (host != null && host.StartsWith("demo.", StringComparison.OrdinalIgnoreCase)) 
    ? "demoenerfone" 
    : "enerfone_pre";
```

## Pruebas Locales

Para probar localmente, modifica el archivo `hosts`:

**Windows**: `C:\Windows\System32\drivers\etc\hosts`
**macOS/Linux**: `/etc/hosts`

Agregar:
```
127.0.0.1 demo.enerfonecrm.local
127.0.0.1 enerfonecrm.local
```

Y acceder a:
- `http://enerfonecrm.local:5169` → BD enerfone_pre
- `http://demo.enerfonecrm.local:5169` → BD demoenerfone

## Mantenimiento

### Actualizar Código

Solo necesitas publicar una vez. Ambos entornos usarán el mismo código:

```bash
dotnet publish -c Release -o ../publicado
```

### Limpiar Datos de Demo

Para resetear la base de datos demo:

```sql
DROP DATABASE demoenerfone;
```

Luego ejecutar de nuevo el script de creación.

### Sincronizar Datos

Para mantener demo sincronizado con producción (programar como tarea):

```bash
#!/bin/bash
mysqldump -u enerfone -p[password] enerfone_pre | mysql -u enerfone -p[password] demoenerfone
```

## Solución de Problemas

### Error: "Database 'demoenerfone' doesn't exist"

Asegúrate de haber ejecutado el script de creación de la base de datos.

### El banner "Entorno Demo" no aparece

Verifica que el hostname sea exactamente `demo.enerfonecrm.com` y que IIS esté configurado correctamente.

### Ambos entornos usan la misma base de datos

Revisa que el HttpContextAccessor esté funcionando correctamente y que el hostname se esté detectando bien.
