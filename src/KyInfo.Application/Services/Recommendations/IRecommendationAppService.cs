using KyInfo.Contracts.Recommendations;

namespace KyInfo.Application.Services.Recommendations;

public interface IRecommendationAppService
{
    Task<RecommendationResponseDto> GetRecommendationsAsync(RecommendationRequestDto request, CancellationToken cancellationToken);
}

