using System.Security.Cryptography;
using System.Text;

namespace KyInfo.Infrastructure.Identity;

public static class PasswordHasher
{
    private const int SaltSize = 16; // 128 bits
    private const int HashSize = 32; // 256 bits
    private const int Iterations = 100000; // 工作因子

    /// <summary>
    /// 当前库中若仍为「仅 SHA256(明文密码)」且 Base64 存 32 字节，则为旧格式，登录成功后应升级为 PBKDF2。
    /// </summary>
    public static bool IsLegacySha256OnlyHash(string? storedHash)
    {
        if (string.IsNullOrEmpty(storedHash))
        {
            return false;
        }

        try
        {
            var bytes = Convert.FromBase64String(storedHash);
            return bytes.Length == HashSize;
        }
        catch (FormatException)
        {
            return false;
        }
    }

    public static string HashPassword(string password)
    {
        // 生成随机盐值
        byte[] salt = new byte[SaltSize];
        using (var rng = RandomNumberGenerator.Create())
        {
            rng.GetBytes(salt);
        }

        // 使用PBKDF2进行密钥拉伸
        using var pbkdf2 = new Rfc2898DeriveBytes(
            password,
            salt,
            Iterations,
            HashAlgorithmName.SHA256);

        byte[] hash = pbkdf2.GetBytes(HashSize);

        // 组合盐值和哈希值
        byte[] hashBytes = new byte[SaltSize + HashSize];
        Array.Copy(salt, 0, hashBytes, 0, SaltSize);
        Array.Copy(hash, 0, hashBytes, SaltSize, HashSize);

        return Convert.ToBase64String(hashBytes);
    }

    public static bool VerifyPassword(string password, string hashedPassword)
    {
        byte[] hashBytes;
        try
        {
            hashBytes = Convert.FromBase64String(hashedPassword);
        }
        catch (FormatException)
        {
            return false;
        }

        // 新版：盐(16) + PBKDF2 输出(32) = 48 字节
        if (hashBytes.Length == SaltSize + HashSize)
        {
            var salt = new byte[SaltSize];
            Array.Copy(hashBytes, 0, salt, 0, SaltSize);

            using var pbkdf2 = new Rfc2898DeriveBytes(
                password,
                salt,
                Iterations,
                HashAlgorithmName.SHA256);

            var hash = pbkdf2.GetBytes(HashSize);

            for (var i = 0; i < HashSize; i++)
            {
                if (hashBytes[i + SaltSize] != hash[i])
                {
                    return false;
                }
            }

            return true;
        }

        // 旧版（项目注释中曾用的示例）：SHA256(UTF8 密码) 直接 Base64，共 32 字节
        if (hashBytes.Length == HashSize)
        {
            var candidate = SHA256.HashData(Encoding.UTF8.GetBytes(password));
            return CryptographicOperations.FixedTimeEquals(hashBytes, candidate);
        }

        return false;
    }
}

