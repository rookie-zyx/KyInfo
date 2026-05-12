using KyInfo.Contracts.Account;

namespace KyInfo.Application.Services.Account;

public interface IAccountAppService
{
    Task<AccountProfileDto> GetMeAsync(int userId, CancellationToken cancellationToken);

    Task<AccountProfileDto> UpdateProfileAsync(int userId, UpdateAccountRequest request, CancellationToken cancellationToken);

    Task ChangePasswordAsync(int userId, ChangePasswordRequest request, CancellationToken cancellationToken);
}

