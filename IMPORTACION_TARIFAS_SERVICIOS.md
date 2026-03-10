# Importación Masiva de Tarifas y Servicios

Sistema completo para importación masiva de **Tarifas** (Energía, Telefonía, Alarmas) y **Servicios** a la base de datos MySQL.

## 📋 Contenido

- `plantilla_tarifas_energia.xlsx` - Plantilla para tarifas de energía
- `plantilla_tarifas_telefonia.xlsx` - Plantilla para tarifas de telefonía
- `plantilla_tarifas_alarmas.xlsx` - Plantilla para tarifas de alarmas
- `plantilla_servicios.xlsx` - Plantilla para servicios
- `importar_tarifas_servicios.py` - Script Python que importa los datos a MySQL
- `generar_plantillas_tarifas_servicios.py` - Script para regenerar las plantillas
- Este documento con instrucciones

## 🚀 Instrucciones de Uso

### Paso 1: Instalar Dependencias

```bash
pip3 install --user pandas openpyxl mysql-connector-python
```

### Paso 2: Generar las Plantillas Excel

```bash
python3 generar_plantillas_tarifas_servicios.py
```

Esto generará cuatro archivos Excel con ejemplos e instrucciones.

### Paso 3: Rellenar las Plantillas

Cada plantilla incluye:
- **Hoja principal**: Donde debes rellenar los datos
- **Hoja de Instrucciones**: Información detallada sobre cada campo
- **Fila de ejemplo**: Para guiarte en el formato

### Paso 4: Ejecutar la Importación

**⚠️ IMPORTANTE**: Especifica el nombre de la base de datos correcta según tu instalación:
- **Enerfone**: `enerfone_pre`
- **Grupo Basette**: `crmgrupobasette`

```bash
# Tarifas de energía
python3 importar_tarifas_servicios.py <nombre_bd> tarifa-energia plantilla_tarifas_energia.xlsx

# Tarifas de telefonía
python3 importar_tarifas_servicios.py <nombre_bd> tarifa-telefonia plantilla_tarifas_telefonia.xlsx

# Tarifas de alarmas
python3 importar_tarifas_servicios.py <nombre_bd> tarifa-alarmas plantilla_tarifas_alarmas.xlsx

# Servicios
python3 importar_tarifas_servicios.py <nombre_bd> servicios plantilla_servicios.xlsx
```

**Ejemplos concretos:**
```bash
# Para Enerfone
python3 importar_tarifas_servicios.py enerfone_pre tarifa-energia plantilla_tarifas_energia.xlsx

# Para Grupo Basette
python3 importar_tarifas_servicios.py crmgrupobasette tarifa-telefonia plantilla_tarifas_telefonia.xlsx
```

## 📊 Detalles de Cada Tipo

### ⚡ Tarifas de Energía

#### Campos Obligatorios (marcados con *)
- **Empresa***: Nombre de la comercializadora (Iberdrola, Endesa, Naturgy, etc.)
- **Tipo***: Tipo de tarifa (Luz, Gas, Luz+Gas)
- **Nombre***: Nombre de la tarifa
- **Potencia1***: Precio potencia periodo 1 (texto)
- **Energia1***: Precio energía periodo 1 (texto)
- **Comision***: Comisión de la tarifa (decimal)
- **PrecioNew***: Precio mensual estimado (decimal)

#### Campos Opcionales
- **Precio**: Precio alternativo (texto)
- **Potencia2-6**: Precios potencia periodos adicionales (texto)
- **Energia2-6**: Precios energía periodos adicionales (texto)
- **TerminoFijoDiario**: Término fijo diario (decimal)
- **PrecioPotenciaP1-P3**: Precio potencia por periodo para comparativa (decimal)
- **PrecioEnergiaP1-P3**: Precio energía por periodo para comparativa (decimal)

#### Ejemplo
```
Empresa: Iberdrola
Tipo: Luz
Nombre: Tarifa 2.0TD Estable
Potencia1: 0.12
Energia1: 0.15
Comision: 50.00
PrecioNew: 45.50
```

---

### 📱 Tarifas de Telefonía

