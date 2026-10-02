# Algoritmo de Cálculo de Comparativas Eléctricas

## 🧮 OBJETIVO

Comparar el coste de la factura eléctrica actual del cliente con las tarifas de Naturgy para calcular el ahorro potencial.

---

## 📥 DATOS DE ENTRADA

### 1. Datos de la factura actual del cliente (`form`)

```javascript
{
  // Fechas del periodo
  invoiceStart: "2026-06-01",
  invoiceEnd: "2026-07-01",
  reportedInvoiceTotal: "850.50", // Total real de la factura
  
  // Arrays por periodo (P1, P2, P3, P4, P5, P6)
  contractedPower: ["10", "5", ...], // kW contratados
  currentPowerPrices: ["0.12", "0.05", ...], // €/kW/día
  energyConsumption: ["1200", "800", ...], // kWh consumidos
  currentEnergyPrices: ["0.15", "0.12", ...], // €/kWh
  
  // Conceptos adicionales
  other: {
    excessPower: "15.00",
    reactiveEnergy: "5.00",
    extensionRights: "0.00",
    accessRights: "0.00",
    socialBonus: "10.00",
    discounts: "-20.00",
    meterRental: "0.81",
    surplusKwh: "50", // Excedentes de autoconsumo
    currentSurplusPrice: "0.05",
    electricityTaxPercent: "5.11",
    vatPercent: "21"
  }
}
```

### 2. Tarifa Naturgy (`tariff`)

```javascript
{
  family: "Plan Fijo Luz",
  variant: "ONE", // Base, ONE, SUPRA
  toll: "2.0TD", // 2.0TD, 3.0TD, 6.1TD
  powerPrices: ["0.123", "0.044"], // €/kW/día
  energyPrices: ["0.142", "0.142", "0.142"], // €/kWh
  status: "valid"
}
```

### 3. Reglas de aplicación (`rules`)

```javascript
{
  electricityTaxPercent: "5.11",
  vatPercent: "21",
  naturgySurplusPrice: "0.06",
  additionalConcepts: {
    naturgy_excess_power: "copy",      // copy | zero | fixed:X
    naturgy_reactive: "zero",
    naturgy_extension_rights: "zero",
    naturgy_access_rights: "zero",
    naturgy_social_bonus: "copy",
    naturgy_discounts: "zero",
    naturgy_meter_rental: "copy"
  }
}
```

---

## ⚙️ PROCESO DE CÁLCULO (PASO A PASO)

### PASO 1: Determinación de periodos según peaje

```javascript
periodCounts(toll) {
  if (toll === "2.0TD") return [2, 3];  // 2 potencias, 3 energías
  if (toll === "3.0TD") return [6, 6];  // 6 potencias, 6 energías
  if (toll === "6.1TD") return [6, 6];  // 6 potencias, 6 energías
}
```

**Periodos definidos:**
- `PERIODS = ['P1', 'P2', 'P3', 'P4', 'P5', 'P6']`

### PASO 2: Cálculo de días del periodo

```javascript
days = (invoiceEnd - invoiceStart) / 86400000
// Ejemplo: del 01/06 al 01/07 = 30 días
```

### PASO 3: Cálculo de costes de POTENCIA

**Fórmula por periodo:**
```
Coste Potencia = Potencia Contratada × Precio × Días
```

**Para cada periodo (P1...P6):**
```javascript
// ACTUAL
powerCurrentAmounts[i] = contractedPower[i] × currentPowerPrices[i] × days

// NATURGY
powerNaturgyAmounts[i] = contractedPower[i] × tariff.powerPrices[i] × days
```

**Totales:**
```javascript
totalPowerCurrent = SUM(powerCurrentAmounts)
totalPowerNaturgy = SUM(powerNaturgyAmounts)
```

**Ejemplo:**
- Periodo P1: 10 kW × 0.12 €/kW/día × 30 días = 36.00 €
- Periodo P2: 5 kW × 0.05 €/kW/día × 30 días = 7.50 €
- **Total Potencia Actual: 43.50 €**

