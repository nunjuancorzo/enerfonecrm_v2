# Importación Masiva de Contratos

Este directorio contiene las herramientas para realizar una importación masiva de contratos a la base de datos, ideal para carga inicial de datos. Soporta tres tipos de contratos: **Energía**, **Telefonía** y **Alarmas**.

## 📋 Contenido

- `plantilla_contratos_energia.xlsx` - Plantilla para contratos de energía
- `plantilla_contratos_telefonia.xlsx` - Plantilla para contratos de telefonía
- `plantilla_contratos_alarmas.xlsx` - Plantilla para contratos de alarmas
- `importar_contratos.py` - Script Python que importa los datos a MySQL
- `generar_plantillas_contratos.py` - Script para regenerar las plantillas
- Este documento con instrucciones

## 🚀 Instrucciones de Uso

### Paso 1: Instalar Dependencias

```bash
pip3 install --user pandas openpyxl mysql-connector-python
```

### Paso 2: Generar las Plantillas Excel

```bash
python3 generar_plantillas_contratos.py
```

Esto generará tres archivos Excel:
- `plantilla_contratos_energia.xlsx`
- `plantilla_contratos_telefonia.xlsx`
- `plantilla_contratos_alarmas.xlsx`

### Paso 3: Rellenar las Plantillas

Cada plantilla tiene:
- **Hoja principal**: Donde debes rellenar los datos de contratos
- **Hoja de Instrucciones**: Información detallada sobre cada campo
- **Hoja adicional** (solo Telefonía): Para líneas móviles adicionales

⚠️ **IMPORTANTE**: El campo `IdCliente*` debe corresponder a un cliente existente en la tabla `clientes_simple`.

### Paso 4: Configurar la Conexión a la Base de Datos

Edita el archivo `importar_contratos.py` y modifica la configuración:

```python
DB_CONFIG = {
    'host': 'localhost',
    'database': 'enerfone_pre',
    'user': 'root',
    'password': 'tu_password'
}
```

### Paso 5: Ejecutar la Importación

```bash
# Para contratos de energía
python3 importar_contratos.py energia plantilla_contratos_energia.xlsx

# Para contratos de telefonía
python3 importar_contratos.py telefonia plantilla_contratos_telefonia.xlsx

# Para contratos de alarmas
python3 importar_contratos.py alarmas plantilla_contratos_alarmas.xlsx
```

## 📊 Detalles de Cada Tipo de Contrato

### 🔋 Contratos de Energía

#### Campos Obligatorios
- **IdCliente***: ID del cliente (debe existir en la base de datos)

#### Campos Principales
- **Estado**: Pendiente, Activo, Cancelado, etc.
- **Comercial**: Nombre del comercial
- **FechaAlta**: Fecha de alta (formato YYYY-MM-DD)
- **EstadoServicio**: Estado del servicio
- **Comercializadora**: Iberdrola, Endesa, Naturgy, etc.
- **Tarifa**: Tarifa contratada (ej: 2.0TD, 3.0TD)
- **CUPS**: Código CUPS de electricidad
- **CUPSGas**: Código CUPS de gas (opcional)
- **Servicios**: Luz, Gas, Luz + Gas
- **IBAN**: Cuenta bancaria
- **TipoOperacion**: Alta, Cambio de titular, Cambio de comercializadora
- **PotenciaContratada**: Potencia en kW (decimal)
- **ConsumoAnual**: Consumo anual en kWh (decimal)
- **Comision**: Comisión del contrato (decimal)
- **Observaciones**: Notas adicionales

#### Ejemplo de Datos
```
IdCliente: 1
Estado: Pendiente
Comercial: Juan García
FechaAlta: 2026-01-29
Comercializadora: Iberdrola
Tarifa: 2.0TD
CUPS: ES0031234567890123456789012
PotenciaContratada: 4.6
ConsumoAnual: 3500
```

---

### 📱 Contratos de Telefonía

#### Campos Obligatorios
- **IdCliente***: ID del cliente (debe existir en la base de datos)

