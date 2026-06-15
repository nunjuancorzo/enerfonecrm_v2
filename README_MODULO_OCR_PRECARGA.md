# Módulo de Comparativas con OCR y PreCarga

## 📋 Descripción General

Se ha ampliado el **Comparador de Tarifas de Energía** con nuevas funcionalidades avanzadas basadas en las especificaciones del plugin WordPress "Enerfone Comparador OCR de Facturas". El módulo ahora incluye:

1. **Análisis de Facturas con OCR** - Extracción automática de datos de facturas PDF/imágenes
2. **Sistema PreCarga** - Mapeo visual de facturas por comercializadora
3. **Histórico de Comparativas** - Registro completo de todas las comparativas realizadas
4. **Mejoras en PDF** - Documentos más completos y profesionales
5. **Envío por Email** - Envío automático de resultados a clientes

---

## 🚀 Nuevas Funcionalidades

### 1. Análisis de Facturas con OCR

#### Ubicación
- **URL**: `/comparador/analizar-factura`
- **Archivo**: `Components/Pages/AnalizarFactura.razor`

#### Características

- **Subida de Archivos**: PDF, JPG, PNG (máximo 10 MB)
- **Procesamiento OCR Automático**:
  - Azure Document Intelligence (Form Recognizer)
  - OpenAI Vision (GPT-4o con visión)
  - Google Vision (preparado, pendiente implementación)
- **Fallback Automático**: Si el proveedor principal falla, usa el secundario
- **Modo Manual**: Permite introducir datos manualmente si el OCR falla

#### Flujo de Uso

1. **Subir Factura**
   - El usuario selecciona un archivo PDF o imagen de su factura
   - El sistema valida formato y tamaño

2. **Procesamiento OCR**
   - Se envía al proveedor configurado (Azure u OpenAI)
   - Extrae automáticamente:
     - CUPS
     - Comercializadora actual
     - Tarifa actual
     - Potencias contratadas (P1, P2, P3...)
     - Consumos por periodo
     - Total de la factura
     - Peaje de luz/gas

3. **Revisión y Corrección**
   - Los datos extraídos se muestran en un formulario editable
   - El usuario puede corregir cualquier dato antes de calcular

4. **Cálculo de Comparativa**
   - Compara con todas las tarifas activas de la base de datos
   - Ordena por coste mensual (menor a mayor)
   - Calcula ahorros y porcentajes

5. **Resultados**
   - Tabla con ranking de tarifas
   - Opción de descargar PDF
   - Opción de enviar por email

#### Configuración OCR

La configuración se realiza en la tabla `configuracion_empresa`:

```sql
-- Configuración Azure Document Intelligence
UPDATE configuracion_empresa SET
    ocr_proveedor = 'azure',
    ocr_endpoint = 'https://RESOURCE.cognitiveservices.azure.com/',
    ocr_api_key = 'YOUR_API_KEY',
    ocr_timeout = 30,
    ocr_proveedor_secundario = 'openai',
    ocr_fallback_automatico = TRUE;

-- Configuración OpenAI Vision
UPDATE configuracion_empresa SET
    ocr_proveedor = 'openai',
    ocr_api_key = 'sk-...',
    ocr_modelo = 'gpt-4o',
    ocr_timeout = 30;
```

---

### 2. Sistema PreCarga

#### ¿Qué es PreCarga?

PreCarga es un sistema de **mapeo visual de facturas** que permite "enseñar" al sistema dónde están ubicados los datos en las facturas de cada comercializadora.

#### Ventajas

- **Mejora la precisión del OCR**: Al conocer la ubicación exacta de cada campo
- **Adaptable por comercializadora**: Cada una tiene su propio diseño de factura
- **Soporte multipágina**: Mapea campos en diferentes páginas del PDF
- **Coordenadas relativas**: Funciona independientemente del tamaño del documento

#### Funcionamiento (Pendiente de Implementación UI)

1. **Crear Plantilla**
   - El administrador crea una nueva plantilla
   - Indica comercializadora y tipo de energía

2. **Subir Factura de Muestra**
   - Se carga una factura típica de esa comercializadora
   - Se muestra una vista previa del PDF

