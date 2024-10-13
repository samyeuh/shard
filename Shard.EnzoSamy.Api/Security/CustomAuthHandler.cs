using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;
using System.Text.Encodings.Web;

namespace Shard.EnzoSamy.Api.Security
{
    public class CustomAuthHandler : AuthenticationHandler<AuthenticationSchemeOptions>
    {
        private readonly AdminCredentials _adminCredentials;

        public CustomAuthHandler(
            IOptionsMonitor<AuthenticationSchemeOptions> options,
            ILoggerFactory logger,
            UrlEncoder encoder,
            ISystemClock clock,
            AdminCredentials adminCredentials) // Inject AdminCredentials
            : base(options, logger, encoder, clock)
        {
            _adminCredentials = adminCredentials;
        }

        protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            if (!Request.Headers.ContainsKey("Authorization"))
                return AuthenticateResult.Fail("Missing Authorization Header");

            try
            {
                var authHeader = AuthenticationHeaderValue.Parse(Request.Headers["Authorization"]);
                if (authHeader.Scheme != "Basic")
                    return AuthenticateResult.Fail("Invalid Authorization Scheme");

                var credentialBytes = Convert.FromBase64String(authHeader.Parameter);
                var credentials = Encoding.UTF8.GetString(credentialBytes).Split(':', 2);
                if (credentials.Length != 2)
                    return AuthenticateResult.Fail("Invalid Basic Authentication Credentials");

                var username = credentials[0];
                var password = credentials[1];

                if (username == _adminCredentials.Username && password == _adminCredentials.Password)
                {
                    var claims = new[] { new Claim(ClaimTypes.Name, username), new Claim(ClaimTypes.Role, "administrator") };
                    var identity = new ClaimsIdentity(claims, Scheme.Name);
                    var principal = new ClaimsPrincipal(identity);
                    var ticket = new AuthenticationTicket(principal, Scheme.Name);

                    return AuthenticateResult.Success(ticket);
                }
                else
                {
                    return AuthenticateResult.Fail("Invalid Username or Password");
                }
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error occurred during authentication");
                return AuthenticateResult.Fail("Invalid Authorization Header");
            }
        }
    }
}
