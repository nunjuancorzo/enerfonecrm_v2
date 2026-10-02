# Sistema de Rol Backoffice con Permisos Granulares

## 📋 Resumen

Se ha implementado un nuevo rol **"Backoffice"** en el sistema con las siguientes características:

- ✅ Permisos granulares por módulo (configurables por el Administrador)
- ✅ **NO recibe comisiones** - El Administrador se lleva el 100% de las comisiones
- ✅ Control total del Administrador sobre qué puede ver cada usuario Backoffice
- ✅ 8 módulos configurables independientemente
- ✅ **SOLO puede haber UN Administrador por sistema** - Todos los demás deben ser Backoffice u otros roles

## ⚠️ IMPORTANTE: Administrador Único

### Política de Administrador Único

**El sistema solo permite UN usuario con rol "Administrador"**:

- En la base de datos actual, el usuario `administrador` es el único administrador
- Todos los demás usuarios con rol "Administrador" deben migrarse a rol "Backoffice"
- No se puede crear un nuevo usuario con rol "Administrador" si ya existe uno
- No se puede cambiar el rol de un usuario a "Administrador" si ya existe uno

### Script de Migración

**Archivo**: `MIGRAR_ADMINISTRADORES_A_BACKOFFICE.sql`

Este script:
1. Identifica todos los usuarios con rol "Administrador
"
2. Cambia el rol a "Backoffice" para todos EXCEPTO el usuario `administrador`
3. Mantiene todos los permisos activados por defecto para los usuarios migrados

**Ejecución**:
```bash
mysql -u [usuario] -p enerfone_crm < MIGRAR_ADMINISTRADORES_A_BACKOFFICE.sql
```

### Validaciones Implementadas

El sistema valida automáticamente que solo puede haber un administrador en:

1. **Interfaz de usuario** (ModalEditarUsuario.razor):
   - Muestra error si se intenta cambiar a rol "Administrador" y ya existe uno
   - Mensaje: "Ya existe un usuario con rol Administrador. Solo puede haber un administrador por sistema."

2. **Servicio de backend** (UsuarioService.cs):
   - Método `CrearAsync`: Valida antes de crear
   - Método `ActualizarAsync`: Valida antes de actualizar
   - Retorna error con mensaje descriptivo si ya existe un administrador

## 🔧 Cambios Implementados

### 1. Modelo de Base de Datos

Se agregaron 8 nuevas columnas a la tabla `usuarios`:

| Columna | Tipo | Descripción |
|---------|------|-------------|
| `puede_ver_clientes` | BOOLEAN | Permiso para ver módulo de Clientes |
| `puede_ver_contratos` | BOOLEAN | Permiso para ver módulo de Contratos |
| `puede_ver_tarifas` | BOOLEAN | Permiso para ver módulo de Tarifas |
| `puede_ver_liquidaciones` | BOOLEAN | Permiso para ver módulo de Liquidaciones |
| `puede_ver_sips` | BOOLEAN | Permiso para ver módulo SIPS |
| `puede_ver_incidencias` | BOOLEAN | Permiso para ver módulo de Incidencias |
| `puede_ver_ofertas` | BOOLEAN | Permiso para ver módulo de Ofertas |
| `puede_ver_usuarios` | BOOLEAN | Permiso para ver gestión de Usuarios |

**Valores por defecto**: Todos los permisos están activados (`TRUE`) por defecto.

### 2. Código Modificado

#### Archivos Modificados:

1. **`Models/Usuario.cs`**
   - Agregadas 8 propiedades booleanas para los permisos
   - Documentadas con comentarios XML

2. **`Services/AuthService.cs`**
   - Nueva propiedad: `EsBackoffice` - Identifica si el usuario actual es Backoffice
   - Nuevas propiedades de verificación:
     - `PuedeVerClientes`
     - `PuedeVerContratos`
     - `PuedeVerTarifas`
     - `PuedeVerLiquidaciones`
     - `PuedeVerSips`
     - `PuedeVerIncidencias`
     - `PuedeVerOfertas`
     - `PuedeVerUsuarios`
   - **Lógica**: Si el rol NO es Backoffice, todos los permisos son `true`. Si es Backoffice, se verifica el permiso específico.

3. **`Services/ComisionService.cs`**
   - Modificado cálculo de comisiones (línea ~100)
   - Si el colaborador es Backoffice:
     - `pctColaborador = 0`
     - `pctGestor = 0`
     - `pctJefeVentas = 0`
     - `pctDirectorComercial = 0`
     - **Resultado**: El Administrador recibe el 100%

4. **`Components/Pages/Usuarios/ModalEditarUsuario.razor`**
   - Agregado "Backoffice" como opción en el selector de Rol
   - Nueva sección "Permisos de visualización" que aparece solo cuando `rol == "Backoffice"`
   - 8 checkboxes para configurar permisos individualmente
   - Diseño en 2 columnas con iconos Bootstrap

