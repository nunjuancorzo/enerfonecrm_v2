using EnerfoneCRM.Data;
using EnerfoneCRM.Models;
using Microsoft.EntityFrameworkCore;

namespace EnerfoneCRM.Services
{
    public class FicheroContratoService
    {
        private readonly DbContextProvider _dbContextProvider;

        public FicheroContratoService(DbContextProvider dbContextProvider)
        {
            _dbContextProvider = dbContextProvider;
        }

        // Obtener todas las facturas de un contrato (sin BLOB para listar)
        public async Task<List<FicheroContrato>> ObtenerFacturasContratoAsync(int idContrato)
        {
            await using var context = _dbContextProvider.CreateDbContext();
            return await context.FicherosContratos
                .Where(f => f.IdContrato == idContrato && f.TipoFichero == "Factura")
                .Select(f => new FicheroContrato
                {
                    Id = f.Id,
                    IdContrato = f.IdContrato,
                    TipoFichero = f.TipoFichero,
                    NombreFichero = f.NombreFichero,
                    Fichero = Array.Empty<byte>()
                })
                .OrderBy(f => f.NombreFichero)
                .ToListAsync();
        }

        // Descargar una factura completa (con BLOB)
        public async Task<FicheroContrato?> DescargarFacturaAsync(int id)
        {
            await using var context = _dbContextProvider.CreateDbContext();
            return await context.FicherosContratos
                .Where(f => f.Id == id)
                .FirstOrDefaultAsync();
        }

        // Guardar una nueva factura
        public async Task<(bool exito, string mensaje)> GuardarFacturaAsync(int idContrato, string nombreArchivo, byte[] contenido)
        {
            try
            {
                await using var context = _dbContextProvider.CreateDbContext();
                var fichero = new FicheroContrato
                {
                    IdContrato = idContrato,
                    TipoFichero = "Factura",
                    NombreFichero = nombreArchivo,
                    Fichero = contenido
                };

                context.FicherosContratos.Add(fichero);
                await context.SaveChangesAsync();

                return (true, "Factura guardada correctamente");
            }
            catch (Exception ex)
            {
                return (false, $"Error al guardar la factura: {ex.Message}");
            }
        }

        // Eliminar una factura
        public async Task<(bool exito, string mensaje)> EliminarFacturaAsync(int id)
        {
            try
            {
                await using var context = _dbContextProvider.CreateDbContext();
                var fichero = await context.FicherosContratos.FindAsync(id);
                
                if (fichero == null)
                {
                    return (false, "Factura no encontrada");
                }

                context.FicherosContratos.Remove(fichero);
                await context.SaveChangesAsync();

                return (true, "Factura eliminada correctamente");
            }
            catch (Exception ex)
            {
                return (false, $"Error al eliminar la factura: {ex.Message}");
            }
        }
    }
}
