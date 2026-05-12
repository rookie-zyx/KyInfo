using KyInfo.Domain.Entities;

namespace KyInfo.Application.Abstractions.Identity;

public interface IJwtTokenService
{
    string GenerateToken(User user);
}

