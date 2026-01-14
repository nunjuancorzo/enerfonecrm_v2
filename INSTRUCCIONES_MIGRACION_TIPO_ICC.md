# Instrucciones para Migración: Campos Tipo de Línea Móvil y Código ICC

## Descripción
Esta migración agrega campos para gestionar el tipo de línea móvil (Contrato o Prepago) y el código ICC para líneas prepago en contratos de telefonía.

## Campos Agregados

### Línea Móvil Principal:
- `tipo_linea_movil_principal` - VARCHAR(20): Indica si la línea es "Contrato" o "Prepago"
- `codigo_icc_principal` - VARCHAR(19): Código ICC de 19 dígitos (obligatorio para prepago)

### Líneas Adicionales (1 a 5):
Para cada línea adicional (linea1 a linea5):
- `tipo_linea[N]_tel` - VARCHAR(20): Tipo de línea
- `codigo_icc_linea[N]_tel` - VARCHAR(19): Código ICC

## Script SQL
Ubicación: `/EnerfoneCRM/Scripts/agregar_campos_tipo_icc_moviles.sql`

## Instrucciones de Aplicación

### 1. Conectarse a la Base de Datos
```bash
mysql -u root -p
```

### 2. Seleccionar la Base de Datos
```sql
USE tu_base_de_datos;
```

### 3. Ejecutar el Script
```bash
mysql -u root -p tu_base_de_datos < /ruta/al/proyecto/EnerfoneCRM/Scripts/agregar_campos_tipo_icc_moviles.sql
```

O desde MySQL:
```sql
source /ruta/al/proyecto/EnerfoneCRM/Scripts/agregar_campos_tipo_icc_moviles.sql;
```

### 4. Verificar la Migración
```sql
DESCRIBE contratos;
```

Buscar las columnas:
- tipo_linea_movil_principal
- codigo_icc_principal
- tipo_linea1_tel hasta tipo_linea5_tel
- codigo_icc_linea1_tel hasta codigo_icc_linea5_tel

## Validación en la Aplicación

### Reglas de Validación Implementadas:
1. **Tipo de Línea**: Opcional, valores permitidos: "Contrato" o "Prepago"
2. **Código ICC**: 
   - Obligatorio solo si el tipo es "Prepago"
   - Debe tener exactamente 19 dígitos
   - Solo caracteres numéricos
   - Formato estándar: 89 34 XX... (89 = SIM, 34 = España)

### Comportamiento en la Interfaz:
- Al seleccionar "Contrato": No se muestra el campo ICC
- Al seleccionar "Prepago": Aparece el campo ICC con validación obligatoria
- Los campos se aplican tanto a la línea móvil principal como a las 5 líneas adicionales

## Rollback (en caso necesario)
```sql
-- Eliminar campos de línea principal
ALTER TABLE contratos DROP COLUMN tipo_linea_movil_principal;
ALTER TABLE contratos DROP COLUMN codigo_icc_principal;

-- Eliminar campos de líneas adicionales
ALTER TABLE contratos DROP COLUMN tipo_linea1_tel;
ALTER TABLE contratos DROP COLUMN codigo_icc_linea1_tel;
ALTER TABLE contratos DROP COLUMN tipo_linea2_tel;
ALTER TABLE contratos DROP COLUMN codigo_icc_linea2_tel;
ALTER TABLE contratos DROP COLUMN tipo_linea3_tel;
ALTER TABLE contratos DROP COLUMN codigo_icc_linea3_tel;
ALTER TABLE contratos DROP COLUMN tipo_linea4_tel;
ALTER TABLE contratos DROP COLUMN codigo_icc_linea4_tel;
ALTER TABLE contratos DROP COLUMN tipo_linea5_tel;
ALTER TABLE contratos DROP COLUMN codigo_icc_linea5_tel;
```

## Notas Adicionales

### Sobre el Código ICC:
El código ICC (Integrated Circuit Card Identifier) es el identificador único de cada tarjeta SIM:
- **Longitud**: 19 dígitos
- **Primeros 2 dígitos**: 89 (telecomunicaciones)
- **Siguientes 2 dígitos**: Código de país (34 para España)
- **Siguientes 2 dígitos**: Operador
- **Restantes**: Identificación única de la SIM

### Compatibilidad:
- Los campos son NULL por defecto, por lo que no afecta a contratos existentes
- Los contratos existentes pueden editarse y agregar estos campos según sea necesario
- La validación solo se activa cuando se selecciona "Prepago"
