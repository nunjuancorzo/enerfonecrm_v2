using EnerfoneCRM.Data;
using EnerfoneCRM.Models;
using Microsoft.EntityFrameworkCore;

namespace EnerfoneCRM.Services
{
    /// <summary>
    /// Servicio para gestionar el cálculo y distribución de comisiones jerárquicas
    /// </summary>
    public class ComisionService
    {
        private readonly DbContextProvider _dbContextProvider;

        public ComisionService(DbContextProvider dbContextProvider)
        {
            _dbContextProvider = dbContextProvider;
        }

        /// <summary>
        /// Obtiene la configuración de comisión para un usuario y proveedor específico
        /// Primero busca configuración específica del proveedor, luego genérica (ProveedorId = 0 o NULL)
        /// </summary>
        public async Task<ConfiguracionComision?> ObtenerConfiguracionAsync(
            int usuarioId, 
            string tipoProveedor, 
            int proveedorId)
        {
            await using var context = _dbContextProvider.CreateDbContext();

            // Primero buscar configuración específica para este proveedor
            var configuracionEspecifica = await context.ConfiguracionesComision
                .Where(c => c.UsuarioId == usuarioId 
                    && c.TipoProveedor == tipoProveedor 
                    && c.ProveedorId == proveedorId 
                    && c.Activa)
                .FirstOrDefaultAsync();
            
            if (configuracionEspecifica != null)
            {
                Console.WriteLine($"[ComisionService] Configuración específica encontrada para usuario {usuarioId}, proveedor {proveedorId}: {configuracionEspecifica.PorcentajeColaborador}%");
                return configuracionEspecifica;
            }

            // Si no hay configuración específica, buscar configuración genérica (ProveedorId = 0 o NULL)
            var configuracionGenerica = await context.ConfiguracionesComision
                .Where(c => c.UsuarioId == usuarioId 
                    && c.TipoProveedor == tipoProveedor 
                    && (c.ProveedorId == 0 || c.ProveedorId == null)
                    && c.Activa)
                .FirstOrDefaultAsync();
            
            if (configuracionGenerica != null)
            {
                Console.WriteLine($"[ComisionService] Configuración genérica encontrada para usuario {usuarioId}: {configuracionGenerica.PorcentajeColaborador}%");
            }
            else
            {
                Console.WriteLine($"[ComisionService] No se encontró configuración para usuario {usuarioId}, tipo {tipoProveedor}. Usando valores por defecto.");
            }
            
            return configuracionGenerica;
        }

        /// <summary>
        /// Obtiene todas las configuraciones de comisión para un usuario
        /// </summary>
        public async Task<List<ConfiguracionComision>> ObtenerConfiguracionesPorUsuarioAsync(int usuarioId)
        {
            await using var context = _dbContextProvider.CreateDbContext();

            return await context.ConfiguracionesComision
                .Where(c => c.UsuarioId == usuarioId && c.Activa)
                .OrderBy(c => c.TipoProveedor)
                .ThenBy(c => c.NombreProveedor)
                .ToListAsync();
        }

        /// <summary>
        /// Crea o actualiza una configuración de comisión
        /// </summary>
        public async Task<ConfiguracionComision> GuardarConfiguracionAsync(ConfiguracionComision configuracion)
        {
            await using var context = _dbContextProvider.CreateDbContext();

            // Validar que los porcentajes sumen máximo 100%
            decimal total = configuracion.PorcentajeColaborador
                + (configuracion.PorcentajeGestor ?? 0)
                + (configuracion.PorcentajeJefeVentas ?? 0)
                + (configuracion.PorcentajeDirectorComercial ?? 0);

            if (total > 100)
            {
                throw new InvalidOperationException($"La suma de porcentajes no puede superar el 100% (actual: {total}%)");
            }

            if (configuracion.Id == 0)
            {
                configuracion.FechaCreacion = DateTime.Now;
                context.ConfiguracionesComision.Add(configuracion);
            }
            else
            {
                context.ConfiguracionesComision.Update(configuracion);
            }

            await context.SaveChangesAsync();

            // Recalcular comisiones de contratos activos/facturables del usuario
            await RecalcularComisionesContratosActivosAsync(configuracion.UsuarioId, configuracion);

            return configuracion;
        }

