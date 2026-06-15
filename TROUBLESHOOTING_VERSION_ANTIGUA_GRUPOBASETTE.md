# 🔍 TROUBLESHOOTING: Versión antigua en Grupo Basette

**Fecha**: 3 de junio de 2026  
**Problema**: Servidor muestra versión 20260409 en lugar de 20260603  
**Estado de compilación**: ✅ CORRECTO (ambos DLLs son idénticos)

---

## ✅ VERIFICACIÓN REALIZADA

Ambas publicaciones locales son **IDÉNTICAS**:

| Publicación | MD5 | Fecha compilación | Tamaño |
|-------------|-----|-------------------|--------|
| `publicado` | `88c7e558711d24faa380a0fd14cb04c3` | 2026-06-03 16:37 | 3.0M |
| `publicadogrupobasette` | `88c7e558711d24faa380a0fd14cb04c3` | 2026-06-03 17:05 | 3.0M |

**Conclusión**: El problema NO es la compilación. Ambos DLLs contienen la versión 20260615 correctamente.

---

## 🔴 EL PROBLEMA ESTÁ EN EL SERVIDOR WINDOWS

Si en Enerfone (publicado) funciona correctamente pero en Grupo Basette sigue apareciendo la versión antigua, el problema está en el despliegue o configuración del servidor.

---

## 🛠️ SOLUCIONES (en orden de probabilidad)

### **1. Reiniciar completamente IIS y el Pool de Aplicaciones**

El pool de aplicaciones podría estar usando DLLs antiguos en memoria.

```powershell
# En el servidor Windows, ejecuta:

# Detener el sitio
Stop-IISSite -Name "grupobasettecrm"

# Detener el pool de aplicaciones
Stop-WebAppPool -Name "grupobasettecrm"

# Esperar unos segundos
Start-Sleep -Seconds 5

# Verificar que no hay procesos dotnet activos
Get-Process -Name "dotnet" -ErrorAction SilentlyContinue | Stop-Process -Force

# Iniciar el pool
Start-WebAppPool -Name "grupobasettecrm"

# Iniciar el sitio
Start-IISSite -Name "grupobasettecrm"
```

Después, **limpia la caché del navegador** (Ctrl+Shift+Delete) y recarga la página.

---

### **2. Verificar que los archivos se copiaron correctamente**

Comprueba que el DLL en el servidor tenga la fecha y tamaño correctos:

```powershell
# Ver el archivo DLL en el servidor
Get-ChildItem C:\grupobasettecrm\EnerfoneCRM.dll | Select-Object Name, Length, LastWriteTime

# Ver el MD5 del archivo
Get-FileHash C:\grupobasettecrm\EnerfoneCRM.dll -Algorithm MD5
```

**Debe mostrar**:
- **Tamaño**: 3,110,912 bytes (3.0 MB)
- **Fecha**: 3 de junio de 2026, 16:37 o posterior
- **MD5**: `88c7e558711d24faa380a0fd14cb04c3`

Si el MD5 es diferente o la fecha es antigua, significa que **el archivo no se copió correctamente**.

---

### **3. Eliminar TODOS los archivos antiguos antes de copiar**

Puede haber archivos antiguos que no se sobrescribieron. Haz una limpieza completa:

```powershell
# IMPORTANTE: Haz backup primero
$fecha = Get-Date -Format "yyyyMMdd_HHmmss"
Copy-Item C:\grupobasettecrm C:\backups\grupobasettecrm_$fecha -Recurse

# Detener el sitio
Stop-IISSite -Name "grupobasettecrm"

# Preservar uploads y storage
Move-Item C:\grupobasettecrm\wwwroot\uploads C:\temp\uploads_grupobasette -Force
Move-Item C:\grupobasettecrm\storage C:\temp\storage_grupobasette -Force

# Eliminar TODO el contenido (excepto uploads y storage)
Remove-Item C:\grupobasettecrm\* -Recurse -Force

# Copiar los archivos nuevos desde tu Mac
# (Copia desde ./publicadogrupobasette a C:\grupobasettecrm)

# Restaurar uploads y storage
Move-Item C:\temp\uploads_grupobasette C:\grupobasettecrm\wwwroot\uploads -Force
Move-Item C:\temp\storage_grupobasette C:\grupobasettecrm\storage -Force

# Iniciar el sitio
Start-IISSite -Name "grupobasettecrm"
```

