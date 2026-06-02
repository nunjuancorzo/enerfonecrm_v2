# DOCUMENTACIÓN TÉCNICA - CorCRM
## Sistema de Gestión de Relaciones con Clientes para Sector Energético y Telecomunicaciones

**Versión**: 2.0  
**Fecha**: 1 de junio de 2026  
**Tipo de Documento**: Documentación Técnica para Registro de Patente

---

## ÍNDICE

1. [Resumen Ejecutivo](#resumen-ejecutivo)
2. [Descripción General del Sistema](#descripción-general-del-sistema)
3. [Arquitectura Técnica](#arquitectura-técnica)
4. [Innovaciones Técnicas Patentables](#innovaciones-técnicas-patentables)
5. [Módulos Funcionales](#módulos-funcionales)
6. [Modelo de Datos](#modelo-de-datos)
7. [Flujos de Trabajo](#flujos-de-trabajo)
8. [Integraciones](#integraciones)
9. [Seguridad y Control de Acceso](#seguridad-y-control-de-acceso)
10. [Características Diferenciadoras](#características-diferenciadoras)

---

## 1. RESUMEN EJECUTIVO

**CorCRM** es un sistema integral de gestión de relaciones con clientes (CRM) especializado en el sector energético y de telecomunicaciones, que incorpora funcionalidades avanzadas de:

- **Gestión multiservicio** (electricidad, gas, telefonía, alarmas, fotovoltaica)
- **Sistema de comisiones jerárquicas multinivel** con cálculo automático
- **Integración con sistemas externos** (SIPS, Revolapps)
- **Comparador inteligente de tarifas** con análisis de ahorro
- **Sistema de liquidaciones automatizado** con distribución jerárquica
- **Gestión documental integrada** con generación automática de PDFs
- **Módulo de ofertas personalizadas** multi-servicio
- **Control de jerarquías organizacionales** complejas (hasta 5 niveles)

---

## 2. DESCRIPCIÓN GENERAL DEL SISTEMA

### 2.1. Propósito

CorCRM es una solución empresarial diseñada para gestionar integralmente el ciclo completo de ventas, comisiones y liquidaciones en empresas comercializadoras de servicios energéticos y de telecomunicaciones, con especial énfasis en la gestión jerárquica de equipos comerciales.

### 2.2. Alcance Funcional

El sistema cubre:
- **Gestión de Clientes**: Registro completo con datos de contacto, documentación y procedencia
- **Gestión de Contratos**: Multi-servicio (luz, gas, telefonía, alarmas, fotovoltaica)
- **Gestión de Tarifas**: Catálogo completo con precios dinámicos por periodo
- **Comisiones**: Cálculo automático con distribución jerárquica multinivel
- **Liquidaciones**: Proceso automatizado con aprobación y seguimiento
- **Incidencias**: Sistema de tickets vinculado a contratos
- **Ofertas**: Generación y seguimiento de propuestas comerciales
- **Comparador**: Análisis comparativo de tarifas del mercado
- **Reportes**: Generación de documentos PDF (liquidaciones, comparativas, SIPS)

### 2.3. Usuarios del Sistema

El sistema contempla los siguientes roles jerárquicos:

1. **Administrador**: Control total del sistema
2. **Backoffice**: Gestión operativa sin comisiones
3. **Director Comercial**: Supervisión de jefes de ventas
4. **Jefe de Ventas**: Coordinación de gestores
5. **Gestor**: Supervisión de colaboradores
6. **Colaborador**: Captación y registro de contratos

---

## 3. ARQUITECTURA TÉCNICA

### 3.1. Stack Tecnológico

**Frontend**:
- **Framework**: Blazor Server (.NET 8.0)
- **UI Components**: Bootstrap 5, Blazorise
- **JavaScript**: Interoperabilidad para funciones específicas (descarga archivos, gráficos)

**Backend**:
- **Framework**: ASP.NET Core 8.0
- **Lenguaje**: C# 12
- **ORM**: Entity Framework Core 8.0
- **Base de Datos**: MySQL 8.0+

**Servicios**:
- **Generación PDF**: wkhtmltopdf
- **Correo Electrónico**: SMTP configurable
- **Cache**: IMemoryCache (in-memory)
- **HTTP Client**: HttpClient para integraciones

### 3.2. Patrones de Diseño Implementados

#### 3.2.1. Arquitectura en Capas

```
┌──────────────────────────────────────┐
│   PRESENTACIÓN (Blazor Components)   │
├──────────────────────────────────────┤
│   SERVICIOS (Business Logic)         │
├──────────────────────────────────────┤
│   DATOS (Entity Framework Core)      │
├──────────────────────────────────────┤
│   BASE DE DATOS (MySQL)              │
└──────────────────────────────────────┘
```

#### 3.2.2. Inyección de Dependencias

Todos los servicios se registran mediante el contenedor DI de .NET:

```csharp
- ClienteService
- ContratoService
- UsuarioService
- ComisionService
- LiquidacionService
- TarifaEnergiaService
- TarifaTelefoniaService
- TarifaAlarmaService
- SipsService
- OfertaService
- PdfLiquidacionService
- PdfComparadorService
- EmailService
- FicheroClienteService
- FicheroContratoService
```

#### 3.2.3. Repository Pattern

El `DbContextProvider` implementa un patrón Factory para crear contextos de base de datos:

```csharp
public class DbContextProvider
{
    public ApplicationDbContext CreateDbContext()
    {
        // Crea contexto con configuración dinámica
        // Gestiona la cadena de conexión según el entorno
    }
}
```

### 3.3. Estructura de la Base de Datos

**Tablas Principales** (35 tablas):

1. **Gestión de Usuarios**:
   - `usuarios`: Información de usuarios del sistema
   - `log_accesos`: Auditoría de accesos
   
2. **Gestión de Clientes**:
   - `clientes_simple`: Datos de clientes
   - `ficheros_clientes`: Documentos adjuntos

3. **Gestión de Contratos**:
   - `contratos`: Contratos multi-servicio
   - `ficheros_contratos`: Documentación de contratos
   - `observaciones_contratos`: Anotaciones históricas
   - `log_activacion_contratos`: Trazabilidad de activaciones

4. **Tarifas**:
   - `tarifasenergia`: Tarifas de luz y gas
   - `tarifastelefonia`: Tarifas de telefonía
   - `tarifas_alarmas`: Tarifas de alarmas

5. **Proveedores**:
   - `comercializadoras`: Compañías de energía
   - `operadoras`: Operadores de telefonía
   - `empresas_alarma`: Empresas de seguridad

6. **Comisiones y Liquidaciones**:
   - `historico_liquidacion`: Liquidaciones aprobadas
   - `detalle_comision_liquidacion`: Distribución jerárquica
   - `decomisiones`: Penalizaciones
   - `configuracion_comisiones`: Porcentajes personalizados
   - `usuario_comision_proveedores`: Comisiones por proveedor/usuario
   - `incidencias_liquidacion`: Incidencias en liquidaciones

7. **Ofertas**:
   - `solicitudes_ofertas`: Solicitudes de ofertas
   - `ofertas_interesados`: Contactos interesados

8. **Sistema SIPS**:
   - `historico_sips_consultas`: Cache de consultas SIPS

9. **Configuración**:
   - `configuracion_empresa`: Datos de la empresa
   - `mensajes_bienvenida`: Mensajes personalizados
   - `tareas_pendientes`: Gestión de tareas

10. **Relaciones N:M**:
    - `usuario_comercializadoras`: Usuarios autorizados por comercializadora
    - `usuario_operadoras`: Usuarios autorizados por operadora
    - `usuario_empresas_alarma`: Usuarios autorizados por empresa de alarma

---

## 4. INNOVACIONES TÉCNICAS PATENTABLES

### 4.1. Sistema de Comisiones Jerárquicas Multinivel

**INNOVACIÓN PRINCIPAL**: Sistema de distribución automática de comisiones en cascada con hasta 5 niveles jerárquicos, con cálculo dinámico basado en configuración por usuario y proveedor.

#### Características Únicas:

1. **Jerarquía en Cascada Automática**:
   ```
   Colaborador (70%)
       └─> Gestor (+10% = 80% total)
           └─> Jefe de Ventas (+10% = 90% total)
               └─> Director Comercial (+5% = 95% total)
                   └─> Administrador (100%)
   ```

2. **Configuración Personalizada por Usuario y Proveedor**:
   - Cada usuario puede tener porcentajes diferentes según el proveedor
   - Tabla: `configuracion_comisiones` y `usuario_comision_proveedores`
   - Permite diferenciar comisiones por comercializadora/operadora/empresa

3. **Cálculo Diferenciado**:
   - **Colaborador**: Recibe % sobre su comisión asignada
   - **Superiores jerárquicos**: Reciben % sobre la comisión base de la tarifa
   - **Administrador**: Recibe el 100% menos lo distribuido

4. **Detalle Granular por Contrato**:
   - Cada contrato registra la distribución completa en `detalle_comision_liquidacion`
   - Campos: colaborador_id, comision_colaborador, gestor_id, comision_gestor, jefe_ventas_id, comision_jefe_ventas, director_comercial_id, comision_director_comercial, administrador_id, comision_administrador
   - Permite auditoría completa y trazabilidad

5. **Liquidaciones Individualizadas**:
   - Al aprobar una liquidación, se generan liquidaciones individuales para cada nivel jerárquico
   - Cada usuario ve solo sus liquidaciones y las de sus subordinados
   - Tabla: `historico_liquidacion` con vinculación al usuario

#### Algoritmo de Cálculo:

```
PARA CADA contrato EN liquidación:
    1. Obtener colaborador del contrato
    2. Obtener jerarquía completa (gestor, jefe, director)
    3. Buscar configuración específica (usuario + proveedor)
    4. Si no existe, usar porcentajes por defecto
    5. Calcular comisión_base_tarifa
    6. Calcular distribución:
       - colaborador: comision_contrato (ya tiene % aplicado)
       - gestor: comision_base * porcentaje_gestor / 100
       - jefe: comision_base * porcentaje_jefe / 100
       - director: comision_base * porcentaje_director / 100
       - administrador: comision_base * porcentaje_restante / 100
    7. Guardar en detalle_comision_liquidacion
    8. Acumular totales por usuario
    9. Crear historico_liquidacion individual para cada usuario con comisión > 0
```

### 4.2. Sistema de Cache Inteligente para Consultas SIPS

**INNOVACIÓN**: Sistema de cache multi-nivel con control de cuotas mensuales para optimizar consultas a API externa de suministros.

#### Características:

1. **Cache de 3 Niveles**:
   - **Memoria**: IMemoryCache (60 minutos por defecto)
   - **Base de Datos**: Tabla `historico_sips_consultas` (configurable días)
   - **Negative Cache**: Evita reconsultar CUPS con errores 4xx (24 horas)

2. **Control de Cuota Mensual**:
   - Configuración: `Sips:MonthlyQuota` en appsettings.json
   - Contador automático de consultas realizadas en el mes actual
   - Bloqueo preventivo al alcanzar el límite

3. **Optimización de Consultas**:
   - **Electricidad**: Dos llamadas combinadas (cliente + consumos históricos)
   - **Gas**: Dos llamadas combinadas (suministro + consumos históricos)
   - Retry automático con sufijo "0F" para CUPS de luz

4. **Almacenamiento Completo**:
   ```sql
   CREATE TABLE historico_sips_consultas (
       id BIGINT AUTO_INCREMENT PRIMARY KEY,
       cups VARCHAR(50) NOT NULL,
       usuario_id INT,
       usuario_nombre VARCHAR(255),
       usuario_email VARCHAR(255),
       fecha_consulta DATETIME NOT NULL,
       success BOOLEAN NOT NULL,
       http_status_code INT,
       error_message TEXT,
       response_json MEDIUMTEXT,
       es_gas BOOLEAN DEFAULT FALSE,
       INDEX idx_cups (cups),
       INDEX idx_fecha_consulta (fecha_consulta),
       INDEX idx_usuario (usuario_id)
   )
   ```

5. **Auditoría Completa**:
   - Registro de usuario que realizó la consulta
   - Timestamp de la consulta
   - Respuesta completa almacenada en JSON
   - Estado de éxito/error
   - Código HTTP de respuesta

### 4.3. Comparador Inteligente de Tarifas

**INNOVACIÓN**: Motor de comparación de tarifas energéticas con cálculo automático de ahorro basado en datos reales del suministro.

#### Características:

1. **Autocompletado desde SIPS**:
   - Ingreso de CUPS
   - Consulta automática a sistema SIPS
   - Autocompletado de:
     * Potencias contratadas (P1, P2, P3)
     * Consumos históricos por periodo
     * Tipo de tarifa (2.0TD, 3.0TD, 6.1TD)
     * Compañía actual

2. **Cálculo Preciso de Costes**:
   ```
   Coste Total Anual = 
       (Término Fijo Diario × 365) +
       Σ(Precio Potencia Pi × Potencia Pi × 365) +
       Σ(Precio Energía Pi × Consumo Pi)
   ```

3. **Comparativa Multi-tarifa**:
   - Ranking automático de tarifas (más económica a más cara)
   - Cálculo de ahorro mensual y anual
   - Desglose detallado por conceptos

4. **Generación de PDF Comparativo**:
   - Documento profesional con logo de empresa
   - Gráficos comparativos
   - Tabla detallada con ahorros
   - Desglose de costes por tarifa

### 4.4. Sistema de Liquidaciones con Aprobación Jerárquica

**INNOVACIÓN**: Flujo automatizado de aprobación de liquidaciones con generación de documentos PDF individualizados y control de estados.

#### Características:

1. **Aprobación Selectiva**:
   - El administrador ve todos los usuarios con contratos facturables
   - Selección múltiple de usuarios para liquidar
   - Agrupación automática de contratos por tipo (energía, telefonía, alarmas)

2. **Generación Automática de Liquidaciones Individuales**:
   - Una liquidación por cada usuario con comisión > 0
   - Incluso si el usuario no registró contratos directamente
   - Incluye comisiones recibidas como superior jerárquico

3. **PDF Personalizado por Usuario**:
   - Logo de empresa
   - Datos del usuario
   - Resumen de comisiones por tipo de servicio
   - Detalle completo de contratos
   - Tabla de comisiones recibidas como superior
   - Total a liquidar

4. **Control de Estados**:
   - **Pendiente**: Liquidación creada, esperando aceptación del usuario
   - **Aceptada**: Usuario confirmó la liquidación
   - **Pagada**: Administrador marcó como pagada

5. **Histórico Completo**:
   - Todas las liquidaciones se almacenan permanentemente
   - Filtros por fecha, usuario, estado
   - Auditoría de quién aprobó y cuándo

6. **Incidencias Vinculadas**:
   - Sistema de tickets asociado a liquidaciones
   - Comentarios y seguimiento
   - Estados: Abierta, En Proceso, Resuelta, Cerrada

### 4.5. Gestión de Decomisiones (Penalizaciones)

**INNOVACIÓN**: Sistema de registro y control de penalizaciones con impacto automático en liquidaciones.

#### Características:

1. **Vinculación Directa a Contratos**:
   - Cada decomisión se asocia a un contrato específico
   - Razón de la penalización
   - Importe a descontar

2. **Impacto en Liquidaciones**:
   - Las decomisiones se restan automáticamente del total
   - Visibilidad en el PDF de liquidación
   - Detalle de contratos penalizados

3. **Auditoría**:
   - Usuario que registró la decomisión
   - Fecha de registro
   - Motivo detallado

### 4.6. Sistema de Ofertas Multi-servicio

**INNOVACIÓN**: Módulo de solicitud de ofertas con formularios dinámicos según el tipo de servicio.

#### Características:

1. **Selección Múltiple de Servicios**:
   - Luz
   - Gas
   - Fotovoltaica
   - Fibra
   - Móvil
   - Fibra + Móvil
   - Fibra + Móvil + TV
   - Alarma

2. **Formularios Dinámicos**:
   - Aparecen campos específicos según el servicio seleccionado
   - Luz/Gas: Adjuntar factura
   - Fotovoltaica: Factura + enlace Google Maps
   - Telefonía: Tipo de operación (alta/portabilidad) + detalles
   - Alarma: Tipo (negocio/residencial) + situación actual

3. **Gestión Documental**:
   - Almacenamiento organizado por tipo de servicio
   - Carpetas: `storage/ofertas/luz-gas/`, `fotovoltaica/`, `telefonia/`
   - Validación de formatos (PDF, JPG, PNG)
   - Límite de tamaño (10MB)

4. **Notificación Automática**:
   - Email al administrador con todos los detalles
   - Incluye enlaces a archivos adjuntos
   - Datos del comercial y cliente interesado

5. **Seguimiento de Estados**:
   - Pendiente
   - En Proceso
   - Completada
   - Cancelada

### 4.7. Gestión Documental Integrada

**INNOVACIÓN**: Sistema de almacenamiento y gestión de archivos vinculados a entidades (clientes, contratos).

#### Características:

1. **Organización Jerárquica**:
   ```
   storage/
   ├── clientes/{id_cliente}/
   │   ├── dni.pdf
   │   ├── factura.pdf
   │   └── otros/
   ├── contratos/{id_contrato}/
   │   ├── contrato_firmado.pdf
   │   ├── dni_cliente.pdf
   │   └── otros/
   └── ofertas/
       ├── luz-gas/
       ├── fotovoltaica/
       └── telefonia/
   ```

2. **Metadatos en Base de Datos**:
   - Tabla: `ficheros_clientes`, `ficheros_contratos`
   - Campos: id, entidad_id, nombre_archivo, ruta_archivo, tipo_documento, fecha_subida, usuario_id

3. **Seguridad**:
   - Archivos fuera de wwwroot
   - Descarga mediante endpoint controlado
   - Validación de permisos de usuario

---

## 5. MÓDULOS FUNCIONALES

### 5.1. Módulo de Gestión de Clientes

#### Funcionalidades:

1. **Registro Completo de Cliente**:
   - Tipo: Particular, Autónomo, Pyme
   - Datos personales: Nombre, DNI/CIF, CNAE, email, teléfono
   - Dirección completa: Tipo vía, dirección, número, escalera, piso, puerta, aclarador, población, provincia, código postal
   - Datos bancarios: IBAN
   - Representante legal
   - Usuario asignado (comercial)
   - Procedencia
   - Observaciones

2. **Validaciones**:
   - Formato email
   - Longitud IBAN (máx. 34 caracteres)
   - DNI/CIF único
   - Campos obligatorios

3. **Gestión Documental**:
   - Adjuntar DNI/CIF
   - Adjuntar factura actual
   - Otros documentos relevantes
   - Vista previa de documentos

4. **Búsqueda y Filtrado**:
   - Por nombre
   - Por DNI/CIF
   - Por usuario asignado
   - Por tipo de cliente

5. **Importación Masiva**:
   - Plantilla Excel predefinida
   - Validación de datos
   - Reporte de errores detallado
   - Creación de clientes nuevos
   - Actualización de clientes existentes (por ID)

#### Especificaciones Técnicas:

**Modelo de Datos**:
```csharp
public class Cliente
{
    public int Id { get; set; }
    public string TipoCliente { get; set; } // Particular, Autónomo, Pyme
    public string Nombre { get; set; }
    public string? DniCif { get; set; }
    public string? Cnae { get; set; }
    public string? Dni { get; set; } // DNI Representante
    public string? Email { get; set; }
    public string? Telefono { get; set; }
    public string? TipoVia { get; set; }
    public string? Direccion { get; set; }
    public string? Numero { get; set; }
    public string? Escalera { get; set; }
    public string? Piso { get; set; }
    public string? Puerta { get; set; }
    public string? Aclarador { get; set; }
    public string? Poblacion { get; set; }
    public string? Provincia { get; set; }
    public string? CodigoPostal { get; set; }
    public string? Iban { get; set; }
    public string? Representante { get; set; }
    public string? Comercial { get; set; } // Nombre (redundante)
    public string? Observaciones { get; set; }
    public DateTime? FechaAlta { get; set; }
    public string? Procedencia { get; set; }
    public int? IdUsuario { get; set; } // FK a usuarios
    
    // No mapeado
    public string? NombreUsuario { get; set; }
}
```

### 5.2. Módulo de Gestión de Contratos

#### Funcionalidades:

1. **Registro Multi-servicio**:
   - **Energía**: Luz, Gas
   - **Telefonía**: Fibra, Móvil, Fijo, Convergente
   - **Alarmas**: Residencial, Negocio
   - **Fotovoltaica**: Instalaciones solares

2. **Datos Específicos por Tipo**:

   **Energía**:
   - CUPS
   - Tarifa (referencia a tarifasenergia)
   - Peaje de acceso
   - Potencias contratadas (P1-P6)
   - Consumo anual
   - Bono social
   - Origen de consumidor (subrogación, baja, activación)
   - Compañía anterior

   **Telefonía**:
   - ICCID (tarjeta SIM)
   - Línea principal
   - Líneas adicionales
   - Segunda línea principal
   - Tarifa (referencia a tarifastelefonia)
   - Operadora
   - Requiere ICC
   - Tipo de móvil adicional

   **Alarmas**:
   - Tipo de inmueble
   - Tarifa (referencia a tarifas_alarmas)
   - Empresa de alarma

3. **Gestión de Estados**:
   - **Alta en proceso**: Contrato registrado, pendiente de activación
   - **Act/Facturable**: Contrato activo y facturable (genera comisión)
   - **Baja en proceso**: Solicitud de baja
   - **Baja**: Contrato dado de baja
   - **Anulado**: Contrato anulado
   - **Incidencia**: Contrato con problemas

4. **Comisiones**:
   - Cálculo automático al crear contrato
   - Basado en la tarifa y porcentaje del usuario
   - Editable manualmente por el administrador

5. **Observaciones Históricas**:
   - Sistema de anotaciones vinculadas al contrato
   - Usuario que registra
   - Fecha y hora
   - Contenido de la observación
   - Visualización cronológica

6. **Log de Activaciones**:
   - Auditoría de cambios de estado
   - Usuario que realizó el cambio
   - Estado anterior y nuevo
   - Fecha del cambio
   - Observaciones del cambio

7. **Gestión Documental**:
   - Contrato firmado
   - DNI del cliente
   - Factura anterior (cambio de compañía)
   - Otros documentos

8. **Importación Masiva**:
   - Plantillas diferenciadas por tipo (energía, telefonía, alarmas)
   - Validación de CUPS/ICCID
   - Validación de tarifas existentes
   - Cálculo automático de comisiones
   - Reporte detallado de éxitos y errores

#### Especificaciones Técnicas:

**Modelo de Datos**:
```csharp
public class Contrato
{
    public int Id { get; set; }
    public string Tipo { get; set; } // LUZ, GAS, TELEFONIA, ALARMA, FOTOVOLTAICA
    public string? Estado { get; set; }
    public DateTime? FechaAlta { get; set; }
    public int? IdCliente { get; set; }
    public string? NombreCliente { get; set; }
    public string? Dni { get; set; }
    public string? Direccion { get; set; }
    public string? Iban { get; set; }
    public decimal? Comision { get; set; }
    public int? UsuarioComercializadoraId { get; set; }
    public int? ServicioId { get; set; }
    
    // ENERGÍA
    public string? Cups { get; set; }
    public int? TarifaEnergiaId { get; set; }
    public string? Peaje { get; set; }
    public decimal? PotenciaP1 { get; set; }
    public decimal? PotenciaP2 { get; set; }
    public decimal? PotenciaP3 { get; set; }
    public decimal? PotenciaP4 { get; set; }
    public decimal? PotenciaP5 { get; set; }
    public decimal? PotenciaP6 { get; set; }
    public decimal? ConsumoAnual { get; set; }
    public bool BonoSocial { get; set; }
    public string? OrigenConsumidor { get; set; }
    public string? CompaniaAnterior { get; set; }
    
    // GAS
    public decimal? ConsumoAnualGas { get; set; }
    public string? PeajeGas { get; set; }
    
    // TELEFONÍA
    public string? Iccid { get; set; }
    public string? LineaPrincipal { get; set; }
    public string? LineasAdicionales { get; set; }
    public string? SegundaLineaPrincipal { get; set; }
    public int? TarifaTelefoniaId { get; set; }
    public int? OperadoraId { get; set; }
    public bool RequiereIcc { get; set; }
    public string? TipoMovilAdicional { get; set; }
    
    // ALARMAS
    public string? TipoInmueble { get; set; }
    public int? TarifaAlarmaId { get; set; }
    public int? EmpresaAlarmaId { get; set; }
    
    // FOTOVOLTAICA
    // (campos específicos para instalaciones solares)
    
    // AUDITORÍA
    public int? CreadoPorUsuarioId { get; set; }
    public DateTime? FechaCreacion { get; set; }
    public int? ModificadoPorUsuarioId { get; set; }
    public DateTime? FechaModificacion { get; set; }
    
    // CONTROL PLATAFORMA
    public bool CargadoPlataforma { get; set; }
    public string? IdContratoExterno { get; set; }
}
```

### 5.3. Módulo de Gestión de Tarifas

#### 5.3.1. Tarifas de Energía

**Funcionalidades**:
- Registro de tarifas de luz y gas
- Precios por periodo (P1-P6)
- Término fijo
- Precio de potencia
- Precio de energía
- Comercializadora asociada
- Estado activa/inactiva
- Penalización por permanencia

**Modelo de Datos**:
```csharp
public class TarifaEnergia
{
    public int Id { get; set; }
    public string? Nombre { get; set; }
    public string? Tipo { get; set; } // LUZ, GAS
    public decimal? TerminoFijo { get; set; }
    public decimal? PrecioPotenciaP1 { get; set; }
    public decimal? PrecioPotenciaP2 { get; set; }
    public decimal? PrecioPotenciaP3 { get; set; }
    public decimal? PrecioPotenciaP4 { get; set; }
    public decimal? PrecioPotenciaP5 { get; set; }
    public decimal? PrecioPotenciaP6 { get; set; }
    public decimal? PrecioEnergiaP1 { get; set; }
    public decimal? PrecioEnergiaP2 { get; set; }
    public decimal? PrecioEnergiaP3 { get; set; }
    public decimal? PrecioEnergiaP4 { get; set; }
    public decimal? PrecioEnergiaP5 { get; set; }
    public decimal? PrecioEnergiaP6 { get; set; }
    public decimal? Comision { get; set; }
    public int? ComercializadoraId { get; set; }
    public bool Activa { get; set; }
    public decimal? PenalizacionPermanencia { get; set; }
    public string? TipoPeaje { get; set; }
}
```

#### 5.3.2. Tarifas de Telefonía

**Funcionalidades**:
- Tarifas de fibra, móvil, fijo, convergentes
- Velocidad de fibra
- Datos móviles (GB)
- Minutos incluidos
- Operadora asociada
- Estado activa/inactiva

**Modelo de Datos**:
```csharp
public class TarifaTelefonia
{
    public int Id { get; set; }
    public string? Nombre { get; set; }
    public string? Tipo { get; set; } // FIBRA, MOVIL, FIJO, CONVERGENTE
    public decimal? Precio { get; set; }
    public string? VelocidadFibra { get; set; }
    public string? DatosMoviles { get; set; }
    public string? Minutos { get; set; }
    public decimal? Comision { get; set; }
    public int? OperadoraId { get; set; }
    public bool Activa { get; set; }
    public bool? FibraSegundaResidencia { get; set; }
}
```

#### 5.3.3. Tarifas de Alarmas

**Funcionalidades**:
- Tarifas diferenciadas por tipo de inmueble
- Precio mensual
- Empresa de alarma asociada
- Estado activa/inactiva

**Modelo de Datos**:
```csharp
public class TarifaAlarma
{
    public int Id { get; set; }
    public string? Nombre { get; set; }
    public decimal? PrecioMensual { get; set; }
    public string? TipoInmueble { get; set; } // RESIDENCIAL, NEGOCIO
    public decimal? Comision { get; set; }
    public int? EmpresaAlarmaId { get; set; }
    public bool Activa { get; set; }
}
```

### 5.4. Módulo de Gestión de Usuarios

#### Funcionalidades:

1. **Registro de Usuarios**:
   - Datos personales: Nombre, apellidos, dirección, código postal, localidad
   - Credenciales: Username, password (hasheado)
   - Contacto: Email, teléfono
   - Rol: Administrador, Backoffice, Director Comercial, Jefe de Ventas, Gestor, Colaborador
   - Jerarquía: Gestor superior, Jefe de Ventas, Director Comercial
   - Porcentaje de comisión
   - Comercializadora asignada (para colaboradores de comercializadoras específicas)
   - Estado: Activo/Inactivo

2. **Gestión de Jerarquías**:
   - Asignación de gestor a colaborador
   - Asignación de jefe de ventas a gestor
   - Asignación de director comercial a jefe de ventas
   - Validación de jerarquías circulares

3. **Permisos Granulares**:
   - Puede ver contratos
   - Puede ver liquidaciones
   - Puede ver incidencias
   - Puede ver ofertas
   - Puede ver usuarios
   - Puede gestionar liquidaciones

4. **Asociación con Proveedores**:
   - Comercializadoras permitidas
   - Operadoras permitidas
   - Empresas de alarma permitidas

5. **Configuración de Comisiones Personalizadas**:
   - Porcentajes específicos por usuario y proveedor
   - Tabla: `configuracion_comisiones`
   - Tabla: `usuario_comision_proveedores`

6. **Datos Bancarios**:
   - Número de cuenta (IBAN)
   - Tipo de entidad

7. **Documentación**:
   - DNI
   - CIF
   - Poder notarial
   - Contrato

#### Especificaciones Técnicas:

**Modelo de Datos**:
```csharp
public class Usuario
{
    public int Id { get; set; }
    public string NombreUsuario { get; set; }
    public string? Nombre { get; set; }
    public string? Apellidos { get; set; }
    public string? Direccion { get; set; }
    public string? CodigoPostal { get; set; }
    public string? Localidad { get; set; }
    public string Email { get; set; }
    public string? Telefono { get; set; }
    public string PasswordHash { get; set; }
    public string Rol { get; set; }
    public int? GestorId { get; set; }
    public int? JefeVentasId { get; set; }
    public int? DirectorComercialId { get; set; }
    public string? Comercializadora { get; set; }
    public decimal Comision { get; set; }
    public bool Activo { get; set; }
    
    // Permisos
    public bool PuedeVerContratos { get; set; }
    public bool PuedeVerLiquidaciones { get; set; }
    public bool PuedeVerIncidencias { get; set; }
    public bool PuedeVerOfertas { get; set; }
    public bool PuedeVerUsuarios { get; set; }
    public bool PuedeGestionarLiquidaciones { get; set; }
    
    // Datos bancarios
    public string? NumeroCuenta { get; set; }
    public string? TipoEntidad { get; set; }
    
    // Archivos
    public string? ArchivoDni { get; set; }
    public string? ArchivoCif { get; set; }
    public string? ArchivoPoder { get; set; }
    public string? ArchivoContrato { get; set; }
    
    public DateTime FechaCreacion { get; set; }
}
```

### 5.5. Módulo de Comisiones y Liquidaciones

#### 5.5.1. Configuración de Comisiones

**Funcionalidades**:
- Configuración global por usuario y tipo de proveedor
- Porcentajes para cada nivel jerárquico:
  - Colaborador
  - Gestor
  - Jefe de Ventas
  - Director Comercial
- Configuración específica por usuario y proveedor individual

**Modelo de Datos**:
```csharp
public class ConfiguracionComision
{
    public int Id { get; set; }
    public int UsuarioId { get; set; }
    public string TipoProveedor { get; set; } // Comercializadora, Operadora, EmpresaAlarma
    public decimal PorcentajeColaborador { get; set; }
    public decimal PorcentajeGestor { get; set; }
    public decimal PorcentajeJefeVentas { get; set; }
    public decimal PorcentajeDirectorComercial { get; set; }
    public DateTime FechaCreacion { get; set; }
    public DateTime? FechaModificacion { get; set; }
}

public class UsuarioComisionProveedor
{
    public int Id { get; set; }
    public int UsuarioId { get; set; }
    public string TipoProveedor { get; set; }
    public int ProveedorId { get; set; }
    public string NombreProveedor { get; set; }
    public decimal PorcentajeColaborador { get; set; }
    public decimal PorcentajeGestor { get; set; }
    public decimal PorcentajeJefeVentas { get; set; }
    public decimal PorcentajeDirectorComercial { get; set; }
    public DateTime FechaAsignacion { get; set; }
}
```

#### 5.5.2. Proceso de Liquidación

**Flujo Completo**:

```
1. PREPARACIÓN
   ↓
   Administrador accede a "Liquidaciones"
   Sistema muestra usuarios con contratos en estado "Act/Facturable"
   Agrupados por tipo: Energía, Telefonía, Alarmas

2. SELECCIÓN
   ↓
   Administrador selecciona usuarios a liquidar
   Puede ver resumen de comisiones por usuario
   Puede filtrar por fecha de alta de contratos

3. APROBACIÓN
   ↓
   Administrador hace clic en "Aprobar Liquidación"
   Sistema procesa TODOS los contratos seleccionados
   
4. CÁLCULO DE DISTRIBUCIÓN
   ↓
   PARA CADA contrato:
      - Obtener usuario colaborador
      - Consultar jerarquía completa
      - Buscar configuración de comisiones
      - Calcular distribución jerárquica
      - Guardar detalle en detalle_comision_liquidacion
      - Acumular totales por usuario

5. CREACIÓN DE LIQUIDACIONES INDIVIDUALES
   ↓
   PARA CADA usuario con comisión > 0:
      - Crear registro en historico_liquidacion
      - Vincular contratos correspondientes
      - Generar PDF personalizado
      - Estado inicial: "Pendiente"
      - Notificar al usuario (opcional)

6. NOTIFICACIÓN
   ↓
   Usuarios reciben notificación de nueva liquidación
   Acceden a su portal de liquidaciones

7. ACEPTACIÓN POR USUARIO
   ↓
   Usuario revisa su liquidación
   Descarga PDF
   Hace clic en "Aceptar Liquidación"
   Estado cambia a "Aceptada"

8. PAGO
   ↓
   Administrador ve liquidaciones aceptadas
   Realiza pagos
   Marca liquidaciones como "Pagada"

9. AUDITORÍA
   ↓
   Todos los cambios quedan registrados
   Histórico completo de liquidaciones
   Trazabilidad de estados
```

**Modelo de Datos**:
```csharp
public class HistoricoLiquidacion
{
    public int Id { get; set; }
    public int UsuarioId { get; set; }
    public string? UsuarioNombre { get; set; }
    public string? UsuarioEmail { get; set; }
    public int CantidadContratos { get; set; }
    public int ContratosEnergia { get; set; }
    public int ContratosTelefonia { get; set; }
    public int ContratosAlarmas { get; set; }
    public decimal ComisionTotal { get; set; }
    public decimal? DecomisionTotal { get; set; }
    public decimal Total { get; set; }
    public DateTime FechaAprobacion { get; set; }
    public int AprobadoPorId { get; set; }
    public string? AprobadoPorNombre { get; set; }
    public string Estado { get; set; } // Pendiente, Aceptada, Pagada
    public DateTime? FechaAceptada { get; set; }
    public DateTime? FechaPagada { get; set; }
    public string? RutaPdf { get; set; }
}

public class DetalleComisionLiquidacion
{
    public int Id { get; set; }
    public int HistoricoLiquidacionId { get; set; }
    public int ContratoId { get; set; }
    
    // Colaborador
    public int ColaboradorId { get; set; }
    public decimal ComisionColaborador { get; set; }
    public decimal PorcentajeColaborador { get; set; }
    
    // Gestor
    public int? GestorId { get; set; }
    public decimal? ComisionGestor { get; set; }
    public decimal? PorcentajeGestor { get; set; }
    
    // Jefe de Ventas
    public int? JefeVentasId { get; set; }
    public decimal? ComisionJefeVentas { get; set; }
    public decimal? PorcentajeJefeVentas { get; set; }
    
    // Director Comercial
    public int? DirectorComercialId { get; set; }
    public decimal? ComisionDirectorComercial { get; set; }
    public decimal? PorcentajeDirectorComercial { get; set; }
    
    // Administrador
    public int AdministradorId { get; set; }
    public decimal ComisionAdministrador { get; set; }
    public decimal PorcentajeAdministrador { get; set; }
    
    // Información del proveedor
    public string? TipoProveedor { get; set; }
    public int? ProveedorId { get; set; }
    public string? NombreProveedor { get; set; }
    
    public DateTime FechaCreacion { get; set; }
}
```

#### 5.5.3. Decomisiones (Penalizaciones)

**Funcionalidades**:
- Registro de penalizaciones vinculadas a contratos específicos
- Motivo de la decomisión
- Importe a descontar
- Impacto automático en liquidaciones

**Modelo de Datos**:
```csharp
public class Decomision
{
    public int Id { get; set; }
    public int ContratoId { get; set; }
    public int UsuarioId { get; set; }
    public string? Razon { get; set; }
    public decimal ImporteDecomision { get; set; }
    public DateTime FechaRegistro { get; set; }
    public int? RegistradoPorUsuarioId { get; set; }
}
```

#### 5.5.4. Incidencias en Liquidaciones

**Funcionalidades**:
- Sistema de tickets asociado a liquidaciones
- Estados: Abierta, En Proceso, Resuelta, Cerrada
- Comentarios con historial
- Asignación a usuarios
- Prioridad

**Modelo de Datos**:
```csharp
public class IncidenciaLiquidacion
{
    public int Id { get; set; }
    public int LiquidacionId { get; set; }
    public string Titulo { get; set; }
    public string? Descripcion { get; set; }
    public string Estado { get; set; }
    public string? Prioridad { get; set; }
    public int CreadoPorUsuarioId { get; set; }
    public int? AsignadoAUsuarioId { get; set; }
    public DateTime FechaCreacion { get; set; }
    public DateTime? FechaResolucion { get; set; }
}
```

### 5.6. Módulo SIPS (Sistema de Información de Puntos de Suministro)

#### Funcionalidades:

1. **Consulta de CUPS**:
   - Electricidad: Consulta a `sips3.php` con dos llamadas (cliente + consumos)
   - Gas: Consulta a `sips3.php` con dos llamadas (suministro + consumos)
   - Normalización automática de CUPS
   - Retry automático con sufijo "0F" para electricidad

2. **Información Obtenida**:
   - **Cliente/Suministro**:
     * Titular del suministro
     * Dirección completa
     * Código postal
     * Municipio
     * Provincia
     * Comercializadora actual
     * Distribuidora
     * Tarifa de acceso vigente
     * Potencias máximas (APM y BIE)
     * Clasificación del punto de suministro
     * Tipo de perfil de consumo
     * Tensión
     * Bono social
   
   - **Consumos Históricos**:
     * Periodo de consumo (fecha inicio - fin)
     * Tarifa aplicada
     * Consumo activo por periodo (P1-P6)
     * Consumo reactivo por periodo
     * Potencia demandada por periodo
     * Código del equipo de medida

3. **Sistema de Cache**:
   - **Memoria**: 60 minutos (configurable)
   - **Base de Datos**: Días configurables
   - **Negative Cache**: 24 horas para errores 4xx

4. **Control de Cuota**:
   - Límite mensual de consultas (configurable)
   - Contador automático
   - Bloqueo al alcanzar límite
   - Dashboard de consumo

5. **Histórico de Consultas**:
   - Auditoría completa de consultas realizadas
   - Usuario que consultó
   - Fecha y hora
   - Respuesta completa almacenada
   - Estado de éxito/error

6. **Generación de PDF**:
   - Documento profesional con datos del suministro
   - Logo de empresa
   - Gráficos de consumo
   - Tabla de consumos históricos
   - Información técnica del punto de suministro

#### Especificaciones Técnicas:

**Endpoints API**:
- Luz (cliente): `http://35.181.7.83/api/sips3.php?id=1&cups={CUPS}`
- Luz (consumos): `http://35.181.7.83/api/sips3.php?id=2&cups={CUPS}`
- Gas (suministro): `http://35.181.7.83/api/sips3.php?id=3&cups={CUPS}`
- Gas (consumos): `http://35.181.7.83/api/sips3.php?id=4&cups={CUPS}`

**Modelo de Datos**:
```csharp
public class HistoricoSipsConsulta
{
    public long Id { get; set; }
    public string Cups { get; set; }
    public int? UsuarioId { get; set; }
    public string? UsuarioNombre { get; set; }
    public string? UsuarioEmail { get; set; }
    public DateTime FechaConsulta { get; set; }
    public bool Success { get; set; }
    public int? HttpStatusCode { get; set; }
    public string? ErrorMessage { get; set; }
    public string? ResponseJson { get; set; }
    public bool EsGas { get; set; }
}

public class SipsResponse
{
    public List<ClienteSips>? ClientesSips { get; set; }
    public List<ConsumoSips>? ConsumosSips { get; set; }
}

public class ClienteSips
{
    public string? CodigoCUPS { get; set; }
    public string? TitularNIF { get; set; }
    public string? TitularNombreRazonSocial { get; set; }
    public string? DireccionSuministro { get; set; }
    public string? CodigoPostal { get; set; }
    public string? Municipio { get; set; }
    public string? Provincia { get; set; }
    public string? NombreEmpresaComercializadora { get; set; }
    public string? NombreEmpresaDistribuidora { get; set; }
    public string? CodigoTarifaATREnVigor { get; set; }
    public decimal? PotenciaMaximaBIEW { get; set; }
    public decimal? PotenciaMaximaAPMW { get; set; }
    public string? TipoPerfilConsumo { get; set; }
    public bool? EsBeneficiarioBonoSocial { get; set; }
    // ... más campos
}

public class ConsumoSips
{
    public string? CodigoCUPS { get; set; }
    public DateTime? FechaInicio { get; set; }
    public DateTime? FechaFin { get; set; }
    public string? CodigoTarifaATR { get; set; }
    public decimal? Activa1 { get; set; }
    public decimal? Activa2 { get; set; }
    public decimal? Activa3 { get; set; }
    public decimal? Activa4 { get; set; }
    public decimal? Activa5 { get; set; }
    public decimal? Activa6 { get; set; }
    public decimal? Reactiva1 { get; set; }
    // ... más campos
}
```

### 5.7. Módulo Comparador de Tarifas

#### Funcionalidades:

1. **Entrada de Datos**:
   - **Manual**: Introducción de datos manualmente
   - **Automática**: Consulta a SIPS mediante CUPS y autocompletado

2. **Datos Requeridos**:
   - Tipo de tarifa: 2.0TD, 3.0TD, 6.1TD
   - Tipo de suministro: Luz, Luz+Gas
   - Potencias contratadas (P1, P2, P3)
   - Consumos anuales (P1, P2, P3)
   - Precio factura actual (opcional)

3. **Cálculo de Costes**:
   - Término fijo anual
   - Coste de potencia anual (por periodo)
   - Coste de energía anual (por periodo)
   - Coste total anual y mensual

4. **Comparativa**:
   - Ranking de tarifas (de más barata a más cara)
   - Ahorro mensual y anual (si se proporcionó precio actual)
   - Visualización en tabla ordenada
   - Modal con desglose detallado por tarifa

5. **Generación de PDF**:
   - Documento profesional personalizado
   - Logo de empresa
   - Datos del cliente y suministro
   - Tabla comparativa
   - Gráfico de barras
   - Desglose de costes
   - Nota sobre impuestos

6. **Integración con SIPS**:
   - Botón "Consultar SIPS"
   - Autocompletado automático de todos los campos
   - Validación de CUPS
   - Manejo de errores

#### Especificaciones Técnicas:

**Fórmulas de Cálculo**:
```
Término Fijo Anual = Precio Término Fijo Diario × 365

Coste Potencia Anual = Σ (Precio Potencia Pi × Potencia Pi) × 365

Coste Energía Anual = Σ (Precio Energía Pi × Consumo Pi)

Coste Total Anual = Término Fijo + Potencia + Energía

Coste Mensual = Coste Anual / 12

Ahorro Mensual = Precio Actual - Coste Mensual

Ahorro Anual = Ahorro Mensual × 12
```

**Tipos de Tarifa**:
- **2.0TD**: Potencia ≤ 10 kW, 2 periodos (P1, P2)
- **3.0TD**: Potencia 10-15 kW, 3 periodos (P1, P2, P3)
- **6.1TD**: Potencia > 15 kW, 3 periodos (P1, P2, P3)

### 5.8. Módulo de Ofertas

#### Funcionalidades:

1. **Tipos de Oferta**:
   - ⚡ Luz
   - 🔥 Gas
   - ☀️ Fotovoltaica
   - 📡 Fibra
   - 📱 Móvil
   - 🌐 Fibra + Móvil
   - 📺 Fibra + Móvil + TV
   - 🚨 Alarma

2. **Formularios Dinámicos**:
   - Aparecen campos específicos según servicios seleccionados
   - **Luz/Gas**: Adjuntar última factura
   - **Fotovoltaica**: Factura + enlace Google Maps ubicación
   - **Telefonía**: 
     * Tipo operación (alta nueva / portabilidad)
     * Si portabilidad: adjuntar factura actual
     * Qué tiene contratado
     * Qué desea contratar
   - **Alarma**:
     * Tipo inmueble (negocio / residencial)
     * Si tiene alarma actualmente
     * Observaciones

3. **Gestión de Interesados**:
   - Nombre del cliente interesado
   - Teléfono de contacto
   - Email
   - Vinculación opcional con cliente existente

4. **Almacenamiento de Archivos**:
   - Organización por tipo de servicio
   - Carpetas: `storage/ofertas/luz-gas/`, `fotovoltaica/`, `telefonia/`
   - Nombres únicos con timestamp
   - Validación de formato (PDF, JPG, PNG)
   - Límite de tamaño: 10MB

5. **Notificación Automática**:
   - Email al administrador con todos los detalles
   - Resumen de servicios solicitados
   - Datos del interesado
   - Enlaces a archivos adjuntos

6. **Seguimiento**:
   - Estados: Pendiente, En Proceso, Completada, Cancelada
   - Historial de solicitudes del usuario
   - Filtros por estado y fecha

7. **Vista de Detalle**:
   - Información completa de la solicitud
   - Descarga de archivos adjuntos
   - Cambio de estado
   - Anotaciones

#### Especificaciones Técnicas:

**Modelo de Datos**:
```csharp
public class SolicitudOferta
{
    public int Id { get; set; }
    public int UsuarioId { get; set; }
    public string NombreUsuario { get; set; }
    public string EmailUsuario { get; set; }
    public DateTime FechaSolicitud { get; set; }
    
    // Servicios solicitados
    public bool SolicitaLuz { get; set; }
    public bool SolicitaGas { get; set; }
    public bool SolicitaFotovoltaica { get; set; }
    public bool SolicitaFibra { get; set; }
    public bool SolicitaMovil { get; set; }
    public bool SolicitaFibraMovil { get; set; }
    public bool SolicitaFibraMovilTv { get; set; }
    public bool SolicitaAlarma { get; set; }
    
    // Luz/Gas
    public string? RutaFacturaLuzGas { get; set; }
    public string? ObservacionesLuzGas { get; set; }
    
    // Fotovoltaica
    public string? RutaFacturaFotovoltaica { get; set; }
    public string? EnlaceGoogleMaps { get; set; }
    public string? ObservacionesFotovoltaica { get; set; }
    
    // Telefonía
    public string? TipoOperacionTelefonia { get; set; } // ALTA_NUEVA, PORTABILIDAD
    public string? RutaFacturaTelefonia { get; set; }
    public string? TieneContratadoTelefonia { get; set; }
    public string? DeseaContratarTelefonia { get; set; }
    
    // Alarma
    public string? TipoInmuebleAlarma { get; set; } // NEGOCIO, RESIDENCIAL
    public bool? TieneAlarmaActualmente { get; set; }
    public string? ObservacionesAlarma { get; set; }
    
    // Interesado
    public string NombreComercial { get; set; }
    public string TelefonoInteresado { get; set; }
    public string? EmailInteresado { get; set; }
    public int? ClienteId { get; set; }
    
    // Estado
    public string Estado { get; set; } // Pendiente, En Proceso, Completada, Cancelada
}
```

### 5.9. Módulo de Incidencias

#### Funcionalidades:

1. **Registro de Incidencias**:
   - Título
   - Descripción detallada
   - Vinculación a contrato (opcional)
   - Prioridad: Baja, Media, Alta, Crítica
   - Estado inicial: Abierta

2. **Gestión de Estados**:
   - **Abierta**: Incidencia registrada
   - **En Proceso**: Trabajando en la solución
   - **Resuelta**: Solución encontrada, pendiente de cierre
   - **Cerrada**: Incidencia finalizada

3. **Asignación**:
   - Asignar a usuario específico
   - Reasignación
   - Notificación al usuario asignado

4. **Sistema de Comentarios**:
   - Conversación asociada a la incidencia
   - Usuario que comenta
   - Fecha y hora del comentario
   - Historial cronológico

5. **Filtros y Búsqueda**:
   - Por estado
   - Por prioridad
   - Por usuario asignado
   - Por fecha de creación
   - Por contrato vinculado

6. **Dashboard**:
   - Resumen de incidencias abiertas
   - Incidencias asignadas al usuario
   - Incidencias pendientes de respuesta

#### Especificaciones Técnicas:

**Modelo de Datos**:
```csharp
public class Incidencia
{
    public int Id { get; set; }
    public string Titulo { get; set; }
    public string? Descripcion { get; set; }
    public int? ContratoId { get; set; }
    public string Estado { get; set; }
    public string? Prioridad { get; set; }
    public int CreadoPorUsuarioId { get; set; }
    public int? AsignadoAUsuarioId { get; set; }
    public DateTime FechaCreacion { get; set; }
    public DateTime? FechaResolucion { get; set; }
    public DateTime? FechaCierre { get; set; }
}

public class ComentarioIncidencia
{
    public int Id { get; set; }
    public int IncidenciaId { get; set; }
    public int UsuarioId { get; set; }
    public string? NombreUsuario { get; set; }
    public string Comentario { get; set; }
    public DateTime FechaComentario { get; set; }
}
```

### 5.10. Módulo de Configuración

#### 5.10.1. Configuración de Empresa

**Funcionalidades**:
- Nombre de la empresa
- CIF
- Dirección completa
- Teléfono
- Email
- Sitio web
- Logo (upload y gestión)
- Configuración SMTP:
  * Host
  * Puerto
  * Usuario
  * Contraseña
  * Email remitente
  * Nombre remitente
  * Usar SSL

**Modelo de Datos**:
```csharp
public class ConfiguracionEmpresa
{
    public int Id { get; set; }
    public string? NombreEmpresa { get; set; }
    public string? Cif { get; set; }
    public string? Direccion { get; set; }
    public string? Telefono { get; set; }
    public string? Email { get; set; }
    public string? SitioWeb { get; set; }
    public string? RutaLogo { get; set; }
    public string? SmtpHost { get; set; }
    public int? SmtpPort { get; set; }
    public string? SmtpUsuario { get; set; }
    public string? SmtpPassword { get; set; }
    public string? SmtpEmailFrom { get; set; }
    public string? SmtpNombreFrom { get; set; }
    public bool? SmtpUsarSsl { get; set; }
}
```

#### 5.10.2. Mensajes de Bienvenida

**Funcionalidades**:
- Mensaje personalizado por rol
- Editor de texto enriquecido
- Vista previa
- Activación/desactivación por rol

**Modelo de Datos**:
```csharp
public class MensajeBienvenida
{
    public int Id { get; set; }
    public string Rol { get; set; }
    public string Mensaje { get; set; }
    public bool Activo { get; set; }
    public DateTime? FechaCreacion { get; set; }
    public DateTime? FechaModificacion { get; set; }
}
```

#### 5.10.3. Control de Módulos

**Funcionalidades**:
- Activar/desactivar módulo SIPS
- Activar/desactivar módulo de liquidaciones
- Configuración de cuotas SIPS
- Configuración de cache SIPS

### 5.11. Módulo de Gestión Documental y PDFs

#### 5.11.1. Generación de PDFs

**PDFs Generados**:

1. **PDF de Liquidación**:
   - Logo de empresa
   - Datos del usuario
   - Periodo de liquidación
   - Resumen de comisiones por servicio
   - Tabla detallada de contratos:
     * Cliente
     * Servicio
     * Comercializadora/Operadora/Empresa
     * Fecha
     * Comisión
   - Tabla de comisiones como superior jerárquico:
     * Usuario subordinado
     * Contrato
     * Comisión recibida
     * Rol en la jerarquía
   - Decomisiones (si existen)
   - Total a liquidar

2. **PDF Comparador de Tarifas**:
   - Logo de empresa
   - Datos del cliente
   - Datos del suministro actual:
     * CUPS
     * Potencias
     * Consumos
     * Precio actual
   - Tabla comparativa de tarifas:
     * Posición
     * Comercializadora
     * Tarifa
     * Coste anual
     * Coste mensual
     * Ahorro mensual
     * Ahorro anual
   - Gráfico de barras comparativo
   - Desglose detallado de tarifas seleccionadas
   - Nota sobre impuestos

3. **PDF Consulta SIPS**:
   - Logo de empresa
   - Datos del suministro:
     * CUPS
     * Titular
     * Dirección
     * Comercializadora actual
     * Distribuidora
     * Tarifa de acceso
     * Potencias máximas
     * Bono social
   - Gráficos de consumo:
     * Consumo por periodo (barras)
     * Evolución temporal (línea)
   - Tabla de consumos históricos
   - Información técnica del punto de suministro

#### 5.11.2. Sistema de Almacenamiento

**Estructura de Carpetas**:
```
storage/
├── clientes/
│   └── {id_cliente}/
│       ├── dni.pdf
│       ├── factura.pdf
│       └── otros/
├── contratos/
│   └── {id_contrato}/
│       ├── contrato_firmado.pdf
│       ├── dni_cliente.pdf
│       ├── factura_anterior.pdf
│       └── otros/
├── ofertas/
│   ├── luz-gas/
│   │   └── factura_{timestamp}.pdf
│   ├── fotovoltaica/
│   │   ├── factura_{timestamp}.pdf
│   │   └── ubicacion_{timestamp}.jpg
│   └── telefonia/
│       └── factura_{timestamp}.pdf
├── liquidaciones/
│   └── {año}/
│       └── {mes}/
│           └── liquidacion_{id}_{usuario}.pdf
└── comparador/
    └── comparativa_{timestamp}.pdf
```

**Seguridad**:
- Archivos almacenados fuera de `wwwroot`
- Acceso mediante endpoint controlado
- Validación de permisos por rol
- Validación de propiedad del archivo

### 5.12. Módulo de Tareas Pendientes

#### Funcionalidades:

1. **Gestión de Tareas**:
   - Título
   - Descripción
   - Prioridad
   - Fecha límite
   - Usuario asignado
   - Estado: Pendiente, En Proceso, Completada

2. **Dashboard de Tareas**:
   - Mis tareas pendientes
   - Tareas vencidas
   - Tareas del día
   - Tareas de la semana

3. **Notificaciones**:
   - Tareas próximas a vencer
   - Tareas vencidas

**Modelo de Datos**:
```csharp
public class TareaPendiente
{
    public int Id { get; set; }
    public string Titulo { get; set; }
    public string? Descripcion { get; set; }
    public string? Prioridad { get; set; }
    public DateTime? FechaLimite { get; set; }
    public int? UsuarioAsignadoId { get; set; }
    public string Estado { get; set; }
    public int CreadoPorUsuarioId { get; set; }
    public DateTime FechaCreacion { get; set; }
    public DateTime? FechaCompletada { get; set; }
}
```

### 5.13. Módulo de Log de Accesos

#### Funcionalidades:

1. **Registro Automático**:
   - Usuario que accede
   - Fecha y hora de acceso
   - Dirección IP
   - Navegador (User Agent)

2. **Auditoría**:
   - Historial completo de accesos
   - Filtros por usuario
   - Filtros por fecha
   - Exportación a Excel

3. **Dashboard de Accesos**:
   - Usuarios conectados hoy
   - Accesos del mes
   - Gráfico de accesos por día

**Modelo de Datos**:
```csharp
public class LogAcceso
{
    public int Id { get; set; }
    public int UsuarioId { get; set; }
    public string? NombreUsuario { get; set; }
    public DateTime FechaAcceso { get; set; }
    public string? DireccionIp { get; set; }
    public string? UserAgent { get; set; }
}
```

---

## 6. MODELO DE DATOS

### 6.1. Diagrama Relacional Principal

```
┌─────────────┐      ┌──────────────┐      ┌─────────────┐
│  USUARIOS   │──┐   │   CLIENTES   │──────│  CONTRATOS  │
└─────────────┘  │   └──────────────┘      └─────────────┘
      │          │           │                     │
      │ gestor_id│           │ id_usuario          │
      │          │           │                     │
      └──────────┘           │                     │
                             │                     │
┌─────────────────────┐     │              ┌──────┴──────┐
│ TARIFAS ENERGIA     │─────┼──────────────│ SERVICIOS   │
└─────────────────────┘     │              └─────────────┘
                             │
┌─────────────────────┐     │
│ TARIFAS TELEFONIA   │─────┤
└─────────────────────┘     │
                             │
┌─────────────────────┐     │
│ TARIFAS ALARMAS     │─────┘
└─────────────────────┘

┌─────────────────────┐
│ COMERCIALIZADORAS   │
└─────────────────────┘
┌─────────────────────┐
│ OPERADORAS          │
└─────────────────────┘
┌─────────────────────┐
│ EMPRESAS ALARMA     │
└─────────────────────┘

┌──────────────────────┐       ┌─────────────────────────────┐
│ HISTORICO_LIQUIDACION│───────│DETALLE_COMISION_LIQUIDACION │
└──────────────────────┘       └─────────────────────────────┘
         │                               │
         │                               │ contrato_id
         │                               │
         └───────────────────────────────┘

┌─────────────────────┐       ┌───────────────────────────┐
│ SOLICITUDES_OFERTAS │       │ INCIDENCIAS               │
└─────────────────────┘       └───────────────────────────┘
                                        │
                                        │
                              ┌─────────┴───────────┐
                              │COMENTARIOS_INCIDENCIA│
                              └─────────────────────┘

┌─────────────────────────┐
│ HISTORICO_SIPS_CONSULTAS│
└─────────────────────────┘

┌────────────────────────┐
│ CONFIGURACION_COMISIONES│
└────────────────────────┘

┌────────────────────────────┐
│ USUARIO_COMISION_PROVEEDORES│
└────────────────────────────┘
```

### 6.2. Tablas Principales (35 tablas)

1. **Gestión de Usuarios** (2):
   - usuarios
   - log_accesos

2. **Gestión de Clientes** (2):
   - clientes_simple
   - ficheros_clientes

3. **Gestión de Contratos** (4):
   - contratos
   - ficheros_contratos
   - observaciones_contratos
   - log_activacion_contratos

4. **Tarifas** (4):
   - tarifasenergia
   - tarifastelefonia
   - tarifas_alarmas
   - servicios

5. **Proveedores** (3):
   - comercializadoras
   - operadoras
   - empresas_alarma

6. **Comisiones y Liquidaciones** (6):
   - historico_liquidacion
   - detalle_comision_liquidacion
   - decomisiones
   - configuracion_comisiones
   - usuario_comision_proveedores
   - incidencias_liquidacion

7. **Relaciones N:M** (3):
   - usuario_comercializadoras
   - usuario_operadoras
   - usuario_empresas_alarma

8. **Ofertas** (2):
   - solicitudes_ofertas
   - ofertas_interesados

9. **Incidencias** (2):
   - incidencias
   - comentarios_incidencia

10. **Sistema SIPS** (1):
    - historico_sips_consultas

11. **Configuración** (3):
    - configuracion_empresa
    - mensajes_bienvenida
    - tareas_pendientes

12. **Noticias** (2):
    - noticias
    - noticias_imagenes

### 6.3. Índices Principales

**Optimización de Consultas**:

1. **usuarios**:
   - `idx_username` en username
   - `idx_gestor` en gestor_id
   - `idx_jefe_ventas` en jefe_ventas_id
   - `idx_director_comercial` en director_comercial_id

2. **contratos**:
   - `idx_estado` en estado
   - `idx_usuario` en usuario_comercializadora_id
   - `idx_fecha_alta` en fecha_alta
   - `idx_cups` en cups
   - `idx_iccid` en iccid

3. **clientes_simple**:
   - `idx_nombre` en nombre
   - `idx_dni_cif` en dni_cif
   - `idx_usuario` en id_usuario

4. **historico_liquidacion**:
   - `idx_usuario` en usuario_id
   - `idx_fecha` en fecha_aprobacion
   - `idx_estado` en estado

5. **detalle_comision_liquidacion**:
   - `idx_liquidacion` en historico_liquidacion_id
   - `idx_contrato` en contrato_id
   - `idx_colaborador` en colaborador_id
   - `idx_gestor` en gestor_id

6. **historico_sips_consultas**:
   - `idx_cups` en cups
   - `idx_fecha` en fecha_consulta
   - `idx_usuario` en usuario_id

---

## 7. FLUJOS DE TRABAJO

### 7.1. Flujo de Alta de Contrato

```
┌─────────────────────┐
│ INICIO: Usuario     │
│ accede a Contratos  │
└──────────┬──────────┘
           │
           ▼
┌─────────────────────┐
│ Hace clic en        │
│ "Nuevo Contrato"    │
└──────────┬──────────┘
           │
           ▼
┌─────────────────────────┐
│ Selecciona tipo servicio│
│ (Luz/Gas/Tel/Alarma)    │
└──────────┬──────────────┘
           │
           ▼
┌─────────────────────────┐
│ Formulario dinámico     │
│ aparece según tipo      │
└──────────┬──────────────┘
           │
           ▼
┌─────────────────────────┐
│ Introduce datos:        │
│ - Cliente (búsqueda)    │
│ - CUPS/ICCID            │
│ - Tarifa (autocompletar)│
│ - Potencias/Consumos    │
│ - Datos específicos     │
└──────────┬──────────────┘
           │
           ▼
┌─────────────────────────┐
│ Sistema calcula         │
│ comisión automáticamente│
│ (Tarifa × % Usuario)    │
└──────────┬──────────────┘
           │
           ▼
┌─────────────────────────┐
│ Usuario puede editar    │
│ comisión manualmente    │
└──────────┬──────────────┘
           │
           ▼
┌─────────────────────────┐
│ Adjunta documentación:  │
│ - Contrato firmado      │
│ - DNI cliente           │
│ - Factura anterior      │
└──────────┬──────────────┘
           │
           ▼
┌─────────────────────────┐
│ Hace clic en "Guardar"  │
└──────────┬──────────────┘
           │
           ▼
┌─────────────────────────┐
│ Sistema valida datos:   │
│ - CUPS/ICCID único      │
│ - Tarifa activa         │
│ - Cliente existe        │
│ - Datos obligatorios    │
└──────────┬──────────────┘
           │
           ├──[ERROR]──┐
           │           ▼
           │   ┌───────────────┐
           │   │ Muestra error │
           │   │ Corregir datos│
           │   └───────┬───────┘
           │           │
           │           └───────────┐
           │                       │
           └──[OK]──────────────►  ▼
                         ┌─────────────────┐
                         │ Contrato creado │
                         │ Estado: Alta    │
                         │ en proceso      │
                         └────────┬────────┘
                                  │
                                  ▼
                         ┌─────────────────┐
                         │ Log registro:   │
                         │ - Usuario       │
                         │ - Fecha/Hora    │
                         └────────┬────────┘
                                  │
                                  ▼
                         ┌─────────────────┐
                         │ FIN: Contrato   │
                         │ registrado      │
                         └─────────────────┘
```

### 7.2. Flujo de Liquidación Completo

```
┌──────────────────────────┐
│ INICIO: Administrador    │
│ accede a Liquidaciones   │
└───────────┬──────────────┘
            │
            ▼
┌───────────────────────────┐
│ Sistema muestra usuarios  │
│ con contratos Act/Factura │
│ - Agrupados por tipo      │
│ - Total comisiones        │
│ - Número de contratos     │
└───────────┬───────────────┘
            │
            ▼
┌───────────────────────────┐
│ Administrador selecciona  │
│ usuarios a liquidar       │
│ - Múltiple selección      │
│ - Filtros por fecha       │
└───────────┬───────────────┘
            │
            ▼
┌───────────────────────────┐
│ Hace clic en "Aprobar     │
│ Liquidación"              │
└───────────┬───────────────┘
            │
            ▼
┌───────────────────────────────────┐
│ Sistema procesa CADA CONTRATO:    │
│                                   │
│ 1. Obtiene usuario colaborador    │
│ 2. Consulta jerarquía completa:   │
│    Gestor → Jefe → Director       │
│ 3. Busca configuración comisiones │
│    por usuario + proveedor        │
│ 4. Si no existe, usa % por defecto│
│ 5. Calcula distribución:          │
│    - Colaborador: comision_contrato│
│    - Gestor: base × %gestor       │
│    - Jefe: base × %jefe           │
│    - Director: base × %director   │
│    - Admin: base × %restante      │
│ 6. Guarda en                      │
│    detalle_comision_liquidacion   │
└───────────┬───────────────────────┘
            │
            ▼
┌───────────────────────────────────┐
│ Sistema agrupa comisiones         │
│ por usuario:                      │
│                                   │
│ usuariosConComision = {}          │
│ PARA CADA detalle:                │
│   - Si colaborador_id:            │
│     acumular comision_colaborador │
│   - Si gestor_id:                 │
│     acumular comision_gestor      │
│   - Si jefe_ventas_id:            │
│     acumular comision_jefe        │
│   - Si director_id:               │
│     acumular comision_director    │
│   - Si administrador_id:          │
│     acumular comision_administrador│
└───────────┬───────────────────────┘
            │
            ▼
┌───────────────────────────────────┐
│ Crea liquidaciones individuales:  │
│                                   │
│ PARA CADA usuario con comisión>0: │
│   1. Crea historico_liquidacion   │
│   2. Vincula contratos            │
│   3. Calcula totales:             │
│      - Total comisiones           │
│      - Decomisiones (restar)      │
│      - Total a liquidar           │
│   4. Genera PDF personalizado     │
│   5. Estado: "Pendiente"          │
└───────────┬───────────────────────┘
            │
            ▼
┌───────────────────────────────────┐
│ Notifica a usuarios (opcional)    │
│ - Email con enlace a liquidación  │
└───────────┬───────────────────────┘
            │
            ▼
┌───────────────────────────────────┐
│ Usuarios acceden a su portal      │
│ - Ven su liquidación pendiente    │
│ - Descargan PDF                   │
│ - Revisan detalles                │
└───────────┬───────────────────────┘
            │
            ▼
┌───────────────────────────────────┐
│ Usuario hace clic en              │
│ "Aceptar Liquidación"             │
└───────────┬───────────────────────┘
            │
            ▼
┌───────────────────────────────────┐
│ Estado cambia a "Aceptada"        │
│ - Fecha aceptación: NOW()         │
└───────────┬───────────────────────┘
            │
            ▼
┌───────────────────────────────────┐
│ Administrador ve liquidaciones    │
│ aceptadas                         │
│ - Filtra por estado "Aceptada"    │
└───────────┬───────────────────────┘
            │
            ▼
┌───────────────────────────────────┐
│ Realiza pagos bancarios           │
│ (proceso externo)                 │
└───────────┬───────────────────────┘
            │
            ▼
┌───────────────────────────────────┐
│ Marca liquidaciones como "Pagada" │
│ - Fecha pago: NOW()               │
│ - Una por una o masivamente       │
└───────────┬───────────────────────┘
            │
            ▼
┌───────────────────────────────────┐
│ FIN: Ciclo de liquidación         │
│ completado                        │
│ - Histórico permanente            │
│ - Auditoría completa              │
└───────────────────────────────────┘
```

### 7.3. Flujo de Consulta SIPS con Cache

```
┌─────────────────────┐
│ Usuario introduce   │
│ CUPS                │
└──────────┬──────────┘
           │
           ▼
┌─────────────────────┐
│ Normalizar CUPS     │
│ (quitar espacios,   │
│  convertir a mayús) │
└──────────┬──────────┘
           │
           ▼
┌─────────────────────┐
│ Buscar en CACHE     │
│ DE MEMORIA          │
│ (60 min)            │
└──────────┬──────────┘
           │
           ├─[FOUND]──►┌──────────────┐
           │           │ Devolver     │
           │           │ desde memoria│
           │           └──────────────┘
           │
           ├─[NOT FOUND]
           │
           ▼
┌─────────────────────────┐
│ Buscar en BASE DE DATOS │
│ (histórico < N días)    │
└──────────┬──────────────┘
           │
           ├─[FOUND]──►┌────────────────┐
           │           │ Guardar en     │
           │           │ cache memoria  │
           │           │ Devolver datos │
           │           └────────────────┘
           │
           ├─[NOT FOUND]
           │
           ▼
┌─────────────────────────┐
│ Verificar NEGATIVE CACHE│
│ (errores 4xx < 24h)     │
└──────────┬──────────────┘
           │
           ├─[FOUND]──►┌────────────────┐
           │           │ Error: CUPS ya │
           │           │ falló antes    │
           │           └────────────────┘
           │
           ├─[NOT FOUND]
           │
           ▼
┌─────────────────────────┐
│ Verificar CUOTA MENSUAL │
│ ¿Alcanzado límite?      │
└──────────┬──────────────┘
           │
           ├─[LIMIT]──►┌────────────────┐
           │           │ Error: Cuota   │
           │           │ agotada        │
           │           └────────────────┘
           │
           ├─[OK]
           │
           ▼
┌─────────────────────────┐
│ Llamar a API SIPS:      │
│ - Si GAS: sips3.php     │
│   id=3 (suministro)     │
│   id=4 (consumos)       │
│ - Si LUZ: sips3.php     │
│   id=1 (cliente)        │
│   id=2 (consumos)       │
└──────────┬──────────────┘
           │
           ├─[ERROR 4xx]──►┌────────────────┐
           │               │ Guardar en BD  │
           │               │ como error     │
           │               │ (negative cache│
           │               └────────────────┘
           │
           ├─[ERROR 5xx]──►┌────────────────┐
           │               │ Error API      │
           │               │ (no cachear)   │
           │               └────────────────┘
           │
           ├─[SUCCESS]
           │
           ▼
┌─────────────────────────┐
│ Guardar en BASE DE DATOS│
│ - Respuesta completa    │
│ - Usuario que consultó  │
│ - Fecha/hora            │
│ - Estado success        │
└──────────┬──────────────┘
           │
           ▼
┌─────────────────────────┐
│ Guardar en CACHE MEMORIA│
│ (60 min)                │
└──────────┬──────────────┘
           │
           ▼
┌─────────────────────────┐
│ Incrementar contador    │
│ de cuota mensual        │
└──────────┬──────────────┘
           │
           ▼
┌─────────────────────────┐
│ Devolver datos al       │
│ usuario                 │
└─────────────────────────┘
```

### 7.4. Flujo de Comparador de Tarifas con SIPS

```
┌─────────────────────────┐
│ Usuario accede a        │
│ Comparador              │
└──────────┬──────────────┘
           │
           ▼
┌─────────────────────────┐
│ Introduce CUPS          │
│ Hace clic "Consultar    │
│ SIPS"                   │
└──────────┬──────────────┘
           │
           ▼
┌─────────────────────────┐
│ Consulta SIPS           │
│ (con cache inteligente) │
└──────────┬──────────────┘
           │
           ├─[ERROR]──►┌────────────────┐
           │           │ Mostrar error  │
           │           │ Permitir manual│
           │           └────────────────┘
           │
           ├─[SUCCESS]
           │
           ▼
┌─────────────────────────┐
│ Sistema autocompleta:   │
│ 1. Tipo tarifa (ATR)    │
│ 2. Potencias (P1-P6)    │
│ 3. Consumos históricos  │
│ 4. Compañía actual      │
└──────────┬──────────────┘
           │
           ▼
┌─────────────────────────┐
│ Usuario revisa/edita    │
│ datos autocompletados   │
│ Introduce precio actual │
│ (opcional)              │
└──────────┬──────────────┘
           │
           ▼
┌─────────────────────────┐
│ Hace clic "Comparar     │
│ Tarifas"                │
└──────────┬──────────────┘
           │
           ▼
┌────────────────────────────────┐
│ Sistema calcula costes PARA    │
│ CADA tarifa activa del mercado:│
│                                │
│ 1. Término Fijo:               │
│    TF × 365                    │
│ 2. Potencia:                   │
│    Σ(PotPi × PrecioPotPi × 365)│
│ 3. Energía:                    │
│    Σ(ConsPi × PrecioEnergPi)   │
│ 4. Total Anual:                │
│    TF + Potencia + Energía     │
│ 5. Total Mensual:              │
│    Anual / 12                  │
│ 6. Ahorro (si hay precio):     │
│    Precio Actual - Total Mensual│
└────────────┬───────────────────┘
             │
             ▼
┌─────────────────────────┐
│ Ordena tarifas por      │
│ coste total (menor a    │
│ mayor)                  │
└──────────┬──────────────┘
           │
           ▼
┌─────────────────────────┐
│ Muestra tabla con:      │
│ - Posición (#1 = mejor) │
│ - Comercializadora      │
│ - Nombre tarifa         │
│ - Coste mensual         │
│ - Coste anual           │
│ - Ahorro mensual        │
│ - Ahorro anual          │
│ - Botón "Ver detalles"  │
└──────────┬──────────────┘
           │
           ▼
┌─────────────────────────┐
│ Usuario hace clic       │
│ "Generar PDF"           │
└──────────┬──────────────┘
           │
           ▼
┌─────────────────────────┐
│ Sistema genera PDF:     │
│ - Logo empresa          │
│ - Datos cliente         │
│ - Datos suministro      │
│ - Tabla comparativa     │
│ - Gráfico de barras     │
│ - Desglose detallado    │
│ - Nota impuestos        │
└──────────┬──────────────┘
           │
           ▼
┌─────────────────────────┐
│ Descarga PDF            │
│ (nombre:                │
│  comparativa_YYYYMMDD)  │
└─────────────────────────┘
```

---

## 8. INTEGRACIONES

### 8.1. Integración SIPS (Sistema de Información de Puntos de Suministro)

**Descripción**: Integración con API externa para consultar datos de suministros eléctricos y de gas.

**API Utilizada**: `http://35.181.7.83/api/sips3.php`

**Endpoints**:
- **Luz - Cliente**: `?id=1&cups={CUPS}`
- **Luz - Consumos**: `?id=2&cups={CUPS}`
- **Gas - Suministro**: `?id=3&cups={CUPS}`
- **Gas - Consumos**: `?id=4&cups={CUPS}`

**Formato Respuesta**: CSV

**Protocolo**:
1. Consulta GET a la API
2. Parseo de respuesta CSV
3. Conversión a objetos C# (SipsResponse)
4. Almacenamiento en cache (memoria + BD)

**Optimizaciones**:
- Cache de 3 niveles
- Control de cuota mensual
- Negative cache para errores
- Retry con sufijo "0F" (electricidad)

### 8.2. Integración Revolapps (Gestión de Incidencias)

**Descripción**: Integración con sistema externo de gestión de incidencias.

**API**: REST API de Revolapps

**Funcionalidades**:
- Creación de tickets
- Actualización de estado
- Consulta de incidencias

**Protocolo**:
- Autenticación: Bearer Token
- Formato: JSON
- Método: HTTP/REST

### 8.3. Integración SMTP (Envío de Correos)

**Descripción**: Sistema de envío de correos electrónicos configurables.

**Funcionalidades**:
- Notificación de nuevas ofertas
- Notificación de liquidaciones
- Alertas de incidencias
- Bienvenida a nuevos usuarios

**Configuración**:
- Host SMTP
- Puerto (25, 465, 587)
- Autenticación (usuario/contraseña)
- SSL/TLS
- Remitente personalizado

**Plantillas**:
- HTML personalizables
- Variables dinámicas
- Logo de empresa

---

## 9. SEGURIDAD Y CONTROL DE ACCESO

### 9.1. Autenticación

**Método**: Cookie-based authentication (ASP.NET Core Identity-like)

**Proceso**:
1. Usuario introduce username y password
2. Sistema verifica credenciales en BD
3. Password hasheado (BCrypt o similar)
4. Si OK: Crea sesión y cookie
5. Cookie válida por sesión

**Gestión de Sesiones**:
- Cookie HttpOnly
- Cookie Secure (HTTPS)
- Expiración configurable
- Logout: Invalida cookie

### 9.2. Autorización por Roles

**Roles del Sistema**:

1. **Administrador**:
   - Acceso total
   - Gestión de usuarios
   - Aprobación de liquidaciones
   - Configuración del sistema
   - Acceso a todos los contratos
   - Acceso a todas las liquidaciones

2. **Backoffice**:
   - Gestión operativa
   - Acceso a todos los contratos
   - No recibe comisiones
   - No puede aprobar liquidaciones
   - Puede gestionar tarifas

3. **Director Comercial**:
   - Ve contratos de su estructura jerárquica
   - Ve liquidaciones de su estructura
   - Recibe comisiones jerárquicas
   - Gestión de jefes de ventas

4. **Jefe de Ventas**:
   - Ve contratos de gestores y colaboradores asignados
   - Ve liquidaciones de su equipo
   - Recibe comisiones jerárquicas
   - Gestión de gestores

5. **Gestor**:
   - Ve contratos de colaboradores asignados
   - Ve liquidaciones propias y de colaboradores
   - Recibe comisiones jerárquicas
   - Gestión de colaboradores

6. **Colaborador**:
   - Ve solo sus contratos
   - Ve solo sus liquidaciones
   - Registra contratos
   - Gestiona clientes
   - No tiene subordinados

### 9.3. Permisos Granulares

**Sistema de Permisos por Funcionalidad**:

```csharp
public class Usuario
{
    public bool PuedeVerContratos { get; set; }
    public bool PuedeVerLiquidaciones { get; set; }
    public bool PuedeVerIncidencias { get; set; }
    public bool PuedeVerOfertas { get; set; }
    public bool PuedeVerUsuarios { get; set; }
    public bool PuedeGestionarLiquidaciones { get; set; }
}
```

**Control de Acceso en Código**:
```csharp
@if (AuthService.PuedeVerContratos)
{
    <div>Contenido de contratos</div>
}

@if (AuthService.PuedeGestionarLiquidaciones)
{
    <button>Aprobar Liquidación</button>
}
```

### 9.4. Filtrado de Datos por Jerarquía

**Contratos**:
- Administrador/Backoffice: TODOS
- Director Comercial: De jefes, gestores y colaboradores en su estructura
- Jefe de Ventas: De gestores y colaboradores en su equipo
- Gestor: De colaboradores asignados
- Colaborador: Solo propios

**Implementación**:
```csharp
public async Task<List<Contrato>> ObtenerContratosPorJerarquiaAsync(int usuarioId)
{
    var usuario = await ObtenerUsuarioAsync(usuarioId);
    
    if (usuario.Rol == "Administrador" || usuario.Rol == "Backoffice")
        return await ObtenerTodosContratosAsync();
    
    if (usuario.Rol == "Director Comercial")
        return await ObtenerContratosDeEstructuraAsync(usuarioId);
    
    if (usuario.Rol == "Jefe de Ventas")
        return await ObtenerContratosDeEquipoAsync(usuarioId);
    
    if (usuario.Rol == "Gestor")
        return await ObtenerContratosDeColaboradoresAsync(usuarioId);
    
    return await ObtenerContratosPropi osAsync(usuarioId);
}
```

### 9.5. Auditoría

**Logs de Acceso**:
- Tabla: `log_accesos`
- Información registrada:
  * Usuario
  * Fecha/hora
  * IP
  * User Agent

**Logs de Activación de Contratos**:
- Tabla: `log_activacion_contratos`
- Información registrada:
  * Contrato afectado
  * Usuario que realizó el cambio
  * Estado anterior
  * Estado nuevo
  * Fecha/hora
  * Observaciones

**Trazabilidad de Comisiones**:
- Tabla: `detalle_comision_liquidacion`
- Registro completo de distribución
- Auditable por contrato

### 9.6. Seguridad de Archivos

**Almacenamiento**:
- Fuera de `wwwroot` (no accesibles directamente por URL)
- Rutas absolutas en servidor

**Descarga Controlada**:
```csharp
[Authorize]
public async Task<IActionResult> DescargarArchivo(int id)
{
    var archivo = await _ficheroService.ObtenerAsync(id);
    
    // Verificar permisos
    if (!PuedeAccederAlArchivo(usuarioActual, archivo))
        return Forbid();
    
    var bytes = await File.ReadAllBytesAsync(archivo.Ruta);
    return File(bytes, "application/octet-stream", archivo.Nombre);
}
```

**Validaciones**:
- Tamaño máximo (configurable)
- Extensiones permitidas
- Escaneo de virus (opcional)

---

## 10. CARACTERÍSTICAS DIFERENCIADORAS

### 10.1. Respecto a CRMs Tradicionales

1. **Especialización Multi-sector**:
   - Único sistema que gestiona energía, gas, telefonía y alarmas en un solo lugar
   - Formularios específicos por tipo de servicio
   - Tarifas diferenciadas con cálculo automático

2. **Sistema de Comisiones Jerárquicas**:
   - Distribución automática hasta 5 niveles
   - Configuración personalizada por usuario y proveedor
   - Trazabilidad completa por contrato
   - Liquidaciones individualizadas con PDF

3. **Integración SIPS Inteligente**:
   - Cache multi-nivel para optimización
   - Control de cuotas mensuales
   - Consulta automática de datos de suministro
   - Histórico completo de consultas

4. **Comparador de Tarifas**:
   - Autocompletado desde SIPS
   - Cálculo preciso de ahorros
   - Generación de PDF profesional
   - Comparativa multi-comercializadora

5. **Gestión Documental Integrada**:
   - Almacenamiento organizado por entidad
   - Seguridad por roles
   - Descarga controlada
   - Metadatos completos

### 10.2. Respecto a Sistemas del Sector Energético

1. **Multi-servicio Real**:
   - No solo energía, también telefonía y alarmas
   - Gestión unificada de contratos heterogéneos

2. **Liquidaciones Automatizadas**:
   - Cálculo automático de distribución jerárquica
   - PDFs personalizados por usuario
   - Control de estados (Pendiente/Aceptada/Pagada)

3. **Sistema de Ofertas**:
   - Formularios dinámicos multi-servicio
   - Gestión documental específica
   - Notificación automática

4. **Decomisiones**:
   - Registro de penalizaciones
   - Impacto automático en liquidaciones
   - Auditoría completa

### 10.3. Ventajas Técnicas

1. **Arquitectura Moderna**:
   - .NET 8.0 (última versión estable)
   - Blazor Server (SPA nativo sin JavaScript complejo)
   - Entity Framework Core (ORM moderno)

2. **Rendimiento Optimizado**:
   - Cache inteligente en memoria
   - Índices optimizados en BD
   - Consultas lazy loading

3. **Escalabilidad**:
   - Arquitectura en capas
   - Servicios desacoplados
   - Fácil extensión de módulos

4. **Mantenibilidad**:
   - Código organizado y documentado
   - Patrones de diseño establecidos
   - Testing automatizable

### 10.4. Funcionalidades Únicas (Patentables)

1. **Algoritmo de Distribución Jerárquica de Comisiones**:
   - Cálculo diferenciado (colaborador vs superiores)
   - Configuración personalizada por relación usuario-proveedor
   - Generación automática de liquidaciones individuales

2. **Sistema de Cache Inteligente SIPS**:
   - 3 niveles de cache (memoria, BD, negative)
   - Control de cuota mensual
   - Optimización de llamadas a API externa

3. **Comparador con Autocompletado SIPS**:
   - Integración SIPS para autocompletado
   - Cálculo preciso de ahorros
   - Generación de PDF comparativo

4. **Gestión Multi-servicio con Formularios Dinámicos**:
   - Formularios adaptativos según tipo de contrato
   - Validaciones específicas por servicio
   - Cálculo de comisiones diferenciado

---

## CONCLUSIÓN

**CorCRM** es un sistema integral de gestión de relaciones con clientes especializado en el sector energético y de telecomunicaciones que incorpora innovaciones técnicas significativas en:

1. **Distribución jerárquica automatizada de comisiones** con configuración personalizada
2. **Sistema de cache inteligente** para optimización de consultas a APIs externas
3. **Comparador de tarifas** con autocompletado desde sistemas de información externos
4. **Gestión multi-servicio unificada** con formularios dinámicos
5. **Proceso de liquidaciones automatizado** con generación de documentos individualizados

El sistema se diferencia de soluciones existentes por su enfoque integral en múltiples sectores (energía, gas, telefonía, alarmas), su sistema de comisiones jerárquicas sofisticado, y su capacidad de integración con sistemas externos del sector energético (SIPS).

**Elementos Patentables**:
- Algoritmo de distribución jerárquica de comisiones multi-nivel
- Sistema de cache inteligente con control de cuotas y negative caching
- Método de comparación de tarifas con autocompletado desde API externa
- Proceso automatizado de generación de liquidaciones individuales por nivel jerárquico

---

**Fecha de Documento**: 1 de junio de 2026  
**Versión**: 1.0  
**Autor**: Equipo de Desarrollo CorCRM  
**Confidencialidad**: CONFIDENCIAL - Para fines de registro de patente
