# Sistema de Permisos para el Rol Backoffice

## ⚠️ IMPORTANTE: Configuración Requerida

Para que un usuario Backoffice funcione correctamente, **DEBE tener los permisos habilitados en la base de datos**. Sin esto, aunque el código esté correcto, no podrá acceder a los módulos.

### Verificación Rápida

Ejecuta el script [VERIFICAR_PERMISOS_BACKOFFICE.sql](VERIFICAR_PERMISOS_BACKOFFICE.sql) para:
1. Ver los permisos actuales del usuario Backoffice
2. Habilitar todos los permisos necesarios
3. Verificar que los cambios se aplicaron correctamente

## Descripción General

El rol **Backoffice** funciona como un "Administrador con permisos granulares". Si un usuario con rol Backoffice tiene acceso a un módulo específico, tendrá **los mismos permisos que un Administrador** dentro de ese módulo.

## Permisos Granulares

Los permisos se configuran en la tabla `usuarios` mediante campos booleanos:

| Campo | Módulo | Permisos al activarlo |
|-------|--------|----------------------|
| `puede_ver_clientes` | **Clientes** | Ver, crear, editar y eliminar clientes |
| `puede_ver_contratos` | **Contratos** (Energía, Telefonía, Alarmas) | Ver, crear, editar y dar de baja contratos |
| `puede_ver_tarifas` | **Tarifas y Catálogos** | Ver y gestionar tarifas, operadoras, comercializadoras, empresas de alarmas, servicios |
| `puede_ver_liquidaciones` | **Liquidaciones** | Ver TODAS las liquidaciones, aprobar, gestionar incidencias, cambiar estados |
| `puede_ver_sips` | **SIPS** | Ver y gestionar consultas SIPS |
| `puede_ver_incidencias` | **Incidencias** | Ver TODAS las incidencias, editar, responder |
| `puede_ver_ofertas` | **Ofertas** | Ver TODAS las ofertas, editar estados y comentarios |

⚠️ **IMPORTANTE - Seguridad de Usuarios:**
- El campo `puede_ver_usuarios` **NO aplica a Backoffice**
- La gestión de usuarios es **exclusiva de Administrador**
- Backoffice nunca puede crear, editar o ver usuarios, independientemente de permisos
- Esto es por seguridad: evita que usuarios Backoffice se otorguen permisos a sí mismos

## Diferencias con el Rol Colaborador

### Colaborador
- **Liquidaciones**: Solo ve sus propias liquidaciones
- **Contratos**: Solo ve los contratos que él creó
- **Ofertas**: Solo ve sus propias ofertas

### Backoffice con permisos
- **Liquidaciones**: Ve TODAS las liquidaciones de todos los usuarios
- **Contratos**: Ve TODOS los contratos
- **Ofertas**: Ve TODAS las ofertas

**La diferencia clave**: El Backoffice con acceso a un módulo tiene **visión y permisos globales**, igual que un Administrador.

## Implementación Técnica

### En AuthService.cs

Se han agregado métodos de gestión:

```csharp
// Verificar si puede GESTIONAR (permisos administrativos)
public bool PuedeGestionarClientes => EsAdministrador || (EsBackoffice && _usuarioActual?.PuedeVerClientes == true);
public bool PuedeGestionarContratos => EsAdministrador || (EsBackoffice && _usuarioActual?.PuedeVerContratos == true);
public bool PuedeGestionarTarifas => EsAdministrador || (EsBackoffice && _usuarioActual?.PuedeVerTarifas == true);
public bool PuedeGestionarLiquidaciones => EsAdministrador || (EsBackoffice && _usuarioActual?.PuedeVerLiquidaciones == true);
public bool PuedeGestionarSips => EsAdministrador || (EsBackoffice && _usuarioActual?.PuedeVerSips == true);
public bool PuedeGestionarIncidencias => EsAdministrador || (EsBackoffice && _usuarioActual?.PuedeVerIncidencias == true);
public bool PuedeGestionarOfertas => EsAdministrador || (EsBackoffice && _usuarioActual?.PuedeVerOfertas == true);
public bool PuedeGestionarUsuarios => EsAdministrador || (EsBackoffice && _usuarioActual?.PuedeVerUsuarios == true);
```

