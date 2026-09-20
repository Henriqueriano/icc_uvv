using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using backend.Contracts.Auth;
using backend.Options;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace backend.Controllers;

[ApiController]
[Route("api/v1/auth")]
public class AuthController : ControllerBase
{
    private readonly AuthOptions _authOptions;

    public AuthController(IOptions<AuthOptions> authOptions)
    {
        _authOptions = authOptions.Value;
    }

    [AllowAnonymous]
    [HttpPost("token")]
    public IActionResult Token([FromBody] AuthTokenRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Username) || string.IsNullOrWhiteSpace(request.Password))
        {
            return BadRequest(new ValidationProblemDetails(new Dictionary<string, string[]>
            {
                ["credentials"] = ["Username and password are required."]
            }));
        }

        var isValidCredentials = string.Equals(request.Username, _authOptions.DefaultUsername, StringComparison.Ordinal)
            && string.Equals(request.Password, _authOptions.DefaultPassword, StringComparison.Ordinal);

        if (!isValidCredentials)
        {
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

        return Ok(new AuthTokenResponse
        {
            AccessToken = new JwtSecurityTokenHandler().WriteToken(token),
            TokenType = "Bearer",
            ExpiresIn = _authOptions.TokenExpirationMinutes * 60
        });
    }
}
