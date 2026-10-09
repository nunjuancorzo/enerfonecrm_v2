using System.Net;
using System.Net.Sockets;

namespace EnerfoneCRM.Services;

public static class FirmaIpResolver
{
    public static string ObtenerIpPublica(HttpContext? httpContext, string? ipNavegador = null)
    {
        var ipReenviada = httpContext?.Request.Headers["X-Forwarded-For"].FirstOrDefault()?.Split(',').FirstOrDefault()?.Trim();
        if (EsIpPublica(ipReenviada, out var ip)) return ip;

        var ipConexion = httpContext?.Connection.RemoteIpAddress?.ToString();
        if (EsIpPublica(ipConexion, out ip)) return ip;

        return EsIpPublica(ipNavegador, out ip) ? ip : string.Empty;
    }

    private static bool EsIpPublica(string? valor, out string ipPublica)
    {
        ipPublica = string.Empty;
        if (!IPAddress.TryParse(valor, out var direccion)) return false;
        if (direccion.IsIPv4MappedToIPv6) direccion = direccion.MapToIPv4();
        if (IPAddress.IsLoopback(direccion) || direccion.Equals(IPAddress.Any) || direccion.Equals(IPAddress.IPv6Any)) return false;

        var bytes = direccion.GetAddressBytes();
        if (direccion.AddressFamily == AddressFamily.InterNetwork)
        {
            var privada = bytes[0] == 0 || bytes[0] == 10 || bytes[0] == 127 || bytes[0] >= 224 ||
                (bytes[0] == 100 && bytes[1] is >= 64 and <= 127) ||
                (bytes[0] == 169 && bytes[1] == 254) ||
                (bytes[0] == 172 && bytes[1] is >= 16 and <= 31) ||
                (bytes[0] == 192 && bytes[1] == 168);
            if (privada) return false;
        }
        else if (direccion.IsIPv6LinkLocal || direccion.IsIPv6Multicast || (bytes[0] & 0xFE) == 0xFC || (bytes[0] & 0xE0) != 0x20)
        {
            return false;
        }

        ipPublica = direccion.ToString();
        return true;
    }
}