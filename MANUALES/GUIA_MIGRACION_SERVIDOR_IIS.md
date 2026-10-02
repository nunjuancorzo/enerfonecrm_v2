# 🔄 Guía de Migración entre Servidores Windows/IIS

## 📋 Migración de enerfonecrm.com y crm.grupobasette.eu

### 🎯 Objetivo
Migrar las aplicaciones EnerfoneCRM y Grupo Basette desde un servidor Windows/IIS a otro servidor Windows/IIS nuevo.

---

## 📦 PARTE 1: EXPORTAR DEL SERVIDOR ANTIGUO

### 1.1. Exportar Aplicaciones Publicadas

#### Para Enerfone (enerfonecrm.com):

**Ubicación típica en servidor:** `C:\inetpub\wwwroot\enerfonecrm` o similar

```powershell
# Conectarse al servidor antiguo por RDP
# Abrir PowerShell como Administrador

# Crear carpeta temporal para exports
mkdir C:\Temp\Migracion
mkdir C:\Temp\Migracion\Enerfone
mkdir C:\Temp\Migracion\GrupoBasette

# Copiar archivos de Enerfone
Copy-Item "C:\inetpub\wwwroot\enerfonecrm\*" -Destination "C:\Temp\Migracion\Enerfone" -Recurse

# Comprimir para facilitar transferencia
Compress-Archive -Path "C:\Temp\Migracion\Enerfone" -DestinationPath "C:\Temp\Migracion\Enerfone.zip"
```

#### Para Grupo Basette (crm.grupobasette.eu):

```powershell
# Copiar archivos de Grupo Basette
Copy-Item "C:\inetpub\wwwroot\grupobasette\*" -Destination "C:\Temp\Migracion\GrupoBasette" -Recurse

# Comprimir
Compress-Archive -Path "C:\Temp\Migracion\GrupoBasette" -DestinationPath "C:\Temp\Migracion\GrupoBasette.zip"
```

### 1.2. Exportar Configuración de IIS

```powershell
# Exportar configuración del sitio Enerfone
Export-IISConfiguration -PhysicalPath "C:\Temp\Migracion\IIS_Config_Enerfone" -SiteName "enerfonecrm.com"

# Exportar configuración del sitio Grupo Basette
Export-IISConfiguration -PhysicalPath "C:\Temp\Migracion\IIS_Config_GrupoBasette" -SiteName "crm.grupobasette.eu"
```

**Alternativa manual desde IIS Manager:**
1. Abrir **IIS Manager** (Administrador de IIS)
2. Seleccionar cada sitio web
3. Anotar:
   - Puerto HTTP/HTTPS
   - Bindings (hostnames)
   - Application Pool usado
   - Versión de .NET Runtime
   - Certificados SSL configurados

### 1.3. Exportar Bases de Datos MySQL

```bash
# Conectarse por SSH o usar MySQL Workbench

# Exportar base de datos Enerfone
mysqldump -u enerfone -p enerfonecrm > enerfonecrm_backup_$(date +%Y%m%d).sql

# Exportar base de datos Grupo Basette
mysqldump -u enerfone -p grupobasettecrm > grupobasettecrm_backup_$(date +%Y%m%d).sql

# Comprimir los backups
gzip enerfonecrm_backup_*.sql
gzip grupobasettecrm_backup_*.sql
```

### 1.4. Exportar Archivos de Configuración

**Archivos importantes a copiar:**
- `appsettings.Production.json` (archivo principal de configuración)
- `web.config` (si existe)
- Certificados SSL (si están almacenados localmente)
- Archivos de configuración SMTP
- Cualquier script auxiliar o cron jobs

💡 **Nota:** Si encuentras archivos como `appsettings.Production.Enerfone.json` o similares, **NO los copies**. Usa solo `appsettings.Production.json` para simplificar.

### 1.5. Transferir Archivos al Nuevo Servidor

Opciones para transferir:
- **RDP:** Copiar/pegar entre servidores
- **FTP/SFTP:** Subir a servidor FTP intermedio
- **OneDrive/Dropbox:** Usar almacenamiento cloud
- **Pendrive/Disco externo:** Si tienes acceso físico

---

## 🖥️ PARTE 2: PREPARAR EL SERVIDOR NUEVO

### 2.1. Verificar Requisitos Previos

