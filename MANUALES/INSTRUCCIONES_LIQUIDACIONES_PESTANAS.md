# Instrucciones para Reestructurar Liquidaciones con Pestañas

## Resumen de Cambios
Convertir la página de Liquidaciones en un sistema de 3 pestañas con estados y fechas:
1. **Pendientes**: Usuarios con contratos Act/Facturable (sin aprobar) 
2. **En Incidencia**: Liquidaciones aprobadas pero con incidencias pendientes
3. **Aceptadas**: Liquidaciones aprobadas sin incidencias
4. **Liquidadas**: Liquidaciones finalmente liquidadas (antiguo histórico)

## Paso 1: Ejecutar Script SQL
Ejecutar el archivo `ADD_ESTADO_LIQUIDACIONES.sql` en la base de datos.

## Paso 2: Modificar el Código C# en Liquidaciones.razor

### 2.1. Añadir Variables de Estado (línea ~790)
```csharp
// Variables para pestañas
private string pestanaActiva = "pendientes";
private List<HistoricoLiquidacion> liquidacionesEnIncidencia = new();
private List<HistoricoLiquidacion> liquidacionesAceptadas = new();
private List<HistoricoLiquidacion> liquidacionesLiquidadas = new();
```

### 2.2. Modificar OnAfterRenderAsync para Cargar Liquidaciones
Después de `await CargarUsuariosConContratos();`, añadir:
```csharp
await CargarLiquidacionesPorEstado();
```

### 2.3. Añadir Método para Cargar Liquidaciones
```csharp
private async Task CargarLiquidacionesPorEstado()
{
    try
    {
        await using var context = DbContextProvider.CreateDbContext();
        
        // Liquidaciones en incidencia
        liquidacionesEnIncidencia = await context.HistoricoLiquidaciones
            .Where(h => h.Estado == "En incidencia")
            .OrderByDescending(h => h.FechaEnIncidencia ?? h.FechaAprobacion)
            .ToListAsync();
        
        // Liquidaciones aceptadas
        liquidacionesAceptadas = await context.HistoricoLiquidaciones
            .Where(h => h.Estado == "Aceptada")
            .OrderByDescending(h => h.FechaAceptada ?? h.FechaAprobacion)
            .ToListAsync();
        
        // Liquidaciones liquidadas
        liquidacionesLiquidadas = await context.HistoricoLiquidaciones
            .Where(h => h.Estado == "Liquidada")
            .OrderByDescending(h => h.FechaLiquidada ?? h.FechaAprobacion)
            .ToListAsync();
            
        StateHasChanged();
    }
    catch (Exception ex)
    {
        mensajeError = $"Error al cargar liquidaciones: {ex.Message}";
    }
}
```

### 2.4. Añadir Método para Cambiar de Pestaña
```csharp
private void CambiarPestana(string pestaña)
{
    pestanaActiva = pestaña;
    StateHasChanged();
}
```

### 2.5. Modificar Método AprobarLiquidacion (línea ~1406)
Cambiar la creación del objeto HistoricoLiquidacion:
```csharp
// Calcular total de comisiones
var totalComisiones = contratos.Sum(c => c.Comision ?? 0);

var historico = new HistoricoLiquidacion
{
    UsuarioId = usuarioDb.Id,
    UsuarioNombre = usuarioDb.NombreUsuario,
    UsuarioEmail = usuarioDb.Email,
    CantidadContratos = contratos.Count,
    ContratosEnergia = contratosEnergia,
    ContratosTelefonia = contratosTelefonia,
    ContratosAlarmas = contratosAlarmas,
    FechaAprobacion = DateTime.Now,
    AprobadoPorId = AuthService.UsuarioActual!.Id,
    AprobadoPorNombre = AuthService.UsuarioActual.NombreUsuario,
    Estado = "Aceptada",  // NUEVO
    FechaAceptada = DateTime.Now,  // NUEVO
    TotalComisiones = totalComisiones  // NUEVO
};
```

Y después de `await context.SaveChangesAsync();`, añadir:
```csharp
// Recargar liquidaciones
await CargarLiquidacionesPorEstado();```

### 2.6. Añadir Métodos para Cambiar Estados

