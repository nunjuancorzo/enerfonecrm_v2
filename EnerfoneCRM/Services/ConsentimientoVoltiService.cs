using System.Text.Json;
using Microsoft.AspNetCore.DataProtection;

namespace EnerfoneCRM.Services;

public class ConsentimientoVoltiService
{
    private static readonly SemaphoreSlim Escritura = new(1, 1);
    private readonly IConfiguration _configuration;
    private readonly IDataProtector _protector;
    private readonly Func<(int EmpresaId, int? UsuarioId, bool PuedeAutorizar)> _identidad;
    private sealed record Autorizacion(int EmpresaId, int UsuarioId, DateTime FechaUtc, bool Activa, int Version = 1);

    public ConsentimientoVoltiService(IConfiguration configuration, IDataProtectionProvider protection, AuthService auth)
        : this(configuration, protection, () => (auth.EmpresaId, auth.UsuarioActual?.Id, auth.EsAdministrador || auth.EsSuperAdmin))
    {
    }

    internal ConsentimientoVoltiService(IConfiguration configuration, IDataProtectionProvider protection,
        Func<(int EmpresaId, int? UsuarioId, bool PuedeAutorizar)> identidad)
    {
        _configuration = configuration;
        _protector = protection.CreateProtector("CorCRM.Volti.AutorizacionEmpresa.v1");
        _identidad = identidad;
    }

    public bool PuedeAutorizar => _identidad() is { EmpresaId: > 0, UsuarioId: not null, PuedeAutorizar: true };
    public event Action? AutorizacionCambiada;

    private string Directorio => _configuration["Gemini:SecretsDirectory"] ?? Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "CorCRM", "Secrets");
    private string Ruta(int empresaId) => Path.Combine(Directorio, $"volti-empresa-{empresaId}.protected");

    public async Task<bool> EstaAutorizadaAsync()
    {
        var identidad = _identidad();
        if (identidad.EmpresaId <= 0 || !identidad.UsuarioId.HasValue || identidad.UsuarioId == 0) return false;
        try
        {
            var ruta = Ruta(identidad.EmpresaId);
            if (!File.Exists(ruta)) return false;
            var json = _protector.Unprotect(await File.ReadAllTextAsync(ruta));
            var autorizacion = JsonSerializer.Deserialize<Autorizacion>(json);
            return autorizacion is { Activa: true, Version: 1 } && autorizacion.EmpresaId == identidad.EmpresaId;
        }
        catch (Exception excepcion) when (excepcion is IOException or UnauthorizedAccessException or JsonException or System.Security.Cryptography.CryptographicException)
        {
            return false;
        }
    }

    public Task AutorizarAsync() => GuardarAsync(true);
    public Task RevocarAsync() => GuardarAsync(false);

    public async Task ExigirAutorizacionAsync()
    {
        if (!await EstaAutorizadaAsync())
            throw new InvalidOperationException("Un administrador debe autorizar el uso de Gemini para esta empresa.");
    }

    private async Task GuardarAsync(bool activa)
    {
        var identidad = _identidad();
        if (identidad.EmpresaId <= 0 || !identidad.UsuarioId.HasValue || identidad.UsuarioId == 0 || !identidad.PuedeAutorizar)
            throw new UnauthorizedAccessException("Solo un administrador puede gestionar la autorizacion de Gemini para su empresa.");
        await Escritura.WaitAsync();
        try
        {
            Directory.CreateDirectory(Directorio);
            if (!OperatingSystem.IsWindows()) File.SetUnixFileMode(Directorio, UnixFileMode.UserRead | UnixFileMode.UserWrite | UnixFileMode.UserExecute);
            var ruta = Ruta(identidad.EmpresaId);
            var temporal = ruta + ".tmp";
            var autorizacion = new Autorizacion(identidad.EmpresaId, identidad.UsuarioId.Value, DateTime.UtcNow, activa);
            await File.WriteAllTextAsync(temporal, _protector.Protect(JsonSerializer.Serialize(autorizacion)));
            if (!OperatingSystem.IsWindows()) File.SetUnixFileMode(temporal, UnixFileMode.UserRead | UnixFileMode.UserWrite);
            File.Move(temporal, ruta, overwrite: true);
        }
        finally { Escritura.Release(); }
        AutorizacionCambiada?.Invoke();
    }
}