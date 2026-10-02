# Mejoras en Consulta SIPS - CSV y Bono Social

**Fecha:** 27 de marzo de 2026

## Resumen de Cambios

Se ha actualizado el servicio SIPS para utilizar la nueva API que devuelve datos en formato CSV y se ha añadido la visualización del campo "Bono Social" en la interfaz de usuario.

## Cambios Técnicos Implementados

### 1. Actualización de SipsService.cs

#### Nueva Arquitectura de Llamadas API

**Para Electricidad (Luz):**
- Se realizan **DOS llamadas** a sips3.php:
  1. `id=1`: Información del punto de suministro (cliente)
  2. `id=2`: Consumos históricos (para gráficos)
- No requiere API key
- Devuelve datos en formato CSV

**Para Gas:**
- Se usa sips2.php con parámetro `gas=1`
- Requiere API key: OPT7393
- URL: `http://35.181.7.83/api/sips2.php?id={CUPS}&key=OPT7393&gas=1`

#### Métodos Nuevos Agregados

**`DeserializarRespuesta(string contenido)`**
- Detecta automáticamente si la respuesta es JSON o CSV
- Enruta al parser correspondiente

**`ParsearCsv(string csv)`**
- Parsea el formato CSV devuelto por sips3.php id=1 (información del cliente)
- Mapea las cabeceras CSV a propiedades del modelo `ClienteSips`
- Convierte potencias de Watts a kW automáticamente
- Extrae todos los campos incluyendo el **Bono Social**
- **Parser robusto**: Maneja correctamente valores entrecomillados con comas

**`ParsearCsvConsumos(string csv)`** ✨ (NUEVO)
- Parsea el formato CSV devuelto por sips3.php id=2 (consumos históricos)
- Devuelve `List<ConsumoSips>` con todos los periodos de facturación
- Convierte consumos de Wh (vatio-hora) a kWh (kilovatio-hora)
- Maneja P1, P2, P3, P4, P5, P6 (períodos tarifarios)
- Filtra registros incompletos (sin fechas válidas)
- Permite generar gráficos de consumo evolutivo

**`ConsultarApiAsync(string url)`** ✨ (NUEVO)
- Método auxiliar para realizar llamadas HTTP a APIs SIPS
- Manejo centralizado de errores HTTP
- Logging detallado de requests/responses
- Validación de contenido no vacío

**`ParsearLineaCsv(string linea)`** ✨ 
- Parser CSV personalizado que respeta el formato estándar RFC 4180
- Maneja correctamente valores entre comillas: `"ENDESA DISTRIBUCION ELECTRICA, S.L."`
- Soporta comillas escapadas dentro de valores (`""`)
- Divide campos por comas respetando el contexto de comillas
- Evita errores de "cabeceras vs valores" causados por split simple

**Métodos auxiliares agregados:**
- `ObtenerValor()`: Extrae valores del diccionario CSV
- `ParsearFecha()`: Convierte fechas en formato yyyy-MM-dd
- `ParsearDecimal()`: Convierte valores numéricos
- `ParsearDecimalKw()`: Convierte Watts a kiloWatts (divide por 1000)

### 2. Actualización de SIPS.razor

Se ha añadido la visualización del **Bono Social** en la sección de "Fechas Importantes":

```razor
<div class="col-md-6 mb-2">
    <strong><i class="bi bi-shield-check text-success me-2"></i>Bono Social:</strong>
    @if (datosCliente.AplicacionBonoSocial == "0" || string.IsNullOrEmpty(datosCliente.AplicacionBonoSocial))
    {
        <p class="mb-0 text-muted"><span class="badge bg-secondary">No aplicado</span></p>
    }
    else
    {
        <p class="mb-0 text-muted"><span class="badge bg-success">Sí aplicado</span></p>
    }
</div>
```

**Funcionalidad:**
- Muestra "No aplicado" (badge gris) cuando el valor es "0" o vacío
- Muestra "Sí aplicado" (badge verde) cuando tiene cualquier otro valor

## Campos CSV Procesados

El parser CSV extrae los siguientes campos:

### Información Básica
- CUPS (Código Único Punto de Suministro)
- Distribuidora (código y nombre)
- Ubicación (código postal, municipio, provincia)