### PASO 4: Cálculo de costes de ENERGÍA

**Fórmula por periodo:**
```
Coste Energía = Consumo × Precio
```

**Para cada periodo (P1...P6):**
```javascript
// ACTUAL
energyCurrentAmounts[i] = energyConsumption[i] × currentEnergyPrices[i]

// NATURGY
energyNaturgyAmounts[i] = energyConsumption[i] × tariff.energyPrices[i]
```

**Totales:**
```javascript
totalEnergyCurrent = SUM(energyCurrentAmounts)
totalEnergyNaturgy = SUM(energyNaturgyAmounts)
```

**Ejemplo:**
- Periodo P1: 1200 kWh × 0.15 €/kWh = 180.00 €
- Periodo P2: 800 kWh × 0.12 €/kWh = 96.00 €
- Periodo P3: 400 kWh × 0.10 €/kWh = 40.00 €
- **Total Energía Actual: 316.00 €**

### PASO 5: Aplicación de REGLAS a conceptos adicionales

**Función `applyRule(rule, currentValue)`:**
```javascript
if (rule === "zero") return 0;
if (rule === "copy") return currentValue;
if (rule.startsWith("fixed:")) return parseFloat(rule.substring(6));
return currentValue;
```

**Conceptos y reglas por defecto:**
```javascript
RULE_DEFAULTS = {
  naturgy_excess_power: "copy",       // Exceso de potencia
  naturgy_reactive: "zero",           // Energía reactiva
  naturgy_extension_rights: "zero",   // Derechos de extensión
  naturgy_access_rights: "zero",      // Derechos de acceso
  naturgy_social_bonus: "copy",       // Financiación del bono social
  naturgy_discounts: "zero",          // Descuentos, campañas y ofertas
  naturgy_meter_rental: "copy"        // Alquiler de equipos
}
```

**Aplicación:**
```javascript
natExcess = applyRule(rules.naturgy_excess_power, currentExcess)
natReactive = applyRule(rules.naturgy_reactive, currentReactive)
natExtension = applyRule(rules.naturgy_extension_rights, currentExtension)
natAccess = applyRule(rules.naturgy_access_rights, currentAccess)
natSocial = applyRule(rules.naturgy_social_bonus, currentSocial)
natDiscounts = applyRule(rules.naturgy_discounts, currentDiscounts)
natMeter = applyRule(rules.naturgy_meter_rental, currentMeter)
```

**Ejemplo:**
- Exceso de potencia actual: 15.00 € → Naturgy: 15.00 € (copy)
- Energía reactiva actual: 5.00 € → Naturgy: 0.00 € (zero)
- Bono social actual: 10.00 € → Naturgy: 10.00 € (copy)
- Descuentos actual: -20.00 € → Naturgy: 0.00 € (zero)
- Alquiler actual: 0.81 € → Naturgy: 0.81 € (copy)

### PASO 6: Cálculo de EXCEDENTES (autoconsumo)

```javascript
// Crédito por venta de excedentes (NEGATIVO porque es un descuento)
currentSurplusCredit = -(surplusKwh × currentSurplusPrice)
natSurplusCredit = -(surplusKwh × naturgySurplusPrice)
```

**Ejemplo:**
- 50 kWh × 0.05 €/kWh = -2.50 € (descuento)
- 50 kWh × 0.06 €/kWh = -3.00 € (descuento Naturgy)

### PASO 7: Cálculo del IMPUESTO ELÉCTRICO

**Base imponible:**
```javascript
electricityTaxBaseCurrent = totalPowerCurrent + totalEnergyCurrent
electricityTaxBaseNaturgy = totalPowerNaturgy + totalEnergyNaturgy
```

**Impuesto (5.11% por defecto):**
```javascript
taxRate = electricityTaxPercent / 100

taxCurrent = electricityTaxBaseCurrent × taxRate
taxNaturgy = electricityTaxBaseNaturgy × taxRate
```