```csharp
private async Task MarcarConIncidencia(int liquidacionId)
{
    try
    {
        await using var context = DbContextProvider.CreateDbContext();
        var liquidacion = await context.HistoricoLiquidaciones.FindAsync(liquidacionId);
        
        if (liquidacion != null)
        {
            liquidacion.Estado = "En incidencia";
            liquidacion.FechaEnIncidencia = DateTime.Now;
            await context.SaveChangesAsync();
            await CargarLiquidacionesPorEstado();
        }
    }
    catch (Exception ex)
    {
        mensajeError = $"Error: {ex.Message}";
    }
}

private async Task MarcarComoAceptada(int liquidacionId)
{
    try
    {
        await using var context = DbContextProvider.CreateDbContext();
        var liquidacion = await context.HistoricoLiquidaciones.FindAsync(liquidacionId);
        
        if (liquidacion != null)
        {
            liquidacion.Estado = "Aceptada";
            if (!liquidacion.FechaAceptada.HasValue)
            {
                liquidacion.FechaAceptada = DateTime.Now;
            }
            await context.SaveChangesAsync();
            await CargarLiquidacionesPorEstado();
        }
    }
    catch (Exception ex)
    {
        mensajeError = $"Error: {ex.Message}";
    }
}

private async Task MarcarComoLiquidada(int liquidacionId)
{
    try
    {
        await using var context = DbContextProvider.CreateDbContext();
        var liquidacion = await context.HistoricoLiquidaciones.FindAsync(liquidacionId);
        
        if (liquidacion != null)
        {
            liquidacion.Estado = "Liquidada";
            liquidacion.FechaLiquidada = DateTime.Now;
            await context.SaveChangesAsync();
            await CargarLiquidacionesPorEstado();
        }
    }
    catch (Exception ex)
    {
        mensajeError = $"Error: {ex.Message}";
    }
}
```

## Paso 3: Modificar la Vista HTML

### 3.1. Reemplazar el encabezado y botón de histórico (línea ~17-25)
```html
<div class="d-flex justify-content-between align-items-center mb-4">
    <h3><i class="bi bi-cash-coin me-2"></i>Liquidaciones</h3>
</div>
```

### 3.2. Añadir Sistema de Pestañas (después del encabezado)
```html
<!-- Pestañas de Liquidaciones -->
<div class="card">
    <div class="card-header bg-white">
        <ul class="nav nav-tabs card-header-tabs" role="tablist">
            <li class="nav-item" role="presentation">
                <button class="nav-link @(pestanaActiva == "pendientes" ? "active" : "")" 
                        @onclick='() => CambiarPestana("pendientes")' 
                        type="button" role="tab">
                    <i class="bi bi-hourglass-split me-2"></i>
                    Pendientes (@usuariosConContratos.Count)
                </button>
            </li>
            <li class="nav-item" role="presentation">
                <button class="nav-link @(pestanaActiva == "incidencia" ? "active" : "")" 
                        @onclick='() => CambiarPestana("incidencia")' 
                        type="button" role="tab">
                    <i class="bi bi-exclamation-triangle me-2"></i>
                    En Incidencia (@liquidacionesEnIncidencia.Count)
                </button>
            </li>
            <li class="nav-item" role="presentation">
                <button class="nav-link @(pestanaActiva == "aceptadas" ? "active" : "")" 
                        @onclick='() => CambiarPestana("aceptadas")' 
                        type="button" role="tab">
                    <i class="bi bi-check-circle me-2"></i>
                    Aceptadas (@liquidacionesAceptadas.Count)
                </button>
            </li>
            <li class="nav-item" role="presentation">
                <button class="nav-link @(pestanaActiva == "liquidadas" ? "active" : "")" 
                        @onclick='() => CambiarPestana("liquidadas")' 
                        type="button" role="tab">
                    <i class="bi bi-cash-stack me-2"></i>
                    Liquidadas (@liquidacionesLiquidadas.Count)
                </button>
            </li>
        </ul>
    </div>
    <div class="card-body">
        <!-- Contenido de cada pestaña -->
    </div>
</div>
```

### 3.3. Contenido de Pestaña "Pendientes"
Mantener la tabla actual de usuarios con contratos facturables.

