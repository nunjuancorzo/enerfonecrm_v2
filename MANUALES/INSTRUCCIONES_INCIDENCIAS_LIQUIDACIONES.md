# Instrucciones para Implementar Sistema de Incidencias en Liquidaciones

## Paso 1: Ejecutar el Script SQL

Abre tu cliente MySQL y ejecuta el siguiente script en tu base de datos:

```sql
-- Script para crear la tabla de incidencias de liquidación
-- Fecha: 28/01/2026

CREATE TABLE IF NOT EXISTS incidencias_liquidacion (
    id INT AUTO_INCREMENT PRIMARY KEY,
    contrato_id INT NOT NULL,
    usuario_colaborador_id INT NOT NULL,
    mensaje_colaborador TEXT NOT NULL,
    fecha_creacion DATETIME NOT NULL,
    respuesta_administrador TEXT NULL,
    usuario_administrador_id INT NULL,
    fecha_respuesta DATETIME NULL,
    estado VARCHAR(50) NOT NULL DEFAULT 'Pendiente',
    
    FOREIGN KEY (contrato_id) REFERENCES contratos(id) ON DELETE CASCADE,
    FOREIGN KEY (usuario_colaborador_id) REFERENCES usuarios(id) ON DELETE CASCADE,
    FOREIGN KEY (usuario_administrador_id) REFERENCES usuarios(id) ON DELETE SET NULL,
    
    INDEX idx_contrato_id (contrato_id),
    INDEX idx_usuario_colaborador_id (usuario_colaborador_id),
    INDEX idx_estado (estado)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
```

## Paso 2: Funcionalidad Implementada

### Para Colaboradores:
1. En la página de Liquidaciones, cuando vean sus contratos facturables, cada fila tiene un botón de incidencia (⚠️).
2. Al hacer clic, se abre un modal donde pueden:
   - Ver incidencias existentes del contrato
   - Crear una nueva incidencia (solo si no hay una pendiente)
   - Ver la respuesta del administrador cuando esté disponible

### Para Administradores:
1. Pueden acceder a todas las incidencias desde el modal
2. Ver incidencias pendientes
3. Responder a las incidencias de los colaboradores
4. Una vez respondida, la incidencia cambia a estado "Respondida"

### Características:
- ✅ Un colaborador solo puede tener UNA incidencia pendiente por contrato
- ✅ Solo el administrador puede responder incidencias
- ✅ Las incidencias se visualizan con colores:
  - Naranja/Amarillo: Pendiente
  - Verde: Respondida
- ✅ Historial completo de incidencias por contrato
- ✅ Fecha y hora de creación y respuesta

## Paso 3: Probar la Funcionalidad

1. Inicia sesión como **Colaborador** que tenga contratos en estado "Act/Facturable"
2. Ve a la página **Liquidaciones**
3. Haz clic en el botón de "Ver Contratos" para tu usuario
4. Haz clic en el botón de incidencia (⚠️) en cualquier contrato
5. Crea una nueva incidencia

6. Luego, inicia sesión como **Administrador**
7. Ve a Liquidaciones y visualiza los contratos del colaborador
8. Abre la incidencia y responde

9. Vuelve a iniciar sesión como **Colaborador** para ver la respuesta

## Archivos Modificados/Creados

### Nuevos Archivos:
- `Models/IncidenciaLiquidacion.cs` - Modelo de datos
- `Services/IncidenciaLiquidacionService.cs` - Servicio para gestión de incidencias
- `Scripts/crear_tabla_incidencias_liquidacion.sql` - Script SQL

### Archivos Modificados:
- `Data/ApplicationDbContext.cs` - Agregado DbSet para incidencias
- `Program.cs` - Registrado servicio de incidencias
- `Components/Pages/Liquidaciones.razor` - Agregada funcionalidad completa de incidencias

## Notas Importantes

- Las incidencias están vinculadas a contratos específicos
- Un colaborador solo puede crear incidencias en sus propios contratos
- El sistema previene múltiples incidencias pendientes por contrato
- El administrador puede responder cualquier incidencia
- Las respuestas son definitivas (una sola respuesta por incidencia)
