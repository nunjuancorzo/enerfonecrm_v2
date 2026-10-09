# Publicacion e importacion de facturas en Windows/IIS

## Paquete preparado

- Carpeta: `publicado`.
- Compilacion: Release, .NET 8, Windows x64, dependiente del runtime instalado.
- IIS usa `EnerfoneCRM.exe` y `ASPNETCORE_ENVIRONMENT=Production.Enerfone`.
- Esta carpeta corresponde a Enerfone; para otro entorno deben revisarse la configuracion y el entorno de IIS. No utilizar la conexion de Enerfone para Grupo Basette.
- PdfPig se incluye en la publicacion: no requiere instalar otro lector para PDF con texto.
- PDF escaneados e imagenes necesitan Tesseract (idioma espanol `spa`). PDF escaneados necesitan tambien Poppler (`pdftoppm`).
- No se necesita Python para la lectura de facturas.
- El importador admite PDF, PNG, JPEG y WebP, hasta 10 MB y 20 paginas. El OCR local tiene un limite de dos minutos. Las incidencias admiten imagenes hasta 20 MB; el `web.config` publicado permite peticiones hasta 25 MB.

## 1. Preparar IIS y .NET

En el servidor Windows, con una cuenta administradora:

1. Instalar el ultimo parche del **Hosting Bundle de .NET 8**, no solamente el SDK o el runtime de escritorio: <https://dotnet.microsoft.com/download/dotnet/8.0>.
2. IIS debe estar instalado antes del Hosting Bundle. Si se instalo despues, ejecutar de nuevo el instalador del Hosting Bundle y elegir reparar.
3. En el pool de la aplicacion: CLR de .NET = **Sin codigo administrado**, habilitar aplicaciones de 32 bits = **False**, identidad = **ApplicationPoolIdentity** (o la cuenta de servicio elegida).
4. En Caracteristicas de Windows/Servidor, habilitar **WebSocket Protocol** para Blazor Server.
5. No cambiar el sitio de produccion a `Development`.

Comprobar en PowerShell:

```powershell
dotnet --list-runtimes
```

Deben aparecer `Microsoft.NETCore.App 8.0.x` y `Microsoft.AspNetCore.App 8.0.x`.

## 2. Instalar Tesseract de 64 bits e idioma espanol

1. Descargar el instalador de Windows desde <https://github.com/UB-Mannheim/tesseract/wiki>.
2. Instalarlo para todos los usuarios en `C:\Program Files\Tesseract-OCR`. Usar la carpeta propuesta o una nueva: no instalarlo dentro de la carpeta del CRM.
3. Seleccionar los datos de idioma **Spanish / Espanol**. Debe existir `C:\Program Files\Tesseract-OCR\tessdata\spa.traineddata`.
4. Si falta el idioma, descargar `spa.traineddata` de <https://raw.githubusercontent.com/tesseract-ocr/tessdata_fast/main/spa.traineddata> y copiarlo a esa carpeta `tessdata`. No guardar la pagina HTML de GitHub como si fuera el modelo.

Comprobar desde PowerShell:

```powershell
& 'C:\Program Files\Tesseract-OCR\tesseract.exe' --version
& 'C:\Program Files\Tesseract-OCR\tesseract.exe' --list-langs
```

La lista debe incluir `spa`. Si existen variables `TESSDATA_PREFIX` antiguas, verificar que no apunten a otro directorio sin ese idioma. Normalmente no hace falta crear esta variable.

## 3. Instalar Poppler de 64 bits

1. Descargar el ZIP de binarios, no el ZIP de codigo fuente, desde <https://github.com/oschwartz10612/poppler-windows/releases>.
2. En las propiedades del ZIP, pulsar **Desbloquear** si Windows lo marca como archivo descargado.
3. Extraerlo conservando todas las carpetas y DLL. Organizarlo para que exista `C:\OCR\poppler\Library\bin\pdftoppm.exe`. No copiar solo ese ejecutable: necesita las DLL del paquete.
4. Si faltan DLL del runtime de Visual C++, instalar Microsoft Visual C++ Redistributable x64: <https://aka.ms/vs/17/release/vc_redist.x64.exe>.

Comprobar:

```powershell
& 'C:\OCR\poppler\Library\bin\pdftoppm.exe' -v
```

La version se escribe normalmente por stderr; eso no significa que haya fallado.

## 4. Configurar el PATH del sistema

Abrir **Sistema > Configuracion avanzada del sistema > Variables de entorno**. En **Variables del sistema > Path**, anadir estas dos entradas sin borrar las existentes:

```text
C:\Program Files\Tesseract-OCR
C:\OCR\poppler\Library\bin
```

Debe ser el PATH del sistema, no solamente el del usuario administrador. La aplicacion ejecuta `pdftoppm` por nombre; una ruta disponible solo en tu sesion no sirve al pool de IIS.

Abrir una PowerShell nueva y comprobar:

```powershell
where.exe tesseract
where.exe pdftoppm
tesseract --list-langs
pdftoppm -v
```

## 5. Permisos de ejecucion y temporales

