# 🚀 INSTRUCCIONES DE DESPLIEGUE - CorCRM

**Fecha**: 3 de junio de 2026  
**Versión**: 20260603  
**Estado**: ✅ **LISTO PARA PRODUCCIÓN**

---

## 📦 CARPETAS PUBLICADAS

### ✅ **ENERFONE CRM**
- **Carpeta local**: `./publicado` (442 MB)
- **Servidor destino**: `C:\enerfonecrm`
- **URL**: https://enerfonecrm.com
- **Base de datos**: `enerfone_pre`
- **Configuración**: `appsettings.Production.Enerfone.json`

### ✅ **GRUPO BASETTE CRM**
- **Carpeta local**: `./publicadogrupobasette` (118 MB)
- **Servidor destino**: `C:\grupobasettecrm`
- **URL**: https://crm.grupobasette.eu
- **Base de datos**: `crmgrupobasette`
- **Configuración**: `appsettings.Production.GrupoBasette.json`

---

## 🔧 CAMBIOS INCLUIDOS EN ESTA VERSIÓN

1. ✅ **Versión actualizada**: 20260603 (visible en el footer)
2. ✅ **Logs de stdout DESACTIVADOS**: `stdoutLogEnabled="false"` en ambos `web.config`
3. ✅ **ID de usuario en modal**: Badge con ID visible en ModalEditarUsuario
4. ✅ **Compilación limpia**: Sin archivos de código fuente, solo archivos publicados

---

## 📋 PASOS PARA DESPLEGAR EN SERVIDOR WINDOWS

### **1. PREPARACIÓN**

Antes de copiar archivos, **haz backup** de las carpetas actuales:

```powershell
# En el servidor Windows
$fecha = Get-Date -Format "yyyyMMdd_HHmmss"

# Backup Enerfone
Copy-Item C:\enerfonecrm C:\backups\enerfonecrm_$fecha -Recurse

# Backup Grupo Basette
Copy-Item C:\grupobasettecrm C:\backups\grupobasettecrm_$fecha -Recurse
```

---

### **2. DESPLIEGUE ENERFONE**

#### **En IIS Manager:**

1. **Detener el sitio**:
   ```powershell
   Stop-IISSite -Name "enerfonecrm"
   ```

2. **Verificar que el proceso se haya detenido**:
   ```powershell
   Get-Process -Name "dotnet" -ErrorAction SilentlyContinue
   ```
   Si hay procesos activos, espera unos segundos.

#### **Copiar archivos:**

```powershell
# Opción A: Con robocopy (RECOMENDADO - preserva uploads y storage)
robocopy "\\TU-MAC\publicado" "C:\enerfonecrm" /MIR /XD "wwwroot\uploads" "storage" /R:3 /W:5

# Opción B: Manual (cuidado con sobrescribir wwwroot\uploads y storage)
# - Copia todo el contenido de ./publicado a C:\enerfonecrm
# - NO sobrescribas: C:\enerfonecrm\wwwroot\uploads
# - NO sobrescribas: C:\enerfonecrm\storage
```

#### **Verificar archivos críticos:**

```powershell
# Comprobar que existan los archivos principales
Test-Path C:\enerfonecrm\EnerfoneCRM.dll
Test-Path C:\enerfonecrm\appsettings.Production.json
Test-Path C:\enerfonecrm\web.config
Test-Path C:\enerfonecrm\wwwroot

# Verificar que stdoutLogEnabled="false"
Select-String -Path "C:\enerfonecrm\web.config" -Pattern "stdoutLogEnabled"
```

Debe mostrar: `stdoutLogEnabled="false"`

#### **Iniciar el sitio:**

```powershell
Start-IISSite -Name "enerfonecrm"
```

#### **Verificar:**

- Accede a https://enerfonecrm.com
- Comprueba el footer: debe mostrar "Versión 20260615"
- Prueba login
- Verifica que las funcionalidades básicas funcionen

---

### **3. DESPLIEGUE GRUPO BASETTE**

#### **En IIS Manager:**

1. **Detener el sitio**:
   ```powershell
   Stop-IISSite -Name "grupobasettecrm"
   ```

2. **Verificar que el proceso se haya detenido**:
   ```powershell
   Get-Process -Name "dotnet" -ErrorAction SilentlyContinue
   ```

#### **Copiar archivos:**

