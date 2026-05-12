using KyInfo.Contracts.ExamScores;

namespace KyInfo.Application.Services.ExamScores;

public interface IExamScoreAppService
{
    Task<List<ExamScoreListItemDto>> SearchAsync(
        int? userId,
        int? year,
        int? schoolId,
        int? majorId,
        CancellationToken cancellationToken);

    Task<ExamScoreDetailDto> GetByIdAsync(int id, CancellationToken cancellationToken);

    Task<int> CreateAsync(ExamScoreCreateDto dto, CancellationToken cancellationToken);
}

