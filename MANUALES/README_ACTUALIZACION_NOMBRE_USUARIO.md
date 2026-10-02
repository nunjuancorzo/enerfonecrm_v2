# Actualización en Cascada del Nombre de Usuario

## Descripción General

Cuando se modifica el nombre de usuario (`NombreUsuario`) de un usuario en la ficha de usuario, el sistema actualiza automáticamente todas las referencias a ese nombre de usuario en toda la base de datos. Esto garantiza la integridad de los datos y evita inconsistencias.

## Implementación

La lógica de actualización en cascada está implementada en el método `ActualizarAsync` del servicio `UsuarioService.cs` (líneas ~88-211).

### Flujo de Actualización

1. **Verificación de Cambio**: El sistema detecta si el `NombreUsuario` ha cambiado comparando con el valor anterior en la base de datos.

2. **Validaciones**: Antes de actualizar, se verifica que:
   - No exista otro usuario con el nuevo nombre de usuario
   - No exista otro usuario con el nuevo email

3. **Actualización en Cascada**: Si el nombre de usuario cambió, se actualizan automáticamente las siguientes tablas:

## Tablas Afectadas

### 1. **Contratos** (`contratos`)
- **Campo**: `Comercial`
- **Descripción**: Almacena el nombre del usuario comercial responsable del contrato
- **Ejemplo**: Si el usuario "juan.perez" cambia a "j.perez", todos los contratos con `Comercial = "juan.perez"` se actualizarán a `Comercial = "j.perez"`

### 2. **Clientes** (`clientes`)
- **Campo**: `NombreUsuario`
- **Descripción**: Almacena el nombre del usuario que gestiona el cliente
- **Ejemplo**: Todos los clientes asignados al usuario se actualizan con el nuevo nombre

### 3. **Tareas Pendientes** (`tareas_pendientes`)
- **Campos**: 
  - `NombreUsuarioAsignado`: Usuario al que está asignada la tarea
  - `NombreUsuarioCreador`: Usuario que creó la tarea
- **Descripción**: Actualiza tanto las tareas asignadas como las creadas por el usuario

### 4. **Incidencias** (`incidencias`)
- **Campo**: `NombreUsuario`
- **Descripción**: Almacena el nombre del usuario relacionado con la incidencia

### 5. **Histórico de Liquidaciones** (`historico_liquidaciones`)
- **Campos**:
  - `UsuarioNombre`: Nombre del usuario cuyas liquidaciones se registran
  - `AprobadoPorNombre`: Nombre del usuario que aprobó la liquidación
- **Descripción**: Actualiza tanto las liquidaciones del usuario como las que el usuario ha aprobado

### 6. **Log de Accesos** (`log_accesos`)
- **Campo**: `NombreUsuario`
- **Descripción**: Registros históricos de accesos del usuario al sistema

### 7. **Observaciones de Contratos** (`observaciones_contratos`)
- **Campo**: `Usuario`
- **Descripción**: Nombre del usuario que escribió la observación en un contrato

### 8. **Log de Activaciones de Contratos** (`log_activaciones_contratos`)
- **Campo**: `Usuario`
- **Descripción**: Nombre del usuario que registró la activación del contrato

## Ejemplo de Uso

### Escenario
Un usuario llamado "maria.garcia" necesita cambiar su nombre de usuario a "m.garcia" por política de la empresa.

### Proceso
1. El administrador accede a **Gestión de Usuarios** → **Editar Usuario**
2. Cambia el campo "Nombre de Usuario" de "maria.garcia" a "m.garcia"
3. Guarda los cambios

### Resultado
El sistema actualiza automáticamente:
- 45 contratos donde `Comercial = "maria.garcia"` → `Comercial = "m.garcia"`
- 12 clientes donde `NombreUsuario = "maria.garcia"` → `NombreUsuario = "m.garcia"`
- 8 tareas asignadas/creadas por "maria.garcia"
- 3 incidencias relacionadas
- 15 registros de liquidaciones (tanto como usuario como aprobador)
- 120 registros de log de accesos
- 28 observaciones de contratos
- 10 logs de activación

El usuario verá un mensaje de confirmación:
> "Usuario actualizado exitosamente. Se actualizaron todas las referencias de 'maria.garcia' a 'm.garcia'"

## Logs de Consola

El sistema registra en la consola del servidor cada actualización:

