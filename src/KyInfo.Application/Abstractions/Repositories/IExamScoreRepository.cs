using KyInfo.Domain.Entities;

namespace KyInfo.Application.Abstractions.Repositories;

public interface IExamScoreRepository
{
    Task<ExamScore?> GetLatestByUserIdAsync(int userId, int? year, CancellationToken cancellationToken);

    Task<List<ExamScore>> SearchAsync(
        int? userId,
        int? year,
        int? schoolId,
        int? majorId,
        CancellationToken cancellationToken);

    Task<ExamScore?> GetByIdAsync(int id, CancellationToken cancellationToken);

    Task<bool> UserExistsAsync(int userId, CancellationToken cancellationToken);

    Task<bool> SchoolExistsAsync(int schoolId, CancellationToken cancellationToken);

    Task<bool> MajorExistsAsync(int majorId, CancellationToken cancellationToken);

    Task<bool> ExistsDuplicateAsync(
        int userId,
        int year,
        int? schoolId,
        int? majorId,
        CancellationToken cancellationToken);

    Task<int> AddAsync(ExamScore entity, CancellationToken cancellationToken);
}

