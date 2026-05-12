using KyInfo.Contracts.RecruitInfos;

namespace KyInfo.Application.Services.RecruitInfos;

public interface IRecruitInfoAppService
{
    Task<List<RecruitInfoListItemDto>> SearchAsync(int? schoolId, int? majorId, int? year, CancellationToken cancellationToken);

    Task<RecruitInfoDetailDto> GetByIdAsync(int id, CancellationToken cancellationToken);

    Task<int> CreateAsync(RecruitInfoCreateDto dto, CancellationToken cancellationToken);
}