```powershell
# Abrir PowerShell como Administrador en el servidor nuevo

# Verificar versión de Windows
systeminfo | findstr /B /C:"OS Name" /C:"OS Version"

# Verificar que IIS esté instalado
Get-WindowsFeature -Name Web-Server
```

### 2.2. Instalar .NET 8 Runtime (ASP.NET Core Hosting Bundle)

1. Descargar desde: https://dotnet.microsoft.com/download/dotnet/8.0
2. Buscar **ASP.NET Core Runtime 8.0.x - Windows Hosting Bundle**
3. Instalar el archivo `.exe`
4. **Reiniciar el servidor** después de la instalación

```powershell
# Verificar instalación
dotnet --list-runtimes
```

### 2.3. Configurar IIS Features Necesarios

```powershell
# Habilitar características de IIS necesarias
Enable-WindowsOptionalFeature -Online -FeatureName IIS-WebServerRole
Enable-WindowsOptionalFeature -Online -FeatureName IIS-WebServer
Enable-WindowsOptionalFeature -Online -FeatureName IIS-CommonHttpFeatures
Enable-WindowsOptionalFeature -Online -FeatureName IIS-HttpErrors
Enable-WindowsOptionalFeature -Online -FeatureName IIS-HttpRedirect
Enable-WindowsOptionalFeature -Online -FeatureName IIS-ApplicationDevelopment
Enable-WindowsOptionalFeature -Online -FeatureName IIS-NetFxExtensibility45
Enable-WindowsOptionalFeature -Online -FeatureName IIS-HealthAndDiagnostics
Enable-WindowsOptionalFeature -Online -FeatureName IIS-HttpLogging
Enable-WindowsOptionalFeature -Online -FeatureName IIS-Security
Enable-WindowsOptionalFeature -Online -FeatureName IIS-RequestFiltering
Enable-WindowsOptionalFeature -Online -FeatureName IIS-Performance
Enable-WindowsOptionalFeature -Online -FeatureName IIS-WebServerManagementTools
Enable-WindowsOptionalFeature -Online -FeatureName IIS-StaticContent
Enable-WindowsOptionalFeature -Online -FeatureName IIS-DefaultDocument
Enable-WindowsOptionalFeature -Online -FeatureName IIS-DirectoryBrowsing
Enable-WindowsOptionalFeature -Online -FeatureName IIS-WebSockets
Enable-WindowsOptionalFeature -Online -FeatureName IIS-ApplicationInit
Enable-WindowsOptionalFeature -Online -FeatureName IIS-ASPNET45
```

### 2.4. Configurar MySQL (si no está instalado)

**Opción 1: Migrar MySQL al nuevo servidor**
- Instalar MySQL Server 8.0
- Configurar usuario `enerfone` con las mismas credenciales
- Importar las bases de datos

**Opción 2: Conectar a servidor MySQL externo**
- Verificar conectividad desde el nuevo servidor
- Probar conexión con MySQL Workbench

---

## 📥 PARTE 3: IMPORTAR EN EL SERVIDOR NUEVO

### 3.1. Crear Estructura de Carpetas

```powershell
# Crear carpetas para las aplicaciones
New-Item -Path "C:\inetpub\wwwroot\enerfonecrm" -ItemType Directory -Force
New-Item -Path "C:\inetpub\wwwroot\grupobasette" -ItemType Directory -Force

# Crear carpeta para logs (opcional)
New-Item -Path "C:\Logs\EnerfoneCRM" -ItemType Directory -Force
New-Item -Path "C:\Logs\GrupoBasette" -ItemType Directory -Force
```

### 3.2. Descomprimir Aplicaciones

```powershell
# Copiar los ZIPs al servidor nuevo (por RDP, FTP, etc.)
# Ubicarlos en C:\Temp\Migracion\

# Descomprimir Enerfone
Expand-Archive -Path "C:\Temp\Migracion\Enerfone.zip" -DestinationPath "C:\inetpub\wwwroot\enerfonecrm" -Force

# Descomprimir Grupo Basette
Expand-Archive -Path "C:\Temp\Migracion\GrupoBasette.zip" -DestinationPath "C:\inetpub\wwwroot\grupobasette" -Force
```

### 3.3. Configurar Permisos de Archivos (IMPORTANTE)

IIS necesita permisos específicos para ejecutar las aplicaciones .NET Core. Aquí tienes varias formas de configurarlos:

