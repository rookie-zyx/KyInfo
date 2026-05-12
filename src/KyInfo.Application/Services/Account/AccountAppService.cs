using KyInfo.Application.Abstractions.Identity;
using KyInfo.Application.Abstractions.Repositories;
using KyInfo.Application.Common;
using KyInfo.Contracts.Account;
using KyInfo.Domain.Enums;

namespace KyInfo.Application.Services.Account;

public class AccountAppService : IAccountAppService
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;

    public AccountAppService(IUserRepository userRepository, IPasswordHasher passwordHasher)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
    }

    public async Task<AccountProfileDto> GetMeAsync(int userId, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByIdAsync(userId, cancellationToken);
        if (user is null)
        {
            throw new NotFoundException("未找到用户");
        }

        return new AccountProfileDto
        {
            Id = user.Id,
            UserName = user.UserName,
            Email = user.Email,
            Role = user.Role.ToString(),
            CreatedAt = user.CreatedAt
        };
    }

    public async Task<AccountProfileDto> UpdateProfileAsync(
        int userId,
        UpdateAccountRequest request,
        CancellationToken cancellationToken)
    {
        if (request is null) throw new ArgumentNullException(nameof(request));

        var userName = (request.UserName ?? string.Empty).Trim();
        var email = (request.Email ?? string.Empty).Trim();

        if (userName.Length is < 2 or > 64)
        {
            throw new ArgumentException("用户名长度应在 2～64 个字符之间");
        }

        if (email.Length is < 5 or > 256 || email.IndexOf('@', StringComparison.Ordinal) <= 0)
        {
            throw new ArgumentException("邮箱格式不正确");
        }

        var user = await _userRepository.GetByIdAsync(userId, cancellationToken);
        if (user is null)
        {
            throw new NotFoundException("未找到用户");
        }

        if (user.Role == UserRole.Root)
        {
            throw new ArgumentException("Root 账号的用户名与邮箱不可修改。");
        }

        if (await _userRepository.IsUserNameTakenAsync(userId, userName, cancellationToken))
        {
            throw new ArgumentException("该用户名已被占用");
        }

        if (await _userRepository.IsEmailTakenAsync(userId, email, cancellationToken))
        {
            throw new ArgumentException("该邮箱已被注册");
        }

        user.UserName = userName;
        user.Email = email;
        await _userRepository.UpdateAsync(user, cancellationToken);

        return new AccountProfileDto
        {
            Id = user.Id,
            UserName = user.UserName,
            Email = user.Email,
            Role = user.Role.ToString(),
            CreatedAt = user.CreatedAt
        };
    }

    public async Task ChangePasswordAsync(int userId, ChangePasswordRequest request, CancellationToken cancellationToken)
    {
        if (request is null) throw new ArgumentNullException(nameof(request));

        if (string.IsNullOrWhiteSpace(request.NewPassword) || request.NewPassword.Length < 6)
        {
            throw new ArgumentException("新密码长度至少 6 位");
        }

        var user = await _userRepository.GetByIdAsync(userId, cancellationToken);
        if (user is null)
        {
            throw new NotFoundException("未找到用户");
        }

        if (!_passwordHasher.VerifyPassword(request.CurrentPassword, user.PasswordHash))
        {
            throw new ArgumentException("当前密码不正确");
        }

        user.PasswordHash = _passwordHasher.HashPassword(request.NewPassword);
        await _userRepository.UpdateAsync(user, cancellationToken);
    }
}