#### Campos Principales
- **Estado**: Estado del contrato
- **Comercial**: Nombre del comercial
- **FechaAlta**: Fecha de alta (formato YYYY-MM-DD)
- **Operadora**: Movistar, Vodafone, Orange, MásMóvil, etc.
- **Tarifa**: Tarifa contratada
- **TipoTarifa**: Fibra 600Mb, Móvil ilimitado, etc.
- **FijoTel**: Número de teléfono fijo
- **LineaMovilPrincipal**: Número de móvil principal
- **TipoLineaMovilPrincipal**: "Contrato" o "Prepago"
- **CodigoICCPrincipal**: Código ICC/SIM (19 dígitos)
- **NumeroLineas**: Cantidad de líneas móviles adicionales
- **IBAN**: Cuenta bancaria
- **Comision**: Comisión del contrato
- **Observaciones**: Notas adicionales

#### Líneas Adicionales
Para contratos con múltiples líneas móviles, usa la hoja "Líneas Adicionales":

| IdCliente* | NumeroLinea | Telefono | Tarifa | TipoLinea | CodigoICC |
|------------|-------------|----------|--------|-----------|-----------|
| 1 | 1 | 622334455 | Tarifa 20GB | Contrato | 8934071234567890456 |
| 1 | 2 | 633445566 | Tarifa 10GB | Contrato | 8934071234567890789 |

Soporta hasta 5 líneas adicionales por contrato.

#### Ejemplo de Datos
```
IdCliente: 1
Estado: Pendiente
Operadora: Movistar
Tarifa: Fusión Total
FijoTel: 912345678
LineaMovilPrincipal: 654321987
TipoLineaMovilPrincipal: Contrato
CodigoICCPrincipal: 8934071234567890123
NumeroLineas: 2
```

---

### 🚨 Contratos de Alarmas

#### Campos Obligatorios
- **IdCliente***: ID del cliente (debe existir en la base de datos)

#### Campos Principales
- **Estado**: Estado del contrato
- **Comercial**: Nombre del comercial
- **FechaAlta**: Fecha de alta (formato YYYY-MM-DD)
- **TipoAlarma**: "Hogar" o "Negocio"
- **SubtipoInmueble**: Piso, Bajo, Chalet, Local, Nave, etc.
- **EmpresaAlarma**: Securitas Direct, Prosegur, Movistar Prosegur Alarmas, etc.
- **KitAlarma**: Kit contratado
- **OpcionalesAlarma**: Opcionales adicionales
- **CampanaAlarma**: Campaña comercial
- **TieneContratoAnterior**: "Sí" o "No"
- **CompaniaAnterior**: Compañía anterior si aplica
- **DireccionInstalacion**: Dirección de instalación
- **NumeroInstalacion**: Número del inmueble
- **PisoInstalacion**: Piso/Puerta
- **CodigoPostalInstalacion**: Código postal
- **ProvinciaInstalacion**: Provincia
- **LocalidadInstalacion**: Localidad
- **IBAN**: Cuenta bancaria
- **Comision**: Comisión del contrato
- **Observaciones**: Notas adicionales

#### Ejemplo de Datos
```
IdCliente: 1
Estado: Pendiente
TipoAlarma: Hogar
SubtipoInmueble: Piso
EmpresaAlarma: Securitas Direct
KitAlarma: Kit Básico
TieneContratoAnterior: No
DireccionInstalacion: Calle Ejemplo
NumeroInstalacion: 25
PisoInstalacion: 3B
CodigoPostalInstalacion: 28001
```

---

## ⚠️ Notas Importantes

### Antes de Importar
1. **Backup**: Haz una copia de seguridad de tu base de datos
2. **Clientes**: Asegúrate de que los clientes existan previamente (importa primero los clientes)
3. **Validación**: Revisa que los datos estén completos y correctos

### Durante la Importación
- El script valida que el cliente exista antes de insertar el contrato
- Los campos vacíos se guardan como NULL
- La fecha de creación se asigna automáticamente
- El tipo de contrato se asigna automáticamente según la plantilla usada

