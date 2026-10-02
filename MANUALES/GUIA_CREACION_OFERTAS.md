# 📋 Guía de Creación de Ofertas para Usuarios

## 📌 Descripción General

El módulo de **Solicitudes de Ofertas** permite a los comerciales solicitar ofertas personalizadas para sus clientes de forma rápida y organizada. El sistema envía automáticamente las solicitudes al equipo administrativo por email y permite hacer seguimiento de cada petición.

Esta guía está diseñada para **usuarios comerciales** que necesitan solicitar ofertas para sus clientes.

---

## 🎯 Acceso al Módulo

1. Inicia sesión en el CRM
2. En el menú superior, ve a **Gestiones**
3. Haz clic en **Ofertas**

---

## ✨ Crear una Nueva Solicitud de Oferta

### Paso 1: Iniciar Nueva Solicitud

1. En la página de Ofertas, haz clic en el botón **"Nueva Solicitud"** (esquina superior derecha)
2. Se abrirá un formulario modal amplio con todas las opciones

### Paso 2: Seleccionar Tipos de Oferta

Marca una o varias casillas según lo que necesite tu cliente:

| Tipo de Oferta | Descripción |
|----------------|-------------|
| ⚡ **Luz** | Oferta de suministro eléctrico |
| 🔥 **Gas** | Oferta de suministro de gas |
| ☀️ **Fotovoltaica** | Instalación de paneles solares |
| 📡 **Fibra** | Internet por fibra óptica |
| 📱 **Móvil** | Línea móvil |
| 🌐 **Fibra + Móvil** | Pack combinado |
| 📺 **Fibra + Móvil + TV** | Pack completo |
| 🚨 **Alarma** | Sistema de seguridad |

💡 **Importante**: Puedes seleccionar múltiples tipos en una misma solicitud. El formulario se adaptará dinámicamente mostrando solo los campos necesarios.

---

## 📝 Completar Datos del Interesado

**Todos estos campos son obligatorios:**

- **Nombre del Interesado**: Nombre completo del cliente potencial
- **Teléfono**: Número de contacto (ej: 600123456)
- **Email**: Correo electrónico del cliente

---

## 📋 Formularios Específicos por Tipo de Oferta

### ⚡🔥 Luz / Gas

Cuando seleccionas Luz o Gas, aparece este formulario:

**Campos requeridos:**

1. **Tipo de Cliente** (obligatorio):
   - 🏠 **Residencial**: Para viviendas particulares
   - 🏢 **PYME**: Para negocios, comercios, autónomos

2. **Última Factura** (opcional pero recomendado):
   - Formatos aceptados: PDF, JPG, PNG
   - Tamaño máximo: 10MB
   - Recomendación: Adjunta la última factura de luz/gas del cliente para que puedan hacer una comparativa real

3. **Observaciones** (opcional):
   - Cualquier información adicional relevante
   - Ejemplos: "Cliente busca tarifa con discriminación horaria", "Tiene placas solares", "Consumo elevado en invierno"

---

### ☀️ Fotovoltaica

Cuando seleccionas Fotovoltaica, aparece este formulario:

**Campos:**

1. **Factura Actual** (opcional pero recomendado):
   - Formatos aceptados: PDF, JPG, PNG
   - Tamaño máximo: 10MB
   - Ayuda a dimensionar correctamente la instalación

2. **Enlace de Google Maps** (opcional):
   - URL completa de la ubicación (ej: `https://maps.google.com/...`)
   - Permite al equipo técnico evaluar la orientación del tejado, espacio disponible, etc.
   - **Cómo obtenerlo**: 
     - Ve a Google Maps
     - Busca la dirección
     - Haz clic derecho en la ubicación
     - Selecciona "Compartir" o "Copiar enlace"

3. **Observaciones** (opcional):
   - Información sobre el tejado (orientación, inclinación, material)
   - Restricciones urbanísticas
   - Objetivos de ahorro o autoconsumo

---

### 📡📱 Telefonía (Fibra / Móvil / Packs)

Cuando seleccionas cualquier opción de telefonía, aparece este formulario:

**Campos:**

1. **Tipo de Solicitud** (obligatorio):
   - ✨ **Alta Nueva**: El cliente no tiene servicio actualmente
   - 🔄 **Portabilidad**: El cliente quiere cambiar desde otra compañía