3. **Mapear Campos**
   - Para cada campo (CUPS, Total, Consumo P1, etc.):
     - Seleccionar el campo en un menú
     - Dibujar un rectángulo sobre la zona donde aparece
     - El sistema guarda las coordenadas relativas

4. **Guardar Plantilla**
   - La plantilla se guarda con todos los campos mapeados
   - Se activa para uso futuro

5. **Uso Automático**
   - Cuando se sube una factura de esa comercializadora
   - El sistema usa la plantilla para localizar los datos
   - Envía coordenadas precisas al OCR

#### Modelo de Datos

```csharp
public class PlantillaPreCarga
{
    public int Id { get; set; }
    public string Nombre { get; set; }
    public string Comercializadora { get; set; }
    public string? AliasComercializadora { get; set; } // "Iberdrola, Iberdrola Clientes"
    public string TipoEnergia { get; set; } // "LUZ", "GAS", "LUZ+GAS"
    public string? VarianteFactura { get; set; }
    public int Prioridad { get; set; }
    public bool Activa { get; set; }
    public string? CamposMapeados { get; set; } // JSON
    public DateTime FechaCreacion { get; set; }
}

public class CampoMapeadoPreCarga
{
    public string Field { get; set; } // "cups", "total_factura"
    public string Label { get; set; } // "CUPS", "Total Factura"
    public int Page { get; set; } // Número de página (1-based)
    public double X { get; set; } // Posición X relativa (0-1)
    public double Y { get; set; } // Posición Y relativa (0-1)
    public double W { get; set; } // Ancho relativo (0-1)
    public double H { get; set; } // Alto relativo (0-1)
    public string? TipoDato { get; set; } // "texto", "numero", "fecha"
}
```

#### Servicio

- **Archivo**: `Services/PlantillaPreCargaService.cs`
- **Métodos**:
  - `ObtenerTodasAsync()` - Lista todas las plantillas
  - `ObtenerActivasAsync()` - Solo plantillas activas
  - `BuscarPlantillaParaFacturaAsync(comercializadora, tipoEnergia)` - Busca la plantilla adecuada
  - `CrearAsync(plantilla)` - Crea nueva plantilla
  - `GuardarCamposMapeadosAsync(plantillaId, campos)` - Guarda el mapeo
  - `ObtenerCamposMapeados(plantilla)` - Deserializa el JSON de campos

---

### 3. Histórico de Comparativas

#### Ubicación
- **URL**: `/comparador/historico`
- **Archivo**: `Components/Pages/HistoricoComparativas.razor`

#### Características

- **Registro Completo**:
  - Fecha y hora de cada comparativa
  - Origen (frontend/backend)
  - Datos utilizados (JSON)
  - Ranking completo (JSON)
  - Mejor tarifa encontrada
  - Ahorro calculado
  - Proveedor OCR utilizado
  - Estado de envío de email
  - Usuario que realizó la comparativa

- **Filtros Avanzados**:
  - Por rango de fechas
  - Por origen (frontend/backend)
  - Por tipo de energía
  - Por usuario (si no es superadmin)

- **Estadísticas**:
  - Total de comparativas
  - Emails enviados
  - Comparativas con OCR
  - Comparativas desde backend

- **Privacidad**:
  - CUPS anonimizado en la lista
  - Acceso restringido por usuario (solo superadmin ve todas)
  - No se guardan facturas originales (solo metadatos)

#### Modelo de Datos

```csharp
public class HistoricoComparativa
{
    public int Id { get; set; }
    public DateTime FechaComparativa { get; set; }
    public string Origen { get; set; } // "frontend", "backend"
    public string? EmailCliente { get; set; }
    public string TipoEnergia { get; set; } // "LUZ", "GAS", "LUZ+GAS"
    public string? Cups { get; set; }
    public string? ComercializadoraActual { get; set; }
    public string? TarifaActual { get; set; }
    public decimal? TotalFacturaActual { get; set; }
    public int? MejorTarifaId { get; set; }
    public string? MejorTarifaNombre { get; set; }
    public string? MejorTarifaEmpresa { get; set; }
    public decimal? AhorroMensual { get; set; }
    public decimal? AhorroAnual { get; set; }
    public decimal? PorcentajeAhorro { get; set; }
    public string? DatosUtilizados { get; set; } // JSON
    public string? ResultadoRanking { get; set; } // JSON
    public string? ProveedorOcr { get; set; }
    public bool EmailEnviado { get; set; }
    public DateTime? FechaEnvioEmail { get; set; }
    public string? Advertencias { get; set; }
    public int? UsuarioId { get; set; }
    public string? NombreArchivoFactura { get; set; }
}
```

