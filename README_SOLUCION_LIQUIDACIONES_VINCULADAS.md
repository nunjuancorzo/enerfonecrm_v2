# Solución: Vinculación de Contratos a Liquidaciones Específicas

## Problema
Cuando había liquidaciones aceptadas no liquidadas y más contratos pasaban a estado "Act/Facturable", el sistema acumulaba incorrectamente las comisiones de todos los contratos liquidados del usuario en cada liquidación, sin distinguir a qué liquidación pertenecía cada contrato.

## Solución Implementada
Se ha creado una **relación directa entre contratos y liquidaciones** mediante un nuevo campo `historico_liquidacion_id` en la tabla `contratos`. Esto permite:

1. Vincular cada contrato a la liquidación específica en la que fue aprobado
2. Calcular correctamente el total de comisiones de cada liquidación
3. Reactivar solo los contratos de una liquidación específica, no todos los del usuario
4. Mantener un historial preciso de qué contratos pertenecen a cada liquidación

## Cambios Realizados

### 1. Base de Datos
**Archivo:** `ADD_HISTORICO_LIQUIDACION_ID_CONTRATOS.sql`

- Agrega columna `historico_liquidacion_id INT NULL` a la tabla `contratos`
- Crea índice para mejorar rendimiento de consultas
- Incluye opción para agregar foreign key constraint (comentado por compatibilidad)

**Instrucciones de aplicación:**
```bash
# Conectar a MySQL
mysql -u root -p

# Ejecutar el script
source ADD_HISTORICO_LIQUIDACION_ID_CONTRATOS.sql
```

O desde la línea de comandos:
```bash
mysql -u root -p corcrmv2 < ADD_HISTORICO_LIQUIDACION_ID_CONTRATOS.sql
```

### 2. Modelo de Datos
**Archivo:** `EnerfoneCRM/Models/Contrato.cs`

- Agregada propiedad `HistoricoLiquidacionId` (nullable)
- Mapeo con atributo `[Column("historico_liquidacion_id")]`

### 3. Lógica de Aprobación de Liquidaciones
**Archivo:** `EnerfoneCRM/Components/Pages/Liquidaciones.razor` - Método `AprobarLiquidacion()`

**Cambios:**
- Primero se guarda el histórico de liquidación para obtener su ID
- Luego se vinculan los contratos al histórico estableciendo `HistoricoLiquidacionId`
- Los contratos cambian de estado "Act/Facturable" a "Liquidado" junto con su vinculación
- Para superadmin (modo sin histórico), solo cambia el estado sin vínculo

**Flujo actualizado:**
1. Consultar contratos "Act/Facturable" del usuario
2. Crear registro en `HistoricoLiquidaciones` con estado "Aceptada"
3. Guardar para obtener el ID generado
4. Actualizar cada contrato con:
   - `Estado = "Liquidado"`
   - `HistoricoLiquidacionId = historico.Id`

### 4. Cálculo de Comisiones
**Archivo:** `EnerfoneCRM/Components/Pages/Liquidaciones.razor` - Método `CargarLiquidacionesPorEstado()`

**Antes:**
```csharp
var totalComisiones = await context.Contratos
    .Where(c => c.Comercial == usuario.NombreUsuario
        && c.Estado == "Liquidado")
    .SumAsync(c => c.Comision ?? 0);
```
Esto sumaba TODOS los contratos liquidados del usuario, causando acumulación incorrecta.

**Ahora:**
```csharp
var totalComisiones = await context.Contratos
    .Where(c => c.HistoricoLiquidacionId == liq.Id)
    .SumAsync(c => c.Comision ?? 0);
```
Solo suma los contratos vinculados a la liquidación específica.

### 5. Reactivación de Liquidaciones
**Archivo:** `EnerfoneCRM/Components/Pages/Liquidaciones.razor` - Método `ReactivarLiquidacion()`

**Antes:**
- Tomaba TODOS los contratos "Liquidado" del usuario
- Causaba que al reactivar una liquidación se afectaran contratos de otras liquidaciones

**Ahora:**
- Solo toma los contratos vinculados a la liquidación específica: `WHERE HistoricoLiquidacionId == historicoId`
- Al reactivar, limpia el vínculo: `HistoricoLiquidacionId = null`
- Cambia estado a "Act/Facturable"

