using System.Text.Json;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.DataProtection;

namespace EnerfoneCRM.Services;

public class GeminiConfiguracionService(IConfiguration configuration, IDataProtectionProvider protection, AuthService auth)
{
    private static readonly SemaphoreSlim Escritura = new(1, 1);
    private string Directorio => configuration["Gemini:SecretsDirectory"] ?? Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "CorCRM", "Secrets");
    private string Ruta => Path.Combine(Directorio, "gemini.protected");
    private IDataProtector Protector => protection.CreateProtector("CorCRM.Gemini.Configuracion.v1");
    internal sealed record Configuracion(string Clave, string Modelo);

    internal Configuracion? Leer()
    {
        if (!File.Exists(Ruta)) return null;
        try { return JsonSerializer.Deserialize<Configuracion>(Protector.Unprotect(File.ReadAllText(Ruta))); }
        catch (Exception excepcion) when (excepcion is System.Security.Cryptography.CryptographicException or JsonException or IOException)
        {
            return null;
        }
    }

    public string Modelo => Leer()?.Modelo ?? configuration["Gemini:Model"] ?? GeminiService.ModeloPredeterminado;
    public bool Configurado => !string.IsNullOrWhiteSpace(Leer()?.Clave ?? configuration["Gemini:ApiKey"] ?? configuration["GEMINI_API_KEY"]);

    public async Task GuardarAsync(string clave, string modelo)
    {
        if (!auth.EsSuperAdmin) throw new UnauthorizedAccessException("Solo superadmin puede configurar Gemini.");
        clave = string.IsNullOrWhiteSpace(clave)
            ? Leer()?.Clave ?? configuration["Gemini:ApiKey"] ?? configuration["GEMINI_API_KEY"] ?? string.Empty
            : clave.Trim();
        modelo = modelo.Trim();
        if (clave.Length is < 10 or > 512 || clave.Any(char.IsWhiteSpace))
            throw new ArgumentException("Introduce una clave de API valida.");
        if (!Regex.IsMatch(modelo, "^[a-z0-9.-]{1,100}$", RegexOptions.CultureInvariant, TimeSpan.FromSeconds(1)))
            throw new ArgumentException("Introduce un nombre de modelo valido.");
        await Escritura.WaitAsync();
        try
        {
            Directory.CreateDirectory(Directorio);
            if (!OperatingSystem.IsWindows()) File.SetUnixFileMode(Directorio, UnixFileMode.UserRead | UnixFileMode.UserWrite | UnixFileMode.UserExecute);
            var temporal = Ruta + ".tmp";
            await File.WriteAllTextAsync(temporal, Protector.Protect(JsonSerializer.Serialize(new Configuracion(clave, modelo))));
            if (!OperatingSystem.IsWindows()) File.SetUnixFileMode(temporal, UnixFileMode.UserRead | UnixFileMode.UserWrite);
            File.Move(temporal, Ruta, overwrite: true);
        }
        finally { Escritura.Release(); }
    }
}