        /// <summary>
        /// Recalcula las comisiones de todos los contratos Act/Facturable de un usuario
        /// cuando se modifica su configuración de comisiones
        /// </summary>
        public async Task RecalcularComisionesContratosActivosAsync(int usuarioId, ConfiguracionComision configuracion)
        {
            await using var context = _dbContextProvider.CreateDbContext();

            Console.WriteLine($"[ComisionService] ===== RECALCULANDO COMISIONES CONTRATOS ACTIVOS =====");
            Console.WriteLine($"[ComisionService] Usuario ID: {usuarioId}");
            Console.WriteLine($"[ComisionService] Nueva configuración - Colaborador: {configuracion.PorcentajeColaborador}%");

            // Estados que deben recalcularse (todos los anteriores y Act/Facturable)
            var estadosRecalculables = new List<string>
            {
                "Pte Carga",
                "Solicitado",
                "Pte Firma",
                "En incidencia",
                "Pte Documentación",
                "Pte Validación",
                "En Curso",
                "En Activación",
                "En tramitación",
                "Activo",
                "Act/Facturable"
            };

            // Buscar todos los contratos del usuario en estados recalculables
            var contratos = await context.Contratos
                .Where(c => c.Comercial == context.Usuarios
                    .Where(u => u.Id == usuarioId)
                    .Select(u => u.NombreUsuario)
                    .FirstOrDefault()
                    && estadosRecalculables.Contains(c.Estado))
                .ToListAsync();

            Console.WriteLine($"[ComisionService] Contratos encontrados: {contratos.Count}");

            // Obtener el usuario para determinar su rol
            var usuario = await context.Usuarios.FindAsync(usuarioId);
            if (usuario == null)
            {
                Console.WriteLine($"[ComisionService] Usuario {usuarioId} no encontrado");
                return;
            }

            foreach (var contrato in contratos)
            {
                // Determinar el tipo de proveedor y obtener el ID del proveedor
                var tipoProveedor = ObtenerTipoProveedorInterno(contrato.Tipo);
                int? proveedorId = await ObtenerProveedorIdInternoAsync(contrato, context);

                // Verificar si esta configuración aplica a este contrato
                bool configuracionAplica = false;

                if (configuracion.ProveedorId == null || configuracion.ProveedorId == 0)
                {
                    // Configuración genérica - aplica si el tipo coincide
                    configuracionAplica = configuracion.TipoProveedor.Equals(tipoProveedor, StringComparison.OrdinalIgnoreCase);
                }
                else
                {
                    // Configuración específica - debe coincidir tipo y proveedor
                    configuracionAplica = configuracion.TipoProveedor.Equals(tipoProveedor, StringComparison.OrdinalIgnoreCase)
                        && configuracion.ProveedorId == proveedorId;
                }

                if (!configuracionAplica)
                {
                    Console.WriteLine($"[ComisionService] Contrato {contrato.Id} - Configuración no aplica (tipo: {tipoProveedor}, proveedor: {proveedorId})");
                    continue;
                }

                // Obtener la comisión base de la tarifa
                decimal comisionBase = await ObtenerComisionBaseTarifaAsync(contrato, context);

                if (comisionBase == 0)
                {
                    Console.WriteLine($"[ComisionService] Contrato {contrato.Id} - Sin comisión base, saltando");
                    continue;
                }

                // Calcular porcentaje según el rol del usuario
                decimal porcentajeTotal = 0;
                var rolUsuario = usuario.Rol;

                if (rolUsuario == "Administrador")
                {
                    // Administrador recibe 100%
                    porcentajeTotal = 100m;
                }
                else if (rolUsuario == "Director Comercial")
                {
                    // Director recibe: colaborador + gestor + jefe + director
                    porcentajeTotal = configuracion.PorcentajeColaborador +
                                    (configuracion.PorcentajeGestor ?? 0) +
                                    (configuracion.PorcentajeJefeVentas ?? 0) +
                                    (configuracion.PorcentajeDirectorComercial ?? 0);
                }
                else if (rolUsuario == "Jefe Comercial")
                {
                    // Jefe recibe: colaborador + gestor + jefe
                    porcentajeTotal = configuracion.PorcentajeColaborador +
                                    (configuracion.PorcentajeGestor ?? 0) +
                                    (configuracion.PorcentajeJefeVentas ?? 0);
                }
                else if (rolUsuario == "Gestor")
                {
                    // Gestor recibe: colaborador + gestor
                    porcentajeTotal = configuracion.PorcentajeColaborador +
                                    (configuracion.PorcentajeGestor ?? 0);
                }
                else
                {
                    // Colaborador recibe solo su porcentaje
                    porcentajeTotal = configuracion.PorcentajeColaborador;
                }

                // Calcular la nueva comisión aplicando el porcentaje total
                decimal nuevaComision = Math.Round(comisionBase * (porcentajeTotal / 100), 2);

                Console.WriteLine($"[ComisionService] Contrato {contrato.Id} - Comisión: {contrato.Comision} → {nuevaComision} (base: {comisionBase}, rol: {rolUsuario}, %total: {porcentajeTotal})");

                // Actualizar la comisión del contrato
                contrato.Comision = nuevaComision;
                contrato.FechaModificacion = DateTime.Now;
                context.Contratos.Update(contrato);
            }

            await context.SaveChangesAsync();

            // Recalcular liquidaciones afectadas
            if (contratos.Any())
            {
                await RecalcularLiquidacionesUsuarioAsync(usuarioId, context);
            }

            Console.WriteLine($"[ComisionService] Recálculo completado - {contratos.Count} contratos actualizados");
        }