---

### 4. Servicios Implementados

#### OcrService

**Ubicación**: `Services/OcrService.cs`

Responsable de procesar facturas con OCR:

```csharp
public class ResultadoOcr
{
    public bool Exito { get; set; }
    public string Mensaje { get; set; }
    public Dictionary<string, string> DatosExtraidos { get; set; }
    public string ProveedorUtilizado { get; set; }
    public string TextoCompleto { get; set; }
}

// Uso
var resultado = await OcrService.ProcesarFacturaAsync(bytesArchivo, nombreArchivo, plantilla);
if (resultado.Exito)
{
    var cups = resultado.DatosExtraidos["cups"];
    var totalFactura = resultado.DatosExtraidos["total_factura"];
    // ...
}
```

**Proveedores Soportados**:

1. **Tesseract OCR** ⭐ **GRATUITO**
   - Completamente gratis y open-source
   - Se ejecuta localmente (sin costos de API)
   - Sin límites de uso
   - Requiere: Instalación en el servidor
   - Precisión: 80-95% según calidad de imagen
   - **Ver**: [TESSERACT_OCR_SETUP.md](TESSERACT_OCR_SETUP.md)

2. **Azure Document Intelligence**
   - Usa el modelo prebuilt-invoice
   - Endpoint: `formrecognizer/documentModels/prebuilt-invoice:analyze`
   - Requiere: `ocr_endpoint` y `ocr_api_key`
   - Capa gratuita: 500 páginas/mes
   - Precisión: 95-98%

3. **OpenAI Vision**
   - Usa GPT-4o o modelo configurado
   - Envía imagen en base64
   - Requiere: `ocr_api_key` y opcionalmente `ocr_modelo`
   - Costo: ~$0.01-0.03 por imagen
   - Precisión: 96-98%

4. **Google Vision**
   - Preparado pero no implementado
   - Espera PR para completar
   - Capa gratuita: 1,000 imágenes/mes

#### PlantillaPreCargaService

**Ubicación**: `Services/PlantillaPreCargaService.cs`

Gestiona las plantillas de mapeo visual:

```csharp
// Buscar plantilla para una factura
var plantilla = await PlantillaPreCargaService.BuscarPlantillaParaFacturaAsync("Iberdrola", "LUZ");

// Obtener campos mapeados
var campos = PlantillaPreCargaService.ObtenerCamposMapeados(plantilla);
// Lista de CampoMapeadoPreCarga con coordenadas

// Crear nueva plantilla
var resultado = await PlantillaPreCargaService.CrearAsync(nuevaPlantilla);

// Guardar campos mapeados
await PlantillaPreCargaService.GuardarCamposMapeadosAsync(plantillaId, listaCampos);
```

#### HistoricoComparativaService

**Ubicación**: `Services/HistoricoComparativaService.cs`

Gestiona el histórico de comparativas:

```csharp
// Guardar comparativa
var resultado = await HistoricoService.GuardarComparativaAsync(
    origen: "backend",
    tipoEnergia: "LUZ",
    datosUtilizados: new { cups, potencias, consumos },
    resultadoRanking: listadoTarifas,
    cups: cups,
    mejorTarifaId: mejorTarifa.Id,
    ahorroMensual: ahorro,
    usuarioId: usuario.Id
);

// Buscar con filtros
var historico = await HistoricoService.BuscarAsync(
    fechaDesde: DateTime.Now.AddDays(-30),
    origen: "backend",
    tipoEnergia: "LUZ"
);

// Estadísticas
var stats = await HistoricoService.ObtenerEstadisticasAsync();
Console.WriteLine($"Total: {stats["total"]}, Con OCR: {stats["con_ocr"]}");
```

---

## 📦 Instalación y Configuración

### 1. Ejecutar Script SQL

Ejecuta el script para crear las nuevas tablas:

