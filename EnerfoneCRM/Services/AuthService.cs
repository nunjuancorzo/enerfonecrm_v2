using EnerfoneCRM.Data;
using EnerfoneCRM.Models;
using Microsoft.EntityFrameworkCore;
using Blazored.SessionStorage;
using Blazored.LocalStorage;
using Microsoft.AspNetCore.DataProtection;
using System.Text.Json;
using Microsoft.JSInterop;

namespace EnerfoneCRM.Services;

public class AuthService
{
    private readonly DbContextProvider _dbContextProvider;
    private readonly ISessionStorageService _sessionStorage;
    private readonly ILocalStorageService _localStorage;
    private readonly LogAccesoService _logAccesoService;
    private readonly EmpresaContext _empresaContext;
    private readonly IDataProtector _sessionProtector;
    private readonly IJSRuntime _jsRuntime;
    private bool _recordarMe;
    internal sealed record SesionProtegida(int UsuarioId, int EmpresaId, DateTime CaducidadUtc);
    private Usuario? _usuarioActual;

    // Usuario superadmin hardcodeado (sin registro en BD ni logs)
    private const string SUPERADMIN_USERNAME = "superadmin";
    private const string SUPERADMIN_PASSWORD = "SuperAdmin2026!";

    public AuthService(
        DbContextProvider dbContextProvider, 
        ISessionStorageService sessionStorage,
        ILocalStorageService localStorage,
        LogAccesoService logAccesoService,
        EmpresaContext empresaContext,
        IDataProtectionProvider dataProtectionProvider,
        IJSRuntime jsRuntime)
    {
        _dbContextProvider = dbContextProvider;
        _sessionStorage = sessionStorage;
        _localStorage = localStorage;
        _logAccesoService = logAccesoService;
        _empresaContext = empresaContext;
        _sessionProtector = dataProtectionProvider.CreateProtector("EnerfoneCRM.SesionEmpresas.v1");
        _jsRuntime = jsRuntime;
    }

    public Usuario? UsuarioActual => _usuarioActual;
    public bool EstaAutenticado => _usuarioActual != null;
    public bool EsAdministrador => _usuarioActual?.Rol == "Administrador";
    public bool EsSuperAdmin => _usuarioActual?.Id == -1 && _usuarioActual.NombreUsuario == SUPERADMIN_USERNAME;
    public int EmpresaId => _empresaContext.EmpresaId;
    public bool EsDirectorComercial => _usuarioActual?.Rol == "Director comercial";
    public bool EsJefeVentas => _usuarioActual?.Rol == "Jefe de ventas";
    public bool EsGestor => _usuarioActual?.Rol == "Gestor";
    public bool EsProveedor => Usuario.EsRolProveedor(_usuarioActual?.Rol);
    public bool EsProveedorEnergia => EsProveedor && _usuarioActual?.TipoProveedor == "energia";
    public bool EsProveedorTelefonia => EsProveedor && _usuarioActual?.TipoProveedor == "telefonia";
    public bool EsProveedorAlarmas => EsProveedor && _usuarioActual?.TipoProveedor == "alarmas";
    public bool EsComercializadora => EsProveedorEnergia;
    public bool EsUsuario => _usuarioActual?.Rol == "Colaborador";
    public bool EsBackoffice => _usuarioActual?.Rol == "Backoffice";
    
    // Propiedad auxiliar: Administrador o Backoffice (para permisos generales)
    // Backoffice tiene los mismos permisos que Administrador excepto gestión de usuarios
    public bool EsAdminOBackoffice => EsAdministrador || EsBackoffice;

    // Métodos de verificación de permisos de visualización (solo aplican a Backoffice)
    public bool PuedeVerClientes => _usuarioActual?.Rol != "Backoffice" || _usuarioActual.PuedeVerClientes;
    public bool PuedeVerContratos => _usuarioActual?.Rol != "Backoffice" || _usuarioActual.PuedeVerContratos;
    public bool PuedeVerTarifas => _usuarioActual?.Rol != "Backoffice" || _usuarioActual.PuedeVerTarifas;
    public bool PuedeVerLiquidaciones => _usuarioActual?.Rol != "Backoffice" || _usuarioActual.PuedeVerLiquidaciones;
    public bool PuedeVerSips => _usuarioActual?.Rol != "Backoffice" || _usuarioActual.PuedeVerSips;
    public bool PuedeVerIncidencias => _usuarioActual?.Rol != "Backoffice" || _usuarioActual.PuedeVerIncidencias;
    public bool PuedeVerOfertas => _usuarioActual?.Rol != "Backoffice" || _usuarioActual.PuedeVerOfertas;
    public bool PuedeVerUsuarios => EsSuperAdmin;