        /// <summary>
        /// Recalcula las liquidaciones de un usuario que incluyan contratos modificados
        /// </summary>
        private async Task RecalcularLiquidacionesUsuarioAsync(int usuarioId, ApplicationDbContext context)
        {
            // Buscar liquidaciones pendientes del usuario
            var liquidacionesPendientes = await context.HistoricoLiquidaciones
                .Where(l => l.UsuarioId == usuarioId && l.Estado != "Pagada")
                .ToListAsync();

            foreach (var liquidacion in liquidacionesPendientes)
            {
                // Obtener todos los detalles de comisión de esta liquidación
                var detalles = await context.DetallesComisionLiquidacion
                    .Where(d => d.HistoricoLiquidacionId == liquidacion.Id)
                    .ToListAsync();

                // Recalcular totales
                decimal nuevoTotalComisiones = 0;

                foreach (var detalle in detalles)
                {
                    // Obtener el contrato actualizado
                    var contrato = await context.Contratos.FindAsync(detalle.ContratoId);
                    if (contrato != null && detalle.ColaboradorId == usuarioId)
                    {
                        // Actualizar la comisión del colaborador en el detalle
                        detalle.ComisionColaborador = contrato.Comision ?? 0m;
                        context.DetallesComisionLiquidacion.Update(detalle);
                    }

                    // Sumar comisiones según el rol
                    if (detalle.ColaboradorId == usuarioId)
                        nuevoTotalComisiones += detalle.ComisionColaborador;
                    else if (detalle.GestorId == usuarioId)
                        nuevoTotalComisiones += detalle.ComisionGestor ?? 0m;
                    else if (detalle.JefeVentasId == usuarioId)
                        nuevoTotalComisiones += detalle.ComisionJefeVentas ?? 0m;
                    else if (detalle.DirectorComercialId == usuarioId)
                        nuevoTotalComisiones += detalle.ComisionDirectorComercial ?? 0m;
                    else if (detalle.AdministradorId == usuarioId)
                        nuevoTotalComisiones += detalle.ComisionAdministrador;
                }

                // Actualizar el total de la liquidación
                liquidacion.TotalComisiones = nuevoTotalComisiones;
                context.HistoricoLiquidaciones.Update(liquidacion);

                Console.WriteLine($"[ComisionService] Liquidación {liquidacion.Id} actualizada - Nuevo total: {nuevoTotalComisiones}€");
            }

            await context.SaveChangesAsync();
        }

        /// <summary>
        /// Obtiene el tipo de proveedor interno (sin el await async)
        /// </summary>
        private string ObtenerTipoProveedorInterno(string? tipoContrato)
        {
            return tipoContrato?.ToLower() switch
            {
                "energia" => "Comercializadora",
                "telefonia" => "Operadora",
                "alarma" => "EmpresaAlarma",
                _ => "Comercializadora"
            };
        }

