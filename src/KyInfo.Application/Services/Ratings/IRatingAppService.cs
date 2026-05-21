using KyInfo.Contracts.Ratings;

namespace KyInfo.Application.Services.Ratings;

public interface IRatingAppService
{
    Task<RatingSubmitResultDto> SubmitAsync(int userId, RatingSubmitDto dto, CancellationToken cancellationToken);

    Task<RatingDetailDto> GetDetailAsync(
        string subjectType,
        int subjectId,
        int? userId,
        CancellationToken cancellationToken);
}
