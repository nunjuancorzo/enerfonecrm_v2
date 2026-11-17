# Sistema de Generación de Datos de Prueba

## Overview

El sistema permite a los administradores generar datos de prueba en empresas especialmente marcadas como "empresas de prueba". Esto es útil para testing, demos y validación de funcionalidades.

## Características

✓ **Marcado de empresas de prueba**: Solo en la sección SuperAdmin → Empresas
✓ **Generación segura**: Solo empresas marcadas pueden generar datos
✓ **Prevención de duplicados**: El sistema verifica datos existentes antes de crear
✓ **Múltiples tablas**: Genera datos en 7 tablas relacionadas
✓ **Rápido y fiable**: Validación de datos y manejo de errores

## Tablas con Datos Generados

1. **Usuarios** (3 registros por empresa)
   - Admin de Prueba (admin@prueba.test)
   - Comercial de Prueba (comercial@prueba.test)
   - Proveedor de Prueba (proveedor@prueba.test)

2. **Comercializadoras** (2 registros globales)
   - Comercializadora Prueba 1
   - Comercializadora Prueba 2

3. **Operadoras** (2 registros globales)
   - Operadora Prueba 1
   - Operadora Prueba 2

4. **Empresas de Alarma** (2 registros globales)
   - Empresa Alarma Prueba 1
   - Empresa Alarma Prueba 2

5. **Tarifas** (3 tipos, 1 registro mínimo por tipo)
   - Tarifa Energía Prueba 1
   - Tarifa Luz Prueba 1
   - Tarifa Gas Prueba 1

6. **Servicios** (3 registros globales)
   - Servicio Prueba 1
   - Servicio Prueba 2
   - Servicio Prueba 3

7. **Clientes** (3 registros por empresa)
   - Cliente Prueba 1 (cliente1@prueba.test)
   - Cliente Prueba 2 (cliente2@prueba.test)
   - Cliente Prueba 3 (cliente3@prueba.test)

## Cómo usar

### 1. Marcar una empresa como empresa de prueba

1. Accede a **SuperAdmin → Empresas**
2. Haz scroll en la tabla de empresas
3. Busca la columna "Empresa de Prueba"
4. Marca el checkbox para la empresa deseada
5. Los cambios se guardan automáticamente

### 2. Generar datos de prueba

1. Accede a **SuperAdmin → Empresas**
2. Busca la empresa marcada como "Empresa de Prueba" (verás un badge rojo)
3. Haz clic en el botón **"Generar Datos"** (solo visible si la empresa es de prueba)
4. Se abrirá un diálogo confirmando la acción
5. Confirma que deseas generar datos
6. El sistema generará datos en todas las tablas asociadas
7. Verás un mensaje de éxito/error cuando finalice

### 3. Validar datos generados

1. Los datos se generan con sufijos "-PRUEBA" o "@prueba.test" para identificarlos fácilmente
2. Puedes acceder a los datos desde cualquier sección donde aparezcan (Usuarios, Clientes, etc.)
3. Los datos solo se crean si no existen duplicados

## Consideraciones

- **Seguridad**: Solo administradores pueden marcar empresas como prueba y generar datos
- **Duplicados**: El sistema NO crea duplicados si ya existen datos con el mismo patrón de búsqueda
- **Separación por empresa**: Los usuarios y clientes están separados por empresaId
- **Datos compartidos**: Comercializadoras, Operadoras, Empresas de Alarma, Tarifas y Servicios son globales (no separados por empresa)
- **Facilidad de identificación**: Todos los datos contienen "Prueba" en el nombre o "@prueba.test" en emails

## Implementación Técnica

### Archivos modificados/creados

1. **DatosPruebaService.cs** (Servicio)
   - Lógica principal de generación de datos
   - Validación de empresa de prueba
   - Prevención de duplicados

2. **ConfiguracionEmpresa.cs** (Modelo)
   - Propiedad `EsEmpresaPrueba`

3. **Empresas.razor** (UI)
   - Checkbox para marcar como empresa de prueba
   - Botón para generar datos
   - Indicador visual (badge)

4. **Program.cs** (DI)
   - Registro del servicio DatosPruebaService

5. **AddEsEmpresaPrueba.sql** (Migración)
   - Script SQL para agregar columna a la BD

### Arquitectura

```
Empresas.razor
    ↓
GenerarDatosPrueba() [UI Handler]
    ↓
DatosPruebaService.GenerarDatosPruebaAsync()
    ↓
ApplicationDbContext [Múltiples operaciones]
    ├── GenerarDatosUsuariosAsync()
    ├── GenerarDatosComercializadorasAsync()
    ├── GenerarDatosOperadorasAsync()
    ├── GenerarDatosEmpresasAlarmaAsync()
    ├── GenerarDatosTarifasAsync()
    ├── GenerarDatosServiciosAsync()
    └── GenerarDatosClientesAsync()
```

## Troubleshooting

### "Esta empresa no está marcada como empresa de prueba"
- Asegúrate de marcar el checkbox en la tabla de empresas
- El cambio debería guardarse automáticamente
- Recarga la página si el cambio no aparece

### "Error generando datos de prueba: ..."
- Revisa el mensaje de error específico
- Verifica que haya conexión a la BD
- Intenta nuevamente

### No veo el botón "Generar Datos"
- El botón solo aparece si la empresa está marcada como prueba
- Verifica que el checkbox esté marcado
- Recarga la página

## Desarrollo futuro

- Agregar más tipos de datos (Incidencias, Ofertas, etc.)
- Permitir seleccionar cantidad de registros
- Generar datos aleatorios más realistas
- Crear fixture de datos más complejos
