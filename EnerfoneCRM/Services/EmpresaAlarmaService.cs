using EnerfoneCRM.Data;
using EnerfoneCRM.Models;
using Microsoft.EntityFrameworkCore;

namespace EnerfoneCRM.Services
{
    public class EmpresaAlarmaService
    {
        private readonly DbContextProvider _dbContextProvider;

        public EmpresaAlarmaService(DbContextProvider dbContextProvider)
        {
            _dbContextProvider = dbContextProvider;
        }

        public async Task<List<EmpresaAlarma>> ObtenerTodasAsync()
        {
            await using var context = _dbContextProvider.CreateDbContext();
            return await context.EmpresasAlarmas
                .OrderBy(e => e.Nombre)
                .ToListAsync();
        }

        public async Task<List<EmpresaAlarma>> ObtenerActivasAsync()
        {
            await using var context = _dbContextProvider.CreateDbContext();
            return await context.EmpresasAlarmas
                .Where(e => e.Activo)
                .OrderBy(e => e.Nombre)
                .ToListAsync();
        }

        public async Task<EmpresaAlarma?> ObtenerPorIdAsync(int id)
        {
            await using var context = _dbContextProvider.CreateDbContext();
            return await context.EmpresasAlarmas.FindAsync(id);
        }

        public async Task<EmpresaAlarma?> ObtenerPorNombreAsync(string nombre)
        {
            await using var context = _dbContextProvider.CreateDbContext();
            return await context.EmpresasAlarmas
                .FirstOrDefaultAsync(e => e.Nombre.ToLower() == nombre.ToLower());
        }

        public async Task<bool> CrearAsync(EmpresaAlarma empresaAlarma)
        {
            await using var context = _dbContextProvider.CreateDbContext();
            empresaAlarma.FechaCreacion = DateTime.Now;
            context.EmpresasAlarmas.Add(empresaAlarma);
            return await context.SaveChangesAsync() > 0;
        }

        public async Task<bool> ActualizarAsync(EmpresaAlarma empresaAlarma)
        {
            await using var context = _dbContextProvider.CreateDbContext();
            // Desconectar cualquier entidad rastreada con el mismo ID
            var tracked = context.ChangeTracker.Entries<EmpresaAlarma>()
                .FirstOrDefault(e => e.Entity.Id == empresaAlarma.Id);
            
            if (tracked != null)
            {
                context.Entry(tracked.Entity).State = EntityState.Detached;
            }

            context.EmpresasAlarmas.Update(empresaAlarma);
            return await context.SaveChangesAsync() > 0;
        }

        public async Task<bool> EliminarAsync(int id)
        {
            await using var context = _dbContextProvider.CreateDbContext();
            var empresaAlarma = await context.EmpresasAlarmas.FindAsync(id);
            if (empresaAlarma == null) return false;

            context.EmpresasAlarmas.Remove(empresaAlarma);
            return await context.SaveChangesAsync() > 0;
        }
    }
}