#### Campos Obligatorios (marcados con *)
- **Compania***: Nombre de la operadora (Movistar, Vodafone, Orange, MásMóvil, etc.)
- **Tipo***: Tipo de tarifa (Solo Fibra, Solo Móvil, Fibra + Móvil, Convergente)
- **PrecioNew***: Precio mensual (decimal)
- **ComisionNew***: Comisión de la tarifa (decimal)

#### Campos Opcionales
- **Fibra**: Velocidad de fibra (100Mb, 300Mb, 600Mb, 1Gb)
- **GbMovil**: Datos móviles incluidos (20GB, 50GB, Ilimitado)
- **Precio**: Precio alternativo (texto)
- **Comision**: Comisión alternativa (texto)
- **TV**: Servicios de TV incluidos

#### Ejemplo
```
Compania: Movistar
Tipo: Fibra + Móvil
Fibra: 600Mb
GbMovil: 50GB
PrecioNew: 49.90
ComisionNew: 40.00
TV: Netflix incluido
```

---

### 🚨 Tarifas de Alarmas

#### Campos Obligatorios (marcados con *)
- **Tipo***: Tipo de tarifa - "Kit", "Opcional" o "Campaña"
- **TipoInmueble***: "Hogar" o "Negocio"
- **NombreTarifa***: Nombre descriptivo de la tarifa
- **CuotaMensual***: Cuota mensual (decimal)
- **Permanencia***: Meses de permanencia (número entero: 0, 12, 24, 36)

#### Campos Opcionales
- **Empresa**: Nombre de la empresa de alarmas
- **Comision**: Comisión de la tarifa (decimal)
- **Descripcion**: Descripción detallada
- **Activa**: Estado de la tarifa - "Sí" o "No" (por defecto Sí)

#### Tipos de Tarifas de Alarmas

**Kit**: Kits completos de alarma
- Ejemplo: Kit Básico Hogar, Kit Premium, Kit Negocio

**Opcional**: Elementos opcionales adicionales
- Ejemplo: Detector de humo, Cámara adicional, Sensor de movimiento

**Campaña**: Ofertas y campañas especiales
- Ejemplo: Promoción Verano, Black Friday, Campaña Navidad

#### Ejemplos
```
Kit:
Tipo: Kit
TipoInmueble: Hogar
NombreTarifa: Kit Básico Hogar
CuotaMensual: 29.90
Permanencia: 24
Empresa: Securitas Direct

Opcional:
Tipo: Opcional
TipoInmueble: Hogar
NombreTarifa: Detector de Humo
CuotaMensual: 3.50
Permanencia: 0

Campaña:
Tipo: Campaña
TipoInmueble: Negocio
NombreTarifa: Promoción Verano
CuotaMensual: 39.90
Permanencia: 36
Empresa: Prosegur
```

---

### 🛠️ Servicios

#### Campos Obligatorios (marcados con *)
- **Tipo***: Tipo de servicio (Energía, Telefonía, Alarmas, General)
- **NombreServicio***: Nombre descriptivo del servicio
- **Precio***: Precio del servicio (texto, puede incluir "/mes", "/año", etc.)

#### Campos Opcionales
- **Empresa**: Empresa que ofrece el servicio

#### Ejemplos por Tipo

**Servicios de Energía:**
```
Tipo: Energía
NombreServicio: Mantenimiento caldera anual
Precio: 120.00
Empresa: Iberdrola
```

**Servicios de Telefonía:**
```
Tipo: Telefonía
NombreServicio: Router WiFi 6
Precio: 5.00/mes
Empresa: Movistar
```

**Servicios de Alarmas:**
```
Tipo: Alarmas
NombreServicio: Mantenimiento preventivo
Precio: 50.00
Empresa: Securitas
```

**Servicios Generales:**
```
Tipo: General
NombreServicio: Consultoría energética
Precio: 200.00
```

---

## ⚠️ Notas Importantes

### Formatos de Datos
- **Decimales**: Usar punto (.) como separador (ej: 49.90, 29.50)
- **Texto**: Los campos de precio en tarifas de energía son texto, pueden incluir descripciones
- **Booleanos**: Sí/No, Si/No, Yes/No, True/False, 1/0
- **Permanencia**: Número entero (0, 12, 24, 36)

### Validaciones Automáticas
✅ Verifica campos obligatorios  
✅ Valida tipos de datos (texto, decimal, entero)  
✅ Convierte decimales automáticamente (acepta punto y coma)  
✅ Valida valores específicos (Tipo de alarma, TipoInmueble, etc.)  
✅ Maneja errores por fila sin detener la importación  

