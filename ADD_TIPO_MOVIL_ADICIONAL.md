# Nuevo Tipo de Tarifa: Móvil Adicional

## Descripción
Se ha agregado un nuevo tipo de tarifa de telefonía llamado **"MovilAdicional"** (Móvil Adicional).

## Propósito
Este tipo de tarifa está diseñado para las líneas móviles adicionales que se añaden a los contratos con tipos:
- **FibraMovil** (Fibra + Móvil)
- **Movil** (Solo Móvil)  
- **FibraMovilTV** (Fibra + Móvil + TV)

## Cambios Implementados

### 1. Modelo de Datos
✅ **No requiere cambios en la base de datos**
- El campo `tipo` en la tabla `tarifastelefonia` ya acepta valores string
- El nuevo valor "MovilAdicional" se almacena sin modificaciones en el esquema

### 2. Interfaz de Usuario (TarifasTelefonia.razor)
✅ Agregado en el selector de tipo:
```html
<option value="MovilAdicional">Móvil Adicional</option>
```

### 3. Creación/Edición de Contratos (ContratosTelefonia.razor)
✅ Método `CargarTarifasMovilSolo()` actualizado para incluir:
- Tarifas de tipo "Movil"
- Tarifas de tipo "MovilAdicional"

Ahora las líneas adicionales pueden seleccionar tanto tarifas móviles estándar como tarifas específicas para móviles adicionales.

### 4. Scripts Python

#### importar_tarifas_telefonia.py
✅ Agregado "MovilAdicional" a tipos válidos:
```python
tipos_validos = {'FibraMovil', 'Fibra', 'Movil', 'MovilAdicional', 'FibraMovilTV', 'FibraSegundaResidencia'}
```

#### generar_plantilla_tarifas_telefonia.py
✅ Agregado ejemplo de tarifa MovilAdicional en plantilla Excel:
```python
['', 'Pepephone', 'MovilAdicional', 'Línea adicional 30GB', '', '30 GB', '', '', '', '10,00', '15,00', '12 meses', '2026-02-24']
```

✅ Actualizada documentación en hoja de instrucciones

## Tipos de Tarifas Disponibles

Después de este cambio, los tipos válidos son:

| Valor en BD | Descripción | Uso |
|-------------|-------------|-----|
| FibraMovil | Fibra y Móvil | Paquetes combinados fibra + móvil |
| Fibra | Fibra | Solo servicio de fibra |
| Movil | Móvil | Línea móvil principal |
| **MovilAdicional** | **Móvil Adicional** | **Líneas móviles adicionales** |
| FibraMovilTV | Fibra, Móvil y TV | Paquetes triple play |
| FibraSegundaResidencia | Fibra Segunda Residencia | Fibra para segunda vivienda |

## Uso

### Creación de Tarifas
1. Ir a **Tarifas → Telefonía**
2. Crear nueva tarifa
3. Seleccionar tipo **"Móvil Adicional"**
4. Configurar GB, precio, comisión, etc.

### Asignación en Contratos
Cuando se crean/editan contratos de telefonía con tipos FibraMovil, Movil o FibraMovilTV:
- Al agregar líneas adicionales, el selector mostrará **automáticamente**:
  - Tarifas de tipo "Movil"
  - Tarifas de tipo "MovilAdicional"

### Importación Masiva
Al importar tarifas desde Excel:
1. Generar plantilla: `python3 generar_plantilla_tarifas_telefonia.py`
2. En columna **TIPO**, usar valor exacto: `MovilAdicional`
3. Importar: `python3 importar_tarifas_telefonia.py archivo.xlsx`

## Notas Importantes

⚠️ **Valor exacto**: El tipo debe escribirse exactamente como `MovilAdicional` (sin espacios, con mayúsculas/minúsculas correctas)

✅ **Compatibilidad**: Este cambio es backward compatible - las tarifas existentes no se ven afectadas

✅ **Exportación**: El script `exportar_tarifas_telefonia.py` exportará correctamente el nuevo tipo sin modificaciones

## Fecha de Implementación
Marzo 2026
