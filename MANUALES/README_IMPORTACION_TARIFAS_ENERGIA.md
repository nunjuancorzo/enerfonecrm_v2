# Importación Masiva de Tarifas de Energía

## Descripción

Esta funcionalidad permite importar tarifas de energía de forma masiva desde archivos Excel a través del módulo "Inicio Rápido".

## Campos Nuevos Agregados al Modelo

Se han agregado los siguientes campos a la tabla `tarifasenergia`:

- `tipo_cliente`: Tipo de cliente (RESIDENCIAL, PYMES, etc.)
- `peaje`: Tipo de peaje (2.0, 3.0, 6.1, etc.)
- `termino_fijo_gas`: Término fijo de gas
- `pvd_sva`: Precio Vapor Diario SVA
- `termino_variable_gas`: Término variable de gas
- `descuento`: Descuento aplicable
- `observaciones_descuentos`: Observaciones sobre descuentos
- `permanencia`: Permanencia del contrato
- `excedentes`: Precio de excedentes
- `bateria_virtual`: Si tiene batería virtual (SI/NO)
- `fecha_carga`: Fecha de carga de la tarifa

## Pasos para Activar la Funcionalidad

### 1. Ejecutar Script SQL

Antes de usar la importación, debes ejecutar el script SQL para agregar las nuevas columnas a la base de datos:

```bash
# Conectarse a MySQL
mysql -u root -p

# Seleccionar la base de datos
USE enerfone_pre;

# Ejecutar el script
source /Users/juanmariacorzo/Documents/CorCRM/EnerfoneCRMv2/EnerfoneCRM/Scripts/agregar_campos_excel_tarifas_energia.sql
```

O directamente desde la línea de comandos:

```bash
mysql -u root -p enerfone_pre < /Users/juanmariacorzo/Documents/CorCRM/EnerfoneCRMv2/EnerfoneCRM/Scripts/agregar_campos_excel_tarifas_energia.sql
```

### 2. Configurar Python (si no está configurado)

Asegúrate de tener Python 3 instalado con las bibliotecas necesarias:

```bash
pip3 install pandas openpyxl mysql-connector-python xlsxwriter
```

### 3. Configurar Conexión a Base de Datos

Edita el archivo `importar_tarifas_energia.py` y actualiza la configuración de la base de datos:

```python
DB_CONFIG = {
    'host': 'localhost',
    'database': 'enerfone_pre',  # Cambiar al nombre de tu base de datos
    'user': 'root',              # Cambiar a tu usuario
    'password': ''               # Añadir tu contraseña
}
```

## Uso de la Funcionalidad

### Desde la Interfaz Web

1. Accede a **Inicio Rápido** (solo para administradores)
2. Busca la tarjeta **"Importar Tarifas de Energía"** (color amarillo/warning)
3. Descarga la plantilla haciendo clic en **"Descargar Plantilla"**
4. Rellena la plantilla con tus datos (o usa el archivo `tarifas_energia.xlsx` existente)
5. Selecciona el archivo Excel
6. Haz clic en **"Importar Tarifas"**
7. Espera a que se procese (puede tardar varios minutos)
8. Revisa el resultado: número de importados y errores

### Formato del Excel

El Excel debe tener las siguientes columnas (primera fila como encabezado):

#### Columnas Obligatorias:
- **COMERCIALIZADORA**: Nombre de la comercializadora
- **ENERGIA**: Tipo de energía (LUZ, GAS, LUZ+GAS)
- **TARIFA**: Nombre de la tarifa
- Al menos **POTENCIA 1** o **ENERGIA 1** debe tener un valor

#### Columnas Opcionales:
- **TIPO**: Tipo de cliente (RESIDENCIAL, PYMES)
- **PEAJE**: Tipo de peaje
- **POTENCIA 1-6**: Precios de potencia por periodo
- **ENERGIA 1-6**: Precios de energía por periodo
- **T. FIJO GAS**: Término fijo de gas
- **PVD SVA**: Precio Vapor Diario SVA
- **T. VARIABLE GAS**: Término variable de gas
- **DESCUENTO**: Descuento aplicable
- **OBJERVACIONES DESCUENTOS**: Observaciones sobre descuentos
- **COMISION**: Comisión de la tarifa
- **PERMANENCIA**: Permanencia del contrato
- **EXCEDENTES**: Precio de excedentes
- **BATERIA VIRTUAL**: Si tiene batería virtual (SI/NO)
- **FECHA CARGA**: Fecha de carga de la tarifa

### Importación Manual desde Terminal

También puedes importar directamente desde la terminal:

```bash
cd /Users/juanmariacorzo/Documents/CorCRM/EnerfoneCRMv2
python3 importar_tarifas_energia.py tarifas_energia.xlsx
```

## Generar Plantilla desde Terminal

Para generar una nueva plantilla desde la terminal:

```bash
cd /Users/juanmariacorzo/Documents/CorCRM/EnerfoneCRMv2
python3 generar_plantilla_tarifas_energia.py
```

Esto creará el archivo `plantilla_tarifas_energia_importacion.xlsx` con:
- Hoja "Tarifas Energía" con encabezados y ejemplos
- Hoja "Instrucciones" con documentación completa

## Notas Importantes

1. **Primera Fila**: Debe contener los encabezados exactos
2. **Valores Decimales**: Usar punto (.) como separador (ej: 0.123)
3. **Fechas**: Formato YYYY-MM-DD
4. **Filas Vacías**: Se omiten automáticamente
5. **Errores**: Se registran y se muestran en el resultado
6. **Tamaño Máximo**: 10 MB por archivo
7. **Duplicados**: Si ya existe una tarifa con el mismo nombre, se creará una nueva entrada

## Solución de Problemas

### Error: "No se encontró el script"
- Verifica que los archivos `.py` estén en el directorio raíz del proyecto
- Revisa que Python esté instalado y configurado correctamente

### Error: "Error de base de datos"
- Verifica la conexión a MySQL
- Comprueba que las credenciales sean correctas
- Asegúrate de haber ejecutado el script SQL de migración

### Error: "No se pudo parsear el resultado"
- Revisa el formato del Excel
- Verifica que la primera fila contenga los encabezados
- Comprueba que no haya caracteres especiales en los nombres de columnas

## Archivos Modificados/Creados

1. **Modelo**: `/EnerfoneCRM/Models/TarifaEnergia.cs`
2. **Script SQL**: `/EnerfoneCRM/Scripts/agregar_campos_excel_tarifas_energia.sql`
3. **Importación**: `/importar_tarifas_energia.py`
4. **Plantilla**: `/generar_plantilla_tarifas_energia.py`
5. **UI**: `/EnerfoneCRM/Components/Pages/InicioRapido.razor`
6. **JavaScript**: `/EnerfoneCRM/wwwroot/js/fileDownload.js`

## Ejemplo de Uso

1. Usuario descarga plantilla desde Inicio Rápido
2. Completa las filas con datos de tarifas
3. Sube el archivo completado
4. Sistema procesa el archivo con Python
5. Se insertan las tarifas en la base de datos
6. Se muestra resumen: "Importados: 150, Errores: 3"
7. Usuario puede revisar detalles de errores si los hay

## Fecha de Implementación

24 de febrero de 2026
