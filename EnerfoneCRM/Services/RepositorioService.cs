using System;
using System.IO;
using Microsoft.Extensions.Configuration;

namespace EnerfoneCRM.Services
{
    public class RepositorioService
    {
        private readonly string directorioBase;
        private readonly IServiceProvider _serviceProvider;

        public RepositorioService(IServiceProvider serviceProvider, IConfiguration configuration)
        {
            _serviceProvider = serviceProvider;
            
            // Usar ruta configurable, con fallback a storage local en desarrollo
            var storagePath = configuration.GetValue<string>("StoragePath");
            if (string.IsNullOrEmpty(storagePath))
            {
                // En desarrollo, usar carpeta storage dentro del proyecto
                storagePath = Path.Combine(Directory.GetCurrentDirectory(), "storage");
            }
            
            directorioBase = Path.Combine(storagePath, "repositorio");
            
            // Crear solo el directorio base si no existe
            if (!Directory.Exists(directorioBase))
            {
                Directory.CreateDirectory(directorioBase);
            }
        }

        public void CrearCarpetaComercializadora(string nombreComercializadora)
        {
            try
            {
                // Asegurar que existe la carpeta Energía
                var carpetaEnergia = Path.Combine(directorioBase, "Energía");
                if (!Directory.Exists(carpetaEnergia))
                {
                    Directory.CreateDirectory(carpetaEnergia);
                }
                
                // Crear carpeta de la comercializadora
                var rutaComercializadora = Path.Combine(carpetaEnergia, SanitizarNombre(nombreComercializadora));
                if (!Directory.Exists(rutaComercializadora))
                {
                    Directory.CreateDirectory(rutaComercializadora);
                    Console.WriteLine($"[RepositorioService] Carpeta creada: {rutaComercializadora}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al crear carpeta de comercializadora: {ex.Message}");
            }
        }

        public void CrearCarpetaOperadora(string nombreOperadora)
        {
            try
            {
                // Asegurar que existe la carpeta Telefonía
                var carpetaTelefonia = Path.Combine(directorioBase, "Telefonía");
                if (!Directory.Exists(carpetaTelefonia))
                {
                    Directory.CreateDirectory(carpetaTelefonia);
                }
                
                // Crear carpeta de la operadora
                var rutaOperadora = Path.Combine(carpetaTelefonia, SanitizarNombre(nombreOperadora));
                if (!Directory.Exists(rutaOperadora))
                {
                    Directory.CreateDirectory(rutaOperadora);
                    Console.WriteLine($"[RepositorioService] Carpeta creada: {rutaOperadora}");
                }
            }
            catch (Exception ex)
            {
                // Asegurar que existe la carpeta Alarmas
                var carpetaAlarmas = Path.Combine(directorioBase, "Alarmas");
                if (!Directory.Exists(carpetaAlarmas))
                {
                    Directory.CreateDirectory(carpetaAlarmas);
                }
                
                // Crear carpeta de la empresa
                var rutaEmpresa = Path.Combine(carpetaAlarmas {ex.Message}");
            }
        }

        public void CrearCarpetaEmpresaAlarma(string nombreEmpresa)
        {
            try
            {
                var rutaEmpresa = Path.Combine(directorioBase, "Alarmas", SanitizarNombre(nombreEmpresa));
                if (!Directory.Exists(rutaEmpresa))
                {
                    Directory.CreateDirectory(rutaEmpresa);
                    Console.WriteLine($"[RepositorioService] Carpeta creada: {rutaEmpresa}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al crear carpeta de empresa de alarma: {ex.Message}");
            }
        }

        public async Task SincronizarEntidadesExistentes()
        {
            try
            {
                using var scope = _serviceProvider.CreateScope();
                
                // Sincronizar Comercializadoras
                var comercializadoraService = scope.ServiceProvider.GetRequiredService<ComercializadoraService>();
                var comercializadoras = await comercializadoraService.ObtenerTodasAsync();
                foreach (var comercializadora in comercializadoras)
                {
                    CrearCarpetaComercializadora(comercializadora.Nombre);
                }
                
                // Sincronizar Operadoras
                var operadoraService = scope.ServiceProvider.GetRequiredService<OperadoraService>();
                var operadoras = await operadoraService.ObtenerTodasAsync();
                foreach (var operadora in operadoras)
                {
                    CrearCarpetaOperadora(operadora.Nombre);
                }
                
                // Sincronizar Empresas de Alarmas
                var empresaAlarmaService = scope.ServiceProvider.GetRequiredService<EmpresaAlarmaService>();
                var empresasAlarmas = await empresaAlarmaService.ObtenerTodasAsync();
                foreach (var empresa in empresasAlarmas)
                {
                    CrearCarpetaEmpresaAlarma(empresa.Nombre);
                }
                
                Console.WriteLine("[RepositorioService] Sincronización de entidades completada");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[RepositorioService] Error al sincronizar entidades: {ex.Message}");
            }
        }

        private string SanitizarNombre(string nombre)
        {
            // Eliminar caracteres no válidos para nombres de carpeta
            var caracteresInvalidos = Path.GetInvalidFileNameChars();
            var nombreSanitizado = nombre;
            
            foreach (var caracter in caracteresInvalidos)
            {
                nombreSanitizado = nombreSanitizado.Replace(caracter, '_');
            }
            
            return nombreSanitizado;
        }
    }
}
