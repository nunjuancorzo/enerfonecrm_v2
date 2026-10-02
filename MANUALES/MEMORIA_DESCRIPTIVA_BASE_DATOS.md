# MEMORIA DESCRIPTIVA DE LA BASE DE DATOS
## CorCRM - Sistema de Gestión de Relaciones con Clientes

**Versión**: 2.0  
**Fecha**: 1 de junio de 2026  
**Motor de Base de Datos**: MySQL 8.0+  
**Codificación**: UTF-8 (utf8mb4_unicode_ci)

---

## ÍNDICE

1. [Descripción General](#1-descripción-general)
2. [Diagrama de Relaciones](#2-diagrama-de-relaciones)
3. [Catálogo de Tablas](#3-catálogo-de-tablas)
4. [Descripción Detallada de Tablas](#4-descripción-detallada-de-tablas)
5. [Relaciones y Claves Foráneas](#5-relaciones-y-claves-foráneas)
6. [Índices y Optimizaciones](#6-índices-y-optimizaciones)
7. [Triggers y Procedimientos](#7-triggers-y-procedimientos)
8. [Consideraciones de Seguridad](#8-consideraciones-de-seguridad)
9. [Estrategia de Backup](#9-estrategia-de-backup)

---

## 1. DESCRIPCIÓN GENERAL

### 1.1. Propósito

La base de datos **EnerfoneCRM** ha sido diseñada para gestionar integralmente un sistema CRM especializado en el sector energético y de telecomunicaciones. Su arquitectura soporta:

- Gestión de clientes y contratos multiservicio
- Sistema de comisiones jerárquicas multinivel
- Control de usuarios con roles y permisos granulares
- Liquidaciones automatizadas con trazabilidad completa
- Integración con sistemas externos (SIPS, proveedores)
- Gestión documental y auditoría de accesos

### 1.2. Características Principales

- **35 tablas principales** organizadas en módulos funcionales
- **Soporte multiservicio**: Energía (luz/gas), Telefonía, Alarmas, Fotovoltaica
- **Jerarquía de usuarios**: 5 niveles (Colaborador → Gestor → Jefe de Ventas → Director Comercial → Administrador)
- **Comisiones configurables**: Por usuario, proveedor y tipo de servicio
- **Auditoría completa**: Logs de acceso, cambios de estado, observaciones históricas
- **Escalabilidad**: Diseñada para soportar crecimiento en volumen de datos y usuarios

### 1.3. Tecnología

- **Motor**: MySQL 8.0+ (compatible con MariaDB 10.5+)
- **ORM**: Entity Framework Core 8.0
- **Codificación**: utf8mb4 para soporte completo de caracteres Unicode
- **Collation**: utf8mb4_unicode_ci (insensible a mayúsculas/minúsculas)
- **Tipo de almacenamiento**: InnoDB (transaccional, con soporte FK)

---

## 2. DIAGRAMA DE RELACIONES

### 2.1. Módulos Principales

```
┌─────────────────────────────────────────────────────────────────┐
│                        MÓDULO DE USUARIOS                         │
│  ┌──────────────┐   ┌──────────────────┐   ┌──────────────┐    │
│  │  usuarios    │───│  log_accesos     │   │configuracion_│    │
│  │              │   │                  │   │  comisiones  │    │
│  └──────┬───────┘   └──────────────────┘   └──────────────┘    │
│         │                                                         │
└─────────┼─────────────────────────────────────────────────────────┘
          │
          │ ┌────────────────────────────────────────────────────┐
          ├─┤        MÓDULO DE CLIENTES Y CONTRATOS             │
          │ │  ┌──────────────┐      ┌──────────────┐           │
          │ │  │clientes_simple│◄────┤  contratos   │           │
          │ │  │              │      │              │           │
          │ │  └──────┬───────┘      └──────┬───────┘           │
          │ │         │                     │                    │
          │ │  ┌──────▼───────┐      ┌──────▼───────┐           │
          │ │  │ficheros_     │      │ficheros_     │           │
          │ │  │clientes      │      │contratos     │           │
          │ │  └──────────────┘      └──────────────┘           │
          │ │                        ┌──────────────┐           │
          │ │                        │observaciones_│           │
          │ │                        │contratos     │           │
          │ │                        └──────────────┘           │
          │ └────────────────────────────────────────────────────┘
          │
          │ ┌────────────────────────────────────────────────────┐
          ├─┤           MÓDULO DE PROVEEDORES                    │
          │ │  ┌──────────────────┐   ┌──────────────────┐      │
          │ │  │comercializadoras │   │   operadoras     │      │
          │ │  └──────────────────┘   └──────────────────┘      │
          │ │  ┌──────────────────┐                             │
          │ │  │empresas_alarma   │                             │
          │ │  └──────────────────┘                             │
          │ └────────────────────────────────────────────────────┘
          │
          │ ┌────────────────────────────────────────────────────┐
          ├─┤              MÓDULO DE TARIFAS                     │
          │ │  ┌──────────────┐  ┌──────────────┐  ┌─────────┐ │
          │ │  │tarifasenergia│  │tarifas       │  │tarifas_ │ │
          │ │  │              │  │telefonia     │  │alarmas  │ │
          │ │  └──────────────┘  └──────────────┘  └─────────┘ │
          │ └────────────────────────────────────────────────────┘
          │
          │ ┌────────────────────────────────────────────────────┐
          └─┤      MÓDULO DE COMISIONES Y LIQUIDACIONES          │
            │  ┌──────────────────┐                              │
            │  │historico_        │                              │
            │  │liquidaciones     │                              │
            │  └────────┬─────────┘                              │
            │           │                                         │
            │  ┌────────▼─────────┐   ┌──────────────────┐      │
            │  │detalle_comision_ │   │  decomisiones    │      │
            │  │liquidacion       │   │                  │      │
            │  └──────────────────┘   └──────────────────┘      │
            │  ┌──────────────────┐                              │
            │  │incidencias_      │                              │
            │  │liquidacion       │                              │
            │  └──────────────────┘                              │
            └────────────────────────────────────────────────────┘
```

---

## 3. CATÁLOGO DE TABLAS

### 3.1. Tablas por Módulo Funcional

#### **MÓDULO DE USUARIOS Y AUTENTICACIÓN** (3 tablas)
| # | Tabla | Registros Típicos | Propósito |
|---|-------|-------------------|-----------|
| 1 | `usuarios` | 10-500 | Usuarios del sistema con roles y jerarquía |
| 2 | `log_accesos` | 1000-100K | Auditoría de accesos al sistema |
| 3 | `configuracion_comisiones` | 50-1000 | Configuración de % comisión por usuario/proveedor |

#### **MÓDULO DE CLIENTES** (2 tablas)
| # | Tabla | Registros Típicos | Propósito |
|---|-------|-------------------|-----------|
| 4 | `clientes_simple` | 1000-50K | Datos de clientes (particulares y empresas) |
| 5 | `ficheros_clientes` | 500-10K | Documentos adjuntos de clientes |

#### **MÓDULO DE CONTRATOS** (4 tablas)
| # | Tabla | Registros Típicos | Propósito |
|---|-------|-------------------|-----------|
| 6 | `contratos` | 2000-100K | Contratos multiservicio (energía, telefonía, alarmas) |
| 7 | `ficheros_contratos` | 1000-20K | Documentación de contratos |
| 8 | `observaciones_contratos` | 500-10K | Historial de anotaciones por contrato |
| 9 | `log_activacion_contratos` | 1000-50K | Trazabilidad de cambios de estado |

#### **MÓDULO DE PROVEEDORES** (3 tablas)
| # | Tabla | Registros Típicos | Propósito |
|---|-------|-------------------|-----------|
| 10 | `comercializadoras` | 10-50 | Compañías de energía |
| 11 | `operadoras` | 5-30 | Operadores de telefonía |
| 12 | `empresas_alarma` | 3-20 | Empresas de sistemas de seguridad |

#### **MÓDULO DE TARIFAS** (3 tablas)
| # | Tabla | Registros Típicos | Propósito |
|---|-------|-------------------|-----------|
| 13 | `tarifasenergia` | 100-1000 | Tarifas de luz y gas |
| 14 | `tarifastelefonia` | 50-300 | Planes de telefonía móvil y fibra |
| 15 | `tarifas_alarmas` | 20-100 | Tarifas de sistemas de alarma |

#### **MÓDULO DE COMISIONES Y LIQUIDACIONES** (5 tablas)
| # | Tabla | Registros Típicos | Propósito |
|---|-------|-------------------|-----------|
| 16 | `historico_liquidaciones` | 100-5K | Liquidaciones aprobadas por usuario |
| 17 | `detalle_comision_liquidacion` | 1000-50K | Distribución jerárquica por contrato |
| 18 | `decomisiones` | 50-2K | Penalizaciones por bajas anticipadas |
| 19 | `incidencias_liquidacion` | 20-500 | Incidencias en liquidaciones |
| 20 | `usuario_comision_proveedores` | 100-2K | % comisión por usuario y proveedor |

#### **MÓDULO DE RELACIONES N:M** (3 tablas)
| # | Tabla | Registros Típicos | Propósito |
|---|-------|-------------------|-----------|
| 21 | `usuario_comercializadoras` | 50-1K | Usuarios autorizados por comercializadora |
| 22 | `usuario_operadoras` | 30-500 | Usuarios autorizados por operadora |
| 23 | `usuario_empresas_alarma` | 20-300 | Usuarios autorizados por empresa de alarma |

#### **MÓDULO SIPS (Sistema de Información del Punto de Suministro)** (1 tabla)
| # | Tabla | Registros Típicos | Propósito |
|---|-------|-------------------|-----------|
| 24 | `historico_sips_consultas` | 500-10K | Cache de consultas a sistema SIPS |

#### **MÓDULO DE OFERTAS** (2 tablas)
| # | Tabla | Registros Típicos | Propósito |
|---|-------|-------------------|-----------|
| 25 | `solicitudes_ofertas` | 100-5K | Solicitudes de ofertas comerciales |
| 26 | `ofertas_interesados` | 100-5K | Datos de interesados en ofertas |

#### **MÓDULO DE INCIDENCIAS** (2 tablas)
| # | Tabla | Registros Típicos | Propósito |
|---|-------|-------------------|-----------|
| 27 | `incidencias` | 100-5K | Incidencias técnicas del sistema |
| 28 | `comentarios_incidencias` | 200-10K | Comentarios de seguimiento |

#### **MÓDULO DE CONFIGURACIÓN** (5 tablas)
| # | Tabla | Registros Típicos | Propósito |
|---|-------|-------------------|-----------|
| 29 | `configuracion_empresa` | 1-2 | Datos de la empresa (logo, SMTP, etc.) |
| 30 | `mensajes_bienvenida` | 1-5 | Mensajes de bienvenida por rol |
| 31 | `noticias` | 10-100 | Noticias del sistema |
| 32 | `noticias_imagenes` | 10-100 | Imágenes asociadas a noticias |
| 33 | `tareas_pendientes` | 20-500 | Tareas internas del sistema |

#### **MÓDULO DE SERVICIOS** (1 tabla)
| # | Tabla | Registros Típicos | Propósito |
|---|-------|-------------------|-----------|
| 34 | `servicios` | 10-100 | Catálogo de servicios adicionales |

**TOTAL: 35 tablas principales**

---

## 4. DESCRIPCIÓN DETALLADA DE TABLAS

### 4.1. MÓDULO DE USUARIOS Y AUTENTICACIÓN

#### **Tabla: `usuarios`**
**Propósito**: Almacena la información de todos los usuarios del sistema, incluyendo su rol, jerarquía comercial, permisos y configuración de comisiones.

| Campo | Tipo | Nulo | PK/FK | Descripción |
|-------|------|------|-------|-------------|
| `idusuarios` | INT | NO | PK | Identificador único del usuario |
| `username` | VARCHAR(45) | NO | | Nombre de usuario para login |
| `email` | VARCHAR(45) | NO | | Email del usuario (único) |
| `password` | VARCHAR(50) | NO | | Hash de la contraseña |
| `nombre` | VARCHAR(100) | SÍ | | Nombre real del usuario |
| `apellidos` | VARCHAR(100) | SÍ | | Apellidos del usuario |
| `telefono` | VARCHAR(20) | SÍ | | Teléfono de contacto (formato +34...) |
| `direccion` | VARCHAR(255) | SÍ | | Dirección completa |
| `codigo_postal` | VARCHAR(10) | SÍ | | Código postal |
| `localidad` | VARCHAR(100) | SÍ | | Localidad |
| `rol` | VARCHAR(45) | NO | | Rol: Administrador, Director Comercial, Jefe de ventas, Gestor, Colaborador, Backoffice |
| `gestor_id` | INT | SÍ | FK | ID del gestor superior (jerarquía) |
| `jefe_ventas_id` | INT | SÍ | FK | ID del jefe de ventas superior |
| `director_comercial_id` | INT | SÍ | FK | ID del director comercial superior |
| `comision` | DECIMAL(10,2) | NO | | Porcentaje de comisión base (0-100) |
| `activo` | BOOLEAN | NO | | Si el usuario está activo (puede acceder) |
| `excluir_log_acceso` | BOOLEAN | NO | | Excluir de logs de acceso |
| `recibir_notificaciones` | BOOLEAN | NO | | Recibir notificaciones por email |
| `puede_ver_clientes` | BOOLEAN | NO | | Permiso para ver clientes (Backoffice) |
| `puede_ver_contratos` | BOOLEAN | NO | | Permiso para ver contratos |
| `puede_ver_tarifas` | BOOLEAN | NO | | Permiso para ver tarifas |
| `puede_ver_liquidaciones` | BOOLEAN | NO | | Permiso para ver liquidaciones |
| `puede_ver_sips` | BOOLEAN | NO | | Permiso para ver módulo SIPS |
| `puede_ver_incidencias` | BOOLEAN | NO | | Permiso para ver incidencias |
| `puede_ver_ofertas` | BOOLEAN | NO | | Permiso para ver ofertas |
| `puede_ver_usuarios` | BOOLEAN | NO | | Permiso para ver usuarios |
| `numero_cuenta` | VARCHAR(34) | SÍ | | IBAN para pagos (formato ES...) |
| `tipo_entidad` | VARCHAR(20) | SÍ | | Tipo: N/A, Autonomo, PYME |
| `archivo_dni` | VARCHAR(500) | SÍ | | Ruta del archivo DNI |
| `archivo_cif` | VARCHAR(500) | SÍ | | Ruta del archivo CIF |
| `archivo_escrituras` | VARCHAR(500) | SÍ | | Ruta de escrituras/poderes |
| `archivo_contrato` | VARCHAR(500) | SÍ | | Ruta del contrato de colaboración |

**Índices**:
- PRIMARY KEY: `idusuarios`
- UNIQUE: `username`
- UNIQUE: `email`
- INDEX: `gestor_id`
- INDEX: `jefe_ventas_id`
- INDEX: `director_comercial_id`
- INDEX: `rol`
- INDEX: `activo`

**Relaciones**:
- AUTO-REFERENCIA: `gestor_id` → `usuarios.idusuarios`
- AUTO-REFERENCIA: `jefe_ventas_id` → `usuarios.idusuarios`
- AUTO-REFERENCIA: `director_comercial_id` → `usuarios.idusuarios`

---

#### **Tabla: `log_accesos`**
**Propósito**: Auditoría de todos los accesos al sistema (login, logout).

| Campo | Tipo | Nulo | PK/FK | Descripción |
|-------|------|------|-------|-------------|
| `id` | INT | NO | PK | Identificador único |
| `usuario_id` | INT | NO | FK | ID del usuario |
| `usuario_nombre` | VARCHAR(100) | NO | | Nombre del usuario |
| `fecha_acceso` | DATETIME | NO | | Timestamp del acceso |
| `ip` | VARCHAR(50) | SÍ | | IP desde donde se accede |
| `user_agent` | VARCHAR(500) | SÍ | | Navegador y SO |

**Índices**:
- PRIMARY KEY: `id`
- INDEX: `usuario_id`
- INDEX: `fecha_acceso`

---

#### **Tabla: `configuracion_comisiones`**
**Propósito**: Configuración de porcentajes de comisión personalizados por usuario y rol en la jerarquía.

| Campo | Tipo | Nulo | PK/FK | Descripción |
|-------|------|------|-------|-------------|
| `id` | INT | NO | PK | Identificador único |
| `usuario_id` | INT | NO | FK | ID del usuario |
| `porcentaje_colaborador` | DECIMAL(5,2) | SÍ | | % para colaboradores (ej: 70.00) |
| `porcentaje_gestor` | DECIMAL(5,2) | SÍ | | % para gestores (ej: 10.00) |
| `porcentaje_jefe_ventas` | DECIMAL(5,2) | SÍ | | % para jefes de ventas (ej: 10.00) |
| `porcentaje_director_comercial` | DECIMAL(5,2) | SÍ | | % para directores (ej: 5.00) |
| `fecha_creacion` | DATETIME | NO | | Fecha de creación |
| `fecha_modificacion` | DATETIME | SÍ | | Última modificación |

**Índices**:
- PRIMARY KEY: `id`
- UNIQUE: `usuario_id`

---

### 4.2. MÓDULO DE CLIENTES

#### **Tabla: `clientes_simple`**
**Propósito**: Registro de clientes (particulares y empresas) con datos completos de contacto y facturación.

| Campo | Tipo | Nulo | PK/FK | Descripción |
|-------|------|------|-------|-------------|
| `id` | INT | NO | PK | Identificador único |
| `tipo_cliente` | VARCHAR(50) | NO | | Tipo: Particular, Autonomo, PYME |
| `nombre` | VARCHAR(255) | NO | | Nombre o razón social |
| `dni_cif` | VARCHAR(50) | SÍ | | DNI o CIF |
| `cnae` | VARCHAR(10) | SÍ | | Código CNAE (empresas) |
| `dni_representante` | VARCHAR(50) | SÍ | | DNI del representante legal |
| `email` | VARCHAR(255) | SÍ | | Email de contacto |
| `telefono` | VARCHAR(20) | SÍ | | Teléfono de contacto |
| `tipo_via` | VARCHAR(50) | SÍ | | Tipo: Calle, Avenida, Plaza, etc. |
| `direccion` | VARCHAR(500) | SÍ | | Nombre de la vía |
| `numero` | VARCHAR(20) | SÍ | | Número de calle |
| `escalera` | VARCHAR(10) | SÍ | | Escalera |
| `piso` | VARCHAR(10) | SÍ | | Piso |
| `puerta` | VARCHAR(10) | SÍ | | Puerta |
| `aclarador` | VARCHAR(255) | SÍ | | Aclaraciones de dirección |
| `poblacion` | VARCHAR(100) | SÍ | | Población |
| `provincia` | VARCHAR(100) | SÍ | | Provincia |
| `codigo_postal` | VARCHAR(10) | SÍ | | CP |
| `iban` | VARCHAR(34) | SÍ | | IBAN para domiciliación |
| `representante` | VARCHAR(255) | SÍ | | Nombre del representante |
| `comercial` | VARCHAR(255) | SÍ | | Nombre del comercial |
| `observaciones` | TEXT | SÍ | | Notas adicionales |
| `fecha_alta` | DATETIME | SÍ | | Fecha de registro |
| `procedencia` | VARCHAR(50) | SÍ | | Origen: Captación propia, Referido, Campaña, etc. |

**Índices**:
- PRIMARY KEY: `id`
- INDEX: `dni_cif`
- INDEX: `email`
- INDEX: `telefono`
- INDEX: `comercial`
- INDEX: `tipo_cliente`
- INDEX: `procedencia`

---

#### **Tabla: `ficheros_clientes`**
**Propósito**: Almacenamiento de documentos adjuntos de clientes.

| Campo | Tipo | Nulo | PK/FK | Descripción |
|-------|------|------|-------|-------------|
| `id` | INT | NO | PK | Identificador único |
| `cliente_id` | INT | NO | FK | ID del cliente |
| `tipo_fichero` | VARCHAR(100) | NO | | Tipo: DNIParticular, DNIPyme, CIFPyme, EscriturasPyme |
| `nombre_archivo` | VARCHAR(255) | NO | | Nombre original del archivo |
| `ruta_archivo` | VARCHAR(500) | NO | | Ruta en el sistema de archivos |
| `fecha_subida` | DATETIME | NO | | Timestamp de subida |
| `subido_por` | VARCHAR(100) | SÍ | | Usuario que subió el archivo |

**Índices**:
- PRIMARY KEY: `id`
- INDEX: `cliente_id`
- INDEX: `tipo_fichero`

**Relaciones**:
- FK: `cliente_id` → `clientes_simple.id` (CASCADE DELETE)

---

### 4.3. MÓDULO DE CONTRATOS

#### **Tabla: `contratos`**
**Propósito**: Contratos multiservicio (energía, telefonía, alarmas) con toda la información técnica y comercial.

| Campo | Tipo | Nulo | PK/FK | Descripción |
|-------|------|------|-------|-------------|
| `id` | INT | NO | PK | Identificador único |
| `id_contrato_externo` | VARCHAR(100) | SÍ | | ID del contrato en sistema externo |
| `tipo` | VARCHAR(50) | SÍ | | Tipo: energia, telefonia, alarma, fotovoltaica |
| `estado` | VARCHAR(100) | SÍ | | Estado: Act/Facturable, Pendiente Activa, Baja, etc. |
| `comercial` | VARCHAR(255) | SÍ | | Nombre del comercial |
| `fecha_creacion` | DATETIME | SÍ | | Fecha de creación del registro |
| `fecha_modificacion` | DATETIME | SÍ | | Última modificación |
| `fecha_activo` | DATETIME | SÍ | | Fecha de activación del contrato |
| `fecha_alta` | DATETIME | SÍ | | Fecha de alta en sistema proveedor |
| `idCliente` | INT | SÍ | FK | ID del cliente |
| `nombre_cliente` | VARCHAR(255) | SÍ | | Nombre del cliente |
| `dni` | VARCHAR(50) | SÍ | | DNI del cliente |
| `direccion` | VARCHAR(500) | SÍ | | Dirección del suministro |
| `iban` | VARCHAR(100) | SÍ | | IBAN para domiciliación |
| `comision` | DECIMAL(10,2) | SÍ | | Comisión del contrato |
| `usuario_comercializadora_id` | INT | SÍ | FK | ID del usuario responsable |
| `servicio_id` | INT | SÍ | FK | ID del servicio |
| `historico_liquidacion_id` | INT | SÍ | FK | ID de la liquidación asociada |

**Campos específicos de ENERGÍA**:
| Campo | Tipo | Nulo | Descripción |
|-------|------|------|-------------|
| `estadoServicio` | VARCHAR(100) | SÍ | Estado del servicio |
| `en_Comercializadora` | VARCHAR(100) | SÍ | Comercializadora |
| `en_Tarifa` | VARCHAR(255) | SÍ | Nombre de la tarifa |
| `en_tarifa_id` | INT | SÍ | ID de la tarifa |
| `en_CUPS` | VARCHAR(255) | SÍ | CUPS de electricidad |
| `en_CUPSGas` | VARCHAR(255) | SÍ | CUPS de gas |
| `en_Servicios` | VARCHAR(255) | SÍ | Servicios contratados |
| `en_IBAN` | VARCHAR(100) | SÍ | IBAN para energía |
| `tipoOperacion` | VARCHAR(100) | SÍ | Alta, Cambio titular, etc. |
| `potencia_contratada_p1` | DECIMAL(10,2) | SÍ | Potencia periodo 1 (kW) |
| `potencia_contratada_p2` | DECIMAL(10,2) | SÍ | Potencia periodo 2 (kW) |
| `potencia_contratada_p3` | DECIMAL(10,2) | SÍ | Potencia periodo 3 (kW) |
| `potencia_contratada_p4` | DECIMAL(10,2) | SÍ | Potencia periodo 4 (kW) |
| `potencia_contratada_p5` | DECIMAL(10,2) | SÍ | Potencia periodo 5 (kW) |
| `potencia_contratada_p6` | DECIMAL(10,2) | SÍ | Potencia periodo 6 (kW) |
| `consumo_ultimos_12_meses` | DECIMAL(18,2) | SÍ | Consumo anual luz (kWh) |
| `consumo_anual_gas` | DECIMAL(18,2) | SÍ | Consumo anual gas (kWh) |
| `peaje_luz` | VARCHAR(50) | SÍ | Peaje eléctrico (2.0TD, 3.0TD, etc.) |
| `peaje_gas` | VARCHAR(50) | SÍ | Peaje de gas |

**Campos específicos de TELEFONÍA**:
| Campo | Tipo | Nulo | Descripción |
|-------|------|------|-------------|
| `tel_Operadora` | VARCHAR(100) | SÍ | Operadora de telefonía |
| `tel_Tarifa` | VARCHAR(255) | SÍ | Nombre de la tarifa |
| `tel_tarifa_id` | INT | SÍ | ID de la tarifa |
| `tel_LineaPrincipal` | VARCHAR(20) | SÍ | Número de línea principal |
| `tel_SegundaLineaPrincipal` | VARCHAR(20) | SÍ | Segunda línea principal |
| `tel_Portabilidad` | BOOLEAN | SÍ | Si hay portabilidad |
| `tel_IBAN` | VARCHAR(100) | SÍ | IBAN para telefonía |
| `tel_TipoMovil` | VARCHAR(50) | SÍ | Contrato, Prepago, Tarjeta |
| `tel_ICC` | VARCHAR(50) | SÍ | ICC de la tarjeta SIM |
| `tel_Promocion` | VARCHAR(255) | SÍ | Promoción aplicada |
| `tel_Fibra` | BOOLEAN | SÍ | Contrata fibra |
| `tel_Tarifa_Fibra` | VARCHAR(255) | SÍ | Tarifa de fibra |
| `tel_Segunda_Residencia` | BOOLEAN | SÍ | Es segunda residencia |
| `tel_Lineas_Adicionales` | INT | SÍ | Número de líneas adicionales |

**Campos específicos de ALARMAS**:
| Campo | Tipo | Nulo | Descripción |
|-------|------|------|-------------|
| `al_EmpresaAlarma` | VARCHAR(100) | SÍ | Empresa de alarmas |
| `al_Tarifa` | VARCHAR(255) | SÍ | Tarifa de alarma |
| `al_tarifa_id` | INT | SÍ | ID de la tarifa |
| `al_TarifaKitInicial` | VARCHAR(255) | SÍ | Tarifa del kit inicial |
| `al_TarifaKitInicial_id` | INT | SÍ | ID tarifa kit |
| `al_TarifaCampana` | VARCHAR(255) | SÍ | Tarifa de campaña |
| `al_TarifaCampana_id` | INT | SÍ | ID tarifa campaña |
| `al_IBAN` | VARCHAR(100) | SÍ | IBAN para alarma |
| `al_Direccion_Instalacion` | VARCHAR(500) | SÍ | Dirección de instalación |

**Campos de control**:
| Campo | Tipo | Nulo | Descripción |
|-------|------|------|-------------|
| `cargado_plataforma` | BOOLEAN | NO | Si se cargó en plataforma externa (defecto: false) |

**Índices**:
- PRIMARY KEY: `id`
- INDEX: `idCliente`
- INDEX: `tipo`
- INDEX: `estado`
- INDEX: `comercial`
- INDEX: `en_CUPS`
- INDEX: `en_CUPSGas`
- INDEX: `tel_LineaPrincipal`
- INDEX: `usuario_comercializadora_id`
- INDEX: `historico_liquidacion_id`
- INDEX: `fecha_activo`
- INDEX: `cargado_plataforma`

**Relaciones**:
- FK: `idCliente` → `clientes_simple.id`
- FK: `usuario_comercializadora_id` → `usuarios.idusuarios`
- FK: `historico_liquidacion_id` → `historico_liquidaciones.id`

---

#### **Tabla: `ficheros_contratos`**
**Propósito**: Documentos adjuntos a contratos (facturas, documentación, etc.).

| Campo | Tipo | Nulo | PK/FK | Descripción |
|-------|------|------|-------|-------------|
| `id` | INT | NO | PK | Identificador único |
| `contrato_id` | INT | NO | FK | ID del contrato |
| `tipo_fichero` | VARCHAR(100) | NO | | Tipo: Factura, Contrato, Documentación, etc. |
| `nombre_archivo` | VARCHAR(255) | NO | | Nombre del archivo |
| `ruta_archivo` | VARCHAR(500) | NO | | Ruta del archivo |
| `fecha_subida` | DATETIME | NO | | Fecha de subida |
| `subido_por` | VARCHAR(100) | SÍ | | Usuario que subió el archivo |

**Índices**:
- PRIMARY KEY: `id`
- INDEX: `contrato_id`
- INDEX: `tipo_fichero`

---

#### **Tabla: `observaciones_contratos`**
**Propósito**: Historial de observaciones/anotaciones sobre contratos.

| Campo | Tipo | Nulo | PK/FK | Descripción |
|-------|------|------|-------|-------------|
| `id` | INT | NO | PK | Identificador único |
| `contrato_id` | INT | NO | FK | ID del contrato |
| `observacion` | TEXT | NO | | Texto de la observación |
| `fecha` | DATETIME | NO | | Fecha de la observación |
| `usuario` | VARCHAR(100) | SÍ | | Usuario que realizó la observación |

**Índices**:
- PRIMARY KEY: `id`
- INDEX: `contrato_id`
- INDEX: `fecha`

---

#### **Tabla: `log_activacion_contratos`**
**Propósito**: Trazabilidad de cambios de estado de contratos.

| Campo | Tipo | Nulo | PK/FK | Descripción |
|-------|------|------|-------|-------------|
| `id` | INT | NO | PK | Identificador único |
| `contrato_id` | INT | NO | FK | ID del contrato |
| `estado_anterior` | VARCHAR(100) | SÍ | | Estado previo |
| `estado_nuevo` | VARCHAR(100) | NO | | Estado nuevo |
| `fecha_cambio` | DATETIME | NO | | Timestamp del cambio |
| `usuario` | VARCHAR(100) | SÍ | | Usuario que realizó el cambio |
| `observacion` | TEXT | SÍ | | Nota sobre el cambio |

**Índices**:
- PRIMARY KEY: `id`
- INDEX: `contrato_id`
- INDEX: `fecha_cambio`

---

### 4.4. MÓDULO DE PROVEEDORES

#### **Tabla: `comercializadoras`**
**Propósito**: Compañías comercializadoras de energía.

| Campo | Tipo | Nulo | PK/FK | Descripción |
|-------|------|------|-------|-------------|
| `id` | INT | NO | PK | Identificador único |
| `nombre` | VARCHAR(255) | NO | | Nombre de la comercializadora |
| `descripcion` | TEXT | SÍ | | Descripción |
| `activa` | BOOLEAN | NO | | Si está activa (defecto: true) |
| `logo_archivo` | VARCHAR(255) | SÍ | | Nombre del archivo del logo |
| `logo_contenido` | LONGBLOB | SÍ | | Contenido binario del logo |
| `fecha_creacion` | DATETIME | NO | | Fecha de creación |
| `porcentaje_comision` | DECIMAL(5,2) | SÍ | | % de comisión por defecto |

**Índices**:
- PRIMARY KEY: `id`
- UNIQUE: `nombre`
- INDEX: `activa`

---

#### **Tabla: `operadoras`**
**Propósito**: Operadores de telefonía.

| Campo | Tipo | Nulo | PK/FK | Descripción |
|-------|------|------|-------|-------------|
| `id` | INT | NO | PK | Identificador único |
| `nombre` | VARCHAR(255) | NO | | Nombre de la operadora |
| `descripcion` | TEXT | SÍ | | Descripción |
| `activa` | BOOLEAN | NO | | Si está activa |
| `logo_archivo` | VARCHAR(255) | SÍ | | Nombre del archivo del logo |
| `logo_contenido` | LONGBLOB | SÍ | | Contenido binario del logo |
| `fecha_creacion` | DATETIME | NO | | Fecha de creación |
| `requiere_icc` | BOOLEAN | NO | | Si requiere ICC (defecto: false) |
| `porcentaje_comision` | DECIMAL(5,2) | SÍ | | % de comisión por defecto |

**Índices**:
- PRIMARY KEY: `id`
- UNIQUE: `nombre`
- INDEX: `activa`

---

#### **Tabla: `empresas_alarma`**
**Propósito**: Empresas de sistemas de seguridad/alarma.

| Campo | Tipo | Nulo | PK/FK | Descripción |
|-------|------|------|-------|-------------|
| `id` | INT | NO | PK | Identificador único |
| `nombre` | VARCHAR(255) | NO | | Nombre de la empresa |
| `descripcion` | TEXT | SÍ | | Descripción |
| `activa` | BOOLEAN | NO | | Si está activa |
| `logo_archivo` | VARCHAR(255) | SÍ | | Nombre del archivo del logo |
| `logo_contenido` | LONGBLOB | SÍ | | Contenido binario del logo |
| `fecha_creacion` | DATETIME | NO | | Fecha de creación |
| `porcentaje_comision` | DECIMAL(5,2) | SÍ | | % de comisión por defecto |

**Índices**:
- PRIMARY KEY: `id`
- UNIQUE: `nombre`
- INDEX: `activa`

---

### 4.5. MÓDULO DE TARIFAS

#### **Tabla: `tarifasenergia`**
**Propósito**: Catálogo de tarifas de luz y gas con precios por periodo.

| Campo | Tipo | Nulo | PK/FK | Descripción |
|-------|------|------|-------|-------------|
| `id` | INT | NO | PK | Identificador único |
| `empresa` | VARCHAR(255) | NO | | Comercializadora |
| `tipo` | VARCHAR(255) | NO | | Tipo: Luz, Gas, Luz+Gas |
| `nombre` | VARCHAR(255) | NO | | Nombre de la tarifa |
| `potencia1` a `potencia6` | VARCHAR(255) | SÍ | | Precio potencia P1-P6 (€/kW/día) |
| `energia1` a `energia6` | VARCHAR(255) | SÍ | | Precio energía P1-P6 (€/kWh) |
| `precio` | VARCHAR(255) | SÍ | | Precio fijo (tarifa plana) |
| `comision` | DECIMAL(10,2) | NO | | Comisión en € |
| `activa` | BOOLEAN | NO | | Si está activa (defecto: true) |
| `penalizacion` | DECIMAL(10,2) | SÍ | | Penalización por baja anticipada (€) |
| `observaciones` | TEXT | SÍ | | Notas sobre la tarifa |

**Índices**:
- PRIMARY KEY: `id`
- INDEX: `empresa`
- INDEX: `tipo`
- INDEX: `activa`

---

#### **Tabla: `tarifastelefonia`**
**Propósito**: Catálogo de tarifas de telefonía móvil y fibra.

| Campo | Tipo | Nulo | PK/FK | Descripción |
|-------|------|------|-------|-------------|
| `id` | INT | NO | PK | Identificador único |
| `operadora` | VARCHAR(255) | NO | | Operadora |
| `nombre` | VARCHAR(255) | NO | | Nombre de la tarifa |
| `precio` | DECIMAL(10,2) | NO | | Precio mensual (€) |
| `gigas` | VARCHAR(50) | SÍ | | GB incluidos |
| `minutos` | VARCHAR(50) | SÍ | | Minutos incluidos |
| `sms` | VARCHAR(50) | SÍ | | SMS incluidos |
| `descripcion` | TEXT | SÍ | | Descripción detallada |
| `comision` | DECIMAL(10,2) | NO | | Comisión en € |
| `activa` | BOOLEAN | NO | | Si está activa |
| `penalizacion` | DECIMAL(10,2) | SÍ | | Penalización por baja (€) |
| `observaciones` | TEXT | SÍ | | Notas |

**Índices**:
- PRIMARY KEY: `id`
- INDEX: `operadora`
- INDEX: `activa`

---

#### **Tabla: `tarifas_alarmas`**
**Propósito**: Catálogo de tarifas de sistemas de alarma.

| Campo | Tipo | Nulo | PK/FK | Descripción |
|-------|------|------|-------|-------------|
| `id` | INT | NO | PK | Identificador único |
| `empresa` | VARCHAR(255) | NO | | Empresa de alarmas |
| `nombre` | VARCHAR(255) | NO | | Nombre de la tarifa |
| `tipo` | VARCHAR(50) | NO | | Tipo: KitInicial, Campana |
| `precio` | DECIMAL(10,2) | NO | | Precio mensual (€) |
| `descripcion` | TEXT | SÍ | | Descripción |
| `comision` | DECIMAL(10,2) | NO | | Comisión en € |
| `activa` | BOOLEAN | NO | | Si está activa |
| `penalizacion` | DECIMAL(10,2) | SÍ | | Penalización por baja (€) |
| `observaciones` | TEXT | SÍ | | Notas |

**Índices**:
- PRIMARY KEY: `id`
- INDEX: `empresa`
- INDEX: `tipo`
- INDEX: `activa`

---

### 4.6. MÓDULO DE COMISIONES Y LIQUIDACIONES

#### **Tabla: `historico_liquidaciones`**
**Propósito**: Registro de liquidaciones aprobadas por usuario.

| Campo | Tipo | Nulo | PK/FK | Descripción |
|-------|------|------|-------|-------------|
| `id` | INT | NO | PK | Identificador único |
| `usuario_id` | INT | NO | FK | ID del usuario liquidado |
| `usuario_nombre` | VARCHAR(100) | NO | | Nombre del usuario |
| `usuario_email` | VARCHAR(255) | SÍ | | Email del usuario |
| `cantidad_contratos` | INT | NO | | Número de contratos |
| `contratos_energia` | INT | NO | | Contratos de energía |
| `contratos_telefonia` | INT | NO | | Contratos de telefonía |
| `contratos_alarmas` | INT | NO | | Contratos de alarmas |
| `fecha_aprobacion` | DATETIME | NO | | Fecha de aprobación |
| `aprobado_por_id` | INT | NO | FK | ID del admin que aprobó |
| `aprobado_por_nombre` | VARCHAR(100) | NO | | Nombre del admin |
| `observaciones` | VARCHAR(500) | SÍ | | Observaciones |
| `estado` | VARCHAR(50) | NO | | Estado: En incidencia, Aceptada, Liquidada |
| `fecha_en_incidencia` | DATETIME | SÍ | | Fecha de incidencia |
| `fecha_aceptada` | DATETIME | SÍ | | Fecha de aceptación |
| `fecha_liquidada` | DATETIME | SÍ | | Fecha de pago |
| `total_comisiones` | DECIMAL(10,2) | SÍ | | Total a pagar |

**Índices**:
- PRIMARY KEY: `id`
- INDEX: `usuario_id`
- INDEX: `fecha_aprobacion`
- INDEX: `estado`
- INDEX: `aprobado_por_id`

---

#### **Tabla: `detalle_comision_liquidacion`**
**Propósito**: Distribución jerárquica de comisiones por contrato. **TABLA CLAVE DEL SISTEMA**.

| Campo | Tipo | Nulo | PK/FK | Descripción |
|-------|------|------|-------|-------------|
| `id` | INT | NO | PK | Identificador único |
| `historico_liquidacion_id` | INT | NO | FK | ID de la liquidación |
| `contrato_id` | INT | NO | FK | ID del contrato |
| `tipo_contrato` | VARCHAR(20) | SÍ | | energia, telefonia, alarma |
| `comision_base` | DECIMAL(10,2) | NO | | Comisión base del contrato (100%) |
| **COLABORADOR** | | | | |
| `colaborador_id` | INT | NO | FK | ID del colaborador |
| `comision_colaborador` | DECIMAL(10,2) | NO | | Importe para colaborador |
| `porcentaje_colaborador` | DECIMAL(5,2) | NO | | % aplicado |
| **GESTOR** | | | | |
| `gestor_id` | INT | SÍ | FK | ID del gestor (si existe) |
| `comision_gestor` | DECIMAL(10,2) | SÍ | | Importe para gestor |
| `porcentaje_gestor` | DECIMAL(5,2) | SÍ | | % aplicado |
| **JEFE DE VENTAS** | | | | |
| `jefe_ventas_id` | INT | SÍ | FK | ID del jefe de ventas |
| `comision_jefe_ventas` | DECIMAL(10,2) | SÍ | | Importe para jefe |
| `porcentaje_jefe_ventas` | DECIMAL(5,2) | SÍ | | % aplicado |
| **DIRECTOR COMERCIAL** | | | | |
| `director_comercial_id` | INT | SÍ | FK | ID del director |
| `comision_director_comercial` | DECIMAL(10,2) | SÍ | | Importe para director |
| `porcentaje_director_comercial` | DECIMAL(5,2) | SÍ | | % aplicado |
| **ADMINISTRADOR** | | | | |
| `administrador_id` | INT | SÍ | FK | ID del administrador |
| `comision_administrador` | DECIMAL(10,2) | SÍ | | Resto no distribuido |
| `fecha_creacion` | DATETIME | NO | | Fecha de creación |

**Índices**:
- PRIMARY KEY: `id`
- INDEX: `historico_liquidacion_id`
- INDEX: `contrato_id`
- INDEX: `colaborador_id`
- INDEX: `gestor_id`
- INDEX: `jefe_ventas_id`
- INDEX: `director_comercial_id`

**Nota importante**: Esta tabla permite auditoría completa de la distribución jerárquica. Cada contrato tiene UNA entrada que registra TODA la cadena de comisiones.

---

#### **Tabla: `decomisiones`**
**Propósito**: Registro de penalizaciones por bajas anticipadas de contratos.

| Campo | Tipo | Nulo | PK/FK | Descripción |
|-------|------|------|-------|-------------|
| `id` | INT | NO | PK | Identificador único |
| `contrato_id` | INT | NO | FK | ID del contrato dado de baja |
| `tipo_contrato` | VARCHAR(20) | SÍ | | energia, telefonia, alarma |
| `usuario_id` | INT | NO | FK | Usuario afectado |
| `importe_penalizacion` | DECIMAL(10,2) | NO | | Importe de la penalización |
| `tipo_decomision` | VARCHAR(50) | NO | | Total o Proporcional |
| `fecha_baja` | DATETIME | NO | | Fecha de baja del contrato |
| `fecha_creacion` | DATETIME | NO | | Fecha de registro |
| `observaciones` | TEXT | SÍ | | Notas |
| `aplicada` | BOOLEAN | NO | | Si se aplicó (defecto: false) |
| `fecha_aplicacion` | DATETIME | SÍ | | Fecha de aplicación |

**Índices**:
- PRIMARY KEY: `id`
- INDEX: `contrato_id`
- INDEX: `usuario_id`
- INDEX: `fecha_baja`
- INDEX: `aplicada`

---

#### **Tabla: `incidencias_liquidacion`**
**Propósito**: Incidencias reportadas en liquidaciones.

| Campo | Tipo | Nulo | PK/FK | Descripción |
|-------|------|------|-------|-------------|
| `id` | INT | NO | PK | Identificador único |
| `historico_liquidacion_id` | INT | NO | FK | ID de la liquidación |
| `usuario_id` | INT | NO | FK | Usuario que reporta |
| `descripcion` | TEXT | NO | | Descripción de la incidencia |
| `fecha_creacion` | DATETIME | NO | | Fecha de reporte |
| `resuelta` | BOOLEAN | NO | | Si está resuelta |
| `fecha_resolucion` | DATETIME | SÍ | | Fecha de resolución |
| `respuesta` | TEXT | SÍ | | Respuesta del admin |

**Índices**:
- PRIMARY KEY: `id`
- INDEX: `historico_liquidacion_id`
- INDEX: `usuario_id`
- INDEX: `resuelta`

---

#### **Tabla: `usuario_comision_proveedores`**
**Propósito**: Configuración de % comisión específico por usuario y proveedor.

| Campo | Tipo | Nulo | PK/FK | Descripción |
|-------|------|------|-------|-------------|
| `id` | INT | NO | PK | Identificador único |
| `usuario_id` | INT | NO | FK | ID del usuario |
| `tipo_proveedor` | VARCHAR(50) | NO | | comercializadora, operadora, empresa_alarma |
| `proveedor_id` | INT | NO | | ID del proveedor |
| `porcentaje_comision` | DECIMAL(5,2) | NO | | % de comisión |
| `fecha_creacion` | DATETIME | NO | | Fecha de creación |
| `fecha_modificacion` | DATETIME | SÍ | | Última modificación |

**Índices**:
- PRIMARY KEY: `id`
- UNIQUE: `usuario_id`, `tipo_proveedor`, `proveedor_id`
- INDEX: `usuario_id`

---

### 4.7. MÓDULO DE RELACIONES N:M

#### **Tabla: `usuario_comercializadoras`**
**Propósito**: Relación N:M entre usuarios y comercializadoras autorizadas.

| Campo | Tipo | Nulo | PK/FK | Descripción |
|-------|------|------|-------|-------------|
| `id` | INT | NO | PK | Identificador único |
| `usuario_id` | INT | NO | FK | ID del usuario |
| `comercializadora_id` | INT | NO | FK | ID de la comercializadora |

**Índices**:
- PRIMARY KEY: `id`
- UNIQUE: `usuario_id`, `comercializadora_id`
- INDEX: `usuario_id`
- INDEX: `comercializadora_id`

---

#### **Tabla: `usuario_operadoras`**
**Propósito**: Relación N:M entre usuarios y operadoras autorizadas.

| Campo | Tipo | Nulo | PK/FK | Descripción |
|-------|------|------|-------|-------------|
| `id` | INT | NO | PK | Identificador único |
| `usuario_id` | INT | NO | FK | ID del usuario |
| `operadora_id` | INT | NO | FK | ID de la operadora |

**Índices**:
- PRIMARY KEY: `id`
- UNIQUE: `usuario_id`, `operadora_id`
- INDEX: `usuario_id`
- INDEX: `operadora_id`

---

#### **Tabla: `usuario_empresas_alarma`**
**Propósito**: Relación N:M entre usuarios y empresas de alarma autorizadas.

| Campo | Tipo | Nulo | PK/FK | Descripción |
|-------|------|------|-------|-------------|
| `id` | INT | NO | PK | Identificador único |
| `usuario_id` | INT | NO | FK | ID del usuario |
| `empresa_alarma_id` | INT | NO | FK | ID de la empresa de alarma |

**Índices**:
- PRIMARY KEY: `id`
- UNIQUE: `usuario_id`, `empresa_alarma_id`
- INDEX: `usuario_id`
- INDEX: `empresa_alarma_id`

---

### 4.8. MÓDULO SIPS

#### **Tabla: `historico_sips_consultas`**
**Propósito**: Cache de consultas al Sistema de Información del Punto de Suministro (SIPS) para evitar consultas repetidas.

| Campo | Tipo | Nulo | PK/FK | Descripción |
|-------|------|------|-------|-------------|
| `id` | INT | NO | PK | Identificador único |
| `cups` | VARCHAR(255) | NO | | CUPS consultado |
| `respuesta_json` | LONGTEXT | NO | | Respuesta completa en JSON |
| `fecha_consulta` | DATETIME | NO | | Fecha de la consulta |
| `usuario_id` | INT | SÍ | FK | Usuario que hizo la consulta |
| `exito` | BOOLEAN | NO | | Si la consulta fue exitosa |

**Índices**:
- PRIMARY KEY: `id`
- INDEX: `cups`
- INDEX: `fecha_consulta`
- INDEX: `usuario_id`

---

### 4.9. MÓDULO DE OFERTAS

#### **Tabla: `solicitudes_ofertas`**
**Propósito**: Solicitudes de ofertas comerciales realizadas por usuarios.

| Campo | Tipo | Nulo | PK/FK | Descripción |
|-------|------|------|-------|-------------|
| `id` | INT | NO | PK | Identificador único |
| `tipo_oferta` | VARCHAR(50) | NO | | energia, telefonia, alarma, fotovoltaica |
| `usuario_id` | INT | NO | FK | Usuario solicitante |
| `usuario_nombre` | VARCHAR(100) | SÍ | | Nombre del usuario |
| `fecha_solicitud` | DATETIME | NO | | Fecha de solicitud |
| `estado` | VARCHAR(50) | NO | | Pendiente, En proceso, Completada |
| `datos_formulario` | LONGTEXT | SÍ | | JSON con datos del formulario |
| `archivo_adjunto` | VARCHAR(500) | SÍ | | Ruta del archivo adjunto |
| `observaciones` | TEXT | SÍ | | Notas adicionales |

**Índices**:
- PRIMARY KEY: `id`
- INDEX: `usuario_id`
- INDEX: `tipo_oferta`
- INDEX: `estado`
- INDEX: `fecha_solicitud`

---

#### **Tabla: `ofertas_interesados`**
**Propósito**: Datos de contacto de interesados en ofertas.

| Campo | Tipo | Nulo | PK/FK | Descripción |
|-------|------|------|-------|-------------|
| `id` | INT | NO | PK | Identificador único |
| `solicitud_oferta_id` | INT | NO | FK | ID de la solicitud |
| `nombre` | VARCHAR(255) | NO | | Nombre del interesado |
| `telefono` | VARCHAR(20) | SÍ | | Teléfono de contacto |
| `email` | VARCHAR(255) | SÍ | | Email de contacto |
| `direccion` | VARCHAR(500) | SÍ | | Dirección |
| `observaciones` | TEXT | SÍ | | Notas |
| `fecha_creacion` | DATETIME | NO | | Fecha de registro |

**Índices**:
- PRIMARY KEY: `id`
- INDEX: `solicitud_oferta_id`

---

### 4.10. MÓDULO DE INCIDENCIAS

#### **Tabla: `incidencias`**
**Propósito**: Sistema de tickets para incidencias técnicas del CRM.

| Campo | Tipo | Nulo | PK/FK | Descripción |
|-------|------|------|-------|-------------|
| `id` | INT | NO | PK | Identificador único |
| `titulo` | VARCHAR(255) | NO | | Título de la incidencia |
| `descripcion` | TEXT | NO | | Descripción detallada |
| `prioridad` | VARCHAR(50) | NO | | Baja, Media, Alta, Crítica |
| `estado` | VARCHAR(50) | NO | | Abierta, En proceso, Resuelta, Cerrada |
| `usuario_id` | INT | NO | FK | Usuario que reporta |
| `usuario_nombre` | VARCHAR(100) | SÍ | | Nombre del usuario |
| `fecha_creacion` | DATETIME | NO | | Fecha de creación |
| `fecha_actualizacion` | DATETIME | SÍ | | Última actualización |
| `imagen_adjunta` | VARCHAR(500) | SÍ | | Ruta de imagen adjunta |
| `asignado_a_id` | INT | SÍ | FK | Usuario asignado |

**Índices**:
- PRIMARY KEY: `id`
- INDEX: `usuario_id`
- INDEX: `estado`
- INDEX: `prioridad`
- INDEX: `asignado_a_id`

---

#### **Tabla: `comentarios_incidencias`**
**Propósito**: Comentarios de seguimiento en incidencias.

| Campo | Tipo | Nulo | PK/FK | Descripción |
|-------|------|------|-------|-------------|
| `id` | INT | NO | PK | Identificador único |
| `incidencia_id` | INT | NO | FK | ID de la incidencia |
| `usuario_id` | INT | NO | FK | Usuario que comenta |
| `usuario_nombre` | VARCHAR(100) | SÍ | | Nombre del usuario |
| `comentario` | TEXT | NO | | Texto del comentario |
| `fecha_creacion` | DATETIME | NO | | Fecha de creación |

**Índices**:
- PRIMARY KEY: `id`
- INDEX: `incidencia_id`
- INDEX: `usuario_id`

---

### 4.11. MÓDULO DE CONFIGURACIÓN

#### **Tabla: `configuracion_empresa`**
**Propósito**: Configuración global de la empresa (logo, datos SMTP, etc.).

| Campo | Tipo | Nulo | PK/FK | Descripción |
|-------|------|------|-------|-------------|
| `id` | INT | NO | PK | Identificador único (siempre 1) |
| `nombre_empresa` | VARCHAR(255) | SÍ | | Nombre de la empresa |
| `logo_archivo` | VARCHAR(255) | SÍ | | Nombre del archivo del logo |
| `logo_contenido` | LONGBLOB | SÍ | | Contenido binario del logo |
| `smtp_host` | VARCHAR(255) | SÍ | | Servidor SMTP |
| `smtp_port` | INT | SÍ | | Puerto SMTP |
| `smtp_usuario` | VARCHAR(255) | SÍ | | Usuario SMTP |
| `smtp_password` | VARCHAR(255) | SÍ | | Contraseña SMTP |
| `smtp_ssl` | BOOLEAN | SÍ | | Usar SSL |
| `email_remitente` | VARCHAR(255) | SÍ | | Email remitente |
| `modulo_sips_activo` | BOOLEAN | NO | | Si el módulo SIPS está activo (defecto: false) |
| `desactivar_modulo_liquidaciones` | BOOLEAN | NO | | Desactivar liquidaciones (defecto: false) |

**Índices**:
- PRIMARY KEY: `id`

---

#### **Tabla: `mensajes_bienvenida`**
**Propósito**: Mensajes de bienvenida personalizados por rol de usuario.

| Campo | Tipo | Nulo | PK/FK | Descripción |
|-------|------|------|-------|-------------|
| `id` | INT | NO | PK | Identificador único |
| `rol` | VARCHAR(50) | NO | | Rol del usuario |
| `mensaje` | TEXT | NO | | Mensaje de bienvenida |
| `activo` | BOOLEAN | NO | | Si está activo |

**Índices**:
- PRIMARY KEY: `id`
- UNIQUE: `rol`

---

#### **Tabla: `noticias`**
**Propósito**: Noticias/anuncios del sistema.

| Campo | Tipo | Nulo | PK/FK | Descripción |
|-------|------|------|-------|-------------|
| `id` | INT | NO | PK | Identificador único |
| `titulo` | VARCHAR(255) | NO | | Título de la noticia |
| `contenido` | TEXT | NO | | Contenido de la noticia |
| `fecha_publicacion` | DATETIME | NO | | Fecha de publicación |
| `autor` | VARCHAR(100) | SÍ | | Autor de la noticia |
| `activa` | BOOLEAN | NO | | Si está activa |

**Índices**:
- PRIMARY KEY: `id`
- INDEX: `fecha_publicacion`
- INDEX: `activa`

---

#### **Tabla: `noticias_imagenes`**
**Propósito**: Imágenes asociadas a noticias.

| Campo | Tipo | Nulo | PK/FK | Descripción |
|-------|------|------|-------|-------------|
| `id` | INT | NO | PK | Identificador único |
| `noticia_id` | INT | NO | FK | ID de la noticia |
| `nombre_archivo` | VARCHAR(255) | NO | | Nombre del archivo |
| `ruta_archivo` | VARCHAR(500) | NO | | Ruta del archivo |
| `orden` | INT | NO | | Orden de visualización |

**Índices**:
- PRIMARY KEY: `id`
- INDEX: `noticia_id`

---

#### **Tabla: `tareas_pendientes`**
**Propósito**: Tareas internas del sistema (cron jobs, procesos pendientes).

| Campo | Tipo | Nulo | PK/FK | Descripción |
|-------|------|------|-------|-------------|
| `id` | INT | NO | PK | Identificador único |
| `tipo` | VARCHAR(100) | NO | | Tipo de tarea |
| `descripcion` | TEXT | SÍ | | Descripción |
| `fecha_creacion` | DATETIME | NO | | Fecha de creación |
| `fecha_ejecucion` | DATETIME | SÍ | | Fecha de ejecución |
| `completada` | BOOLEAN | NO | | Si está completada |
| `resultado` | TEXT | SÍ | | Resultado de la ejecución |

**Índices**:
- PRIMARY KEY: `id`
- INDEX: `completada`
- INDEX: `tipo`

---

### 4.12. MÓDULO DE SERVICIOS

#### **Tabla: `servicios`**
**Propósito**: Catálogo de servicios adicionales/complementarios.

| Campo | Tipo | Nulo | PK/FK | Descripción |
|-------|------|------|-------|-------------|
| `id` | INT | NO | PK | Identificador único |
| `nombre` | VARCHAR(255) | NO | | Nombre del servicio |
| `descripcion` | TEXT | SÍ | | Descripción |
| `precio` | DECIMAL(10,2) | SÍ | | Precio del servicio |
| `activo` | BOOLEAN | NO | | Si está activo |

**Índices**:
- PRIMARY KEY: `id`
- INDEX: `activo`

---

## 5. RELACIONES Y CLAVES FORÁNEAS

### 5.1. Relaciones Principales

#### **Auto-referencias en `usuarios`**
```sql
usuarios.gestor_id → usuarios.idusuarios (ON DELETE SET NULL)
usuarios.jefe_ventas_id → usuarios.idusuarios (ON DELETE SET NULL)
usuarios.director_comercial_id → usuarios.idusuarios (ON DELETE SET NULL)
```

#### **Cliente → Contratos (1:N)**
```sql
contratos.idCliente → clientes_simple.id (ON DELETE SET NULL)
```

#### **Usuario → Contratos (1:N)**
```sql
contratos.usuario_comercializadora_id → usuarios.idusuarios (ON DELETE SET NULL)
```

#### **Liquidación → Contratos (1:N)**
```sql
contratos.historico_liquidacion_id → historico_liquidaciones.id (ON DELETE SET NULL)
```

#### **Liquidación → Detalles de Comisión (1:N)**
```sql
detalle_comision_liquidacion.historico_liquidacion_id → historico_liquidaciones.id (ON DELETE CASCADE)
```

#### **Usuarios ↔ Proveedores (N:M)**
```sql
usuario_comercializadoras.usuario_id → usuarios.idusuarios (ON DELETE CASCADE)
usuario_comercializadoras.comercializadora_id → comercializadoras.id (ON DELETE CASCADE)

usuario_operadoras.usuario_id → usuarios.idusuarios (ON DELETE CASCADE)
usuario_operadoras.operadora_id → operadoras.id (ON DELETE CASCADE)

usuario_empresas_alarma.usuario_id → usuarios.idusuarios (ON DELETE CASCADE)
usuario_empresas_alarma.empresa_alarma_id → empresas_alarma.id (ON DELETE CASCADE)
```

---

## 6. ÍNDICES Y OPTIMIZACIONES

### 6.1. Estrategia de Indexación

**Índices primarios** (PKs):
- Todas las tablas tienen un índice PRIMARY KEY en el campo `id` o `idusuarios`

**Índices únicos** (UNIQUE):
- `usuarios.username`
- `usuarios.email`
- `comercializadoras.nombre`
- `operadoras.nombre`
- `empresas_alarma.nombre`
- Combinaciones en tablas N:M para evitar duplicados

**Índices de búsqueda**:
- Campos de estado (`activo`, `estado`, `completada`)
- Campos de fechas (`fecha_creacion`, `fecha_acceso`, `fecha_aprobacion`)
- Campos de relación (`usuario_id`, `cliente_id`, `contrato_id`)
- Campos de búsqueda frecuente (`cups`, `dni_cif`, `telefono`)

### 6.2. Optimizaciones Aplicadas

1. **Índices compuestos** en tablas de relación N:M para garantizar unicidad
2. **Índices en campos de filtrado** comunes (estado, rol, tipo)
3. **Índices en campos de ordenación** (fecha_creacion DESC)
4. **Cache en tabla `historico_sips_consultas`** para evitar consultas repetidas a API externa

---

## 7. TRIGGERS Y PROCEDIMIENTOS

### 7.1. Procedimientos Almacenados

#### **AddColumnIfNotExists**
Patrón utilizado para agregar columnas de forma segura:

```sql
DELIMITER //
CREATE PROCEDURE AddColumnIfNotExists()
BEGIN
    DECLARE CONTINUE HANDLER FOR SQLEXCEPTION BEGIN END;
    ALTER TABLE tabla ADD COLUMN columna TIPO;
END //
DELIMITER ;

CALL AddColumnIfNotExists();
DROP PROCEDURE AddColumnIfNotExists;
```

### 7.2. Triggers (No implementados actualmente)

**Potenciales triggers para futuras versiones**:
- `BEFORE INSERT` en `contratos`: Validar CUPS/ICC según tipo
- `AFTER UPDATE` en `contratos`: Registrar cambios en `log_activacion_contratos`
- `AFTER INSERT` en `historico_liquidaciones`: Actualizar estado de contratos

---

## 8. CONSIDERACIONES DE SEGURIDAD

### 8.1. Autenticación y Autorización

- **Passwords**: Almacenados en campo `usuarios.password` (hash)
- **Roles**: 6 roles con permisos granulares (Administrador, Director Comercial, Jefe de ventas, Gestor, Colaborador, Backoffice)
- **Permisos por módulo**: Campos `puede_ver_*` en tabla `usuarios` para rol Backoffice

### 8.2. Auditoría

- **Log de accesos**: Tabla `log_accesos` con IP y user-agent
- **Trazabilidad de contratos**: Tabla `log_activacion_contratos`
- **Observaciones históricas**: Tabla `observaciones_contratos`
- **Cambios de liquidaciones**: Tabla `incidencias_liquidacion`

### 8.3. Protección de Datos

- **GDPR/LOPD**: Campos sensibles en tablas separadas con FK CASCADE DELETE
- **Encriptación**: Contraseñas con hash (responsabilidad de la aplicación)
- **Backups**: Estrategia diaria con retención de 30 días

---

## 9. ESTRATEGIA DE BACKUP

### 9.1. Backup Completo

**Frecuencia**: Diario a las 02:00 AM
**Retención**: 30 días
**Comando**:
```bash
mysqldump -u usuario -p --single-transaction --routines --triggers \
  --databases EnerfoneCRM > backup_$(date +%Y%m%d).sql
```

### 9.2. Backup Incremental

**Frecuencia**: Cada 6 horas
**Método**: Binary log (binlog) de MySQL
**Configuración** en `my.cnf`:
```ini
[mysqld]
log-bin=mysql-bin
expire_logs_days=7
```

### 9.3. Recuperación ante Desastres

**RTO (Recovery Time Objective)**: 4 horas
**RPO (Recovery Point Objective)**: 6 horas

**Procedimiento de restauración**:
```bash
# 1. Restaurar backup completo
mysql -u usuario -p EnerfoneCRM < backup_20260601.sql

# 2. Aplicar binary logs incrementales
mysqlbinlog mysql-bin.000001 | mysql -u usuario -p EnerfoneCRM
```

---

## RESUMEN ESTADÍSTICO

| Métrica | Valor |
|---------|-------|
| **Total de tablas** | 35 |
| **Campos totales** | ~450 |
| **Relaciones FK** | ~40 |
| **Índices únicos** | ~20 |
| **Índices de búsqueda** | ~80 |
| **Tablas con BLOB** | 5 (logos, imágenes) |
| **Tablas de auditoría** | 4 |
| **Tablas de configuración** | 5 |

---

## CONTROL DE VERSIONES

| Versión | Fecha | Cambios |
|---------|-------|---------|
| 1.0 | 2024-01-15 | Versión inicial de la base de datos |
| 1.5 | 2025-03-20 | Añadido módulo de liquidaciones y comisiones jerárquicas |
| 1.8 | 2025-08-10 | Implementación de módulo SIPS y ofertas |
| 2.0 | 2026-06-01 | Versión estable con 35 tablas completas |

---

**Documento generado**: 1 de junio de 2026  
**Autor**: Equipo de Desarrollo CorCRM  
**Confidencialidad**: Documento interno
