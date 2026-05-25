# Funcionalidad: Desactivar Módulo de Liquidaciones

## Descripción
Se ha implementado la capacidad de desactivar temporalmente el módulo de liquidaciones desde la configuración de empresa. Cuando está desactivado, ningún usuario (incluidos administradores) puede acceder al módulo y se muestra un mensaje de "Módulo en Mantenimiento".

## Cambios Realizados

### 1. Base de Datos
**Archivo**: `ADD_DESACTIVAR_MODULO_LIQUIDACIONES.sql`
- Se agregó el campo `modulo_liquidaciones_activo` (BOOLEAN) a la tabla `configuracion_empresa`
- Valor por defecto: `TRUE` (módulo activo)

### 2. Modelo
**Archivo**: `Models/ConfiguracionEmpresa.cs`
- Se agregó la propiedad `ModuloLiquidacionesActivo` (bool)
- Valor por defecto: `true`

### 3. Interfaz de Configuración
**Archivo**: `Components/Pages/Configuracion.razor`
- Se agregó una nueva sección "Control de Módulos" con un switch para activar/desactivar el módulo de liquidaciones
- Incluye texto descriptivo explicando el comportamiento

### 4. Página de Liquidaciones
**Archivo**: `Components/Pages/Liquidaciones.razor`
- Se agregó validación al cargar la página para verificar si el módulo está activo
- Si está desactivado, se muestra una pantalla de "Módulo en Mantenimiento" con:
  - Icono de herramientas
  - Mensaje explicativo
  - Botón para volver al inicio
  - Mensaje en el footer para contactar soporte

## Uso

1. **Activar/Desactivar el módulo**:
   - Ir a "Configuración de Empresa" (solo administradores)
   - En la sección "Control de Módulos", activar/desactivar el switch "Módulo de Liquidaciones"
   - Guardar cambios

2. **Comportamiento cuando está desactivado**:
   - Todos los usuarios que intenten acceder a `/liquidaciones` verán la pantalla de mantenimiento
   - No se permite ninguna operación en el módulo
   - El sistema muestra un mensaje amigable y profesional

3. **Aplicación del script SQL**:
   ```bash
   mysql -u usuario -p nombre_base_datos < ADD_DESACTIVAR_MODULO_LIQUIDACIONES.sql
   ```

## Notas Técnicas
- La validación se realiza después de verificar autenticación y permisos
- Se usa el DbContextProvider para acceder a la configuración
- El estado se almacena en la variable `moduloActivo` del componente
- El módulo por defecto está ACTIVO para no interrumpir el funcionamiento actual

## Casos de Uso
- **Mantenimiento programado**: Desactivar el módulo durante actualizaciones del sistema de liquidaciones
- **Periodos de cierre**: Desactivar temporalmente mientras se realizan cierres contables
- **Resolución de incidencias**: Desactivar mientras se corrigen problemas técnicos
- **Formación**: Desactivar en entornos de prueba para evitar operaciones no deseadas
