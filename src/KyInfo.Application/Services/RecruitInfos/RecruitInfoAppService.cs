using KyInfo.Application.Abstractions.Repositories;
using KyInfo.Application.Common;
using KyInfo.Contracts.RecruitInfos;
using KyInfo.Domain.Entities;

namespace KyInfo.Application.Services.RecruitInfos;

public class RecruitInfoAppService : IRecruitInfoAppService
{
    private readonly IRecruitInfoRepository _recruitInfoRepository;

    public RecruitInfoAppService(IRecruitInfoRepository recruitInfoRepository)
    {
        _recruitInfoRepository = recruitInfoRepository;
    }

    public async Task<List<RecruitInfoListItemDto>> SearchAsync(
        int? schoolId,
        int? majorId,
        int? year,
        CancellationToken cancellationToken)
    {
        var rows = await _recruitInfoRepository.SearchAsync(schoolId, majorId, year, cancellationToken);

        return rows.Select(x => new RecruitInfoListItemDto
        {
            Id = x.Id,
            Year = x.Year,
            SchoolId = x.SchoolId,
            SchoolName = x.School.Name,
            MajorId = x.MajorId,
            MajorName = x.Major.Name,
            PlanCount = x.PlanCount
        }).ToList();
    }

    public async Task<RecruitInfoDetailDto> GetByIdAsync(int id, CancellationToken cancellationToken)
    {
        var x = await _recruitInfoRepository.GetByIdAsync(id, cancellationToken);
        if (x is null)
        {
            throw new NotFoundException("未找到该招生信息");
        }

        return new RecruitInfoDetailDto
        {
            Id = x.Id,
            Year = x.Year,
            SchoolId = x.SchoolId,
            SchoolName = x.School.Name,
            MajorId = x.MajorId,
            MajorName = x.Major.Name,
            PlanCount = x.PlanCount,
            ExamSubjects = x.ExamSubjects,
            ExtraRequirements = x.ExtraRequirements,
            SourceUrl = x.SourceUrl,
            PublishedAt = x.PublishedAt
        };
    }

    public async Task<int> CreateAsync(RecruitInfoCreateDto dto, CancellationToken cancellationToken)
    {
        if (dto is null)
        {
            throw new ArgumentNullException(nameof(dto));
        }

        if (dto.Year <= 0)
        {
            throw new ArgumentException("Year 不合法", nameof(dto.Year));
        }

        if (!await _recruitInfoRepository.SchoolExistsAsync(dto.SchoolId, cancellationToken))
        {
            throw new ArgumentException("学校不存在", nameof(dto.SchoolId));
        }

        if (!await _recruitInfoRepository.MajorExistsAsync(dto.MajorId, cancellationToken))
        {
            throw new ArgumentException("专业不存在", nameof(dto.MajorId));
        }

        if (await _recruitInfoRepository.ExistsAsync(dto.Year, dto.SchoolId, dto.MajorId, cancellationToken))
        {
            throw new ArgumentException("该年份该院校该专业的招生信息已存在");
        }

        var entity = new RecruitInfo
        {
            Year = dto.Year,
            SchoolId = dto.SchoolId,
            MajorId = dto.MajorId,
            PlanCount = dto.PlanCount,
            ExamSubjects = dto.ExamSubjects,
            ExtraRequirements = dto.ExtraRequirements,
            SourceUrl = dto.SourceUrl,
            PublishedAt = dto.PublishedAt
        };

        return await _recruitInfoRepository.CreateAsync(entity, cancellationToken);
    }
}

