# Módulo de Solicitudes de Ofertas - Instalación

## 📋 Descripción

Este módulo permite a los comerciales solicitar ofertas de diferentes servicios (luz, gas, fotovoltaica, fibra, móvil, alarmas) directamente desde el CRM. Las solicitudes se almacenan en la base de datos y se envía un email automático al administrador.

## 🛠️ Instalación

### 1. Crear la tabla en la base de datos

⚠️ **IMPORTANTE**: Antes de usar el módulo, debes crear la tabla `solicitudes_ofertas` en tu base de datos.

#### Opción A: Usando el script de instalación (recomendado)

```bash
cd /Users/juanmariacorzo/Documents/CorCRM/EnerfoneCRMv2
./instalar_modulo_ofertas.sh
```

El script te pedirá las credenciales de MySQL y ejecutará automáticamente el SQL.

#### Opción B: Ejecutar SQL manualmente

Si el script no funciona, ejecuta el archivo SQL manualmente usando uno de estos métodos:

**Desde línea de comandos:**
```bash
/usr/local/mysql-9.1.0-macos14-arm64/bin/mysql -h localhost -P 3306 -u TU_USUARIO -p enerfone_pre < ADD_SOLICITUDES_OFERTAS.sql
```

**Desde un cliente gráfico** (phpMyAdmin, MySQL Workbench, TablePlus, etc.):
1. Abre el archivo `ADD_SOLICITUDES_OFERTAS.sql`
2. Conéctate a la base de datos `enerfone_pre`
3. Ejecuta el contenido del archivo SQL

#### Opción C: Copiar y pegar el SQL

Abre el archivo `ADD_SOLICITUDES_OFERTAS.sql` y copia su contenido en tu cliente MySQL favorito.

### 2. Verificar carpetas de almacenamiento

Las carpetas para almacenar archivos adjuntos ya se han creado automáticamente en:
- `storage/ofertas/luz-gas/`
- `storage/ofertas/fotovoltaica/`
- `storage/ofertas/telefonia/`

### 3. Reiniciar la aplicación

Después de crear la tabla, reinicia la aplicación:

```bash
cd EnerfoneCRM
dotnet run
```

## 📖 Uso

### Para Comerciales

1. Navega a **Gestiones > Ofertas**
2. Haz clic en **Nueva Solicitud**
3. Selecciona los tipos de oferta que necesitas (puedes seleccionar múltiples)
4. Completa los formularios específicos que aparecen según tu selección:
   - **Luz/Gas**: Adjunta la última factura y añade observaciones
   - **Fotovoltaica**: Adjunta factura, enlace de Google Maps de la ubicación y observaciones
   - **Telefonía (Fibra/Móvil)**: Indica si es alta nueva o portabilidad, adjunta factura si es portabilidad, indica qué tiene contratado y qué desea
   - **Alarma**: Indica si es negocio o residencial, si tiene alarma actualmente y observaciones
5. Haz clic en **Enviar Solicitud**

### Notificación

- El administrador recibirá automáticamente un email con todos los detalles de la solicitud
- Puedes ver el historial de tus solicitudes en la misma página
- El estado de cada solicitud se actualiza: Pendiente → En Proceso → Completada

## 🔧 Características

### Tipos de Oferta Disponibles
- ⚡ Luz
- 🔥 Gas
- ☀️ Fotovoltaica
- 📡 Fibra
- 📱 Móvil
- 🌐 Fibra + Móvil
- 📺 Fibra + Móvil + TV
- 🚨 Alarma

### Funcionalidades
- ✅ Formularios dinámicos según el tipo de oferta seleccionado
- ✅ Adjuntar facturas (PDF, JPG, PNG) hasta 10MB
- ✅ Email automático al administrador con todos los detalles
- ✅ Historial de solicitudes por usuario
- ✅ Estados de seguimiento
- ✅ Visualización detallada de cada solicitud

## 📧 Configuración de Email

Asegúrate de que la configuración SMTP esté correctamente configurada en **Configuración > Datos de Empresa** para que los emails se envíen correctamente al administrador.

## 🗂️ Estructura de Archivos Creados

- `EnerfoneCRM/Models/SolicitudOferta.cs` - Modelo de datos
- `EnerfoneCRM/Services/OfertaService.cs` - Lógica de negocio
- `EnerfoneCRM/Components/Pages/Ofertas.razor` - Interfaz de usuario
- `ADD_SOLICITUDES_OFERTAS.sql` - Script de creación de tabla
- `storage/ofertas/` - Carpetas de almacenamiento

## 🔄 Actualizaciones en Archivos Existentes

- `EnerfoneCRM/Data/ApplicationDbContext.cs` - Agregado DbSet<SolicitudOferta>
- `EnerfoneCRM/Program.cs` - Registrado OfertaService
- `EnerfoneCRM/Components/Layout/NavMenu.razor` - Enlace al módulo de ofertas en el menú

---

**Fecha de implementación**: 23 de febrero de 2026