**Ejemplo:**
- Base actual: 43.50 + 316.00 = 359.50 €
- Impuesto actual: 359.50 × 0.0511 = 18.37 €

### PASO 8: Cálculo de BASE IMPONIBLE (antes de IVA)

```javascript
baseCurrent = 
  currentExcess +
  totalPowerCurrent +
  totalEnergyCurrent +
  currentReactive +
  currentExtension +
  currentAccess +
  taxCurrent +
  currentSocial +
  currentDiscounts +
  currentMeter +
  currentSurplusCredit

baseNaturgy = 
  natExcess +
  totalPowerNaturgy +
  totalEnergyNaturgy +
  natReactive +
  natExtension +
  natAccess +
  taxNaturgy +
  natSocial +
  natDiscounts +
  natMeter +
  natSurplusCredit
```

**Ejemplo (Actual):**
```
15.00    (exceso potencia)
43.50    (potencia)
316.00   (energía)
5.00     (reactiva)
0.00     (extensión)
0.00     (acceso)
18.37    (impuesto eléctrico)
10.00    (bono social)
-20.00   (descuentos)
0.81     (alquiler)
-2.50    (excedentes)
─────────
386.18 € (base imponible)
```

### PASO 9: Cálculo del IVA

```javascript
vatRate = vatPercent / 100

vatCurrent = baseCurrent × vatRate
vatNaturgy = baseNaturgy × vatRate
```

**Ejemplo:**
- IVA actual: 386.18 × 0.21 = 81.10 €
- IVA Naturgy: 348.25 × 0.21 = 73.13 €

### PASO 10: Cálculo de TOTALES FINALES

```javascript
// ACTUAL: Se usa el total REAL de la factura (dato introducido)
totalCurrent = reportedInvoiceTotal

// NATURGY: Base + IVA
totalNaturgy = baseNaturgy + vatNaturgy
```

**⚠️ IMPORTANTE:** 
- El `totalCurrent` NO se recalcula, se toma directamente del campo `reportedInvoiceTotal`.
- Esto permite comparar contra el total real de la factura, que puede incluir conceptos no modelados.

**Ejemplo:**
- Total Actual: **850.50 €** (dato real de la factura)
- Total Naturgy: 348.25 + 73.13 = **421.38 €**

### PASO 11: Cálculo del AHORRO

```javascript
// Ahorro del periodo
saving = totalCurrent - totalNaturgy

// Ahorro diario
savingDaily = saving / days

// Ahorro anual
savingAnnual = savingDaily × 365

// Ahorro mensual
savingMonthly = savingAnnual / 12

// Porcentaje de ahorro sobre factura actual
savingPercentCurrent = (saving / totalCurrent) × 100
```

**Ejemplo:**
- Ahorro del periodo: 850.50 - 421.38 = **429.12 €**
- Ahorro diario: 429.12 / 30 = **14.30 €/día**
- Ahorro anual: 14.30 × 365 = **5,221.50 €/año**
- Ahorro mensual: 5,221.50 / 12 = **435.13 €/mes**
- Porcentaje: (429.12 / 850.50) × 100 = **50.45%**

### PASO 12: Determinación del ESTADO

```javascript
if (saving >= 0) {
  status = "saving"  // Ahorro
} else {
  status = "overcost"  // Sobrecoste
  warnings.push("Esta oferta genera un sobrecoste estimado frente al total real de la factura actual.")
}
```

---

## 📤 ESTRUCTURA DE DATOS DE SALIDA