### 3.4. Contenido de Pestaña "En Incidencia"
```html
@if (pestanaActiva == "incidencia")
{
    @if (!liquidacionesEnIncidencia.Any())
    {
        <div class="alert alert-info">
            <i class="bi bi-info-circle me-2"></i>
            No hay liquidaciones en incidencia.
        </div>
    }
    else
    {
        <div class="table-responsive">
            <table class="table table-hover table-striped">
                <thead style="background-color: #dc3545; color: white;">
                    <tr>
                        <th>Fecha Incidencia</th>
                        <th>Usuario</th>
                        <th>Email</th>
                        <th class="text-center">Contratos</th>
                        <th class="text-center">Total</th>
                        <th>Aprobado Por</th>
                        <th class="text-center">Acciones</th>
                    </tr>
                </thead>
                <tbody>
                    @foreach (var liq in liquidacionesEnIncidencia)
                    {
                        <tr>
                            <td>
                                <strong>@((liq.FechaEnIncidencia ?? liq.FechaAprobacion).ToString("dd/MM/yyyy"))</strong>
                                <br />
                                <small class="text-muted">@((liq.FechaEnIncidencia ?? liq.FechaAprobacion).ToString("HH:mm"))</small>
                            </td>
                            <td>
                                <strong>@liq.UsuarioNombre</strong>
                                <br />
                                <small class="text-muted">ID: @liq.UsuarioId</small>
                            </td>
                            <td>@liq.UsuarioEmail</td>
                            <td class="text-center">
                                <span class="badge bg-primary">@liq.CantidadContratos</span>
                            </td>
                            <td class="text-center">
                                <span class="badge bg-success">@((liq.TotalComisiones ?? 0).ToString("N2"))€</span>
                            </td>
                            <td>
                                <strong>@liq.AprobadoPorNombre</strong>
                                <br />
                                <small class="text-muted">@liq.FechaAprobacion.ToString("dd/MM/yyyy HH:mm")</small>
                            </td>
                            <td class="text-center">
                                <button class="btn btn-sm btn-success me-1" @onclick="() => MarcarComoAceptada(liq.Id)" title="Marcar como Aceptada">
                                    <i class="bi bi-check-circle"></i>
                                </button>
                                <button class="btn btn-sm btn-warning me-1" @onclick="() => AbrirModalIncidencia(new Usuario { Id = liq.UsuarioId, NombreUsuario = liq.UsuarioNombre })" title="Ver Incidencias">
                                    <i class="bi bi-exclamation-triangle"></i>
                                </button>
                            </td>
                        </tr>
                    }
                </tbody>
            </table>
        </div>
    }
}
```

### 3.5. Contenido de Pestaña "Aceptadas"
```html
@if (pestanaActiva == "aceptadas")
{
    @if (!liquidacionesAceptadas.Any())
    {
        <div class="alert alert-info">
            <i class="bi bi-info-circle me-2"></i>
            No hay liquidaciones aceptadas.
        </div>
    }
    else
    {
        <div class="table-responsive">
            <table class="table table-hover table-striped">
                <thead style="background-color: #28a745; color: white;">
                    <tr>
                        <th>Fecha Aceptación</th>
                        <th>Usuario</th>
                        <th>Email</th>
                        <th class="text-center">Contratos</th>
                        <th class="text-center">Total</th>
                        <th>Aprobado Por</th>
                        <th class="text-center">Acciones</th>
                    </tr>
                </thead>
                <tbody>
                    @foreach (var liq in liquidacionesAceptadas)
                    {
                        <tr>
                            <td>
                                <strong>@((liq.FechaAceptada ?? liq.FechaAprobacion).ToString("dd/MM/yyyy"))</strong>
                                <br />
                                <small class="text-muted">@((liq.FechaAceptada ?? liq.FechaAprobacion).ToString("HH:mm"))</small>
                            </td>
                            <td>
                                <strong>@liq.UsuarioNombre</strong>
                                <br />
                                <small class="text-muted">ID: @liq.UsuarioId</small>
                            </td>
                            <td>@liq.UsuarioEmail</td>
                            <td class="text-center">
                                <span class="badge bg-primary">@liq.CantidadContratos</span>
                            </td>
                            <td class="text-center">
                                <span class="badge bg-success">@((liq.TotalComisiones ?? 0).ToString("N2"))€</span>
                            </td>
                            <td>
                                <strong>@liq.AprobadoPorNombre</strong>
                                <br />
                                <small class="text-muted">@liq.FechaAprobacion.ToString("dd/MM/yyyy HH:mm")</small>
                            </td>
                            <td class="text-center">
                                <button class="btn btn-sm btn-primary me-1" @onclick="() => MarcarComoLiquidada(liq.Id)" title="Marcar como Liquidada">
                                    <i class="bi bi-cash-stack"></i>
                                </button>
                                <button class="btn btn-sm btn-danger me-1" @onclick="() => MarcarConIncidencia(liq.Id)" title="Marcar con Incidencia">
                                    <i class="bi bi-exclamation-triangle"></i>
                                </button>
                            </td>
                        </tr>
                    }
                </tbody>
            </table>
        </div>
    }
}
```

