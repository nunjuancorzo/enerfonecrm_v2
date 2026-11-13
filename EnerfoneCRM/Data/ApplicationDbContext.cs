using Microsoft.EntityFrameworkCore;
using EnerfoneCRM.Models;
using System.Linq.Expressions;

namespace EnerfoneCRM.Data;

public class ApplicationDbContext : DbContext
{
    public int EmpresaId { get; }
    public bool EsSuperAdmin { get; }
    internal event Action? ContratosActualizados;
    internal bool PermitirAltaUsuarioInactiva { get; set; }
    internal bool PermitirEdicionDatosEmpresa { get; set; }
    internal bool PermitirEdicionPaletaEmpresa { get; set; }
    private readonly bool _autenticacion;
    private readonly bool _publico;

    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options, int empresaId, bool esSuperAdmin = false)
        : base(options)
    {
        EmpresaId = empresaId;
        EsSuperAdmin = esSuperAdmin;
    }

    internal ApplicationDbContext(DbContextOptions<ApplicationDbContext> options, bool autenticacion)
        : base(options)
    {
        _autenticacion = autenticacion;
    }

    internal ApplicationDbContext(DbContextOptions<ApplicationDbContext> options, bool autenticacion, bool publico)
        : base(options)
    {
        _autenticacion = autenticacion;
        _publico = publico;
    }

    public DbSet<Empresa> Empresas { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.Entity<Usuario>().HasIndex(usuario => usuario.NombreUsuario).IsUnique();
        modelBuilder.Entity<Empresa>().HasQueryFilter(empresa => _publico || _autenticacion || EsSuperAdmin || empresa.Id == EmpresaId);
        var referencias = new Dictionary<string, Type>
        {
            ["UsuarioId"] = typeof(Usuario), ["GestorId"] = typeof(Usuario), ["JefeVentasId"] = typeof(Usuario),
            ["DirectorComercialId"] = typeof(Usuario), ["UsuarioComercialId"] = typeof(Usuario),
            ["UsuarioComercializadoraId"] = typeof(Usuario), ["ColaboradorId"] = typeof(Usuario),
            ["AdministradorId"] = typeof(Usuario), ["AprobadoPorId"] = typeof(Usuario),
            ["UsuarioColaboradorId"] = typeof(Usuario), ["UsuarioAdministradorId"] = typeof(Usuario),
            ["CreadoPorUsuarioId"] = typeof(Usuario), ["UsuarioCreacionId"] = typeof(Usuario), ["UsuarioCreadorId"] = typeof(Usuario),
            ["IdCliente"] = typeof(Cliente), ["ClienteId"] = typeof(Cliente),
            ["IdContrato"] = typeof(Contrato), ["ContratoId"] = typeof(Contrato),
            ["ComercializadoraId"] = typeof(Comercializadora), ["OperadoraId"] = typeof(Operadora),
            ["EmpresaAlarmaId"] = typeof(EmpresaAlarma), ["ServicioId"] = typeof(Servicio), ["ServicioGasId"] = typeof(ServicioGas),
            ["HistoricoLiquidacionId"] = typeof(HistoricoLiquidacion), ["LiquidacionOriginalId"] = typeof(HistoricoLiquidacion),
            ["LiquidacionDecomisionId"] = typeof(HistoricoLiquidacion), ["SolicitudFirmaId"] = typeof(SolicitudFirma),
            ["DocumentoOriginalId"] = typeof(FicheroContrato), ["DocumentoFirmadoId"] = typeof(FicheroContrato),
            ["DocumentoId"] = typeof(FicheroContrato), ["IncidenciaId"] = typeof(Incidencia), ["MensajeId"] = typeof(MensajeBienvenida)
        };
        foreach (var entityType in modelBuilder.Model.GetEntityTypes().ToList())
        {
            foreach (var property in entityType.GetProperties().ToList())
            {
                if (!referencias.TryGetValue(property.Name, out var principal) ||
                    (property.ClrType != typeof(int) && property.ClrType != typeof(int?)) ||
                    entityType.GetForeignKeys().Any(key => key.Properties.Contains(property))) continue;
                modelBuilder.Entity(entityType.ClrType).HasOne(principal).WithMany().HasForeignKey(property.Name).OnDelete(DeleteBehavior.NoAction);
            }
            if (entityType.ClrType == typeof(OptimeFirma) || entityType.ClrType == typeof(OptimeDocumento))
                modelBuilder.Entity(entityType.ClrType).HasOne(typeof(OptimeSolicitud)).WithMany().HasForeignKey("SolicitudId").OnDelete(DeleteBehavior.NoAction);
        }
        foreach (var entityType in modelBuilder.Model.GetEntityTypes().Where(entity => entity.ClrType != typeof(Empresa)).ToList())
        {
            var entity = modelBuilder.Entity(entityType.ClrType);
            entity.Property<int>("EmpresaId").HasColumnName("id_empresa").IsConcurrencyToken();
            entity.HasIndex("EmpresaId");
            var parameter = Expression.Parameter(entityType.ClrType, "registro");
            var property = Expression.Call(typeof(EF), nameof(EF.Property), new[] { typeof(int) }, parameter, Expression.Constant("EmpresaId"));
            var context = Expression.Constant(this);
            var filter = Expression.OrElse(
                Expression.Field(context, nameof(_autenticacion)),
                Expression.AndAlso(Expression.GreaterThan(Expression.Property(context, nameof(EmpresaId)), Expression.Constant(0)),
                    Expression.Equal(property, Expression.Property(context, nameof(EmpresaId)))));
            entity.HasQueryFilter(Expression.Lambda(filter, parameter));
        }
    }

    private void ValidarEmpresa()
    {
        var cambios = ChangeTracker.Entries().Where(entry => entry.State is EntityState.Added or EntityState.Modified or EntityState.Deleted).ToList();
        foreach (var entry in cambios)
        {
            if (_autenticacion && !_publico)
                throw new UnauthorizedAccessException("El contexto de autenticacion es de solo lectura.");
            if (entry.Entity is Empresa)
            {
                if (!EsSuperAdmin) throw new UnauthorizedAccessException("Solo superadmin puede gestionar empresas.");
                continue;
            }
            var esRelacionDeUsuario = entry.Entity is UsuarioComercializadora or UsuarioOperadora or UsuarioEmpresaAlarma;
            var esAltaPendiente = !EsSuperAdmin && PermitirAltaUsuarioInactiva && entry.State == EntityState.Added &&
                (entry.Entity is Usuario { Activo: false } || esRelacionDeUsuario);
            var esEdicionDatosEmpresa = entry.Entity is ConfiguracionEmpresa && PermitirEdicionDatosEmpresa &&
                entry.State is EntityState.Added or EntityState.Modified;
            var esEdicionPaletaEmpresa = entry.Entity is ConfiguracionEmpresa && PermitirEdicionPaletaEmpresa &&
                entry.State == EntityState.Modified &&
                entry.Properties.Where(property => property.IsModified).All(property =>
                    property.Metadata.Name is nameof(ConfiguracionEmpresa.TemaVisual) or
                        nameof(ConfiguracionEmpresa.TemaColorAcento) or
                        nameof(ConfiguracionEmpresa.TemaColorBarra) or
                        nameof(ConfiguracionEmpresa.TemaColorFondo) or
                        nameof(ConfiguracionEmpresa.TemaVisualAnterior) or
                        nameof(ConfiguracionEmpresa.TemaAnteriorColorAcento) or
                        nameof(ConfiguracionEmpresa.TemaAnteriorColorBarra) or
                        nameof(ConfiguracionEmpresa.TemaAnteriorColorFondo) or
                        nameof(ConfiguracionEmpresa.TemaActualizadoPor) or
                        nameof(ConfiguracionEmpresa.TemaActualizadoEn) or
                        nameof(ConfiguracionEmpresa.FechaActualizacion));
            if ((entry.Metadata.ClrType.Name.StartsWith("Usuario", StringComparison.Ordinal) || entry.Entity is ConfiguracionEmpresa) && !EsSuperAdmin && !esAltaPendiente && !esEdicionDatosEmpresa && !esEdicionPaletaEmpresa)
                throw new UnauthorizedAccessException("Solo superadmin puede gestionar usuarios.");
            
            // En contexto público, permitir operaciones sin validación de EmpresaId
            if (!_publico && EmpresaId <= 0)
                throw new UnauthorizedAccessException("Debe seleccionar una empresa.");
            
            if (entry.Entity is Usuario usuario && entry.State != EntityState.Deleted &&
                usuario.NombreUsuario.Equals("superadmin", StringComparison.OrdinalIgnoreCase))
                throw new InvalidOperationException("El nombre superadmin esta reservado.");
            
            var empresa = entry.Property("EmpresaId");
            
            // En contexto público, no aplicar restricciones de EmpresaId
            if (_publico)
                continue;
                
            if (entry.State != EntityState.Added)
            {
                var stored = entry.GetDatabaseValues();
                if (stored == null || (int)stored["EmpresaId"]! != EmpresaId)
                    throw new UnauthorizedAccessException("El registro no pertenece a la empresa actual.");
                empresa.OriginalValue = EmpresaId;
            }
            if ((int)empresa.CurrentValue! != 0 && (int)empresa.CurrentValue! != EmpresaId)
                throw new UnauthorizedAccessException("No se puede cambiar la empresa de un registro.");
            empresa.CurrentValue = EmpresaId;
        }
        foreach (var entry in cambios.Where(entry => entry.Entity is not Empresa && entry.State != EntityState.Deleted))
        {
            foreach (var foreignKey in entry.Metadata.GetForeignKeys())
            {
                if (entry.State != EntityState.Added && !foreignKey.Properties.Any(property => entry.Property(property.Name).IsModified)) continue;
                var values = foreignKey.Properties.Select(property => entry.Property(property.Name).CurrentValue).ToArray();
                if (values.Any(value => value == null || value is int identifier && identifier <= 0)) continue;
                var principal = Find(foreignKey.PrincipalEntityType.ClrType, values);
                if (principal == null || (int)Entry(principal).Property("EmpresaId").CurrentValue! != EmpresaId)
                    throw new UnauthorizedAccessException("La relacion no pertenece a la empresa actual.");
            }
        }
    }

    public override int SaveChanges(bool acceptAllChangesOnSuccess)
    {
        var modificoContratos = ChangeTracker.Entries<Contrato>()
            .Any(entry => entry.State is EntityState.Added or EntityState.Modified or EntityState.Deleted);
        ValidarEmpresa();
        var resultado = base.SaveChanges(acceptAllChangesOnSuccess);
        if (modificoContratos) ContratosActualizados?.Invoke();
        return resultado;
    }

    public override async Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default)
    {
        var modificoContratos = ChangeTracker.Entries<Contrato>()
            .Any(entry => entry.State is EntityState.Added or EntityState.Modified or EntityState.Deleted);
        ValidarEmpresa();
        var resultado = await base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
        if (modificoContratos) ContratosActualizados?.Invoke();
        return resultado;
    }

    public DbSet<Usuario> Usuarios { get; set; }
    public DbSet<ConfiguracionEmpresa> ConfiguracionesEmpresa { get; set; }
    public DbSet<TarifaEnergia> TarifasEnergia { get; set; }
    public DbSet<TarifaLuz> TarifasLuz { get; set; }
    public DbSet<TarifaGas> TarifasGas { get; set; }
    public DbSet<TarifaTelefonia> TarifasTelefonia { get; set; }
    public DbSet<TarifaAlarma> TarifasAlarmas { get; set; }
    public DbSet<Servicio> Servicios { get; set; }
    public DbSet<ServicioGas> ServiciosGas { get; set; }
    public DbSet<Comercializadora> Comercializadoras { get; set; }
    public DbSet<Operadora> Operadoras { get; set; }
    public DbSet<EmpresaAlarma> EmpresasAlarmas { get; set; }
    public DbSet<Contrato> Contratos { get; set; }
    public DbSet<EstadoServicioContrato> EstadosServicioContrato { get; set; }
    public DbSet<Cliente> Clientes { get; set; }
    public DbSet<TareaPendiente> TareasPendientes { get; set; }
    public DbSet<FicheroCliente> FicherosClientes { get; set; }
    public DbSet<FicheroContrato> FicherosContratos { get; set; }
    public DbSet<ObservacionContrato> ObservacionesContratos { get; set; }
    public DbSet<HistoricoCambioContrato> HistoricoCambiosContratos { get; set; }
    public DbSet<LogAcceso> LogAccesos { get; set; }
    public DbSet<UsuarioComercializadora> UsuarioComercializadoras { get; set; }
    public DbSet<UsuarioOperadora> UsuarioOperadoras { get; set; }
    public DbSet<UsuarioEmpresaAlarma> UsuarioEmpresasAlarmas { get; set; }
    public DbSet<UsuarioComisionProveedor> UsuarioComisionesProveedores { get; set; }
    public DbSet<LogActivacionContrato> LogActivacionesContratos { get; set; }
    public DbSet<MensajeBienvenida> MensajesBienvenida { get; set; }
    public DbSet<NoticiaImagen> NoticiasImagenes { get; set; }
    public DbSet<IncidenciaLiquidacion> IncidenciasLiquidacion { get; set; }
    public DbSet<HistoricoLiquidacion> HistoricoLiquidaciones { get; set; }
    public DbSet<ConfiguracionComision> ConfiguracionesComision { get; set; }
    public DbSet<DetalleComisionLiquidacion> DetallesComisionLiquidacion { get; set; }
    public DbSet<Decomision> Decomisiones { get; set; }
    public DbSet<Incidencia> Incidencias { get; set; }
    public DbSet<ComentarioIncidencia> ComentariosIncidencias { get; set; }
    public DbSet<SolicitudOferta> SolicitudesOfertas { get; set; }
    public DbSet<HistoricoSipsConsulta> HistoricoSipsConsultas { get; set; }
    public DbSet<HistoricoComparativa> HistoricoComparativas { get; set; }
    public DbSet<PlantillaPreCarga> PlantillasPreCarga { get; set; }
    public DbSet<CodigoPostal> CodigosPostales { get; set; }
    public DbSet<SolicitudFirma> SolicitudesFirma { get; set; }
    public DbSet<SolicitudFirmaEvento> SolicitudesFirmaEventos { get; set; }
    public DbSet<SolicitudFirmaColaborador> SolicitudesFirmaColaboradores { get; set; }
    public DbSet<OptimeSolicitud> OptimeSolicitudes { get; set; }
    public DbSet<OptimeDocumento> OptimeDocumentos { get; set; }
    public DbSet<OptimeFirma> OptimeFirmas { get; set; }
}
