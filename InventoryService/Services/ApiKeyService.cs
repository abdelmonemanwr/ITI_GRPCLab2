namespace InventoryService.Services
{
    public class ApiKeyService : IApiKeyService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IConfiguration _configuration;

        public ApiKeyService(IHttpContextAccessor httpContextAccessor, IConfiguration configuration)
        {
            _httpContextAccessor = httpContextAccessor;
            _configuration = configuration;
        }

        public bool Authenticate()
        {
            var context = _httpContextAccessor.HttpContext;
            if (!context.Request.Headers.TryGetValue(Declartions.ApiKeyHeaderName, out var apiKey))
            {
                return false;
            }
            var apiKeySettings = _configuration.GetSection(Declartions.ApiKeySettingsKey).Value;
            return apiKey == apiKeySettings;
        }
    }
}
