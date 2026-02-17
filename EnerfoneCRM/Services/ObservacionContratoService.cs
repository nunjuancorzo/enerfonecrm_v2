using EnerfoneCRM.Data;
using EnerfoneCRM.Models;
using Microsoft.EntityFrameworkCore;

namespace EnerfoneCRM.Services
{
    public class ObservacionContratoService
    {
        private readonly DbContextProvider _dbContextProvider;

        public ObservacionContratoService(DbContextProvider dbContextProvider)
        {
            _dbContextProvider = dbContextProvider;
        }

        public async Task<List<ObservacionContrato>> ObtenerPorContratoAsync(int idContrato)
        {
            await using var context = _dbContextProvider.CreateDbContext();
            return await context.ObservacionesContratos
                .Where(o => o.IdContrato == idContrato)
                .OrderByDescending(o => o.FechaHora)
                .ToListAsync();
        }

        public async Task<bool> CrearAsync(ObservacionContrato observacion)
        {
            try
            {
                // Si el usuario es superadmin, no registramos la observación (modo invisible)
                if (observacion.Usuario == "superadmin")
                {
                    Console.WriteLine($"[ObservacionContrato] SUPERADMIN - Observación NO registrada (modo invisible)");
                    return true;
                }
                
                await using var context = _dbContextProvider.CreateDbContext();
                observacion.FechaHora = DateTime.Now;
                context.ObservacionesContratos.Add(observacion);
                await context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> EliminarAsync(int id)
        {
            try
            {
                await using var context = _dbContextProvider.CreateDbContext();
                var observacion = await context.ObservacionesContratos.FindAsync(id);
                if (observacion == null) return false;

                context.ObservacionesContratos.Remove(observacion);
                await context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<ObservacionContrato?> ObtenerPorIdAsync(int id)
        {
            await using var context = _dbContextProvider.CreateDbContext();
            return await context.ObservacionesContratos.FindAsync(id);
        }
    }
}