        /// <summary>
        /// Obtiene el ID del proveedor de forma asíncrona buscando en las tablas correspondientes
        /// </summary>
        private async Task<int?> ObtenerProveedorIdInternoAsync(Contrato contrato, ApplicationDbContext context)
        {
            try
            {
                switch (contrato.Tipo?.ToLower())
                {
                    case "energia":
                        if (!string.IsNullOrWhiteSpace(contrato.EnComercializadora))
                        {
                            var comercializadora = await context.Comercializadoras
                                .FirstOrDefaultAsync(c => c.Nombre == contrato.EnComercializadora);
                            return comercializadora?.Id;
                        }
                        break;

                    case "telefonia":
                        if (!string.IsNullOrWhiteSpace(contrato.OperadoraTel))
                        {
                            var operadora = await context.Operadoras
                                .FirstOrDefaultAsync(o => o.Nombre == contrato.OperadoraTel);
                            return operadora?.Id;
                        }
                        break;

                    case "alarma":
                        if (!string.IsNullOrWhiteSpace(contrato.EmpresaAlarma))
                        {
                            var empresa = await context.EmpresasAlarmas
                                .FirstOrDefaultAsync(e => e.Nombre == contrato.EmpresaAlarma);
                            return empresa?.Id;
                        }
                        break;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ComisionService] Error obteniendo proveedor ID: {ex.Message}");
            }

            return null;
        }

        /// <summary>
        /// Calcula la distribución de comisiones para un contrato según la jerarquía del usuario
        /// </summary>
        public async Task<DetalleComisionLiquidacion> CalcularDistribucionComisionAsync(
            Contrato contrato, 
            Usuario colaborador,
            int? liquidacionId = null)
        {
            await using var context = _dbContextProvider.CreateDbContext();

            // Obtener la configuración de comisión para este usuario y proveedor
            var tipoProveedor = ObtenerTipoProveedor(contrato.Tipo);
            var proveedorId = ObtenerProveedorId(contrato);
            var nombreProveedor = ObtenerNombreProveedor(contrato);

            Console.WriteLine($"[ComisionService] ===== CALCULANDO DISTRIBUCIÓN =====");
            Console.WriteLine($"[ComisionService] Contrato ID: {contrato.Id}, Tipo: {contrato.Tipo}");
            Console.WriteLine($"[ComisionService] Colaborador: {colaborador.NombreUsuario} (ID: {colaborador.Id})");
            Console.WriteLine($"[ComisionService] TipoProveedor: {tipoProveedor}, ProveedorId: {proveedorId}");

            var configuracion = await ObtenerConfiguracionAsync(colaborador.Id, tipoProveedor, proveedorId);

            // Si no hay configuración específica, usar porcentajes por defecto
            var pctColaborador = configuracion?.PorcentajeColaborador ?? 70m;
            var pctGestor = configuracion?.PorcentajeGestor ?? 10m;
            var pctJefeVentas = configuracion?.PorcentajeJefeVentas ?? 10m;
            
            Console.WriteLine($"[ComisionService] Configuración encontrada: {(configuracion != null ? "SÍ" : "NO (usando defaults)")}");
            if (configuracion != null)
            {
                Console.WriteLine($"[ComisionService] Config ID: {configuracion.Id}, Usuario: {configuracion.UsuarioId}, Proveedor: {configuracion.ProveedorId}");
                Console.WriteLine($"[ComisionService] Porcentajes: Colab={configuracion.PorcentajeColaborador}%, Gestor={configuracion.PorcentajeGestor}%, Jefe={configuracion.PorcentajeJefeVentas}%, Dir={configuracion.PorcentajeDirectorComercial}%");
            }
            Console.WriteLine($"[ComisionService] Porcentajes a usar: Colab={pctColaborador}%, Gestor={pctGestor}%, Jefe={pctJefeVentas}%");
            var pctDirectorComercial = configuracion?.PorcentajeDirectorComercial ?? 0m;

            // Si el colaborador es Backoffice, no recibe comisión (todo va al Administrador)
            if (colaborador.Rol == "Backoffice")
            {
                pctColaborador = 0;
                pctGestor = 0;
                pctJefeVentas = 0;
                pctDirectorComercial = 0;
            }

            // Calcular el porcentaje del administrador (el restante)
            var pctAdministrador = 100m - pctColaborador - pctGestor - pctJefeVentas - pctDirectorComercial;

            // Obtener la comisión base de la tarifa (antes de aplicar porcentaje de usuario)
            decimal comisionBaseTarifa = await ObtenerComisionBaseTarifaAsync(contrato, context);
            
            // La comisión del contrato ya tiene aplicado el porcentaje del colaborador
            var comisionColaborador = contrato.Comision ?? 0;

            // Obtener el administrador de la empresa
            var administrador = await context.Usuarios
                .Where(u => u.Rol == "Administrador" && u.Activo)
                .FirstOrDefaultAsync() 
                ?? throw new InvalidOperationException("No se encontró un administrador activo");

            // Obtener los usuarios de la jerarquía
            var gestor = colaborador.GestorId.HasValue 
                ? await context.Usuarios.FindAsync(colaborador.GestorId.Value) 
                : null;

            var jefeVentas = colaborador.JefeVentasId.HasValue 
                ? await context.Usuarios.FindAsync(colaborador.JefeVentasId.Value) 
                : null;

            var directorComercial = colaborador.DirectorComercialId.HasValue 
                ? await context.Usuarios.FindAsync(colaborador.DirectorComercialId.Value) 
                : null;

            // Crear el detalle de comisión
            var detalle = new DetalleComisionLiquidacion
            {
                HistoricoLiquidacionId = liquidacionId ?? 0,
                ContratoId = contrato.Id,
                TipoContrato = contrato.Tipo,
                ComisionBase = comisionBaseTarifa,

                // Colaborador: usar la comisión del contrato directamente (ya tiene su % aplicado)
                ColaboradorId = colaborador.Id,
                ComisionColaborador = comisionColaborador,
                PorcentajeColaborador = pctColaborador,

                // Gestor: calcular sobre la comisión base de la tarifa
                GestorId = gestor?.Id,
                ComisionGestor = gestor != null ? Math.Round(comisionBaseTarifa * pctGestor / 100, 2) : null,
                PorcentajeGestor = gestor != null ? pctGestor : null,

                // Jefe de Ventas: calcular sobre la comisión base de la tarifa
                JefeVentasId = jefeVentas?.Id,
                ComisionJefeVentas = jefeVentas != null ? Math.Round(comisionBaseTarifa * pctJefeVentas / 100, 2) : null,
                PorcentajeJefeVentas = jefeVentas != null ? pctJefeVentas : null,

                // Director Comercial: calcular sobre la comisión base de la tarifa
                DirectorComercialId = directorComercial?.Id,
                ComisionDirectorComercial = directorComercial != null && pctDirectorComercial > 0 
                    ? Math.Round(comisionBaseTarifa * pctDirectorComercial / 100, 2) 
                    : null,
                PorcentajeDirectorComercial = directorComercial != null && pctDirectorComercial > 0 
                    ? pctDirectorComercial 
                    : null,

                // Administrador: calcular sobre la comisión base de la tarifa
                AdministradorId = administrador.Id,
                ComisionAdministrador = Math.Round(comisionBaseTarifa * pctAdministrador / 100, 2),
                PorcentajeAdministrador = pctAdministrador,

                // Información del proveedor
                NombreProveedor = nombreProveedor,
                TipoProveedor = tipoProveedor,

                FechaCreacion = DateTime.Now
            };

            return detalle;
        }

