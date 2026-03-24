# Iconos de Contacto del Comercial en Contratos

## Descripción
Se han añadido 3 iconos junto al campo de comercial en la edición de contratos de energía que permiten contactar rápidamente con el comercial en caso de alguna incidencia.

## Iconos Disponibles

1. **📧 Email** - Abre el cliente de correo predeterminado con el email del comercial
2. **💬 WhatsApp** - Abre WhatsApp Web/App con el número del comercial
3. **📞 Teléfono** - Permite llamar directamente al teléfono del comercial

## Ubicación
Los iconos aparecen junto al campo "Comercial" en el modal de edición/detalle de un contrato de energía.

## Requisitos

### 1. Actualizar la Base de Datos
Ejecutar el script SQL `ADD_TELEFONO_USUARIOS.sql` para agregar el campo de teléfono a la tabla de usuarios:

```bash
mysql -u usuario -p enerfonecrm < ADD_TELEFONO_USUARIOS.sql
```

### 2. Agregar Teléfonos a los Usuarios
Los teléfonos de los comerciales deben ser agregados en la gestión de usuarios. El formato recomendado es:
- Con código de país: `+34612345678`
- Este formato es importante para que WhatsApp funcione correctamente

## Funcionamiento

- Los iconos solo aparecen si:
  1. El contrato tiene un comercial asignado
  2. El comercial tiene datos de contacto en su perfil

- **Icono de Email**: Solo aparece si el usuario tiene email configurado
- **Iconos de WhatsApp y Teléfono**: Solo aparecen si el usuario tiene teléfono configurado

## Comportamiento de los Iconos

1. **Email**: Al hacer clic se abre el cliente de correo predeterminado con una nueva ventana dirigida al email del comercial

2. **WhatsApp**: Al hacer clic se abre WhatsApp (Web o App) en una nueva pestaña con una conversación con el número del comercial

3. **Teléfono**: Al hacer clic se inicia una llamada telefónica (en móviles) o se muestra el número para copiar (en escritorio)

## Información de Contacto
Al pasar el cursor sobre cada icono, se muestra un tooltip con:
- Email del comercial
- Teléfono del comercial (para los iconos de WhatsApp y teléfono)

## Estilos
- Icono de Email: Azul (`btn-outline-primary`)
- Icono de WhatsApp: Verde (`btn-outline-success`)
- Icono de Teléfono: Gris (`btn-outline-secondary`)
