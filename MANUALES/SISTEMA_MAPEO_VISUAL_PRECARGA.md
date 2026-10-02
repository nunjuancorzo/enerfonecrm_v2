# Sistema de Mapeo Visual de Facturas (PreCarga)

## 📋 Descripción General

El sistema PreCarga permite enseñar al OCR dónde están ubicados los datos en las facturas de cada comercializadora mediante mapeo visual interactivo.

## 🎯 Beneficios

- **Mayor precisión**: El usuario indica exactamente dónde está cada dato
- **Menos errores**: Evita confusiones del OCR automático
- **Reutilizable**: Una vez creada la plantilla, se aplica a todas las facturas similares
- **Multipágina**: Soporta facturas con datos en varias páginas
- **Flexible**: Cada comercializadora puede tener múltiples variantes

## 🚀 Cómo Usar

### 1. Crear una Plantilla

1. Accede a **Comparador > Mapeo Manual**
2. Completa la información básica:
   - Nombre de la plantilla (ej: "Iberdrola Luz 2.0TD")
   - Comercializadora
   - Tipo de energía (Luz, Gas o Luz+Gas)
   - Variante (opcional: 2.0TD, 3.0TD, etc.)
3. Sube una factura de ejemplo (PDF, JPG o PNG)
4. Click en **Iniciar Mapeo**

### 2. Mapear Campos Visualmente

1. En la columna izquierda, selecciona un campo (ej: "CUPS")
2. En la imagen de la factura, **haz clic y arrastra** para dibujar un rectángulo sobre donde aparece ese dato
3. El sistema guarda automáticamente las coordenadas
4. Repite para cada campo que necesites extraer
5. Usa los botones de navegación para mapear campos en diferentes páginas

### 3. Guardar la Plantilla

1. Revisa la lista de campos mapeados
2. Click en **Guardar Plantilla**
3. La plantilla estará disponible para usar automáticamente

### 4. Usar la Plantilla

Cuando subes una nueva factura de esa comercializadora:
- El sistema busca automáticamente la plantilla compatible
- Extrae el texto SOLO de las zonas marcadas usando Tesseract
- Los datos se completan automáticamente en el formulario

## 📊 Campos Disponibles para Mapear

### Datos Generales
- CUPS
- Comercializadora Actual
- Tarifa Actual
- Peaje Luz / Gas
- Inicio y Fin del Periodo
- Días del Periodo
- Total Factura €
- IVA %
- Impuestos
- Alquiler Contador/Equipos

### Luz - Potencia
- Potencia Contratada P1 a P6 (kW)
- Precio Potencia P1 a P3 (€/kW)

### Luz - Energía
- Consumo Energía P1 a P6 (kWh)
- Precio Energía P1 a P3 (€/kWh)

### Gas
- Consumo Gas (kWh)
- Término Fijo Gas (€)
- Término Variable Gas (€/kWh)

### Otros
- Servicios Adicionales
- Descuentos

## 🎨 Coordenadas Relativas

Las coordenadas se guardan como **porcentajes** (0-1) en lugar de píxeles absolutos:

```json
{
  "field": "cups",
  "label": "CUPS",
  "page": 1,
  "x": 0.128,      // 12.8% desde la izquierda
  "y": 0.312,      // 31.2% desde arriba
  "w": 0.352,      // 35.2% de ancho
  "h": 0.041       // 4.1% de alto
}
```

Esto hace que las plantillas funcionen independientemente del tamaño de visualización.

## 🔍 Gestionar Plantillas

En **Comparador > Plantillas PreCarga** puedes:

- Ver todas las plantillas creadas
- Activar/Desactivar plantillas
- Ver el detalle de campos mapeados
- Eliminar plantillas
- Ver fecha de creación y prioridad

## ⚙️ Prioridad de Plantillas

Si existen varias plantillas para la misma comercializadora:
1. Se usa la que tenga **mayor prioridad**
2. Si tienen igual prioridad, la **más reciente**
3. Solo se usan plantillas **activas**

## 📌 Consejos Importantes

### ✅ Buenas Prácticas