```bash
mysql -u root -p corcrmdb < ADD_MODULO_COMPARATIVAS_OCR.sql
```

Este script crea:
- Tabla `historico_comparativas`
- Tabla `plantillas_precarga`
- Campos OCR en `configuracion_empresa`

### 2. Configurar Proveedor OCR

#### ⭐ Opción A: Tesseract OCR (GRATUITO - Recomendado para empezar)

1. Instalar Tesseract en el servidor:

```bash
./install-tesseract.sh
```

2. Verificar instalación:

```bash
tesseract --version
tesseract --list-langs | grep spa
```

3. Ya está configurado por defecto en la BD (el script SQL lo configura)

**Documentación completa**: [TESSERACT_OCR_SETUP.md](TESSERACT_OCR_SETUP.md)

#### Opción B: Azure Document Intelligence (Capa gratuita: 500/mes)

1. Crear recurso en Azure Portal
2. Obtener endpoint y API key
3. Actualizar configuración:

```sql
UPDATE configuracion_empresa SET
    ocr_proveedor = 'azure',
    ocr_endpoint = 'https://YOUR_RESOURCE.cognitiveservices.azure.com/',
    ocr_api_key = 'YOUR_API_KEY',
    ocr_proveedor_secundario = 'tesseract',
    ocr_timeout = 30;
```

#### Opción C: OpenAI Vision (De pago)

1. Obtener API key de OpenAI
2. Actualizar configuración:

```sql
UPDATE configuracion_empresa SET
    ocr_proveedor = 'openai',
    ocr_api_key = 'sk-...',
    ocr_modelo = 'gpt-4o',
    ocr_proveedor_secundario = 'tesseract',
    ocr_timeout = 30;
```

### 3. Configurar Fallback (Opcional pero Recomendado)

```sql
UPDATE configuracion_empresa SET
    ocr_proveedor_secundario = 'openai',
    ocr_fallback_automatico = TRUE;
```

Si el proveedor principal falla, intentará con el secundario automáticamente.

### 4. Verificar Configuración SMTP

El envío de emails usa la configuración SMTP existente:

```sql
SELECT smtp_servidor, smtp_puerto, smtp_usuario, smtp_email_desde 
FROM configuracion_empresa;
```

Si falta, configurar:

```sql
UPDATE configuracion_empresa SET
    smtp_servidor = 'smtp.gmail.com',
    smtp_puerto = 587,
    smtp_usuario = 'tu_email@gmail.com',
    smtp_password = 'tu_password_app',
    smtp_usar_ssl = TRUE,
    smtp_email_desde = 'tu_email@gmail.com',
    smtp_nombre_desde = 'Enerfone CRM';
```

---

## 🎯 Casos de Uso

### Caso 1: Cliente Llama con Factura en Mano

**Escenario**: Un cliente llama preguntando si puede ahorrar

**Flujo**:
1. Agente solicita que envíe la factura por email o WhatsApp
2. Agente guarda la factura en el ordenador
3. Accede a `/comparador/analizar-factura`
4. Sube la factura
5. Click en "Analizar con OCR"
6. El sistema extrae todos los datos automáticamente
7. Agente revisa que los datos son correctos
8. Click en "Calcular Comparativa"
9. Ve el ranking de tarifas
10. Informa al cliente del ahorro
11. Envía el PDF por email al cliente

**Tiempo estimado**: 2-3 minutos

### Caso 2: Crear Plantilla para Comercializadora Nueva

**Escenario**: Se reciben muchas facturas de una comercializadora nueva

**Flujo**:
1. Administrador accede a configuración de PreCarga
2. Crea nueva plantilla para "Comercializadora X"
3. Sube una factura de ejemplo
4. Mapea visualmente los campos importantes:
   - Dibuja rectángulo sobre CUPS
   - Dibuja rectángulo sobre total factura
   - Dibuja rectángulo sobre consumos
   - Etc.
5. Guarda la plantilla
6. Activa la plantilla

**Resultado**: Las próximas facturas de esa comercializadora se procesarán más rápido y con mayor precisión.

### Caso 3: Auditoría de Comparativas Realizadas

**Escenario**: Revisar cuántas comparativas se han hecho este mes

