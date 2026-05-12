using KyInfo.Domain.Entities;

namespace KyInfo.Application.Abstractions.Repositories;

public interface IUserRepository
{
    Task<bool> ExistsByUsernameOrEmailAsync(string userName, string email, CancellationToken cancellationToken);

    Task<User?> GetByUsernameOrEmailAsync(string userNameOrEmail, CancellationToken cancellationToken);

    Task<User?> GetByIdAsync(int id, CancellationToken cancellationToken);

    Task<bool> IsUserNameTakenAsync(int userId, string userName, CancellationToken cancellationToken);

    Task<bool> IsEmailTakenAsync(int userId, string email, CancellationToken cancellationToken);

    Task AddAsync(User user, CancellationToken cancellationToken);

    Task UpdateAsync(User user, CancellationToken cancellationToken);

    Task<IReadOnlyList<User>> GetAllAsync(CancellationToken cancellationToken);

    Task DeleteAsync(int id, CancellationToken cancellationToken);
}

