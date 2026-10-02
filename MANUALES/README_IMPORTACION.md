# Sistema de Importación Masiva de Datos

Sistema completo para importación masiva de **Clientes**, **Contratos** (Energía, Telefonía, Alarmas), **Tarifas** y **Servicios** a la base de datos MySQL, ideal para carga inicial de datos.

## 📦 Archivos del Sistema

### Plantillas Excel - Clientes y Contratos
- `plantilla_clientes.xlsx` - Para importar clientes
- `plantilla_contratos_energia.xlsx` - Para contratos de energía
- `plantilla_contratos_telefonia.xlsx` - Para contratos de telefonía
- `plantilla_contratos_alarmas.xlsx` - Para contratos de alarmas

### Plantillas Excel - Tarifas y Servicios
- `plantilla_tarifas_energia.xlsx` - Para tarifas de energía
- `plantilla_tarifas_telefonia.xlsx` - Para tarifas de telefonía
- `plantilla_tarifas_alarmas.xlsx` - Para tarifas de alarmas
- `plantilla_servicios.xlsx` - Para servicios

### Scripts de Importación
- `importar_clientes.py` - Importa clientes a MySQL
- `importar_contratos.py` - Importa contratos de los tres tipos a MySQL
- `importar_tarifas_servicios.py` - Importa tarifas y servicios a MySQL

### Scripts Generadores
- `generar_plantilla.py` / `generar_plantilla_simple.py` - Regenera plantilla de clientes
- `generar_plantillas_contratos.py` - Regenera plantillas de contratos
- `generar_plantillas_tarifas_servicios.py` - Regenera plantillas de tarifas y servicios

### Documentación
- `IMPORTACION_CLIENTES.md` - Guía completa de importación de clientes
- `IMPORTACION_CONTRATOS.md` - Guía completa de importación de contratos
- `IMPORTACION_TARIFAS_SERVICIOS.md` - Guía completa de importación de tarifas y servicios
- `README_IMPORTACION.md` - Este archivo (guía rápida)

## 🚀 Inicio Rápido

### 1. Instalar Dependencias

```bash
pip3 install --user pandas openpyxl mysql-connector-python xlsxwriter
```

### 2. Generar Plantillas (si no las tienes)

```bash
# Plantilla de clientes
python3 generar_plantilla_simple.py

# Plantillas de contratos
python3 generar_plantillas_contratos.py

# Plantillas de tarifas y servicios
python3 generar_plantillas_tarifas_servicios.py
```

### 3. Rellenar Plantillas Excel

Abre las plantillas Excel y rellena con tus datos:
- Cada plantilla incluye una fila de ejemplo
- Cada plantilla tiene una hoja de "Instrucciones" con ayuda detallada
- **NO elimines** la fila de encabezados

### 4. Ejecutar Importaciones

⚠️ **IMPORTANTE**: Especifica siempre el nombre de la base de datos correcta:
- **Enerfone**: `enerfone_pre`
- **Grupo Basette**: `crmgrupobasette`

```bash
# 1. PRIMERO: Importar clientes
python3 importar_clientes.py <nombre_bd> plantilla_clientes.xlsx

# 2. DESPUÉS: Importar contratos (en cualquier orden)
python3 importar_contratos.py <nombre_bd> energia plantilla_contratos_energia.xlsx
python3 importar_contratos.py <nombre_bd> telefonia plantilla_contratos_telefonia.xlsx
python3 importar_contratos.py <nombre_bd> alarmas plantilla_contratos_alarmas.xlsx

# 3. DESPUÉS: Importar tarifas y servicios (en cualquier orden)
python3 importar_tarifas_servicios.py <nombre_bd> tarifa-energia plantilla_tarifas_energia.xlsx
python3 importar_tarifas_servicios.py <nombre_bd> tarifa-telefonia plantilla_tarifas_telefonia.xlsx
python3 importar_tarifas_servicios.py <nombre_bd> tarifa-alarmas plantilla_tarifas_alarmas.xlsx
python3 importar_tarifas_servicios.py <nombre_bd> servicios plantilla_servicios.xlsx
```

**Ejemplos para Grupo Basette:**
```bash
python3 importar_clientes.py crmgrupobasette plantilla_clientes.xlsx
python3 importar_tarifas_servicios.py crmgrupobasette tarifa-energia plantilla_tarifas_energia.xlsx
```

## 📋 Guía Rápida por Tipo de Dato

### 👥 Clientes

**Campos obligatorios:**
- TipoCliente: "Particular" o "Empresa"
- Nombre: Nombre o razón social

**Campos opcionales:** DNI/CIF, Email, Teléfono, Dirección completa, IBAN, Representante, Comercial, Observaciones

**Comando:**
```bash
python3 importar_clientes.py plantilla_clientes.xlsx [id_usuario]
```

