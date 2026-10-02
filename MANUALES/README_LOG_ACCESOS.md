# Sistema de Log de Accesos de Usuarios

## Descripción
Se ha implementado un sistema completo de registro de accesos de usuarios que permite al administrador:
- Ver todos los accesos realizados por los usuarios
- Filtrar por nombre de usuario
- Filtrar por rango de fechas
- Ordenar por usuario, rol o fecha de acceso

## Archivos creados/modificados:

### 1. Base de datos
- **Scripts/crear_tabla_log_accesos.sql**: Script para crear la tabla `log_accesos`

### 2. Modelo
- **Models/LogAcceso.cs**: Modelo de datos para el log de accesos

### 3. Servicios
- **Services/LogAccesoService.cs**: Servicio para gestionar el log de accesos
- **Services/AuthService.cs**: Modificado para registrar accesos automáticamente al hacer login

### 4. Vistas
- **Components/Pages/Usuarios/LogAccesos.razor**: Página para visualizar el log de accesos
- **Components/Pages/Usuarios/ListaUsuarios.razor**: Añadido botón "Log de Accesos"

### 5. Configuración
- **Data/ApplicationDbContext.cs**: Añadido DbSet para LogAccesos
- **Program.cs**: Registrado LogAccesoService

## Instrucciones de uso:

1. **Ejecutar el script SQL**:
   - Abre MySQL Workbench o tu cliente MySQL
   - Ejecuta el contenido del archivo `Scripts/crear_tabla_log_accesos.sql`
   - Esto creará la tabla `log_accesos` con índices optimizados

2. **Acceder al log**:
   - Inicia sesión como Administrador
   - Ve a "Gestión Usuarios"
   - Haz clic en el botón "Log de Accesos"

3. **Funcionalidades disponibles**:
   - Ver todos los accesos registrados
   - Filtrar por nombre de usuario
   - Filtrar por rango de fechas (fecha inicio y fecha fin)
   - Ordenar por usuario, rol o fecha de acceso (clic en las cabeceras)
   - Ver el total de accesos filtrados

## Funcionamiento automático:
- Cada vez que un usuario inicia sesión, se registra automáticamente:
  - ID del usuario
  - Nombre del usuario
  - Rol del usuario
  - Fecha y hora exacta del acceso

## Seguridad:
- Solo los usuarios con rol "Administrador" pueden acceder al log de accesos
- El registro de accesos NO interrumpe el proceso de login en caso de error
