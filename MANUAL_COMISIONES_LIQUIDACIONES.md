# 📊 Manual de Comisiones y Liquidaciones - CorCRM

## 🎯 Índice

1. [Introducción](#introducción)
2. [Conceptos Clave](#conceptos-clave)
3. [Configuración de Comisiones](#configuración-de-comisiones)
4. [Cálculo de Comisiones](#cálculo-de-comisiones)
5. [Liquidaciones](#liquidaciones)
6. [Preguntas Frecuentes](#preguntas-frecuentes)

---

## 📖 Introducción

El sistema de **Comisiones y Liquidaciones** de CorCRM gestiona automáticamente el reparto de comisiones entre los diferentes niveles jerárquicos comerciales.

### Definiciones

- **Comisión**: Porcentaje o cantidad que recibe cada comercial por cerrar un contrato, calculado según tarifa, configuración del usuario y proveedor.
- **Liquidación**: Documento que detalla cuánto cobra cada usuario por los contratos vendidos en un período.

---

## 🔑 Conceptos Clave

### Jerarquía Comercial

El sistema reconoce 5 niveles jerárquicos con **comisiones acumulativas**:

```
Administrador (100% de la comisión base)
    ↓
Director Comercial (Colab + Gestor + Jefe + Director)
    ↓
Jefe Comercial (Colab + Gestor + Jefe)
    ↓
Gestor (Colab + Gestor)
    ↓
Colaborador (Solo su porcentaje)
```

### Comisión Base vs Comisión del Contrato

- **Comisión Base**: Monto total en € configurado en la tarifa (ej: 100€)
- **Comisión del Contrato**: Resultado de aplicar el porcentaje del usuario según su rol

### Cálculo Jerárquico

**Ejemplo de configuración**:
- Colaborador: 70%
- Gestor: 10%
- Jefe: 10%
- Director: 5%
- **TOTAL: 95%** (5% margen empresa)

**Si la tarifa tiene 100€ de comisión base**:

| Rol | Cálculo | Comisión |
|-----|---------|----------|
| Colaborador | 70% | 70€ |
| Gestor | 70% + 10% = 80% | 80€ |
| Jefe Comercial | 70% + 10% + 10% = 90% | 90€ |
| Director Comercial | 70% + 10% + 10% + 5% = 95% | 95€ |
| Administrador | 100% | 100€ |

**⚠️ REGLA FUNDAMENTAL**: La suma de TODOS los porcentajes **NUNCA puede superar el 100%**.

---

## ⚙️ Configuración de Comisiones

### Tabla: `configuracion_comisiones`

| Campo | Descripción | Ejemplo |
|-------|-------------|---------|
| `usuario_id` | ID del usuario | 5 |
| `tipo_proveedor` | 'comercializadora', 'operadora' o 'alarma' | operadora |
| `proveedor_id` | ID del proveedor (NULL = genérico) | 2 |
| `porcentaje_colaborador` | % del colaborador | 70.00 |
| `porcentaje_gestor` | % adicional gestor | 10.00 |
| `porcentaje_jefe_ventas` | % adicional jefe | 10.00 |
| `porcentaje_director_comercial` | % adicional director | 5.00 |

### Ejemplo de Inserción

```sql
INSERT INTO configuracion_comisiones (
    usuario_id, tipo_proveedor, proveedor_id,
    porcentaje_colaborador, porcentaje_gestor,
    porcentaje_jefe_ventas, porcentaje_director_comercial
) VALUES (
    5, 'operadora', 2,
    70.00, 10.00, 10.00, 5.00
);
-- SUMA: 95% ✅ (5% margen empresa)
```

### Búsqueda de Configuración

El sistema busca en orden:
1. **Específica**: usuario + proveedor concreto
2. **Genérica**: usuario + tipo proveedor (proveedor_id NULL)
3. **Sin configuración**: Aplica 100% de la comisión base

---

## 💰 Cálculo de Comisiones

### Al Crear un Contrato

El sistema calcula la comisión según el **rol del usuario**:

#### 1. En Wizards de Creación
Cuando se selecciona una tarifa, se ejecuta `CalcularComisionConPorcentajeUsuario()`:

```csharp
// Busca configuración del usuario + proveedor
// Calcula porcentaje total según rol:
if (Administrador) → 100%
else if (Director) → Colab + Gestor + Jefe + Director
else if (Jefe) → Colab + Gestor + Jefe
else if (Gestor) → Colab + Gestor
else → Colab

// Aplica: comisionFinal = comisionBase × (porcentajeTotal / 100)
```

#### 2. En Listado de Clientes
Al seleccionar tarifa en energía/telefonía/alarmas, automáticamente:
- `@bind:after="ActualizarDatosTarifaEnergia"`
- `@bind:after="ActualizarDatosTarifaTelefonia"`
- `@bind-Value:after="ActualizarDatosTarifaAlarmas"`

Cada método llama a `CalcularComisionConPorcentajeUsuario()` con la misma lógica jerárquica.

### Ejemplo Práctico

**Datos**:
- Usuario: Juan (Gestor)
- Tarifa: 100€ comisión base
- Configuración: Colab 70%, Gestor 10%

**Cálculo**:
- Porcentaje total: 70% + 10% = **80%**
- Comisión final: 100€ × 80% = **80€**
- Se guarda en `contrato.comision = 80`

Si Juan fuera **Colaborador**: 100€ × 70% = **70€**  
Si Juan fuera **Jefe**: 100€ × 90% = **90€**

### Cambios en Configuración

Cuando se modifica `configuracion_comisiones`, el sistema **recalcula automáticamente** todos los contratos en estos estados:
- Pte Carga
- Solicitado
- Pte Firma
- En incidencia
- Pte Documentación
- Pte Validación
- En Curso
- En Activación
- En tramitación
- Activo
- Act/Facturable

**Los contratos finalizados o cancelados NO se recalculan**.

### Cambios en Tarifas

Si se modifica la comisión de una tarifa:
- **Contratos existentes**: Mantienen su comisión original
- **Contratos nuevos**: Usan la nueva comisión

---

## 📋 Liquidaciones

### Generación

Al crear una liquidación para un período:

1. **Busca contratos** con `fecha_activo` en el período y estado liquidable
2. **Colaborador**: Recibe el valor de `contrato.comision`
3. **Jerarquía**: Calcula desde la comisión BASE de la tarifa:
   - Gestor: comisionBase × porcentajeGestor
   - Jefe: comisionBase × porcentajeJefe
   - Director: comisionBase × porcentajeDirector

### Distribución Ejemplo

**Contrato con tarifa de 100€ base**:
- Colaborador ya tiene 70€ guardado
- Sistema busca la tarifa y aplica a la jerarquía:
  - Gestor: 100€ × 10% = 10€
  - Jefe: 100€ × 10% = 10€
  - Director: 100€ × 5% = 5€

**Total distribuido**: 70 + 10 + 10 + 5 = **95€** (5€ margen empresa)

### Tabla: `detalle_comision_liquidacion`

| Liquidación | Contrato | Usuario | Rol | Comisión |
|-------------|----------|---------|-----|----------|
| 5 | 123 | Juan | Colaborador | 70€ |
| 5 | 123 | María | Gestor | 10€ |
| 5 | 123 | Pedro | Jefe | 10€ |
| 5 | 123 | Ana | Director | 5€ |

---

## ❓ Preguntas Frecuentes

### ¿Los porcentajes pueden sumar más de 100%?
**NO**. La suma NUNCA puede superar el 100% de la comisión base de la tarifa.

### ¿Qué pasa si cambio el porcentaje de un usuario?
Se recalculan automáticamente los contratos en estados pre-activación (11 estados). Los contratos finalizados mantienen su comisión original.

### ¿Qué pasa si cambio la comisión de una tarifa?
Los contratos existentes mantienen su comisión. Solo los nuevos usan la nueva comisión.

### ¿Cómo funciona la comisión acumulativa?
Usuarios de nivel superior reciben la suma de porcentajes de niveles inferiores:
- Gestor = Colaborador + Gestor
- Jefe = Colaborador + Gestor + Jefe
- Director = Colaborador + Gestor + Jefe + Director
- Administrador = 100% (todo)

### ¿Dónde se aplica el cálculo jerárquico?
En **todos** los flujos de creación de contratos:
- Wizards de Energía/Telefonía/Alarmas
- Formulario rápido desde Listado de Clientes
- Recálculo automático al cambiar configuración

### ¿Puedo tener diferentes porcentajes por proveedor?
Sí. Cada combinación usuario+proveedor puede tener su configuración, siempre respetando el 100% máximo.

---

## 📞 Soporte

**Contacta con el administrador** para modificar configuraciones o resolver dudas.

---

**Versión del Manual**: 2.0  
**Fecha**: 30 de abril de 2026  
**Sistema**: CorCRM v20260430
