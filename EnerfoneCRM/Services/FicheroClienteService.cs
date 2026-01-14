using EnerfoneCRM.Data;
using EnerfoneCRM.Models;
using Microsoft.EntityFrameworkCore;

namespace EnerfoneCRM.Services
{
    public class FicheroClienteService
    {
        private readonly DbContextProvider _dbContextProvider;

        public FicheroClienteService(DbContextProvider dbContextProvider)
        {
            _dbContextProvider = dbContextProvider;
        }

        public async Task<List<FicheroCliente>> ObtenerPorClienteAsync(int idCliente)
        {
            await using var context = _dbContextProvider.CreateDbContext();
            return await context.FicherosClientes
                .Where(f => f.IdCliente == idCliente)
                .OrderByDescending(f => f.FechaCarga)
                .Select(f => new FicheroCliente
                {
                    Id = f.Id,
                    IdCliente = f.IdCliente,
                    TipoFichero = f.TipoFichero,
                    NombreArchivo = f.NombreArchivo,
                    FechaCarga = f.FechaCarga,
                    ContenidoArchivo = null // No cargamos el contenido en el listado
                })
                .ToListAsync();
        }

        public async Task<FicheroCliente?> ObtenerPorIdAsync(int id)
        {
            await using var context = _dbContextProvider.CreateDbContext();
            return await context.FicherosClientes.FindAsync(id);
        }

        public async Task<List<FicheroCliente>> ObtenerPorClienteYTipoAsync(int idCliente, string tipoFichero)
        {
            await using var context = _dbContextProvider.CreateDbContext();
            return await context.FicherosClientes
                .Where(f => f.IdCliente == idCliente && f.TipoFichero == tipoFichero)
                .OrderByDescending(f => f.FechaCarga)
                .Select(f => new FicheroCliente
                {
                    Id = f.Id,
                    IdCliente = f.IdCliente,
                    TipoFichero = f.TipoFichero,
                    NombreArchivo = f.NombreArchivo,
                    FechaCarga = f.FechaCarga,
                    ContenidoArchivo = null
                })
                .ToListAsync();
        }

        public async Task<(bool exito, string mensaje, FicheroCliente? fichero)> GuardarArchivoAsync(
            int idCliente,
            string tipoFichero,
            string nombreArchivo,
            byte[] contenido)
        {
            try
            {
                await using var context = _dbContextProvider.CreateDbContext();
                // Ya no eliminamos archivos anteriores, permitimos múltiples archivos del mismo tipo
                var fichero = new FicheroCliente
                {
                    IdCliente = idCliente,
                    TipoFichero = tipoFichero,
                    NombreArchivo = nombreArchivo,
                    ContenidoArchivo = contenido,
                    FechaCarga = DateTime.Now
                };

                context.FicherosClientes.Add(fichero);
                await context.SaveChangesAsync();

                return (true, "Archivo guardado correctamente", fichero);
            }
            catch (Exception ex)
            {
                return (false, $"Error al guardar archivo: {ex.Message}", null);
            }
        }

        public async Task<(bool exito, string mensaje, byte[]? contenido, string? nombreArchivo)> DescargarArchivoAsync(int idFichero)
        {
            try
            {
                await using var context = _dbContextProvider.CreateDbContext();
                var fichero = await context.FicherosClientes.FindAsync(idFichero);

                if (fichero == null)
                {
                    return (false, "Archivo no encontrado", null, null);
                }

                if (fichero.ContenidoArchivo == null)
                {
                    return (false, "Contenido del archivo no disponible", null, null);
                }

                return (true, "Archivo descargado", fichero.ContenidoArchivo, fichero.NombreArchivo);
            }
            catch (Exception ex)
            {
                return (false, $"Error al descargar archivo: {ex.Message}", null, null);
            }
        }

        public async Task<(bool exito, string mensaje)> EliminarArchivoAsync(int idFichero)
        {
            try
            {
                await using var context = _dbContextProvider.CreateDbContext();
                var fichero = await context.FicherosClientes.FindAsync(idFichero);

                if (fichero == null)
                {
                    return (false, "Archivo no encontrado");
                }

                context.FicherosClientes.Remove(fichero);
                await context.SaveChangesAsync();

                return (true, "Archivo eliminado correctamente");
            }
            catch (Exception ex)
            {
                return (false, $"Error al eliminar archivo: {ex.Message}");
            }
        }

        public static string ObtenerEtiquetaTipoFichero(string tipoFichero)
        {
            return tipoFichero switch
            {
                "DNIParticular" => "DNI Particular",
                "DNIPyme" => "DNI Representante (Pyme)",
                "CIFPyme" => "CIF (Pyme)",
                "EscriturasPyme" => "Escrituras (Pyme)",
                _ => tipoFichero
            };
        }

        public static string ObtenerIconoTipoFichero(string tipoFichero)
        {
            return tipoFichero switch
            {
                "DNIParticular" => "bi-person-badge",
                "DNIPyme" => "bi-person-vcard",
                "CIFPyme" => "bi-building",
                "EscriturasPyme" => "bi-file-earmark-text",
                _ => "bi-file-earmark"
            };
        }
    }
}
