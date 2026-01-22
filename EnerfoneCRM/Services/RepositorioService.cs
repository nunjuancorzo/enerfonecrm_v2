using System;
using System.IO;

namespace EnerfoneCRM.Services
{
    public class RepositorioService
    {
        private readonly string directorioBase;
        private readonly IServiceProvider _serviceProvider;

        public RepositorioService(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
            directorioBase = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "repositorio");
            InicializarEstructura();
        }

        private void InicializarEstructura()
        {
            // Crear directorio base si no existe
            if (!Directory.Exists(directorioBase))
            {
                Directory.CreateDirectory(directorioBase);
            }

            // Crear carpetas principales
            var carpetasPrincipales = new[] { "Energía", "Telefonía", "Alarmas" };
            foreach (var carpeta in carpetasPrincipales)
            {
                var ruta = Path.Combine(directorioBase, carpeta);
                if (!Directory.Exists(ruta))
                {
                    Directory.CreateDirectory(ruta);
                }
            }
        }

        public void CrearCarpetaComercializadora(string nombreComercializadora, byte[]? logoContenido = null, string? logoArchivo = null)
        {
            try
            {
                var rutaComercializadora = Path.Combine(directorioBase, "Energía", SanitizarNombre(nombreComercializadora));
                if (!Directory.Exists(rutaComercializadora))
                {
                    Directory.CreateDirectory(rutaComercializadora);
                }

                // Copiar logo si existe
                if (logoContenido != null && logoContenido.Length > 0 && !string.IsNullOrEmpty(logoArchivo))
                {
                    var rutaLogo = Path.Combine(rutaComercializadora, logoArchivo);
                    File.WriteAllBytes(rutaLogo, logoContenido);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al crear carpeta de comercializadora: {ex.Message}");
            }
        }

        public void CrearCarpetaOperadora(string nombreOperadora, byte[]? logoContenido = null, string? logoArchivo = null)
        {
            try
            {
                var rutaOperadora = Path.Combine(directorioBase, "Telefonía", SanitizarNombre(nombreOperadora));
                if (!Directory.Exists(rutaOperadora))
                {
                    Directory.CreateDirectory(rutaOperadora);
                }

                // Copiar logo si existe
                if (logoContenido != null && logoContenido.Length > 0 && !string.IsNullOrEmpty(logoArchivo))
                {
                    var rutaLogo = Path.Combine(rutaOperadora, logoArchivo);
                    File.WriteAllBytes(rutaLogo, logoContenido);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al crear carpeta de operadora: {ex.Message}");
            }
        }

        public void CrearCarpetaEmpresaAlarma(string nombreEmpresa, byte[]? logoContenido = null, string? logoArchivo = null)
        {
            try
            {
                var rutaEmpresa = Path.Combine(directorioBase, "Alarmas", SanitizarNombre(nombreEmpresa));
                if (!Directory.Exists(rutaEmpresa))
                {
                    Directory.CreateDirectory(rutaEmpresa);
                }

                // Copiar logo si existe
                if (logoContenido != null && logoContenido.Length > 0 && !string.IsNullOrEmpty(logoArchivo))
                {
                    var rutaLogo = Path.Combine(rutaEmpresa, logoArchivo);
                    File.WriteAllBytes(rutaLogo, logoContenido);
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
                    CrearCarpetaComercializadora(comercializadora.Nombre, comercializadora.LogoContenido, comercializadora.LogoArchivo);
                }
                
                // Sincronizar Operadoras
                var operadoraService = scope.ServiceProvider.GetRequiredService<OperadoraService>();
                var operadoras = await operadoraService.ObtenerTodasAsync();
                foreach (var operadora in operadoras)
                {
                    CrearCarpetaOperadora(operadora.Nombre, operadora.LogoContenido, operadora.LogoArchivo);
                }
                
                // Sincronizar Empresas de Alarmas
                var empresaAlarmaService = scope.ServiceProvider.GetRequiredService<EmpresaAlarmaService>();
                var empresasAlarmas = await empresaAlarmaService.ObtenerTodasAsync();
                foreach (var empresa in empresasAlarmas)
                {
                    CrearCarpetaEmpresaAlarma(empresa.Nombre, empresa.LogoContenido, empresa.LogoArchivo);
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