Usar el nombre real del pool en lugar de `enerfonecrm` en los ejemplos. En PowerShell como administrador:

```powershell
$pool = 'enerfonecrm'
$cuenta = "IIS AppPool\$pool"
icacls 'C:\Program Files\Tesseract-OCR' /grant "${cuenta}:(OI)(CI)(RX)" /T
icacls 'C:\OCR\poppler' /grant "${cuenta}:(OI)(CI)(RX)" /T
```

No ejecutar el pool como Administrador ni conceder Control total a Todos para resolver permisos.

El OCR escribe archivos temporales y los elimina al terminar. Para tener una ubicacion controlada, crear una carpeta privada para este pool:

```powershell
$temporal = "C:\OCR\temp\$pool"
New-Item -ItemType Directory -Path $temporal -Force
icacls $temporal /inheritance:r
icacls $temporal /grant:r '*S-1-5-18:(OI)(CI)(F)' '*S-1-5-32-544:(OI)(CI)(F)' "${cuenta}:(OI)(CI)(M)"
```

Los SID utilizados representan SYSTEM y el grupo Administradores, independientemente del idioma de Windows. La identidad del pool solo recibe Modificar en su carpeta temporal.

En el `web.config` del servidor, dentro del `environmentVariables` ya existente, anadir TEMP y TMP con esa misma ruta, conservando el entorno de produccion:

```xml
<environmentVariables>
  <environmentVariable name="ASPNETCORE_ENVIRONMENT" value="Production.Enerfone" />
  <environmentVariable name="TEMP" value="C:\OCR\temp\enerfonecrm" />
  <environmentVariable name="TMP" value="C:\OCR\temp\enerfonecrm" />
</environmentVariables>
```

Si se usa una cuenta de servicio propia en vez de ApplicationPoolIdentity, asignar los permisos a esa cuenta. Permitir la ejecucion de los binarios en antivirus/AppLocker segun la politica del servidor; no desactivar la proteccion global.

## 6. Aplicar las variables nuevas a IIS

Los servicios WAS/W3SVC pueden conservar el PATH anterior aunque se recicle un pool. Programar una ventana de mantenimiento: estos comandos afectan a todos los sitios de IIS.

```powershell
net stop was /y
net start w3svc
```

Un reinicio programado del servidor es otra opcion. Referencia: <https://learn.microsoft.com/aspnet/core/host-and-deploy/iis/hosting-bundle?view=aspnetcore-8.0>.

## 7. Subir la publicacion

1. Hacer copia de seguridad del sitio, la base de datos y la configuracion.
2. Detener el sitio y su pool, o usar `app_offline.htm` durante la copia.
3. Copiar el contenido de `publicado` a la carpeta del sitio.
4. **Conservar los archivos reales de `storage`, `uploads` y `wwwroot\uploads`, asi como las claves, cadenas de conexion y ajustes propios del servidor.** No usar un mirroring que elimine esos datos. Revisar `appsettings.Production.Enerfone.json`, que se carga en este paquete por el nombre del entorno.
5. Mantener el nuevo `web.config` y combinar con cuidado las variables TEMP/TMP y otros ajustes locales. Los scripts antiguos de publicacion fijaban 10 MB en IIS; esta entrega usa 25 MB.
6. Quitar `app_offline.htm` si se utilizo e iniciar el pool y el sitio.
7. La lectura de facturas no requiere una migracion nueva de base de datos. Las demas funcionalidades desplegadas deben tener sus tablas y migraciones correspondientes ya aplicadas.

## 8. Prueba del OCR bajo IIS

Probar primero los ejecutables con una factura de prueba no sensible:

```powershell
pdftoppm -f 1 -singlefile -r 150 -png 'C:\OCR\prueba\factura.pdf' 'C:\OCR\prueba\pagina'
tesseract 'C:\OCR\prueba\pagina.png' stdout -l spa --psm 3
```

Estas pruebas como administrador no demuestran que el pool tenga permisos: la comprobacion definitiva es dentro de la aplicacion.

1. Acceder al comparador e importar un PDF con texto. Debe indicar **PDF local**.
2. Importar un PDF escaneado o una imagen legible. Debe indicar **OCR local**.
3. Revisar los datos y sus advertencias antes de aplicarlos. No enviar datos de clientes reales a proveedores externos para esta prueba.
4. Si funciona por PowerShell pero falla en IIS, revisar PATH del servicio, `spa`, permisos, variables TEMP/TMP y antivirus/AppLocker.

## Proveedores externos (opcionales)

El modo local no necesita API keys. Para usar OpenAI o Azure, configurar el proveedor, el modelo/endpoint y su credencial en la configuracion de la empresa, y permitir HTTPS saliente a sus APIs. El envio requiere consentimiento en el dialogo y puede tener coste. Google Vision no esta implementado. No incluir credenciales en comandos, capturas o incidencias.

## Comprobaciones realizadas

La publicacion se ha generado y revisado desde macOS: Release `win-x64`, DLL reciente, PdfPig, dependencias nativas de Windows y recursos. El arranque en IIS y la ejecucion de Tesseract/Poppler bajo la identidad del pool deben verificarse en el servidor Windows.