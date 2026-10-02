# Instrucciones - Modificación Módulo de Ofertas

## Cambios Realizados

Se ha modificado el módulo de ofertas para incluir:

1. **Datos del Interesado**: Nombre, Teléfono y Email del cliente final
2. **Tipo de Cliente**: Selector Residencial/PYME para ofertas de Luz/Gas
3. **Actualización de Emails**: Los correos ahora incluyen los nuevos campos

## Archivos Modificados

### 1. Modelo de Datos
- **Archivo**: `EnerfoneCRM/Models/SolicitudOferta.cs`
- **Cambios**: 
  - Agregados campos `NombreInteresado`, `TelefonoInteresado`, `EmailInteresado`
  - Agregado campo `LuzGasTipoCliente` (Residencial/PYME)

### 2. Formulario de Ofertas
- **Archivo**: `EnerfoneCRM/Components/Pages/Ofertas.razor`
- **Cambios**:
  - Nueva sección "Datos del Interesado" con 3 campos obligatorios
  - Selector de Tipo de Cliente en la sección de Luz/Gas
  - Validaciones agregadas para los nuevos campos
  - Modal de detalle actualizado para mostrar la nueva información

### 3. Servicio de Email
- **Archivo**: `EnerfoneCRM/Services/OfertaService.cs`
- **Cambios**:
  - Email al administrador ahora incluye datos del interesado
  - Email incluye tipo de cliente (Residencial/PYME) para Luz/Gas

## Pasos para Aplicar los Cambios

### 1. Ejecutar Script SQL
**IMPORTANTE**: Debes ejecutar el script SQL antes de usar la aplicación modificada.

```bash
mysql -u tu_usuario -p corcrmv2 < ADD_CAMPOS_INTERESADO_OFERTAS.sql
```

O ejecutar manualmente en tu cliente MySQL:
```sql
USE corcrmv2;

ALTER TABLE solicitudes_ofertas
ADD COLUMN nombre_interesado VARCHAR(200) NULL COMMENT 'Nombre completo del interesado' AFTER email_comercial,
ADD COLUMN telefono_interesado VARCHAR(20) NULL COMMENT 'Teléfono del interesado' AFTER nombre_interesado,
ADD COLUMN email_interesado VARCHAR(100) NULL COMMENT 'Email del interesado' AFTER telefono_interesado;

ALTER TABLE solicitudes_ofertas
ADD COLUMN luz_gas_tipo_cliente VARCHAR(50) NULL COMMENT 'Tipo de cliente: Residencial o PYME' AFTER tipo_alarma;
```

### 2. Reiniciar la Aplicación
Una vez ejecutado el script SQL, reinicia la aplicación:

```bash
cd EnerfoneCRM
dotnet run
```

## Características del Formulario Actualizado

### Datos del Interesado (Obligatorio)
Al abrir el formulario de nueva solicitud de oferta, se debe completar:
- **Nombre del Interesado** *: Nombre completo del cliente
- **Teléfono** *: Teléfono de contacto 
- **Email** *: Correo electrónico del cliente

### Para Ofertas de Luz/Gas
Ahora incluye:
- **Tipo de Cliente** * (obligatorio): Selector radio button con opciones:
  - 🏠 Residencial
  - 🏢 PYME
- **Última Factura**: Upload de archivo (opcional)
- **Observaciones**: Campo de texto libre

### Para Fotovoltaica
- **Factura Actual**: Upload de archivo (opcional)
- **Enlace de Google Maps**: Campo para pegar URL de ubicación
- **Observaciones**: Campo de texto libre

### Para Telefonía
Mantiene la funcionalidad actual:
- Tipo de solicitud (Alta Nueva / Portabilidad)
- Factura anterior (si es portabilidad)
- Lo que tiene contratado actualmente
- Observaciones sobre lo que desea contratar

## Validaciones

El formulario valida que:
1. Se haya seleccionado al menos un tipo de oferta
2. Los datos del interesado estén completos (Nombre, Teléfono y Email)
3. Si se selecciona Luz o Gas, se debe elegir el tipo de cliente (Residencial/PYME)

## Email al Administrador

Cuando se envía una solicitud, el administrador recibe un email con:
- Información del comercial que crea la solicitud
- **NUEVO**: Datos del interesado (Nombre, Teléfono y Email)
- Tipos de oferta solicitados
- **NUEVO**: Tipo de cliente (Residencial/PYME) para Luz/Gas
- Detalles específicos de cada tipo de oferta seleccionado

## Visualización de Solicitudes

En el modal de detalle de cada solicitud ahora se muestra:
- Sección "Datos del Interesado" con nombre, teléfono y email
- En la sección de Luz/Gas se muestra el tipo de cliente seleccionado

## Notas Adicionales

- Los campos nuevos permiten valores NULL para compatibilidad con solicitudes antiguas
- Las solicitudes creadas antes de este cambio no tendrán estos campos completados
- El valor por defecto para nuevas solicitudes de Luz/Gas es "Residencial"

## Soporte

Si encuentras algún problema con los cambios, verifica:
1. Que el script SQL se haya ejecutado correctamente
2. Que no haya errores en la consola del navegador
3. Que la aplicación se haya reiniciado después de los cambios
