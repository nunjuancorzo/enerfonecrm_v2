# Comparador de Tarifas de Energía

## Descripción General

Se ha implementado un **Comparador de Tarifas de Energía** que permite a los usuarios introducir los datos de su contrato actual (potencias, consumos, factura actual) y obtener una comparativa con las tarifas disponibles en el mercado, ordenadas de más económica a más cara.

## Ubicación

- **URL**: `/comparador/energia`
- **Menú**: "🔍 Comparador" (visible para todos los usuarios autenticados)
- **Archivo**: `/Components/Pages/ComparadorEnergia.razor`

## Funcionalidades

### 1. Entrada de Datos

El usuario puede introducir:

- **Tipo de Tarifa**: 
  - 2.0TD (≤ 10 kW) - 2 periodos
  - 3.0TD (10-15 kW) - 3 periodos
  - 6.1TD (> 15 kW) - 3 periodos

- **Tipo de Suministro**: Luz o Luz+Gas

- **Potencias Contratadas** (kW):
  - Periodo 1 (Punta)
  - Periodo 2 (Valle/Llano)
  - Periodo 3 (Valle) - solo para 3.0TD y 6.1TD

- **Consumos Anuales** (kWh/año):
  - Periodo 1 (Punta)
  - Periodo 2 (Valle/Llano)
  - Periodo 3 (Valle) - solo para 3.0TD y 6.1TD

- **Factura Actual** (opcional): Para calcular el ahorro potencial

### 2. Resultados de la Comparativa

La tabla de resultados muestra:

