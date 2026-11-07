using EnerfoneCRM.Models;
using Microsoft.EntityFrameworkCore;

namespace EnerfoneCRM.Services;

public class EmpresaService(DbContextProvider dbContextProvider, EmpresaContext empresaContext)
{
    private void ExigirSuperAdmin()
    {
        if (!empresaContext.EsSuperAdmin)
            throw new UnauthorizedAccessException("Solo superadmin puede gestionar empresas.");
    }

    public async Task<List<Empresa>> ObtenerTodasAsync()
    {
        ExigirSuperAdmin();
        await using var context = dbContextProvider.CreateDbContext();
        return await context.Empresas.AsNoTracking().OrderBy(empresa => empresa.Nombre).ToListAsync();
    }

    public async Task GuardarAsync(Empresa empresa)
    {
        ExigirSuperAdmin();
        empresa.Nombre = empresa.Nombre.Trim();
        if (string.IsNullOrWhiteSpace(empresa.Nombre) || empresa.Nombre.Length > 500)
            throw new InvalidOperationException("Introduzca un nombre de empresa de hasta 500 caracteres.");
        await using var context = dbContextProvider.CreateDbContext();
        if (await context.Empresas.AnyAsync(existente => existente.Nombre == empresa.Nombre && existente.Id != empresa.Id))
            throw new InvalidOperationException("Ya existe una empresa con ese nombre.");
        if (empresa.Id == 0) context.Empresas.Add(empresa);
        else context.Empresas.Update(empresa);
        await context.SaveChangesAsync();
        EmpresasActualizadas?.Invoke();
    }

    public event Action? EmpresasActualizadas;
}