```javascript
{
  days: 30,
  powerPeriodCount: 2,
  energyPeriodCount: 3,
  status: "saving",  // "saving" | "overcost"
  warnings: [],
  
  // Desglose por periodo - POTENCIA
  powerLines: [
    {
      period: "P1",
      quantity: 10,           // kW contratados
      currentPrice: 0.12,     // €/kW/día actual
      naturgyPrice: 0.123,    // €/kW/día Naturgy
      currentAmount: 36.00,   // Coste actual
      naturgyAmount: 36.90    // Coste Naturgy
    },
    {
      period: "P2",
      quantity: 5,
      currentPrice: 0.05,
      naturgyPrice: 0.044,
      currentAmount: 7.50,
      naturgyAmount: 6.60
    }
  ],
  
  // Desglose por periodo - ENERGÍA
  energyLines: [
    {
      period: "P1",
      quantity: 1200,         // kWh consumidos
      currentPrice: 0.15,     // €/kWh actual
      naturgyPrice: 0.142,    // €/kWh Naturgy
      currentAmount: 180.00,  // Coste actual
      naturgyAmount: 170.40   // Coste Naturgy
    },
    {
      period: "P2",
      quantity: 800,
      currentPrice: 0.12,
      naturgyPrice: 0.142,
      currentAmount: 96.00,
      naturgyAmount: 113.60
    },
    {
      period: "P3",
      quantity: 400,
      currentPrice: 0.10,
      naturgyPrice: 0.142,
      currentAmount: 40.00,
      naturgyAmount: 56.80
    }
  ],
  
  // Totales y resultados
  totals: {
    totalPowerCurrent: 43.50,
    totalPowerNaturgy: 43.50,
    totalEnergyCurrent: 316.00,
    totalEnergyNaturgy: 340.80,
    totalConsumption: 2400,  // Total kWh
    
    // Bases para impuesto eléctrico
    electricityTaxBaseCurrent: 359.50,
    electricityTaxBaseNaturgy: 384.30,
    
    // Impuesto eléctrico (5.11%)
    electricityTaxCurrent: 18.37,
    electricityTaxNaturgy: 19.64,
    
    // Base imponible (antes de IVA)
    taxableBaseCurrent: 386.18,
    taxableBaseNaturgy: 348.25,
    
    // IVA (21%)
    vatCurrent: 81.10,
    vatNaturgy: 73.13,
    
    // Totales finales
    totalCurrent: 850.50,        // Total REAL de la factura
    totalCurrentReal: 850.50,
    totalNaturgy: 421.38,        // Total estimado con Naturgy
    
    // Ahorros
    savingPeriod: 429.12,        // Ahorro en el periodo
    savingDaily: 14.30,          // Ahorro diario
    savingMonthly: 435.13,       // Ahorro mensual
    savingAnnual: 5221.50,       // Ahorro anual
    savingPercentCurrent: 50.45, // % sobre factura actual
    legacyPercent: 101.84        // % sobre total Naturgy
  },
  
  // Conceptos adicionales
  concepts: {
    current: {
      excessPower: 15.00,
      reactiveEnergy: 5.00,
      extensionRights: 0.00,
      accessRights: 0.00,
      socialBonus: 10.00,
      discounts: -20.00,
      meterRental: 0.81,
      surplusCredit: -2.50,
      electricityTaxPercent: 5.11,
      vatPercent: 21
    },
    naturgy: {
      excessPower: 15.00,
      reactiveEnergy: 0.00,
      extensionRights: 0.00,
      accessRights: 0.00,
      socialBonus: 10.00,
      discounts: 0.00,
      meterRental: 0.81,
      surplusCredit: -3.00,
      surplusPrice: 0.06
    }
  }
}
```

---

## 🔑 PUNTOS CLAVE

### 1. Precisión decimal
- **Obligatorio**: Usar aritmética de precisión arbitraria (clase `Dec` en el código)
- Evita errores de redondeo en cálculos financieros
- Mantiene precisión de hasta 24 decimales

### 2. Periodos dinámicos según peaje

| Peaje  | Periodos Potencia | Periodos Energía |
|--------|-------------------|------------------|
| 2.0TD  | 2 (P1, P2)        | 3 (P1, P2, P3)   |
| 3.0TD  | 6 (P1...P6)       | 6 (P1...P6)      |
| 6.1TD  | 6 (P1...P6)       | 6 (P1...P6)      |

