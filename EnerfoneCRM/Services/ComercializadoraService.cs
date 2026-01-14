using EnerfoneCRM.Data;
using EnerfoneCRM.Models;
using Microsoft.EntityFrameworkCore;

namespace EnerfoneCRM.Services
{
    public class ComercializadoraService
    {
        private readonly DbContextProvider _dbContextProvider;

        public ComercializadoraService(DbContextProvider dbContextProvider)
        {
            _dbContextProvider = dbContextProvider;
        }

        public async Task<List<Comercializadora>> ObtenerTodasAsync()
        {
            await using var context = _dbContextProvider.CreateDbContext();
            return await context.Comercializadoras
                .OrderBy(c => c.Nombre)
                .ToListAsync();
        }

        public async Task<List<Comercializadora>> ObtenerActivasAsync()
        {
            await using var context = _dbContextProvider.CreateDbContext();
            return await context.Comercializadoras
                .Where(c => c.Activo)
                .OrderBy(c => c.Nombre)
                .ToListAsync();
        }

        public async Task<Comercializadora?> ObtenerPorIdAsync(int id)
        {
            await using var context = _dbContextProvider.CreateDbContext();
            return await context.Comercializadoras.FindAsync(id);
        }

        public async Task<Comercializadora?> ObtenerPorNombreAsync(string nombre)
        {
            await using var context = _dbContextProvider.CreateDbContext();
            return await context.Comercializadoras
                .FirstOrDefaultAsync(c => c.Nombre.ToLower() == nombre.ToLower());
        }

        public async Task<bool> CrearAsync(Comercializadora comercializadora)
        {
            await using var context = _dbContextProvider.CreateDbContext();
            comercializadora.FechaCreacion = DateTime.Now;
            context.Comercializadoras.Add(comercializadora);
            return await context.SaveChangesAsync() > 0;
        }

        public async Task<bool> ActualizarAsync(Comercializadora comercializadora)
        {
            await using var context = _dbContextProvider.CreateDbContext();
            // Desconectar cualquier entidad rastreada con el mismo ID
            var tracked = context.ChangeTracker.Entries<Comercializadora>()
                .FirstOrDefault(e => e.Entity.Id == comercializadora.Id);
            
            if (tracked != null)
            {
                context.Entry(tracked.Entity).State = EntityState.Detached;
            }

            context.Comercializadoras.Update(comercializadora);
            return await context.SaveChangesAsync() > 0;
        }

        public async Task<bool> EliminarAsync(int id)
        {
            await using var context = _dbContextProvider.CreateDbContext();
            var comercializadora = await context.Comercializadoras.FindAsync(id);
            if (comercializadora == null) return false;

            context.Comercializadoras.Remove(comercializadora);
            return await context.SaveChangesAsync() > 0;
        }
    }
}