```powershell
# Opción A: Con robocopy (RECOMENDADO)
robocopy "\\TU-MAC\publicadogrupobasette" "C:\grupobasettecrm" /MIR /XD "wwwroot\uploads" "storage" /R:3 /W:5

# Opción B: Manual (cuidado con sobrescribir uploads y storage)
# - Copia todo el contenido de ./publicadogrupobasette a C:\grupobasettecrm
# - NO sobrescribas: C:\grupobasettecrm\wwwroot\uploads
# - NO sobrescribas: C:\grupobasettecrm\storage
```

#### **Verificar archivos críticos:**

```powershell
# Comprobar que existan los archivos principales
Test-Path C:\grupobasettecrm\EnerfoneCRM.dll
Test-Path C:\grupobasettecrm\appsettings.Production.json
Test-Path C:\grupobasettecrm\web.config
Test-Path C:\grupobasettecrm\wwwroot

# Verificar que stdoutLogEnabled="false"
Select-String -Path "C:\grupobasettecrm\web.config" -Pattern "stdoutLogEnabled"
```

Debe mostrar: `stdoutLogEnabled="false"`

#### **Iniciar el sitio:**

```powershell
Start-IISSite -Name "grupobasettecrm"
```

#### **Verificar:**

- Accede a https://crm.grupobasette.eu
- Comprueba el footer: debe mostrar "Versión 20260615"
- Prueba login
- Verifica que las funcionalidades básicas funcionen

---

## 🧹 LIMPIEZA DE LOGS (IMPORTANTE)

Después de desplegar y verificar que todo funciona, **limpia los logs antiguos**:

### **Enerfone:**

```powershell
# Ver tamaño antes de limpiar
Get-ChildItem C:\enerfonecrm\logs -Recurse | Measure-Object -Property Length -Sum | Select-Object @{Name="Size(MB)";Expression={[math]::Round($_.Sum/1MB,2)}}

# Eliminar todos los logs
Remove-Item C:\enerfonecrm\logs\* -Recurse -Force

# Crear carpeta vacía si se eliminó
New-Item -Path "C:\enerfonecrm\logs" -ItemType Directory -Force
```

### **Grupo Basette:**

```powershell
# Ver tamaño antes de limpiar
Get-ChildItem C:\grupobasettecrm\logs -Recurse | Measure-Object -Property Length -Sum | Select-Object @{Name="Size(MB)";Expression={[math]::Round($_.Sum/1MB,2)}}

# Eliminar todos los logs
Remove-Item C:\grupobasettecrm\logs\* -Recurse -Force

# Crear carpeta vacía si se eliminó
New-Item -Path "C:\grupobasettecrm\logs" -ItemType Directory -Force
```

---

## ✅ VERIFICACIÓN POST-DESPLIEGUE

### **1. Comprobar versión:**

- Abre cada aplicación en el navegador
- Scroll al footer
- **Debe mostrar**: "Versión 20260615"

### **2. Verificar que los logs NO crezcan:**

```powershell
# Después de unos días, ejecuta:
Get-ChildItem C:\enerfonecrm\logs | Sort-Object LastWriteTime -Descending | Select-Object -First 5
Get-ChildItem C:\grupobasettecrm\logs | Sort-Object LastWriteTime -Descending | Select-Object -First 5
```

**Si NO hay archivos nuevos** (posteriores al despliegue), ✅ **la solución de logs funciona correctamente**.

### **3. Pruebas funcionales:**

- ✅ Login con usuario existente
- ✅ Ver listado de clientes
- ✅ Abrir ficha de usuario (debe mostrar ID en el modal)
- ✅ Ver contratos
- ✅ Generar un PDF
- ✅ Consultar liquidaciones

---

## 🔴 SOLUCIÓN DE PROBLEMAS

### **Error: "La aplicación no arranca"**

```powershell
# Ver logs de eventos de Windows
Get-EventLog -LogName Application -Source "IIS AspNetCore Module*" -Newest 10

# Verificar permisos
icacls C:\enerfonecrm /grant "IIS AppPool\enerfonecrm:(OI)(CI)F" /T
icacls C:\grupobasettecrm /grant "IIS AppPool\grupobasettecrm:(OI)(CI)F" /T
```

### **Error: "Could not find a part of the path"**

```powershell
# Verificar que existan las carpetas necesarias
New-Item -Path "C:\enerfonecrm\wwwroot\uploads" -ItemType Directory -Force
New-Item -Path "C:\enerfonecrm\storage" -ItemType Directory -Force
New-Item -Path "C:\enerfonecrm\logs" -ItemType Directory -Force

New-Item -Path "C:\grupobasettecrm\wwwroot\uploads" -ItemType Directory -Force
New-Item -Path "C:\grupobasettecrm\storage" -ItemType Directory -Force
New-Item -Path "C:\grupobasettecrm\logs" -ItemType Directory -Force
```