#### Opción 1: Usando ICACLS (Recomendado)

```powershell
# Abrir PowerShell como Administrador

# ====================
# ENERFONE CRM
# ====================

# Dar permisos completos a IIS_IUSRS (grupo de usuarios de IIS)
icacls "C:\inetpub\wwwroot\enerfonecrm" /grant "IIS_IUSRS:(OI)(CI)F" /T

# Dar permisos completos a IUSR (usuario anónimo de IIS)
icacls "C:\inetpub\wwwroot\enerfonecrm" /grant "IUSR:(OI)(CI)F" /T

# Dar permisos al Application Pool específico (más seguro)
icacls "C:\inetpub\wwwroot\enerfonecrm" /grant "IIS APPPOOL\EnerfoneCRM:(OI)(CI)F" /T

# ====================
# GRUPO BASETTE CRM
# ====================

# Dar permisos completos a IIS_IUSRS
icacls "C:\inetpub\wwwroot\grupobasette" /grant "IIS_IUSRS:(OI)(CI)F" /T

# Dar permisos completos a IUSR
icacls "C:\inetpub\wwwroot\grupobasette" /grant "IUSR:(OI)(CI)F" /T

# Dar permisos al Application Pool específico (más seguro)
icacls "C:\inetpub\wwwroot\grupobasette" /grant "IIS APPPOOL\GrupoBasetteCRM:(OI)(CI)F" /T
```

**Explicación de parámetros:**
- `(OI)` = Object Inherit - Los archivos heredan estos permisos
- `(CI)` = Container Inherit - Las carpetas heredan estos permisos
- `F` = Full Control - Control total
- `/T` = Aplicar recursivamente a subcarpetas y archivos

#### Opción 2: Permisos Específicos por Carpeta

Si quieres ser más granular con los permisos:

```powershell
# Para ENERFONE

# Lectura y ejecución para toda la aplicación
icacls "C:\inetpub\wwwroot\enerfonecrm" /grant "IIS APPPOOL\EnerfoneCRM:(OI)(CI)RX" /T

# Escritura para carpeta de logs
icacls "C:\inetpub\wwwroot\enerfonecrm\logs" /grant "IIS APPPOOL\EnerfoneCRM:(OI)(CI)M" /T

# Escritura para carpeta de uploads (si existe)
icacls "C:\inetpub\wwwroot\enerfonecrm\wwwroot\uploads" /grant "IIS APPPOOL\EnerfoneCRM:(OI)(CI)M" /T

# Para GRUPO BASETTE

# Lectura y ejecución para toda la aplicación
icacls "C:\inetpub\wwwroot\grupobasette" /grant "IIS APPPOOL\GrupoBasetteCRM:(OI)(CI)RX" /T

# Escritura para carpeta de logs
icacls "C:\inetpub\wwwroot\grupobasette\logs" /grant "IIS APPPOOL\GrupoBasetteCRM:(OI)(CI)M" /T

# Escritura para carpeta de uploads (si existe)
icacls "C:\inetpub\wwwroot\grupobasette\wwwroot\uploads" /grant "IIS APPPOOL\GrupoBasetteCRM:(OI)(CI)M" /T
```

**Tipos de permisos:**
- `F` = Full Control (Control Total)
- `M` = Modify (Modificar - leer, escribir, eliminar)
- `RX` = Read & Execute (Leer y Ejecutar)
- `R` = Read (Solo lectura)
- `W` = Write (Solo escritura)

#### Opción 3: Usando PowerShell con ACL (Más verboso pero más control)

```powershell
# Para ENERFONE
$path = "C:\inetpub\wwwroot\enerfonecrm"
$user = "IIS APPPOOL\EnerfoneCRM"

# Obtener ACL actual
$acl = Get-Acl $path

# Crear nueva regla de acceso
$rule = New-Object System.Security.AccessControl.FileSystemAccessRule(
    $user,
    "FullControl",
    "ContainerInherit,ObjectInherit",
    "None",
    "Allow"
)

# Agregar la regla
$acl.SetAccessRule($rule)

# Aplicar ACL
Set-Acl $path $acl

# Aplicar recursivamente a subcarpetas
Get-ChildItem $path -Recurse | ForEach-Object {
    $acl = Get-Acl $_.FullName
    $acl.SetAccessRule($rule)
    Set-Acl $_.FullName $acl
}

Write-Host "Permisos aplicados correctamente para Enerfone" -ForegroundColor Green
```