### 3. Total actual vs. Total calculado
- ✅ **Total Actual**: Siempre se usa el `reportedInvoiceTotal` (dato real introducido)
- ❌ **NO se recalcula** sumando componentes
- Permite comparar contra facturas con conceptos no modelados

### 4. Excedentes de autoconsumo
- Se restan del total (valor negativo = descuento)
- Precio configurable para Naturgy (por defecto 0.06 €/kWh)

### 5. Impuestos en cascada
1. **Primero**: Impuesto eléctrico (5.11%) sobre Potencia + Energía
2. **Después**: IVA (21%) sobre la base total (incluyendo el impuesto eléctrico)

### 6. Reglas flexibles para conceptos adicionales

**3 tipos de reglas:**
- `"copy"`: Copiar el valor de la factura actual
- `"zero"`: Establecer a 0 €
- `"fixed:X"`: Usar valor fijo X (ejemplo: `"fixed:12.50"`)

**Ejemplo de configuración:**
```javascript
{
  naturgy_excess_power: "copy",      // Mantener exceso
  naturgy_reactive: "zero",          // Eliminar reactiva
  naturgy_meter_rental: "fixed:0.81" // Alquiler fijo
}
```

### 7. Variantes de oferta

**Pymes:**
- `Base` - Tarifa estándar
- `ONE` - Descuento nivel 1
- `SUPRA` - Descuento nivel 2

**Residencial:**
- `PorUso` - Tarifa plana 24h (precio único)
- `Noche` - Discriminación horaria (3 periodos)

### 8. Cálculo múltiple
El sistema calcula **simultáneamente** las 3 variantes (Base, ONE, SUPRA) o las 2 residenciales (PorUso, Noche) y selecciona automáticamente la mejor (menor `totalNaturgy`).

---

## 📊 DIAGRAMA DE FLUJO SIMPLIFICADO

```
┌─────────────────────────┐
│ Datos Factura Actual    │
│ + Tarifa Naturgy        │
│ + Reglas                │
└───────────┬─────────────┘
            │
            ▼
┌─────────────────────────┐
│ Calcular días periodo   │
└───────────┬─────────────┘
            │
            ▼
┌─────────────────────────┐
│ Coste Potencia          │
│ (kW × €/kW/día × días)  │
├─────────────────────────┤
│ Actual │ Naturgy        │
└───────────┬─────────────┘
            │
            ▼
┌─────────────────────────┐
│ Coste Energía           │
│ (kWh × €/kWh)           │
├─────────────────────────┤
│ Actual │ Naturgy        │
└───────────┬─────────────┘
            │
            ▼
┌─────────────────────────┐
│ Aplicar reglas a        │
│ conceptos adicionales   │
└───────────┬─────────────┘
            │
            ▼
┌─────────────────────────┐
│ Calcular excedentes     │
│ (crédito negativo)      │
└───────────┬─────────────┘
            │
            ▼
┌─────────────────────────┐
│ Impuesto Eléctrico      │
│ 5.11% × (Pot + Energ)   │
└───────────┬─────────────┘
            │
            ▼
┌─────────────────────────┐
│ Base Imponible          │
│ (suma todos conceptos)  │
└───────────┬─────────────┘
            │
            ▼
┌─────────────────────────┐
│ IVA (21% × Base)        │
└───────────┬─────────────┘
            │
            ▼
┌─────────────────────────┐
│ Total Final             │
│ Actual: reportedTotal   │
│ Naturgy: Base + IVA     │
└───────────┬─────────────┘
            │
            ▼
┌─────────────────────────┐
│ Ahorro = Actual - Natur │
│ ├─ Periodo              │
│ ├─ Diario               │
│ ├─ Mensual              │
│ ├─ Anual                │
│ └─ Porcentaje           │
└─────────────────────────┘
```

---

## 🧪 EJEMPLO COMPLETO