### **Error: "Database connection failed"**

```powershell
# Verificar que el appsettings.Production.json tenga la cadena de conexión correcta
notepad C:\enerfonecrm\appsettings.Production.json
notepad C:\grupobasettecrm\appsettings.Production.json

# Verificar conectividad con MySQL
Test-NetConnection -ComputerName [IP_MYSQL] -Port 3306
```

---

## 📊 MONITOREO CONTINUO

### **Script de monitoreo de logs:**

Crea un archivo `C:\scripts\monitorear_logs_crm.ps1`:

```powershell
$fecha = Get-Date -Format "yyyy-MM-dd HH:mm:ss"

Write-Host "=== MONITOREO LOGS CRM - $fecha ===" -ForegroundColor Cyan
Write-Host ""

# Enerfone
$sizeEnerfone = (Get-ChildItem C:\enerfonecrm\logs -Recurse -ErrorAction SilentlyContinue | Measure-Object -Property Length -Sum).Sum / 1MB
Write-Host "Enerfone logs: $([math]::Round($sizeEnerfone, 2)) MB" -ForegroundColor $(if ($sizeEnerfone -gt 100) {"Red"} else {"Green"})

# Grupo Basette
$sizeGrupoBasette = (Get-ChildItem C:\grupobasettecrm\logs -Recurse -ErrorAction SilentlyContinue | Measure-Object -Property Length -Sum).Sum / 1MB
Write-Host "Grupo Basette logs: $([math]::Round($sizeGrupoBasette, 2)) MB" -ForegroundColor $(if ($sizeGrupoBasette -gt 100) {"Red"} else {"Green"})

Write-Host ""

if ($sizeEnerfone -gt 100 -or $sizeGrupoBasette -gt 100) {
    Write-Host "⚠️ ADVERTENCIA: Logs exceden 100 MB" -ForegroundColor Yellow
    Write-Host "Verifica que stdoutLogEnabled='false' en web.config" -ForegroundColor Yellow
}
```

Ejecuta periódicamente:

```powershell
C:\scripts\monitorear_logs_crm.ps1
```

---

## 📝 NOTAS IMPORTANTES

### ⚠️ **PRESERVAR DATOS DEL USUARIO**

**NUNCA** sobrescribir estas carpetas al desplegar:
- `wwwroot\uploads` - Documentos subidos por usuarios
- `storage` - Almacenamiento persistente

### ⚠️ **RESPALDO DE BASE DE DATOS**

Antes de desplegar, **siempre** haz backup de la base de datos:

```sql
-- MySQL
mysqldump -u root -p enerfone_pre > backup_enerfone_pre_20260603.sql
mysqldump -u root -p crmgrupobasette > backup_crmgrupobasette_20260603.sql
```

### ⚠️ **REINICIO LIMPIO DE IIS**

Si algo no funciona después del despliegue:

```powershell
# Reinicio completo de IIS
iisreset /stop
Start-Sleep -Seconds 5
iisreset /start
```

---

## 📞 CONTACTO Y SOPORTE

Si encuentras algún problema durante el despliegue, revisa:

1. **Logs de IIS**: `C:\inetpub\logs\LogFiles\`
2. **Event Viewer**: Windows Logs → Application
3. **Logs de la aplicación** (si `stdoutLogEnabled="true"` temporalmente para debugging)

---

## ✅ CHECKLIST DE DESPLIEGUE

### **Antes de empezar:**
- [ ] Backup de carpetas actuales
- [ ] Backup de bases de datos
- [ ] Verificar acceso RDP al servidor
- [ ] Verificar permisos de administrador

### **Durante el despliegue:**
- [ ] Detener sitios en IIS
- [ ] Copiar archivos (preservando uploads y storage)
- [ ] Verificar web.config con `stdoutLogEnabled="false"`
- [ ] Iniciar sitios en IIS

### **Después del despliegue:**
- [ ] Verificar versión en footer (20260603)
- [ ] Probar login
- [ ] Probar funcionalidades básicas
- [ ] Limpiar logs antiguos
- [ ] Monitorear logs durante 2-3 días

---

**Fecha de creación**: 3 de junio de 2026  
**Última actualización**: 3 de junio de 2026  
**Próxima revisión**: Después del despliegue