---

### **4. Verificar el directorio físico en IIS**

Asegúrate de que IIS está apuntando al directorio correcto:

```powershell
# Ver la configuración del sitio
Get-IISSite -Name "grupobasettecrm" | Select-Object Name, State, PhysicalPath

# Ver la configuración del pool
Get-IISAppPool -Name "grupobasettecrm" | Select-Object Name, State
```

**Debe mostrar**:
- **PhysicalPath**: `C:\grupobasettecrm` (o el path correcto)
- **State**: `Started`

Si el path es diferente, copia los archivos al directorio correcto.

---

### **5. Limpiar caché de Blazor en el navegador**

Blazor Server usa caché agresivo. Prueba:

1. **Cerrar completamente el navegador**
2. **Abrir el navegador de incógnito / privado**
3. **Acceder a** https://crm.grupobasette.eu
4. **Verificar la versión en el footer**

Si en modo incógnito aparece la versión correcta (20260603), el problema es caché del navegador.

---

### **6. Reiniciar completamente IIS (última opción)**

```powershell
# Reinicio completo de IIS
iisreset /stop
Start-Sleep -Seconds 10
iisreset /start
```

---

## 📋 PROCEDIMIENTO RECOMENDADO

Sigue estos pasos en orden:

### **Paso 1: Verificar el DLL en el servidor**

```powershell
Get-FileHash C:\grupobasettecrm\EnerfoneCRM.dll -Algorithm MD5
```

¿El MD5 es `88c7e558711d24faa380a0fd14cb04c3`?

- ✅ **SÍ** → Ve al Paso 2 (el archivo está correcto)
- ❌ **NO** → Los archivos NO se copiaron correctamente. Cópialos de nuevo.

### **Paso 2: Reiniciar IIS completamente**

```powershell
Stop-IISSite -Name "grupobasettecrm"
Stop-WebAppPool -Name "grupobasettecrm"
Get-Process -Name "dotnet" -ErrorAction SilentlyContinue | Stop-Process -Force
Start-Sleep -Seconds 5
Start-WebAppPool -Name "grupobasettecrm"
Start-IISSite -Name "grupobasettecrm"
```

### **Paso 3: Limpiar caché del navegador**

1. Presiona **Ctrl+Shift+Delete**
2. Selecciona **"Imágenes y archivos en caché"**
3. Selecciona **"Todo"** en el rango de tiempo
4. Haz clic en **"Borrar datos"**

### **Paso 4: Verificar en modo incógnito**

1. Abre una ventana de incógnito
2. Accede a https://crm.grupobasette.eu
3. Verifica el footer

¿Aparece "Versión 20260615"?

- ✅ **SÍ** → El problema era caché. Ya está resuelto.
- ❌ **NO** → Continúa al Paso 5.

### **Paso 5: Verificar el directorio físico**

```powershell
Get-IISSite -Name "grupobasettecrm" | Select-Object Name, PhysicalPath
```

¿El PhysicalPath es correcto?

- ✅ **SÍ** → Continúa al Paso 6
- ❌ **NO** → Copia los archivos al directorio correcto

### **Paso 6: Limpieza completa del directorio**

Si nada anterior funciona, haz una limpieza completa (ver sección 3).

---

## 🔍 DIAGNÓSTICO AVANZADO

Si después de todos los pasos anteriores sigue sin funcionar:

### **Verificar logs de IIS**

```powershell
# Ver últimos errores en Event Viewer
Get-EventLog -LogName Application -Source "IIS*" -Newest 20 | Format-Table TimeGenerated, Message -Wrap
```

### **Verificar que el web.config se aplicó**

```powershell
# Ver el contenido del web.config
Get-Content C:\grupobasettecrm\web.config
```