**Flujo**:
1. Acceder a `/comparador/historico`
2. Filtrar por rango de fechas (este mes)
3. Ver estadísticas:
   - Total de comparativas
   - Cuántas se enviaron por email
   - Cuántas usaron OCR
4. Exportar listado (futuro)

---

## 🔧 Arquitectura Técnica

### Diagrama de Flujo - Análisis con OCR

```
┌────────────────┐
│ Usuario sube   │
│ factura PDF    │
└───────┬────────┘
        │
        ▼
┌────────────────┐
│ Validar archivo│
│ (tipo, tamaño) │
└───────┬────────┘
        │
        ▼
┌────────────────┐        ┌──────────────────┐
│ Buscar plantilla│───────▶│ PlantillaPreCarga│
│ PreCarga       │        │ Service          │
└───────┬────────┘        └──────────────────┘
        │
        ▼
┌────────────────┐        ┌──────────────────┐
│ Procesar con   │───────▶│ Azure Document   │
│ OCR            │        │ Intelligence     │
└───────┬────────┘        └──────────────────┘
        │                          │
        │ Fallo?                   │
        ├──────────────────────────┘
        │
        ▼
┌────────────────┐        ┌──────────────────┐
│ Fallback a     │───────▶│ OpenAI Vision    │
│ secundario     │        │ GPT-4o           │
└───────┬────────┘        └──────────────────┘
        │
        ▼
┌────────────────┐
│ Mostrar datos  │
│ extraídos      │
│ (editables)    │
└───────┬────────┘
        │
        ▼
┌────────────────┐        ┌──────────────────┐
│ Calcular       │───────▶│ TarifaEnergia    │
│ comparativa    │        │ Service          │
└───────┬────────┘        └──────────────────┘
        │
        ▼
┌────────────────┐
│ Ordenar tarifas│
│ por coste      │
└───────┬────────┘
        │
        ▼
┌────────────────┐        ┌──────────────────┐
│ Guardar en     │───────▶│ HistoricoCompar- │
│ histórico      │        │ ativaService     │
└───────┬────────┘        └──────────────────┘
        │
        ▼
┌────────────────┐        ┌──────────────────┐
│ Generar PDF    │───────▶│ PdfComparador    │
│ y/o enviar     │        │ Service          │
│ por email      │        └──────────────────┘
└────────────────┘
```

### Integración con el Comparador Existente

El nuevo módulo se integra perfectamente con el comparador actual:

- **ComparadorEnergia.razor**: Comparador manual existente (se mantiene)
- **AnalizarFactura.razor**: Nuevo comparador con OCR
- **HistoricoComparativas.razor**: Vista de histórico

Ambos comparten:
- `TarifaEnergiaService` para obtener tarifas
- `PdfComparadorService` para generar PDFs
- `EmailService` para enviar correos
- `HistoricoComparativaService` para registrar actividad

---

## 📊 Modelo de Datos - Tablas Nuevas

### historico_comparativas

```sql
CREATE TABLE historico_comparativas (
    id INT AUTO_INCREMENT PRIMARY KEY,
    fecha_comparativa DATETIME NOT NULL,
    origen VARCHAR(50) NOT NULL,
    email_cliente VARCHAR(255),
    tipo_energia VARCHAR(50) NOT NULL,
    cups VARCHAR(255),
    comercializadora_actual VARCHAR(255),
    tarifa_actual VARCHAR(255),
    total_factura_actual DECIMAL(10,2),
    mejor_tarifa_id INT,
    mejor_tarifa_nombre VARCHAR(255),
    mejor_tarifa_empresa VARCHAR(255),
    ahorro_mensual DECIMAL(10,2),
    ahorro_anual DECIMAL(10,2),
    porcentaje_ahorro DECIMAL(5,2),
    datos_utilizados TEXT,
    resultado_ranking TEXT,
    proveedor_ocr VARCHAR(100),
    email_enviado BOOLEAN DEFAULT FALSE,
    fecha_envio_email DATETIME,
    advertencias TEXT,
    usuario_id INT,
    nombre_archivo_factura VARCHAR(500),
    INDEX idx_fecha_comparativa (fecha_comparativa),
    INDEX idx_origen (origen),
    FOREIGN KEY (usuario_id) REFERENCES usuarios(id)
);
```