**Documentación completa:** [IMPORTACION_CLIENTES.md](IMPORTACION_CLIENTES.md)

---

### 🔋 Contratos de Energía

**Campo obligatorio:**
- IdCliente: ID del cliente (debe existir previamente)

**Campos principales:** Comercializadora, Tarifa, CUPS, CUPSGas, PotenciaContratada, ConsumoAnual, Estado, Comisión

**Comando:**
```bash
python3 importar_contratos.py energia plantilla_contratos_energia.xlsx
```

---

### 📱 Contratos de Telefonía

**Campo obligatorio:**
- IdCliente: ID del cliente (debe existir previamente)

**Campos principales:** Operadora, Tarifa, FijoTel, LineaMovilPrincipal, CodigoICCPrincipal, NumeroLineas

**Características especiales:**
- Soporta línea principal + hasta 5 líneas adicionales
- Hoja adicional "Líneas Adicionales" para líneas extra

**Comando:**
```bash
python3 importar_contratos.py telefonia plantilla_contratos_telefonia.xlsx
```

---

### 🚨 Contratos de Alarmas

**Campo obligatorio:**
- IdCliente: ID del cliente (debe existir previamente)

**Campos principales:** TipoAlarma (Hogar/Negocio), EmpresaAlarma, KitAlarma, DireccionInstalacion completa

**Comando:**
```bash
python3 importar_contratos.py alarmas plantilla_contratos_alarmas.xlsx
```

---

### ⚡ Tarifas de Energía

**Campos obligatorios:**
- Empresa, Tipo, Nombre, Potencia1, Energia1, Comision, PrecioNew

**Campos principales:** Potencia y energía por periodos, precios comparativos

**Comando:**
```bash
python3 importar_tarifas_servicios.py tarifa-energia plantilla_tarifas_energia.xlsx
```

---

### 📱 Tarifas de Telefonía

**Campos obligatorios:**
- Compania, Tipo, PrecioNew, ComisionNew

**Campos principales:** Fibra, GbMovil, TV

**Comando:**
```bash
python3 importar_tarifas_servicios.py tarifa-telefonia plantilla_tarifas_telefonia.xlsx
```

---

### 🚨 Tarifas de Alarmas

**Campos obligatorios:**
- Tipo (Kit/Opcional/Campaña), TipoInmueble (Hogar/Negocio), NombreTarifa, CuotaMensual, Permanencia

**Comando:**
```bash
python3 importar_tarifas_servicios.py tarifa-alarmas plantilla_tarifas_alarmas.xlsx
```

---

### 🛠️ Servicios

**Campos obligatorios:**
- Tipo (Energía/Telefonía/Alarmas/General), NombreServicio, Precio

**Comando:**
```bash
python3 importar_tarifas_servicios.py servicios plantilla_servicios.xlsx
```

---

**Documentación completa de contratos:** [IMPORTACION_CONTRATOS.md](IMPORTACION_CONTRATOS.md)
**Documentación completa de tarifas y servicios:** [IMPORTACION_TARIFAS_SERVICIOS.md](IMPORTACION_TARIFAS_SERVICIOS.md)

## ⚠️ Consideraciones Importantes

### Orden de Importación
1. **Primero**: Importar clientes
2. **Segundo**: Importar contratos (requieren que los clientes existan)
3. **Tercero**: Importar tarifas y servicios (pueden importarse independientemente)

### Antes de Importar
- ✅ Haz backup de tu base de datos
- ✅ Verifica que las plantillas estén completas
- ✅ Prueba con 2-3 registros primero
- ✅ Configura correctamente la conexión a la base de datos

### Formatos de Datos
- **Fechas**: YYYY-MM-DD (ej: 2026-01-29)
- **Decimales**: Usar punto (.) no coma (ej: 4.6, 50.00)
- **Booleanos**: Sí/No, Si/No, True/False, 1/0
- **Emails**: Formato válido (se valida automáticamente)

### Validaciones Automáticas
- ✅ Clientes: Valida TipoCliente, Nombre, Email, IBAN
- ✅ Contratos: Valida que el cliente exista antes de insertar
- ✅ Tarifas Alarmas: Valida Tipo (Kit/Opcional/Campaña) y TipoInmueble (Hogar/Negocio)
- ✅ Fechas: Convierte automáticamente a formato correcto
- ✅ Decimales: Acepta punto y coma como separador

## 📊 Ejemplo de Salida

```
============================================================
IMPORTACIÓN DE CLIENTES A LA BASE DE DATOS
============================================================
Archivo: plantilla_clientes.xlsx
Base de datos: enerfone_pre
============================================================

Leyendo archivo: plantilla_clientes.xlsx
✓ Se encontraron 10 filas
Conectando a la base de datos enerfone_pre...
✓ Fila 2: Empresa Ejemplo S.L. - Importado correctamente
✓ Fila 3: Juan Pérez García - Importado correctamente
✓ Fila 4: María López Martín - Importado correctamente
...
✓ Fila 11: Ana Torres Ruiz - Importado correctamente

============================================================
✅ IMPORTACIÓN COMPLETADA
============================================================
Clientes importados: 10
Filas con errores: 0

✓ Conexión cerrada
```

