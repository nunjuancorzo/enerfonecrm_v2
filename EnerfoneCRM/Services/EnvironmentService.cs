using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Hosting;

namespace EnerfoneCRM.Services
{
    public class EnvironmentService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IWebHostEnvironment _environment;

        public EnvironmentService(IHttpContextAccessor httpContextAccessor, IWebHostEnvironment environment)
        {
            _httpContextAccessor = httpContextAccessor;
            _environment = environment;
        }

        public bool IsDemo()
        {
            var host = _httpContextAccessor.HttpContext?.Request.Host.Host;
            return host != null && host.StartsWith("demo.", StringComparison.OrdinalIgnoreCase);
        }

        public string GetEnvironmentName()
        {
            return IsDemo() ? "Demo" : "Producción";
        }

        public string GetDatabaseName()
        {
            // Si es entorno de desarrollo, usar la base de datos de desarrollo
            if (_environment.IsDevelopment())
            {
                return "enerfonecrm";
            }
            
            return IsDemo() ? "demoenerfone" : "enerfone_pre";
        }

        public string GetDatabaseUser()
        {
            // Si es entorno de desarrollo, usar root
            if (_environment.IsDevelopment())
            {
                return "root";
            }
            
            // En producci\u00f3n y demo usar enerfone
            return "enerfone";
        }

        public string GetDatabasePassword()
        {
            // Si es entorno de desarrollo, usar la contrase\u00f1a de desarrollo
            if (_environment.IsDevelopment())
            {
                return "A76262136.r";
            }
            
            // En producci\u00f3n y demo usar Salaiet6680.
            return "Salaiet6680.";
        }
    }
}