        /// <summary>
        /// Obtiene la comisión base de la tarifa (sin porcentaje de usuario aplicado)
        /// </summary>
        private async Task<decimal> ObtenerComisionBaseTarifaAsync(Contrato contrato, ApplicationDbContext context)
        {
            try
            {
                switch (contrato.Tipo?.ToLower())
                {
                    case "energia":
                        if (contrato.EnTarifaId.HasValue)
                        {
                            var tarifa = await context.TarifasEnergia.FindAsync(contrato.EnTarifaId.Value);
                            return tarifa?.Comision ?? 0;
                        }
                        // Si no hay ID, buscar por nombre
                        if (!string.IsNullOrEmpty(contrato.EnTarifa))
                        {
                            var tarifa = await context.TarifasEnergia
                                .FirstOrDefaultAsync(t => t.Nombre == contrato.EnTarifa);
                            return tarifa?.Comision ?? 0;
                        }
                        break;

                    case "telefonia":
                    case "telefonía":
                        if (contrato.TarifaTelId.HasValue)
                        {
                            var tarifa = await context.TarifasTelefonia.FindAsync(contrato.TarifaTelId.Value);
                            return tarifa?.ComisionNew ?? 0;
                        }
                        // Si no hay ID, buscar por nombre
                        if (!string.IsNullOrEmpty(contrato.TarifaTel))
                        {
                            var tarifa = await context.TarifasTelefonia
                                .FirstOrDefaultAsync(t => t.Tarifa == contrato.TarifaTel);
                            return tarifa?.ComisionNew ?? 0;
                        }
                        break;

                    case "alarma":
                    case "alarmas":
                        if (contrato.KitAlarmaId.HasValue)
                        {
                            var tarifa = await context.TarifasAlarmas.FindAsync(contrato.KitAlarmaId.Value);
                            return tarifa?.Comision ?? 0;
                        }
                        // Si no hay ID, buscar por nombre
                        if (!string.IsNullOrEmpty(contrato.KitAlarma))
                        {
                            var tarifa = await context.TarifasAlarmas
                                .FirstOrDefaultAsync(t => t.NombreTarifa == contrato.KitAlarma);
                            return tarifa?.Comision ?? 0;
                        }
                        break;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ComisionService] Error al obtener comisión base de tarifa: {ex.Message}");
            }

            // Si no se encuentra, usar la comisión del contrato como fallback
            return contrato.Comision ?? 0;
        }

