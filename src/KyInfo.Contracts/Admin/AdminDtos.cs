namespace KyInfo.Contracts.Admin;

public class AdminUserListItemDto
{
    public int Id { get; set; }
    public string UserName { get; set; } = default!;
    public string Email { get; set; } = default!;
    public string Role { get; set; } = default!;
    public DateTime CreatedAt { get; set; }
}

public class AdminCreateUserRequest
{
    public string UserName { get; set; } = default!;
    public string Email { get; set; } = default!;
    public string Password { get; set; } = default!;
    /// <summary>仅支持 \"User\"（管理后台创建普通用户）。</summary>
    public string Role { get; set; } = "User";
}

/// <summary>
/// Root 创建管理员账号（角色固定为 Admin）。
/// </summary>
public class AdminCreateAdministratorRequest
{
    public string UserName { get; set; } = default!;
    public string Email { get; set; } = default!;
    public string Password { get; set; } = default!;
}

public class AdminUpdateUserRequest
{
    public string UserName { get; set; } = default!;
    public string Email { get; set; } = default!;
    /// <summary>仅支持 \"User\"（更新普通用户）。</summary>
    public string Role { get; set; } = "User";
    /// <summary>可选：不填则不修改密码。</summary>
    public string? Password { get; set; }
}

public class ExcelImportErrorDto
{
    public int RowNumber { get; set; }
    public string Message { get; set; } = default!;
}

public class ExcelImportResultDto
{
    public int SuccessCount { get; set; }
    public List<ExcelImportErrorDto> Errors { get; set; } = new();
}