- **Posición**: Ranking de las tarifas (la #1 es la más barata)
- **Comercializadora**: Nombre de la empresa
- **Tarifa**: Nombre de la tarifa específica
- **Coste Anual**: Precio total anual sin impuestos
- **Coste Mensual**: Precio mensual (anual / 12)
- **Ahorro Mensual**: Si se introdujo factura actual, muestra cuánto se ahorraría
- **Ahorro Anual**: Ahorro total en el año
- **Desglose**: Botón para ver detalles completos

### 3. Modal de Detalles

Al hacer clic en "Ver detalles", se muestra:

- Desglose de costes:
  - Término fijo anual
  - Coste de potencia anual
  - Coste de energía anual
  - Total anual y mensual

- Comparativa con tarifa actual (si se introdujo el precio actual)
- Nota sobre impuestos (los precios no incluyen IVA 21% e impuesto eléctrico 5.11%)

## Cálculo de Costes

El sistema calcula el coste total anual como:

```
Coste Término Fijo = Precio Término Fijo Diario × 365 días

Coste Potencia = (Precio Potencia P1 × Potencia P1 + 
                  Precio Potencia P2 × Potencia P2 + 
                  Precio Potencia P3 × Potencia P3) × 365 días

Coste Energía = (Precio Energía P1 × Consumo P1 + 
                 Precio Energía P2 × Consumo P2 + 
                 Precio Energía P3 × Consumo P3)

Coste Total Anual = Coste Término Fijo + Coste Potencia + Coste Energía
```

## Datos de Tarifas (IMPORTANTE)

### Implementación Actual

La versión actual utiliza **datos de ejemplo ficticios** de 6 comercializadoras españolas principales:

1. Iberdrola - Tarifa One Luz
2. Endesa - Tempo Happy
3. Naturgy - Tarifa Precio Fijo
4. Repsol - Tarifa Sin Permanencia
5. Holaluz - Tarifa Plana Solar
6. Factor Energía - Tarifa Estable

Estos datos están **hardcodeados en el código** con precios orientativos del mercado español.

### Personalización para Producción

Para adaptar el comparador a tus tarifas reales, tienes **3 opciones**:

#### **Opción 1: Modificar los datos de ejemplo (más rápido)**

Edita el método `CompararTarifas()` en `ComparadorEnergia.razor` (línea ~479) y actualiza la lista `tarifasEjemplo` con tus propias tarifas:

```csharp
var tarifasEjemplo = new List<DatosTarifaEjemplo>
{
    new DatosTarifaEjemplo 
    { 
        Comercializadora = "Tu Comercializadora", 
        NombreTarifa = "Tu Tarifa",
        TerminoFijoDiario = 0.12m,  // €/día
        PrecioPotenciaP1 = 0.105m,  // €/kW/día
        PrecioPotenciaP2 = 0.046m,  // €/kW/día
        PrecioPotenciaP3 = 0.0034m, // €/kW/día
        PrecioEnergiaP1 = 0.168m,   // €/kWh
        PrecioEnergiaP2 = 0.098m,   // €/kWh
        PrecioEnergiaP3 = 0.065m    // €/kWh
    },
    // Añade más tarifas aquí...
};
```

#### **Opción 2: Crear tabla de tarifas detalladas (recomendado para producción)**

1. Crea una nueva tabla en PostgreSQL:

```sql
CREATE TABLE tarifas_energia_detalladas (
    id SERIAL PRIMARY KEY,
    comercializadora VARCHAR(255) NOT NULL,
    nombre_tarifa VARCHAR(255) NOT NULL,
    tipo_tarifa VARCHAR(10) NOT NULL, -- 2.0TD, 3.0TD, 6.1TD
    termino_fijo_diario DECIMAL(10,5) NOT NULL,
    precio_potencia_p1 DECIMAL(10,5) NOT NULL,
    precio_potencia_p2 DECIMAL(10,5) NOT NULL,
    precio_potencia_p3 DECIMAL(10,5),
    precio_energia_p1 DECIMAL(10,5) NOT NULL,
    precio_energia_p2 DECIMAL(10,5) NOT NULL,
    precio_energia_p3 DECIMAL(10,5),
    activa BOOLEAN DEFAULT TRUE,
    fecha_creacion TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);
```

2. Crea el modelo correspondiente en `Models/TarifaEnergiaDetallada.cs`
3. Crea el servicio `TarifaEnergiaDetalladaService.cs`
4. Modifica `CompararTarifas()` para obtener las tarifas de la base de datos

#### **Opción 3: Modificar tabla tarifasenergia existente**

Añade columnas numéricas a la tabla existente `tarifasenergia`:

```sql
ALTER TABLE tarifasenergia
ADD COLUMN termino_fijo_diario DECIMAL(10,5),
ADD COLUMN precio_potencia_p1 DECIMAL(10,5),
ADD COLUMN precio_potencia_p2 DECIMAL(10,5),
ADD COLUMN precio_potencia_p3 DECIMAL(10,5),
ADD COLUMN precio_energia_p1 DECIMAL(10,5),
ADD COLUMN precio_energia_p2 DECIMAL(10,5),
ADD COLUMN precio_energia_p3 DECIMAL(10,5);
```

Luego actualiza el modelo `TarifaEnergia.cs` y modifica el método `CompararTarifas()` para usar estos campos.

## Precios de Referencia (Mercado Español 2025)

Para ayudarte a configurar tus tarifas, aquí tienes precios orientativos del mercado español:

### Término Fijo
- Entre 0.10€ y 0.15€ por día

### Precios de Potencia (€/kW/día)
- Periodo 1 (Punta): 0.10€ - 0.12€
- Periodo 2 (Llano/Valle): 0.04€ - 0.05€
- Periodo 3 (Valle): 0.003€ - 0.004€

### Precios de Energía (€/kWh)
- Periodo 1 (Punta): 0.15€ - 0.18€
- Periodo 2 (Llano): 0.09€ - 0.11€
- Periodo 3 (Valle): 0.06€ - 0.08€

**Nota**: Estos son precios sin impuestos. El precio final incluye IVA (21%) e impuesto eléctrico (5.11%).

## Mejoras Futuras Sugeridas

1. **Filtro por comercializadora**: Permitir seleccionar comercializadoras específicas
2. **Filtro por tipo de energía**: Verde, 100% renovable, etc.
3. **Gráficos comparativos**: Visualización con Chart.js o similar
4. **Exportar resultados**: A PDF o Excel
5. **Calculadora de consumo**: Ayuda para estimar consumos si no se conocen
6. **Histórico de comparativas**: Guardar búsquedas previas
7. **Alertas de ahorro**: Notificar cuando una nueva tarifa sea más económica
8. **Integración con APIs**: Obtener precios actualizados de APIs de comercializadoras

## Notas Técnicas

- Los cálculos se realizan en el frontend (Blazor Server)
- No se requiere autenticación especial (disponible para todos los usuarios)
- Los precios mostrados NO incluyen impuestos (se indica claramente al usuario)
- El sistema es responsive y funciona en móviles

## Soporte

Para modificaciones o dudas sobre el comparador, consulta:
- Código fuente: `/Components/Pages/ComparadorEnergia.razor`
- Menú de navegación: `/Components/Layout/NavMenu.razor`
