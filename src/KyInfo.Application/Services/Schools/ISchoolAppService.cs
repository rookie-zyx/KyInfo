using KyInfo.Contracts.Schools;

namespace KyInfo.Application.Services.Schools;

public interface ISchoolAppService
{
    Task<List<SchoolListItemDto>> SearchAsync(string? keyword, string? province, string? levelTag, CancellationToken cancellationToken);

    Task<SchoolDetailDto> GetByIdAsync(int id, CancellationToken cancellationToken);

    Task<int> CreateAsync(SchoolDetailDto dto, CancellationToken cancellationToken);
}