    // Métodos de verificación de permisos de gestión (Administrador o Backoffice con permiso)
    // Backoffice con acceso a un módulo tiene los mismos permisos que Administrador en ese módulo
    public bool PuedeGestionarClientes => EsAdministrador || (EsBackoffice && _usuarioActual?.PuedeVerClientes == true);
    public bool PuedeGestionarContratos => EsAdministrador || (EsBackoffice && _usuarioActual?.PuedeVerContratos == true);
    public bool PuedeGestionarTarifas => EsAdministrador || (EsBackoffice && _usuarioActual?.PuedeVerTarifas == true);
    public bool PuedeGestionarLiquidaciones => EsAdministrador || (EsBackoffice && _usuarioActual?.PuedeVerLiquidaciones == true);
    public bool PuedeGestionarSips => EsAdministrador || (EsBackoffice && _usuarioActual?.PuedeVerSips == true);
    public bool PuedeGestionarIncidencias => EsAdministrador || (EsBackoffice && _usuarioActual?.PuedeVerIncidencias == true);
    public bool PuedeGestionarOfertas => EsAdministrador || (EsBackoffice && _usuarioActual?.PuedeVerOfertas == true);
    
    // USUARIOS: Solo Administrador puede gestionarlos (seguridad)
    public bool PuedeGestionarUsuarios => EsSuperAdmin;

    public event Action? OnAuthStateChanged;

