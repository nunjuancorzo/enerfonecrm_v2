using System.ComponentModel.DataAnnotations;
using System.Data.Common;
using EnerfoneCRM.Models;
using Microsoft.EntityFrameworkCore;

namespace EnerfoneCRM.Services;

public class PerfilSuperAdminService(DbContextProvider dbContextProvider, ILogger<PerfilSuperAdminService> logger)
{
    public async Task<PerfilSuperAdmin?> ObtenerAsync()
    {
        await using var context = dbContextProvider.CreateDbContext();
        if (!context.EsSuperAdmin)
            throw new UnauthorizedAccessException("Solo superadmin puede consultar su perfil global.");

        return await context.PerfilesSuperAdmin.AsNoTracking().SingleOrDefaultAsync(perfil => perfil.Id == 1);
    }

    public async Task<(bool exito, string mensaje)> GuardarAsync(PerfilSuperAdmin perfil)
    {
        await using var context = dbContextProvider.CreateDbContext();
        if (!context.EsSuperAdmin)
            return (false, "Solo superadmin puede editar su perfil global.");

        perfil.Email = perfil.Email?.Trim() ?? string.Empty;
        var errores = new List<ValidationResult>();
        if (!Validator.TryValidateObject(perfil, new ValidationContext(perfil), errores, validateAllProperties: true))
            return (false, string.Join(" ", errores.Select(error => error.ErrorMessage)));

        try
        {
            var guardado = await context.PerfilesSuperAdmin.SingleOrDefaultAsync(actual => actual.Id == 1);
            if (guardado == null)
            {
                guardado = new PerfilSuperAdmin();
                context.PerfilesSuperAdmin.Add(guardado);
            }

            guardado.Nombre = perfil.Nombre;
            guardado.Apellidos = perfil.Apellidos;
            guardado.Email = perfil.Email;
            guardado.Telefono = perfil.Telefono;
            guardado.Direccion = perfil.Direccion;
            guardado.CodigoPostal = perfil.CodigoPostal;
            guardado.Localidad = perfil.Localidad;
            await context.SaveChangesAsync();
            return (true, "Perfil global actualizado. Las solicitudes de usuarios se notificarán a este email.");
        }
        catch (Exception ex) when (ex is DbException or DbUpdateException)
        {
            logger.LogError(ex, "No se pudo guardar el perfil global de superadmin");
            return (false, "No se pudo guardar el perfil global. Comprueba la conexión y que se haya aplicado la actualización de base de datos.");
        }
    }
}
