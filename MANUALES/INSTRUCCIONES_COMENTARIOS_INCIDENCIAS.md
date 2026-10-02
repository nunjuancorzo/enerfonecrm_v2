# Sistema de Comentarios de Incidencias

**Fecha**: 27 de marzo de 2026

## Resumen

Se ha implementado un sistema completo de comentarios para las incidencias, permitiendo a los administradores y usuarios Backoffice interactuar con los usuarios que reportan incidencias mediante comentarios con opción de envío de email automático.

## Cambios Implementados

### 1. Nuevo Estado de Incidencia: "En validación"

Se ha agregado el estado "En validación" a las incidencias, permitiendo una mejor categorización del flujo de trabajo.

**Estados disponibles:**
- Pendiente
- Pendiente subir PRO
- **En validación** ✨ (NUEVO)
- En Proceso
- Resuelta
- Cerrada

### 2. Sistema de Comentarios

#### Características:
- **Múltiples comentarios por incidencia**: Historial completo de interacciones
- **Solo Administradores y Backoffice** pueden agregar comentarios
- **Envío opcional de email** al usuario que creó la incidencia
- **Indicador visual** de emails enviados (icono verde)
- **Timestamp** de cada comentario
- **Interfaz intuitiva** con scroll para muchos comentarios

#### Modelo de Datos:
- **Tabla**: `comentarios_incidencias`
- **Campos**:
  - `id`: Identificador único
  - `incidencia_id`: Referencia a la incidencia
  - `usuario_id`: Usuario que hizo el comentario
  - `nombre_usuario`: Nombre mostrado
  - `comentario`: Texto (hasta 2000 caracteres)
  - `fecha_creacion`: Timestamp
  - `email_enviado`: Booleano

### 3. Notificaciones por Email

Cuando se agrega un comentario con la opción de enviar email:
- Se envía automáticamente al usuario que creó la incidencia
- Email HTML formateado profesionalmente
- Incluye información de la incidencia
- Muestra el comentario completo
- Marca el comentario como "Email enviado"

## Instalación

### Paso 1: Ejecutar Script SQL

```bash
# Conectarse a MySQL
mysql -u tu_usuario -p

# Usar la base de datos
USE enerfone_crm;

# Ejecutar el script (desde MySQL CLI)
source /ruta/al/proyecto/ADD_COMENTARIOS_INCIDENCIAS.sql;

# O copiar y pegar el contenido del archivo SQL
```

**El script creará:**
- Tabla `comentarios_incidencias`
- Índices para optimización
- Claves foráneas para integridad referencial

### Paso 2: Reiniciar la Aplicación

El servidor ya está corriendo con los cambios aplicados.

## Uso

### Para Administradores/Backoffice:

#### Visualizar Incidencias:
1. Ir a **Gestión de Incidencias**
2. Filtrar por estado "En validación" si es necesario
3. Hacer clic en el botón "Ver Detalle" de cualquier incidencia

#### Ver Comentarios:
- En el modal de detalle, después de las observaciones del administrador
- Se muestra el historial completo de comentarios
- Los comentarios con email enviado tienen un icono verde ✉

#### Agregar Comentario:
1. En el modal de detalle, scroll hasta la sección "Comentarios"
2. Escribir el comentario en el textarea
3. **Opcional**: Marcar checkbox "Enviar notificación por email al usuario"
4. Hacer clic en "Agregar comentario"

#### Cambiar Estado a "En validación":
1. En la tabla de incidencias, hacer clic en el botón "Editar" (lápiz)
2. Seleccionar "En validación" en el desplegable de Estado
3. **Opcional**: Agregar observaciones del administrador
4. Guardar cambios

### Para Usuarios Regulares:

- **Solo lectura**: Los usuarios que crearon la incidencia pueden ver sus comentarios
- **Notificaciones**: Reciben emails cuando hay nuevos comentarios (si el admin lo activa)
- **No pueden comentar**: Solo administradores/backoffice pueden agregar comentarios

## Estructura del Email

El email enviado incluye:

