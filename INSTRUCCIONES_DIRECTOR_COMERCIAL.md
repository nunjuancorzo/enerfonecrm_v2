# Instrucciones: Nuevo Rol "Director Comercial"

## Descripción

Se ha añadido el rol **"Director comercial"** al sistema. Este rol está en la jerarquía entre "Jefe de ventas" y "Administrador".

## Permisos del Director Comercial

El Director comercial puede ver y gestionar contratos de:

1. **Jefes de ventas asignados**: Puede tener asignados varios Jefes de ventas
2. **Gestores de los Jefes de ventas**: Ve los contratos de los gestores que dependen de sus Jefes de ventas asignados
3. **Colaboradores de los Jefes de ventas**: Ve los contratos de los colaboradores directos de sus Jefes de ventas
4. **Colaboradores de los Gestores**: Ve los contratos de los colaboradores asignados a los gestores de sus Jefes de ventas
5. **Gestores asignados directamente**: Puede tener gestores asignados directamente sin pasar por un Jefe de ventas
6. **Colaboradores de esos Gestores**: Ve los contratos de los colaboradores de los gestores asignados directamente
7. **Colaboradores asignados directamente**: Puede tener colaboradores asignados directamente
8. **Sus propios contratos**: Como cualquier usuario comercial

### Restricciones

- **NO tiene acceso** a funciones de administración como:
  - Gestión de tarifas de energía
  - Gestión de tarifas de telefonía
  - Configuración de la empresa
  - Gestión de usuarios
  - Otras funciones administrativas

- Solo puede **editar y gestionar contratos**, no eliminarlos (solo los Administradores pueden eliminar)

## Instalación

### 1. Ejecutar el script SQL

Debes ejecutar el siguiente script en tu base de datos MySQL para añadir el campo necesario:

```bash
mysql -u tu_usuario -p enerfonecrm < EnerfoneCRM/Data/DirectorComercial.sql
```

O ejecutar manualmente en MySQL:

```sql
USE enerfonecrm;

ALTER TABLE usuarios 
ADD COLUMN director_comercial_id INT NULL AFTER jefe_ventas_id;

ALTER TABLE usuarios
ADD INDEX idx_director_comercial (director_comercial_id);
```

### 2. Verificar los cambios

Comprueba que el campo se ha añadido correctamente:

```sql
DESCRIBE usuarios;
```

Deberías ver la columna `director_comercial_id` después de `jefe_ventas_id`.

## Uso en el Sistema

### Creación de un Director Comercial

1. Ve a la sección de **Usuarios**
2. Haz clic en **Nuevo Usuario**
3. Selecciona el rol **"Director comercial"**
4. Completa los datos del usuario
5. (Opcional) Asigna Jefes de ventas, Gestores o Colaboradores desde los checkboxes correspondientes

### Asignar usuarios a un Director Comercial

Al crear o editar un **Jefe de ventas**, **Gestor** o **Colaborador**:

1. En el formulario de usuario, verás un nuevo campo: **"Director Comercial Asignado"**
2. Selecciona el Director comercial correspondiente
3. Guarda los cambios

**Importante**: Un usuario solo puede tener asignado UNO de los siguientes supervisores:
- Gestor
- Jefe de ventas
- Director comercial

Al seleccionar uno, los otros se limpiarán automáticamente.

## Jerarquía Completa

```
Administrador (acceso total)
    ↓
Director comercial (gestiona equipos completos)
    ↓
Jefe de ventas (gestiona gestores y colaboradores)
    ↓
Gestor (gestiona colaboradores)
    ↓
Colaborador (nivel base)
```

## Filtrado de Contratos

El sistema filtra automáticamente los contratos según el rol:

- **Administrador**: Ve TODOS los contratos
- **Director comercial**: Ve contratos de su equipo completo (cascada jerárquica)
- **Jefe de ventas**: Ve contratos de sus gestores y colaboradores
- **Gestor**: Ve contratos de sus colaboradores
- **Colaborador**: Ve solo sus propios contratos

## Archivos Modificados

1. **EnerfoneCRM/Data/DirectorComercial.sql** - Script de base de datos
2. **EnerfoneCRM/Models/Usuario.cs** - Añadido campo `DirectorComercialId`
3. **EnerfoneCRM/Services/AuthService.cs** - Añadida propiedad `EsDirectorComercial` y `EsJefeVentas`
4. **EnerfoneCRM/Services/ContratoService.cs** - Lógica de filtrado jerárquico para Director comercial
5. **EnerfoneCRM/Components/Pages/Usuarios/ModalCrearUsuario.razor** - Campo de selección de Director comercial
6. **EnerfoneCRM/Components/Pages/Usuarios/ModalEditarUsuario.razor** - Campo de selección de Director comercial

## Notas Importantes

- El filtrado es **automático** - no necesitas hacer nada especial
- La jerarquía es **flexible** - un Director comercial puede tener usuarios asignados directamente o a través de Jefes de ventas
- El sistema **previene duplicados** al obtener la lista de usuarios subordinados
- Los permisos son **restrictivos** - el Director comercial no puede acceder a funciones de administrador

## Verificación

Después de ejecutar el script SQL, reinicia la aplicación y:

1. Crea un usuario con rol "Director comercial"
2. Asigna un Jefe de ventas a ese Director comercial
3. Inicia sesión con el Director comercial
4. Verifica que puedes ver los contratos del Jefe de ventas y sus subordinados

## Solución de Problemas

### No aparece el rol "Director comercial" en el dropdown
- Verifica que has recargado la página (Ctrl+F5 / Cmd+Shift+R)
- Comprueba que no hay errores de compilación

### No se filtran bien los contratos
- Verifica que el script SQL se ejecutó correctamente
- Comprueba que los usuarios tienen asignado correctamente el `director_comercial_id`
- Revisa la consola del navegador y los logs del servidor

### Error al guardar usuario
- Asegúrate de que el script SQL se ejecutó sin errores
- Verifica que la columna `director_comercial_id` existe en la tabla `usuarios`
