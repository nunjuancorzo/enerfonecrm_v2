# Instrucciones para configurar el sistema de Incidencias

## 1. Ejecutar el script SQL

Debes ejecutar el siguiente script en tu base de datos MySQL (`enerfonecrm`):

### Opción 1: Desde línea de comandos
```bash
mysql -u root -p enerfonecrm < EnerfoneCRM/Data/Incidencias.sql
```

### Opción 2: Desde MySQL Workbench o phpMyAdmin
1. Abre MySQL Workbench o phpMyAdmin
2. Selecciona la base de datos `enerfonecrm`
3. Abre el archivo `EnerfoneCRM/Data/Incidencias.sql`
4. Ejecuta el script

### Opción 3: Desde terminal de MySQL
```bash
mysql -u root -p
```
Luego:
```sql
USE enerfonecrm;

CREATE TABLE IF NOT EXISTS incidencias (
    id INT AUTO_INCREMENT PRIMARY KEY,
    asunto VARCHAR(200) NOT NULL,
    tipo_incidencia VARCHAR(50) NOT NULL,
    prioridad VARCHAR(20) NOT NULL,
    descripcion VARCHAR(2000) NOT NULL,
    usuario_id INT NOT NULL,
    nombre_usuario VARCHAR(100) NOT NULL,
    email_usuario VARCHAR(100) NOT NULL,
    estado VARCHAR(20) NOT NULL DEFAULT 'Pendiente',
    fecha_creacion DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    fecha_actualizacion DATETIME NULL,
    observaciones_admin VARCHAR(1000) NULL,
    tiene_imagen BOOLEAN NOT NULL DEFAULT FALSE,
    nombre_imagen VARCHAR(200) NULL,
    INDEX idx_usuario_id (usuario_id),
    INDEX idx_estado (estado),
    INDEX idx_prioridad (prioridad),
    INDEX idx_fecha_creacion (fecha_creacion)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
```

## 2. Verificar que la tabla se creó correctamente

```sql
SHOW TABLES LIKE 'incidencias';
DESC incidencias;
```

## 3. Reiniciar la aplicación

Una vez ejecutado el script, la aplicación debería funcionar correctamente.

## Características del sistema de incidencias

- **Botón en el navbar**: Lleva directamente al listado de incidencias
- **Listado con filtros**: Por estado, prioridad y búsqueda de texto
- **Roles diferenciados**:
  - Administrador: Ve todas las incidencias y puede editarlas
  - Otros roles: Solo ven sus propias incidencias
- **Crear nueva incidencia**: Botón disponible en el listado
- **Estados**: Pendiente, En Proceso, Resuelta, Cerrada
- **Prioridades**: Crítica, Alta, Media, Baja
