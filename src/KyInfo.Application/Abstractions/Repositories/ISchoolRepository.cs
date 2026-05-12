using KyInfo.Domain.Entities;

namespace KyInfo.Application.Abstractions.Repositories;

public interface ISchoolRepository
{
    Task<List<School>> SearchAsync(string? keyword, string? province, string? levelTag, CancellationToken cancellationToken);

    Task<School?> GetByIdAsync(int id, CancellationToken cancellationToken);

    Task<int> CreateAsync(School school, CancellationToken cancellationToken);
}

