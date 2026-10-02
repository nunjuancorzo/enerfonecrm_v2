# Correcciones al Sistema de Liquidaciones

## Problemas Identificados

### Problema 1: Las comisiones mostradas no corresponden al usuario
**Descripción:** Cuando un gestor, jefe de ventas o director comercial visualiza sus contratos en la liquidación, se mostraba la comisión del colaborador que creó el contrato, no la comisión que le corresponde según su rol en la jerarquía.

**Solución Implementada:**
- Se ha creado un diccionario `comisionesPorContrato` que almacena la comisión específica que le corresponde a cada usuario por cada contrato
- Se ha implementado el método `ObtenerComisionUsuario(int contratoId)` que devuelve la comisión correcta desde el diccionario
- Se han modificado las tres tablas de contratos (Energía, Telefonía y Alarmas) para mostrar la comisión correcta usando `ObtenerComisionUsuario(contrato.Id)` en lugar de `contrato.Comision`

**Resultado:** Ahora cada usuario verá en sus contratos la comisión exacta que le corresponde según su porcentaje configurado.

### Problema 2: No se generan liquidaciones para todos los roles
**Descripción:** No se estaban generando liquidaciones para el director comercial y el gestor.

**Diagnóstico Implementado:**
Se han agregado logs detallados en múltiples puntos para diagnosticar el problema:

1. **En `AprobarLiquidacion`:**
   - Log de la jerarquía del usuario al inicio
   - Log detallado de cada contrato procesado mostrando las comisiones calculadas para cada rol
   - Log específico cuando se agrega o no se agrega un gestor al diccionario de usuarios con comisión
   - Log específico cuando se agrega o no se agrega un director comercial
   - Log de resumen mostrando todos los usuarios que recibirán liquidación con sus totales

2. **En `ComisionService.CalcularDistribucionComisionAsync`:**
   - Log de los usuarios de jerarquía obtenidos (gestor, jefe, director)
   - Log de los porcentajes que se aplicarán a cada rol

## Archivos Modificados

1. **EnerfoneCRM/Components/Pages/Liquidaciones.razor**
   - Agregado diccionario `comisionesPorContrato`
   - Modificado método `VerDetalleUsuario` para calcular y guardar comisiones específicas
   - Agregado método `ObtenerComisionUsuario`
   - Modificadas las tres tablas de contratos para mostrar comisiones correctas
   - Agregados logs detallados en `AprobarLiquidacion`

2. **EnerfoneCRM/Services/ComisionService.cs**
   - Agregados logs detallados en `CalcularDistribucionComisionAsync`

3. **DEBUG_LIQUIDACIONES.sql** (nuevo archivo)
   - Script SQL para diagnosticar el sistema de liquidaciones
   - Verifica jerarquías de usuarios
   - Verifica configuración de comisiones
   - Muestra contratos facturables
   - Muestra liquidaciones históricas
   - Muestra detalles de comisión

## Cómo Verificar la Corrección

### 1. Verificar las Comisiones Mostradas
1. Accede a Liquidaciones
2. Haz clic en "Ver Contratos" de un gestor/jefe/director que tenga contratos de colaboradores bajo su cargo
3. Verifica que las comisiones mostradas sean las correctas según el porcentaje configurado

### 2. Diagnosticar por qué no se generan liquidaciones para algunos roles

**Ejecutar el script de diagnóstico:**
```bash
mysql -u usuario -p nombre_base_datos < DEBUG_LIQUIDACIONES.sql
```

**Verificar específicamente:**

1. **Jerarquía del colaborador** (Query 1):
   - Verifica que el colaborador tenga asignado un `gestor_id` y `director_comercial_id`
   - Si están en NULL, el colaborador no tiene asignados estos roles

2. **Configuración de comisiones** (Query 2):
   - Verifica que exista configuración de comisiones para el colaborador
   - Verifica que los porcentajes de gestor y director comercial NO sean 0 o NULL
   - La suma de porcentajes debe ser <= 100%

3. **Ver logs de la consola:**
   - Cuando pongas contratos en Act/Facturable y los apruebes, revisa la consola del navegador
   - Busca los logs que empiezan con `[LIQUIDACIONES]` y `[ComisionService]`
   - Verifica qué usuarios se están agregando al diccionario de comisiones
   - Verifica si hay advertencias sobre usuarios no encontrados o inactivos

### Causas Comunes de que NO se Generen Liquidaciones

1. **El colaborador no tiene asignado el rol en su jerarquía**
   - Solución: Editar el usuario colaborador y asignar gestor_id o director_comercial_id

2. **El porcentaje configurado es 0 o NULL**
   - Solución: En la configuración de comisiones, asignar un porcentaje > 0 para gestor/director

3. **El usuario gestor/director está inactivo**
   - Solución: Activar el usuario en la tabla usuarios

4. **No existe configuración de comisiones para el colaborador**
   - Solución: Crear una configuración de comisiones con porcentajes para todos los roles

## Próximos Pasos

1. Ejecutar el script DEBUG_LIQUIDACIONES.sql y revisar los resultados
2. Corregir cualquier problema encontrado (jerarquía faltante, porcentajes en 0, usuarios inactivos)
3. Poner 2 contratos en Act/Facturable
4. Revisar los logs de la consola durante la aprobación
5. Verificar que se creen liquidaciones para TODOS los roles configurados
6. Verificar que las comisiones mostradas sean las correctas para cada usuario
