using backend.Contracts.Auth;
using backend.Options;
using backend.Services.Auditing;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace backend.Controllers;

[ApiController]
[Route("api/v1/auth")]
[EnableRateLimiting("auth")]
public class AuthController : ControllerBase
{
    private readonly AuthOptions _authOptions;
    private readonly IAuditService _auditService;

    public AuthController(IOptions<AuthOptions> authOptions, IAuditService auditService)
    {
        _authOptions = authOptions.Value;
        _auditService = auditService;
    }

    [AllowAnonymous]
    [HttpPost("token")]
    public async Task<IActionResult> Token([FromBody] AuthTokenRequest request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Username) || string.IsNullOrWhiteSpace(request.Password))
        {
            await _auditService.AuditAsync("auth-token", "credentials", "anonymous", "missing username or password", false, cancellationToken);
            return BadRequest(new ValidationProblemDetails(new Dictionary<string, string[]>
            {
                ["credentials"] = ["Username and password are required."]
            }));
        }

        var isValidCredentials = string.Equals(request.Username, _authOptions.DefaultUsername, StringComparison.Ordinal)
            && string.Equals(request.Password, _authOptions.DefaultPassword, StringComparison.Ordinal);

        if (!isValidCredentials)
        {
            await _auditService.AuditAsync("auth-token", "credentials", request.Username, "invalid credentials", false, cancellationToken);
            return Unauthorized();
        }

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, request.Username),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new Claim(ClaimTypes.Name, request.Username),
            new Claim(ClaimTypes.Role, "RdfAdmin")
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_authOptions.JwtKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var expires = DateTime.UtcNow.AddMinutes(_authOptions.TokenExpirationMinutes);

        var token = new JwtSecurityToken(
            issuer: _authOptions.JwtIssuer,
            audience: _authOptions.JwtAudience,
            claims: claims,
            expires: expires,
            signingCredentials: credentials);

        await _auditService.AuditAsync("auth-token", "credentials", request.Username, "token issued successfully", true, cancellationToken);

        return Ok(new AuthTokenResponse
        {
            AccessToken = new JwtSecurityTokenHandler().WriteToken(token),
            TokenType = "Bearer",
            ExpiresIn = _authOptions.TokenExpirationMinutes * 60
        });
    }
}