### Formato de Datos
- **Fechas**: Formato YYYY-MM-DD (ej: 2026-01-29)
- **Decimales**: Usar punto (.) como separador (ej: 4.6, 50.00)
- **Booleanos**: Sí/No, Si/No, Yes/No, True/False, 1/0
- **Códigos CUPS**: Máximo 30 caracteres
- **Códigos ICC**: 19 dígitos
- **IBAN**: Máximo 34 caracteres

## 🔍 Validaciones del Script

El script realiza las siguientes validaciones:

✅ Verifica que el cliente exista en la base de datos  
✅ Valida formatos de fecha  
✅ Convierte correctamente decimales y enteros  
✅ Limpia valores vacíos y los convierte a NULL  
✅ Valida valores booleanos  
✅ Maneja errores por fila sin detener la importación  

## 📈 Salida del Script

Ejemplo de salida exitosa:
```
============================================================
IMPORTACIÓN DE CONTRATOS A LA BASE DE DATOS
============================================================
Tipo: ENERGÍA
Archivo: plantilla_contratos_energia.xlsx
Base de datos: enerfone_pre
============================================================

Leyendo archivo: plantilla_contratos_energia.xlsx
✓ Se encontraron 5 filas
Conectando a la base de datos enerfone_pre...
✓ Fila 2: Contrato para cliente 1 - Importado
✓ Fila 3: Contrato para cliente 2 - Importado
✓ Fila 4: Contrato para cliente 3 - Importado
❌ Fila 5: Error - El cliente con ID 999 no existe
✓ Fila 6: Contrato para cliente 4 - Importado

============================================================
✅ IMPORTACIÓN DE CONTRATOS DE ENERGÍA COMPLETADA
============================================================
Contratos importados: 4
Filas con errores: 1

============================================================
ERRORES DETECTADOS:
============================================================
❌ Fila 5: El cliente con ID 999 no existe

✓ Proceso finalizado
```

## 🐛 Solución de Problemas

### "El cliente con ID X no existe"
- Verifica que hayas importado primero los clientes
- Comprueba que el ID sea correcto en la tabla `clientes_simple`

### "Error al conectar a la base de datos"
- Verifica usuario y contraseña en `DB_CONFIG`
- Asegúrate de que MySQL esté en ejecución
- Verifica que tengas permisos en la base de datos

### "No se encontró la hoja"
- Asegúrate de que el nombre de la hoja sea exacto
- Para energía: "Contratos Energía"
- Para telefonía: "Contratos Telefonía"
- Para alarmas: "Contratos Alarmas"

### Error con decimales
- Usa punto (.) como separador decimal, no coma (,)
- El script intentará convertir automáticamente

## 📚 Orden Recomendado de Importación

1. **Primero**: Importar clientes
   ```bash
   python3 importar_clientes.py plantilla_clientes.xlsx
   ```

2. **Después**: Importar contratos (en cualquier orden)
   ```bash
   python3 importar_contratos.py energia plantilla_contratos_energia.xlsx
   python3 importar_contratos.py telefonia plantilla_contratos_telefonia.xlsx
   python3 importar_contratos.py alarmas plantilla_contratos_alarmas.xlsx
   ```

## 🔄 Regenerar Plantillas

Si necesitas regenerar las plantillas:

```bash
python3 generar_plantillas_contratos.py
```

Esto creará nuevas plantillas con los datos de ejemplo.

## 💡 Consejos

- **Plantillas separadas**: Usa una plantilla diferente por cada tipo de contrato
- **Datos de prueba**: Haz primero una importación con 2-3 registros de prueba
- **Verificación**: Después de importar, revisa en la aplicación web que los datos sean correctos
- **Errores**: Si hay errores, corrígelos en el Excel y vuelve a importar solo esos registros
- **Respaldo**: Guarda las plantillas completadas como respaldo

---

Para más información sobre la importación de clientes, consulta [IMPORTACION_CLIENTES.md](IMPORTACION_CLIENTES.md)
