using KyInfo.Contracts.ScoreLines;

namespace KyInfo.Application.Services.ScoreLines;

public interface IScoreLineAppService
{
    Task<List<ScoreLineListItemDto>> SearchAsync(
        int? schoolId,
        int? majorId,
        int? year,
        bool? isNational,
        CancellationToken cancellationToken);

    Task<List<ScoreLineTrendPointDto>> GetTrendAsync(
        int? schoolId,
        int? majorId,
        bool? isNational,
        CancellationToken cancellationToken);

    Task<ScoreLineDetailDto> GetByIdAsync(int id, CancellationToken cancellationToken);

    Task<int> CreateAsync(ScoreLineCreateDto dto, CancellationToken cancellationToken);
}