### Datos Técnicos
- Fecha alta suministro
- Tarifa ATR en vigor
- Código tensión
- Potencias máximas BIE y APM
- Tipo perfil consumo
- Fases equipo medida (M=Monofásico, T=Trifásico)

### Potencias Contratadas
- P1, P2, P3, P4, P5, P6 (convertidas automáticamente de W a kW)

### Información Contractual
- Fecha último movimiento contrato
- Fecha último cambio comercializador
- Fecha última lectura
- Código comercializadora
- Tipo contrato
- Periodicidad facturación

### Información del Titular
- Tipo ID titular
- Es vivienda habitual
- CNAE
- **Aplicación Bono Social** ✨ (NUEVO)
- Información impagos
- Depósito garantía

### Datos Técnicos Avanzados
- Telegestion
- Autoconsumo
- Propiedad equipo medida
- Disponibilidad ICP

## Pruebas Realizadas

### CUPS de Prueba

**CUPS Válido (con datos):**
- `ES0031101823268022VG0F` ✅
  - Distribuidora: ENDESA
  - Provincia: Badajoz (06)
  - Potencia: 9.8 kW
  - Bono Social: No aplicado (0)

**CUPS Sin Datos:**
- `ES0207000076289487ZM` ❌
  - Mensaje API: "No se encontraron resultados para los parámetros proporcionados"

### Respuesta CSV de Ejemplo

```csv
codigoEmpresaDistribuidora,cups,nombreEmpresaDistribuidora,...,aplicacionBonoSocial
"0031","ES0031101823268022VG0F","ENDESA DISTRIBUCION ELECTRICA, S.L.",...,"0"
```

## Compatibilidad

El sistema mantiene **compatibilidad total** con la API anterior (sips2.php):
- Respuestas JSON se procesan con el método `DeserializarJson()`
- Respuestas CSV se procesan con el método `ParsearCsv()`
- La detección es automática según el contenido de la respuesta

## Beneficios

✅ **Información más completa:** La API sips3.php devuelve más datos que sips2.php  
✅ **Mayor tasa de éxito:** CUPS que no funcionaban con sips2.php ahora devuelven información  
✅ **Transparencia:** Los usuarios pueden ver si un suministro tiene bono social aplicado  
✅ **Conversión automática:** Las potencias y consumos se muestran en kW/kWh (más legible que W/Wh)  
✅ **Retrocompatibilidad:** El sistema sigue funcionando si alguna consulta devuelve JSON  
✅ **Gráficos de consumo:** Visualización temporal de consumos históricos 📊  
✅ **Análisis por períodos:** Desglose P1, P2, P3 para optimización tarifaria  
✅ **Histórico completo:** Tabla con todos los períodos de facturación disponibles  
✅ **Robustez:** Si falla la consulta de consumos, aún se muestran datos del cliente

## Configuración

### Para Electricidad (Luz)
```csharp
// Llamada 1: Información del cliente/suministro
url = "http://35.181.7.83/api/sips3.php?id=1&cups={CUPS}";

// Llamada 2: Consumos históricos (para gráficos)
url = "http://35.181.7.83/api/sips3.php?id=2&cups={CUPS}";
```

**Ejemplo de consulta completa:**
1. Usuario introduce CUPS de electricidad: `ES0031101823268022VG0F`
2. Sistema realiza llamada a id=1 → Obtiene datos del cliente (distribuidora, potencias, bono social, etc.)
3. Sistema realiza llamada a id=2 → Obtiene consumos históricos (últimos 12-24 meses)
4. Se muestran datos del cliente + gráficos de consumo evolutivo

### Para Gas
```csharp
// Gas: usar sips2.php con id=CUPS y parámetro gas=1
url = $"{API_URL}?id={Uri.EscapeDataString(cupsNormalizado)}&key={API_KEY}&gas=1";
```

**Nota:** La API de gas actualmente devuelve respuestas vacías para algunos CUPS. Esto podría indicar:
- CUPS de gas no disponibles en la base de datos SIPS actual
- Necesidad de usar una API diferente para gas
- Pendiente verificar con proveedor de la API

## Problemas Solucionados