5. **`Components/Pages/Usuarios/ModalCrearUsuario.razor`**
   - Agregado "Backoffice" como opción en el selector de Rol

6. **`Components/Layout/NavMenu.razor`**
   - Aplicados filtros de permisos a todos los enlaces del menú:
     - **Clientes**: Solo visible si `PuedeVerClientes`
     - **Contratos**: Solo visible si `PuedeVerContratos`
     - **Tarifas**: Solo visible si `PuedeVerTarifas` (Backoffice puede ver tarifas si tiene permiso)
     - **Liquidaciones**: Solo visible si `PuedeVerLiquidaciones`
     - **Ofertas**: Solo visible si `PuedeVerOfertas`
     - **SIPS**: Solo visible si `PuedeVerSips`
     - **Incidencias**: Solo visible si `PuedeVerIncidencias`
     - **Gestión Usuarios**: Solo visible si `PuedeVerUsuarios`

### 3. Script SQL

**Archivo**: `ADD_ROL_BACKOFFICE_PERMISOS.sql`

- Verifica y agrega las 8 columnas de permisos si no existen
- Usa prepared statements para evitar errores si ya existen
- Incluye verificación final y mensajes informativos

## 📖 Instrucciones de Uso

### Paso 1: Ejecutar el Script SQL

```bash
# Desde la raíz del proyecto
mysql -u [usuario] -p enerfone_crm < ADD_ROL_BACKOFFICE_PERMISOS.sql
```

### Paso 2: Reiniciar el Servidor

El servidor detectará automáticamente las nuevas columnas gracias a Entity Framework.

### Paso 3: Crear un Usuario Backoffice

1. Ir a **Configuración** → **Usuarios** → **Gestión Usuarios**
2. Click en **"Crear Usuario"**
3. Rellenar los datos básicos
4. Seleccionar **"Backoffice"** en el campo Rol
5. Guardar el usuario

### Paso 4: Configurar Permisos

1. Ir a **Configuración** → **Usuarios** → **Gestión Usuarios**
2. Click en el botón **Editar** del usuario Backoffice creado
3. Scroll hacia abajo hasta la sección **"Permisos de Visualización (Rol Backoffice)"**
4. **Desmarcar** los módulos que NO quieras que el usuario vea
5. Guardar cambios

### Paso 5: Verificar

1. Cerrar sesión del Administrador
2. Iniciar sesión con el usuario Backoffice
3. Verificar que solo aparecen en el menú los módulos permitidos
4. Intentar acceder a un módulo deshabilitado (debería no aparecer en el menú)

## 🎯 Casos de Uso

### Caso 1: Backoffice solo para Clientes y Contratos

**Configuración**:
- ✅ `puede_ver_clientes` = TRUE
- ✅ `puede_ver_contratos` = TRUE
- ❌ `puede_ver_tarifas` = FALSE
- ❌ `puede_ver_liquidaciones` = FALSE
- ❌ `puede_ver_sips` = FALSE
- ❌ `puede_ver_incidencias` = FALSE
- ❌ `puede_ver_ofertas` = FALSE
- ❌ `puede_ver_usuarios` = FALSE

**Resultado**: El usuario solo verá en el menú:
- Inicio
- Clientes
- Contratos
- Noticias (siempre visible)
- Repositorio (siempre visible)
- Tareas (siempre visible)

### Caso 2: Backoffice para todo EXCEPTO SIPS

**Configuración**:
- ✅ Todos en TRUE excepto
- ❌ `puede_ver_sips` = FALSE

**Resultado**: El usuario verá todo el menú excepto la opción "SIPS"

### Caso 3: Backoffice sin acceso a Liquidaciones ni Usuarios

**Configuración**:
- ✅ Todos en TRUE excepto
- ❌ `puede_ver_liquidaciones` = FALSE
- ❌ `puede_ver_usuarios` = FALSE

**Resultado**: No podrá ver comisiones ni gestionar otros usuarios

## ⚠️ Consideraciones Importantes

### 1. Comisiones

- Los contratos registrados por usuarios **Backoffice NO generan comisiones para nadie excepto el Administrador**
- El cálculo es automático: Si el colaborador es Backoffice, el 100% va al Administrador
- No es necesario configurar nada adicional

### 2. Permisos por Defecto

- Al crear un usuario Backoffice, **todos los permisos están activados** por defecto
- Es responsabilidad del Administrador desactivar los que no correspondan

### 3. Compatibilidad con Otros Roles

- Los permisos **SOLO aplican al rol Backoffice**
- Otros roles (Administrador, Colaborador, Gestor, etc.) tienen el comportamiento habitual
- No se requieren cambios en usuarios existentes

### 4. Navegación Directa

⚠️ **Importante**: Aunque el menú oculta las opciones, si un usuario Backoffice conoce la URL y la escribe manualmente, podría acceder a módulos bloqueados.