### Antes de Importar
1. **Backup**: Haz una copia de seguridad de tu base de datos
2. **Prueba**: Importa 2-3 registros de prueba primero
3. **Verifica**: Revisa que los datos estén completos y correctos en el Excel

## 📈 Salida del Script

Ejemplo de salida exitosa:
```
============================================================
IMPORTACIÓN A LA BASE DE DATOS
============================================================
Tipo: TARIFA-ENERGIA
Archivo: plantilla_tarifas_energia.xlsx
Base de datos: enerfone_pre
============================================================

Leyendo archivo: plantilla_tarifas_energia.xlsx
✓ Se encontraron 5 filas
Conectando a la base de datos enerfone_pre...
✓ Fila 2: Tarifa 2.0TD Estable - Importado
✓ Fila 3: Tarifa 3.0TD Premium - Importado
✓ Fila 4: Gas Natural Estándar - Importado
✓ Fila 5: Luz + Gas Combinada - Importado
✓ Fila 6: Tarifa Indexada - Importado

============================================================
✅ IMPORTACIÓN DE TARIFAS DE ENERGÍA COMPLETADA
============================================================
Registros importados: 5
Filas con errores: 0

✓ Proceso finalizado
```

## 🐛 Solución de Problemas

### Error: "Tipo debe ser 'Kit', 'Opcional' o 'Campaña'"
- En tarifas de alarmas, el tipo debe ser exactamente uno de estos tres valores
- Respeta mayúsculas y minúsculas

### Error: "TipoInmueble debe ser 'Hogar' o 'Negocio'"
- Solo se aceptan estos dos valores para tarifas de alarmas
- Respeta mayúsculas y minúsculas

### Error con decimales
- Usa punto (.) como separador decimal
- El script intenta convertir automáticamente comas

### "Campo X es obligatorio"
- Asegúrate de rellenar todos los campos marcados con asterisco (*)
- No dejes celdas vacías en campos obligatorios

## 💡 Consejos

1. **Organización**: Rellena una plantilla a la vez
2. **Copiar filas**: Puedes duplicar la fila de ejemplo y modificar los datos
3. **No eliminar encabezados**: Nunca elimines la primera fila
4. **Validar antes**: Revisa los datos en Excel antes de importar
5. **Importar por partes**: Si tienes muchos registros, importa en lotes

## 🔄 Regenerar Plantillas

Si necesitas regenerar las plantillas:

```bash
python3 generar_plantillas_tarifas_servicios.py
```

## 📚 Resumen de Comandos

```bash
# Instalación
pip3 install --user pandas openpyxl mysql-connector-python

# Generar plantillas
python3 generar_plantillas_tarifas_servicios.py

# Importar datos (especifica la base de datos correcta)
python3 importar_tarifas_servicios.py <nombre_bd> tarifa-energia plantilla_tarifas_energia.xlsx
python3 importar_tarifas_servicios.py <nombre_bd> tarifa-telefonia plantilla_tarifas_telefonia.xlsx
python3 importar_tarifas_servicios.py <nombre_bd> tarifa-alarmas plantilla_tarifas_alarmas.xlsx
python3 importar_tarifas_servicios.py <nombre_bd> servicios plantilla_servicios.xlsx
```

## 📁 Estructura de Tablas en la Base de Datos

### tarifasenergia
- Almacena tarifas de luz, gas y combinadas
- Incluye campos para comparativa de precios

### tarifastelefonia
- Almacena tarifas de fibra, móvil y convergentes
- Incluye información de datos y TV

### tarifas_alarmas
- Almacena kits, opcionales y campañas
- Diferencia entre hogar y negocio

### servicios
- Almacena servicios adicionales de cualquier tipo
- Clasificados por tipo (Energía, Telefonía, Alarmas, General)

---

Para más información sobre la importación de clientes y contratos, consulta:
- [IMPORTACION_CLIENTES.md](IMPORTACION_CLIENTES.md)
- [IMPORTACION_CONTRATOS.md](IMPORTACION_CONTRATOS.md)
- [README_IMPORTACION.md](README_IMPORTACION.md)

**Última actualización**: 29 de enero de 2026