Verifica que contenga:
- `<aspNetCore processPath="dotnet" arguments=".\EnerfoneCRM.dll"`
- `stdoutLogEnabled="false"`
- `<environmentVariable name="ASPNETCORE_ENVIRONMENT" value="Production" />`

### **Verificar permisos**

```powershell
# Dar permisos completos al pool de aplicaciones
icacls C:\grupobasettecrm /grant "IIS AppPool\grupobasettecrm:(OI)(CI)F" /T
```

---

## ✅ VERIFICACIÓN FINAL

Después de aplicar la solución, verifica:

1. **URL**: https://crm.grupobasette.eu
2. **Footer**: Debe mostrar "**Versión 20260615**"
3. **ID de usuario**: Debe aparecer en el modal de editar usuario
4. **Logs**: La carpeta `C:\grupobasettecrm\logs` NO debe crecer

---

## 📞 SI NADA FUNCIONA

Si después de seguir todos los pasos anteriores sigue apareciendo la versión antigua:

1. **Toma una captura de pantalla** del footer con la versión
2. **Ejecuta estos comandos** y guarda el resultado:

```powershell
Get-FileHash C:\grupobasettecrm\EnerfoneCRM.dll -Algorithm MD5
Get-IISSite -Name "grupobasettecrm" | Select-Object Name, State, PhysicalPath
Get-WebAppPool -Name "grupobasettecrm" | Select-Object Name, State
Get-Content C:\grupobasettecrm\web.config
```

3. **Compara el MD5** con el esperado: `88c7e558711d24faa380a0fd14cb04c3`

Si el MD5 es diferente, significa que los archivos en el servidor NO son los correctos.

---

## 💡 RECOMENDACIÓN

Para futuros despliegues, usa este procedimiento para evitar problemas:

```powershell
# Script de despliegue seguro para Windows Server
$fecha = Get-Date -Format "yyyyMMdd_HHmmss"

# 1. Backup
Copy-Item C:\grupobasettecrm C:\backups\grupobasettecrm_$fecha -Recurse

# 2. Detener todo
Stop-IISSite -Name "grupobasettecrm"
Stop-WebAppPool -Name "grupobasettecrm"
Get-Process -Name "dotnet" | Where-Object {$_.Path -like "*grupobasette*"} | Stop-Process -Force

# 3. Preservar datos
Move-Item C:\grupobasettecrm\wwwroot\uploads C:\temp\uploads_gb -Force -ErrorAction SilentlyContinue
Move-Item C:\grupobasettecrm\storage C:\temp\storage_gb -Force -ErrorAction SilentlyContinue

# 4. Limpiar completamente
Remove-Item C:\grupobasettecrm\* -Recurse -Force -Exclude "web.config"

# 5. Copiar archivos nuevos
# (Aquí copias desde tu origen)

# 6. Restaurar datos
Move-Item C:\temp\uploads_gb C:\grupobasettecrm\wwwroot\uploads -Force -ErrorAction SilentlyContinue
Move-Item C:\temp\storage_gb C:\grupobasettecrm\storage -Force -ErrorAction SilentlyContinue

# 7. Verificar archivo principal
$hash = (Get-FileHash C:\grupobasettecrm\EnerfoneCRM.dll -Algorithm MD5).Hash
if ($hash -eq "88C7E558711D24FAA380A0FD14CB04C3") {
    Write-Host "✅ DLL verificado correctamente" -ForegroundColor Green
} else {
    Write-Host "❌ ERROR: DLL incorrecto. MD5: $hash" -ForegroundColor Red
    exit 1
}

# 8. Iniciar todo
Start-WebAppPool -Name "grupobasettecrm"
Start-IISSite -Name "grupobasettecrm"

# 9. Limpiar logs
Remove-Item C:\grupobasettecrm\logs\* -Recurse -Force

Write-Host "✅ Despliegue completado. Verifica en https://crm.grupobasette.eu" -ForegroundColor Green
```

---

**Fecha de creación**: 3 de junio de 2026  
**Última actualización**: 3 de junio de 2026  
**Versión esperada**: 20260603  
**MD5 esperado**: 88c7e558711d24faa380a0fd14cb04c3