```powershell
# Para GRUPO BASETTE
$path = "C:\inetpub\wwwroot\grupobasette"
$user = "IIS APPPOOL\GrupoBasetteCRM"

$acl = Get-Acl $path
$rule = New-Object System.Security.AccessControl.FileSystemAccessRule(
    $user,
    "FullControl",
    "ContainerInherit,ObjectInherit",
    "None",
    "Allow"
)
$acl.SetAccessRule($rule)
Set-Acl $path $acl

Get-ChildItem $path -Recurse | ForEach-Object {
    $acl = Get-Acl $_.FullName
    $acl.SetAccessRule($rule)
    Set-Acl $_.FullName $acl
}

Write-Host "Permisos aplicados correctamente para Grupo Basette" -ForegroundColor Green
```

#### Verificar Permisos Actuales

```powershell
# Ver permisos de una carpeta
icacls "C:\inetpub\wwwroot\enerfonecrm"

# Ver permisos de forma más legible con PowerShell
Get-Acl "C:\inetpub\wwwroot\enerfonecrm" | Format-List

# Ver permisos detallados
(Get-Acl "C:\inetpub\wwwroot\enerfonecrm").Access | Format-Table IdentityReference,FileSystemRights,AccessControlType
```

#### Script Completo de Configuración de Permisos

Guarda esto como `ConfigurarPermisos.ps1`:

```powershell
# ConfigurarPermisos.ps1
# Script para configurar permisos de IIS para EnerfoneCRM y GrupoBasetteCRM

# Verificar que se ejecuta como Administrador
if (-NOT ([Security.Principal.WindowsPrincipal][Security.Principal.WindowsIdentity]::GetCurrent()).IsInRole([Security.Principal.WindowsBuiltInRole] "Administrator")) {
    Write-Warning "Debes ejecutar este script como Administrador!"
    Break
}

Write-Host "==================================" -ForegroundColor Cyan
Write-Host "Configurando permisos para IIS..." -ForegroundColor Cyan
Write-Host "==================================" -ForegroundColor Cyan

# Configuración
$paths = @{
    "EnerfoneCRM" = @{
        Path = "C:\inetpub\wwwroot\enerfonecrm"
        AppPool = "IIS APPPOOL\EnerfoneCRM"
    }
    "GrupoBasette" = @{
        Path = "C:\inetpub\wwwroot\grupobasette"
        AppPool = "IIS APPPOOL\GrupoBasetteCRM"
    }
}

foreach ($app in $paths.Keys) {
    $config = $paths[$app]
    $path = $config.Path
    $appPool = $config.AppPool
    
    Write-Host "`nConfigurando: $app" -ForegroundColor Yellow
    Write-Host "Ruta: $path" -ForegroundColor Gray
    Write-Host "AppPool: $appPool" -ForegroundColor Gray
    
    if (Test-Path $path) {
        try {
            # Aplicar permisos con ICACLS
            Write-Host "  Aplicando permisos a IIS_IUSRS..." -NoNewline
            icacls $path /grant "IIS_IUSRS:(OI)(CI)F" /T /Q | Out-Null
            Write-Host " OK" -ForegroundColor Green
            
            Write-Host "  Aplicando permisos a IUSR..." -NoNewline
            icacls $path /grant "IUSR:(OI)(CI)F" /T /Q | Out-Null
            Write-Host " OK" -ForegroundColor Green
            
            Write-Host "  Aplicando permisos al Application Pool..." -NoNewline
            icacls $path /grant "$($appPool):(OI)(CI)F" /T /Q | Out-Null
            Write-Host " OK" -ForegroundColor Green
            
            # Crear carpeta de logs si no existe
            $logsPath = Join-Path $path "logs"
            if (-not (Test-Path $logsPath)) {
                Write-Host "  Creando carpeta de logs..." -NoNewline
                New-Item -Path $logsPath -ItemType Directory -Force | Out-Null
                icacls $logsPath /grant "$($appPool):(OI)(CI)F" /T /Q | Out-Null
                Write-Host " OK" -ForegroundColor Green
            }
            
            Write-Host "  Permisos configurados correctamente!" -ForegroundColor Green
        }
        catch {
            Write-Host " ERROR" -ForegroundColor Red
            Write-Host "  Error: $($_.Exception.Message)" -ForegroundColor Red
        }
    }
    else {
        Write-Host "  ADVERTENCIA: La ruta no existe!" -ForegroundColor Red
    }
}

