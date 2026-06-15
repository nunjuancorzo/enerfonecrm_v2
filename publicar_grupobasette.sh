#!/bin/bash
# ==============================================================================
# Script de publicación para GRUPO BASETTE CRM (versión Bash para macOS)
# Destino: C:\grupobasettecrm
# Base de datos: crmgrupobasette
# ==============================================================================

echo "========================================"
echo "PUBLICACIÓN GRUPO BASETTE CRM"
echo "========================================"
echo ""

# Configuración
PROYECTO_PATH="./EnerfoneCRM/EnerfoneCRM.csproj"
PUBLISH_PATH="./publicadogrupobasette"
APPSETTINGS_PRODUCCION="./appsettings.Production.GrupoBasette.json"

# Paso 0: Limpiar compilaciones anteriores
echo "0. Limpiando compilaciones anteriores..." 
rm -rf ./EnerfoneCRM/bin
rm -rf ./EnerfoneCRM/obj
rm -rf "$PUBLISH_PATH"
dotnet clean "$PROYECTO_PATH"
echo "✓ Limpieza completada"
echo ""

# Paso 1: Compilar y publicar
echo "1. Compilando y publicando proyecto..."
dotnet publish "$PROYECTO_PATH" -c Release -o "$PUBLISH_PATH" --no-self-contained --force

if [ $? -ne 0 ]; then
    echo "ERROR: Falló la compilación"
    exit 1
fi
echo "✓ Compilación completada"
echo ""

# Paso 2: Copiar appsettings.Production.json correcto
echo "2. Configurando appsettings.Production.json para Grupo Basette..."
cp "$APPSETTINGS_PRODUCCION" "$PUBLISH_PATH/appsettings.Production.json"
echo "✓ Configuración copiada"
echo ""

# Paso 3: Crear web.config
echo "3. Creando web.config..."
cat > "$PUBLISH_PATH/web.config" << 'EOF'
<?xml version="1.0" encoding="utf-8"?>
<configuration>
  <location path="." inheritInChildApplications="false">
    <system.webServer>
      <handlers>
        <add name="aspNetCore" path="*" verb="*" modules="AspNetCoreModuleV2" resourceType="Unspecified" />
      </handlers>
      <aspNetCore processPath="dotnet" arguments=".\EnerfoneCRM.dll" stdoutLogEnabled="false" stdoutLogFile=".\logs\stdout" hostingModel="InProcess">
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
EOF
echo "✓ web.config creado"
echo ""

# Paso 4: Verificación
echo "4. Verificando publicación..."
if [ -f "$PUBLISH_PATH/EnerfoneCRM.dll" ]; then
    echo "✓ EnerfoneCRM.dll presente ($(ls -lh $PUBLISH_PATH/EnerfoneCRM.dll | awk '{print $5}'))"
else
    echo "✗ ERROR: No se encontró EnerfoneCRM.dll"
    exit 1
fi

if [ -f "$PUBLISH_PATH/appsettings.Production.json" ]; then
    echo "✓ appsettings.Production.json presente"
else
    echo "✗ ERROR: No se encontró appsettings.Production.json"
    exit 1
fi

if [ -f "$PUBLISH_PATH/web.config" ]; then
    echo "✓ web.config presente"
    grep -q 'stdoutLogEnabled="false"' "$PUBLISH_PATH/web.config" && echo "✓ stdoutLogEnabled=false configurado" || echo "✗ WARNING: stdoutLogEnabled no está en false"
else
    echo "✗ ERROR: No se encontró web.config"
    exit 1
fi

echo ""
echo "========================================"
echo "PUBLICACIÓN COMPLETADA"
echo "========================================"
echo ""
echo "Carpeta de publicación: $PUBLISH_PATH"
echo "Tamaño: $(du -sh $PUBLISH_PATH | cut -f1)"
echo ""
echo "PASOS SIGUIENTES EN EL SERVIDOR WINDOWS:"
echo "1. Detener el sitio en IIS:"
echo "   Stop-IISSite -Name 'grupobasettecrm'"
echo ""
echo "2. Hacer backup de la carpeta actual (recomendado):"
echo "   Copy-Item C:\\grupobasettecrm C:\\backups\\grupobasettecrm_\$(Get-Date -Format 'yyyyMMdd_HHmmss') -Recurse"
echo ""
echo "3. Copiar archivos al servidor:"
echo "   - Copiar todo el contenido de '$PUBLISH_PATH' a 'C:\\grupobasettecrm'"
echo "   - EXCEPTO: wwwroot\\uploads y storage (para preservar archivos)"
echo "   - Puedes usar: robocopy \"\\\\origen\\$PUBLISH_PATH\" \"C:\\grupobasettecrm\" /MIR /XD \"wwwroot\\uploads\" \"storage\""
echo ""
echo "4. Iniciar el sitio en IIS:"
echo "   Start-IISSite -Name 'grupobasettecrm'"
echo ""
echo "5. Limpiar logs antiguos (IMPORTANTE):"
echo "   Remove-Item C:\\grupobasettecrm\\logs\\* -Recurse -Force"
echo ""
echo "6. Verificar en https://crm.grupobasette.eu"
echo "   - El footer debe mostrar: Versión 20260615"
echo ""
