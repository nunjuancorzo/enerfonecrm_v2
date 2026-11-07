# Gemini y Volti

## Configuracion

Superadmin configura la clave y el modelo en Ajustes > Inteligencia artificial (`/ajustes/ia`). La clave no vuelve a mostrarse. Se guarda cifrada con ASP.NET Core Data Protection en `CorCRM/Secrets/gemini.protected`, dentro del directorio privado LocalApplicationData del usuario del servidor, nunca en wwwroot ni en el repositorio.

Alternativas de despliegue: `Gemini__ApiKey` o `GEMINI_API_KEY` en variables de entorno y `Gemini__Model` para el modelo. La configuracion cifrada tiene prioridad. `Gemini:SecretsDirectory` permite seleccionar un directorio privado distinto por instalacion. Conservar el anillo de claves de Data Protection entre reinicios y usar HTTPS en produccion.

Probar conexion realiza una peticion breve sin datos del CRM mediante `generateContent`. El modelo predeterminado es `gemini-3.1-flash-lite`; debe estar disponible para la clave utilizada. Consultar modelos obtiene la lista compatible con generateContent de Google para seleccionar otro modelo. Si ya hay una clave guardada, se puede cambiar el modelo dejando el campo de clave vacio. Los errores muestran el codigo HTTP y causas seguras de Google, sin revelar claves ni el contenido privado de la solicitud.

La captura de una clave compartida debe tratarse como exposicion: revocar y sustituir la clave. No incluir claves en capturas, logs, SQL o archivos versionados.

## Autorizacion por empresa

Una unica autorizacion habilita Google Gemini para Volti, importacion de facturas, importacion de DNI y explicaciones de comparativas. La concede una sola vez un Administrador de esa empresa o superadmin y se comparte entre sus usuarios. Si la empresa ya estaba autorizada para Volti, se reutiliza ese registro sin pedir otra confirmacion.

Se guarda cifrada en el servidor con el ID de empresa, el usuario que la concede y fecha UTC, conservando el nombre historico `volti-empresa-<id>.protected` dentro del directorio privado de Gemini. Se mantiene al navegar, recargar, cambiar de navegador o reiniciar. No autoriza a otras empresas. El administrador puede retirarla desde el chat y la revocacion afecta a las cuatro funciones. El servidor comprueba este permiso antes de enviar datos a Google, aunque no se muestre ninguna casilla en la pantalla.

Mantener el directorio privado y las claves de Data Protection persistentes y compartidos entre instancias de una misma instalacion. Las revisiones de datos extraidos se mantienen: no son nuevas autorizaciones a Gemini.

## Facturas y DNI

Importar factura aparece en el primer paso del comparador y utiliza exclusivamente Gemini. Si la empresa ya esta autorizada, permite seleccionar el archivo sin nuevas casillas de autorizacion. Se muestran los campos para revisar y seleccionar. Aplicar datos no guarda una comparativa ni un contrato.

Leer DNI/NIE aparece al crear clientes y dentro de los asistentes de contratos. Utiliza exclusivamente Gemini con la misma autorizacion de empresa, sin pedir otra por documento. Se comprueba el checksum de DNI/NIE y se permite revisar y corregir los campos antes de pulsar Aplicar al cliente. El cliente no se guarda automaticamente.

El domicilio se extrae y revisa desglosado: tipo de via, nombre de la calle, numero, escalera, piso, puerta, codigo postal, localidad, provincia, pais y aclarador. Solo se aplican campos con valor; los que no figuren en el documento no se inventan ni borran valores existentes. Si el domicilio aparece en el reverso, incluir esa cara en el archivo enviado.

No hay selector de proveedor ni alternativa local automatica. Si Gemini no esta configurado, rechaza la clave o falla, se informa del error sin utilizar otro motor.

Maximo 10 MB por documento. Facturas: 20 paginas. DNI/NIE con Gemini: 4 paginas. No se piden fecha de nacimiento, firma, fotografia o numero de soporte como campos extraidos. Una factura mixta, unidades ambiguas o datos no reconocidos requieren revision manual.

## Explicaciones

Explicar comparativa utiliza las ofertas seleccionadas (maximo 5) o las tres primeras. Con la autorizacion de empresa no solicita otra confirmacion para generar la explicacion. Solo envia importes, peaje, dias y tarifas, sin nombre del cliente ni CUPS. Gemini explica los calculos existentes; no calcula ni cambia el ahorro. La proyeccion anual no es una garantia de ahorro futuro.

## Volti

Volti se abre desde el boton fijo de la aplicacion. Su conversacion solo vive en memoria durante la sesion. Cambiar empresa o usuario descarta la conversacion.

Volti utiliza la autorizacion general de IA de la empresa y permite retirarla a los administradores. El servidor vuelve a comprobarla antes de las consultas y antes de enviar resultados al modelo.

Solo lectura: contratos, clientes, tareas, incidencias y ofertas. Los filtros de empresa siempre se aplican y se reutilizan los permisos y filtros de usuario del CRM. Gemini propone un plan limitado, nunca SQL; el servidor valida el plan y ejecuta consultas conocidas. No hay acciones de alta, edicion ni borrado mediante el chat.

Las listas tienen un maximo de 20 registros; los conteos abarcan todas las coincidencias autorizadas. Los filtros temporales se refieren a la fecha de creacion. No se consultan contrasenas, claves, DNI, IBAN, telefonos ni emails como campos. Los enlaces llevan a la pantalla correspondiente del CRM.

Tambien se enmascaran patrones detectables de DNI/NIE, IBAN, CUPS, emails, telefonos espanoles y claves en las preguntas y textos libres que se envian al modelo. Esto no sustituye la minimizacion de datos ni la revision de los usuarios.

## Consumo y privacidad

`Gemini:RequestsPerMinute` limita las peticiones por empresa y usuario; por defecto 20 por minuto y proceso. Cada consulta completa de Volti suele consumir dos peticiones. Las solicitudes tienen un plazo de 90 segundos y una salida maxima de 4096 tokens. `generateContent` recibe solicitudes independientes, sin IDs de conversacion almacenados por la aplicacion; esto no sustituye las condiciones de tratamiento y conservacion de datos de Google.

Antes de utilizar documentos o datos personales reales, revisar las condiciones del plan de Google, la base legal, el tratamiento internacional y el cumplimiento del RGPD. La aplicacion solo envia documentos si existe autorizacion de IA para la empresa. Las pruebas de integracion deben utilizar documentos ficticios.