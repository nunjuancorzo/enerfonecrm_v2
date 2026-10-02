# Instrucciones para Agregar Histórico de Observaciones

## Descripción
Se ha implementado un sistema de histórico de observaciones para los contratos que **reemplaza** el campo `ObservacionesEstado`. Cada observación registra:
- Texto de la observación
- Usuario que la realizó
- Fecha y hora de creación

**Cambios importantes:**
- El campo `ObservacionesEstado` ya no se muestra en los formularios
- Las observaciones existentes en ese campo se migran automáticamente al histórico
- Todas las observaciones se gestionan ahora desde el histórico
- En el listado de contratos, el ícono de reloj muestra el histórico completo (solo lectura)
- Las observaciones solo se pueden agregar/editar desde el botón "Editar" del contrato

## Pasos para la implementación

### 1. Ejecutar el script SQL

**IMPORTANTE:** El script incluye la migración automática de observaciones existentes.

Debes ejecutar el script SQL para crear la nueva tabla y migrar datos:

```bash
mysql -u root -p nombre_base_datos < Scripts/crear_tabla_observaciones_contratos.sql
```

O bien, conectarte a MySQL y ejecutar:

```sql
source /ruta/completa/Scripts/crear_tabla_observaciones_contratos.sql;
```

El script realizará:
1. Crear la tabla `observaciones_contratos`
2. Migrar automáticamente todas las observaciones existentes del campo `observaciones_estado` al histórico
3. Asignar como usuario el comercial del contrato o "Sistema" si no hay comercial
4. Usar la fecha de creación del contrato como fecha de la observación

### 2. Verificar la creación de la tabla

Conecta a MySQL y verifica que la tabla se creó correctamente:

```sql
USE nombre_base_datos;
SHOW TABLES LIKE 'observaciones_contratos';
DESCRIBE observaciones_contratos;
```

### 3. Funcionalidades implementadas

#### Desde el listado de contratos:

- **Ícono de reloj**: Al lado del estado de cada contrato aparece un ícono de reloj
- **Solo lectura**: Al hacer clic, se abre un modal con el histórico completo de observaciones
- **No editable**: Desde el listado solo se puede visualizar, no editar

#### En el modal de edición de contratos (ContratosTelefonia.razor):

- **Campo ObservacionesEstado eliminado**: Ya no aparece en el formulario
- **Sección de histórico**: Muestra todas las observaciones del contrato
- **Visualización completa**: Lista con todas las observaciones ordenadas por fecha (más recientes primero)
- **Agregar observación**: Botón para agregar nuevas observaciones (solo en modo edición)
- **Información mostrada**: 
  - Texto de la observación
  - Usuario que la creó
  - Fecha y hora
- **Eliminar observación**: Solo el usuario que creó la observación puede eliminarla

#### En el formulario de creación (Clientes.razor):

- **Campo ObservacionesEstado eliminado**: Ya no aparece en el formulario de creación
- Las observaciones se agregan después de crear el contrato, desde el modo edición

#### Archivos modificados/creados:

1. **Modelo**: `Models/ObservacionContrato.cs`
2. **Servicio**: `Services/ObservacionContratoService.cs`
3. **Script SQL**: `Scripts/crear_tabla_observaciones_contratos.sql`
4. **Contexto**: `Data/ApplicationDbContext.cs` (agregado DbSet)
5. **Program.cs**: (registrado el servicio)
6. **Vista**: `Components/Pages/ContratosTelefonia.razor` (UI completa)

## Estructura de la tabla

```sql
observaciones_contratos
├── id (INT, AUTO_INCREMENT, PRIMARY KEY)
├── id_contrato (INT, NOT NULL, FOREIGN KEY)
├── observacion (TEXT, NOT NULL)
├── usuario (VARCHAR(100), NOT NULL)
└── fecha_hora (DATETIME, NOT NULL, DEFAULT CURRENT_TIMESTAMP)
```

## Características de seguridad

- La relación con contratos tiene `ON DELETE CASCADE`, lo que significa que al eliminar un contrato, todas sus observaciones se eliminan automáticamente
- Solo el usuario que creó una observación puede eliminarla
- Las observaciones requieren autenticación (se usa el usuario actual de la sesión)

## Notas

- El histórico se muestra en un contenedor con scroll cuando hay muchas observaciones
- Las observaciones se actualizan automáticamente al agregar o eliminar
- La interfaz es responsive y sigue el diseño del resto de la aplicación