Write-Host "`n==================================" -ForegroundColor Cyan
Write-Host "Configuración completada!" -ForegroundColor Cyan
Write-Host "==================================" -ForegroundColor Cyan

# Reiniciar IIS para aplicar cambios
Write-Host "`n¿Deseas reiniciar IIS ahora? (S/N): " -NoNewline -ForegroundColor Yellow
$respuesta = Read-Host
if ($respuesta -eq 'S' -or $respuesta -eq 's') {
    Write-Host "Reiniciando IIS..." -ForegroundColor Yellow
    iisreset /restart
    Write-Host "IIS reiniciado correctamente!" -ForegroundColor Green
}
```

**Para ejecutar el script:**

```powershell
# Opción 1: Ejecutar directamente
.\ConfigurarPermisos.ps1

# Opción 2: Si hay error de políticas de ejecución
Set-ExecutionPolicy -ExecutionPolicy RemoteSigned -Scope Process
.\ConfigurarPermisos.ps1
```

#### Solución de Problemas de Permisos

**Error: "Access Denied"**
```powershell
# Tomar propiedad de la carpeta primero
takeown /F "C:\inetpub\wwwroot\enerfonecrm" /R /D Y

# Luego aplicar permisos
icacls "C:\inetpub\wwwroot\enerfonecrm" /grant "Administrators:(OI)(CI)F" /T
icacls "C:\inetpub\wwwroot\enerfonecrm" /grant "IIS_IUSRS:(OI)(CI)F" /T
```

**Resetear permisos a valores predeterminados:**
```powershell
# Resetear herencia de permisos
icacls "C:\inetpub\wwwroot\enerfonecrm" /reset /T
```

**Verificar que el Application Pool existe:**
```powershell
Import-Module WebAdministration
Get-IISAppPool | Where-Object {$_.Name -eq "EnerfoneCRM"}
```

### 3.4. Importar Bases de Datos MySQL

```bash
# Si MySQL está en el mismo servidor
mysql -u root -p

# Crear bases de datos
CREATE DATABASE enerfonecrm CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci;
CREATE DATABASE grupobasettecrm CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci;

# Crear/verificar usuario
CREATE USER IF NOT EXISTS 'enerfone'@'localhost' IDENTIFIED BY 'Salaiet6680.';
GRANT ALL PRIVILEGES ON enerfonecrm.* TO 'enerfone'@'localhost';
GRANT ALL PRIVILEGES ON grupobasettecrm.* TO 'enerfone'@'localhost';
FLUSH PRIVILEGES;
EXIT;

# Importar datos
mysql -u enerfone -p enerfonecrm < enerfonecrm_backup_*.sql
mysql -u enerfone -p grupobasettecrm < grupobasettecrm_backup_*.sql
```

### 3.5. Actualizar Cadenas de Conexión

**IMPORTANTE:** Usar solo `appsettings.Production.json` (sin archivos adicionales) para simplificar.

Editar `appsettings.Production.json` en cada aplicación:

**Para Enerfone** (`C:\inetpub\wwwroot\enerfonecrm\appsettings.Production.json`):

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=enerfonecrm;User=root;Password=TU_PASSWORD;Port=3306;"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*"
}
```

**Para Grupo Basette** (`C:\inetpub\wwwroot\grupobasette\appsettings.Production.json`):

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=grupobasettecrm;User=root;Password=TU_PASSWORD;Port=3306;"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*"
}
```

💡 **Nota:** Usa la cadena de conexión **simple** sin parámetros adicionales. Solo lo básico: Server, Database, User, Password, Port.

---

## 🌐 PARTE 4: CONFIGURAR IIS

### 4.1. Crear Application Pools

```powershell
# Abrir IIS Manager o usar PowerShell

# Crear Application Pool para Enerfone
New-WebAppPool -Name "EnerfoneCRM" -Force
Set-ItemProperty IIS:\AppPools\EnerfoneCRM -Name managedRuntimeVersion -Value ""
Set-ItemProperty IIS:\AppPools\EnerfoneCRM -Name enable32BitAppOnWin64 -Value $false

