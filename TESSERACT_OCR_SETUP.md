# Configuración de Tesseract OCR - Opción GRATUITA

## 🎉 ¿Por qué Tesseract?

**Tesseract OCR** es la alternativa **100% GRATUITA** para análisis de facturas:

- ✅ **Totalmente gratuito** y open-source
- ✅ **Sin límites de uso** (procesa miles de facturas sin coste)
- ✅ **Funciona offline** (no requiere internet)
- ✅ **Sin API keys ni registros**
- ✅ **Sin costos mensuales**
- ✅ **Desarrollado por Google** (mantenido activamente)
- ✅ **Soporta más de 100 idiomas** incluido español

### Comparativa de Costos

| Proveedor | Costo | Límite Gratis | Requiere Tarjeta |
|-----------|-------|---------------|------------------|
| **Tesseract** | **$0** | **Ilimitado** | **NO** |
| Azure Document Intelligence | ~$1.50 / 1000 págs | 500/mes | Sí |
| OpenAI Vision | ~$0.01-0.03 / imagen | 0 | Sí |
| Google Vision | ~$1.50 / 1000 img | 1000/mes | Sí |

---

## 📦 Instalación

### Método Automático (Recomendado)

Ejecuta el script de instalación incluido:

```bash
cd /Users/juanmariacorzo/Documents/CorCRM/EnerfoneCRMv2
./install-tesseract.sh
```

El script detecta automáticamente tu sistema operativo e instala todo lo necesario.

### Instalación Manual

#### En macOS (con Homebrew)

```bash
# Instalar Homebrew si no lo tienes
/bin/bash -c "$(curl -fsSL https://raw.githubusercontent.com/Homebrew/install/HEAD/install.sh)"

# Instalar Tesseract y dependencias
brew install tesseract tesseract-lang poppler imagemagick

# Verificar instalación
tesseract --version
tesseract --list-langs | grep spa
```

#### En Ubuntu/Debian

```bash
# Actualizar repositorios
sudo apt update

# Instalar Tesseract y dependencias
sudo apt install -y tesseract-ocr tesseract-ocr-spa poppler-utils imagemagick

# Verificar instalación
tesseract --version
tesseract --list-langs | grep spa
```

#### En CentOS/RHEL

```bash
# Habilitar EPEL
sudo yum install -y epel-release

# Instalar Tesseract y dependencias
sudo yum install -y tesseract tesseract-langpack-spa poppler-utils ImageMagick

# Verificar instalación
tesseract --version
tesseract --list-langs | grep spa
```

#### En Windows

1. Descargar instalador desde: https://github.com/UB-Mannheim/tesseract/wiki
2. Instalar Tesseract (asegurarse de incluir el idioma español)
3. Agregar al PATH: `C:\Program Files\Tesseract-OCR`
4. Instalar Poppler desde: https://github.com/oschwartz10612/poppler-windows/releases
5. Agregar Poppler al PATH

---

## ⚙️ Configuración en CorCRM

### Opción 1: Tesseract como Proveedor Principal (Recomendado para comenzar)

```sql
UPDATE configuracion_empresa SET
    ocr_proveedor = 'tesseract',
    ocr_proveedor_secundario = NULL,
    ocr_fallback_automatico = TRUE
WHERE id = 1;
```

**Ventajas:**
- 100% gratuito
- Sin límites
- Ideal para pruebas y producción con bajo volumen

**Desventajas:**
- Menos preciso que Azure/OpenAI en facturas complejas
- Requiere preprocesamiento para mejores resultados

### Opción 2: Azure + Tesseract (Híbrido Inteligente)

```sql
UPDATE configuracion_empresa SET
    ocr_proveedor = 'azure',
    ocr_api_key = 'TU_API_KEY_AZURE',
    ocr_endpoint = 'https://RECURSO.cognitiveservices.azure.com/',
    ocr_proveedor_secundario = 'tesseract',
    ocr_fallback_automatico = TRUE
WHERE id = 1;
```

**Estrategia:**
- Usa Azure para las primeras 500 facturas/mes (capa gratuita)
- Después automáticamente usa Tesseract (gratis)
- Lo mejor de ambos mundos

### Opción 3: Tesseract + OpenAI (Para casos difíciles)