### Error: "CSV inválido: 57 cabeceras vs 58 valores"

**Causa:**
El parser CSV inicial usaba un `Split(',')` simple que no respetaba las comillas. Cuando un valor CSV contenía comas dentro de comillas (como `"ENDESA DISTRIBUCION ELECTRICA, S.L."`), el split lo dividía incorrectamente en múltiples campos.

**Ejemplo del problema:**
```csv
nombreEmpresaDistribuidora,codigoPostalPS
"ENDESA DISTRIBUCION ELECTRICA, S.L.","06800"
```

Con `Split(',')` simple:
- Cabeceras: 2 campos
- Valores: 3 campos (se divide en "ENDESA DISTRIBUCION ELECTRICA", " S.L.", "06800")

**Solución:**
Implementación del método `ParsearLineaCsv()` que:
1. Rastrea si está dentro de comillas
2. Solo divide por comas cuando NO está dentro de comillas
3. Maneja comillas dobles escapadas (`""`)
4. Respeta el estándar RFC 4180 de CSV

**Resultado:**
- ✅ Parsing correcto de todos los campos CSV
- ✅ Nombres de distribuidoras con comas se procesan correctamente
- ✅ Coincidencia exacta entre cabeceras y valores

## Gráficos de Consumo Implementados

### Datos de Consumo Eléctrico

La API sips3.php con id=2 devuelve histórico de consumos con los siguientes campos:

**Campos CSV de Consumos:**
- `cups`: Código CUPS del punto de suministro
- `fechaInicioMesConsumo`: Fecha inicio del período
- `fechaFinMesConsumo`: Fecha fin del período
- `codigoTarifaATR`: Código de tarifa aplicada
- `consumoEnergiaActivaEnWhP1-P6`: Consumo activo por período (en Wh)
- `consumoEnergiaReactivaEnVArhP1-P6`: Consumo reactivo por período (en VArh)
- `potenciaDemandadaEnWP1-P6`: Potencia demandada por período (en W)
- `codigoDHEquipoDeMedida`: Código equipo de medida
- `codigoTipoLectura`: Tipo de lectura

**Procesamiento:**
1. CSV parseado línea por línea
2. Conversión automática Wh → kWh (divide entre 1000)
3. Filtrado de registros incompletos
4. Ordenación temporal (más recientes primero)

**Visualización:**
- **Gráfico 1**: Evolución del consumo mensual total
- **Gráfico 2**: Distribución por períodos (P1, P2, P3...)
- **Tabla**: Histórico completo con totales acumulados

### Diferencias Gas vs Electricidad

| Aspecto | Electricidad | Gas |
|---------|-------------|-----|
| API | sips3.php (id=1 y id=2) | sips2.php (gas=1) |
| Formato | CSV | JSON (previsto) |
| Llamadas | 2 (cliente + consumos) | 1 (cliente) |
| API Key | No requerida | OPT7393 |
| Gráficos | ✅ Sí (histórico disponible) | ❌ Pendiente implementar |
| Bono Social | ✅ Sí | ❓ Pendiente verificar |

## Notas Técnicas

- El CSV puede contener valores vacíos representados como `,,` (comas consecutivas)
- Los valores numéricos se convierten usando `CultureInfo.InvariantCulture` para evitar problemas de formato
- Las fechas se esperan en formato ISO 8601: `yyyy-MM-dd`
- El bono social se almacena como string ("0" o "1")
- Los logs incluyen información detallada para debugging de problemas CSV

## Archivos Modificados

1. **EnerfoneCRM/Services/SipsService.cs**
   - Método `DeserializarRespuesta()` - Detección automática CSV/JSON
   - Método `ParsearCsv()` - Parser completo CSV
   - Métodos auxiliares de parsing

2. **EnerfoneCRM/Components/Pages/SIPS.razor**
   - Visualización del campo Bono Social con badges de color

## Próximos Pasos (Opcional)

- [ ] Agregar más CUPS de prueba para validar diferentes escenarios
- [ ] Implementar caché específica para respuestas CSV
- [ ] Añadir documentación de códigos (ej: qué significa código Fases "T" vs "M")
- [ ] Considerar mostrar más campos técnicos en la UI según necesidades del usuario