# Crear Application Pool para Grupo Basette
New-WebAppPool -Name "GrupoBasetteCRM" -Force
Set-ItemProperty IIS:\AppPools\GrupoBasetteCRM -Name managedRuntimeVersion -Value ""
Set-ItemProperty IIS:\AppPools\GrupoBasetteCRM -Name enable32BitAppOnWin64 -Value $false
```

**Desde IIS Manager (GUI):**
1. Abrir **IIS Manager**
2. Click derecho en **Application Pools** → **Add Application Pool**
3. Nombre: `EnerfoneCRM`
4. **.NET CLR version:** `No Managed Code` (importante para .NET Core)
5. Repetir para `GrupoBasetteCRM`

### 4.2. Crear Sitios Web en IIS

#### Para Enerfone (enerfonecrm.com):

```powershell
# Crear sitio web
New-Website -Name "enerfonecrm.com" `
    -PhysicalPath "C:\inetpub\wwwroot\enerfonecrm" `
    -ApplicationPool "EnerfoneCRM" `
    -Port 80 `
    -HostHeader "enerfonecrm.com" `
    -Force

# Agregar binding para www
New-WebBinding -Name "enerfonecrm.com" -Protocol "http" -Port 80 -HostHeader "www.enerfonecrm.com"
```

#### Para Grupo Basette (crm.grupobasette.eu):

```powershell
# Crear sitio web
New-Website -Name "crm.grupobasette.eu" `
    -PhysicalPath "C:\inetpub\wwwroot\grupobasette" `
    -ApplicationPool "GrupoBasetteCRM" `
    -Port 80 `
    -HostHeader "crm.grupobasette.eu" `
    -Force

# Agregar binding para www
New-WebBinding -Name "crm.grupobasette.eu" -Protocol "http" -Port 80 -HostHeader "www.crm.grupobasette.eu"
```

### 4.3. Configurar SSL/HTTPS

**Opción 1: Importar certificados existentes**

1. Exportar certificados del servidor antiguo:
   - IIS Manager → Server Certificates → Export
   - Guardar como `.pfx` con contraseña

2. Importar en servidor nuevo:
   ```powershell
   # Importar certificado
   Import-PfxCertificate -FilePath "C:\Temp\enerfonecrm.pfx" -CertStoreLocation Cert:\LocalMachine\My -Password (ConvertTo-SecureString -String "password" -AsPlainText -Force)
   ```

3. Configurar binding HTTPS:
   - IIS Manager → Sitio → Bindings → Add
   - Type: https
   - Port: 443
   - SSL Certificate: Seleccionar el importado

**Opción 2: Generar nuevos certificados Let's Encrypt**

1. Instalar **Win-ACME** (https://www.win-acme.com/)
2. Ejecutar para cada dominio
3. Configurará automáticamente los bindings HTTPS

### 4.4. Configurar Variables de Entorno

**RECOMENDADO:** No usar variables de entorno personalizadas. Solo configurar `ASPNETCORE_ENVIRONMENT=Production` en el web.config (ver sección 4.5).

Si por alguna razón necesitas configurar variables globales:

```powershell
# Variable global del sistema (opcional)
[System.Environment]::SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", "Production", "Machine")
```

**Desde IIS Manager (solo si es necesario):**
1. Seleccionar sitio web
2. Configuration Editor
3. Section: `system.webServer/aspNetCore`
4. Agregar variables de entorno

💡 **Mejor práctica:** Configurar todo en `web.config` (ver sección 4.5) en lugar de variables globales.

### 4.5. Configurar web.config (si no existe)

Crear `web.config` en cada carpeta de la aplicación.

**IMPORTANTE:** Usar configuración simple sin variables de entorno personalizadas (igual que FactioX).

**Para Enerfone** (`C:\inetpub\wwwroot\enerfonecrm\web.config`):

```xml
<?xml version="1.0" encoding="utf-8"?>
<configuration>
  <location path="." inheritInChildApplications="false">
    <system.webServer>
      <handlers>
        <add name="aspNetCore" path="*" verb="*" modules="AspNetCoreModuleV2" resourceType="Unspecified" />
      </handlers>
      <aspNetCore processPath="dotnet" 
                  arguments=".\EnerfoneCRM.dll" 
                  stdoutLogEnabled="true" 
                  stdoutLogFile=".\logs\stdout" 
                  hostingModel="inprocess">
        <environmentVariables>
          <environmentVariable name="ASPNETCORE_ENVIRONMENT" value="Production" />
        </environmentVariables>
      </aspNetCore>
    </system.webServer>
  </location>
