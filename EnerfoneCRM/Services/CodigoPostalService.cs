using EnerfoneCRM.Data;
using EnerfoneCRM.Models;
using Microsoft.EntityFrameworkCore;

namespace EnerfoneCRM.Services
{
    public class CodigoPostalService
    {
        private readonly DbContextProvider _dbContextProvider;

        public CodigoPostalService(DbContextProvider dbContextProvider)
        {
            _dbContextProvider = dbContextProvider;
        }

        /// <summary>
        /// Obtiene todos los códigos postales activos ordenados por código postal
        /// </summary>
        public async Task<List<CodigoPostal>> ObtenerActivosAsync()
        {
            await using var context = _dbContextProvider.CreateDbContext();
            return await context.CodigosPostales
                .Where(cp => cp.Activo)
                .OrderBy(cp => cp.CodigoPostalValor)
                .ToListAsync();
        }

        /// <summary>
        /// Obtiene todos los códigos postales (activos e inactivos)
        /// </summary>
        public async Task<List<CodigoPostal>> ObtenerTodosAsync()
        {
            await using var context = _dbContextProvider.CreateDbContext();
            return await context.CodigosPostales
                .OrderBy(cp => cp.CodigoPostalValor)
                .ToListAsync();
        }

        /// <summary>
        /// Busca un código postal específico por su valor
        /// </summary>
        public async Task<CodigoPostal?> BuscarPorCodigoAsync(string codigoPostal)
        {
            await using var context = _dbContextProvider.CreateDbContext();
            return await context.CodigosPostales
                .FirstOrDefaultAsync(cp => cp.CodigoPostalValor == codigoPostal && cp.Activo);
        }

        /// <summary>
        /// Obtiene un código postal por su ID
        /// </summary>
        public async Task<CodigoPostal?> ObtenerPorIdAsync(int id)
        {
            await using var context = _dbContextProvider.CreateDbContext();
            return await context.CodigosPostales.FindAsync(id);
        }

        /// <summary>
        /// Busca códigos postales que coincidan con el texto de búsqueda (código, ciudad o provincia)
        /// </summary>
        public async Task<List<CodigoPostal>> BuscarAsync(string textoBusqueda)
        {
            await using var context = _dbContextProvider.CreateDbContext();
            
            if (string.IsNullOrWhiteSpace(textoBusqueda))
            {
                return await ObtenerActivosAsync();
            }

            var busqueda = textoBusqueda.Trim().ToLower();
            
            return await context.CodigosPostales
                .Where(cp => cp.Activo && (
                    cp.CodigoPostalValor.Contains(busqueda) ||
                    cp.Ciudad.ToLower().Contains(busqueda) ||
                    cp.Provincia.ToLower().Contains(busqueda)
                ))
                .OrderBy(cp => cp.CodigoPostalValor)
                .ToListAsync();
        }

        /// <summary>
        /// Crea un nuevo código postal
        /// </summary>
        public async Task<bool> CrearAsync(CodigoPostal codigoPostal)
        {
            await using var context = _dbContextProvider.CreateDbContext();
            
            // Verificar si ya existe
            var existe = await context.CodigosPostales
                .AnyAsync(cp => cp.CodigoPostalValor == codigoPostal.CodigoPostalValor);
            
            if (existe)
            {
                throw new InvalidOperationException($"El código postal {codigoPostal.CodigoPostalValor} ya existe");
            }

            codigoPostal.FechaCreacion = DateTime.Now;
            codigoPostal.FechaModificacion = DateTime.Now;
            
            context.CodigosPostales.Add(codigoPostal);
            return await context.SaveChangesAsync() > 0;
        }

        /// <summary>
        /// Actualiza un código postal existente
        /// </summary>
        public async Task<bool> ActualizarAsync(CodigoPostal codigoPostal)
        {
            await using var context = _dbContextProvider.CreateDbContext();
            
            // Desconectar cualquier entidad rastreada con el mismo ID
            var tracked = context.ChangeTracker.Entries<CodigoPostal>()
                .FirstOrDefault(e => e.Entity.Id == codigoPostal.Id);
            
            if (tracked != null)
            {
                context.Entry(tracked.Entity).State = EntityState.Detached;
            }

            codigoPostal.FechaModificacion = DateTime.Now;
            context.CodigosPostales.Update(codigoPostal);
            return await context.SaveChangesAsync() > 0;
        }

        /// <summary>
        /// Desactiva un código postal (no lo elimina físicamente)
        /// </summary>
        public async Task<bool> DesactivarAsync(int id)
        {
            await using var context = _dbContextProvider.CreateDbContext();
            var codigoPostal = await context.CodigosPostales.FindAsync(id);
            
            if (codigoPostal == null) return false;

            codigoPostal.Activo = false;
            codigoPostal.FechaModificacion = DateTime.Now;
            
            return await context.SaveChangesAsync() > 0;
        }

        /// <summary>
        /// Elimina físicamente un código postal
        /// </summary>
        public async Task<bool> EliminarAsync(int id)
        {
            await using var context = _dbContextProvider.CreateDbContext();
            var codigoPostal = await context.CodigosPostales.FindAsync(id);
            
            if (codigoPostal == null) return false;

            context.CodigosPostales.Remove(codigoPostal);
            return await context.SaveChangesAsync() > 0;
        }

        /// <summary>
        /// Obtiene todas las provincias únicas
        /// </summary>
        public async Task<List<string>> ObtenerProvinciasAsync()
        {
            await using var context = _dbContextProvider.CreateDbContext();
            return await context.CodigosPostales
                .Where(cp => cp.Activo)
                .Select(cp => cp.Provincia)
                .Distinct()
                .OrderBy(p => p)
                .ToListAsync();
        }

        /// <summary>
        /// Obtiene todos los códigos postales de una provincia específica
        /// </summary>
        public async Task<List<CodigoPostal>> ObtenerPorProvinciaAsync(string provincia)
        {
            await using var context = _dbContextProvider.CreateDbContext();
            return await context.CodigosPostales
                .Where(cp => cp.Activo && cp.Provincia.ToLower() == provincia.ToLower())
                .OrderBy(cp => cp.CodigoPostalValor)
                .ToListAsync();
        }
    }
}