        /// <summary>
        /// Guarda un detalle de comisión en la base de datos
        /// </summary>
        public async Task<DetalleComisionLiquidacion> GuardarDetalleComisionAsync(DetalleComisionLiquidacion detalle)
        {
            await using var context = _dbContextProvider.CreateDbContext();

            context.DetallesComisionLiquidacion.Add(detalle);
            await context.SaveChangesAsync();

            return detalle;
        }

        /// <summary>
        /// Obtiene los detalles de comisión de una liquidación
        /// </summary>
        public async Task<List<DetalleComisionLiquidacion>> ObtenerDetallesPorLiquidacionAsync(int liquidacionId)
        {
            await using var context = _dbContextProvider.CreateDbContext();

            return await context.DetallesComisionLiquidacion
                .Where(d => d.HistoricoLiquidacionId == liquidacionId)
                .Include(d => d.Contrato)
                .OrderBy(d => d.FechaCreacion)
                .ToListAsync();
        }

        /// <summary>
        /// Calcula el total de comisiones por usuario en una liquidación
        /// </summary>
        public async Task<Dictionary<int, decimal>> CalcularTotalesPorUsuarioAsync(int liquidacionId)
        {
            await using var context = _dbContextProvider.CreateDbContext();

            var detalles = await context.DetallesComisionLiquidacion
                .Where(d => d.HistoricoLiquidacionId == liquidacionId)
                .ToListAsync();

            var totales = new Dictionary<int, decimal>();

            foreach (var detalle in detalles)
            {
                // Colaborador
                AgregarOSumar(totales, detalle.ColaboradorId, detalle.ComisionColaborador);

                // Gestor
                if (detalle.GestorId.HasValue && detalle.ComisionGestor.HasValue)
                    AgregarOSumar(totales, detalle.GestorId.Value, detalle.ComisionGestor.Value);

                // Jefe de Ventas
                if (detalle.JefeVentasId.HasValue && detalle.ComisionJefeVentas.HasValue)
                    AgregarOSumar(totales, detalle.JefeVentasId.Value, detalle.ComisionJefeVentas.Value);

                // Director Comercial
                if (detalle.DirectorComercialId.HasValue && detalle.ComisionDirectorComercial.HasValue)
                    AgregarOSumar(totales, detalle.DirectorComercialId.Value, detalle.ComisionDirectorComercial.Value);

                // Administrador
                AgregarOSumar(totales, detalle.AdministradorId, detalle.ComisionAdministrador);
            }

            return totales;
        }