</configuration>
```

**Para Grupo Basette** (`C:\inetpub\wwwroot\grupobasette\web.config`):

```xml
<?xml version="1.0" encoding="utf-8"?>
<configuration>
  <location path="." inheritInChildApplications="false">
    <system.webServer>
      <handlers>
        <add name="aspNetCore" path="*" verb="*" modules="AspNetCoreModuleV2" resourceType="Unspecified" />
      </handlers>
      <aspNetCore processPath="dotnet" 
                  arguments=".\EnerfoneCRM.dll" 
                  stdoutLogEnabled="true" 
                  stdoutLogFile=".\logs\stdout" 
                  hostingModel="inprocess">
        <environmentVariables>
          <environmentVariable name="ASPNETCORE_ENVIRONMENT" value="Production" />
        </environmentVariables>
      </aspNetCore>
    </system.webServer>
  </location>
</configuration>
```

**Notas clave:**
- ✅ `hostingModel="inprocess"` en **minúsculas** (sensible a mayúsculas)
- ✅ Solo variable `ASPNETCORE_ENVIRONMENT=Production` (sin `APPSETTINGS_FILE`)
- ✅ La app cargará automáticamente `appsettings.Production.json`
- ✅ `stdoutLogEnabled="true"` para debug

---

## 🔧 PARTE 5: CONFIGURAR DNS Y FIREWALL

### 5.1. Actualizar Registros DNS

Apuntar los dominios a la IP del nuevo servidor:

**En tu proveedor de DNS:**
```
Tipo    Nombre                  Valor                           TTL
A       enerfonecrm.com        [IP_NUEVO_SERVIDOR]             3600
A       www.enerfonecrm.com    [IP_NUEVO_SERVIDOR]             3600
A       crm.grupobasette.eu    [IP_NUEVO_SERVIDOR]             3600
```

💡 **Recomendación:** Reducir el TTL a 300 (5 minutos) antes de la migración para facilitar el cambio.

### 5.2. Configurar Firewall de Windows

```powershell
# Permitir tráfico HTTP
New-NetFirewallRule -DisplayName "HTTP Port 80" -Direction Inbound -LocalPort 80 -Protocol TCP -Action Allow

# Permitir tráfico HTTPS
New-NetFirewallRule -DisplayName "HTTPS Port 443" -Direction Inbound -LocalPort 443 -Protocol TCP -Action Allow
```

---

## ✅ PARTE 6: VERIFICACIÓN Y PRUEBAS

### 6.1. Probar Localmente (antes de cambiar DNS)

Editar archivo `hosts` en tu computadora:
```
# Windows: C:\Windows\System32\drivers\etc\hosts
# Mac/Linux: /etc/hosts

[IP_NUEVO_SERVIDOR]  enerfonecrm.com
[IP_NUEVO_SERVIDOR]  www.enerfonecrm.com
[IP_NUEVO_SERVIDOR]  crm.grupobasette.eu
```

Abrir navegador y probar:
- http://enerfonecrm.com
- http://crm.grupobasette.eu

### 6.2. Checklist de Verificación

- [ ] Los sitios cargan correctamente
- [ ] El login funciona (conexión a BD OK)
- [ ] Los datos se muestran correctamente
- [ ] Las operaciones CRUD funcionan
- [ ] Los archivos subidos se guardan
- [ ] Los logs se generan correctamente
- [ ] HTTPS funciona (si está configurado)
- [ ] Los emails SMTP se envían

### 6.3. Monitorear Logs

```powershell
# Ver logs de IIS
Get-Content "C:\inetpub\logs\LogFiles\W3SVC*\*.log" -Tail 50 -Wait

# Ver logs de la aplicación
Get-Content "C:\inetpub\wwwroot\enerfonecrm\logs\stdout*.log" -Tail 50 -Wait
```

### 6.4. Reiniciar Servicios

Si hay problemas:

```powershell
# Reiniciar IIS
iisreset /restart