```sql
UPDATE configuracion_empresa SET
    ocr_proveedor = 'tesseract',
    ocr_proveedor_secundario = 'openai',
    ocr_api_key = 'sk-...',
    ocr_modelo = 'gpt-4o',
    ocr_fallback_automatico = TRUE
WHERE id = 1;
```

**Estrategia:**
- Usa Tesseract (gratis) para todas las facturas
- Solo si Tesseract falla, usa OpenAI (de pago)
- Minimiza costos, máxima precisión cuando es necesario

---

## 🎯 Uso

### Desde la Interfaz Web

1. Navega a `/comparador/analizar-factura`
2. Sube tu factura (PDF, JPG, PNG)
3. Click en "Analizar con OCR"
4. Tesseract procesa la factura automáticamente
5. Revisa y corrige los datos si es necesario
6. Calcula la comparativa

### Prueba desde Terminal

```bash
# Procesar una imagen
tesseract factura.jpg salida -l spa

# Ver el resultado
cat salida.txt

# Procesar PDF (requiere pdftoppm)
pdftoppm -png factura.pdf pagina
tesseract pagina-1.png salida -l spa
```

---

## 🔧 Mejores Prácticas para Tesseract

### 1. Calidad de Imagen

Tesseract funciona mejor con:
- **Imágenes de alta resolución** (300 DPI mínimo)
- **Alto contraste** (texto negro sobre fondo blanco)
- **Sin ruido** (sin manchas o artifacts)
- **Texto horizontal** (sin rotación)

### 2. Preprocesamiento (Opcional)

Para mejorar resultados, preprocesa con ImageMagick:

```bash
# Convertir a escala de grises
convert factura.jpg -colorspace Gray factura_gray.jpg

# Aumentar contraste
convert factura.jpg -contrast-stretch 0 factura_contrast.jpg

# Eliminar ruido
convert factura.jpg -despeckle factura_clean.jpg

# Aumentar resolución
convert factura.jpg -density 300 factura_hires.jpg
```

### 3. Configuración de Tesseract

Parámetros PSM (Page Segmentation Mode):

| PSM | Descripción | Uso Recomendado |
|-----|-------------|-----------------|
| 3 | Automático (default) | General |
| 6 | Bloque de texto uniforme | **Facturas** (recomendado) |
| 11 | Texto disperso | Facturas complejas |
| 13 | Línea única | Extraer CUPS, totales |

El código usa `--psm 6` por defecto (óptimo para facturas).

---

## 📊 Precisión Esperada

### Facturas Digitales (PDF generado por ordenador)

- **Precisión**: 95-98%
- **Velocidad**: 2-5 segundos
- **Campos que detecta bien**:
  - CUPS
  - Totales
  - Comercializadora
  - Peajes
  - Potencias

### Facturas Escaneadas (Calidad buena)

- **Precisión**: 80-90%
- **Velocidad**: 5-10 segundos
- **Requiere**: Revisión manual de algunos campos

### Facturas Escaneadas (Calidad baja)

- **Precisión**: 60-75%
- **Velocidad**: 10-15 segundos
- **Requiere**: Preprocesamiento + revisión manual

### Fotos de Móvil

- **Precisión**: Variable (50-80%)
- **Requiere**: Buena iluminación, sin sombras, enfoque correcto
- **Recomendación**: Usar app de escaneo (como Adobe Scan, Microsoft Lens)

---

## 🐛 Troubleshooting

### "Tesseract OCR no está instalado"

**Problema**: El sistema no encuentra Tesseract

**Solución**:
```bash
# Verificar instalación
which tesseract

# Si no está, instalar:
# macOS:
brew install tesseract

# Ubuntu/Debian:
sudo apt install tesseract-ocr

# Verificar PATH
echo $PATH
```

### "No se pudo convertir el PDF a imágenes"

**Problema**: Falta poppler-utils (pdftoppm)

**Solución**:
```bash
# macOS:
brew install poppler

# Ubuntu/Debian:
sudo apt install poppler-utils

# Verificar:
which pdftoppm
```

### Resultados con baja precisión

**Problema**: Tesseract no extrae bien los datos

