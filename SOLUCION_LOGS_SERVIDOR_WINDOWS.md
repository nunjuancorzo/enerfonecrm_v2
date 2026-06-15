# 🛑 SOLUCIÓN: DESACTIVAR LOGS DE STDOUT EN SERVIDOR WINDOWS

**Fecha**: 3 de junio de 2026  
**Problema**: La carpeta `logs` en el servidor Windows está creciendo mucho  
**Causa**: El parámetro `stdoutLogEnabled="true"` en el `web.config` genera archivos de log  
**Estado**: ✅ **SOLUCIONADO**

---

## 📋 ¿QUÉ SE HA HECHO?

### 1. ✅ **Logs de stdout DESACTIVADOS**

Se ha cambiado `stdoutLogEnabled="true"` a `stdoutLogEnabled="false"` en los scripts de publicación:

- ✅ [PUBLICAR_ENERFONE.ps1](PUBLICAR_ENERFONE.ps1) (línea 41)
- ✅ [PUBLICAR_GRUPOBASETTE.ps1](PUBLICAR_GRUPOBASETTE.ps1) (línea 41)

**Antes**:
```xml
<aspNetCore processPath="dotnet" arguments=".\EnerfoneCRM.dll" 
            stdoutLogEnabled="true" 
            stdoutLogFile=".\logs\stdout" 
            hostingModel="InProcess">
```

**Después**:
```xml
<aspNetCore processPath="dotnet" arguments=".\EnerfoneCRM.dll" 
            stdoutLogEnabled="false" 
            stdoutLogFile=".\logs\stdout" 
            hostingModel="InProcess">
```

**Resultado**: A partir de la próxima publicación, **NO se generarán más archivos de log** en la carpeta `logs`.

---

## 🧹 PASO 2: LIMPIAR LOGS EXISTENTES EN EL SERVIDOR

### En el servidor Windows (Enerfone):

```powershell
# Navegar a la carpeta de la aplicación
cd C:\enerfonecrm\logs

# Ver el tamaño de la carpeta
dir | Measure-Object -Property Length -Sum

# Eliminar todos los archivos de log
Remove-Item .\* -Recurse -Force

# O eliminar solo logs antiguos (más de 7 días)
Get-ChildItem -Path . -Recurse | Where-Object {$_.LastWriteTime -lt (Get-Date).AddDays(-7)} | Remove-Item -Force
```

### En el servidor Windows (Grupo Basette):

```powershell
# Navegar a la carpeta de la aplicación
cd C:\grupobasettecrm\logs

# Ver el tamaño de la carpeta
dir | Measure-Object -Property Length -Sum

# Eliminar todos los archivos de log
Remove-Item .\* -Recurse -Force

# O eliminar solo logs antiguos (más de 7 días)
Get-ChildItem -Path . -Recurse | Where-Object {$_.LastWriteTime -lt (Get-Date).AddDays(-7)} | Remove-Item -Force
```

---

## 🚀 APLICAR LA SOLUCIÓN

### 1️⃣ **Publicar la aplicación**

```powershell
# Para Enerfone
.\PUBLICAR_ENERFONE.ps1

# Para Grupo Basette
.\PUBLICAR_GRUPOBASETTE.ps1
```

Esto generará un nuevo `web.config` con `stdoutLogEnabled="false"`.

### 2️⃣ **Copiar archivos al servidor**

Según las instrucciones del script de publicación:

**Enerfone**:
1. Detener el sitio en IIS
2. Copiar `.\publicado\*` a `C:\enerfonecrm\`
3. Iniciar el sitio en IIS

**Grupo Basette**:
1. Detener el sitio en IIS
2. Copiar `.\publicadogrupobasette\*` a `C:\grupobasettecrm\`
3. Iniciar el sitio en IIS

### 3️⃣ **Limpiar logs existentes** (en el servidor)

Una vez actualizado el `web.config`, elimina los logs antiguos:

```powershell
# Enerfone
Remove-Item C:\enerfonecrm\logs\* -Recurse -Force

# Grupo Basette
Remove-Item C:\grupobasettecrm\logs\* -Recurse -Force
```

### 4️⃣ **Verificar**

Después de unos días, comprueba que NO se están creando nuevos archivos:

```powershell
# Ver últimos archivos modificados en la carpeta logs
Get-ChildItem C:\enerfonecrm\logs | Sort-Object LastWriteTime -Descending | Select-Object -First 5
```

Si no hay archivos recientes (posteriores a la actualización), la solución funciona correctamente.

---

## 📊 IMPACTO ESPERADO

| Antes | Después |
|-------|---------|
| ❌ Crecimiento continuo de logs | ✅ Sin generación de logs |
| ❌ Carpeta `logs` con GB de archivos | ✅ Carpeta vacía o sin crecimiento |
| ❌ Logs de cada request/error | ✅ Solo logs en consola de IIS |
| ❌ Disco lleno en el servidor | ✅ Espacio liberado |

**Ejemplo**: Si la carpeta `logs` ocupaba 5 GB:
- Después de limpiar: **5 GB recuperados**
- Sin crecimiento futuro

---

## 🔄 ¿CÓMO REACTIVAR LOS LOGS EN EL FUTURO?

Si necesitas reactivar los logs para debugging (temporalmente):

### Opción 1: Editar manualmente el web.config en el servidor

```xml
<!-- Cambiar false a true -->
<aspNetCore processPath="dotnet" arguments=".\EnerfoneCRM.dll" 
            stdoutLogEnabled="true" 
            stdoutLogFile=".\logs\stdout" 
            hostingModel="InProcess">
