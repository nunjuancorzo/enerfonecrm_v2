# Sistema de Comisiones Jerárquicas y Decomisiones

## Descripción General

Este documento describe el sistema completo de comisiones jerárquicas y decomisiones implementado en EnerfoneCRM v2. El sistema permite la distribución automática de comisiones entre diferentes niveles de la jerarquía comercial y gestiona penalizaciones por bajas anticipadas de contratos.

## Tabla de Contenidos

1. [Arquitectura del Sistema](#arquitectura-del-sistema)
2. [Modelos de Datos](#modelos-de-datos)
3. [Distribución Jerárquica](#distribución-jerárquica)
4. [Penalizaciones y Decomisiones](#penalizaciones-y-decomisiones)
5. [Configuración](#configuración)
6. [Proceso de Liquidación](#proceso-de-liquidación)
7. [Instalación y Migración](#instalación-y-migración)

---

## Arquitectura del Sistema

### Componentes Principales

```
┌─────────────────────────────────────────────────────────┐
│                    CONTRATO                              │
│  - Tipo (Energía/Telefonía/Alarma)                      │
│  - Comercial (Usuario que lo registró)                  │
│  - Comisión Base (100%)                                 │
│  - Estado (Act/Facturable → Liquidado → Baja)          │
└────────────────────┬────────────────────────────────────┘
                     │
                     ▼
┌─────────────────────────────────────────────────────────┐
│              COMISION SERVICE                            │
│  - CalcularDistribucionComisionAsync()                  │
│  - VerificarPenalizacionAsync()                         │
│  - CrearDecomisionAsync()                               │
└────────────────────┬────────────────────────────────────┘
                     │
        ┌────────────┴────────────┐
        ▼                         ▼
┌──────────────────┐    ┌──────────────────┐
│ LIQUIDACIÓN      │    │ DECOMISIÓN       │
│ - Total por      │    │ - Por baja       │
│   usuario        │    │   anticipada     │
│ - Distribución   │    │ - Total o        │
│   jerárquica     │    │   Proporcional   │
└──────────────────┘    └──────────────────┘
```

---

## Modelos de Datos

### 1. ConfiguracionComision

Define los porcentajes de comisión personalizados por usuario y proveedor.

```csharp
public class ConfiguracionComision
{
    public int Id { get; set; }
    public int UsuarioId { get; set; }
    public string TipoProveedor { get; set; }  // "Comercializadora", "Operadora", "EmpresaAlarma"
    public int ProveedorId { get; set; }
    public string? NombreProveedor { get; set; }
    
    // Distribución de porcentajes (suma máxima 100%)
    public decimal PorcentajeColaborador { get; set; }
    public decimal? PorcentajeGestor { get; set; }
    public decimal? PorcentajeJefeVentas { get; set; }
    public decimal? PorcentajeDirectorComercial { get; set; }
    
    // Calculado automáticamente: 100% - suma de anteriores
    public decimal PorcentajeAdministrador => 100 - PorcentajeColaborador 
        - (PorcentajeGestor ?? 0) 
        - (PorcentajeJefeVentas ?? 0) 
        - (PorcentajeDirectorComercial ?? 0);
}
```

**Ejemplo:**
```
Usuario: Juan Pérez (Colaborador)
Proveedor: Iberdrola (Comercializadora)
Distribución:
  - Colaborador: 70%
  - Gestor: 10%
  - Jefe de Ventas: 10%
  - Administrador: 10% (calculado automáticamente)
```

### 2. DetalleComisionLiquidacion

Registra la distribución de comisiones de cada contrato en una liquidación específica.

```csharp
public class DetalleComisionLiquidacion
{
    public int Id { get; set; }
    public int HistoricoLiquidacionId { get; set; }
    public int ContratoId { get; set; }
    
    public decimal ComisionBase { get; set; }  // 100% de la comisión del contrato
    
    // Colaborador (siempre presente)
    public int ColaboradorId { get; set; }
    public decimal ComisionColaborador { get; set; }
    public decimal PorcentajeColaborador { get; set; }
    
    // Gestor (opcional según jerarquía)
    public int? GestorId { get; set; }
    public decimal? ComisionGestor { get; set; }
    public decimal? PorcentajeGestor { get; set; }
    
    // Jefe de Ventas (opcional)
    public int? JefeVentasId { get; set; }
    public decimal? ComisionJefeVentas { get; set; }
    public decimal? PorcentajeJefeVentas { get; set; }
    
    // Director Comercial (opcional)
    public int? DirectorComercialId { get; set; }
    public decimal? ComisionDirectorComercial { get; set; }
    public decimal? PorcentajeDirectorComercial { get; set; }
    
    // Administrador (siempre recibe el restante)
    public int AdministradorId { get; set; }
    public decimal ComisionAdministrador { get; set; }
    public decimal PorcentajeAdministrador { get; set; }
}
```

**Ejemplo de Distribución:**
```
Contrato de Energía - Comisión Base: 100€

Colaborador (Juan Pérez):    70€ (70%)
Gestor (María García):        10€ (10%)
Jefe Ventas (Carlos López):  10€ (10%)
Administrador (Admin):        10€ (10%)
─────────────────────────────────
TOTAL:                       100€ (100%)
```

### 3. Decomision

Registra penalizaciones por bajas anticipadas de contratos.

```csharp
public class Decomision
{
    public int Id { get; set; }
    public int ContratoId { get; set; }
    public int UsuarioId { get; set; }
    
    public string TipoDecomision { get; set; }  // "Total" o "Proporcional"
    
    public decimal ComisionOriginal { get; set; }
    public decimal ImporteDecomision { get; set; }
    
    public int DiasPenalizacion { get; set; }    // De la tarifa
    public int DiasActivo { get; set; }          // Tiempo que estuvo activo
    public int DiasPendientes { get; set; }      // Días que faltaban
    
    public DateTime? FechaAlta { get; set; }
    public DateTime FechaBaja { get; set; }
    
    public string Estado { get; set; }  // "Pendiente", "Aplicada", "Cancelada"
}
```

### 4. Campos Agregados a Tarifas

Se agregaron campos de penalización a las tres tablas de tarifas:

```csharp
// TarifaEnergia, TarifaTelefonia, TarifaAlarma
public int? DiasPenalizacion { get; set; }
public string? TipoPenalizacion { get; set; }  // "Total" o "Proporcional"
```

---

## Distribución Jerárquica

### Jerarquía de Usuarios

```
Administrador (único por empresa)
    │
    └─→ Director Comercial
            │
            └─→ Jefe de Ventas
                    │
                    └─→ Gestor
                            │
                            └─→ Colaborador
```

### Configuraciones por Proveedor

El sistema permite configurar porcentajes diferentes para cada combinación de:
- **Usuario**: Cualquier usuario del sistema
- **Tipo de Proveedor**: Comercializadora, Operadora o Empresa de Alarmas
- **Proveedor Específico**: ID del proveedor concreto

**Ejemplo:**
```
Juan Pérez puede tener:
  - Iberdrola (Comercializadora): 70% colaborador, 10% gestor, 10% jefe, 10% admin
  - Repsol (Comercializadora):    65% colaborador, 15% gestor, 10% jefe, 10% admin
  - Movistar (Operadora):         75% colaborador, 10% gestor, 5% jefe, 10% admin
```

### Porcentajes por Defecto

Si no existe configuración específica:
- **Colaborador**: 70%
- **Gestor**: 10%
- **Jefe de Ventas**: 10%
- **Director Comercial**: 0%
- **Administrador**: 10%

---

## Penalizaciones y Decomisiones

### Tipos de Penalización

#### 1. Penalización Total
Se devuelve el **100% de la comisión** si el contrato se da de baja antes de cumplir el periodo.

```
Ejemplo:
  Comisión original: 100€
  Días penalización: 365 días
  Días activo: 90 días
  Tipo: Total
  
  → Decomisión: 100€
```

#### 2. Penalización Proporcional
Se devuelve una cantidad proporcional según los días pendientes.

```
Fórmula:
  ImporteDecomision = (ComisionOriginal × DiasPendientes) / DiasPenalizacion

Ejemplo:
  Comisión original: 100€
  Días penalización: 365 días
  Días activo: 305 días
  Días pendientes: 60 días
  Tipo: Proporcional
  
  → Decomisión: (100 × 60) / 365 = 16,43€
```

### Proceso de Baja con Decomisión

1. Usuario marca contrato como "Baja"
2. Sistema verifica si tiene penalización configurada en la tarifa
3. Calcula si cumplió el periodo mínimo
4. Si NO cumplió:
   - Crea registro de Decomisión
   - Calcula importe (Total o Proporcional)
   - Marca como "Pendiente"
5. En la siguiente liquidación, se incluye como importe negativo

---

## Configuración

### Acceso a la Configuración

**Ruta**: `/configuracion-comisiones`

**Permisos**: Solo Administradores

### Pantalla de Administración

La página [ConfiguracionComisiones.razor](EnerfoneCRM/Components/Pages/ConfiguracionComisiones.razor) permite:

1. **Ver configuraciones existentes**
   - Filtrar por usuario
   - Ver distribución de porcentajes
   - Estado (Activa/Inactiva)

2. **Crear nuevas configuraciones**
   - Seleccionar usuario
   - Elegir tipo de proveedor
   - Seleccionar proveedor específico
   - Definir porcentajes
   - Validación automática (suma ≤ 100%)

3. **Editar configuraciones**
   - Modificar solo los porcentajes
   - Usuario y proveedor no se pueden cambiar

4. **Eliminar configuraciones**
   - Confirmación requerida

### Validaciones

- La suma de porcentajes no puede superar el 100%
- El porcentaje del Administrador se calcula automáticamente
- No se permiten porcentajes negativos
- No se pueden duplicar configuraciones (mismo usuario + proveedor)

---

## Proceso de Liquidación

### Flujo Actualizado

```
1. Usuario tiene contratos en estado "Act/Facturable"
2. Administrador aprueba liquidación
3. Sistema crea HistoricoLiquidacion
   │
   └─→ Para cada contrato:
       │
       ├─→ Busca configuración de comisión específica
       │   (usuario + proveedor)
       │
       ├─→ Si no existe, usa porcentajes por defecto
       │
       ├─→ Calcula distribución jerárquica:
       │   │
       │   ├─→ Obtiene jerarquía del usuario
       │   │   (Colaborador → Gestor → Jefe → Director → Admin)
       │   │
       │   ├─→ Aplica porcentajes configurados
       │   │
       │   └─→ Calcula importes por nivel
       │
       ├─→ Crea DetalleComisionLiquidacion
       │   (registra distribución completa)
       │
       └─→ Actualiza contrato:
           - Estado: "Liquidado"
           - HistoricoLiquidacionId: ID de liquidación
```

### Vista de Comisiones por Usuario

El sistema proporciona una vista SQL auxiliar:

```sql
CREATE OR REPLACE VIEW v_comisiones_por_usuario AS
SELECT 
    dcl.historico_liquidacion_id,
    u.id AS usuario_id,
    u.nombre_usuario,
    u.rol,
    SUM(comisión_del_usuario) AS total_comisiones,
    COUNT(DISTINCT dcl.contrato_id) AS total_contratos
FROM detalle_comision_liquidacion dcl
-- Agrupa las comisiones de cada usuario según su rol
GROUP BY dcl.historico_liquidacion_id, u.id;
```

---

## Instalación y Migración

### Scripts SQL a Ejecutar

**En orden:**

1. **ADD_PENALIZACION_TARIFAS.sql**
   - Agrega campos `dias_penalizacion` y `tipo_penalizacion` a tarifas
   - Establece valores por defecto basados en permanencias existentes

2. **ADD_CONFIGURACION_COMISIONES.sql**
   - Crea tabla `configuracion_comisiones`
   - Índices y claves foráneas

3. **ADD_DETALLE_COMISION_LIQUIDACION.sql**
   - Crea tabla `detalle_comision_liquidacion`
   - Vista `v_comisiones_por_usuario`
   - Stored procedure `sp_calcular_totales_liquidacion`

4. **ADD_DECOMISIONES.sql**
   - Crea tabla `decomisiones`
   - Vistas `v_decomisiones_por_usuario` y `v_decomisiones_pendientes`
   - Stored procedure `sp_crear_decomision`

### Ejecución

```bash
# Conectar a MySQL
mysql -u usuario -p enerfone_crm

# Ejecutar scripts
source ADD_PENALIZACION_TARIFAS.sql
source ADD_CONFIGURACION_COMISIONES.sql
source ADD_DETALLE_COMISION_LIQUIDACION.sql
source ADD_DECOMISIONES.sql
```

### Verificación

```sql
-- Verificar tablas creadas
SHOW TABLES LIKE '%comision%';
SHOW TABLES LIKE 'decomisiones';

-- Verificar campos agregados
DESCRIBE tarifasenergia;
DESCRIBE tarifastelefonia;
DESCRIBE tarifas_alarmas;
```

---

## Uso del Sistema

### Para Administradores

1. **Configurar comisiones personalizadas**
   - Ir a `/configuracion-comisiones`
   - Crear configuración por usuario y proveedor
   - Definir porcentajes específicos

2. **Configurar penalizaciones en tarifas**
   - Editar tarifa de energía/telefonía/alarma
   - Establecer `dias_penalizacion` (ej: 365)
   - Definir `tipo_penalizacion` ("Total" o "Proporcional")

3. **Aprobar liquidaciones**
   - El sistema calculará automáticamente la distribución
   - Se crearán registros de `DetalleComisionLiquidacion`

4. **Gestionar bajas con decomisión**
   - El sistema detecta automáticamente si aplica penalización
   - Crea registro de `Decomision`
   - Se incluirá en próxima liquidación como importe negativo

### Para Usuarios

- Los colaboradores solo ven sus propias comisiones
- Los gestores/jefes/directores ven comisiones de su equipo
- Las comisiones se muestran según jerarquía y permisos

---

## Consideraciones Técnicas

### Servicios

- **ComisionService**: Lógica de cálculo y distribución
- **ContratoService**: Gestión de bajas con verificación de penalizaciones
- Ambos están registrados en el contenedor DI (`Program.cs`)

### Inyección de Dependencias

```csharp
builder.Services.AddScoped<ComisionService>();
```

### Uso en Componentes

```csharp
@inject ComisionService ComisionService

// Calcular distribución
var detalle = await ComisionService.CalcularDistribucionComisionAsync(
    contrato, 
    colaborador, 
    liquidacionId
);

// Verificar penalización
var (requiere, dias, importe) = await ComisionService.VerificarPenalizacionAsync(
    contrato
);
```

---

## Consultas Útiles

### Ver distribución de una liquidación

```sql
SELECT 
    u.nombre_usuario,
    u.rol,
    SUM(CASE 
        WHEN d.colaborador_id = u.id THEN d.comision_colaborador
        WHEN d.gestor_id = u.id THEN d.comision_gestor
        WHEN d.jefe_ventas_id = u.id THEN d.comision_jefe_ventas
        WHEN d.director_comercial_id = u.id THEN d.comision_director_comercial
        WHEN d.administrador_id = u.id THEN d.comision_administrador
    END) as total_comision
FROM detalle_comision_liquidacion d
CROSS JOIN usuarios u
WHERE d.historico_liquidacion_id = {ID_LIQUIDACION}
  AND u.id IN (
      d.colaborador_id, d.gestor_id, d.jefe_ventas_id, 
      d.director_comercial_id, d.administrador_id
  )
GROUP BY u.id, u.nombre_usuario, u.rol;
```

### Ver decomisiones pendientes

```sql
SELECT * FROM v_decomisiones_pendientes;
```

### Ver configuraciones de un usuario

```sql
SELECT * FROM configuracion_comisiones 
WHERE usuario_id = {ID_USUARIO} 
  AND activa = 1;
```

---

## Soporte y Mantenimiento

### Logs

El sistema genera logs Console en:
- Cálculo de distribuciones
- Creación de decomisiones
- Aprobación de liquidaciones

Buscar por: `[LIQUIDACIONES]` o `[COMISIONES]`

### Troubleshooting

**Problema**: Las comisiones no se distribuyen correctamente
- Verificar que exista un Administrador activo
- Revisar jerarquía de usuarios (GestorId, JefeVentasId, etc.)
- Verificar configuraciones activas

**Problema**: Decomisión no se crea
- Verificar que la tarifa tenga `dias_penalizacion` configurado
- Verificar que el contrato tenga `FechaAlta`
- Revisar que el contrato esté vinculado a una liquidación

---

## Fecha de Implementación

**Marzo 2026**

## Autor

Sistema implementado para EnerfoneCRM v2