### En las páginas .razor

**Antes:**
```csharp
@if (AuthService.EsAdministrador)
{
    <button>Eliminar</button>
}
```

**Ahora:**
```csharp
@if (AuthService.PuedeGestionarClientes)
{
    <button>Eliminar</button>
}
```

## Ejemplo de Configuración de Usuario Backoffice

Para crear un usuario Backoffice que solo pueda gestionar liquidaciones e incidencias:

```sql
UPDATE usuarios SET
    rol = 'Backoffice',
    puede_ver_liquidaciones = 1,
    puede_ver_incidencias = 1,
    puede_ver_clientes = 0,
    puede_ver_contratos = 0,
    puede_ver_tarifas = 0,
    puede_ver_sips = 0,
    puede_ver_ofertas = 0,
    puede_ver_usuarios = 0
WHERE id = 123;
```

Ese usuario podrá:
- ✅ Ver y gestionar TODAS las liquidaciones (aprobar, cambiar estados, gestionar incidencias)
- ✅ Ver y gestionar TODAS las incidencias (editar, responder)
- ❌ No podrá acceder a Clientes, Contratos, Tarifas, SIPS, Ofertas ni Usuarios

## Archivos Modificados

### Páginas de Gestión General
1. **EnerfoneCRM/Services/AuthService.cs** - Agregados métodos `PuedeGestionar*`
2. **EnerfoneCRM/Components/Pages/Configuracion.razor** - Permite Administrador y Backoffice
3. **EnerfoneCRM/Components/Pages/ConfiguracionComisiones.razor** - Permite Administrador y Backoffice
4. **EnerfoneCRM/Components/Pages/Usuarios/ListaUsuarios.razor** - Usa `PuedeGestionarUsuarios`

### Módulos de Datos
5. **EnerfoneCRM/Components/Pages/Liquidaciones.razor** - Usa `PuedeGestionarLiquidaciones` y `PuedeVerLiquidaciones`
6. **EnerfoneCRM/Components/Pages/Clientes.razor** - Usa `PuedeGestionarClientes`
7. **EnerfoneCRM/Components/Pages/ContratosEnergia.razor** - Usa `PuedeGestionarContratos`
8. **EnerfoneCRM/Components/Pages/ContratosTelefonia.razor** - Usa `PuedeGestionarContratos`
9. **EnerfoneCRM/Components/Pages/ContratosAlarmas.razor** - Usa `PuedeGestionarContratos`
10. **EnerfoneCRM/Components/Pages/Incidencias.razor** - Usa `PuedeGestionarIncidencias`
11. **EnerfoneCRM/Components/Pages/Ofertas.razor** - Usa `PuedeGestionarOfertas`

### Catálogos y Tarifas
12. **EnerfoneCRM/Components/Pages/Operadoras.razor** - Usa `PuedeGestionarTarifas`
13. **EnerfoneCRM/Components/Pages/Comercializadoras.razor** - Usa `PuedeGestionarTarifas`
14. **EnerfoneCRM/Components/Pages/EmpresasAlarmas.razor** - Usa `PuedeGestionarTarifas`
15. **EnerfoneCRM/Components/Pages/Servicios.razor** - Usa `PuedeGestionarTarifas`

## Notas Importantes

- El rol **Administrador** mantiene acceso completo a todo sin restricciones
- El rol **SuperAdmin** es invisible en logs y tiene acceso total
- Los módulos sin permisos granulares (como Configuración, Log de Accesos) siguen siendo exclusivos de Administrador
- El menú de navegación ya usa `PuedeVer*` para mostrar u ocultar opciones