        /// <summary>
        /// Verifica si un contrato requiere decomisión por baja anticipada
        /// </summary>
        public async Task<(bool requiere, int? diasPenalizacion, string? tipoPenalizacion)> 
            VerificarPenalizacionAsync(Contrato contrato, DateTime? fechaBaja = null)
        {
            await using var context = _dbContextProvider.CreateDbContext();

            fechaBaja ??= DateTime.Now;

            Console.WriteLine($"[DEBUG ComisionService] Verificando penalización para contrato {contrato.Id}, Tipo: {contrato.Tipo}");

            // Obtener la tarifa del contrato para verificar si tiene penalización
            int? diasPenalizacion = null;
            string? tipoPenalizacion = null;

            // Normalizar el tipo eliminando acentos para evitar problemas de comparación
            var tipoNormalizado = contrato.Tipo?.ToLower()
                .Replace("í", "i")
                .Replace("ó", "o")
                .Replace("á", "a")
                .Replace("é", "e")
                .Replace("ú", "u");

            switch (tipoNormalizado)
            {
                case "energia":
                    TarifaEnergia? tarifaEnergia = null;
                    
                    // Buscar primero por ID (más seguro y rápido)
                    if (contrato.EnTarifaId.HasValue)
                    {
                        tarifaEnergia = await context.TarifasEnergia
                            .FirstOrDefaultAsync(t => t.Id == contrato.EnTarifaId.Value);
                    }
                    
                    // Fallback: buscar por nombre si no hay ID o no se encontró
                    if (tarifaEnergia == null && !string.IsNullOrWhiteSpace(contrato.EnTarifa))
                    {
                        tarifaEnergia = await context.TarifasEnergia
                            .FirstOrDefaultAsync(t => t.Nombre == contrato.EnTarifa);
                    }
                    
                    diasPenalizacion = tarifaEnergia?.DiasPenalizacion;
                    tipoPenalizacion = tarifaEnergia?.TipoPenalizacion;
                    Console.WriteLine($"[DEBUG] Tarifa Energía encontrada: ID={tarifaEnergia?.Id}, Nombre={tarifaEnergia?.Nombre}, DiasPen={diasPenalizacion}, TipoPen={tipoPenalizacion}");
                    break;

                case "telefonia":
                    TarifaTelefonia? tarifaTelefonia = null;
                    
                    // Buscar primero por ID (más seguro y rápido)
                    if (contrato.TarifaTelId.HasValue)
                    {
                        tarifaTelefonia = await context.TarifasTelefonia
                            .FirstOrDefaultAsync(t => t.Id == contrato.TarifaTelId.Value);
                    }
                    
                    // Fallback: buscar por nombre si no hay ID o no se encontró
                    if (tarifaTelefonia == null && !string.IsNullOrWhiteSpace(contrato.TarifaTel))
                    {
                        tarifaTelefonia = await context.TarifasTelefonia
                            .FirstOrDefaultAsync(t => t.Tarifa == contrato.TarifaTel);
                    }
                    
                    diasPenalizacion = tarifaTelefonia?.DiasPenalizacion;
                    tipoPenalizacion = tarifaTelefonia?.TipoPenalizacion;
                    break;

                case "alarma":
                    TarifaAlarma? tarifaAlarma = null;
                    
                    // Buscar primero por ID (más seguro y rápido)
                    if (contrato.KitAlarmaId.HasValue)
                    {
                        tarifaAlarma = await context.TarifasAlarmas
                            .FirstOrDefaultAsync(t => t.Id == contrato.KitAlarmaId.Value);
                    }
                    
                    // Fallback: buscar por nombre si no hay ID o no se encontró
                    if (tarifaAlarma == null && !string.IsNullOrWhiteSpace(contrato.KitAlarma))
                    {
                        tarifaAlarma = await context.TarifasAlarmas
                            .FirstOrDefaultAsync(t => t.NombreTarifa == contrato.KitAlarma);
                    }
                    
                    diasPenalizacion = tarifaAlarma?.DiasPenalizacion;
                    tipoPenalizacion = tarifaAlarma?.TipoPenalizacion;
                    break;
            }

            if (!diasPenalizacion.HasValue || diasPenalizacion <= 0)
            {
                Console.WriteLine($"[DEBUG] Sin penalización configurada: diasPenalizacion={diasPenalizacion}");
                return (false, null, null);
            }

            // Verificar si el contrato ha estado activo el tiempo suficiente
            // Usar FechaActivo (cuando pasó a estado Activo) en lugar de FechaAlta
            if (!contrato.FechaActivo.HasValue)
            {
                Console.WriteLine($"[DEBUG] FechaActivo es NULL, requiere penalización");
                return (true, diasPenalizacion, tipoPenalizacion);
            }

            var diasActivo = (fechaBaja.Value - contrato.FechaActivo.Value).Days;
            bool requiere = diasActivo < diasPenalizacion.Value;

            Console.WriteLine($"[DEBUG] FechaActivo={(contrato.FechaActivo?.ToString("yyyy-MM-dd"))}, diasActivo={diasActivo}, diasRequeridos={diasPenalizacion}, requierePen={requiere}");

            return (requiere, diasPenalizacion, tipoPenalizacion);
        }

