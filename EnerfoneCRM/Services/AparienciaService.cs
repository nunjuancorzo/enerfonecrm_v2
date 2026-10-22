namespace EnerfoneCRM.Services;

public sealed record PaletaVisual(
    string Id,
    string Nombre,
    string Acento,
    string BarraLateral,
    string Fondo)
{
    public string AcentoRgb => AparienciaService.ConvertirAcentoRgb(Acento);
    public string TextoAccion => AparienciaService.ObtenerTextoContraste(Acento);
    public string TextoBarra => AparienciaService.ObtenerTextoContraste(BarraLateral);
}

public class AparienciaService
{
    public const string PaletaPredeterminada = "azul";
    public const string PaletaPersonalizada = "personalizada";
    private static readonly PaletaVisual ColoresPersonalizadosPredeterminados = new(PaletaPersonalizada, "Personalizada", "#1D4ED8", "#172B4D", "#EFF6FF");

    public static IReadOnlyList<PaletaVisual> Paletas { get; } =
    [
        new("azul", "Azul corporativo", "#1D4ED8", "#172B4D", "#EFF6FF"),
        new("bosque", "Verde bosque", "#166534", "#17372C", "#F0FDF4"),
        new("indigo", "Índigo", "#5B21B6", "#2C2248", "#F5F3FF"),
        new("petroleo", "Petróleo", "#0E6A75", "#173A3D", "#ECFEFF"),
        new("grafito", "Grafito", "#374151", "#1F2329", "#F3F4F6"),
        new("granate", "Granate", "#9F2344", "#4A1F2F", "#FFF1F3")
    ];

    public PaletaVisual PaletaActualVisual { get; private set; } = Paletas[0];
    public string PaletaActual => PaletaActualVisual.Id;
    public string VariablesCss => $"--crm-accent:{PaletaActualVisual.Acento};--crm-accent-rgb:{PaletaActualVisual.AcentoRgb};--crm-sidebar:{PaletaActualVisual.BarraLateral};--crm-sidebar-text:{PaletaActualVisual.TextoBarra};--crm-canvas:{PaletaActualVisual.Fondo};--crm-on-accent:{PaletaActualVisual.TextoAccion}";
    public event Action? OnChange;

    public static bool EsPaletaValida(string? id) => id == PaletaPersonalizada || Paletas.Any(paleta => paleta.Id == id);

    public static bool EsColorValido(string? color) =>
        color is { Length: 7 } && color[0] == '#' && color.AsSpan(1).ToString().All(Uri.IsHexDigit);

    public static PaletaVisual CrearPersonalizada(string? acento, string? barraLateral, string? fondo) => new(
        PaletaPersonalizada,
        "Personalizada",
        EsColorValido(acento) ? acento! : Paletas[0].Acento,
        EsColorValido(barraLateral) ? barraLateral! : Paletas[0].BarraLateral,
        EsColorValido(fondo) ? fondo! : Paletas[0].Fondo);

    public static PaletaVisual Resolver(string? id, string? acento = null, string? barraLateral = null, string? fondo = null) =>
        id == PaletaPersonalizada
            ? CrearPersonalizada(acento, barraLateral, fondo)
            : Paletas.FirstOrDefault(paleta => paleta.Id == id) ?? Paletas[0];

    public void Aplicar(string? id, string? acento = null, string? barraLateral = null, string? fondo = null)
    {
        PaletaActualVisual = Resolver(id, acento, barraLateral, fondo);
        OnChange?.Invoke();
    }

    public static PaletaVisual ObtenerBasePersonalizada() => ColoresPersonalizadosPredeterminados;

    internal static string ConvertirAcentoRgb(string color)
    {
        var hex = color.TrimStart('#');
        return $"{Convert.ToInt32(hex[..2], 16)}, {Convert.ToInt32(hex.Substring(2, 2), 16)}, {Convert.ToInt32(hex.Substring(4, 2), 16)}";
    }

    internal static string ObtenerTextoContraste(string color)
    {
        var hex = color.TrimStart('#');
        var rojo = Convert.ToInt32(hex[..2], 16);
        var verde = Convert.ToInt32(hex.Substring(2, 2), 16);
        var azul = Convert.ToInt32(hex.Substring(4, 2), 16);
        return (0.299 * rojo + 0.587 * verde + 0.114 * azul) / 255 >= 0.58 ? "#000000" : "#FFFFFF";
    }
}