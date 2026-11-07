using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using VibeVoice.Application.Abstractions;
using VibeVoice.Domain.Entities;

namespace VibeVoice.Infrastructure.Auth;

public sealed class JwtOptions
{
    public const string Section = "Jwt";

    public string Issuer { get; set; } = "VibeVoice";
    public string Audience { get; set; } = "VibeVoice";

    /// <summary>HMAC-SHA256 signing key, at least 32 characters. Set via <c>Jwt__Key</c> outside Development.</summary>
    public string Key { get; set; } = "";

    public int ExpiryHours { get; set; } = 24 * 7;

    public SymmetricSecurityKey SigningKey()
    {
        if (Key.Length < 32)
            throw new InvalidOperationException("Jwt:Key must be at least 32 characters. Set the Jwt__Key environment variable.");
        return new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Key));
    }
}

public sealed class JwtTokenService(JwtOptions options, TimeProvider clock) : ITokenService
{
    /// <summary>The role claim is informational; permissions are checked against the database on every request.</summary>
    public (string Token, DateTime ExpiresAt) CreateToken(User user, string roleName)
    {
        var now = clock.GetUtcNow().UtcDateTime;
        var expires = now.AddHours(options.ExpiryHours);
        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, user.Email),
            new Claim(JwtRegisteredClaimNames.Name, user.DisplayName),
            new Claim(ClaimTypes.Role, roleName),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString("N")),
        };
        var token = new JwtSecurityToken(
            options.Issuer, options.Audience, claims, now, expires,
            new SigningCredentials(options.SigningKey(), SecurityAlgorithms.HmacSha256));
        return (new JwtSecurityTokenHandler().WriteToken(token), expires);
    }
}