2. **Factura Anterior** (solo si es Portabilidad):
   - Formatos aceptados: PDF, JPG, PNG
   - Tamaño máximo: 10MB
   - Obligatorio en portabilidades para ver qué tiene contratado y comparar

3. **¿Qué tiene contratado actualmente?** (opcional pero útil):
   - Describe el servicio actual del cliente
   - Ejemplos: "Fibra 300Mb + 2 líneas móviles con 50GB", "Solo fibra 100Mb"

4. **¿Qué desea?** (opcional):
   - Describe lo que el cliente necesita o quiere mejorar
   - Ejemplos: "Quiere más velocidad de fibra", "Necesita añadir 2 líneas móviles más", "Busca abaratar la factura"

---

### 🚨 Alarmas

Cuando seleccionas Alarma, aparece este formulario:

**Campos:**

1. **Tipo de Cliente** (obligatorio):
   - 🏢 **Negocio**: Local comercial, oficina, nave, etc.
   - 🏠 **Residencial**: Vivienda particular

2. **¿Tiene alarma actualmente?** (obligatorio):
   - ✅ Sí
   - ❌ No

3. **Observaciones** (opcional):
   - Características del inmueble (tamaño, nº de estancias)
   - Si tiene alarma actual: compañía, tipo de sistema, cuota mensual
   - Necesidades específicas: cámaras, detector de humo, acceso remoto, etc.

---

## 📤 Enviar la Solicitud

1. Una vez completados todos los campos necesarios, revisa la información
2. Haz clic en el botón **"Enviar Solicitud"** en la parte inferior del formulario
3. Si hay algún error (campo obligatorio vacío, archivo muy grande), el sistema te lo indicará
4. Si todo está correcto, verás un mensaje de confirmación: ✅ "Solicitud enviada correctamente"

### ¿Qué pasa después?

- 📧 Se envía automáticamente un **email al equipo administrativo** con toda la información
- 📋 Tu solicitud aparece en el listado con estado **"Pendiente"**
- 🔔 Recibes un email de confirmación
- ⏱️ El equipo revisará tu solicitud y te contactará con las ofertas disponibles

---

## 📊 Gestionar y Consultar Solicitudes

### Ver tus Solicitudes

En la página de Ofertas verás una tabla con todas tus solicitudes:

| Columna | Descripción |
|---------|-------------|
| **Fecha** | Fecha y hora de creación de la solicitud |
| **Usuario** | Tu nombre (el comercial que creó la solicitud) |
| **Tipos Solicitados** | Badges de colores mostrando qué servicios se solicitaron |
| **Estado** | Estado actual de la solicitud |
| **Acciones** | Botones para ver detalles |

### Estados de las Solicitudes

| Estado | Significado |
|--------|-------------|
| ⚠️ **Pendiente** | Solicitud recién creada, esperando revisión del equipo |
| ℹ️ **En Proceso** | El equipo está trabajando en conseguir las ofertas |
| ✅ **Completada** | Las ofertas están listas o ya se han enviado |

### Ver Detalles de una Solicitud

1. Haz clic en el botón **"Ver"** (👁️) de cualquier solicitud
2. Se abrirá un modal con toda la información detallada:
   - Datos del interesado
   - Tipos de oferta solicitados
   - Todos los campos completados
   - Archivos adjuntos (con opción de descargar)
   - Comentarios del equipo (si los hay)
   - Archivo de respuesta (si está completada)

### Paginación y Filtrado

- **Mostrar más/menos resultados**: Selecciona 10, 25 o 50 solicitudes por página
- **Navegar entre páginas**: Usa los botones de paginación en la parte inferior
- El total de solicitudes se muestra en todo momento

---

## 💡 Consejos y Buenas Prácticas

### ✅ Recomendaciones

1. **Adjunta siempre las facturas cuando sea posible**
   - Permiten hacer comparativas reales y precisas
   - Aumentan la tasa de conversión

2. **Completa el campo de observaciones**
   - Cualquier detalle útil acelera el proceso
   - Anticipa preguntas del equipo administrativo

3. **Usa el enlace de Google Maps en fotovoltaica**
   - Es fundamental para evaluar la viabilidad del proyecto
   - Ahorra tiempo en llamadas y visitas

