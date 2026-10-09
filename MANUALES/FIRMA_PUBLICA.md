# Firma publica de contratos

## Implementacion

La funcionalidad usa una solicitud de firma independiente del estado operativo del contrato.

- `SolicitudFirma` conserva el token como hash SHA-256, nunca el token original.
- La URL publica tiene el formato `/firma/{token}`.
- El token contiene 256 bits aleatorios generados con `RandomNumberGenerator`.
- La solicitud caduca por configuracion y puede cancelarse.
- Cada operacion publica registra un evento en `solicitudes_firma_eventos`.
- El documento original y el documento firmado se conservan como documentos separados en `ficheroscontratos`.
- La pagina publica no requiere usuario ni contrasena.

## Puesta en marcha

1. Copiar `contrato_clientes.pdf` junto a la solucion. El proyecto lo copia al directorio de salida y de publicacion.
2. Ejecutar `ADD_FIRMA_PUBLICA.sql` sobre la base de datos usada por la aplicacion.
3. Configurar `PublicSigning` en el archivo de configuracion del entorno.
4. Publicar la aplicacion normalmente para IIS.

Ejemplo de configuracion:

```json
"PublicSigning": {
  "Enabled": true,
  "BaseUrl": "https://contratos.midominio.com",
  "TokenExpirationDays": 30
}
```

`BaseUrl` debe ser la URL HTTPS publica que recibira el cliente. No debe contener una barra final.

## Flujo para el colaborador

### Registro de un nuevo usuario

- El email del registro es obligatorio y debe tener un formato valido. Se guarda sin espacios al principio o al final.
- Tras crear el usuario Colaborador, todavia inactivo, el registro solicita automaticamente la firma del contrato de colaboracion. El correo contiene un enlace para consultar y firmar el documento; no requiere iniciar sesion ni activar antes la cuenta.
- Se reutiliza `FirmaService.CrearSolicitudColaboradorAsync`, la plantilla `Resources/contrato_colaboradores.pdf` y la solicitud de firma del colaborador. La URL es `/firma-colaborador/{token}` y su caducidad actual es de dos dias.
- Deben estar disponibles la tabla `solicitudes_firma_colaboradores`, la plantilla y la configuracion SMTP. `PublicSigning:Enabled` debe estar activo y `PublicSigning:BaseUrl` debe ser una URL HTTPS publica, sin query ni fragmento. HTTP solo se admite para localhost en desarrollo.
- La pantalla confirma el envio cuando SMTP lo acepta. Si falla el envio o la generacion, la cuenta no se vuelve a crear: se muestra un aviso y un administrador puede reenviar el contrato desde la ficha del usuario.
- Las notificaciones a administradores se conservan y se envian despues de solicitar la firma al nuevo colaborador.

### Envio de contratos al cliente

1. Abrir la ficha del contrato en energia, telefonia o alarmas.
2. Comprobar que existe cliente, email valido y datos de proveedor.
3. Pulsar `Enviar documentacion al cliente`.
4. El sistema genera una copia cumplimentada de `contrato_clientes.pdf`, la guarda como documento original y envia el enlace por SMTP.
5. Mientras este pendiente se puede reenviar, lo que invalida la solicitud anterior, o cancelar.
6. Cuando el cliente termina, la ficha muestra el estado y permite descargar el documento firmado.

## Flujo para el cliente

El cliente abre el enlace, consulta el PDF, acepta la documentacion, confirma la operacion y dibuja su firma desde movil, tablet u ordenador.

Los documentos se entregan como contenido temporal validado en servidor. No se publican rutas fisicas ni URLs permanentes de almacenamiento.

## Base de datos y trazabilidad

`solicitudes_firma` almacena el proceso, caducidad, estado, hashes de documentos, imagen de firma y referencia a los documentos original/firmado.

`solicitudes_firma_eventos` registra apertura, consulta, envio, cancelacion, caducidad y finalizacion.

## IIS

- Instalar el ASP.NET Core Hosting Bundle de .NET 8.
- Configurar binding HTTPS para el dominio usado en `BaseUrl`.
- Mantener WebSockets habilitado para Blazor Server.
- Permitir el limite de peticion configurado actualmente de 10 MB.
- Dar permisos de lectura a la plantilla publicada.
- No publicar ni enlazar documentos de firma bajo `/storage`.

No es necesario crear una regla IIS especial para `/firma/...`; la ruta la resuelve ASP.NET Core mediante Blazor. Las demas rutas mantienen sus comprobaciones privadas existentes.

## Limitacion juridica

La primera version captura una firma manuscrita dibujada en pantalla y registra evidencias tecnicas del proceso. No constituye por si misma una firma electronica avanzada o cualificada. Debe validarse juridicamente si este mecanismo satisface el nivel exigible para cada contrato de energia, telefonia o alarmas. La arquitectura separa el proceso de firma mediante `FirmaService` para poder sustituirlo posteriormente por Signaturit, DocuSign u otro proveedor.