# 📝 NUEVA FUNCIONALIDAD: Indicador de Contratos Cargados en Plataforma Externa

**Fecha implementación:** 05/05/2026

## 🎯 Objetivo

Permitir que los colaboradores que tienen acceso directo a la plataforma de las comercializadoras puedan indicar en el CRM que un contrato ya ha sido cargado directamente en la plataforma externa, evitando así duplicaciones y facilitando el seguimiento de liquidaciones.

## ✨ Funcionalidades Implementadas

### 1. Campo Nuevo en el Modelo
- **Campo:** `cargado_en_plataforma` (TINYINT(1))
- **Valor por defecto:** `false` (0)
- **Tipo de dato:** Booleano
- **Ubicación:** Tabla `contratos`

### 2. Checkbox en Formularios de Creación/Edición

Se ha añadido un checkbox en los formularios de los 3 tipos de contratos:

#### ContratosEnergia
- **Ubicación:** Debajo del campo "Estado Contrato"
- **Texto:** "Ya está cargado en la plataforma de la comercializadora"
- **Icono:** ☁️ `bi-cloud-check`

#### ContratosTelefonia
- **Ubicación:** Debajo del campo "Comercial"
- **Texto:** "Ya está cargado en la plataforma de la operadora"
- **Icono:** ☁️ `bi-cloud-check`

#### ContratosAlarmas
- **Ubicación:** Debajo del campo "Comercial"
- **Texto:** "Ya está cargado en la plataforma de la empresa"
- **Icono:** ☁️ `bi-cloud-check`

### 3. Indicador Visual en Tablas

Se ha añadido una nueva columna en las tablas de listado de contratos:

- **Icono de columna:** ☁️ (nube con check)
- **Indicador:** ✅ Icono verde `bi-cloud-check-fill` cuando el contrato está cargado
- **Tooltip:** "Ya cargado en plataforma externa"
- **Tamaño:** 40px de ancho
- **Ubicación:** Justo después de la columna "Estado"

## 📋 Uso de la Funcionalidad

### Para Colaboradores con Acceso Directo

1. **Al crear un nuevo contrato:**
   - Completar todos los datos del contrato normalmente
   - **Marcar** el checkbox "Ya está cargado en plataforma..."
   - Guardar el contrato

2. **Al editar un contrato existente:**
   - Si ya cargaste el contrato en la plataforma externa
   - **Marca** el checkbox para indicarlo
   - Esto ayudará al equipo de administración a saber que no necesita volver a cargarlo

### Para el Equipo de Administración/Backoffice

1. **En el listado de contratos:**
   - Ver rápidamente qué contratos ya están cargados (icono ✅ verde)
   - Los contratos SIN el icono aún necesitan ser cargados en la plataforma externa

2. **Para liquidaciones:**
   - Usar este indicador como referencia para saber el estado de carga
   - Facilita la revisión antes de procesar liquidaciones

## 🔧 Detalles Técnicos

### Archivos Modificados

1. **Modelo:**
   - `Models/Contrato.cs` (3 ubicaciones)

2. **Formularios:**
   - `Components/Pages/ContratosEnergia.razor` (3 ubicaciones)
   - `Components/Pages/ContratosTelefonia.razor` (3 ubicaciones)
   - `Components/Pages/ContratosAlarmas.razor` (3 ubicaciones)

3. **Base de Datos:**
   - Script SQL: `ADD_CAMPO_CARGADO_PLATAFORMA.sql`

### Script SQL Ejecutado

```sql
ALTER TABLE contratos 
ADD COLUMN cargado_en_plataforma TINYINT(1) NOT NULL DEFAULT 0 
COMMENT 'Indica si el contrato ya está cargado directamente en la plataforma de la comercializadora';
```

### Propiedad del Modelo

```csharp
[Column("cargado_en_plataforma")]
public bool CargadoEnPlataforma { get; set; } = false;
```

## 📊 Estadísticas

Al momento de la implementación:
- **Total contratos:** 343
- **Cargados en plataforma:** 0
- **Pendientes de marcar:** 343

## 🎨 Diseño Visual

### Checkbox en Formulario
```html
<div class="form-check">
    <input class="form-check-input" type="checkbox" 
           id="cargadoEnPlataforma" 
           @bind="contratoSeleccionado.CargadoEnPlataforma" 
           disabled="@(!PuedeEditarCampo())">
    <label class="form-check-label" for="cargadoEnPlataforma">
        <i class="bi bi-cloud-check me-1"></i>
        <strong>Ya está cargado en la plataforma de la comercializadora</strong>
    </label>
    <div class="form-text">
        Marca esta casilla si el contrato ya fue cargado directamente en la plataforma externa
    </div>
</div>
```

### Indicador en Tabla
```html
<!-- Columna en Header -->
<th style="width: 40px; text-align: center;" title="Cargado en plataforma externa">
    <i class="bi bi-cloud-check"></i>
</th>

<!-- Celda en Fila -->
<td style="text-align: center;">
    @if (contrato.CargadoEnPlataforma)
    {
        <i class="bi bi-cloud-check-fill text-success" 
           style="font-size: 1.2rem;" 
           title="Ya cargado en plataforma externa"></i>
    }
</td>
```

## ⚠️ Consideraciones

1. **Permisos de Edición:** El checkbox respeta los mismos permisos que el resto del formulario (`PuedeEditarCampo()`)

2. **Valor por Defecto:** Todos los contratos nuevos tendrán el campo en `false` hasta que se marque explícitamente

3. **Contratos Existentes:** Los 343 contratos existentes tienen el campo en `false`. Si ya están cargados en plataforma, deben editarse manualmente para marcar el checkbox

4. **Liquidaciones:** Este campo es informativo y NO afecta automáticamente el proceso de liquidaciones. Es responsabilidad del equipo verificar este indicador antes de procesar pagos

## 📝 Próximos Pasos Sugeridos

1. **Revisar contratos existentes:** Marcar como cargados aquellos que ya estén en las plataformas externas

2. **Comunicar a colaboradores:** Informar sobre la nueva funcionalidad y su importancia

3. **Crear procedimiento:** Establecer un procedimiento estándar para el uso de este indicador

4. **Reportes (Futuro):** Considerar crear un reporte que muestre:
   - Contratos facturables pendientes de cargar
   - Contratos marcados como cargados por comercializadora
   - Estadísticas de carga por colaborador

## 🔗 Referencias

- Script SQL: `/ADD_CAMPO_CARGADO_PLATAFORMA.sql`
- Modelo: `/Models/Contrato.cs`
- Formularios: `/Components/Pages/Contratos*.razor`

---

**Implementado por:** GitHub Copilot  
**Solicitado por:** Equipo de Liquidaciones  
**Estado:** ✅ Completado y en Producción