# Reciclar Application Pools
Restart-WebAppPool -Name "EnerfoneCRM"
Restart-WebAppPool -Name "GrupoBasetteCRM"
```

---

## 🚨 RESOLUCIÓN DE PROBLEMAS COMUNES

### Timeout al acceder a la aplicación

**Síntomas:** La aplicación no responde y el navegador muestra error de timeout

**Causas comunes:**
1. **web.config incorrecto:**
   - `hostingModel="InProcess"` con mayúscula → debe ser `hostingModel="inprocess"` (minúsculas)
   - Variable `APPSETTINGS_FILE` apuntando a archivo inexistente o incorrecto
   
2. **Múltiples archivos appsettings:**
   - Tener `appsettings.Production.json` Y `appsettings.Production.Enerfone.json` causa confusión

**Solución:**
```powershell
# 1. Simplificar web.config (quitar APPSETTINGS_FILE)
# Editar C:\inetpub\wwwroot\enerfonecrm\web.config
# Dejar solo: ASPNETCORE_ENVIRONMENT=Production
# Usar: hostingModel="inprocess" (minúsculas)

# 2. Usar solo appsettings.Production.json
Rename-Item "appsettings.Production.Enerfone.json" "appsettings.Production.Enerfone.json.OLD"

# 3. Simplificar cadena de conexión
# En appsettings.Production.json usar:
# "Server=localhost;Database=enerfonecrm;User=root;Password=XXX;Port=3306;"

# 4. Reiniciar Application Pool
Restart-WebAppPool -Name "EnerfoneCRM"

# 5. Ver logs para verificar
Get-Content "C:\inetpub\wwwroot\enerfonecrm\logs\stdout*.log" -Tail 50
```

### Error 500.30 - ASP.NET Core app failed to start

**Causa:** Runtime de .NET no instalado o web.config incorrecto

**Solución:**
1. Verificar que ASP.NET Core Hosting Bundle esté instalado
2. Revisar `web.config`
3. Verificar logs en `.\logs\stdout*.log`

### Error 500.19 - Cannot read configuration

**Causa:** Permisos incorrectos o web.config corrupto

**Solución:**
```powershell
icacls "C:\inetpub\wwwroot\enerfonecrm" /grant "IIS_IUSRS:(OI)(CI)F" /T
```

### Error de conexión a BD

**Causa:** Credenciales incorrectas o MySQL no accesible

**Solución:**
1. Verificar `appsettings.Production.*.json`
2. Probar conexión con MySQL Workbench
3. Verificar firewall de MySQL (puerto 3306)

### WebSockets no funciona

**Solución:**
```powershell
Enable-WindowsOptionalFeature -Online -FeatureName IIS-WebSockets
iisreset /restart
```

---

## 📝 RESUMEN DEL PROCESO

### Servidor Antiguo:
1. ✅ Exportar aplicaciones publicadas
2. ✅ Hacer backup de bases de datos MySQL
3. ✅ Guardar archivos de configuración
4. ✅ Anotar configuración de IIS

### Servidor Nuevo:
1. ✅ Instalar .NET 8 Hosting Bundle
2. ✅ Configurar IIS con features necesarios
3. ✅ Importar aplicaciones
4. ✅ Importar bases de datos
5. ✅ Crear Application Pools
6. ✅ Crear sitios web en IIS
7. ✅ Configurar SSL/HTTPS
8. ✅ Actualizar DNS
9. ✅ Probar y verificar

### Después de la Migración:
- ✅ Monitorear logs por 24-48 horas
- ✅ Verificar que todos los procesos funcionen
- ✅ Mantener el servidor antiguo como respaldo por 1 semana
- ✅ Actualizar DNS TTL a valores normales (3600)

---

## 📞 COMANDOS RÁPIDOS DE REFERENCIA

```powershell
# Verificar estado de IIS
Get-Website

# Ver Application Pools
Get-IISAppPool

# Reiniciar sitio específico
Restart-WebAppPool -Name "EnerfoneCRM"

# Ver bindings
Get-WebBinding -Name "enerfonecrm.com"

# Ver logs en tiempo real
Get-Content "C:\inetpub\wwwroot\enerfonecrm\logs\stdout*.log" -Tail 50 -Wait
```

---

## ⏱️ TIEMPO ESTIMADO DE MIGRACIÓN

- **Exportación:** 30-60 minutos
- **Preparación servidor nuevo:** 1-2 horas
- **Importación y configuración:** 2-3 horas
- **Pruebas:** 1-2 horas
- **Total:** 4-8 horas (dependiendo de experiencia)

---

¡Buena suerte con la migración! 🚀
