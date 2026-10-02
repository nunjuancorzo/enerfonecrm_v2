# Importación Masiva de Clientes

Este directorio contiene las herramientas para realizar una importación masiva de clientes a la base de datos, ideal para carga inicial de datos.

## 📋 Contenido

- `plantilla_clientes.xlsx` - Plantilla Excel para rellenar con los datos de clientes
- `importar_clientes.py` - Script Python que importa los datos a MySQL
- Este documento con instrucciones

## 🚀 Instrucciones de Uso

### Paso 1: Preparar la Plantilla Excel

1. Abre el archivo `plantilla_clientes.xlsx`
2. Verás dos hojas:
   - **Clientes**: Donde debes rellenar los datos
   - **Instrucciones**: Información detallada sobre cada campo

3. Rellena la hoja "Clientes" con tus datos:
   - **NO elimines la fila de encabezados** (fila 1)
   - Puedes eliminar la fila de ejemplo (fila 2)
   - Añade tantas filas como clientes necesites

### Paso 2: Campos de la Plantilla

#### Campos Obligatorios (marcados con *)
- **TipoCliente***: Debe ser exactamente `Particular` o `Empresa`
- **Nombre***: Nombre o razón social del cliente

#### Campos Opcionales
- **DNI/CIF**: Documento de identificación
- **DNI Representante**: DNI del representante legal
- **Email**: Correo electrónico (se valida el formato)
- **Teléfono**: Número de teléfono
- **Dirección**: Calle o avenida
- **Número**: Número del inmueble
- **Escalera**: Escalera (si aplica)
- **Piso**: Número de piso
- **Puerta**: Puerta
- **Aclarador**: Información adicional de ubicación
- **Población**: Ciudad o municipio
- **Provincia**: Provincia
- **Código Postal**: Código postal
- **IBAN**: Cuenta bancaria (máximo 34 caracteres)
- **Representante**: Nombre del representante
- **Comercial**: Nombre del comercial asignado
- **Observaciones**: Notas adicionales

### Paso 3: Instalar Dependencias

Necesitas Python 3 y las siguientes librerías:

```bash
# Instalar pandas para leer Excel
pip3 install pandas openpyxl

# Instalar conector de MySQL
pip3 install mysql-connector-python
```

### Paso 4: Configurar la Conexión a la Base de Datos

Edita el archivo `importar_clientes.py` y modifica la configuración de la base de datos:

```python
DB_CONFIG = {
    'host': 'localhost',        # Cambia si tu MySQL está en otro servidor
    'database': 'enerfone_pre', # Nombre de tu base de datos
    'user': 'root',             # Tu usuario de MySQL
    'password': 'tu_password'   # Tu contraseña de MySQL
}
```

### Paso 5: Ejecutar la Importación

```bash
# Formato básico
python3 importar_clientes.py plantilla_clientes.xlsx

# Especificar un ID de usuario diferente (por defecto es 1)
python3 importar_clientes.py plantilla_clientes.xlsx 5
```

### Paso 6: Verificar Resultados

El script mostrará:
- ✓ Cada cliente importado correctamente
- ❌ Errores si los hay (con número de fila y descripción)
- Resumen final con total de importados y errores

Ejemplo de salida:
```
✓ Fila 2: Ejemplo Cliente S.L. - Importado correctamente
✓ Fila 3: Juan Pérez - Importado correctamente
❌ Fila 4: Error - Email no válido

============================================================
✅ IMPORTACIÓN COMPLETADA
============================================================
Clientes importados: 2
Filas con errores: 1
```

## ⚠️ Notas Importantes

1. **Backup**: Haz una copia de seguridad de tu base de datos antes de importar
2. **Validaciones**: El script valida los datos antes de insertarlos
3. **Duplicados**: El script NO verifica duplicados, insertará todos los registros válidos
4. **Fecha de Alta**: Se asigna automáticamente la fecha actual
5. **ID Usuario**: Por defecto se asigna el usuario con ID 1, puedes cambiarlo como segundo argumento

## 🔍 Validaciones que Realiza el Script

- TipoCliente debe ser 'Particular' o 'Empresa'
- Nombre es obligatorio
- Email debe tener formato válido
- IBAN no puede exceder 34 caracteres
- Campos de texto respetan los límites de la base de datos

## 🐛 Solución de Problemas

### Error: "No se encontró el archivo"
- Verifica que el nombre del archivo sea correcto
- Asegúrate de estar en el directorio correcto

### Error: "Access denied for user"
- Verifica usuario y contraseña en DB_CONFIG
- Asegúrate de que el usuario tenga permisos en la base de datos

### Error: "Unknown database"
- Verifica que el nombre de la base de datos sea correcto
- La base de datos debe existir previamente

### "El archivo no contiene datos"
- Verifica que la hoja se llame exactamente "Clientes"
- Asegúrate de tener al menos una fila con datos (además de los encabezados)

## 📊 Ejemplo de Datos

| TipoCliente | Nombre | DNI/CIF | Email | Teléfono | Dirección | Número |
|-------------|---------|---------|-------|----------|-----------|--------|
| Empresa | Ejemplo S.L. | B12345678 | info@ejemplo.com | 912345678 | Calle Mayor | 10 |
| Particular | Juan Pérez | 12345678A | juan@email.com | 654321987 | Av. Principal | 25 |

## 🔄 Alternativa: Importación Manual SQL

Si prefieres no usar Python, puedes generar un archivo SQL desde Excel. Contacta para más información.
