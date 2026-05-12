using KyInfo.Domain.Entities;
using KyInfo.Application.Common;

namespace KyInfo.Application.Abstractions.Repositories;

public interface IScoreLineRepository
{
    Task<List<ScoreLine>> GetProfessionalScoreLinesAsync(int year, CancellationToken cancellationToken);

    Task<List<ScoreLine>> SearchAsync(
        int? schoolId,
        int? majorId,
        int? year,
        bool? isNational,
        CancellationToken cancellationToken);

    Task<ScoreLine?> GetByIdAsync(int id, CancellationToken cancellationToken);

    Task<bool> SchoolExistsAsync(int schoolId, CancellationToken cancellationToken);

    Task<bool> MajorExistsAsync(int majorId, CancellationToken cancellationToken);

    Task<int> CreateAsync(ScoreLine entity, CancellationToken cancellationToken);

    Task<List<ScoreLineTrendPoint>> GetTrendAsync(
        int? schoolId,
        int? majorId,
        bool? isNational,
        CancellationToken cancellationToken);
}

