using EnerfoneCRM.Data;
using EnerfoneCRM.Models;
using Microsoft.EntityFrameworkCore;

namespace EnerfoneCRM.Services
{
    public class OperadoraService
    {
        private readonly DbContextProvider _dbContextProvider;

        public OperadoraService(DbContextProvider dbContextProvider)
        {
            _dbContextProvider = dbContextProvider;
        }

        public async Task<List<Operadora>> ObtenerTodasAsync()
        {
            await using var context = _dbContextProvider.CreateDbContext();
            return await context.Operadoras
                .OrderBy(o => o.Nombre)
                .ToListAsync();
        }

        public async Task<List<Operadora>> ObtenerActivasAsync()
        {
            await using var context = _dbContextProvider.CreateDbContext();
            return await context.Operadoras
                .Where(o => o.Activo)
                .OrderBy(o => o.Nombre)
                .ToListAsync();
        }

        public async Task<Operadora?> ObtenerPorIdAsync(int id)
        {
            await using var context = _dbContextProvider.CreateDbContext();
            return await context.Operadoras.FindAsync(id);
        }

        public async Task<Operadora?> ObtenerPorNombreAsync(string nombre)
        {
            await using var context = _dbContextProvider.CreateDbContext();
            return await context.Operadoras
                .FirstOrDefaultAsync(o => o.Nombre.ToLower() == nombre.ToLower());
        }

        public async Task<bool> CrearAsync(Operadora operadora)
        {
            await using var context = _dbContextProvider.CreateDbContext();
            operadora.FechaCreacion = DateTime.Now;
            context.Operadoras.Add(operadora);
            return await context.SaveChangesAsync() > 0;
        }

        public async Task<bool> ActualizarAsync(Operadora operadora)
        {
            await using var context = _dbContextProvider.CreateDbContext();
            // Desconectar cualquier entidad rastreada con el mismo ID
            var tracked = context.ChangeTracker.Entries<Operadora>()
                .FirstOrDefault(e => e.Entity.Id == operadora.Id);
            
            if (tracked != null)
            {
                context.Entry(tracked.Entity).State = EntityState.Detached;
            }

            context.Operadoras.Update(operadora);
            return await context.SaveChangesAsync() > 0;
        }

        public async Task<bool> EliminarAsync(int id)
        {
            await using var context = _dbContextProvider.CreateDbContext();
            var operadora = await context.Operadoras.FindAsync(id);
            if (operadora == null) return false;

            context.Operadoras.Remove(operadora);
            return await context.SaveChangesAsync() > 0;
        }
    }
}
