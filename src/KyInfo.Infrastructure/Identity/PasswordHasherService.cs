using KyInfo.Application.Abstractions.Identity;

namespace KyInfo.Infrastructure.Identity;

public class PasswordHasherService : IPasswordHasher
{
    public bool IsLegacySha256OnlyHash(string? storedHash)
    {
        return PasswordHasher.IsLegacySha256OnlyHash(storedHash);
    }

    public string HashPassword(string password)
    {
        return PasswordHasher.HashPassword(password);
    }

    public bool VerifyPassword(string password, string hashedPassword)
    {
        return PasswordHasher.VerifyPassword(password, hashedPassword);
    }
}

