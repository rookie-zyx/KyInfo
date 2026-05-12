using KyInfo.Application.Abstractions.Repositories;
using KyInfo.Application.Common;
using KyInfo.Contracts.ExamScores;
using KyInfo.Domain.Entities;

namespace KyInfo.Application.Services.ExamScores;

public class ExamScoreAppService : IExamScoreAppService
{
    private readonly IExamScoreRepository _examScoreRepository;

    public ExamScoreAppService(IExamScoreRepository examScoreRepository)
    {
        _examScoreRepository = examScoreRepository;
    }

    public async Task<List<ExamScoreListItemDto>> SearchAsync(
        int? userId,
        int? year,
        int? schoolId,
        int? majorId,
        CancellationToken cancellationToken)
    {
        var entities = await _examScoreRepository
            .SearchAsync(userId, year, schoolId, majorId, cancellationToken);

        return entities
            .Select(x => new ExamScoreListItemDto
            {
                Id = x.Id,
                Year = x.Year,
                TotalScore = x.TotalScore,
                UserId = x.UserId,
                UserName = x.User.UserName,
                SchoolId = x.SchoolId,
                SchoolName = x.School?.Name,
                MajorId = x.MajorId,
                MajorName = x.Major?.Name
            })
            .ToList();
    }

    public async Task<ExamScoreDetailDto> GetByIdAsync(int id, CancellationToken cancellationToken)
    {
        var x = await _examScoreRepository.GetByIdAsync(id, cancellationToken);
        if (x is null)
        {
            throw new NotFoundException("未找到该成绩记录");
        }

        return new ExamScoreDetailDto
        {
            Id = x.Id,
            Year = x.Year,
            TotalScore = x.TotalScore,
            UserId = x.UserId,
            UserName = x.User.UserName,
            SchoolId = x.SchoolId,
            SchoolName = x.School?.Name,
            MajorId = x.MajorId,
            MajorName = x.Major?.Name,
            PoliticsScore = x.PoliticsScore,
            EnglishScore = x.EnglishScore,
            MathScore = x.MathScore,
            MajorSubjectScore = x.MajorSubjectScore
        };
    }

    public async Task<int> CreateAsync(ExamScoreCreateDto dto, CancellationToken cancellationToken)
    {
        if (dto is null)
        {
            throw new ArgumentNullException(nameof(dto));
        }

        if (dto.Year <= 0)
        {
            throw new ArgumentException("Year 不合法", nameof(dto.Year));
        }

        if (dto.TotalScore <= 0)
        {
            throw new ArgumentException("TotalScore 不合法", nameof(dto.TotalScore));
        }

        if (!await _examScoreRepository.UserExistsAsync(dto.UserId, cancellationToken))
        {
            throw new ArgumentException("用户不存在", nameof(dto.UserId));
        }

        if (dto.SchoolId.HasValue &&
            !await _examScoreRepository.SchoolExistsAsync(dto.SchoolId.Value, cancellationToken))
        {
            throw new ArgumentException("学校不存在", nameof(dto.SchoolId));
        }

        if (dto.MajorId.HasValue &&
            !await _examScoreRepository.MajorExistsAsync(dto.MajorId.Value, cancellationToken))
        {
            throw new ArgumentException("专业不存在", nameof(dto.MajorId));
        }

        if (await _examScoreRepository.ExistsDuplicateAsync(dto.UserId, dto.Year, dto.SchoolId, dto.MajorId, cancellationToken))
        {
            throw new ArgumentException("该用户在该年份该院校该专业的成绩记录已存在");
        }

        var entity = new ExamScore
        {
            Year = dto.Year,
            TotalScore = dto.TotalScore,
            PoliticsScore = dto.PoliticsScore,
            EnglishScore = dto.EnglishScore,
            MathScore = dto.MathScore,
            MajorSubjectScore = dto.MajorSubjectScore,
            UserId = dto.UserId,
            SchoolId = dto.SchoolId,
            MajorId = dto.MajorId
        };

        return await _examScoreRepository.AddAsync(entity, cancellationToken);
    }
}

