# Funcionalidad: Control de Módulo SIPS

## Descripción
Se ha implementado un sistema de control para activar/desactivar el módulo SIPS desde la configuración de empresa. Esta funcionalidad está restringida exclusivamente al rol **SuperAdmin**.

## Cambios Realizados

### 1. Base de Datos
**Archivo**: `ADD_MODULO_SIPS_ACTIVO.sql`
- Se agregó el campo `modulo_sips_activo` (BOOLEAN) a la tabla `configuracion_empresa`
- Valor por defecto: `TRUE`

### 2. Modelo
**Archivo**: `EnerfoneCRM/Models/ConfiguracionEmpresa.cs`
- Se agregó la propiedad `ModuloSipsActivo` (bool, default: true)

### 3. Servicio
**Archivo**: `EnerfoneCRM/Services/ConfiguracionService.cs`
- Se actualizó el método `GuardarConfiguracionAsync` para incluir el campo `ModuloSipsActivo`

### 4. Interfaz de Configuración
**Archivo**: `EnerfoneCRM/Components/Pages/Configuracion.razor`
- Se agregó un checkbox para controlar el módulo SIPS
- El checkbox **solo es visible para usuarios con rol SuperAdmin**
- Incluye badge "Solo SuperAdmin" y descripción explicativa

### 5. Páginas SIPS
**Archivos**: 
- `EnerfoneCRM/Components/Pages/SIPS.razor`
- `EnerfoneCRM/Components/Pages/SipsHistorico.razor`

Cambios en ambas páginas:
- Se agregó el campo `moduloActivo` en las variables del componente
- Se verifica el estado del módulo al cargar la página
- Si el módulo está desactivado, se muestra una pantalla de mantenimiento con:
  - Icono de herramientas
  - Mensaje informativo
  - Botón para volver al inicio

## Uso

### Activar/Desactivar el Módulo SIPS

1. **Acceso**: Solo usuarios con rol **SuperAdmin** pueden ver y modificar esta opción
2. **Ubicación**: Configuración de Empresa → Control de Módulos
3. **Acción**: 
   - Marcar el checkbox para activar el módulo
   - Desmarcar para desactivar
   - Guardar cambios

### Comportamiento

#### Cuando el módulo está ACTIVADO:
- Los usuarios SuperAdmin pueden acceder a `/sips` y `/sips/historico` normalmente
- Todas las funcionalidades SIPS están disponibles

#### Cuando el módulo está DESACTIVADO:
- Al intentar acceder a `/sips` o `/sips/historico`, se muestra:
  - Pantalla de mantenimiento
  - Mensaje: "El módulo SIPS se encuentra temporalmente desactivado"
  - Botón para volver al inicio
- Ningún usuario (incluyendo SuperAdmin) puede usar las funcionalidades SIPS

## Scripts SQL

Para aplicar los cambios en la base de datos:

```bash
mysql -u root -p[password] enerfonecrm < ADD_MODULO_SIPS_ACTIVO.sql
```

## Notas Técnicas

- El módulo SIPS (Sistema de Información de Puntos de Suministro) permite consultar datos de CUPS
- La restricción a SuperAdmin garantiza que solo el administrador principal pueda controlar este módulo
- El campo se guarda automáticamente al actualizar la configuración de la empresa
- La verificación se realiza en cada carga de las páginas SIPS para máxima seguridad

## Casos de Uso

1. **Mantenimiento programado**: Desactivar temporalmente mientras se realizan actualizaciones
2. **Control de cuota**: Limitar el acceso cuando se alcanza la cuota mensual de consultas SIPS
3. **Pruebas**: Desactivar en entornos específicos sin modificar código
4. **Seguridad**: Bloquear acceso rápidamente en caso de problemas detectados

## Integración con Otros Módulos

- Similar al control implementado para el módulo de Liquidaciones
- Sigue el mismo patrón de diseño para consistencia
- La configuración se carga desde la base de datos en cada acceso a las páginas SIPS
