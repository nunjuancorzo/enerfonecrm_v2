# Solución para Carpetas Duplicadas en Repositorio

## Problema
Las carpetas "Energía", "Telefonía" y "Alarmas" podían duplicarse en producción debido a diferentes codificaciones Unicode de los caracteres acentuados (í). Esto ocurre cuando:
- Un sistema guarda "Energía" con forma NFC (composed: í como un carácter)
- Otro sistema guarda "Energía" con forma NFD (decomposed: i + tilde como dos caracteres)

El sistema de archivos ve estos como nombres diferentes, causando carpetas duplicadas.

## Solución Implementada

### 1. Normalización Unicode en RepositorioService.cs
Se ha implementado normalización Unicode (FormC) en todos los métodos que crean o manipulan carpetas:
- `SanitizarNombre()`: Normaliza nombres de comercializadoras/operadoras
- `NormalizarNombreCarpetaBase()`: Normaliza nombres de carpetas base (Energía, Telefonía, Alarmas)
- Todos los métodos de creación de carpetas ahora usan normalización consistente

### 2. Normalización en Repositorio.razor
Se normalizan las comparaciones de nombres de carpetas para asegurar que las comparaciones sean consistentes independientemente de la codificación.

## Limpieza de Carpetas Duplicadas en Producción

Si ya tienes carpetas duplicadas en producción, sigue estos pasos:

### Paso 1: Identificar carpetas duplicadas
```bash
cd storage/repositorio
ls -la
```

Verás algo como:
```
Energía/          <- Versión NFC
Energía/          <- Versión NFD (se ve igual pero es diferente)
```

### Paso 2: Verificar contenido de cada carpeta
```bash
# En Linux/macOS, usa comillas para manejar caracteres especiales
ls -la "Energía"
ls -la $'Energi\u0301a'  # Forma NFD
```

### Paso 3: Consolidar carpetas (CUIDADO: Hacer backup primero)

**IMPORTANTE: Detén la aplicación antes de hacer cambios manuales**

```bash
# 1. Hacer backup completo
cd storage
tar -czf backup_repositorio_$(date +%Y%m%d_%H%M%S).tar.gz repositorio/

# 2. Identificar versiones de la carpeta
cd repositorio

# 3. Para cada carpeta duplicada (Energía, Telefonía, Alarmas)
# Primero, crear una carpeta temporal con nombre sin tildes
mkdir -p Energia_temp

# 4. Mover contenido de todas las versiones a la temporal
# Esto combinará el contenido de ambas carpetas
find . -maxdepth 1 -name "*nerg*" -type d -exec cp -r {}/* Energia_temp/ \;

# 5. Eliminar las carpetas duplicadas
find . -maxdepth 1 -name "*nerg*" -type d -exec rm -rf {} \;

# 6. Crear la carpeta con nombre normalizado usando Python
python3 << 'EOF'
import os
import shutil
nombre_normalizado = "Energía".encode('utf-8').decode('utf-8')
os.mkdir(nombre_normalizado)
# Mover contenido de temporal a la normalizada
for item in os.listdir('Energia_temp'):
    shutil.move(os.path.join('Energia_temp', item), nombre_normalizado)
os.rmdir('Energia_temp')
EOF

# 7. Repetir para Telefonía y Alarmas si están duplicadas
```

### Paso 4: Método alternativo más seguro (recomendado)

Si el método anterior es muy complejo, usa este script de migración:

```bash
#!/bin/bash
# Script: consolidar_carpetas.sh

REPO_PATH="/ruta/a/storage/repositorio"
cd "$REPO_PATH"

# Función para consolidar una carpeta
consolidar_carpeta() {
    local NOMBRE_BASE=$1
    local TEMP_DIR="${NOMBRE_BASE}_consolidado_temp"
    
    echo "Consolidando $NOMBRE_BASE..."
    
    # Crear carpeta temporal
    mkdir -p "$TEMP_DIR"
    
    # Buscar todas las variantes y copiar contenido
    find . -maxdepth 1 -type d -iname "*${NOMBRE_BASE}*" | while read -r dir; do
        if [ "$dir" != "./$TEMP_DIR" ]; then
            echo "  Copiando desde: $dir"
            cp -r "$dir"/* "$TEMP_DIR/" 2>/dev/null || true
        fi
    done
    
    # Eliminar carpetas originales
    find . -maxdepth 1 -type d -iname "*${NOMBRE_BASE}*" -not -name "$TEMP_DIR" -exec rm -rf {} \; 2>/dev/null || true
    
    # Renombrar temporal al nombre final normalizado
    # Usar Python para asegurar normalización correcta
    python3 -c "
import os
import shutil
nombre = '$NOMBRE_BASE'.encode('utf-8').decode('utf-8')
if os.path.exists('$TEMP_DIR'):
    shutil.move('$TEMP_DIR', nombre)
print(f'Consolidado: {nombre}')
"
}

# Hacer backup
echo "Creando backup..."
cd ..
tar -czf "backup_repositorio_$(date +%Y%m%d_%H%M%S).tar.gz" repositorio/
cd repositorio/

# Consolidar cada carpeta
consolidar_carpeta "Energia"    # Sin tilde temporalmente
consolidar_carpeta "Telefonia"  # Sin tilde temporalmente  
consolidar_carpeta "Alarmas"

echo "Consolidación completa. Verifica el resultado antes de reiniciar la app."
```

### Paso 5: Verificación después de la consolidación

```bash
# Verificar estructura
ls -la

# Debe mostrar solo una versión de cada carpeta:
# Energía/
# Telefonía/
# Alarmas/

# Verificar que el contenido está completo
ls -la Energía/
ls -la Telefonía/
ls -la Alarmas/
```

### Paso 6: Reiniciar la aplicación

Una vez consolidadas las carpetas, reinicia la aplicación. El nuevo código con normalización Unicode prevendrá futuras duplicaciones.

```bash
systemctl restart enerfonecrm  # O el método que uses para reiniciar
```

## Prevención

Con los cambios implementados:
1. **RepositorioService.cs** normaliza todos los nombres a Unicode NFC
2. **Repositorio.razor** usa comparaciones normalizadas
3. Las nuevas carpetas se crearán siempre con la misma codificación
4. No se crearan más duplicados

## Notas importantes

- **Siempre haz backup antes de manipular carpetas**
- **Detén la aplicación durante la consolidación**
- Si tienes muchas comercializadoras, el proceso puede tomar tiempo
- Verifica que no se pierdan archivos comparando el backup con el resultado final

## Verificación de éxito

Después de aplicar los cambios:
1. ✅ Solo existe una carpeta "Energía", "Telefonía" y "Alarmas"
2. ✅ Al crear una nueva comercializadora, no se duplica la carpeta
3. ✅ Los logos se guardan y muestran correctamente
4. ✅ La navegación en el repositorio funciona sin errores

## Soporte

Si encuentras problemas:
1. Restaura el backup
2. Revisa los logs de la aplicación
3. Verifica permisos de carpetas (`chmod -R 755 storage/repositorio`)
4. Verifica ownership (`chown -R usuario:grupo storage/repositorio`)
