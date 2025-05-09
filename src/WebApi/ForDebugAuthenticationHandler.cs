#if DEBUG
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;
using System.Security.Claims;
using System.Text.Encodings.Web;

namespace Stellantis.ProjectName.WebApi
{
    internal class ForDebugAuthenticationHandler(IOptionsMonitor<AuthenticationSchemeOptions> options, ILoggerFactory logger, UrlEncoder encoder)
        : AuthenticationHandler<AuthenticationSchemeOptions>(options, logger, encoder)
    {
        protected override Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            Claim[] claims = [];
            ClaimsIdentity identity = new(claims, Scheme.Name);
            ClaimsPrincipal principal = new(identity);
            AuthenticationTicket ticket = new(principal, Scheme.Name);
            return Task.FromResult(AuthenticateResult.Success(ticket));
        }
    }
}
#endif