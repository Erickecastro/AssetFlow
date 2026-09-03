using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace AssetFlow.Api.Authentication;

public sealed class TokenService
{
    private readonly JwtOptions _options;

    public TokenService(IOptions<JwtOptions> options)
    {
        _options = options.Value;
    }

    public string? CreateToken(string username, string password)
    {
        if (string.IsNullOrWhiteSpace(_options.DemoUsername) ||
            string.IsNullOrWhiteSpace(_options.DemoPassword) ||
            !string.Equals(username, _options.DemoUsername, StringComparison.Ordinal) ||
            !string.Equals(password, _options.DemoPassword, StringComparison.Ordinal))
        {
            return null;
        }

        var credentials = new SigningCredentials(
            new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_options.SigningKey)),
            SecurityAlgorithms.HmacSha256);
        var token = new JwtSecurityToken(
            _options.Issuer,
            _options.Audience,
            [new Claim(JwtRegisteredClaimNames.Sub, username)],
            expires: DateTime.UtcNow.AddHours(8),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}

public sealed record LoginRequest(string Username, string Password);
public sealed record TokenResponse(string AccessToken, string TokenType, int ExpiresInSeconds);
