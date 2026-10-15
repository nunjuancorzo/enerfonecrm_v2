using System.Globalization;

namespace EnerfoneCRM.Services;

public static class DecimalPegado
{
    public static bool TryParse(string? texto, out decimal valor) =>
        decimal.TryParse(texto?.Trim().Replace(',', '.'),
            NumberStyles.AllowLeadingSign | NumberStyles.AllowDecimalPoint,
            CultureInfo.InvariantCulture, out valor);

    public static void Validar(IReadOnlyList<string> columnas, params int[] indices)
    {
        foreach (var indice in indices)
        {
            if (indice < columnas.Count && !string.IsNullOrWhiteSpace(columnas[indice])
                && !TryParse(columnas[indice], out _))
                throw new FormatException($"Columna {indice + 1}: decimal no válido «{columnas[indice]}». Usa coma o punto sin separadores de miles.");
        }
    }
}