**Flujo actualizado:**
1. Buscar contratos con `HistoricoLiquidacionId == historicoId` y estado "Liquidado"
2. Cambiar estado a "Act/Facturable"
3. Establecer `HistoricoLiquidacionId = null`
4. Eliminar el registro de `HistoricoLiquidaciones`

## Comportamiento Esperado Después de la Solución

### Escenario 1: Múltiples Liquidaciones del Mismo Usuario
1. Usuario tiene contratos A, B, C en "Act/Facturable"
2. Se aprueba → Liquidación 1 con comisiones de A+B+C
3. Llegan contratos D, E a "Act/Facturable"
4. Se aprueba → Liquidación 2 con comisiones de D+E (¡correcto!)
5. Liquidación 1 mantiene solo comisiones de A+B+C
6. Liquidación 2 mantiene solo comisiones de D+E

### Escenario 2: Reactivación de Liquidación Específica
1. Usuario tiene Liquidación 1 (contratos A, B) y Liquidación 2 (contratos C, D)
2. Se reactiva Liquidación 2
3. Solo contratos C y D vuelven a "Act/Facturable"
4. Contratos A y B permanecen en "Liquidado" vinculados a Liquidación 1

### Escenario 3: Liquidaciones Aceptadas Pendientes de Pago
1. Usuario tiene Liquidación 1 con estado "Aceptada" (aprobada pero no pagada)
2. Nuevos contratos pasan a "Act/Facturable"
3. Al aprobar, se crea Liquidación 2 SEPARADA
4. Cada liquidación mantiene su total de comisiones independiente
5. No hay acumulación ni solapamiento

## Validación de la Solución

Para verificar que la solución funciona correctamente:

1. **Verificar vinculación de contratos:**
```sql
SELECT c.id, c.estado, c.historico_liquidacion_id, c.comision
FROM contratos c
WHERE c.comercial = 'nombre_usuario'
ORDER BY c.historico_liquidacion_id;
```

2. **Verificar totales de liquidaciones:**
```sql
SELECT 
    hl.id,
    hl.usuario_nombre,
    hl.cantidad_contratos,
    hl.total_comisiones,
    COUNT(c.id) AS contratos_vinculados,
    SUM(c.comision) AS comisiones_reales
FROM historico_liquidaciones hl
LEFT JOIN contratos c ON c.historico_liquidacion_id = hl.id
WHERE hl.usuario_id = ?
GROUP BY hl.id;
```

3. **Verificar independencia entre liquidaciones:**
- Crear liquidación A con 3 contratos
- Crear liquidación B con 2 contratos del mismo usuario
- Verificar que cada liquidación muestre solo sus comisiones correspondientes

## Migración de Datos Existentes (Opcional)

Para liquidaciones históricas que no tienen contratos vinculados, podrías ejecutar un script de migración basándote en:
- Fecha de aprobación de la liquidación
- Estado "Liquidado" de contratos en ese período
- Usuario comercial

**Nota:** Este script debe diseñarse con cuidado según las reglas de negocio específicas.

## Beneficios de la Solución

1. ✅ **Precisión en cálculo de comisiones:** Cada liquidación muestra solo sus comisiones
2. ✅ **Trazabilidad:** Se puede identificar exactamente qué contratos fueron incluidos en cada liquidación
3. ✅ **Reactivación segura:** Solo afecta contratos de la liquidación específica
4. ✅ **Escalabilidad:** Permite múltiples liquidaciones del mismo usuario sin conflictos
5. ✅ **Auditoría:** Historial claro de vinculaciones contrato-liquidación
6. ✅ **Prevención de duplicados:** Un contrato solo puede pertenecer a una liquidación

## Compatibilidad con Datos Existentes

- El campo `historico_liquidacion_id` es **nullable**
- Contratos existentes sin vínculo funcionarán normalmente
- Solo nuevas aprobaciones crearán el vínculo
- El cálculo de `TotalComisiones` solo usa contratos vinculados, por lo que liquidaciones antiguas podrían recalcularse como 0 si no tienen contratos vinculados

**Recomendación:** Si hay liquidaciones históricas importantes, considera ejecutar un script de migración para vincular sus contratos basándose en fechas y estados.
