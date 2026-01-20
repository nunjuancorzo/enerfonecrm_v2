# Manual de Usuario - Enerfone CRM

## Índice
1. [Introducción](#introducción)
2. [Acceso al Sistema](#acceso-al-sistema)
3. [Gestión de Clientes](#gestión-de-clientes)
4. [Gestión de Contratos](#gestión-de-contratos)
   - [Contratos de Energía](#contratos-de-energía)
   - [Contratos de Telefonía](#contratos-de-telefonía)
   - [Contratos de Alarmas](#contratos-de-alarmas)
5. [Gestión de Tarifas](#gestión-de-tarifas)
6. [Activaciones de Contratos](#activaciones-de-contratos)

---

## Introducción

Enerfone CRM es una aplicación web diseñada para la gestión integral de clientes y contratos en los sectores de energía, telefonía y alarmas. La aplicación permite registrar clientes, crear diferentes tipos de contratos, hacer seguimiento del estado de los mismos, gestionar documentación adjunta y mantener un histórico completo de observaciones.

---

## Acceso al Sistema

1. Accede a la URL de la aplicación a través de tu navegador web
2. Introduce tu nombre de usuario y contraseña
3. Haz clic en "Iniciar Sesión"

**Roles de usuario:**
- **Administrador**: Acceso completo a todas las funcionalidades
- **Comercial**: Acceso a sus propios clientes y contratos
- **Usuario Comercializadora**: Acceso limitado a contratos de su comercializadora

---

## Gestión de Clientes

### Listado de Clientes
- En el menú principal, selecciona "Clientes"
- Verás un listado de todos los clientes registrados
- Puedes filtrar por nombre, teléfono, email, DNI, IBAN, etc.
- Los filtros se aplican automáticamente al escribir

### Crear Nuevo Cliente

1. Haz clic en el botón "Nuevo Cliente" (esquina superior derecha)
2. Rellena el formulario con los siguientes campos:

**Datos Básicos Obligatorios:**
- Tipo de Cliente: Particular o Pyme
- Nombre completo
- DNI/CIF
- Teléfono
- Email
- IBAN
- Fecha de Alta

**Documentación Adjunta:**
Según el tipo de cliente, podrás adjuntar:
- **Particular**: DNI (anverso y reverso)
- **Pyme**: DNI, CIF, Escrituras de constitución

3. Haz clic en "Guardar" para crear el cliente

### Editar Cliente

1. En el listado de clientes, haz clic en el botón de "Editar" (icono de lápiz) del cliente deseado
2. Modifica los campos necesarios
3. Puedes agregar, descargar o eliminar documentación adjunta
4. Haz clic en "Guardar" para aplicar los cambios

### Crear Contratos desde la Ficha del Cliente

Desde la ficha de edición de un cliente, puedes crear directamente contratos asociados:
- **Contrato de Energía**: Botón "Crear Contrato Energía"
- **Contrato de Telefonía**: Botón "Crear Contrato Telefonía"  
- **Contrato de Alarmas**: Botón "Crear Contrato Alarmas"

---

## Gestión de Contratos

### Contratos de Energía

#### Acceso
Selecciona "Contratos Energía" en el menú principal

#### Crear Contrato de Energía

**Opción 1: Desde la página de Contratos Energía**
1. Haz clic en "Nuevo Contrato"
2. Selecciona el cliente asociado

**Opción 2: Desde la ficha del Cliente**
1. Abre la ficha del cliente
2. Haz clic en "Crear Contrato Energía"
3. El cliente ya estará preseleccionado

**Campos Obligatorios:**
- **Tipo de Operación**: Cambio comercializadora, CC con cambio titularidad, o Alta nueva
- **Comercializadora**: Empresa suministradora de energía
- **Tarifa**: Tarifa contratada (se filtra según la comercializadora seleccionada)
- **Dirección de Suministro**:
  - Dirección (nombre de la calle)
  - Número
  - Código Postal
  - Provincia
  - Localidad

**Campos Opcionales:**
- Estado del contrato
- Estado del servicio
- CUPS Luz
- CUPS Gas
- DNI
- Potencia contratada (kW)
- Consumo anual (kWh)
- Servicio asociado
- Campos de dirección complementarios: Escalera, Piso, Puerta, Aclarador
- Usuario de Comercializadora (para asignación interna)

**Titular IBAN Diferente:**
Si el titular del IBAN es diferente al cliente:
1. Marca la casilla "El titular del IBAN es diferente al cliente"
2. Rellena los datos del titular: DNI, Nombre completo, Número IBAN

**Documentación:**
- Puedes adjuntar el PDF del contrato firmado
- Los archivos del cliente (DNI, CIF, etc.) se muestran en modo lectura
- Puedes adjuntar facturas del contrato (PDF, JPG, PNG, DOC, DOCX)

**Observaciones:**
- Puedes agregar observaciones con contexto sobre el contrato
- El histórico de observaciones queda registrado con fecha, hora y usuario
- Solo puedes eliminar tus propias observaciones

#### Editar Contrato de Energía

1. En el listado, haz clic en "Editar" (icono de lápiz)
2. **Permisos de edición**:
   - Administradores: Pueden editar todos los campos
   - Comerciales: Solo pueden editar Estado y Observaciones de sus contratos
   - Usuario Comercializadora: Solo pueden ver contratos asignados

3. **Cambio de Estado a "Activo"**:
   - Al cambiar el estado a "Activo", se solicita:
     - Fecha de activación (obligatoria)
     - Observaciones (opcional)
   - Esto registra la activación en el log de activaciones

4. **Descargar Documentación**:
   - Puedes descargar archivos del cliente y facturas del contrato
   - Los botones de descarga NO activan validaciones del formulario

5. Haz clic en "Guardar" para aplicar cambios

#### Filtros Avanzados

- **Por Cliente**: Nombre, teléfono, email
- **Por Datos del Contrato**: Comercializadora, tarifa, CUPS, DNI
- **Por Estado**: Múltiples estados disponibles
- **Por Comercial**: Solo administradores
- **Búsqueda General**: Campo de texto que busca en múltiples campos

#### Exportar Datos

- **Excel Completo**: Exporta todos los contratos filtrados con todos los campos
- **Excel Reducido**: Exporta campos principales para envío a comercializadoras

#### Operaciones Masivas

1. Selecciona varios contratos mediante las casillas
2. Haz clic en "Cambio de Estado Masivo"
3. Selecciona el nuevo estado
4. Confirma la operación

---

### Contratos de Telefonía

#### Acceso
Selecciona "Contratos Telefonía" en el menú principal

#### Crear Contrato de Telefonía

**Campos Obligatorios:**
- **Operadora**: Compañía telefónica
- **Tipo de Tarifa**: Convergente, Solo móvil, Solo fibra, etc.
- **Tarifa Principal**: Tarifa contratada (filtrada por operadora)
- **Dirección de Instalación**:
  - Dirección
  - Número
  - Código Postal
  - Provincia
  - Localidad

**Líneas de Teléfono:**
- **Línea Principal**: Fijo o móvil principal
  - Si es móvil, se puede indicar tipo (Contrato/Prepago)
  - Código ICC (opcional)
- **Líneas Adicionales**: Hasta 5 líneas adicionales
  - Para cada línea: número, tarifa, tipo y código ICC

**Instalación:**
- Fecha de instalación
- Horario de instalación
- Servicios adicionales a contratar

**Documentación:**
- Facturas del contrato
- Documentación del cliente (solo lectura)

**Observaciones:**
- Histórico completo con fecha, hora y usuario

#### Editar Contrato de Telefonía

Funcionamiento similar a contratos de energía:
- Permisos según rol
- Cambio de estado a "Activo" registra activación
- Gestión de documentación adjunta
- Histórico de observaciones

---

### Contratos de Alarmas

#### Acceso
Selecciona "Contratos Alarmas" en el menú principal

#### Crear Contrato de Alarmas

**Campos Obligatorios:**
- **Empresa de Alarma**: Compañía de seguridad
- **Tipo de Inmueble**: Hogar o Negocio
- **Subtipo**: Piso, Bajo, Chalet, Adosado, Nave, Local, Oficina
- **Kit de Alarma**: Kit contratado (filtrado por empresa)
- **Campaña**: Campaña comercial aplicable
- **Dirección de Instalación**:
  - Dirección
  - Número
  - Código Postal
  - Provincia
  - Localidad

**Contrato Anterior:**
Si el cliente tiene contrato de alarma anterior:
1. Marca "Tiene contrato anterior"
2. Indica:
   - Compañía anterior
   - Número de contrato anterior
   - Fecha de permanencia

**Opcionales de Alarma:**
- Equipamiento adicional o sensores extra
- Observaciones específicas del servicio

**Documentación:**
- Facturas del contrato
- Documentación del cliente (solo lectura)

**Observaciones:**
- Histórico completo con seguimiento de incidencias

#### Editar Contrato de Alarmas

Funcionamiento similar a otros tipos de contrato:
- Control de permisos por rol
- Registro de activaciones
- Gestión de documentación
- Observaciones con histórico

---

## Gestión de Tarifas

### Tarifas de Energía

1. Accede a "Tarifas Energía"
2. **Crear Nueva Tarifa**:
   - Nombre de la tarifa
   - Comercializadora asociada
   - Precios de energía (P1, P2, P3, P4, P5, P6)
   - Precios de potencia (P1, P2, P3, P4, P5, P6)
   - Servicios incluidos
3. **Editar/Eliminar**: Usa los botones correspondientes en el listado

### Tarifas de Telefonía

Similar a tarifas de energía:
- Asociadas a operadora
- Tipo de tarifa
- Precio mensual
- Servicios incluidos

### Tarifas de Alarmas

- Asociadas a empresa de alarma
- Kit de alarma
- Precio de instalación
- Cuota mensual

---

## Activaciones de Contratos

### Acceso al Log de Activaciones

1. Desde "Contratos Energía", haz clic en "Ver Activaciones" (botón superior derecho)
2. Se muestra un listado completo de todas las activaciones registradas

### Información Mostrada

- **Contrato**: Identificador y tipo
- **Cliente**: Nombre del cliente
- **Comercializadora/Operadora/Empresa**: Según tipo de contrato
- **Fecha de Activación**: Fecha en que se activó el servicio
- **Usuario**: Quien registró la activación
- **Observaciones**: Notas sobre la activación
- **Fecha de Registro**: Cuándo se guardó en el sistema

### Filtros Disponibles

- **Por fechas**: Desde/Hasta
- **Por comercial**: Solo para administradores
- **Por tipo de contrato**: Energía, Telefonía, Alarmas
- **Búsqueda general**: En nombre de cliente, observaciones, etc.

### Exportar Activaciones

Botón "Exportar a Excel" genera un archivo con todas las activaciones filtradas.

---

## Consejos y Buenas Prácticas

1. **Valida los datos antes de guardar**: Los campos obligatorios están marcados con asterisco (*)

2. **Usa observaciones**: Documenta cualquier incidencia o información relevante en el histórico de observaciones

3. **Adjunta documentación**: Mantén toda la documentación del contrato actualizada y accesible

4. **Revisa antes de activar**: Al cambiar un contrato a estado "Activo", asegúrate de introducir la fecha correcta de activación

5. **Filtra para encontrar**: Usa los filtros para localizar rápidamente contratos o clientes específicos

6. **Exporta datos regularmente**: Los administradores pueden exportar datos para análisis o backup

7. **Descarga sin miedo**: Los botones de descarga de archivos no requieren que el formulario esté completo

---

## Soporte Técnico

Para cualquier duda o incidencia con la aplicación, contacta con el administrador del sistema.

**Versión del documento**: 1.0  
**Fecha**: Enero 2026
