using KyInfo.Contracts.Majors;

namespace KyInfo.Application.Services.Majors;

public interface IMajorAppService
{
    Task<List<MajorListItemDto>> SearchAsync(string? keyword, int? schoolId, CancellationToken cancellationToken);

    Task<MajorDetailDto> GetByIdAsync(int id, CancellationToken cancellationToken);

    Task<int> CreateAsync(MajorDetailDto dto, CancellationToken cancellationToken);
}