### 3.6. Contenido de Pestaña "Liquidadas"
```html
@if (pestanaActiva == "liquidadas")
{
    @if (!liquidacionesLiquidadas.Any())
    {
        <div class="alert alert-info">
            <i class="bi bi-info-circle me-2"></i>
            No hay liquidaciones finalizadas.
        </div>
    }
    else
    {
        <div class="table-responsive">
            <table class="table table-hover table-striped">
                <thead style="background-color: #007bff; color: white;">
                    <tr>
                        <th>Fecha Liquidación</th>
                        <th>Usuario</th>
                        <th>Email</th>
                        <th class="text-center">Contratos</th>
                        <th class="text-center">E</th>
                        <th class="text-center">T</th>
                        <th class="text-center">A</th>
                        <th class="text-center">Total</th>
                        <th>Aprobado Por</th>
                        <th>Observaciones</th>
                        <th class="text-center">Acciones</th>
                    </tr>
                </thead>
                <tbody>
                    @foreach (var liq in liquidacionesLiquidadas)
                    {
                        <tr>
                            <td>
                                <strong>@((liq.FechaLiquidada ?? liq.FechaAprobacion).ToString("dd/MM/yyyy"))</strong>
                                <br />
                                <small class="text-muted">@((liq.FechaLiquidada ?? liq.FechaAprobacion).ToString("HH:mm"))</small>
                            </td>
                            <td>
                                <strong>@liq.UsuarioNombre</strong>
                                <br />
                                <small class="text-muted">ID: @liq.UsuarioId</small>
                            </td>
                            <td>@liq.UsuarioEmail</td>
                            <td class="text-center">
                                <span class="badge bg-primary">@liq.CantidadContratos</span>
                            </td>
                            <td class="text-center">
                                <span class="badge bg-warning text-dark">@liq.ContratosEnergia</span>
                            </td>
                            <td class="text-center">
                                <span class="badge bg-info">@liq.ContratosTelefonia</span>
                            </td>
                            <td class="text-center">
                                <span class="badge bg-danger">@liq.ContratosAlarmas</span>
                            </td>
                            <td class="text-center">
                                <span class="badge bg-success">@((liq.TotalComisiones ?? 0).ToString("N2"))€</span>
                            </td>
                            <td>
                                <strong>@liq.AprobadoPorNombre</strong>
                                <br />
                                <small class="text-muted">@liq.FechaAprobacion.ToString("dd/MM/yyyy HH:mm")</small>
                            </td>
                            <td>
                                @if (!string.IsNullOrEmpty(liq.Observaciones))
                                {
                                    <small>@liq.Observaciones</small>
                                }
                                else
                                {
                                    <span class="text-muted">-</span>
                                }
                            </td>
                            <td class="text-center">
                                <button class="btn btn-sm btn-info" @onclick="() => VerContratosHistorico(liq)" title="Ver Contratos">
                                    <i class="bi bi-eye"></i>
                                </button>
                            </td>
                        </tr>
                    }
                </tbody>
            </table>
        </div>
    }
}
```

## Paso 4: Eliminar Modal de Histórico

Eliminar el código del modal "Histórico de Liquidaciones" (líneas ~498-650) ya que ahora está integrado como pestaña.

También eliminar los métodos:
- `AbrirHistorico()`
- `CerrarHistorico()` 

Y las variables:
- `mostrarHistorico`
- `historicoLiquidaciones` (reemplazada por las tres listas por estado)

## Paso 5: Probar
1. Ejecutar el script SQL  
2. Reiniciar la aplicación
3. Verificar que las pestañas funcionen correctamente
4. Probar el flujo: Pendientes → Aceptadas → Liquidadas
5. Verificar que las fechas se guarden correctamente en cada transición de estado
