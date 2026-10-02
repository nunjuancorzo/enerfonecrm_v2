# Integración con Incidencias Revolapps

Este documento explica cómo **otras aplicaciones** pueden:

1) **Enviar incidencias** a `Incidencias Revolapps` (vía API)
2) **Recibir notificaciones** cuando cambie el estado de una incidencia (vía webhook)

> Base URL (producción): `https://incidencias.revolapps.es`

---

## 1) Autenticación (API)

Todas las llamadas a `/api/*` requieren el header:

- `x-api-key: <TU_API_KEY>`

La `API_KEY` la configura el administrador del servidor.

---

## 2) Registrar una aplicación (para filtrar + webhook)

Antes de enviar incidencias, registra tu aplicación (o deja que se cree automáticamente al recibir la primera incidencia). Registrar es recomendable para:

- Asignar un **nombre legible**
- Configurar el **webhook** de cambios de estado

**Endpoint**

- `POST /api/apps`

**Body (JSON)**

```json
{
  "key": "crm",
  "name": "CRM Revolapps",
  "webhook_url": "https://tuapp.com/webhooks/incidencias?token=..."
}
```

- `key`: identificador estable (se usará como `app_key` en incidencias)
- `webhook_url`: URL donde tu app recibirá `status_changed` (opcional)

**Ejemplo (curl)**

```bash
curl -X POST "https://incidencias.revolapps.es/api/apps" \
  -H "content-type: application/json" \
  -H "x-api-key: TU_API_KEY" \
  -d '{"key":"crm","name":"CRM Revolapps","webhook_url":"https://tuapp.com/webhooks/incidencias?token=..."}'
```

---

## 3) Enviar una incidencia (crear)

**Endpoint**

- `POST /api/incidents`

**Formato**

- `multipart/form-data`
- Adjunto opcional en el campo `attachment`
- Límite de tamaño de archivo: **5 MB** (configurable en servidor)

### Campos soportados

Obligatorios:

- `app_key` *(también acepta alias: `app`, `application`)*
- `subject` *(alias: `asunto`)*
- `incident_type` *(alias: `tipo`, `tipo_incidencia`)*
- `priority` *(alias: `prioridad`)*
- `description` *(alias: `descripcion`)*

Opcionales:

- `user_name` *(alias: `usuario`)*
- `user_email` *(alias: `email`)*
- `attachment` (archivo)

### Valores recomendados

- `incident_type`: `bug` | `mejora` | `soporte` | `otro`
- `priority`: `baja` | `media` | `alta` | `critica`

### Ejemplo (curl)

```bash
curl -X POST "https://incidencias.revolapps.es/api/incidents" \
  -H "x-api-key: TU_API_KEY" \
  -F "app_key=crm" \
  -F "user_name=superadmin" \
  -F "user_email=superadmin@system.local" \
  -F "subject=Error al guardar contrato de energía" \
  -F "incident_type=bug" \
  -F "priority=alta" \
  -F "description=Pasos: 1) ... 2) ..." \
  -F "attachment=@./captura.png"
```

### Ejemplo (JavaScript / fetch)

```js
async function enviarIncidencia() {
  const baseUrl = 'https://incidencias.revolapps.es';
  const apiKey = process.env.INCIDENCIAS_API_KEY;

  const form = new FormData();
  form.append('app_key', 'crm');
  form.append('user_name', 'superadmin');
  form.append('user_email', 'superadmin@system.local');
  form.append('subject', 'Error al guardar contrato de energía');
  form.append('incident_type', 'bug');
  form.append('priority', 'alta');
  form.append('description', 'Pasos: 1) ... 2) ...');
  // form.append('attachment', file);

  const res = await fetch(`${baseUrl}/api/incidents`, {
    method: 'POST',
    headers: { 'x-api-key': apiKey },
    body: form,
  });

  if (!res.ok) {
    throw new Error(`Error creando incidencia: ${res.status} ${await res.text()}`);
  }

  const data = await res.json();
  return data.incident; // incluye id
}
```

### Respuesta

- `201 Created`

```json
{ "incident": { "id": 123, "app_key": "crm", "status": "abierta", "created_at": "..." } }
```

---

## 4) Consultar incidencias (opcional)

**Endpoint**

- `GET /api/incidents?app_key=crm&status=abierta&q=texto`

Parámetros:

- `app_key` (filtro por aplicación)
- `status` (filtro por estado)
- `q` (búsqueda por asunto/descripción/usuario)

---

## 5) Cambiar estado de una incidencia

Normalmente el cambio de estado se hace desde la web centralizada. Si necesitas automatizarlo desde otra app:

**Endpoint**

- `PATCH /api/incidents/:id/status`

**Body (JSON)**

```json
{ "status": "en_progreso" }
```

Estados soportados:

- `abierta`
- `en_progreso`
- `resuelta`
- `cerrada`

---

## 6) Recibir cambios de estado (Webhook)

Cuando una incidencia cambia de estado, si la aplicación (`app_key`) tiene `webhook_url` configurado, el servidor enviará:

- `POST <webhook_url>`
- `content-type: application/json`

**Payload**

```json
{
  "event": "incident.status_changed",
  "incident_id": 123,
  "app_key": "crm",
  "old_status": "abierta",
  "new_status": "en_progreso",
  "updated_at": "2026-02-11 12:34:56"
}
```

### Recomendación de seguridad para webhooks

El emisor no firma el mensaje (por simplicidad). Para proteger tu endpoint:

- Usa **HTTPS**
- Añade un **token** en la URL (`.../webhooks/incidencias?token=...`) y valida en tu app
- Opcional: restringe por IP/Firewall si tu infraestructura lo permite

### Ejemplo receptor (Express)

```js
import express from 'express';

const app = express();
app.use(express.json());

app.post('/webhooks/incidencias', (req, res) => {
  const token = req.query.token;
  if (token !== process.env.INCIDENCIAS_WEBHOOK_TOKEN) {
    return res.status(401).send('unauthorized');
  }

  const evt = req.body;
  if (evt.event !== 'incident.status_changed') return res.status(204).end();

  // Ej: actualizar tu registro local
  // incident_id, app_key, old_status, new_status, updated_at

  res.status(200).send('ok');
});

app.listen(3001);
```

---

## 7) Errores comunes

- `401 No autorizado`: falta `x-api-key` o es incorrecta.
- `400 Bad Request`: faltan campos obligatorios (por ejemplo `app_key` o `subject`).
- `500`: configuración del servidor incompleta (por ejemplo `API_KEY` no configurada).
