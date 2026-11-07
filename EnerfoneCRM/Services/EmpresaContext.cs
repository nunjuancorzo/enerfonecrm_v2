namespace EnerfoneCRM.Services;

public sealed class EmpresaContext
{
    public int EmpresaId { get; private set; }
    public bool EsSuperAdmin { get; private set; }

    internal void Establecer(int empresaId, bool esSuperAdmin = false)
    {
        EmpresaId = empresaId;
        EsSuperAdmin = esSuperAdmin;
    }

    public string RutaStorage(IConfiguration configuration)
    {
        if (EmpresaId <= 0) throw new UnauthorizedAccessException("Debe seleccionar una empresa.");
        var raiz = configuration.GetValue<string>("StoragePath");
        if (string.IsNullOrEmpty(raiz)) raiz = Path.Combine(Directory.GetCurrentDirectory(), "storage");
        return EmpresaId == configuration.GetValue("Empresas:EmpresaDatosExistentesId", 1)
            ? raiz : Path.Combine(raiz, "empresas", EmpresaId.ToString());
    }

    public string UrlStorage(IConfiguration configuration, string ruta)
    {
        if (EmpresaId <= 0) throw new UnauthorizedAccessException("Debe seleccionar una empresa.");
        return EmpresaId == configuration.GetValue("Empresas:EmpresaDatosExistentesId", 1)
            ? $"/storage/{ruta}" : $"/storage/empresas/{EmpresaId}/{ruta}";
    }
}