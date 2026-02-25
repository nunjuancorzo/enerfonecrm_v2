using System;
using System.IO;
using System.Text;
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
                // Sanitizar el nombre primero para evitar rutas anidadas
                var nombreSanitizado = SanitizarNombre(nombreComercializadora);
                
                // Buscar carpeta Energía existente (puede tener diferente codificación Unicode)
                var carpetaEnergia = ObtenerOCrearCarpetaBase("Energía");
                
                // Crear carpeta de la comercializadora directamente dentro de Energía
                var rutaComercializadora = Path.Combine(carpetaEnergia, nombreSanitizado);
                if (!Directory.Exists(rutaComercializadora))
                {
                    Directory.CreateDirectory(rutaComercializadora);
                    Console.WriteLine($"[RepositorioService] Carpeta comercializadora creada: {rutaComercializadora}");
                }
                else
                {
                    Console.WriteLine($"[RepositorioService] La carpeta ya existe: {rutaComercializadora}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[RepositorioService] Error al crear carpeta de comercializadora '{nombreComercializadora}': {ex.Message}");
            }
        }

        public async Task GuardarLogoComercializadora(string nombreComercializadora, byte[] logoContenido, string logoNombre)
        {
            try
            {
                if (logoContenido == null || logoContenido.Length == 0)
                {
                    Console.WriteLine($"[RepositorioService] No hay contenido de logo para guardar");
                    return;
                }

                // Sanitizar el nombre de la comercializadora
                var nombreSanitizado = SanitizarNombre(nombreComercializadora);
                
                // Obtener carpeta de energía y de la comercializadora
                var carpetaEnergia = ObtenerOCrearCarpetaBase("Energía");
                var rutaComercializadora = Path.Combine(carpetaEnergia, nombreSanitizado);
                
                // Crear la carpeta si no existe
                if (!Directory.Exists(rutaComercializadora))
                {
                    Directory.CreateDirectory(rutaComercializadora);
                }

                // Guardar el logo con su nombre original
                var rutaLogo = Path.Combine(rutaComercializadora, logoNombre);
                await File.WriteAllBytesAsync(rutaLogo, logoContenido);
                
                Console.WriteLine($"[RepositorioService] Logo guardado en: {rutaLogo}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[RepositorioService] Error al guardar logo de comercializadora '{nombreComercializadora}': {ex.Message}");
                throw; // Re-lanzar para que el llamador sepa que hubo un error
            }
        }

        public void CrearCarpetaOperadora(string nombreOperadora)
        {
            try
            {
                // Buscar carpeta Telefonía existente (puede tener diferente codificación Unicode)
                var carpetaTelefonia = ObtenerOCrearCarpetaBase("Telefonía");
                
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
                Console.WriteLine($"Error al crear carpeta de operadora: {ex.Message}");
            }
        }

        public async Task GuardarLogoOperadora(string nombreOperadora, byte[] logoContenido, string logoNombre)
        {
            try
            {
                if (logoContenido == null || logoContenido.Length == 0)
                {
                    Console.WriteLine($"[RepositorioService] No hay contenido de logo para guardar");
                    return;
                }

                // Sanitizar el nombre de la operadora
                var nombreSanitizado = SanitizarNombre(nombreOperadora);
                
                // Obtener carpeta de telefonía y de la operadora
                var carpetaTelefonia = ObtenerOCrearCarpetaBase("Telefonía");
                var rutaOperadora = Path.Combine(carpetaTelefonia, nombreSanitizado);
                
                // Crear la carpeta si no existe
                if (!Directory.Exists(rutaOperadora))
                {
                    Directory.CreateDirectory(rutaOperadora);
                }

                // Guardar el logo con su nombre original
                var rutaLogo = Path.Combine(rutaOperadora, logoNombre);
                await File.WriteAllBytesAsync(rutaLogo, logoContenido);
                
                Console.WriteLine($"[RepositorioService] Logo guardado en: {rutaLogo}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[RepositorioService] Error al guardar logo de operadora '{nombreOperadora}': {ex.Message}");
                throw; // Re-lanzar para que el llamador sepa que hubo un error
            }
        }

        public void CrearCarpetaEmpresaAlarma(string nombreEmpresa)
        {
            try
            {
                // Buscar carpeta Alarmas existente (puede tener diferente codificación Unicode)
                var carpetaAlarmas = ObtenerOCrearCarpetaBase("Alarmas");
                
                var rutaEmpresa = Path.Combine(carpetaAlarmas, SanitizarNombre(nombreEmpresa));
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

        public async Task GuardarLogoEmpresaAlarma(string nombreEmpresa, byte[] logoContenido, string logoNombre)
        {
            try
            {
                if (logoContenido == null || logoContenido.Length == 0)
                {
                    Console.WriteLine($"[RepositorioService] No hay contenido de logo para guardar");
                    return;
                }

                // Sanitizar el nombre de la empresa
                var nombreSanitizado = SanitizarNombre(nombreEmpresa);
                
                // Obtener carpeta de alarmas y de la empresa
                var carpetaAlarmas = ObtenerOCrearCarpetaBase("Alarmas");
                var rutaEmpresa = Path.Combine(carpetaAlarmas, nombreSanitizado);
                
                // Crear la carpeta si no existe
                if (!Directory.Exists(rutaEmpresa))
                {
                    Directory.CreateDirectory(rutaEmpresa);
                }

                // Guardar el logo con su nombre original
                var rutaLogo = Path.Combine(rutaEmpresa, logoNombre);
                await File.WriteAllBytesAsync(rutaLogo, logoContenido);
                
                Console.WriteLine($"[RepositorioService] Logo guardado en: {rutaLogo}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[RepositorioService] Error al guardar logo de empresa de alarma '{nombreEmpresa}': {ex.Message}");
                throw; // Re-lanzar para que el llamador sepa que hubo un error
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
            if (string.IsNullOrWhiteSpace(nombre))
                return "Sin_Nombre";
                
            // Normalizar Unicode a forma NFC para evitar duplicados por diferentes codificaciones
            var nombreNormalizado = nombre.Normalize(NormalizationForm.FormC);
            
            // Eliminar caracteres no válidos para nombres de carpeta
            var caracteresInvalidos = Path.GetInvalidFileNameChars();
            var nombreSanitizado = nombreNormalizado.Trim();
            
            foreach (var caracter in caracteresInvalidos)
            {
                nombreSanitizado = nombreSanitizado.Replace(caracter, '_');
            }
            
            // Además, reemplazar separadores de ruta explícitamente para evitar subdirectorios
            nombreSanitizado = nombreSanitizado.Replace('/', '_').Replace('\\', '_');
            
            Console.WriteLine($"[RepositorioService] Nombre sanitizado: '{nombre}' -> '{nombreSanitizado}'");
            
            return nombreSanitizado;
        }
        
        /// <summary>
        /// Normaliza el nombre de una carpeta base (Energía, Telefonía, Alarmas)
        /// para asegurar consistencia en la codificación Unicode
        /// </summary>
        private string NormalizarNombreCarpetaBase(string nombre)
        {
            return nombre.Normalize(NormalizationForm.FormC);
        }
        
        /// <summary>
        /// Obtiene la ruta de una carpeta base existente o la crea si no existe.
        /// Busca carpetas existentes independientemente de su codificación Unicode.
        /// </summary>
        /// <param name="nombreBase">Nombre de la carpeta base (Energía, Telefonía, Alarmas)</param>
        /// <returns>Ruta completa de la carpeta</returns>
        private string ObtenerOCrearCarpetaBase(string nombreBase)
        {
            // Normalizar el nombre que buscamos
            var nombreNormalizado = nombreBase.Normalize(NormalizationForm.FormC);
            
            // Verificar si el directorio base existe
            if (!Directory.Exists(directorioBase))
            {
                Directory.CreateDirectory(directorioBase);
            }
            
            // Buscar carpetas existentes que coincidan (independientemente de codificación Unicode)
            var carpetasExistentes = Directory.GetDirectories(directorioBase);
            foreach (var carpeta in carpetasExistentes)
            {
                var nombreCarpeta = Path.GetFileName(carpeta);
                var nombreCarpetaNormalizado = nombreCarpeta.Normalize(NormalizationForm.FormC);
                
                if (nombreCarpetaNormalizado.Equals(nombreNormalizado, StringComparison.OrdinalIgnoreCase))
                {
                    Console.WriteLine($"[RepositorioService] Carpeta base encontrada: {carpeta} (original: {nombreCarpeta})");
                    return carpeta; // Retornar la carpeta existente con su codificación original
                }
            }
            
            // Si no existe, crear con nombre normalizado
            var rutaNueva = Path.Combine(directorioBase, nombreNormalizado);
            Directory.CreateDirectory(rutaNueva);
            Console.WriteLine($"[RepositorioService] Carpeta base creada: {rutaNueva}");
            return rutaNueva;
        }
    }
}
