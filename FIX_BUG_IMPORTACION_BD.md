# 🐛 FIX: Bug de Importación a Base de Datos Incorrecta

## El Problema

El script `importar_tarifas_servicios.py` tenía **hardcoded** el nombre de la base de datos `enerfone_pre`, lo que causaba que:

### ❌ Comportamiento Incorrecto
Cuando se ejecutaba el script desde la instalación de **Grupo Basette**, las tarifas siempre se importaban a `enerfone_pre` en lugar de a `crmgrupobasette`.

```python
# ❌ ANTES (código antiguo)
DB_CONFIG = {
    'host': 'localhost',
    'database': 'enerfone_pre',  # SIEMPRE enerfone_pre
    'user': 'root',
    'password': ''
}
```

## La Solución

### ✅ Código Actualizado
Ahora el script acepta el nombre de la base de datos como **argumento de línea de comandos**, igual que los otros scripts de importación.

```python
# ✅ AHORA (código corregido)
database_name = sys.argv[1]
db_user = sys.argv[4] if len(sys.argv) > 4 else 'root'
db_password = sys.argv[5] if len(sys.argv) > 5 else 'A76262136.r'

DB_CONFIG = {
    'host': 'localhost',
    'database': database_name,  # Se pasa como argumento
    'user': db_user,
    'password': db_password
}
```

## 📝 Nueva Sintaxis de Uso

### Antes
```bash
python3 importar_tarifas_servicios.py tarifa-energia plantilla_tarifas_energia.xlsx
```

### Ahora
```bash
# Para Enerfone
python3 importar_tarifas_servicios.py enerfone_pre tarifa-energia plantilla_tarifas_energia.xlsx

# Para Grupo Basette
python3 importar_tarifas_servicios.py crmgrupobasette tarifa-energia plantilla_tarifas_energia.xlsx
```

## ⚠️ Importante

**SIEMPRE especifica la base de datos correcta según la instalación:**

| Instalación      | Base de Datos       |
|------------------|---------------------|
| Enerfone         | `enerfone_pre`      |
| Grupo Basette    | `crmgrupobasette`   |
| Demo Enerfone    | `demoenerfone`      |

## 📦 Archivos Modificados

1. ✅ `importar_tarifas_servicios.py` - Script principal
2. ✅ `publicado/importar_tarifas_servicios.py` - Copia en carpeta de publicación
3. ✅ `IMPORTACION_TARIFAS_SERVICIOS.md` - Documentación actualizada
4. ✅ `README_IMPORTACION.md` - Documentación actualizada

## 🔍 Cómo Prevenir Este Error en el Futuro

1. **Siempre verifica** el nombre de la base de datos antes de ejecutar importaciones
2. **Comprueba** que las tarifas se hayan importado en la BD correcta después de la importación:
   ```sql
   -- Verificar última importación
   SELECT COUNT(*) FROM tarifasenergia WHERE FechaCreacion >= CURDATE();
   ```
3. Si importaste por error a la BD incorrecta, elimina los registros antes de reimportar:
   ```sql
   -- Eliminar tarifas importadas por error
   DELETE FROM tarifasenergia WHERE FechaCreacion >= CURDATE();
   ```

## 📅 Fecha de Corrección

9 de marzo de 2026