## 🔧 Solución de Problemas

### Error: "ModuleNotFoundError"
```bash
pip3 install --user pandas openpyxl mysql-connector-python xlsxwriter
```

### Error: "Access denied for user"
- Verifica usuario y contraseña en `DB_CONFIG`
- Asegúrate de tener permisos en la base de datos

### Error: "El cliente con ID X no existe"
- Importa primero los clientes antes de importar contratos
- Verifica que el ID sea correcto

### Error: "No se encontró la hoja"
- Verifica que el nombre de la hoja sea exacto
- No modifiques los nombres de las hojas en las plantillas

### Errores de formato
- Fechas: Usa formato YYYY-MM-DD
- Decimales: Usa punto (.) como separador
- El script intentará convertir automáticamente

## 📁 Estructura de Archivos

```
EnerfoneCRMv2/
├── plantilla_clientes.xlsx                    # Plantilla clientes
├── plantilla_contratos_energia.xlsx           # Plantilla energía
├── plantilla_contratos_telefonia.xlsx         # Plantilla telefonía
├── plantilla_contratos_alarmas.xlsx           # Plantilla alarmas
├── plantilla_tarifas_energia.xlsx             # Plantilla tarifas energía
├── plantilla_tarifas_telefonia.xlsx           # Plantilla tarifas telefonía
├── plantilla_tarifas_alarmas.xlsx             # Plantilla tarifas alarmas
├── plantilla_servicios.xlsx                   # Plantilla servicios
├── importar_clientes.py                       # Script importación clientes
├── importar_contratos.py                      # Script importación contratos
├── importar_tarifas_servicios.py              # Script importación tarifas y servicios
├── generar_plantilla_simple.py                # Generador plantilla clientes
├── generar_plantillas_contratos.py            # Generador plantillas contratos
├── generar_plantillas_tarifas_servicios.py    # Generador plantillas tarifas
├── IMPORTACION_CLIENTES.md                    # Docs clientes
├── IMPORTACION_CONTRATOS.md                   # Docs contratos
├── IMPORTACION_TARIFAS_SERVICIOS.md           # Docs tarifas y servicios
└── README_IMPORTACION.md                      # Este archivo
```

## 💡 Consejos y Buenas Prácticas

1. **Prueba primero**: Importa 2-3 registros de prueba antes de la carga masiva
2. **Backup**: Siempre haz backup antes de importar datos
3. **Verifica**: Después de importar, revisa en la aplicación web
4. **Guarda plantillas**: Conserva las plantillas completadas como respaldo
5. **Errores**: Si hay errores, corrígelos y vuelve a importar solo esos registros
6. **Orden**: Respeta el orden: primero clientes, después contratos

## 🔄 Regenerar Plantillas

Si necesitas regenerar las plantillas vacías:

```bash
# Plantilla de clientes
python3 generar_plantilla_simple.py

# Plantillas de contratos
python3 generar_plantillas_contratos.py

# Plantillas de tarifas y servicios
python3 generar_plantillas_tarifas_servicios.py
```

## 📞 Resumen de Comandos

```bash
# Instalación
pip3 install --user pandas openpyxl mysql-connector-python xlsxwriter

# Generar plantillas
python3 generar_plantilla_simple.py
python3 generar_plantillas_contratos.py
python3 generar_plantillas_tarifas_servicios.py

# Importar datos
python3 importar_clientes.py plantilla_clientes.xlsx
python3 importar_contratos.py energia plantilla_contratos_energia.xlsx
python3 importar_contratos.py telefonia plantilla_contratos_telefonia.xlsx
python3 importar_contratos.py alarmas plantilla_contratos_alarmas.xlsx
python3 importar_tarifas_servicios.py tarifa-energia plantilla_tarifas_energia.xlsx
python3 importar_tarifas_servicios.py tarifa-telefonia plantilla_tarifas_telefonia.xlsx
python3 importar_tarifas_servicios.py tarifa-alarmas plantilla_tarifas_alarmas.xlsx
python3 importar_tarifas_servicios.py servicios plantilla_servicios.xlsx
```

## 📚 Documentación Detallada

- **Clientes**: Ver [IMPORTACION_CLIENTES.md](IMPORTACION_CLIENTES.md)
- **Contratos**: Ver [IMPORTACION_CONTRATOS.md](IMPORTACION_CONTRATOS.md)
- **Tarifas y Servicios**: Ver [IMPORTACION_TARIFAS_SERVICIOS.md](IMPORTACION_TARIFAS_SERVICIOS.md)

---

**Última actualización**: 29 de enero de 2026
