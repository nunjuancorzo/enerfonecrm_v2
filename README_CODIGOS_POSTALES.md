# Importación de Códigos Postales de España

## 📄 Archivos Generados

### 1. **generar_codigos_postales_sql.py**
Script Python que descarga y procesa automáticamente los datos de Geonames.

### 2. **ADD_MAESTRO_CODIGOS_POSTALES_COMPLETO.sql** ✨
Archivo SQL completo con **11,150 códigos postales** de España.

---

## 📊 Estadísticas del Archivo Generado

- **Total códigos postales**: 11,150
- **Líneas de código**: 11,258
- **Tamaño**: 0.46 MB
- **Lotes de INSERT**: 12 (1,000 registros por lote)
- **Fuente**: Geonames (http://download.geonames.org/export/zip/ES.zip)
- **Fecha generación**: 2026-06-15

---

## 🚀 Cómo Usar el Archivo SQL

### Opción 1: Desde línea de comandos MySQL

```bash
# Ejecutar el script completo
mysql -u root -p corcrm_db < ADD_MAESTRO_CODIGOS_POSTALES_COMPLETO.sql

# O si usas un usuario específico
mysql -u tu_usuario -p corcrm_db < ADD_MAESTRO_CODIGOS_POSTALES_COMPLETO.sql
```

### Opción 2: Desde cliente MySQL/HeidiSQL/Workbench

1. Abrir el cliente MySQL
2. Seleccionar la base de datos CorCRM
3. Abrir el archivo `ADD_MAESTRO_CODIGOS_POSTALES_COMPLETO.sql`
4. Ejecutar el script completo (F9 o botón ejecutar)

### Opción 3: Desde la aplicación .NET (si tienes Migrations)

```bash
cd EnerfoneCRM
dotnet ef database update
```

---

## 📦 Estructura de la Tabla

```sql
CREATE TABLE codigos_postales (
    id INT AUTO_INCREMENT PRIMARY KEY,
    codigo_postal VARCHAR(5) NOT NULL UNIQUE,
    ciudad VARCHAR(100) NOT NULL,
    provincia VARCHAR(50) NOT NULL,
    activo BOOLEAN DEFAULT TRUE,
    fecha_creacion DATETIME DEFAULT CURRENT_TIMESTAMP,
    fecha_modificacion DATETIME DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    
    INDEX idx_ciudad (ciudad),
    INDEX idx_provincia (provincia),
    INDEX idx_activo (activo)
);
```

---

## 🗺️ Cobertura Geográfica

El archivo incluye códigos postales de:

- ✅ Todas las provincias de España
- ✅ Ciudades Autónomas (Ceuta y Melilla)
- ✅ Islas Baleares
- ✅ Islas Canarias
- ✅ Municipios principales y localidades

### Ejemplos de provincias incluidas:

- Madrid (28xxx)
- Barcelona (08xxx)
- Valencia (46xxx)
- Sevilla (41xxx)
- País Vasco (01xxx, 20xxx, 48xxx)
- Canarias (35xxx, 38xxx)
- Baleares (07xxx)
- Y todas las demás provincias...

---

## ⚙️ Características del Script Python

### ¿Qué hace el script?

1. **Descarga** automáticamente los datos desde Geonames
2. **Extrae** el archivo ZIP
3. **Procesa** 37,867 líneas del archivo original
4. **Filtra** y valida códigos postales (elimina duplicados e inválidos)
5. **Genera** SQL optimizado en lotes de 1,000 registros
6. **Limpia** archivos temporales automáticamente

### Ventajas:

- ✅ Datos oficiales y actualizados de Geonames
- ✅ Validación automática de formato (5 dígitos)
- ✅ Eliminación de duplicados
- ✅ Optimizado para MySQL (lotes de 1,000)
- ✅ `ON DUPLICATE KEY UPDATE` para actualizaciones seguras
- ✅ Caracteres especiales escapados correctamente

---

## 🔄 Regenerar el Archivo

Si necesitas actualizar los datos en el futuro:

```bash
# Volver a ejecutar el script Python
python3 generar_codigos_postales_sql.py

# Se generará un nuevo archivo con datos actualizados de Geonames
```

---

## 📝 Notas Importantes

1. **Primera ejecución**: El script crea la tabla si no existe
2. **Actualizaciones**: Usa `ON DUPLICATE KEY UPDATE`, puedes ejecutarlo múltiples veces de forma segura
3. **Tiempo de ejecución**: ~2-5 segundos para descargar y procesar
4. **Importación SQL**: ~5-10 segundos para 11,150 registros

---

## 🛠️ Verificación Post-Importación

Después de ejecutar el SQL, verifica los datos:

```sql
-- Ver estadísticas
SELECT 
    COUNT(*) as total_codigos_postales,
    COUNT(DISTINCT provincia) as total_provincias,
    COUNT(DISTINCT ciudad) as total_ciudades
FROM codigos_postales;

-- Buscar un código postal específico
SELECT * FROM codigos_postales WHERE codigo_postal = '28001';

-- Ver códigos postales de Madrid
SELECT * FROM codigos_postales WHERE provincia LIKE '%Madrid%' LIMIT 20;

-- Ver todas las provincias disponibles
SELECT DISTINCT provincia FROM codigos_postales ORDER BY provincia;
```

---

## ✅ Listo para Usar

Una vez importado, el sistema CorCRM podrá:

- ✨ Autocompletar códigos postales en formularios de clientes
- ✨ Autocompletar ciudad y provincia al seleccionar un CP
- ✨ Validar códigos postales existentes
- ✨ Gestionar maestro desde `/administracion/codigos-postales`

---

**Última actualización**: 2026-06-15  
**Fuente de datos**: [Geonames.org](http://www.geonames.org/)  
**Licencia**: CC BY 4.0 (Geonames)