### plantillas_precarga

```sql
CREATE TABLE plantillas_precarga (
    id INT AUTO_INCREMENT PRIMARY KEY,
    nombre VARCHAR(255) NOT NULL,
    comercializadora VARCHAR(255) NOT NULL,
    alias_comercializadora VARCHAR(500),
    tipo_energia VARCHAR(50) NOT NULL,
    variante_factura VARCHAR(255),
    prioridad INT DEFAULT 0,
    activa BOOLEAN DEFAULT TRUE,
    campos_mapeados TEXT,
    notas_internas TEXT,
    archivo_factura_ejemplo VARCHAR(500),
    fecha_creacion DATETIME DEFAULT CURRENT_TIMESTAMP,
    fecha_modificacion DATETIME,
    usuario_creador_id INT,
    INDEX idx_comercializadora (comercializadora),
    INDEX idx_activa (activa),
    FOREIGN KEY (usuario_creador_id) REFERENCES usuarios(id)
);
```

---

## 🎨 Mejoras Futuras / Roadmap

### Corto Plazo (v1.1)

- [ ] Implementar UI completa de PreCarga con visor PDF.js
- [ ] Mejorar prompts de OpenAI para mayor precisión
- [ ] Agregar soporte para facturas multipágina en mapeo
- [ ] Implementar caché de resultados OCR

### Medio Plazo (v1.2)

- [ ] Página pública (sin login) para clientes
- [ ] Exportar histórico a Excel/CSV
- [ ] Gráficas de estadísticas de comparativas
- [ ] Notificaciones por email cuando se crea comparativa
- [ ] Integración con WhatsApp para recibir facturas

### Largo Plazo (v2.0)

- [ ] Machine Learning para mejorar detección automática
- [ ] OCR local con Tesseract como opción gratuita
- [ ] App móvil para escanear facturas con la cámara
- [ ] Dashboard de analytics para gerencia
- [ ] API REST para integraciones externas

---

## 🐛 Troubleshooting

### OCR no funciona

**Problema**: Al procesar factura, da error de OCR

**Soluciones**:
1. Verificar que `ocr_proveedor`, `ocr_api_key` y `ocr_endpoint` están configurados
2. Verificar que la API key es válida
3. Verificar conexión a internet del servidor
4. Revisar logs para ver error específico
5. Probar con el proveedor secundario
6. Como último recurso, usar modo manual

### Datos extraídos incorrectos

**Problema**: El OCR extrae datos pero están mal

**Soluciones**:
1. Crear una plantilla PreCarga para esa comercializadora
2. Usar un proveedor diferente (Azure vs OpenAI)
3. Mejorar calidad de la imagen/PDF de entrada
4. Revisar manualmente y corregir antes de calcular

### No se guardan comparativas en histórico

**Problema**: Las comparativas no aparecen en el histórico

**Soluciones**:
1. Verificar que la tabla `historico_comparativas` existe
2. Verificar permisos de BD
3. Revisar logs de aplicación
4. Verificar que `HistoricoComparativaService` está registrado en `Program.cs`

### Email no se envía

**Problema**: PDF no llega por email al cliente

**Soluciones**:
1. Verificar configuración SMTP en `configuracion_empresa`
2. Probar conexión SMTP manualmente
3. Verificar que el email del cliente es válido
4. Revisar logs de `EmailService`
5. Verificar firewall/puertos del servidor

---

## 📚 Referencias

- [Azure Document Intelligence](https://learn.microsoft.com/azure/ai-services/document-intelligence/)
- [OpenAI Vision API](https://platform.openai.com/docs/guides/vision)
- [PDF.js Documentation](https://mozilla.github.io/pdf.js/)
- [ASP.NET Core Blazor](https://learn.microsoft.com/aspnet/core/blazor/)

---

## 👥 Soporte

Para soporte técnico o reportar bugs:
- Crear issue en el repositorio
- Contactar al equipo de desarrollo
- Documentar pasos para reproducir el problema
- Incluir logs relevantes

---

**Versión**: 1.0.0  
**Fecha**: 10 de junio de 2026  
**Autor**: CorCRM Development Team