4. **Sé específico en "¿Qué desea?"**
   - "Busca precio más bajo" vs "Necesita fibra 1Gb para teletrabajo y gaming"
   - Cuanto más detalle, mejores ofertas

5. **Revisa antes de enviar**
   - Verifica que el email y teléfono del cliente son correctos
   - Una vez enviada, no podrás editar la solicitud por tu cuenta

### ⚠️ Limitaciones y Restricciones

- **Tamaño máximo de archivos**: 10MB por archivo
- **Formatos aceptados**: PDF, JPG, JPEG, PNG
- **No puedes borrar solicitudes**: Contacta con soporte si necesitas eliminar una solicitud
- **No puedes editar solicitudes**: Una vez enviada, contacta con el equipo si necesitas hacer cambios

---

## 🆘 Solución de Problemas

### "No se pudo guardar el archivo"

**Causas posibles:**
- El archivo supera 10MB
- No tienes permisos de escritura en el servidor
- El formato del archivo no es compatible

**Solución:**
- Comprime el PDF o reduce la calidad de la imagen
- Contacta con soporte técnico si persiste

### "Debes seleccionar al menos un tipo de oferta"

**Causa:**
- No has marcado ninguna casilla de tipo de oferta

**Solución:**
- Marca al menos un checkbox (Luz, Gas, Fibra, etc.)

### "No se pudo enviar el email"

**Causa:**
- Problema con la configuración del sistema de email

**Solución:**
- La solicitud se ha guardado correctamente en la base de datos
- El equipo administrativo podrá verla aunque no haya recibido el email
- Contacta con soporte técnico para informar del problema

### No veo el botón "Nueva Solicitud"

**Causa:**
- Puede que tu rol de usuario no tenga permisos

**Solución:**
- Contacta con soporte técnico para verificar tus permisos

---

##  Soporte

Si tienes dudas o problemas:

1. **Consulta esta guía** primero
2. **Contacta con el equipo de soporte** del sistema
3. **Revisa los emails recibidos** tras crear una solicitud

---

## 📅 Información del Documento

- **Versión**: 1.0
- **Fecha**: 13 de marzo de 2026
- **Última actualización**: 13 de marzo de 2026

---

## 🎓 Ejemplos Prácticos

### Ejemplo 1: Solicitud de Luz para Cliente Residencial

```
Tipos seleccionados: ✓ Luz

Datos del Interesado:
- Nombre: María García López
- Teléfono: 654321987
- Email: maria.garcia@gmail.com

Luz/Gas:
- Tipo de cliente: Residencial
- Factura: factura_luz_enero.pdf (adjunta)
- Observaciones: "Cliente tiene tarifa 2.0TD. Busca precio competitivo 
  con discriminación horaria. Consume unos 300kWh/mes"
```

### Ejemplo 2: Pack Completo de Telefonía

```
Tipos seleccionados: ✓ Fibra + Móvil + TV

Datos del Interesado:
- Nombre: Juan Rodríguez SL
- Teléfono: 912345678
- Email: info@juanrodriguez.com

Telefonía:
- Tipo: Portabilidad (desde Movistar)
- Factura: factura_movistar.pdf (adjunta)
- Tiene contratado: "Fibra 600Mb + 2 líneas con 100GB + Netflix"
- Desea: "Mantener mismo servicio pero reducir coste mensual. 
  Dispuesto a cambiar si las condiciones son mejores"
```

### Ejemplo 3: Fotovoltaica Completa

```
Tipos seleccionados: ✓ Luz ✓ Fotovoltaica

Datos del Interesado:
- Nombre: Pedro Martínez
- Teléfono: 600111222
- Email: pedro.m@hotmail.com

Luz/Gas:
- Tipo de cliente: Residencial
- Factura: factura_actual.pdf
- Observaciones: "Consumo elevado, ideal para autoconsumo"

Fotovoltaica:
- Factura: (la misma adjunta en Luz)
- Google Maps: https://maps.google.com/?q=40.416775,-3.703790
- Observaciones: "Tejado orientado al sur, 60m² disponibles, 
  sin sombras. Busca amortización en 7-8 años máximo"
```

---

¡Listo! Ya estás preparado para crear solicitudes de ofertas de forma profesional y eficiente. 🚀