    public async Task<bool> CargarSesionAsync()
    {
        // Si ya hay un usuario autenticado, no hacer nada (idempotente)
        // Esto evita disparar eventos múltiples veces
        if (_usuarioActual != null)
        {
            return true;
        }

        try
        {
            // Primero intentar desde SessionStorage (sesión actual)
            var token = await _sessionStorage.GetItemAsync<string>("sesionProtegida");
            if (string.IsNullOrEmpty(token))
            {
                token = await _localStorage.GetItemAsync<string>("sesionProtegida");
                _recordarMe = !string.IsNullOrEmpty(token);
            }
            if (string.IsNullOrEmpty(token)) return false;
            var sesion = JsonSerializer.Deserialize<SesionProtegida>(_sessionProtector.Unprotect(token));
            if (sesion == null || sesion.CaducidadUtc <= DateTime.UtcNow) return false;
            await GuardarCookieAsync(token, _recordarMe);
            int? usuarioId = sesion.UsuarioId;
            
            if (usuarioId.HasValue)
            {
                // Si es el ID especial del superadmin (-1), recrear el usuario
                if (usuarioId.Value == -1)
                {
                    _usuarioActual = new Usuario
                    {
                        Id = -1,
                        NombreUsuario = SUPERADMIN_USERNAME,
                        Nombre = "Super",
                        Apellidos = "Admin",
                        Email = "superadmin@system.local",
                        Rol = "Administrador",
                        Activo = true,
                        ExcluirLogAcceso = true
                    };
                    _empresaContext.Establecer(0, true);
                    await _jsRuntime.InvokeVoidAsync("voltiaApariencia.establecerModo", "superadmin");
                    if (sesion.EmpresaId > 0)
                    {
                        await using var empresas = _dbContextProvider.CreateDbContext();
                        if (await empresas.Empresas.AnyAsync(empresa => empresa.Id == sesion.EmpresaId && empresa.Activa))
                        {
                            _empresaContext.Establecer(sesion.EmpresaId, true);
                            _usuarioActual.EmpresaId = sesion.EmpresaId;
                        }
                    }
                    Console.WriteLine($"[AUTH] SUPERADMIN restaurado de sesión - modo invisible");
                    NotificarCambioAutenticacion();
                    return true;
                }
                
                // Usuario normal de BD
                if (usuarioId.Value > 0)
                {
                    await using var context = _dbContextProvider.CreateAuthenticationDbContext();
                    _usuarioActual = await context.Usuarios.FindAsync(usuarioId.Value);
                    if (_usuarioActual != null && _usuarioActual.Activo &&
                        await context.Empresas.AnyAsync(empresa => empresa.Id == _usuarioActual.EmpresaId && empresa.Activa))
                    {
                        _empresaContext.Establecer(_usuarioActual.EmpresaId);
                        await _jsRuntime.InvokeVoidAsync("voltiaApariencia.establecerModo", "empresa");
                        await CargarTipoProveedorAsync(_usuarioActual, context);
                        Console.WriteLine($"[AUTH] Usuario restaurado de sesión: {_usuarioActual.NombreUsuario}");
                        NotificarCambioAutenticacion();
                        return true;
                    }
                    _usuarioActual = null;
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[AUTH] Error al cargar sesión: {ex.Message}");
            _usuarioActual = null;
            _empresaContext.Establecer(0);
        }
        return false;
    }

    public async Task<(bool exito, string mensaje)> Login(string nombreUsuario, string password)
    {
        try
        {
            // Verificar si es el superadmin hardcodeado
            if (nombreUsuario == SUPERADMIN_USERNAME && password == SUPERADMIN_PASSWORD)
            {
                Console.WriteLine($"[AUTH] Login SUPERADMIN detectado - modo invisible activado");
                
                // Crear usuario superadmin sin registrar en BD
                _usuarioActual = new Usuario
                {
                    Id = -1, // ID especial para identificarlo internamente
                    NombreUsuario = SUPERADMIN_USERNAME,
                    Nombre = "Super",
                    Apellidos = "Admin",
                    Email = "superadmin@system.local",
                    Rol = "Administrador",
                    Activo = true,
                    ExcluirLogAcceso = true
                };
                _empresaContext.Establecer(0, true);
                
                Console.WriteLine($"[AUTH] SUPERADMIN autenticado - SIN registro en logs");
                NotificarCambioAutenticacion();
                return (true, "Login exitoso");
            }

            await using var context = _dbContextProvider.CreateAuthenticationDbContext();
            Console.WriteLine($"[AUTH] Intentando login para: {nombreUsuario}");
            
            var usuario = await context.Usuarios
                .FirstOrDefaultAsync(u => u.NombreUsuario == nombreUsuario);

            if (usuario == null)
            {
                Console.WriteLine($"[AUTH] Usuario no encontrado: {nombreUsuario}");
                return (false, "Usuario no encontrado");
            }

            Console.WriteLine($"[AUTH] Usuario encontrado. Rol: {usuario.Rol}");

            // Verificar si el usuario está activo
            if (!usuario.Activo)
            {
                Console.WriteLine($"[AUTH] Usuario inactivo: {nombreUsuario}");
                return (false, "Tu cuenta está pendiente de activación. Contacta con el administrador.");
            }

            if (!await context.Empresas.AnyAsync(empresa => empresa.Id == usuario.EmpresaId && empresa.Activa))
                return (false, "La empresa de este usuario no esta activa.");

            // Comparación directa de contraseñas en texto plano
            if (usuario.PasswordHash != password)
            {
                Console.WriteLine($"[AUTH] Contraseña incorrecta");
                return (false, "Contraseña incorrecta");
            }

            await CargarTipoProveedorAsync(usuario, context);
            _usuarioActual = usuario;
            _empresaContext.Establecer(usuario.EmpresaId);
            Console.WriteLine($"[AUTH] Login exitoso. Usuario autenticado: {_usuarioActual.NombreUsuario}");
            
            // Registrar el acceso en el log (solo si no está excluido)
            if (!usuario.ExcluirLogAcceso)
            {
                await _logAccesoService.RegistrarAccesoAsync(_usuarioActual.Id, _usuarioActual.NombreUsuario, _usuarioActual.Rol);
            }
            
            NotificarCambioAutenticacion();

            return (true, "Login exitoso");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[AUTH] Error en login: {ex.Message}");
            return (false, $"Error: {ex.Message}");
        }
    }

    private static async Task CargarTipoProveedorAsync(Usuario usuario, ApplicationDbContext context)
    {
        usuario.TipoProveedor = null;
        if (!Usuario.EsRolProveedor(usuario.Rol)) return;

        if (!string.IsNullOrWhiteSpace(usuario.Comercializadora) ||
            await context.UsuarioComercializadoras.AnyAsync(uc => uc.UsuarioId == usuario.Id))
        {
            usuario.TipoProveedor = "energia";
            return;
        }

        if (await context.UsuarioOperadoras.AnyAsync(uo => uo.UsuarioId == usuario.Id))
        {
            usuario.TipoProveedor = "telefonia";
            return;
        }

        if (await context.UsuarioEmpresasAlarmas.AnyAsync(uea => uea.UsuarioId == usuario.Id))
            usuario.TipoProveedor = "alarmas";
    }

    public async Task GuardarSesionAsync(bool recordarMe = false)
    {
        _recordarMe = recordarMe;
        try
        {
            if (_usuarioActual != null)
            {
                await _jsRuntime.InvokeVoidAsync("voltiaApariencia.establecerModo", EsSuperAdmin ? "superadmin" : "empresa");
                // Siempre guardar en SessionStorage
                var token = _sessionProtector.Protect(JsonSerializer.Serialize(new SesionProtegida(
                    _usuarioActual.Id, EmpresaId, DateTime.UtcNow.Add(recordarMe ? TimeSpan.FromDays(30) : TimeSpan.FromHours(12)))));
                await _sessionStorage.SetItemAsync("sesionProtegida", token);
                await GuardarCookieAsync(token, recordarMe);
                Console.WriteLine($"[AUTH] Sesión guardada en SessionStorage con ID: {_usuarioActual.Id}");
                
                // Si marcó "Recordarme", también guardar en LocalStorage
                if (recordarMe)
                {
                    await _localStorage.SetItemAsync("sesionProtegida", token);
                    Console.WriteLine($"[AUTH] Sesión guardada en LocalStorage (Recordarme) con ID: {_usuarioActual.Id}");
                }
                else
                {
                    // Si no marcó "Recordarme", limpiar LocalStorage por si acaso
                    await _localStorage.RemoveItemAsync("sesionProtegida");
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[AUTH] Error al guardar sesión: {ex.Message}");
        }
    }

    public async Task Logout()
    {
        _usuarioActual = null;
        _empresaContext.Establecer(0);
        try
        {
            await _sessionStorage.RemoveItemAsync("usuarioId");
            await _localStorage.RemoveItemAsync("usuarioId");
            await _sessionStorage.RemoveItemAsync("sesionProtegida");
            await _localStorage.RemoveItemAsync("sesionProtegida");
            await using var module = await _jsRuntime.InvokeAsync<IJSObjectReference>("import", "./js/empresa-session.js");
            await module.InvokeVoidAsync("eliminar");
            await _jsRuntime.InvokeVoidAsync("voltiaApariencia.limpiar");
            // NO limpiar las credenciales guardadas (recordar_usuario y recordar_password)
            // para que persistan incluso después del logout
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[AUTH] Error al limpiar sesión: {ex.Message}");
        }
        Console.WriteLine("[AUTH] Sesión cerrada");
        NotificarCambioAutenticacion();
    }

    private void NotificarCambioAutenticacion()
    {
        OnAuthStateChanged?.Invoke();
    }

    private async Task GuardarCookieAsync(string token, bool recordar)
    {
        await using var module = await _jsRuntime.InvokeAsync<IJSObjectReference>("import", "./js/empresa-session.js");
        await module.InvokeVoidAsync("guardar", token, recordar);
    }

    public async Task SeleccionarEmpresaAsync(int empresaId)
    {
        if (!EsSuperAdmin) throw new UnauthorizedAccessException("Solo superadmin puede seleccionar empresas.");
        await using var context = _dbContextProvider.CreateDbContext();
        if (!await context.Empresas.AnyAsync(empresa => empresa.Id == empresaId && empresa.Activa))
            throw new InvalidOperationException("Empresa no disponible.");
        _empresaContext.Establecer(empresaId, true);
        _usuarioActual!.EmpresaId = empresaId;
        await GuardarSesionAsync(_recordarMe);
        NotificarCambioAutenticacion();
    }
}
