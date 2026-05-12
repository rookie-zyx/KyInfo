using KyInfo.Application.Abstractions.Repositories;
using KyInfo.Domain.Entities;
using KyInfo.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace KyInfo.Infrastructure.Persistence.Repositories.Auth;

public class UserRepository : IUserRepository
{
    private readonly AppDbContext _db;

    public UserRepository(AppDbContext db)
    {
        _db = db;
    }

    public async Task<bool> ExistsByUsernameOrEmailAsync(
        string userName,
        string email,
        CancellationToken cancellationToken)
    {
        return await _db.Users.AnyAsync(
            u => u.UserName == userName || u.Email == email,
            cancellationToken);
    }

    public async Task<User?> GetByUsernameOrEmailAsync(
        string userNameOrEmail,
        CancellationToken cancellationToken)
    {
        return await _db.Users
            .FirstOrDefaultAsync(
                u => u.UserName == userNameOrEmail || u.Email == userNameOrEmail,
                cancellationToken);
    }

    public Task<User?> GetByIdAsync(int id, CancellationToken cancellationToken)
    {
        return _db.Users.FirstOrDefaultAsync(u => u.Id == id, cancellationToken);
    }

    public Task<bool> IsUserNameTakenAsync(int userId, string userName, CancellationToken cancellationToken)
    {
        return _db.Users.AnyAsync(u => u.Id != userId && u.UserName == userName, cancellationToken);
    }

    public Task<bool> IsEmailTakenAsync(int userId, string email, CancellationToken cancellationToken)
    {
        return _db.Users.AnyAsync(u => u.Id != userId && u.Email == email, cancellationToken);
    }

    public async Task AddAsync(User user, CancellationToken cancellationToken)
    {
        _db.Users.Add(user);
        await _db.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(User user, CancellationToken cancellationToken)
    {
        _db.Users.Update(user);
        await _db.SaveChangesAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<User>> GetAllAsync(CancellationToken cancellationToken)
    {
        var list = await _db.Users
            .OrderBy(u => u.Id)
            .ToListAsync(cancellationToken);
        return list;
    }

    public async Task DeleteAsync(int id, CancellationToken cancellationToken)
    {
        var user = await _db.Users.FirstOrDefaultAsync(u => u.Id == id, cancellationToken);
        if (user is null)
        {
            return;
        }

        _db.Users.Remove(user);
        await _db.SaveChangesAsync(cancellationToken);
    }
}

