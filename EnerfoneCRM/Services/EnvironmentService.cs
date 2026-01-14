using Microsoft.AspNetCore.Http;

namespace EnerfoneCRM.Services
{
    public class EnvironmentService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public EnvironmentService(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
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
    }
}
