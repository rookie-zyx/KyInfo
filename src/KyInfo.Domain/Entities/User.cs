using KyInfo.Domain.Enums;

namespace KyInfo.Domain.Entities;

public class User
{
    public int Id { get; set; }

    public string UserName { get; set; } = default!;

    public string Email { get; set; } = default!;

    public string PasswordHash { get; set; } = default!;

    /// <summary>
    /// 用户角色，用于授权判断。
    /// </summary>
    public UserRole Role { get; set; } = UserRole.User;

    /// <summary>
    /// 账号创建时间（UTC）。
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