### Entrada
```javascript
form = {
  invoiceStart: "2026-06-01",
  invoiceEnd: "2026-07-01",
  reportedInvoiceTotal: "850.50",
  contractedPower: ["10", "5"],
  currentPowerPrices: ["0.12", "0.05"],
  energyConsumption: ["1200", "800", "400"],
  currentEnergyPrices: ["0.15", "0.12", "0.10"],
  other: {
    excessPower: "15.00",
    reactiveEnergy: "5.00",
    socialBonus: "10.00",
    discounts: "-20.00",
    meterRental: "0.81",
    surplusKwh: "50",
    currentSurplusPrice: "0.05",
    electricityTaxPercent: "5.11",
    vatPercent: "21"
  }
}

tariff = {
  family: "Plan Fijo Luz",
  variant: "ONE",
  toll: "2.0TD",
  powerPrices: ["0.123", "0.044"],
  energyPrices: ["0.142", "0.142", "0.142"]
}
```

### Proceso
1. Días: 30
2. Potencia Actual: 10×0.12×30 + 5×0.05×30 = 36.00 + 7.50 = **43.50 €**
3. Potencia Naturgy: 10×0.123×30 + 5×0.044×30 = 36.90 + 6.60 = **43.50 €**
4. Energía Actual: 1200×0.15 + 800×0.12 + 400×0.10 = 180+96+40 = **316.00 €**
5. Energía Naturgy: 1200×0.142 + 800×0.142 + 400×0.142 = 170.4+113.6+56.8 = **340.80 €**
6. Impuesto Eléctrico Actual: (43.50+316.00) × 0.0511 = **18.37 €**
7. Impuesto Eléctrico Naturgy: (43.50+340.80) × 0.0511 = **19.64 €**
8. Base Actual: 15+43.50+316+5+0+0+18.37+10-20+0.81-2.50 = **386.18 €**
9. Base Naturgy: 15+43.50+340.80+0+0+0+19.64+10+0+0.81-3.00 = **426.75 €**
10. IVA Actual: 386.18 × 0.21 = **81.10 €**
11. IVA Naturgy: 426.75 × 0.21 = **89.62 €**
12. Total Actual: **850.50 €** (dato real)
13. Total Naturgy: 426.75 + 89.62 = **516.37 €**

### Resultado
- **Ahorro periodo**: 850.50 - 516.37 = **334.13 €**
- **Ahorro diario**: 334.13 / 30 = **11.14 €/día**
- **Ahorro anual**: 11.14 × 365 = **4,066.10 €/año**
- **Ahorro mensual**: 4,066.10 / 12 = **338.84 €/mes**
- **Porcentaje**: (334.13 / 850.50) × 100 = **39.29%**
- **Estado**: ✅ **saving** (ahorro positivo)

---

## 📝 NOTAS DE IMPLEMENTACIÓN

### Validaciones requeridas
1. ✅ Fechas válidas (formato YYYY-MM-DD)
2. ✅ Fecha final posterior a inicial
3. ✅ Total factura > 0
4. ✅ Todos los arrays con valores no negativos
5. ✅ Tarifa con estado "valid"
6. ✅ Número correcto de periodos según peaje

### Manejo de errores
```javascript
try {
  result = calculateComparison(form, tariff, rules);
} catch (error) {
  // error.message contiene descripción clara del problema
  console.error(error.message);
}
```

### Modo estricto vs. no estricto
- **`strict=true`** (por defecto): Requiere todos los campos obligatorios
- **`strict=false`**: Permite campos vacíos (usa 0 como valor por defecto)

---

## 🔗 REFERENCIAS

- **Código fuente**: `public/assets/app.js` → función `calculateComparison()`
- **Documentación tarifas**: `docs/INVENTARIO_TARIFAS.md`
- **Esquema base de datos**: `database/schema.sql` → tabla `comparisons`
- **Datos de prueba**: `database/seed_tariffs.json`

---

**Última actualización**: 2026-08-03  
**Versión del algoritmo**: 6.0.0
