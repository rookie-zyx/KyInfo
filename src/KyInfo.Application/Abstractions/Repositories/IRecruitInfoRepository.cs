using KyInfo.Domain.Entities;

namespace KyInfo.Application.Abstractions.Repositories;

public interface IRecruitInfoRepository
{
    Task<List<RecruitInfo>> SearchAsync(int? schoolId, int? majorId, int? year, CancellationToken cancellationToken);

    Task<RecruitInfo?> GetByIdAsync(int id, CancellationToken cancellationToken);

    Task<bool> SchoolExistsAsync(int schoolId, CancellationToken cancellationToken);

    Task<bool> MajorExistsAsync(int majorId, CancellationToken cancellationToken);

    Task<bool> ExistsAsync(int year, int schoolId, int majorId, CancellationToken cancellationToken);

    Task<int> CreateAsync(RecruitInfo entity, CancellationToken cancellationToken);
}

