using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using KyInfo.Application.Abstractions.Identity;
using KyInfo.Domain.Entities;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace KyInfo.Infrastructure.Identity;

public class JwtTokenService : IJwtTokenService
{
    private readonly IConfiguration _configuration;

    public JwtTokenService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public string GenerateToken(User user)
    {
        var jwtSection = _configuration.GetSection("Jwt");

        var keyString = jwtSection["Key"] ?? throw new InvalidOperationException("JWT Key 未配置");
        var issuer = jwtSection["Issuer"];
        var audience = jwtSection["Audience"];
        var expireMinutesStr = jwtSection["ExpireMinutes"] ?? "0";

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(keyString));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        // 使用短名称 "role"，与 JwtBearer 校验时的 RoleClaimType = "role" 一致（见 Api Program.cs）
        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.UniqueName, user.UserName),
            new Claim("role", user.Role.ToString())
        };

        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(int.Parse(expireMinutesStr)),
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}

