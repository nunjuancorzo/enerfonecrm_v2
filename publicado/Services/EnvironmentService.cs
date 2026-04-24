using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;

namespace EnerfoneCRM.Services
{
    public class EnvironmentService
    {
        private readonly IWebHostEnvironment _environment;
        private readonly IConfiguration _configuration;

        public EnvironmentService(IWebHostEnvironment environment, IConfiguration configuration)
        {
            _environment = environment;
            _configuration = configuration;
        }

        public string GetEnvironmentName()
        {
            return "Producción";
        }

        public string GetDatabaseName()
        {
            // Leer de la cadena de conexión
            var connectionString = _configuration.GetConnectionString("DefaultConnection");
            if (!string.IsNullOrEmpty(connectionString))
            {
                // Extraer el nombre de la base de datos de la cadena de conexión
                var dbNameIndex = connectionString.IndexOf("Database=", StringComparison.OrdinalIgnoreCase);
                if (dbNameIndex >= 0)
                {
                    var start = dbNameIndex + "Database=".Length;
                    var end = connectionString.IndexOf(";", start);
                    if (end > start)
                    {
                        return connectionString.Substring(start, end - start);
                    }
                }
            }

            // Fallback a valores por defecto si no se puede leer de la configuración
            if (_environment.IsDevelopment())
            {
                return "enerfonecrm";
            }
            
            return "enerfone_pre";
        }

        public string GetDatabaseUser()
        {
            // Leer de la cadena de conexión
            var connectionString = _configuration.GetConnectionString("DefaultConnection");
            if (!string.IsNullOrEmpty(connectionString))
            {
                var userIndex = connectionString.IndexOf("User=", StringComparison.OrdinalIgnoreCase);
                if (userIndex >= 0)
                {
                    var start = userIndex + "User=".Length;
                    var end = connectionString.IndexOf(";", start);
                    if (end > start)
                    {
                        return connectionString.Substring(start, end - start);
                    }
                }
            }

            // Fallback
            if (_environment.IsDevelopment())
            {
                return "root";
            }
            
            return "enerfone";
        }

        public string GetDatabasePassword()
        {
            // Leer de la cadena de conexión
            var connectionString = _configuration.GetConnectionString("DefaultConnection");
            if (!string.IsNullOrEmpty(connectionString))
            {
                var passwordIndex = connectionString.IndexOf("Password=", StringComparison.OrdinalIgnoreCase);
                if (passwordIndex >= 0)
                {
                    var start = passwordIndex + "Password=".Length;
                    var end = connectionString.IndexOf(";", start);
                    if (end > start)
                    {
                        return connectionString.Substring(start, end - start);
                    }
                }
            }

            // Fallback
            if (_environment.IsDevelopment())
            {
                return "A76262136.r";
            }
            
            return "Salaiet6680.";
        }
    }
}
