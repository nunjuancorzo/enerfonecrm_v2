using EnerfoneCRM.Components;
using EnerfoneCRM.Data;
using EnerfoneCRM.Services;
using Microsoft.EntityFrameworkCore;
using Blazored.SessionStorage;
using Blazored.LocalStorage;
using Microsoft.AspNetCore.HttpOverrides;

var builder = WebApplication.CreateBuilder(args);

// Agregar HttpContextAccessor para acceder al hostname
builder.Services.AddHttpContextAccessor();

// Configurar MySQL con selección dinámica de base de datos
builder.Services.AddDbContextFactory<ApplicationDbContext>(options =>
{
    // Configuración base - se actualizará dinámicamente en cada solicitud
    var baseConnectionString = builder.Configuration.GetConnectionString("DefaultConnection");
    options.UseMySql(baseConnectionString, ServerVersion.AutoDetect(baseConnectionString));
});

// Agregar DbContext scoped que se crea por solicitud
builder.Services.AddScoped<ApplicationDbContext>(serviceProvider =>
{
    var httpContextAccessor = serviceProvider.GetRequiredService<IHttpContextAccessor>();
    var httpContext = httpContextAccessor.HttpContext;
    
    // Obtener hostname desde diferentes fuentes (IIS puede usar X-Forwarded-Host)
    var host = httpContext?.Request.Headers["X-Forwarded-Host"].FirstOrDefault() 
               ?? httpContext?.Request.Headers["Host"].FirstOrDefault()
               ?? httpContext?.Request.Host.Host 
               ?? "";
    
    // Log temporal para debugging (ELIMINAR EN PRODUCCIÓN)
    Console.WriteLine($"[DEBUG] Hostname detectado: {host}");
    
    // Determinar qué base de datos usar según el hostname
    var databaseName = host.StartsWith("demo.", StringComparison.OrdinalIgnoreCase) 
        ? "demoenerfone" 
        : "enerfone_pre";
    
    Console.WriteLine($"[DEBUG] Base de datos seleccionada: {databaseName}");
    
    var configuration = serviceProvider.GetRequiredService<IConfiguration>();
    var baseConnectionString = configuration.GetConnectionString("DefaultConnection") ?? "";
    var connectionString = baseConnectionString.Replace("enerfone_pre", databaseName);
    
    var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
    optionsBuilder.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString));
    
    return new ApplicationDbContext(optionsBuilder.Options);
});

// Servicios
builder.Services.AddBlazoredSessionStorage();
builder.Services.AddBlazoredLocalStorage();
builder.Services.AddScoped<DbContextProvider>();
builder.Services.AddScoped<EnvironmentService>();
builder.Services.AddScoped<LogAccesoService>();
builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<UsuarioService>();
builder.Services.AddScoped<ConfiguracionService>();
builder.Services.AddScoped<TarifaEnergiaService>();
builder.Services.AddScoped<TarifaTelefoniaService>();
builder.Services.AddScoped<TarifaAlarmaService>();
builder.Services.AddScoped<ServicioService>();
builder.Services.AddScoped<ComercializadoraService>();
builder.Services.AddScoped<OperadoraService>();
builder.Services.AddScoped<EmpresaAlarmaService>();
builder.Services.AddScoped<ContratoService>();
builder.Services.AddScoped<ClienteService>();
builder.Services.AddScoped<TareaPendienteService>();
builder.Services.AddScoped<FicheroClienteService>();
builder.Services.AddScoped<FicheroContratoService>();
builder.Services.AddScoped<ObservacionContratoService>();
builder.Services.AddScoped<LogActivacionContratoService>();
builder.Services.AddScoped<MensajeBienvenidaService>();
builder.Services.AddScoped<IncidenciaLiquidacionService>();
builder.Services.AddScoped<EmailService>();
builder.Services.AddScoped<PdfLiquidacionService>();
builder.Services.AddSingleton<RepositorioService>();

// Integración externa de incidencias (INTEGRACION.md)
builder.Services.AddHttpClient<RevolappsIncidenciasService>();
builder.Services.AddScoped<RevolappsIncidenciasService>();

// Configurar tamaño máximo de formularios y archivos
builder.Services.Configure<Microsoft.AspNetCore.Http.Features.FormOptions>(options =>
{
    options.MultipartBodyLengthLimit = 10485760; // 10MB
});

// Configurar Kestrel para permitir archivos de hasta 10MB
builder.Services.Configure<Microsoft.AspNetCore.Server.Kestrel.Core.KestrelServerOptions>(options =>
{
    options.Limits.MaxRequestBodySize = 10485760; // 10MB
});

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

var app = builder.Build();

// Configurar ForwardedHeaders para IIS (necesario para detectar hostname correctamente)
app.UseForwardedHeaders(new ForwardedHeadersOptions
{
    ForwardedHeaders = ForwardedHeaders.XForwardedHost | ForwardedHeaders.XForwardedProto
});

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

// Configurar archivos estáticos desde almacenamiento externo
var storagePath = builder.Configuration.GetValue<string>("StoragePath");
if (string.IsNullOrEmpty(storagePath))
{
    // En desarrollo, usar carpeta storage dentro del proyecto
    storagePath = Path.Combine(Directory.GetCurrentDirectory(), "storage");
}

if (!Directory.Exists(storagePath))
{
    Directory.CreateDirectory(storagePath);
}

app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new Microsoft.Extensions.FileProviders.PhysicalFileProvider(storagePath),
    RequestPath = "/storage"
});

app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

// NOTA: Sincronización de entidades comentada para evitar duplicación de carpetas en el repositorio
// Los logos se gestionan desde la base de datos, no es necesario crear carpetas automáticamente
// using (var scope = app.Services.CreateScope())
// {
//     var repositorioService = scope.ServiceProvider.GetRequiredService<RepositorioService>();
//     await repositorioService.SincronizarEntidadesExistentes();
// }

app.Run();
