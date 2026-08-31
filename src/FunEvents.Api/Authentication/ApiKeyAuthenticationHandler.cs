using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;

namespace FunEvents.Api.Authentication;

public class ApiKeyAuthenticationHandler(
    IOptionsMonitor<AuthenticationSchemeOptions> options,
    ILoggerFactory logger,
    UrlEncoder encoder,
    IOptions<List<ApiKeyConfiguration>> apiKeys) : AuthenticationHandler<AuthenticationSchemeOptions>(options, logger, encoder)
{
    private const string ApiKeyHeader = "X-API-Key";

    private readonly List<ApiKeyConfiguration> _apiKeys = apiKeys.Value;

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        if (!Request.Headers.TryGetValue(ApiKeyHeader, out var apiKey)) return Task.FromResult(AuthenticateResult.NoResult());

        var client = _apiKeys.FirstOrDefault(x => x.Key == apiKey);

        if (client is null) return Task.FromResult(AuthenticateResult.Fail("Invalid API Key."));

        var claims = new[]
        {
            new Claim(ClaimTypes.Name, client.Name),
            new Claim("client_id", client.ClientId),
            new Claim(ClaimTypes.AuthenticationMethod, "ApiKey")
        };

        var identity = new ClaimsIdentity(claims, Scheme.Name);

        var principal = new ClaimsPrincipal(identity);

        var ticket = new AuthenticationTicket(principal, Scheme.Name);

        return Task.FromResult(AuthenticateResult.Success(ticket));
    }
}