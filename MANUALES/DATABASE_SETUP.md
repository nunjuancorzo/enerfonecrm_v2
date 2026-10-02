# Configuración de la Base de Datos

## Instrucciones para crear la base de datos

1. Asegúrate de tener MySQL instalado y en ejecución
2. Actualiza la cadena de conexión en `appsettings.json` con tus credenciales de MySQL
3. Ejecuta el script SQL ubicado en `Data/InitialSetup.sql` en tu servidor MySQL

### Opción 1: Desde la línea de comandos
```bash
mysql -u root -p < Data/InitialSetup.sql
```

### Opción 2: Desde MySQL Workbench
1. Abre MySQL Workbench
2. Conecta a tu servidor
3. Abre el archivo `Data/InitialSetup.sql`
4. Ejecuta el script

## Usuario de Prueba

Después de ejecutar el script, tendrás disponible:
- **Email:** admin@enerfone.com
- **Password:** 123456

## Cadena de Conexión

Actualiza tu `appsettings.json`:
```json
"ConnectionStrings": {
  "DefaultConnection": "server=localhost;port=3306;database=EnerfoneCRM;user=tu_usuario;password=tu_contraseña"
}
```
