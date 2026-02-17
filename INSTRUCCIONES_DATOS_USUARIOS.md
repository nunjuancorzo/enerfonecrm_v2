# Instrucciones para añadir campos de documentación a Usuarios

## Cambios realizados

Se han añadido nuevos campos a la tabla `usuarios` para almacenar información bancaria y documentación:

### Campos añadidos:
1. **numero_cuenta** (VARCHAR(34)): IBAN del usuario
2. **tipo_entidad** (ENUM): Autónomo, PYME o N/A
3. **archivo_dni** (VARCHAR(255)): Ruta del archivo DNI
4. **archivo_cif** (VARCHAR(255)): Ruta del archivo CIF
5. **archivo_poder** (VARCHAR(255)): Ruta del archivo Poder/Escrituras
6. **archivo_contrato** (VARCHAR(255)): Ruta del archivo Contrato de Colaboración

## Instalación

### 1. Ejecutar el script SQL

Ejecuta el archivo `DatosUsuarios.sql` en tu base de datos MySQL:

```bash
mysql -u root -p enerfonecrm < EnerfoneCRM/Data/DatosUsuarios.sql
```

O desde MySQL Workbench/HeidiSQL, abre y ejecuta el archivo `DatosUsuarios.sql`.

### 2. Crear el directorio de almacenamiento

Los archivos se guardarán en `storage/usuarios/`. El sistema creará el directorio automáticamente, pero asegúrate de que la aplicación tenga permisos de escritura:

```bash
mkdir -p storage/usuarios
chmod 755 storage/usuarios
```

## Uso

### En la creación/edición de usuarios:

1. **Número de Cuenta**: Ingresa el IBAN del usuario (formato: ES91 2100 0418 4502 0005 1332)

2. **Tipo de Entidad**: Selecciona si el usuario es:
   - Autónomo
   - PYME  
   - N/A (no aplica)

3. **Documentación Adjunta**: Puedes subir hasta 4 documentos:
   - **Copia DNI**: Documento de identidad
   - **Copia CIF**: Código de Identificación Fiscal (para empresas)
   - **Copia Poder/Escrituras**: Documentos legales de la empresa
   - **Contrato de Colaboración**: Contrato firmado con el usuario

### Formatos permitidos:
- PDF (.pdf)
- Imágenes (.jpg, .jpeg, .png)
- Tamaño máximo: 10 MB por archivo

### Almacenamiento:
Los archivos se guardan en `storage/usuarios/` con el formato:
```
{timestamp}_{tipo}_{nombre_original}
```

Ejemplo: `20260209143025_dni_documento.pdf`

## Notas importantes

- Los archivos adjuntos son **opcionales**
- Los archivos se guardan con marca de tiempo para evitar duplicados
- El límite de tamaño es de 10 MB por archivo
- Los archivos antiguos no se eliminan automáticamente al actualizar un usuario
- Asegúrate de tener suficiente espacio en disco para almacenar los documentos

## Solución de problemas

### Error al subir archivos
- Verifica que el directorio `storage/usuarios` existe y tiene permisos de escritura
- Comprueba que el archivo no supera los 10 MB
- Verifica que el formato del archivo es permitido (.pdf, .jpg, .jpeg, .png)

### Los archivos no se guardan
- Comprueba los permisos del directorio storage
- Revisa los logs de la aplicación para ver errores específicos
