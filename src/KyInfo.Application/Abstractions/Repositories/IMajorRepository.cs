using KyInfo.Domain.Entities;

namespace KyInfo.Application.Abstractions.Repositories;

public interface IMajorRepository
{
    Task<List<Major>> SearchAsync(string? keyword, int? schoolId, CancellationToken cancellationToken);

    Task<Major?> GetByIdAsync(int id, CancellationToken cancellationToken);

    Task<bool> SchoolExistsAsync(int schoolId, CancellationToken cancellationToken);

    Task<int> CreateAsync(Major entity, CancellationToken cancellationToken);
}

