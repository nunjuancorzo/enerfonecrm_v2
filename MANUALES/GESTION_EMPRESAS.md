# Gestion de empresas

## Despliegue

1. Detener el CRM y cualquier importador o proceso que escriba en su base de datos.
2. Hacer una copia completa de la base de datos y del almacenamiento de ficheros.
3. Seleccionar expresamente la base de datos del CRM en el cliente MySQL y ejecutar `EnerfoneCRM/Data/MigrarEmpresas.sql`.
4. Comprobar que existe Enerfone y que la consulta final de tablas sin empresa no devuelve filas.
5. Publicar la nueva version y arrancar el CRM. Las sesiones antiguas dejan de ser validas: todos los usuarios deben iniciar sesion de nuevo.

La migracion conserva las claves actuales, crea Enerfone y asigna a ella todos los registros sin empresa. Se puede repetir: no cambia las asignaciones validas existentes. MySQL confirma automaticamente los cambios DDL; no se debe confiar en ROLLBACK para deshacer la migracion. Ante un fallo, mantener la aplicacion detenida, corregir la causa y repetir o restaurar la copia.

No se establece una empresa por defecto para nuevas escrituras. Los importadores SQL/Python y las integraciones que escriban directamente deben indicar `id_empresa` en cada INSERT y filtrar por ella en SELECT, UPDATE y DELETE. No ejecutar importadores antiguos tras la migracion.

## Administracion

Solo superadmin tiene acceso a `/empresas`, al alta y mantenimiento de usuarios y a su edicion masiva. Desde Empresas se crea o edita una empresa y se accede a sus usuarios. El selector del menu cambia la empresa activa del superadmin y recarga la pagina para descartar los datos de la empresa anterior.

Los usuarios normales pertenecen a una empresa y mantienen los roles existentes. No pueden cambiar de empresa. Los nombres de usuario son unicos en todo el CRM, porque el inicio de sesion no pide empresa. El limite existente de un Administrador se aplica por empresa.

Las empresas se desactivan, no se eliminan con sus datos. La desactivacion impide nuevos inicios de sesion y la restauracion de sesiones de sus usuarios.

El repositorio original de ficheros se conserva para Enerfone. Su ID se configura con `Empresas:EmpresaDatosExistentesId` (por defecto 1); comprobar el ID devuelto por la migracion y configurar este valor si es distinto. Las nuevas empresas usan `storage/empresas/<id>/repositorio`, fuera de la carpeta antigua.

Las imagenes de incidencias, noticias y los adjuntos de ofertas nuevos tambien se guardan bajo el almacenamiento de su empresa. `/storage` exige una sesion protegida vigente y bloquea rutas de otras empresas. El almacenamiento de la empresa original conserva sus rutas para evitar mover ficheros existentes. No publicar esa carpeta mediante un alias de IIS/Nginx que evite el middleware del CRM.

Las entidades de empresa tienen `EmpresaId`, mapeada a `id_empresa`; salvo Usuario y Empresa, es una propiedad sombra de EF. Consultas y escrituras usan la empresa del contexto. Un contexto sin empresa no puede consultar ni escribir datos de las empresas. El perfil global de superadmin es una excepcion explicita: no tiene `id_empresa` y solo superadmin puede consultarlo o editarlo desde la interfaz, incluso sin empresa seleccionada.

## Perfil global y avisos de solicitudes de usuarios

Antes de publicar esta funcionalidad, realizar una copia de seguridad y ejecutar `EnerfoneCRM/Data/CrearPerfilSuperAdmin.sql` en la base de datos del CRM. El script es repetible y no crea ni modifica usuarios de empresas. No se ejecuta automaticamente al arrancar la aplicacion.

`/mi-perfil` y sus enlaces de menu son exclusivos de superadmin. El perfil se guarda en una unica fila de `perfil_superadmin`, independiente de la empresa seleccionada y de la tabla `usuarios`. No cambia las credenciales de acceso. Iniciar sesion como superadmin, abrir **Mi perfil**, indicar un email valido y guardar. El correo permanece tras cerrar sesion o reiniciar la aplicacion. No se reutiliza el correo de un administrador de empresa ni una direccion ficticia cuando falta configuracion.

Al registrar una solicitud en **Ajustes > Solicitar usuario**, la cuenta queda inactiva y se envia un aviso al email guardado en el perfil global. Incluye la empresa, nombre de usuario, nombre completo, rol y un enlace a gestion de usuarios, pero nunca la contrasena. Se utiliza la configuracion SMTP de la empresa solicitante; debe estar configurada y ser accesible desde el servidor IIS.

Si falta el email global, la tabla no esta creada o el envio falla, se conserva la solicitud inactiva y se muestra una advertencia; no se anuncia un envio correcto ni se crea de nuevo la solicitud. No hay reintentos automaticos ni cola de correo. Comprobar el envio con una solicitud de prueba despues del despliegue. Los usuarios de empresa siguen editandose desde su mantenimiento, no desde este perfil global.

Las sesiones estan protegidas con ASP.NET Core Data Protection. En produccion, mantener el anillo de claves persistente y compartido entre instancias del mismo despliegue; no compartirlo entre instalaciones independientes.