        /// <summary>
        /// Crea una decomisión por baja anticipada
        /// </summary>
        public async Task<Decomision?> CrearDecomisionAsync(
            Contrato contrato,
            Usuario usuario,
            DateTime fechaBaja,
            int? liquidacionOriginalId = null,
            int? creadoPorUsuarioId = null,
            string? observaciones = null)
        {
            var (requierePenalizacion, diasPenalizacion, tipoPenalizacion) = 
                await VerificarPenalizacionAsync(contrato, fechaBaja);

            if (!requierePenalizacion || !diasPenalizacion.HasValue)
                return null;

            await using var context = _dbContextProvider.CreateDbContext();

            var comisionOriginal = contrato.Comision ?? 0;
            // Usar FechaActivo (cuando pasó a estado Activo) para calcular penalización
            var fechaActivacion = contrato.FechaActivo ?? DateTime.Now.AddDays(-diasPenalizacion.Value - 1);
            var diasActivo = (fechaBaja - fechaActivacion).Days;
            var diasPendientes = Math.Max(0, diasPenalizacion.Value - diasActivo);

            decimal importeDecomision;

            if (tipoPenalizacion == "Proporcional")
            {
                // Cálculo proporcional: (comisión * días pendientes) / días penalización
                importeDecomision = Math.Round((comisionOriginal * diasPendientes) / diasPenalizacion.Value, 2);
            }
            else
            {
                // Decomisión total
                importeDecomision = comisionOriginal;
            }

            var decomision = new Decomision
            {
                ContratoId = contrato.Id,
                UsuarioId = usuario.Id,
                NombreUsuario = usuario.NombreUsuario,
                LiquidacionOriginalId = liquidacionOriginalId,
                TipoDecomision = tipoPenalizacion ?? "Total",
                ComisionOriginal = comisionOriginal,
                ImporteDecomision = importeDecomision,
                Diaspenalizacion = diasPenalizacion.Value,
                DiasActivo = diasActivo,
                DiasPendientes = diasPendientes,
                FechaAlta = contrato.FechaAlta,
                FechaBaja = fechaBaja,
                FechaCreacion = DateTime.Now,
                CreadoPorUsuarioId = creadoPorUsuarioId,
                Observaciones = observaciones,
                Estado = "Pendiente",
                TipoContrato = contrato.Tipo,
                NombreProveedor = ObtenerNombreProveedor(contrato)
            };

            context.Decomisiones.Add(decomision);
            await context.SaveChangesAsync();

            return decomision;
        }

        /// <summary>
        /// Obtiene las decomisiones pendientes de un usuario
        /// </summary>
        public async Task<List<Decomision>> ObtenerDecomisionesPendientesAsync(int usuarioId)
        {
            await using var context = _dbContextProvider.CreateDbContext();

            return await context.Decomisiones
                .Where(d => d.UsuarioId == usuarioId && d.Estado == "Pendiente")
                .OrderByDescending(d => d.FechaCreacion)
                .ToListAsync();
        }

        // Métodos auxiliares privados

        private void AgregarOSumar(Dictionary<int, decimal> diccionario, int key, decimal valor)
        {
            if (diccionario.ContainsKey(key))
                diccionario[key] += valor;
            else
                diccionario[key] = valor;
        }

        private string ObtenerTipoProveedor(string? tipoContrato)
        {
            return tipoContrato?.ToLower() switch
            {
                "energia" => "Comercializadora",
                "telefonia" => "Operadora",
                "alarma" => "EmpresaAlarma",
                _ => "Comercializadora"
            };
        }

        private int ObtenerProveedorId(Contrato contrato)
        {
            // Por ahora retornamos 1 por defecto
            // En una implementación real, deberías buscar el ID real del proveedor
            // basándote en el nombre de la comercializadora/operadora/empresa
            return contrato.UsuarioComercializadoraId ?? 1;
        }

        private string ObtenerNombreProveedor(Contrato contrato)
        {
            return contrato.Tipo?.ToLower() switch
            {
                "energia" => contrato.EnComercializadora ?? "Sin asignar",
                "telefonia" => contrato.OperadoraTel ?? "Sin asignar",
                "alarma" => contrato.EmpresaAlarma ?? "Sin asignar",
                _ => "Sin asignar"
            };
        }
    }
}
