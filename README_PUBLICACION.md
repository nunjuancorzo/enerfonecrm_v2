# Guía de Publicación - EnerfoneCRM v2

## Estructura de Publicación

Este proyecto tiene **dos instancias de producción** independientes:

### 1. Enerfone CRM
- **URL**: https://enerfonecrm.com
- **Base de datos**: `enerfone_pre`
- **Ruta servidor**: `C:\enerfonecrm`
- **Carpeta publicación local**: `publicado/`

### 2. Grupo Basette CRM
- **URL**: https://crm.grupobasette.eu
- **Base de datos**: `crmgrupobasette`
- **Ruta servidor**: `C:\grupobasettecrm`
- **Carpeta publicación local**: `publicadogrupobasette/`

---

## Publicar desde tu Mac (desarrollo)

### Opción 1: Scripts PowerShell (recomendado)

**Para Enerfone:**
```powershell
.\PUBLICAR_ENERFONE.ps1
```

**Para Grupo Basette:**
```powershell
.\PUBLICAR_GRUPOBASETTE.ps1
```

### Opción 2: Desde Visual Studio

1. Click derecho en el proyecto **EnerfoneCRM**
2. **Publicar...**
3. Selecciona el perfil:
   - `FolderProfile-Enerfone` → publica en `publicado/`
   - `FolderProfile-GrupoBasette` → publica en `publicadogrupobasette/`
4. Click **Publicar**

### Opción 3: Línea de comandos manual

**Enerfone:**
```bash
dotnet publish EnerfoneCRM/EnerfoneCRM.csproj -c Release -o publicado --no-self-contained
cp appsettings.Production.Enerfone.json publicado/appsettings.Production.json
```

**Grupo Basette:**
```bash
dotnet publish EnerfoneCRM/EnerfoneCRM.csproj -c Release -o publicadogrupobasette --no-self-contained
cp appsettings.Production.GrupoBasette.json publicadogrupobasette/appsettings.Production.json
```

---

## Desplegar en el Servidor Windows

### Paso 1: Preparar archivos
1. Comprimir la carpeta correspondiente (`publicado/` o `publicadogrupobasette/`)
2. Transferir al servidor vía FTP/RDP

### Paso 2: En el servidor
1. Abrir **Administrador de IIS**
2. Detener el sitio web correspondiente
3. **Backup opcional**: Renombrar carpeta actual a `[nombre]_backup_YYYYMMDD`

### Paso 3: Copiar archivos
**⚠️ IMPORTANTE**: NO sobrescribir estas carpetas:
- `wwwroot/uploads/`
- `storage/`

**Copiar del ZIP al destino**:
- Para Enerfone → `C:\enerfonecrm`
- Para Grupo Basette → `C:\grupobasettecrm`

### Paso 4: Verificar permisos
Ejecutar en PowerShell como administrador:

**Enerfone:**
```powershell
icacls "C:\enerfonecrm" /grant "IIS AppPool\Enerfonecrm:(OI)(CI)M"
icacls "C:\enerfonecrm\wwwroot\uploads" /grant "IIS AppPool\Enerfonecrm:(OI)(CI)F"
icacls "C:\enerfonecrm\storage" /grant "IIS AppPool\Enerfonecrm:(OI)(CI)F"
```

**Grupo Basette:**
```powershell
icacls "C:\grupobasettecrm" /grant "IIS AppPool\GrupoBasetteCRM:(OI)(CI)M"
icacls "C:\grupobasettecrm\wwwroot\uploads" /grant "IIS AppPool\GrupoBasetteCRM:(OI)(CI)F"
icacls "C:\grupobasettecrm\storage" /grant "IIS AppPool\GrupoBasetteCRM:(OI)(CI)F"
```

### Paso 5: Reiniciar IIS
```powershell
iisreset
```

O desde IIS Manager:
- Iniciar el sitio web
- Click derecho → **Administrar sitio web** → **Reiniciar**

### Paso 6: Verificar
- Enerfone: https://enerfonecrm.com
- Grupo Basette: https://crm.grupobasette.eu

---

## Verificación Post-Despliegue

✅ **Checklist**:
- [ ] El sitio carga correctamente
- [ ] Login funciona
- [ ] Se pueden crear/editar contratos
- [ ] Los archivos subidos funcionan
- [ ] No hay errores en `logs/stdout` del servidor
- [ ] La base de datos conecta correctamente

---

## Configuración de Bases de Datos

### Ambas instancias usan:
- **Usuario**: `enerfone`
- **Password**: `Salaiet6680.`
- **Servidor**: `localhost`
- **Puerto**: `3306`

### Diferencia solo en el nombre de BD:
- Enerfone: `enerfone_pre`
- Grupo Basette: `crmgrupobasette`

---

## Migraciones de Base de Datos

Antes de publicar, ejecuta en cada BD:

**Para enerfone_pre:**
```sql
USE enerfone_pre;
-- Ejecutar SCRIPT_COMPLETO_LIQ.sql (cambiar USE a enerfone_pre)
```

**Para crmgrupobasette:**
```sql
USE crmgrupobasette;
-- Ejecutar SCRIPT_COMPLETO_LIQ.sql
```

---

## Troubleshooting

### Error 500 al cargar el sitio
1. Revisar `C:\[carpeta]\logs\stdout*.log`
2. Verificar que `appsettings.Production.json` tiene la conexión correcta
3. Verificar que el pool de aplicaciones está iniciado

### No conecta a la base de datos
1. Verificar que MySQL está corriendo
2. Probar conexión con HeidiSQL/MySQL Workbench
3. Revisar usuario y contraseña en `appsettings.Production.json`

### Error de permisos
```powershell
# Ejecutar como administrador
icacls "C:\[carpeta]" /grant "IIS AppPool\[NombrePool]:(OI)(CI)F"
iisreset
```

---

## Contacto

Para dudas sobre la publicación, revisar este documento o contactar al equipo de desarrollo.