```

Guarda el archivo y IIS detectará el cambio automáticamente.

### Opción 2: Modificar el script de publicación

En `PUBLICAR_ENERFONE.ps1` o `PUBLICAR_GRUPOBASETTE.ps1`, cambia:

```powershell
# Línea 41 (aproximadamente)
stdoutLogEnabled="true"  # Cambiar a true
```

Luego vuelve a publicar.

> ⚠️ **IMPORTANTE**: Recuerda desactivarlo después del debugging para evitar que vuelvan a crecer los logs.

---

## 📝 ¿QUÉ SON ESTOS LOGS?

Los logs de `stdout` en IIS/ASP.NET Core capturan:
- Toda la salida de `Console.WriteLine()`
- Errores no capturados
- Información de inicio de la aplicación
- Stacktraces de excepciones

**Por qué crecen tanto**:
- Cada request puede generar múltiples líneas de log
- Los archivos se rotan pero se acumulan en la carpeta
- Aplicaciones con mucho tráfico = muchos logs

**Por qué desactivarlos**:
- En producción, los errores deberían manejarse correctamente
- Los `Console.WriteLine()` no deberían usarse en producción
- IIS ya tiene sus propios logs (Event Viewer, IIS logs)
- Ocupan espacio innecesario

---

## 🎯 ALTERNATIVA: LOGGING CONFIGURADO (AVANZADO)

Si quieres un sistema de logging controlado en lugar de stdout:

### Opción A: Usar Serilog con rotación de archivos

```bash
# Instalar paquetes
dotnet add package Serilog.AspNetCore
dotnet add package Serilog.Sinks.File
```

En `Program.cs`:
```csharp
using Serilog;

Log.Logger = new LoggerConfiguration()
    .WriteTo.File("logs/app-.log", 
        rollingInterval: RollingInterval.Day,
        retainedFileCountLimit: 7)  // Solo los últimos 7 días
    .CreateLogger();

builder.Host.UseSerilog();
```

### Opción B: Configurar Azure Application Insights

Para logging en la nube sin ocupar espacio local.

---

## 🛠️ MANTENIMIENTO FUTURO

Si decides reactivar los logs, crea una tarea programada en Windows para limpiarlos:

```powershell
# Crear script de limpieza: C:\scripts\limpiar_logs_crm.ps1
Get-ChildItem -Path "C:\enerfonecrm\logs" -Recurse | 
    Where-Object {$_.LastWriteTime -lt (Get-Date).AddDays(-7)} | 
    Remove-Item -Force

Get-ChildItem -Path "C:\grupobasettecrm\logs" -Recurse | 
    Where-Object {$_.LastWriteTime -lt (Get-Date).AddDays(-7)} | 
    Remove-Item -Force
```

Crear tarea programada:
```powershell
$action = New-ScheduledTaskAction -Execute "PowerShell.exe" -Argument "-File C:\scripts\limpiar_logs_crm.ps1"
$trigger = New-ScheduledTaskTrigger -Daily -At 3am
Register-ScheduledTask -Action $action -Trigger $trigger -TaskName "LimpiarLogsCRM" -Description "Elimina logs antiguos del CRM"
```

---

## ✅ RESUMEN

| Acción | Estado |
|--------|--------|
| Scripts de publicación actualizados | ✅ Hecho |
| `stdoutLogEnabled="false"` configurado | ✅ Hecho |
| Pendiente: Publicar y copiar al servidor | ⏳ **EJECUTAR PUBLICACIÓN** |
| Pendiente: Limpiar carpeta logs en servidor | ⏳ **LIMPIAR MANUALMENTE** |

---

## 📞 COMANDOS ÚTILES

```powershell
# Ver tamaño de carpeta logs
Get-ChildItem -Path C:\enerfonecrm\logs -Recurse | 
    Measure-Object -Property Length -Sum | 
    Select-Object @{Name="Size(MB)";Expression={[math]::Round($_.Sum/1MB,2)}}

# Ver archivos más recientes
Get-ChildItem C:\enerfonecrm\logs | Sort-Object LastWriteTime -Descending | Select-Object -First 10

# Ver archivos más grandes
Get-ChildItem C:\enerfonecrm\logs -Recurse | Sort-Object Length -Descending | Select-Object -First 10

# Contar archivos en la carpeta
(Get-ChildItem C:\enerfonecrm\logs -Recurse).Count
```

---

**Fecha de aplicación**: 3 de junio de 2026  
**Desarrollador**: Sistema CorCRM  
**Impacto**: POSITIVO - Liberación de espacio en disco y eliminación de logs innecesarios
