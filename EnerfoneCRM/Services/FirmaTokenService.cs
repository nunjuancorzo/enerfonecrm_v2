using System.Security.Cryptography;
using System.Text;

namespace EnerfoneCRM.Services;

public class FirmaTokenService
{
    public string Generar()
    {
        return Convert.ToBase64String(RandomNumberGenerator.GetBytes(32))
            .TrimEnd('=')
            .Replace('+', '-')
            .Replace('/', '_');
    }

    public string Hash(string token)
    {
        return Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(token)));
    }

    public bool Coincide(string token, string hashEsperado)
    {
        var hashCalculado = Convert.FromHexString(Hash(token));
        var hashRecibido = Convert.FromHexString(hashEsperado);
        return CryptographicOperations.FixedTimeEquals(hashCalculado, hashRecibido);
    }
}