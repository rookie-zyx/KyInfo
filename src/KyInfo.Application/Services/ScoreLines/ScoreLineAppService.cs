using KyInfo.Application.Abstractions.Repositories;
using KyInfo.Application.Common;
using KyInfo.Contracts.ScoreLines;
using KyInfo.Domain.Entities;

namespace KyInfo.Application.Services.ScoreLines;

public class ScoreLineAppService : IScoreLineAppService
{
    private readonly IScoreLineRepository _scoreLineRepository;

    public ScoreLineAppService(IScoreLineRepository scoreLineRepository)
    {
        _scoreLineRepository = scoreLineRepository;
    }

    public async Task<List<ScoreLineListItemDto>> SearchAsync(
        int? schoolId,
        int? majorId,
        int? year,
        bool? isNational,
        CancellationToken cancellationToken)
    {
        var entities = await _scoreLineRepository.SearchAsync(
            schoolId,
            majorId,
            year,
            isNational,
            cancellationToken);

        return entities.Select(x => new ScoreLineListItemDto
            {
                Id = x.Id,
                Year = x.Year,
                Score = x.Score,
                IsNational = x.IsNational,
                SchoolId = x.SchoolId,
                SchoolName = x.School?.Name,
                MajorId = x.MajorId,
                MajorName = x.Major?.Name
            })
            .ToList();
    }

    public async Task<List<ScoreLineTrendPointDto>> GetTrendAsync(
        int? schoolId,
        int? majorId,
        bool? isNational,
        CancellationToken cancellationToken)
    {
        var points = await _scoreLineRepository.GetTrendAsync(
            schoolId,
            majorId,
            isNational,
            cancellationToken);

        return points.Select(p => new ScoreLineTrendPointDto
        {
            Year = p.Year,
            Score = p.Score
        }).ToList();
    }

    public async Task<ScoreLineDetailDto> GetByIdAsync(int id, CancellationToken cancellationToken)
    {
        var x = await _scoreLineRepository.GetByIdAsync(id, cancellationToken);
        if (x is null)
        {
            throw new NotFoundException("未找到该分数线");
        }

        return new ScoreLineDetailDto
        {
            Id = x.Id,
            Year = x.Year,
            Score = x.Score,
            IsNational = x.IsNational,
            SchoolId = x.SchoolId,
            SchoolName = x.School?.Name,
            MajorId = x.MajorId,
            MajorName = x.Major?.Name,
            Note = x.Note
        };
    }

    public async Task<int> CreateAsync(ScoreLineCreateDto dto, CancellationToken cancellationToken)
    {
        if (dto is null)
        {
            throw new ArgumentNullException(nameof(dto));
        }

        if (dto.Year <= 0) throw new ArgumentException("Year 不合法", nameof(dto.Year));
        if (dto.Score <= 0) throw new ArgumentException("Score 不合法", nameof(dto.Score));

        if (dto.IsNational)
        {
            if (dto.SchoolId.HasValue || dto.MajorId.HasValue)
            {
                throw new ArgumentException("国家线不应绑定学校/专业");
            }
        }
        else
        {
            if (dto.MajorId.HasValue && !await _scoreLineRepository.MajorExistsAsync(dto.MajorId.Value, cancellationToken))
            {
                throw new ArgumentException("专业不存在", nameof(dto.MajorId));
            }

            if (dto.SchoolId.HasValue && !await _scoreLineRepository.SchoolExistsAsync(dto.SchoolId.Value, cancellationToken))
            {
                throw new ArgumentException("学校不存在", nameof(dto.SchoolId));
            }
        }

        var entity = new ScoreLine
        {
            Year = dto.Year,
            Score = dto.Score,
            IsNational = dto.IsNational,
            SchoolId = dto.SchoolId,
            MajorId = dto.MajorId,
            Note = dto.Note
        };

        return await _scoreLineRepository.CreateAsync(entity, cancellationToken);
    }
}

