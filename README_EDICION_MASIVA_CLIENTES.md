# Edición Masiva de Clientes - Funcionalidad Tipo Excel

## 📋 Descripción

Nueva funcionalidad que permite gestionar clientes de forma masiva directamente en una interfaz web tipo Excel, eliminando la necesidad de descargar plantillas e importar archivos.

## 🎯 Características Principales

### 1. **Tabla Editable Completa**
- Todos los campos del modelo Cliente son editables
- El campo ID es de solo lectura para registros existentes
- Los nuevos registros se marcan visualmente con badge "Nuevo"

### 2. **Funcionalidad Tipo Excel**
- ✅ **Copy & Paste**: Copia y pega desde Excel manteniendo la estructura de filas y columnas
- ✅ **Múltiples celdas**: Pega rangos completos de datos
- ✅ **Añadir filas**: Agrega nuevos clientes con un solo clic
- ✅ **Eliminar filas**: Elimina filas nuevas antes de guardar

### 3. **Validación en Tiempo Real**
- Validación de campos obligatorios (Tipo Cliente, Nombre)
- Validación de formato de email
- Validación de longitud de campos
- Mensajes de error descriptivos por fila

### 4. **Guardado Masivo**
- Guarda todos los cambios en una sola operación
- Diferencia entre creación y actualización automáticamente
- Reporta éxitos y errores de forma detallada
- Recarga automática tras guardar con éxito

## 🗂️ Campos Disponibles

| Campo | Tipo | Obligatorio | Longitud | Observaciones |
|-------|------|-------------|----------|---------------|
| ID | Integer | No (auto) | - | Solo lectura para existentes |
| Tipo Cliente | Enum | **Sí** | - | Particular, Pyme, Autónomo |
| Nombre | String | **Sí** | 255 | - |
| Apellidos | String | No | 255 | - |
| DNI/CIF | String | No | 50 | - |
| Email | String | No | 255 | Validación de formato |
| Teléfono | String | No | 20 | - |
| Empresa | String | No | 255 | - |
| CIF Empresa | String | No | 20 | - |
| Tipo Vía | String | No | 50 | - |
| Dirección | String | No | 500 | - |
| Número | String | No | 20 | - |
| Escalera | String | No | 10 | - |
| Piso | String | No | 10 | - |
| Puerta | String | No | 10 | - |
| Aclarador | String | No | 255 | - |
| Población | String | No | 100 | - |
| Ciudad | String | No | 100 | - |
| Provincia | String | No | 100 | - |
| Código Postal | String | No | 10 | - |
| País | String | No | 100 | Default: España |
| IBAN | String | No | 34 | - |
| Representante | String | No | 255 | - |
| DNI Representante | String | No | 50 | - |
| CNAE | String | No | 10 | - |
| Procedencia | String | No | 50 | - |
| Comercial | String | No | 255 | - |
| Observaciones | Text | No | - | - |
| Copia Recibo Bancario | String | No | 500 | - |

## 🚀 Cómo Usar

### Acceso
1. **Desde el menú**: `Configuración > Edición Masiva de Clientes`
2. **Desde Inicio Rápido**: Banner con enlace directo

### Edición

#### Editar un Campo Existente
1. Haz clic en la celda que deseas editar
2. Modifica el valor
3. El cambio se guarda al hacer clic en "Guardar Cambios"

#### Copiar y Pegar desde Excel
1. Selecciona y copia datos en Excel (Ctrl+C / Cmd+C)
2. Haz clic en la celda inicial donde quieres pegar
3. Pega los datos (Ctrl+V / Cmd+V)
4. Los datos se distribuirán automáticamente en filas y columnas

**Ejemplo de pegado desde Excel:**
```
Nombre1    Apellido1    email1@test.com    123456789
Nombre2    Apellido2    email2@test.com    987654321
Nombre3    Apellido3    email3@test.com    555555555
```

#### Añadir Nuevos Clientes
1. Haz clic en "Añadir Fila" (esquina superior derecha o pie de tabla)
2. La nueva fila aparecerá al inicio con badge "Nuevo"
3. Completa los campos obligatorios mínimo
4. Guarda los cambios

#### Eliminar Filas Nuevas
1. Las filas nuevas (no guardadas) tienen un botón de eliminar (🗑️)
2. Haz clic en el botón para eliminar la fila
3. No es necesario guardar para eliminar (se elimina instantáneamente)

### Guardar Cambios
1. Revisa los cambios realizados
2. Haz clic en "Guardar Cambios" (esquina superior derecha)
3. El sistema validará todos los campos
4. Se mostrará un resumen de:
   - ✅ Registros creados
   - ✅ Registros actualizados
   - ❌ Errores (si los hay)

## ⚡ Atajos de Teclado

- **Ctrl/Cmd + S**: Guardar cambios
- **Ctrl/Cmd + N**: Añadir nueva fila
- **Ctrl/Cmd + V**: Pegar (en celda seleccionada)

