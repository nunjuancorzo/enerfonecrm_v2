using EnerfoneCRM.Data;
using EnerfoneCRM.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace EnerfoneCRM.Services
{
    /// <summary>
    /// Servicio para gestionar la generación automática de liquidaciones
    /// </summary>
    public class LiquidacionService
    {
        private readonly DbContextProvider _dbContextProvider;
        private readonly ComisionService _comisionService;
        private readonly ILogger<LiquidacionService> _logger;

        public LiquidacionService(DbContextProvider dbContextProvider, ComisionService comisionService, ILogger<LiquidacionService> logger)
        {
            _dbContextProvider = dbContextProvider;
            _comisionService = comisionService;
            _logger = logger;
        }

        /// <summary>
        /// Genera una liquidación pendiente para el usuario dueño cuando un contrato pasa a Act/Facturable
        /// </summary>
        public async Task<bool> GenerarLiquidacionPendienteAsync(int contratoId, int usuarioId)
        {
            try
            {
                await using var context = _dbContextProvider.CreateDbContext();

                Console.WriteLine($"[LiquidacionService] Generando liquidación pendiente para contrato {contratoId}, usuario {usuarioId}");

                // Verificar que el contrato existe y está en Act/Facturable
                var contrato = await context.Contratos.FindAsync(contratoId);
                if (contrato == null || contrato.Estado != "Act/Facturable")
                {
                    Console.WriteLine($"[LiquidacionService] Contrato no válido o no está en Act/Facturable");
                    return false;
                }

                // Verificar que el usuario existe
                var usuario = await context.Usuarios.FindAsync(usuarioId);
                if (usuario == null)
                {
                    Console.WriteLine($"[LiquidacionService] Usuario {usuarioId} no encontrado");
                    return false;
                }

                // Verificar si ya existe una liquidación pendiente para este usuario con este contrato
                var liquidacionExistente = await context.HistoricoLiquidaciones
                    .Where(l => l.UsuarioId == usuarioId && l.Estado == "Pendiente")
                    .FirstOrDefaultAsync();

                HistoricoLiquidacion liquidacion;

                if (liquidacionExistente != null)
                {
                    // Ya existe una liquidación pendiente, vincular el contrato a ella
                    Console.WriteLine($"[LiquidacionService] Liquidación pendiente existente {liquidacionExistente.Id}, vinculando contrato");
                    liquidacion = liquidacionExistente;
                }
                else
                {
                    // Crear nueva liquidación pendiente
                    liquidacion = new HistoricoLiquidacion
                    {
                        UsuarioId = usuarioId,
                        UsuarioNombre = usuario.NombreUsuario,
                        UsuarioEmail = usuario.Email ?? "",
                        Estado = "Pendiente",
                        FechaAprobacion = DateTime.Now,
                        AprobadoPorId = usuarioId, // Auto-aprobada por el sistema
                        AprobadoPorNombre = "Sistema",
                        CantidadContratos = 0,
                        TotalComisiones = 0
                    };

                    context.HistoricoLiquidaciones.Add(liquidacion);
                    await context.SaveChangesAsync();
                    Console.WriteLine($"[LiquidacionService] Nueva liquidación pendiente creada: {liquidacion.Id}");
                }

                // Vincular el contrato a la liquidación
                contrato.HistoricoLiquidacionId = liquidacion.Id;
                await context.SaveChangesAsync();

                // Obtener el usuario colaborador completo
                var colaborador = await context.Usuarios
                    .FirstOrDefaultAsync(u => u.Id == usuarioId);
                
                if (colaborador != null)
                {  
                    // Crear el detalle de comisión con el desglose jerárquico completo
                    var detalle = await _comisionService.CalcularDistribucionComisionAsync(
                        contrato, colaborador, liquidacion.Id);
                    
                    context.DetallesComisionLiquidacion.Add(detalle);
                    await context.SaveChangesAsync();
                    _logger.LogInformation("[LiquidacionService] Detalle de comisión creado para contrato {ContratoId}", contratoId);
                }

                // Recalcular totales de la liquidación
                await RecalcularTotalesLiquidacionAsync(liquidacion.Id);

                Console.WriteLine($"[LiquidacionService] Liquidación pendiente generada exitosamente");
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[LiquidacionService] Error al generar liquidación pendiente: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Genera liquidaciones pendientes para los roles adicionales cuando el usuario dueño acepta su liquidación
        /// </summary>
        public async Task<bool> GenerarLiquidacionesRolesAdicionalesAsync(int liquidacionAceptadaId)
        {
            _logger.LogInformation("[LiquidacionService] ===== MÉTODO LLAMADO ===== ID: {LiquidacionId}", liquidacionAceptadaId);
            
            try
            {
                await using var context = _dbContextProvider.CreateDbContext();

                _logger.LogInformation("[LiquidacionService] ===== GENERANDO LIQUIDACIONES ROLES ADICIONALES =====");
                _logger.LogInformation("[LiquidacionService] Liquidación aceptada ID: {LiquidacionId}", liquidacionAceptadaId);

                // Obtener la liquidación aceptada
                var liquidacionAceptada = await context.HistoricoLiquidaciones
                    .FirstOrDefaultAsync(l => l.Id == liquidacionAceptadaId);

                if (liquidacionAceptada == null)
                {
                    _logger.LogWarning("[LiquidacionService] ERROR: Liquidación {LiquidacionId} no encontrada", liquidacionAceptadaId);
                    return false;
                }

                // Obtener el usuario colaborador
                var colaborador = await context.Usuarios.FindAsync(liquidacionAceptada.UsuarioId);
                if (colaborador == null)
                {
                    _logger.LogWarning("[LiquidacionService] ERROR: Usuario {UsuarioId} no encontrado", liquidacionAceptada.UsuarioId);
                    return false;
                }

                _logger.LogInformation("[LiquidacionService] Colaborador: {NombreUsuario} (ID: {UsuarioId}, Rol: {Rol})", colaborador.NombreUsuario, colaborador.Id, colaborador.Rol);

                // Obtener contratos vinculados a esta liquidación
                var contratos = await context.Contratos
                    .Where(c => c.HistoricoLiquidacionId == liquidacionAceptadaId)
                    .ToListAsync();

                _logger.LogInformation("[LiquidacionService] Contratos vinculados: {Count}", contratos.Count);

                if (contratos.Count == 0)
                {
                    _logger.LogWarning("[LiquidacionService] No hay contratos vinculados");
                    return false;
                }

                // Obtener la jerarquía del colaborador EN CASCADA
                // Colaborador → Gestor → Jefe de Ventas → Director Comercial
                Usuario? gestor = null;
                Usuario? jefeVentas = null;
                Usuario? directorComercial = null;

                if (colaborador.GestorId.HasValue)
                {
                    gestor = await context.Usuarios
                        .Where(u => u.Id == colaborador.GestorId.Value && u.Activo)
                        .FirstOrDefaultAsync();
                    
                    _logger.LogInformation("[LiquidacionService] Gestor del colaborador: {Gestor}", gestor != null ? $"{gestor.NombreUsuario} (ID: {gestor.Id})" : "NO ENCONTRADO");
                    
                    // Del Gestor, obtener su Jefe de Ventas
                    if (gestor != null && gestor.JefeVentasId.HasValue)
                    {
                        jefeVentas = await context.Usuarios
                            .Where(u => u.Id == gestor.JefeVentasId.Value && u.Activo)
                            .FirstOrDefaultAsync();
                        
                        _logger.LogInformation("[LiquidacionService] Jefe de Ventas del gestor: {JefeVentas}", jefeVentas != null ? $"{jefeVentas.NombreUsuario} (ID: {jefeVentas.Id})" : "NO ENCONTRADO");
                        
                        // Del Jefe de Ventas, obtener su Director Comercial
                        if (jefeVentas != null && jefeVentas.DirectorComercialId.HasValue)
                        {
                            directorComercial = await context.Usuarios
                                .Where(u => u.Id == jefeVentas.DirectorComercialId.Value && u.Activo)
                                .FirstOrDefaultAsync();
                            
                            _logger.LogInformation("[LiquidacionService] Director Comercial del jefe: {DirectorComercial}", directorComercial != null ? $"{directorComercial.NombreUsuario} (ID: {directorComercial.Id})" : "NO ENCONTRADO");
                        }
                    }
                }

                // Diccionario: UsuarioId -> (Rol, Lista de comisiones)
                var comisionesPorUsuario = new Dictionary<int, (string Rol, List<(int ContratoId, decimal Comision)> Comisiones)>();

                // Calcular comisiones para cada contrato según ConfiguracionComisiones
                foreach (var contrato in contratos)
                {
                    _logger.LogInformation("[LiquidacionService] Procesando contrato {ContratoId} ({Tipo})", contrato.Id, contrato.Tipo);

                    // Determinar tipo de proveedor
                    string tipoProveedor = contrato.Tipo?.ToLower() switch
                    {
                        "energia" => "Comercializadora",
                        "telefonia" => "Operadora",
                        "telefonía" => "Operadora",
                        "alarma" => "EmpresaAlarma",
                        _ => "Comercializadora"
                    };

                    // Para simplificar, usar proveedorId = 0 para buscar configuración genérica
                    int proveedorId = 0;

                    // Buscar configuración para este colaborador y proveedor
                    var configuracion = await _comisionService.ObtenerConfiguracionAsync(colaborador.Id, tipoProveedor, proveedorId);

                    // Si no hay configuración, usar valores por defecto
                    var pctGestor = configuracion?.PorcentajeGestor ?? 10m;
                    var pctJefeVentas = configuracion?.PorcentajeJefeVentas ?? 10m;
                    var pctDirectorComercial = configuracion?.PorcentajeDirectorComercial ?? 0m;

                    _logger.LogInformation("[LiquidacionService] Config para contrato {ContratoId}: Gestor={PctGestor}%, Jefe={PctJefe}%, Director={PctDirector}%", contrato.Id, pctGestor, pctJefeVentas, pctDirectorComercial);

                    // Obtener comisión base: primero del contrato, luego de la tarifa
                    decimal comisionBase = 0;
                    switch (contrato.Tipo?.ToLower())
                    {
                        case "energia":
                        case "energía":
                            // Primero intentar con la comisión del contrato
                            comisionBase = contrato.Comision ?? 0;
                            // Si no tiene, buscar en la tarifa
                            if (comisionBase == 0 && contrato.EnTarifaId.HasValue)
                            {
                                var tarifa = await context.TarifasEnergia.FindAsync(contrato.EnTarifaId.Value);
                                comisionBase = tarifa?.Comision ?? 0;
                            }
                            break;
                        case "telefonia":
                        case "telefonía":
                            // Primero intentar con la comisión del contrato
                            comisionBase = contrato.Comision ?? 0;
                            // Si no tiene, buscar en la tarifa
                            if (comisionBase == 0 && contrato.TarifaTelId.HasValue)
                            {
                                var tarifa = await context.TarifasTelefonia.FindAsync(contrato.TarifaTelId.Value);
                                comisionBase = tarifa?.ComisionNew ?? 0;
                            }
                            break;
                        case "alarma":
                            comisionBase = contrato.Comision ?? 0;
                            break;
                    }

                    _logger.LogInformation("[LiquidacionService] Comisión base contrato {ContratoId}: {ComisionBase:N2}€", contrato.Id, comisionBase);

                    if (comisionBase == 0)
                    {
                        _logger.LogWarning("[LiquidacionService] Contrato {ContratoId} sin comisión base, saltando", contrato.Id);
                        continue;
                    }

                    // Calcular comisiones para cada rol
                    if (gestor != null && pctGestor > 0)
                    {
                        var comisionGestor = Math.Round(comisionBase * pctGestor / 100, 2);
                        if (!comisionesPorUsuario.ContainsKey(gestor.Id))
                            comisionesPorUsuario[gestor.Id] = ("Gestor", new List<(int, decimal)>());
                        comisionesPorUsuario[gestor.Id].Comisiones.Add((contrato.Id, comisionGestor));
                        _logger.LogInformation("[LiquidacionService]   Gestor {NombreUsuario}: {Comision:N2}€", gestor.NombreUsuario, comisionGestor);
                    }

                    if (jefeVentas != null && pctJefeVentas > 0)
                    {
                        var comisionJefe = Math.Round(comisionBase * pctJefeVentas / 100, 2);
                        if (!comisionesPorUsuario.ContainsKey(jefeVentas.Id))
                            comisionesPorUsuario[jefeVentas.Id] = ("Jefe de Ventas", new List<(int, decimal)>());
                        comisionesPorUsuario[jefeVentas.Id].Comisiones.Add((contrato.Id, comisionJefe));
                        _logger.LogInformation("[LiquidacionService]   Jefe {NombreUsuario}: {Comision:N2}€", jefeVentas.NombreUsuario, comisionJefe);
                    }

                    if (directorComercial != null && pctDirectorComercial > 0)
                    {
                        var comisionDirector = Math.Round(comisionBase * pctDirectorComercial / 100, 2);
                        if (!comisionesPorUsuario.ContainsKey(directorComercial.Id))
                            comisionesPorUsuario[directorComercial.Id] = ("Director Comercial", new List<(int, decimal)>());
                        comisionesPorUsuario[directorComercial.Id].Comisiones.Add((contrato.Id, comisionDirector));
                        _logger.LogInformation("[LiquidacionService]   Director {NombreUsuario}: {Comision:N2}€", directorComercial.NombreUsuario, comisionDirector);
                    }
                }

                _logger.LogInformation("[LiquidacionService] Total roles adicionales con comisiones: {Count}", comisionesPorUsuario.Count);

                // Crear/actualizar liquidaciones pendientes para cada rol
                foreach (var kvp in comisionesPorUsuario)
                {
                    var usuarioId = kvp.Key;
                    var (rolNombre, comisiones) = kvp.Value;

                    var usuario = await context.Usuarios.FindAsync(usuarioId);
                    if (usuario == null || !usuario.Activo)
                        continue;

                    var totalComision = comisiones.Sum(c => c.Comision);
                    var cantidadContratos = comisiones.Count;

                    _logger.LogInformation("[LiquidacionService] {Rol} {NombreUsuario}: {Cantidad} contratos, {Total:N2}€", rolNombre, usuario.NombreUsuario, cantidadContratos, totalComision);

                    // Buscar liquidación pendiente existente
                    var liquidacionExistente = await context.HistoricoLiquidaciones
                        .Where(l => l.UsuarioId == usuarioId && l.Estado == "Pendiente")
                        .FirstOrDefaultAsync();

                    HistoricoLiquidacion liquidacionRol;

                    if (liquidacionExistente != null)
                    {
                        liquidacionRol = liquidacionExistente;
                        _logger.LogInformation("[LiquidacionService] Vinculando contratos a liquidación pendiente existente ID: {LiquidacionId}", liquidacionRol.Id);
                    }
                    else
                    {
                        // Crear nueva liquidación pendiente
                        liquidacionRol = new HistoricoLiquidacion
                        {
                            UsuarioId = usuarioId,
                            UsuarioNombre = usuario.NombreUsuario,
                            UsuarioEmail = usuario.Email ?? "",
                            Estado = "Pendiente",
                            FechaAprobacion = DateTime.Now,
                            AprobadoPorId = colaborador.Id,
                            AprobadoPorNombre = colaborador.NombreUsuario,
                            CantidadContratos = 0,
                            TotalComisiones = 0
                        };

                        context.HistoricoLiquidaciones.Add(liquidacionRol);
                        await context.SaveChangesAsync();
                        _logger.LogInformation("[LiquidacionService] Nueva liquidación pendiente creada ID: {LiquidacionId}", liquidacionRol.Id);
                    }

                    // Los contratos ya tienen DetalleComisionLiquidacion creados por la liquidación del colaborador
                    // La liquidación del rol adicional lee esos detalles por su ID (GestorId, JefeVentasId, DirectorComercialId)
                    // ACTUALIZACIÓN: Crear registros en DetallesComisionLiquidacion para vincular contratos a la liquidación del rol
                    _logger.LogInformation("[LiquidacionService] Creando {Cantidad} registros en DetallesComisionLiquidacion para liquidación {LiquidacionId} ({Rol})", comisiones.Count, liquidacionRol.Id, rolNombre);
                    
                    foreach (var (contratoId, comision) in comisiones)
                    {
                        var detalleExistente = await context.DetallesComisionLiquidacion
                            .FirstOrDefaultAsync(d => d.HistoricoLiquidacionId == liquidacionRol.Id && d.ContratoId == contratoId);
                        
                        if (detalleExistente == null)
                        {
                            var nuevoDetalle = new DetalleComisionLiquidacion
                            {
                                HistoricoLiquidacionId = liquidacionRol.Id,
                                ContratoId = contratoId,
                                ComisionColaborador = 0, // El colaborador ya tiene su liquidación
                                ColaboradorId = 0, // No aplica para roles adicionales
                                ComisionGestor = rolNombre == "Gestor" ? comision : 0,
                                GestorId = rolNombre == "Gestor" ? usuarioId : null,
                                ComisionJefeVentas = rolNombre == "Jefe de Ventas" ? comision : 0,
                                JefeVentasId = rolNombre == "Jefe de Ventas" ? usuarioId : null,
                                ComisionDirectorComercial = rolNombre == "Director Comercial" ? comision : 0,
                                DirectorComercialId = rolNombre == "Director Comercial" ? usuarioId : null
                            };
                            context.DetallesComisionLiquidacion.Add(nuevoDetalle);
                            _logger.LogInformation("[LiquidacionService]   Agregado: ContratoId={ContratoId}, Comision={Comision:N2}€, UsuarioId={UsuarioId}", contratoId, comision, usuarioId);
                        }
                        else
                        {
                            _logger.LogWarning("[LiquidacionService]   Ya existe: ContratoId={ContratoId}", contratoId);
                        }
                    }
                    await context.SaveChangesAsync();
                    
                    _logger.LogInformation("[LiquidacionService] {Cantidad} contratos vinculados a liquidación {LiquidacionId} para {Rol}", comisiones.Count, liquidacionRol.Id, rolNombre);

                    // Recalcular totales sumando las comisiones de este rol
                    liquidacionRol.CantidadContratos = liquidacionRol.CantidadContratos + cantidadContratos;
                    liquidacionRol.TotalComisiones = (liquidacionRol.TotalComisiones ?? 0) + totalComision;

                    context.HistoricoLiquidaciones.Update(liquidacionRol);
                    await context.SaveChangesAsync();
                    
                    _logger.LogInformation("[LiquidacionService] Liquidación {LiquidacionId} actualizada: {Cantidad} contratos, {Total:N2}€", liquidacionRol.Id, liquidacionRol.CantidadContratos, liquidacionRol.TotalComisiones);
                }

                _logger.LogInformation("[LiquidacionService] ===== LIQUIDACIONES ROLES ADICIONALES GENERADAS =====");
                return comisionesPorUsuario.Count > 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[LiquidacionService] ERROR generando liquidaciones roles adicionales");
                return false;
            }
        }

        /// <summary>
        /// Recalcula los totales de una liquidación basándose en los detalles de comisión
        /// </summary>
        private async Task RecalcularTotalesLiquidacionAsync(int liquidacionId)
        {
            await using var context = _dbContextProvider.CreateDbContext();

            var liquidacion = await context.HistoricoLiquidaciones
                .FirstOrDefaultAsync(l => l.Id == liquidacionId);

            if (liquidacion == null)
            {
                Console.WriteLine($"[LiquidacionService] RecalcularTotales: Liquidación {liquidacionId} no encontrada");
                return;
            }

            // Obtener detalles de comisión de esta liquidación
            var detalles = await context.DetallesComisionLiquidacion
                .Where(d => d.HistoricoLiquidacionId == liquidacionId)
                .ToListAsync();

            if (detalles.Count == 0)
            {
                Console.WriteLine($"[LiquidacionService] RecalcularTotales: No hay detalles para liquidación {liquidacionId}");
                return;
            }

            // Calcular totales sumando las comisiones del usuario
            var usuarioId = liquidacion.UsuarioId;
            decimal totalComisiones = 0;
            int contratosEnergia = 0;
            int contratosTelefonia = 0;
            int contratosAlarmas = 0;

            foreach (var detalle in detalles)
            {
                // Determinar qué comisión corresponde al usuario de esta liquidación
                decimal comisionUsuario = 0;

                if (detalle.ColaboradorId == usuarioId)
                    comisionUsuario = detalle.ComisionColaborador;
                else if (detalle.GestorId == usuarioId && detalle.ComisionGestor.HasValue)
                    comisionUsuario = detalle.ComisionGestor.Value;
                else if (detalle.JefeVentasId == usuarioId && detalle.ComisionJefeVentas.HasValue)
                    comisionUsuario = detalle.ComisionJefeVentas.Value;
                else if (detalle.DirectorComercialId == usuarioId && detalle.ComisionDirectorComercial.HasValue)
                    comisionUsuario = detalle.ComisionDirectorComercial.Value;
                else if (detalle.AdministradorId == usuarioId)
                    comisionUsuario = detalle.ComisionAdministrador;

                if (comisionUsuario > 0)
                {
                    totalComisiones += comisionUsuario;

                    // Contar por tipo
                    switch (detalle.TipoContrato?.ToLower())
                    {
                        case "energia":
                            contratosEnergia++;
                            break;
                        case "telefonia":
                        case "telefonía":
                            contratosTelefonia++;
                            break;
                        case "alarma":
                            contratosAlarmas++;
                            break;
                    }
                }
            }

            liquidacion.CantidadContratos = contratosEnergia + contratosTelefonia + contratosAlarmas;
            liquidacion.TotalComisiones = totalComisiones;
            liquidacion.ContratosEnergia = contratosEnergia;
            liquidacion.ContratosTelefonia = contratosTelefonia;
            liquidacion.ContratosAlarmas = contratosAlarmas;

            await context.SaveChangesAsync();

            Console.WriteLine($"[LiquidacionService] Liquidación {liquidacionId} recalculada: {liquidacion.CantidadContratos} contratos, {liquidacion.TotalComisiones:N2}€");
        }
    }
}
