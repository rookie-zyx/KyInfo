namespace KyInfo.Application.Abstractions.Identity;

public interface IPasswordHasher
{
    bool IsLegacySha256OnlyHash(string? storedHash);

    string HashPassword(string password);

    bool VerifyPassword(string password, string hashedPassword);
}

