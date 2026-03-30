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
        /// </summary>
        public async Task<ConfiguracionComision?> ObtenerConfiguracionAsync(
            int usuarioId, 
            string tipoProveedor, 
            int proveedorId)
        {
            await using var context = _dbContextProvider.CreateDbContext();

            return await context.ConfiguracionesComision
                .Where(c => c.UsuarioId == usuarioId 
                    && c.TipoProveedor == tipoProveedor 
                    && c.ProveedorId == proveedorId 
                    && c.Activa)
                .FirstOrDefaultAsync();
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
            return configuracion;
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

            var configuracion = await ObtenerConfiguracionAsync(colaborador.Id, tipoProveedor, proveedorId);

            // Si no hay configuración específica, usar porcentajes por defecto
            var pctColaborador = configuracion?.PorcentajeColaborador ?? 70m;
            var pctGestor = configuracion?.PorcentajeGestor ?? 10m;
            var pctJefeVentas = configuracion?.PorcentajeJefeVentas ?? 10m;
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

            // Obtener la comisión base del contrato
            var comisionBase = contrato.Comision ?? 0;

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
                ComisionBase = comisionBase,

                // Colaborador
                ColaboradorId = colaborador.Id,
                ComisionColaborador = Math.Round(comisionBase * pctColaborador / 100, 2),
                PorcentajeColaborador = pctColaborador,

                // Gestor
                GestorId = gestor?.Id,
                ComisionGestor = gestor != null ? Math.Round(comisionBase * pctGestor / 100, 2) : null,
                PorcentajeGestor = gestor != null ? pctGestor : null,

                // Jefe de Ventas
                JefeVentasId = jefeVentas?.Id,
                ComisionJefeVentas = jefeVentas != null ? Math.Round(comisionBase * pctJefeVentas / 100, 2) : null,
                PorcentajeJefeVentas = jefeVentas != null ? pctJefeVentas : null,

                // Director Comercial
                DirectorComercialId = directorComercial?.Id,
                ComisionDirectorComercial = directorComercial != null && pctDirectorComercial > 0 
                    ? Math.Round(comisionBase * pctDirectorComercial / 100, 2) 
                    : null,
                PorcentajeDirectorComercial = directorComercial != null && pctDirectorComercial > 0 
                    ? pctDirectorComercial 
                    : null,

                // Administrador
                AdministradorId = administrador.Id,
                ComisionAdministrador = Math.Round(comisionBase * pctAdministrador / 100, 2),
                PorcentajeAdministrador = pctAdministrador,

                // Información del proveedor
                NombreProveedor = nombreProveedor,
                TipoProveedor = tipoProveedor,

                FechaCreacion = DateTime.Now
            };

            return detalle;
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
