using Microsoft.EntityFrameworkCore;
using EnerfoneCRM.Models;

namespace EnerfoneCRM.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<Usuario> Usuarios { get; set; }
    public DbSet<ConfiguracionEmpresa> ConfiguracionesEmpresa { get; set; }
    public DbSet<TarifaEnergia> TarifasEnergia { get; set; }
    public DbSet<TarifaTelefonia> TarifasTelefonia { get; set; }
    public DbSet<TarifaAlarma> TarifasAlarmas { get; set; }
    public DbSet<Servicio> Servicios { get; set; }
    public DbSet<Comercializadora> Comercializadoras { get; set; }
    public DbSet<Operadora> Operadoras { get; set; }
    public DbSet<EmpresaAlarma> EmpresasAlarmas { get; set; }
    public DbSet<Contrato> Contratos { get; set; }
    public DbSet<Cliente> Clientes { get; set; }
    public DbSet<TareaPendiente> TareasPendientes { get; set; }
    public DbSet<FicheroCliente> FicherosClientes { get; set; }
    public DbSet<FicheroContrato> FicherosContratos { get; set; }
    public DbSet<ObservacionContrato> ObservacionesContratos { get; set; }
    public DbSet<LogAcceso> LogAccesos { get; set; }
    public DbSet<UsuarioComercializadora> UsuarioComercializadoras { get; set; }
    public DbSet<LogActivacionContrato> LogActivacionesContratos { get; set; }
    public DbSet<MensajeBienvenida> MensajesBienvenida { get; set; }
}
