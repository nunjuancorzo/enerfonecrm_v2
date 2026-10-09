namespace EnerfoneCRM.Models;

public class FacturaImportada
{
    public List<CampoFacturaImportada> Campos { get; set; } = new();
    public List<string> Advertencias { get; set; } = new();
    public string Origen { get; set; } = string.Empty;
    public string Texto { get; set; } = string.Empty;

    public string? Obtener(string clave) => Campos.FirstOrDefault(campo => campo.Clave == clave && campo.Seleccionado)?.Valor.Trim();
}

public class CampoFacturaImportada
{
    public string Clave { get; set; } = string.Empty;
    public string Etiqueta { get; set; } = string.Empty;
    public string Valor { get; set; } = string.Empty;
    public string Evidencia { get; set; } = string.Empty;
    public bool Seleccionado { get; set; }
    public bool Conflicto { get; set; }
    public bool Aplicable { get; set; } = true;
}