**Soluciones**:
1. **Mejorar calidad de entrada**:
   - Usar PDFs originales (no escaneados)
   - Aumentar resolución de escaneo a 300 DPI
   - Asegurar buen contraste

2. **Configurar proveedor secundario**:
   ```sql
   UPDATE configuracion_empresa SET
       ocr_proveedor_secundario = 'azure',
       ocr_fallback_automatico = TRUE;
   ```

3. **Usar modo manual**:
   - En la página de análisis, click en "Introducir datos manualmente"
   - Es más rápido que corregir OCR malo

### Tesseract muy lento

**Problema**: Tarda mucho en procesar

**Optimizaciones**:
```bash
# Reducir resolución de PDFs
pdftoppm -png -r 150 factura.pdf pagina  # En lugar de 300 DPI

# Procesar solo página 1
pdftoppm -png -f 1 -l 1 factura.pdf pagina

# Usar modo más rápido
tesseract imagen.png salida -l spa --psm 6 --oem 1
```

---

## 🚀 Rendimiento

### Recursos del Servidor

- **CPU**: 1-5% durante procesamiento
- **RAM**: ~50-100 MB por factura
- **Disco**: ~2 MB temporal por factura (se limpia automáticamente)
- **Tiempo**: 2-10 segundos por factura

### Escalabilidad

Tesseract puede procesar:
- **Servidor modesto**: 50-100 facturas/hora
- **Servidor dedicado**: 500-1000 facturas/hora
- **Sin costos adicionales** sin importar el volumen

---

## 💡 Estrategia Recomendada para Producción

### Fase 1: Inicio (0-1000 facturas/mes)

```sql
UPDATE configuracion_empresa SET
    ocr_proveedor = 'tesseract',
    ocr_proveedor_secundario = NULL;
```

**Costo**: $0/mes  
**Razón**: Tesseract es suficiente para volumen bajo

### Fase 2: Crecimiento (1000-5000 facturas/mes)

```sql
UPDATE configuracion_empresa SET
    ocr_proveedor = 'azure',
    ocr_proveedor_secundario = 'tesseract',
    ocr_fallback_automatico = TRUE;
```

**Costo**: $0-$7/mes (usar capa gratuita de Azure + Tesseract)  
**Razón**: Azure mejora precisión, Tesseract evita costos

### Fase 3: Escala (5000+ facturas/mes)

```sql
UPDATE configuracion_empresa SET
    ocr_proveedor = 'tesseract',
    ocr_proveedor_secundario = 'azure',
    ocr_fallback_automatico = TRUE;
```

**Costo**: $7-$15/mes  
**Razón**: Tesseract para la mayoría, Azure para casos complejos

---

## 📈 Comparación Real: Costos Anuales

**Escenario**: 3,000 facturas procesadas al mes

| Configuración | Costo/Mes | Costo/Año | Precisión |
|---------------|-----------|-----------|-----------|
| **Solo Tesseract** | **$0** | **$0** | 85% |
| Azure + Tesseract | $4.50 | $54 | 95% |
| Solo Azure | $9 | $108 | 96% |
| Solo OpenAI | $30-90 | $360-1080 | 97% |

**Recomendación**: Empieza con Tesseract (gratis), agrega Azure si necesitas más precisión.

---

## 🎓 Recursos Adicionales

- [Documentación oficial Tesseract](https://tesseract-ocr.github.io/)
- [Mejores prácticas OCR](https://tesseract-ocr.github.io/tessdoc/ImproveQuality.html)
- [Foro de Tesseract](https://groups.google.com/g/tesseract-ocr)
- [GitHub Tesseract](https://github.com/tesseract-ocr/tesseract)

---

## ✅ Checklist de Configuración

- [ ] Tesseract instalado (`tesseract --version`)
- [ ] Idioma español instalado (`tesseract --list-langs | grep spa`)
- [ ] Poppler instalado para PDFs (`which pdftoppm`)
- [ ] Script SQL ejecutado para configuración
- [ ] Proveedor configurado en BD
- [ ] Prueba básica realizada
- [ ] Página web funcionando (`/comparador/analizar-factura`)

---

**¡Listo!** Ahora tienes OCR gratuito e ilimitado para tu comparador de tarifas. 🎉
