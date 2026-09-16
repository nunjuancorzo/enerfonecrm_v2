using EnerfoneCRM.Services;
using Xunit;

namespace EnerfoneCRM.Tests;

public class FirmaTokenServiceTests
{
    private readonly FirmaTokenService tokenService = new();

    [Fact]
    public void GenerarProduceTokenLargoYUrlSafe()
    {
        var token = tokenService.Generar();

        Assert.True(token.Length >= 40);
        Assert.DoesNotContain("+", token);
        Assert.DoesNotContain("/", token);
        Assert.DoesNotContain("=", token);
    }

    [Fact]
    public void TokenValidoCoincideConSuHash()
    {
        var token = tokenService.Generar();
        var hash = tokenService.Hash(token);

        Assert.True(tokenService.Coincide(token, hash));
    }

    [Fact]
    public void TokenManipuladoNoCoincide()
    {
        var token = tokenService.Generar();
        var hash = tokenService.Hash(token);
        var manipulado = token[..^1] + (token[^1] == 'A' ? 'B' : 'A');

        Assert.False(tokenService.Coincide(manipulado, hash));
    }

    [Fact]
    public void TokenInexistenteNoCoincide()
    {
        var hash = tokenService.Hash(tokenService.Generar());

        Assert.False(tokenService.Coincide(tokenService.Generar(), hash));
    }
}