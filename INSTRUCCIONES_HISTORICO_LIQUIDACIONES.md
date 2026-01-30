# Instrucciones: Histórico de Liquidaciones

## Descripción
Se ha implementado un sistema de histórico de liquidaciones que guarda automáticamente todas las liquidaciones aprobadas para poder consultarlas posteriormente.

## Configuración de Base de Datos

### Ejecutar el Script SQL
Debes ejecutar el siguiente script SQL en tu base de datos MySQL **antes de usar la funcionalidad de histórico**:

```bash
mysql -u tu_usuario -p nombre_base_datos < EnerfoneCRM/Scripts/LIQcrear_tabla_historico_liquidaciones.sql
```

O copiando y ejecutando directamente el contenido del archivo: `EnerfoneCRM/Scripts/LIQcrear_tabla_historico_liquidaciones.sql`

### Contenido del Script
El script crea la tabla `historico_liquidaciones` con los siguientes campos:
- **id**: Identificador único del registro
- **usuario_id**: ID del usuario al que se aprobó la liquidación
- **usuario_nombre**: Nombre del usuario
- **usuario_email**: Email del usuario
- **cantidad_contratos**: Total de contratos liquidados
- **contratos_energia**: Cantidad de contratos de energía liquidados
- **contratos_telefonia**: Cantidad de contratos de telefonía liquidados
- **contratos_alarmas**: Cantidad de contratos de alarmas liquidados
- **fecha_aprobacion**: Fecha y hora de aprobación de la liquidación
- **aprobado_por_id**: ID del administrador que aprobó
- **aprobado_por_nombre**: Nombre del administrador que aprobó
- **observaciones**: Campo opcional para notas adicionales

## Funcionalidades Implementadas

### 1. Botón "Aprobar" (solo para Administradores)
- Se encuentra en la tabla de liquidaciones, en la columna "Acciones"
- Al hacer clic, cambia el estado de todos los contratos del usuario de "Act/Facturable" a "Liquidado"
- Guarda automáticamente un registro en el histórico con toda la información de la liquidación
- El usuario desaparece de la lista de liquidaciones activas después de aprobar

### 2. Botón "Ver Histórico" (solo para Administradores)
- Se encuentra en la parte superior de la página de liquidaciones
- Abre un modal con todas las liquidaciones aprobadas
- Muestra información detallada: fecha, usuario, cantidad de contratos por tipo, quién aprobó, etc.
- Los registros se ordenan por fecha de aprobación (más recientes primero)

### 3. Modal de Histórico
Muestra una tabla con las siguientes columnas:
- Fecha de Aprobación
- Usuario (nombre e ID)
- Email del usuario
- Total de contratos
- Cantidad de contratos de Energía (badge amarillo)
- Cantidad de contratos de Telefonía (badge azul)
- Cantidad de contratos de Alarmas (badge rojo)
- Aprobado por (nombre e ID del administrador)
- Observaciones

## Archivos Modificados/Creados

### Nuevos Archivos
1. **Models/HistoricoLiquidacion.cs**: Modelo de datos para el histórico
2. **Scripts/LIQcrear_tabla_historico_liquidaciones.sql**: Script de creación de tabla

### Archivos Modificados
1. **Data/ApplicationDbContext.cs**: Agregado DbSet<HistoricoLiquidacion>
2. **Components/Pages/Liquidaciones.razor**:
   - Agregado botón "Ver Histórico" en el header
   - Agregado modal de histórico de liquidaciones
   - Modificado método `AprobarLiquidacion` para guardar en histórico
   - Agregados métodos `AbrirHistorico` y `CerrarHistorico`
   - Agregadas variables para manejar el estado del histórico

## Flujo de Trabajo

1. **Aprobación de Liquidación**:
   - Administrador hace clic en botón "Aprobar" (✓ verde)
   - Sistema cuenta contratos por tipo
   - Cambia estado de contratos a "Liquidado"
   - Crea registro en histórico_liquidaciones con:
     * Información del usuario
     * Cantidad de contratos por tipo
     * Fecha actual
     * Información del administrador que aprueba
   - Recarga la lista de liquidaciones (el usuario desaparece)

2. **Consulta de Histórico**:
   - Administrador hace clic en "Ver Histórico"
   - Sistema carga todos los registros del histórico
   - Muestra modal con tabla ordenada por fecha descendente
   - Permite cerrar el modal sin afectar los datos

## Notas Importantes

- ✅ La funcionalidad está **totalmente implementada**
- ⚠️ Debes **ejecutar el script SQL** antes de usar el histórico
- 🔐 Solo los **administradores** pueden aprobar liquidaciones y ver el histórico
- 📊 El histórico guarda **automáticamente** cada aprobación
- 🔄 Los datos del histórico son **permanentes** (no se eliminan al aprobar nuevas liquidaciones)
- 💾 Los contratos cambian de "Act/Facturable" a "**Liquidado**" al aprobar

## Ejemplo de Uso

1. Entrar como administrador en la página de Liquidaciones
2. Ver la lista de usuarios con contratos facturables
3. Hacer clic en el botón verde "Aprobar" (✓) del usuario deseado
4. El usuario desaparece de la lista (sus contratos ahora están en estado "Liquidado")
5. Hacer clic en "Ver Histórico" para consultar todas las liquidaciones aprobadas
6. Ver el registro recién creado con toda la información de la aprobación
