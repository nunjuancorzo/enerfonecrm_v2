using EnerfoneCRM.Data;
using EnerfoneCRM.Models;
using Microsoft.EntityFrameworkCore;

namespace EnerfoneCRM.Services
{
    public class IncidenciaLiquidacionService
    {
        private readonly DbContextProvider _dbContextProvider;

        public IncidenciaLiquidacionService(DbContextProvider dbContextProvider)
        {
            _dbContextProvider = dbContextProvider;
        }

        public async Task<List<IncidenciaLiquidacion>> ObtenerPorUsuarioAsync(int usuarioId)
        {
            await using var context = _dbContextProvider.CreateDbContext();
            return await context.Set<IncidenciaLiquidacion>()
                .Include(i => i.UsuarioColaborador)
                .Include(i => i.UsuarioAdministrador)
                .Where(i => i.UsuarioColaboradorId == usuarioId)
                .OrderByDescending(i => i.FechaCreacion)
                .ToListAsync();
        }

        public async Task<List<IncidenciaLiquidacion>> ObtenerTodasAsync()
        {
            await using var context = _dbContextProvider.CreateDbContext();
            return await context.Set<IncidenciaLiquidacion>()
                .Include(i => i.UsuarioColaborador)
                .Include(i => i.UsuarioAdministrador)
                .OrderByDescending(i => i.FechaCreacion)
                .ToListAsync();
        }

        public async Task<List<IncidenciaLiquidacion>> ObtenerPendientesAsync()
        {
            await using var context = _dbContextProvider.CreateDbContext();
            return await context.Set<IncidenciaLiquidacion>()
                .Include(i => i.UsuarioColaborador)
                .Where(i => i.Estado == "Pendiente")
                .OrderByDescending(i => i.FechaCreacion)
                .ToListAsync();
        }

        public async Task<IncidenciaLiquidacion?> ObtenerPorIdAsync(int id)
        {
            await using var context = _dbContextProvider.CreateDbContext();
            return await context.Set<IncidenciaLiquidacion>()
                .Include(i => i.UsuarioColaborador)
                .Include(i => i.UsuarioAdministrador)
                .FirstOrDefaultAsync(i => i.Id == id);
        }

        public async Task<IncidenciaLiquidacion> CrearAsync(IncidenciaLiquidacion incidencia)
        {
            await using var context = _dbContextProvider.CreateDbContext();
            context.Set<IncidenciaLiquidacion>().Add(incidencia);
            await context.SaveChangesAsync();
            return incidencia;
        }

        public async Task<bool> ResponderAsync(int incidenciaId, string respuesta, int administradorId)
        {
            await using var context = _dbContextProvider.CreateDbContext();
            var incidencia = await context.Set<IncidenciaLiquidacion>()
                .FirstOrDefaultAsync(i => i.Id == incidenciaId);

            if (incidencia == null)
                return false;

            incidencia.RespuestaAdministrador = respuesta;
            incidencia.UsuarioAdministradorId = administradorId;
            incidencia.FechaRespuesta = DateTime.Now;
            incidencia.Estado = "Respondida";

            await context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> EliminarAsync(int id)
        {
            await using var context = _dbContextProvider.CreateDbContext();
            var incidencia = await context.Set<IncidenciaLiquidacion>()
                .FirstOrDefaultAsync(i => i.Id == id);

            if (incidencia == null)
                return false;

            context.Set<IncidenciaLiquidacion>().Remove(incidencia);
            await context.SaveChangesAsync();
            return true;
        }

        public async Task<int> ContarPendientesPorUsuarioAsync(int usuarioId)
        {
            await using var context = _dbContextProvider.CreateDbContext();
            return await context.Set<IncidenciaLiquidacion>()
                .Where(i => i.UsuarioColaboradorId == usuarioId && i.Estado == "Pendiente")
                .CountAsync();
        }

        public async Task<bool> TienePendientesAsync(int usuarioId)
        {
            await using var context = _dbContextProvider.CreateDbContext();
            return await context.Set<IncidenciaLiquidacion>()
                .AnyAsync(i => i.UsuarioColaboradorId == usuarioId && i.Estado == "Pendiente");
        }
    }
}
