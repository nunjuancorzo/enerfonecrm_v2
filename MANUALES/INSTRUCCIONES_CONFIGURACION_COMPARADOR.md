# Configuración de Precios para Comparador de Tarifas de Energía

## 📋 Pasos para Configurar

### 1. Ejecutar el Script SQL

Primero, debes añadir las columnas de precios a la tabla `tarifasenergia`:

```bash
psql -U postgres -d enerfonecrm -f Scripts/agregar_campos_precios_tarifas_energia.sql
```

O bien, conéctate a tu base de datos y ejecuta el SQL directamente.

### 2. Configurar Precios en las Tarifas Existentes

Una vez ejecutado el script, debes actualizar tus tarifas existentes con los precios. Puedes hacerlo de dos formas:

#### Opción A: Mediante SQL

```sql
-- Ejemplo: Actualizar una tarifa específica
UPDATE tarifasenergia 
SET termino_fijo_diario = 0.12,    -- €/día
    precio_potencia_p1 = 0.105,     -- €/kW/día
    precio_potencia_p2 = 0.046,     -- €/kW/día
    precio_potencia_p3 = 0.0034,    -- €/kW/día
    precio_energia_p1 = 0.168,      -- €/kWh
    precio_energia_p2 = 0.098,      -- €/kWh
    precio_energia_p3 = 0.065       -- €/kWh
WHERE id = 1;  -- Ajusta el ID según tu tarifa
```

#### Opción B: Desde la aplicación (Recomendado)

1. Ve a **Tarifas > Energía** en el menú
2. Edita cada tarifa
3. Completa los nuevos campos de precios:
   - Término Fijo Diario (€/día)
   - Precio Potencia P1 (€/kW/día)
   - Precio Potencia P2 (€/kW/día)
   - Precio Potencia P3 (€/kW/día) - opcional
   - Precio Energía P1 (€/kWh)
   - Precio Energía P2 (€/kWh)
   - Precio Energía P3 (€/kWh) - opcional
4. Guarda los cambios

### 3. Verificar que Funciona

1. Ve a **Comparador** en el menú principal
2. Introduce datos de ejemplo:
   - Tipo de tarifa: 2.0TD
   - Potencia P1: 3.45 kW
   - Potencia P2: 3.45 kW
   - Consumo P1: 1200 kWh/año
   - Consumo P2: 1800 kWh/año
3. Haz clic en "Comparar Tarifas"
4. Deberías ver las tarifas que tengan precios configurados

## 📊 Guía de Precios de Referencia (España 2025)

### Término Fijo Diario
- **Rango típico**: 0.10€ - 0.15€ por día
- **Promedio**: 0.12€/día

### Precios de Potencia (€/kW/día)

| Periodo | Descripción | Rango | Valor Típico |
|---------|-------------|-------|--------------|
| P1 | Punta | 0.100 - 0.120 | 0.105 |
| P2 | Llano/Valle | 0.040 - 0.055 | 0.046 |
| P3 | Valle (solo 3.0TD y 6.1TD) | 0.003 - 0.004 | 0.0034 |

### Precios de Energía (€/kWh)

| Periodo | Descripción | Rango | Valor Típico |
|---------|-------------|-------|--------------|
| P1 | Punta | 0.150 - 0.180 | 0.168 |
| P2 | Llano | 0.090 - 0.110 | 0.098 |
| P3 | Valle | 0.060 - 0.080 | 0.065 |

### Horarios de los Periodos (Península)

**Tarifa 2.0TD (2 periodos)**
- **P1 (Punta)**: L-V de 10h-14h y 18h-22h
- **P2 (Valle)**: Resto de horas y festivos

**Tarifa 3.0TD (3 periodos)**
- **P1 (Punta)**: L-V de 10h-14h y 18h-22h
- **P2 (Llano)**: L-V de 8h-10h, 14h-18h y 22h-24h
- **P3 (Valle)**: L-V de 0h-8h, S-D y festivos

## 💡 Ejemplos de Configuración

### Ejemplo 1: Tarifa Económica
```sql
UPDATE tarifasenergia 
SET termino_fijo_diario = 0.10,
    precio_potencia_p1 = 0.100,
    precio_potencia_p2 = 0.042,
    precio_potencia_p3 = 0.0030,
    precio_energia_p1 = 0.160,
    precio_energia_p2 = 0.092,
    precio_energia_p3 = 0.060
WHERE nombre = 'Tarifa Económica';
```

### Ejemplo 2: Tarifa Premium
```sql
UPDATE tarifasenergia 
SET termino_fijo_diario = 0.13,
    precio_potencia_p1 = 0.110,
    precio_potencia_p2 = 0.050,
    precio_potencia_p3 = 0.0036,
    precio_energia_p1 = 0.175,
    precio_energia_p2 = 0.105,
    precio_energia_p3 = 0.070
WHERE nombre = 'Tarifa Premium';
```

### Ejemplo 3: Tarifa Verde
```sql
UPDATE tarifasenergia 
SET termino_fijo_diario = 0.115,
    precio_potencia_p1 = 0.108,
    precio_potencia_p2 = 0.048,
    precio_potencia_p3 = 0.0035,
    precio_energia_p1 = 0.172,
    precio_energia_p2 = 0.102,
    precio_energia_p3 = 0.068
WHERE nombre = 'Tarifa 100% Verde';
```

## ⚠️ Notas Importantes

1. **Todos los precios son SIN IMPUESTOS**: Los precios mostrados al usuario en el comparador no incluyen IVA (21%) ni impuesto eléctrico (5.11%)

2. **Campos Obligatorios Mínimos**: Para que una tarifa aparezca en el comparador, debe tener configurados:
   - `termino_fijo_diario` > 0
   - `precio_potencia_p1` > 0
   - `precio_energia_p1` > 0

3. **Campos Opcionales**: 
   - P2 y P3 de potencia y energía son opcionales
   - Si no se configuran, se usará 0 en los cálculos

4. **Precisión Decimal**: Los campos admiten hasta 5 decimales para máxima precisión

5. **Actualización de Precios**: Puedes actualizar los precios en cualquier momento y se reflejarán inmediatamente en el comparador

## 🔍 Verificación de Datos

Para ver qué tarifas tienen precios configurados:

```sql
SELECT 
    empresa,
    nombre,
    termino_fijo_diario,
    precio_potencia_p1,
    precio_energia_p1
FROM tarifasenergia
WHERE termino_fijo_diario > 0 
  AND precio_potencia_p1 > 0 
  AND precio_energia_p1 > 0;
```

## 🚀 Siguientes Pasos

Una vez configurados los precios:

1. ✅ Verifica que las tarifas aparecen en el comparador
2. ✅ Prueba con diferentes configuraciones de consumo
3. ✅ Comprueba que los cálculos son correctos
4. ✅ Forma a tu equipo comercial en el uso del comparador
5. ✅ Actualiza periódicamente los precios según las ofertas del mercado

## 📞 Soporte

Si necesitas ayuda para configurar los precios o ajustar el comparador, consulta:
- Archivo del modelo: `/Models/TarifaEnergia.cs`
- Comparador: `/Components/Pages/ComparadorEnergia.razor`
- Script SQL: `/Scripts/agregar_campos_precios_tarifas_energia.sql`