```
[UsuarioService] Actualizando referencias de 'maria.garcia' a 'm.garcia'
[UsuarioService] Actualizados 45 contratos
[UsuarioService] Actualizados 12 clientes
[UsuarioService] Actualizadas 8 tareas pendientes
[UsuarioService] Actualizadas 3 incidencias
[UsuarioService] Actualizadas 15 liquidaciones
[UsuarioService] Actualizados 120 registros de acceso
[UsuarioService] Actualizadas 28 observaciones de contratos
[UsuarioService] Actualizados 10 logs de activación
```

## Integridad de Datos

### Transaccionalidad
Todas las actualizaciones se realizan dentro de una única transacción de Entity Framework. Si alguna actualización falla, se revierte toda la operación para mantener la integridad de los datos.

### Campos NO Afectados
Los siguientes campos relacionados con usuarios **NO** se actualizan porque usan IDs en lugar de nombres:
- `UsuarioId` en cualquier tabla
- `DirectorComercialId`, `GestorId`, `JefeVentasId` en la tabla `usuarios`
- `UsuarioColaboradorId`, `UsuarioAdministradorId` en `incidencias_liquidacion`
- Foreign keys que referencian IDs de usuario

## Consideraciones de Rendimiento

- La actualización puede tardar varios segundos si el usuario tiene muchos registros asociados
- El sistema muestra un spinner de carga mientras se procesa la actualización
- Se recomienda realizar cambios de nombre de usuario en horarios de baja actividad si el usuario tiene miles de registros

## Seguridad

- Solo los usuarios con rol **Administrador** pueden modificar nombres de usuario
- Los usuarios normales no pueden cambiar su propio nombre de usuario
- El superadmin no se puede modificar

## Pruebas Recomendadas

Para verificar el correcto funcionamiento:

1. Crear un usuario de prueba
2. Asignarle algunos contratos, clientes y tareas
3. Cambiar su nombre de usuario
4. Verificar en la base de datos que todos los registros se actualizaron:
   ```sql
   SELECT 'Contratos' AS Tabla, COUNT(*) AS Total FROM contratos WHERE Comercial = 'nuevo_nombre';
   SELECT 'Clientes' AS Tabla, COUNT(*) AS Total FROM clientes WHERE NombreUsuario = 'nuevo_nombre';
   SELECT 'Tareas Asignadas' AS Tabla, COUNT(*) AS Total FROM tareas_pendientes WHERE NombreUsuarioAsignado = 'nuevo_nombre';
   SELECT 'Tareas Creadas' AS Tabla, COUNT(*) AS Total FROM tareas_pendientes WHERE NombreUsuarioCreador = 'nuevo_nombre';
   SELECT 'Incidencias' AS Tabla, COUNT(*) AS Total FROM incidencias WHERE NombreUsuario = 'nuevo_nombre';
   SELECT 'Liquidaciones Usuario' AS Tabla, COUNT(*) AS Total FROM historico_liquidaciones WHERE UsuarioNombre = 'nuevo_nombre';
   SELECT 'Liquidaciones Aprobador' AS Tabla, COUNT(*) AS Total FROM historico_liquidaciones WHERE AprobadoPorNombre = 'nuevo_nombre';
   SELECT 'Log Accesos' AS Tabla, COUNT(*) AS Total FROM log_accesos WHERE NombreUsuario = 'nuevo_nombre';
   SELECT 'Observaciones' AS Tabla, COUNT(*) AS Total FROM observaciones_contratos WHERE Usuario = 'nuevo_nombre';
   SELECT 'Log Activaciones' AS Tabla, COUNT(*) AS Total FROM log_activaciones_contratos WHERE Usuario = 'nuevo_nombre';
   ```

## Código Relevante

### Archivo Principal
- **Ruta**: `/EnerfoneCRM/Services/UsuarioService.cs`
- **Método**: `ActualizarAsync(Usuario usuario)`
- **Líneas**: ~88-211

### Componentes de UI
- **Modal de Edición**: `/EnerfoneCRM/Components/Pages/Usuarios/ModalEditarUsuario.razor`
- **Lista de Usuarios**: `/EnerfoneCRM/Components/Pages/Usuarios/ListaUsuarios.razor`

## Historial de Cambios

- **Fecha**: 6 de marzo de 2026
- **Versión**: 2.0
- **Cambios**: Implementada actualización en cascada completa del nombre de usuario en 8 tablas de la base de datos
