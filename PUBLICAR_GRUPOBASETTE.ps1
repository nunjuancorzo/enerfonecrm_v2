# ==============================================================================
# Script de publicación para GRUPO BASETTE CRM
# Destino: C:\grupobasettecrm
# Base de datos: crmgrupobasette
# ==============================================================================

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "PUBLICACIÓN GRUPO BASETTE CRM" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

# Configuración
$proyectoPath = ".\EnerfoneCRM\EnerfoneCRM.csproj"
$publishPath = ".\publicadogrupobasette"
$destinoProduccion = "C:\grupobasettecrm"
$appSettingsProduccion = ".\appsettings.Production.GrupoBasette.json"

# Paso 1: Compilar y publicar
Write-Host "1. Compilando y publicando proyecto..." -ForegroundColor Yellow
dotnet publish $proyectoPath -c Release -o $publishPath --no-self-contained

if ($LASTEXITCODE -ne 0) {
    Write-Host "ERROR: Falló la compilación" -ForegroundColor Red
    exit 1
}

# Paso 2: Copiar appsettings.Production.json correcto
Write-Host "2. Configurando appsettings.Production.json para Grupo Basette..." -ForegroundColor Yellow
Copy-Item $appSettingsProduccion -Destination "$publishPath\appsettings.Production.json" -Force

# Paso 3: Crear web.config
Write-Host "3. Creando web.config..." -ForegroundColor Yellow
$webConfigContent = @"
<?xml version="1.0" encoding="utf-8"?>
<configuration>
  <location path="." inheritInChildApplications="false">
    <system.webServer>
      <handlers>
        <add name="aspNetCore" path="*" verb="*" modules="AspNetCoreModuleV2" resourceType="Unspecified" />
      </handlers>
      <aspNetCore processPath="dotnet" arguments=".\EnerfoneCRM.dll" stdoutLogEnabled="true" stdoutLogFile=".\logs\stdout" hostingModel="InProcess">
        <environmentVariables>
          <environmentVariable name="ASPNETCORE_ENVIRONMENT" value="Production" />
        </environmentVariables>
      </aspNetCore>
      <security>
        <requestFiltering>
          <requestLimits maxAllowedContentLength="10485760" />
        </requestFiltering>
      </security>
    </system.webServer>
  </location>
  <system.webServer>
    <httpRedirect enabled="false" destination="" />
  </system.webServer>
</configuration>
"@

$webConfigContent | Out-File -FilePath "$publishPath\web.config" -Encoding utf8 -Force

Write-Host ""
Write-Host "========================================" -ForegroundColor Green
Write-Host "PUBLICACIÓN COMPLETADA" -ForegroundColor Green
Write-Host "========================================" -ForegroundColor Green
Write-Host ""
Write-Host "Carpeta de publicación: $publishPath" -ForegroundColor White
Write-Host ""
Write-Host "PASOS SIGUIENTES EN EL SERVIDOR:" -ForegroundColor Yellow
Write-Host "1. Detener el sitio en IIS (C:\grupobasettecrm)" -ForegroundColor White
Write-Host "2. Hacer backup de la carpeta actual (opcional pero recomendado)" -ForegroundColor White
Write-Host "3. Copiar todo el contenido de '$publishPath' a '$destinoProduccion'" -ForegroundColor White
Write-Host "   - EXCEPTO: wwwroot\uploads y storage (para preservar archivos)" -ForegroundColor White
Write-Host "4. Iniciar el sitio en IIS" -ForegroundColor White
Write-Host "5. Verificar en https://crm.grupobasette.eu" -ForegroundColor White
Write-Host ""