**Recomendación**: En producción, agregar validaciones en las páginas para verificar permisos no solo en la UI sino también en el código de cada componente.

**Ejemplo de validación en una página**:

```razor
@page "/clientes"
@inject AuthService AuthService

@if (!AuthService.PuedeVerClientes)
{
    <div class="alert alert-danger">
        <i class="bi bi-shield-exclamation me-2"></i>
        No tienes permiso para ver este módulo.
    </div>
    return;
}

<!-- Resto del contenido de la página -->
```

## 🔒 Seguridad Adicional (Recomendado)

Para fortalecer la seguridad, se recomienda agregar validaciones en **cada página** principal:

### Páginas a Proteger:

1. `/clientes` → Verificar `PuedeVerClientes`
2. `/contratos/*` → Verificar `PuedeVerContratos`
3. `/tarifas/*` → Verificar `PuedeVerTarifas`
4. `/liquidaciones` → Verificar `PuedeVerLiquidaciones`
5. `/sips` → Verificar `PuedeVerSips`
6. `/incidencias` → Verificar `PuedeVerIncidencias`
7. `/ofertas` → Verificar `PuedeVerOfertas`
8. `/usuarios` → Verificar `PuedeVerUsuarios`

**Patrón a seguir**:

```razor
@code {
    protected override void OnInitialized()
    {
        if (!AuthService.PuedeVerXXX)
        {
            Navigation.NavigateTo("/acceso-denegado");
            return;
        }
        
        // Resto de la inicialización
    }
}
```

## 📊 Estructura de Datos

### Ejemplo de Usuario Backoffice en BD:

```sql
SELECT 
    username,
    rol,
    puede_ver_clientes,
    puede_ver_contratos,
    puede_ver_tarifas,
    puede_ver_liquidaciones,
    puede_ver_sips,
    puede_ver_incidencias,
    puede_ver_ofertas,
    puede_ver_usuarios
FROM usuarios
WHERE rol = 'Backoffice';
```

**Resultado esperado**:

| username | rol | clientes | contratos | tarifas | liquidaciones | sips | incidencias | ofertas | usuarios |
|----------|-----|----------|-----------|---------|---------------|------|-------------|---------|----------|
| backoffice1 | Backoffice | 1 | 1 | 0 | 0 | 0 | 1 | 1 | 0 |

## 🧪 Testing

### Checklist de Pruebas:

- [ ] Ejecutar script SQL sin errores
- [ ] Crear usuario con rol Backoffice
- [ ] Verificar que aparece sección de permisos en edición
- [ ] Desactivar permisos específicos y guardar
- [ ] Iniciar sesión como usuario Backoffice
- [ ] Verificar que menú solo muestra opciones permitidas
- [ ] Registrar un contrato como Backoffice
- [ ] Verificar en liquidaciones que el 100% va al Administrador
- [ ] Intentar acceso directo a URL bloqueada (debería fallar con validación adicional)

## 📝 Notas de Migración

Si ya tienes usuarios en producción:

1. **Antes de ejecutar el script**: Hacer backup de la BD
2. **Después de ejecutar el script**: Todos los usuarios tendrán los nuevos campos con valor `1` (TRUE)
3. **Para usuarios Backoffice existentes** (si los hay): Editar manualmente sus permisos desde la interfaz

## 🆘 Troubleshooting

### Problema: No aparece la sección de permisos al editar usuario

**Solución**: Verificar que el campo `Rol` esté seleccionado como "Backoffice" y guardar primero. Reabrir el modal de edición.

### Problema: Usuario Backoffice ve módulos bloqueados

**Solución**: 
1. Verificar que el script SQL se ejecutó correctamente
2. Verificar valores en BD directamente
3. Cerrar sesión y volver a iniciar
4. Limpiar caché del navegador

### Problema: Comisiones se siguen repartiendo aunque sea Backoffice

**Solución**:
1. Verificar que el campo `Comercial` del contrato corresponde al username del Backoffice
2. Verificar en logs del servidor el cálculo de comisiones
3. Ejecutar una nueva liquidación tras reiniciar el servidor

## 📚 Referencias

- **Modelo**: `EnerfoneCRM/Models/Usuario.cs`
- **Servicio**: `EnerfoneCRM/Services/AuthService.cs`
- **Comisiones**: `EnerfoneCRM/Services/ComisionService.cs`
- **UI**: `EnerfoneCRM/Components/Pages/Usuarios/ModalEditarUsuario.razor`
- **Navegación**: `EnerfoneCRM/Components/Layout/NavMenu.razor`
- **Migración**: `ADD_ROL_BACKOFFICE_PERMISOS.sql`

---

**Fecha de Implementación**: 26 de Marzo de 2026  
**Versión**: 1.0.0  
**Autor**: Sistema Automatizado de Desarrollo
