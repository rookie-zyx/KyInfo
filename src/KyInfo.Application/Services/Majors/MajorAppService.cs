using KyInfo.Application.Abstractions.Repositories;
using KyInfo.Application.Common;
using KyInfo.Contracts.Majors;
using KyInfo.Domain.Entities;

namespace KyInfo.Application.Services.Majors;

public class MajorAppService : IMajorAppService
{
    private readonly IMajorRepository _majorRepository;

    public MajorAppService(IMajorRepository majorRepository)
    {
        _majorRepository = majorRepository;
    }

    public async Task<List<MajorListItemDto>> SearchAsync(string? keyword, int? schoolId, CancellationToken cancellationToken)
    {
        var majors = await _majorRepository.SearchAsync(keyword, schoolId, cancellationToken);
        return majors.Select(m => new MajorListItemDto
        {
            Id = m.Id,
            Name = m.Name,
            Code = m.Code,
            DisciplineCategory = m.DisciplineCategory,
            DegreeType = m.DegreeType,
            StudyType = m.StudyType,
            DurationYears = m.DurationYears,
            SchoolId = m.SchoolId,
            SchoolName = m.School.Name
        }).ToList();
    }

    public async Task<MajorDetailDto> GetByIdAsync(int id, CancellationToken cancellationToken)
    {
        var m = await _majorRepository.GetByIdAsync(id, cancellationToken);
        if (m is null)
        {
            throw new NotFoundException("未找到该专业");
        }

        return new MajorDetailDto
        {
            Id = m.Id,
            Name = m.Name,
            Code = m.Code,
            DisciplineCategory = m.DisciplineCategory,
            DegreeType = m.DegreeType,
            StudyType = m.StudyType,
            DurationYears = m.DurationYears,
            SchoolId = m.SchoolId,
            SchoolName = m.School.Name,
            TuitionPerYear = m.TuitionPerYear,
            SchoolDepartment = m.SchoolDepartment,
            Description = m.Description
        };
    }

    public async Task<int> CreateAsync(MajorDetailDto dto, CancellationToken cancellationToken)
    {
        if (dto is null) throw new ArgumentNullException(nameof(dto));
        if (string.IsNullOrWhiteSpace(dto.Name)) throw new ArgumentException("专业名称必填");
        if (string.IsNullOrWhiteSpace(dto.Code)) throw new ArgumentException("专业代码必填");

        if (!await _majorRepository.SchoolExistsAsync(dto.SchoolId, cancellationToken))
        {
            throw new ArgumentException("学校不存在", nameof(dto.SchoolId));
        }

        var entity = new Major
        {
            Name = dto.Name.Trim(),
            Code = dto.Code.Trim(),
            DisciplineCategory = dto.DisciplineCategory.Trim(),
            DegreeType = dto.DegreeType.Trim(),
            StudyType = dto.StudyType.Trim(),
            DurationYears = dto.DurationYears,
            TuitionPerYear = dto.TuitionPerYear,
            SchoolDepartment = dto.SchoolDepartment?.Trim(),
            Description = dto.Description?.Trim(),
            SchoolId = dto.SchoolId
        };

        return await _majorRepository.CreateAsync(entity, cancellationToken);
    }
}

