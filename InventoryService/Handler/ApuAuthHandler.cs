using InventoryService.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;
using System.Security.Claims;
using System.Text.Encodings.Web;

namespace InventoryService.Handler
{
    public class ApuAuthHandler : AuthenticationHandler<AuthenticationSchemeOptions>
    {
        private readonly IApiKeyService _apiKey;
        public ApuAuthHandler(IOptionsMonitor<AuthenticationSchemeOptions> options, ILoggerFactory logger, UrlEncoder encoder, ISystemClock clock, IApiKeyService apiKey) : base(options, logger, encoder, clock)
        {
            _apiKey = apiKey;
        }

        protected override Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            var isAuthenticated = _apiKey.Authenticate();
            if (!isAuthenticated)
            {
                return Task.FromResult(AuthenticateResult.Fail("Invalid API Key"));
            }
            var claims = new[]
            {
                new Claim(ClaimTypes.Name, "Name"),
                new Claim(ClaimTypes.Role, "User")
            };

            var identity = new ClaimsIdentity(claims, Scheme.Name);

            var principal = new ClaimsPrincipal(identity);

            var ticket = new AuthenticationTicket(principal, Scheme.Name);

            return Task.FromResult(AuthenticateResult.Success(ticket));
        }
    }
}
