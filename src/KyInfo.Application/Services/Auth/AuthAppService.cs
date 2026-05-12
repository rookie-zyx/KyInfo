using KyInfo.Application.Abstractions.Identity;
using KyInfo.Application.Abstractions.Repositories;
using KyInfo.Contracts.Auth;
using KyInfo.Domain.Entities;
using KyInfo.Domain.Enums;

namespace KyInfo.Application.Services.Auth;

public class AuthAppService : IAuthAppService
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtTokenService _jwtTokenService;

    public AuthAppService(
        IUserRepository userRepository,
        IPasswordHasher passwordHasher,
        IJwtTokenService jwtTokenService)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
        _jwtTokenService = jwtTokenService;
    }

    public async Task RegisterAsync(RegisterRequest request, CancellationToken cancellationToken)
    {
        if (request is null)
        {
            throw new ArgumentNullException(nameof(request));
        }

        if (string.IsNullOrWhiteSpace(request.UserName) ||
            string.IsNullOrWhiteSpace(request.Email) ||
            string.IsNullOrWhiteSpace(request.Password))
        {
            throw new ArgumentException("用户名/邮箱/密码不能为空。");
        }

        var exists = await _userRepository
            .ExistsByUsernameOrEmailAsync(request.UserName, request.Email, cancellationToken);
        if (exists)
        {
            throw new ArgumentException("用户名或邮箱已存在");
        }

        var user = new User
        {
            UserName = request.UserName,
            Email = request.Email,
            PasswordHash = _passwordHasher.HashPassword(request.Password),
            Role = UserRole.User
        };

        await _userRepository.AddAsync(user, cancellationToken);
    }

    public async Task<AuthResponse?> LoginAsync(LoginRequest request, CancellationToken cancellationToken)
    {
        if (request is null)
        {
            throw new ArgumentNullException(nameof(request));
        }

        if (string.IsNullOrWhiteSpace(request.UserNameOrEmail) ||
            string.IsNullOrWhiteSpace(request.Password))
        {
            return null;
        }

        var user = await _userRepository
            .GetByUsernameOrEmailAsync(request.UserNameOrEmail, cancellationToken);

        if (user is null)
        {
            return null;
        }

        if (!_passwordHasher.VerifyPassword(request.Password, user.PasswordHash))
        {
            return null;
        }

        // 若数据库里仍是旧版「仅 SHA256」哈希，验证通过后升级为 PBKDF2
        if (_passwordHasher.IsLegacySha256OnlyHash(user.PasswordHash))
        {
            user.PasswordHash = _passwordHasher.HashPassword(request.Password);
            await _userRepository.UpdateAsync(user, cancellationToken);
        }

        var token = _jwtTokenService.GenerateToken(user);
        return new AuthResponse
        {
            Token = token,
            UserName = user.UserName,
            Role = user.Role.ToString()
        };
    }
}