```html
💬 Nuevo Comentario en tu Incidencia
- ID de Incidencia: #123
- Asunto: [Asunto de la incidencia]
- Estado: [Estado actual]

Comentario de [Nombre Administrador]:
[Texto del comentario]

Fecha: 27/03/2026 15:30
```

## Flujo de Trabajo Recomendado

### Caso 1: Incidencia Requiere Validación
1. Usuario reporta incidencia (Estado: Pendiente)
2. Admin revisa → Cambia a "En validación"
3. Admin agrega comentario: "Estamos revisando tu caso, necesitamos más información..."
4. Marca checkbox para enviar email
5. Usuario recibe notificación

### Caso 2: Seguimiento de Incidencia
1. Usuario creó incidencia hace días
2. Admin verifica progreso → Agrega comentario sin cambiar estado
3. Comentario: "Hemos identificado el problema, trabajando en solución..."
4. Envía email para mantener informado al usuario

### Caso 3: Resolución con Explicación
1. Incidencia resuelta técnicamente
2. Admin cambia estado a "Resuelta"
3. Agrega comentario explicando la solución
4. Envía email de cierre

## Permisos

| Rol | Ver Incidencias | Ver Comentarios | Agregar Comentarios | Cambiar Estado |
|-----|----------------|-----------------|---------------------|----------------|
| Administrador | Todas | ✅ | ✅ | ✅ |
| Backoffice | Todas | ✅ | ✅ | ✅ |
| Otros usuarios | Solo propias | ✅ | ❌ | ❌ |

## Archivos Modificados

1. **Models/ComentarioIncidencia.cs** (NUEVO)
   - Modelo de datos para comentarios

2. **Data/ApplicationDbContext.cs**
   - Agregado `DbSet<ComentarioIncidencia>`

3. **Components/Pages/Incidencias.razor**
   - Agregado estado "En validación" en filtros
   - Agregado selector de estado "En validación"
   - Sección completa de comentarios en modal de detalle
   - Formulario para agregar comentarios
   - Checkbox para envío de email
   - Métodos: `CargarComentarios()`, `AgregarComentario()`, `EnviarEmailComentario()`
   - Template HTML para email de comentario

4. **ADD_COMENTARIOS_INCIDENCIAS.sql** (NUEVO)
   - Script SQL para crear tabla y estructura

## Verificación

### Comprobar que la tabla existe:
```sql
DESC comentarios_incidencias;
```

### Ver comentarios de una incidencia:
```sql
SELECT * FROM comentarios_incidencias 
WHERE incidencia_id = 1 
ORDER BY fecha_creacion;
```

### Estadísticas de uso:
```sql
SELECT 
    COUNT(*) AS total_comentarios,
    SUM(email_enviado) AS emails_enviados,
    COUNT(DISTINCT incidencia_id) AS incidencias_con_comentarios
FROM comentarios_incidencias;
```

## Mejoras Futuras (Opcional)

- [ ] Permitir adjuntar archivos a comentarios
- [ ] Notificaciones en tiempo real (SignalR)
- [ ] Editar/Eliminar comentarios propios
- [ ] Menciones a otros administradores (@usuario)
- [ ] Plantillas de comentarios predefinidas
- [ ] Exportar historial de comentarios a PDF

## Notas Importantes

⚠️ **Ejecutar el script SQL antes de usar la funcionalidad**
- El servidor compilará pero fallará al intentar usar comentarios si la tabla no existe

✅ **Backup automático**
- El script no modifica datos existentes
- Solo agrega la nueva tabla

🔒 **Seguridad**
- Los comentarios están vinculados a incidencias con foreign keys
- Al eliminar una incidencia, sus comentarios se eliminan automáticamente (CASCADE)

📧 **Configuración de Email**
- Asegurarse de que el servicio EmailService esté configurado correctamente
- Los emails se envían de forma asíncrona, no bloquean la UI

## Soporte

Para problemas o preguntas:
1. Verificar que el script SQL se ejecutó correctamente
2. Comprobar logs de consola del servidor
3. Verificar que el rol del usuario tiene los permisos adecuados