## 🔒 Permisos

- **Rol requerido**: Administrador
- Los usuarios con otros roles no pueden acceder a esta funcionalidad

## 💡 Consejos y Mejores Prácticas

### ✅ Recomendaciones
1. **Copia desde Excel**: Asegúrate de que el orden de columnas coincida con la tabla
2. **Campos obligatorios**: Completa siempre Tipo Cliente y Nombre
3. **Validación de email**: Usa formatos válidos (usuario@dominio.com)
4. **Guarda frecuentemente**: No esperes a editar todos los registros, guarda en lotes
5. **Revisa errores**: Lee los mensajes de error y corrígelos antes de volver a guardar

### ⚠️ Limitaciones Conocidas
1. **Archivos adjuntos**: El campo "Copia Recibo Bancario" es solo la ruta/URL del archivo
2. **Paginación**: Todos los clientes se cargan simultáneamente (puede ser lento con muchos registros)
3. **Deshacer**: No hay función de deshacer, los cambios son permanentes al guardar

### 🎯 Casos de Uso Ideales
1. **Migración inicial**: Pegar grandes lotes de clientes desde Excel
2. **Corrección masiva**: Actualizar campos específicos en múltiples clientes
3. **Normalización**: Estandarizar formatos de datos (provincias, países, etc.)
4. **Limpieza**: Revisar y completar datos faltantes

## 🔧 Implementación Técnica

### Archivos Creados/Modificados

#### Nuevos Archivos
- `/Components/Pages/ClientesEdicionMasiva.razor` - Página principal
- `/wwwroot/js/clientesEdicionMasiva.js` - Lógica JavaScript para copy/paste
- `/ADD_CAMPOS_FALTANTES_CLIENTES.sql` - Script de migración DB

#### Archivos Modificados
- `/Models/Cliente.cs` - Agregados campos: Apellidos, Empresa, Cif, Ciudad, Pais
- `/Components/Layout/NavMenu.razor` - Agregado enlace en menú
- `/Components/App.razor` - Incluido script JavaScript
- `/Components/Pages/InicioRapido.razor` - Agregado banner informativo

### Tecnologías Utilizadas
- **Blazor Server**: Renderizado del lado del servidor
- **Entity Framework Core**: ORM para acceso a datos
- **Bootstrap 5**: Estilos y componentes UI
- **JavaScript nativo**: Manejo de eventos copy/paste
- **MySQL**: Base de datos

## 🐛 Solución de Problemas

### El pegado desde Excel no funciona
- **Causa**: JavaScript no se cargó correctamente
- **Solución**: Recarga la página (Ctrl+R / Cmd+R)

### Campos no se guardan
- **Causa**: Errores de validación
- **Solución**: Revisa el panel de errores y corrige los campos marcados

### La tabla tarda en cargar
- **Causa**: Muchos registros en la base de datos
- **Solución**: Considera usar filtros o paginación (futura mejora)

### Error al pegar: "No se encontró la celda"
- **Causa**: Intento de pegar fuera del área de la tabla
- **Solución**: Asegúrate de hacer clic en una celda válida antes de pegar

## 📊 Comparativa: Antes vs Ahora

| Aspecto | Método Anterior (Excel) | Nuevo Método (Web) |
|---------|-------------------------|---------------------|
| **Descarga plantilla** | ✅ Requerida | ❌ No necesaria |
| **Edición offline** | ✅ Sí | ❌ Requiere conexión |
| **Validación en tiempo real** | ❌ No | ✅ Sí |
| **Errores visibles** | ❌ Después de importar | ✅ Antes de guardar |
| **Pasos requeridos** | 4 (Descargar, Editar, Guardar, Importar) | 2 (Editar, Guardar) |
| **Interfaz** | Excel externo | Web integrada |
| **Colaboración** | ❌ No simultánea | ✅ Potencial para futuro |

## 🔮 Mejoras Futuras

### Corto Plazo
- [ ] Paginación para grandes volúmenes de datos
- [ ] Filtros en columnas
- [ ] Ordenamiento por columna
- [ ] Búsqueda rápida

### Mediano Plazo
- [ ] Edición colaborativa en tiempo real
- [ ] Historial de cambios por cliente
- [ ] Exportar selección a Excel
- [ ] Copiar formato de Excel (negritas, colores)

### Largo Plazo
- [ ] Importación de archivos adjuntos inline
- [ ] Validaciones personalizadas por empresa
- [ ] Integración con servicios externos (validación DNI/CIF)
- [ ] Autocompletado inteligente

## 📞 Soporte

Si encuentras problemas o tienes sugerencias:
1. Documenta el error (captura de pantalla)
2. Anota los pasos para reproducirlo
3. Contacta al equipo de desarrollo

---

**Fecha de implementación**: 2026-06-23  
**Versión**: 1.0  
**Autor**: CorCRM Development Team
