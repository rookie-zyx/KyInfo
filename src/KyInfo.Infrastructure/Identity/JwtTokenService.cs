using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using KyInfo.Application.Abstractions.Identity;
using KyInfo.Domain.Entities;
using KyInfo.Infrastructure.Options;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Extensions.Options;

namespace KyInfo.Infrastructure.Identity;

public class JwtTokenService : IJwtTokenService
{
    private readonly JwtOptions _options;

    public JwtTokenService(IOptions<JwtOptions> options)
    {
        _options = options.Value;
    }

    public string GenerateToken(User user)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_options.Key));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        // 使用短名称 "role"，与 JwtBearer 校验时的 RoleClaimType = "role" 一致（见 Api Program.cs）
        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.UniqueName, user.UserName),
            new Claim("role", user.Role.ToString())
        };

        var token = new JwtSecurityToken(
            issuer: _options.Issuer,
            audience: _options.Audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(_options.ExpireMinutes),
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