- Usa facturas **anonimizadas** (sin datos personales reales)
- Dibuja rectángulos **ajustados** al texto
- Mapea solo los campos que realmente necesites
- Crea variantes para diferentes modelos de factura de la misma comercializadora
- Usa nombres descriptivos para las plantillas

### ⚠️ Evita

- Subir facturas con datos de clientes reales
- Dibujar rectángulos muy grandes que incluyan texto innecesario
- Mapear campos que no aparecen en todas las facturas
- Crear plantillas duplicadas sin necesidad

## 🔄 Flujo de Extracción con Plantilla

1. Usuario sube factura
2. Sistema detecta comercializadora (manual o automático)
3. Busca plantilla compatible (comercializadora + tipo energía)
4. Convierte PDF a imagen (si es necesario)
5. Para cada campo mapeado:
   - Calcula coordenadas absolutas desde las relativas
   - Recorta esa zona de la imagen
   - Ejecuta Tesseract OCR solo en esa zona
   - Limpia y valida el texto extraído
6. Completa el formulario con los datos extraídos

## 🛠️ Tecnologías Utilizadas

- **PDF.js**: Renderizado de PDFs en canvas HTML5
- **Canvas API**: Dibujo interactivo de rectángulos
- **Tesseract OCR**: Extracción de texto de zonas marcadas
- **JSON**: Almacenamiento de coordenadas en base de datos
- **Blazor Server**: Interfaz interactiva en tiempo real

## 📁 Estructura de Archivos

```
EnerfoneCRM/
├── Components/Pages/
│   ├── MapeoManualFactura.razor      # Página de mapeo visual
│   └── GestionPlantillasPreCarga.razor  # Gestión de plantillas
├── Models/
│   ├── PlantillaPreCarga.cs          # Modelo de plantilla
│   └── CampoMapeadoPreCarga.cs       # Modelo de campo mapeado
├── Services/
│   └── PlantillaPreCargaService.cs   # Lógica de negocio
├── wwwroot/
│   ├── js/mapeo-factura.js           # JavaScript para canvas
│   └── css/mapeo-factura.css         # Estilos del mapeo
└── ADD_MODULO_COMPARATIVAS_OCR.sql   # Script de base de datos
```

## 🗄️ Tabla Base de Datos

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
    campos_mapeados TEXT,  -- JSON
    notas_internas TEXT,
    archivo_factura_ejemplo VARCHAR(500),
    fecha_creacion DATETIME,
    fecha_modificacion DATETIME,
    usuario_creador_id INT
);
```

## 🎯 Casos de Uso

### Caso 1: Primera Factura de Iberdrola
1. Crear plantilla "Iberdrola Luz 2.0TD"
2. Mapear: CUPS, Total, P1, P2, Consumo P1, Consumo P2
3. Guardar plantilla
4. Próximas facturas de Iberdrola se procesan automáticamente

### Caso 2: Variantes de Endesa
- Plantilla 1: "Endesa Luz Doméstica" (prioridad: 10)
- Plantilla 2: "Endesa Luz Empresas" (prioridad: 5)
- El sistema elige la correcta según prioridad y coincidencia

### Caso 3: Factura Dual
1. Crear plantilla "Naturgy Luz+Gas"
2. Mapear campos de luz en página 1
3. Mapear campos de gas en página 2
4. Una plantilla cubre ambos suministros

## 🔐 Seguridad y Privacidad

- Las facturas de ejemplo se almacenan en `/uploads/mapeo/`
- **IMPORTANTE**: Usar facturas anonimizadas
- Las plantillas se pueden desactivar sin eliminar
- Solo usuarios autorizados pueden crear plantillas
- Los archivos temporales se limpian automáticamente

## 📖 Próximos Pasos

Después de crear plantillas:
1. Pruébalas subiendo facturas reales
2. Ajusta las coordenadas si es necesario
3. Crea variantes para diferentes formatos
4. Documenta qué campos son obligatorios vs opcionales

---

**Versión**: 1.0  
**Fecha**: Junio 2026  
**Sistema**: EnerfoneCRM - Comparador Energético con OCR
