using KyInfo.Application.Abstractions.Repositories;
using KyInfo.Application.Common;
using KyInfo.Contracts.Schools;
using KyInfo.Domain.Entities;

namespace KyInfo.Application.Services.Schools;

public class SchoolAppService : ISchoolAppService
{
    private readonly ISchoolRepository _schoolRepository;

    public SchoolAppService(ISchoolRepository schoolRepository)
    {
        _schoolRepository = schoolRepository;
    }

    public async Task<List<SchoolListItemDto>> SearchAsync(
        string? keyword,
        string? province,
        string? levelTag,
        CancellationToken cancellationToken)
    {
        var schools = await _schoolRepository.SearchAsync(keyword, province, levelTag, cancellationToken);

        return schools
            .Select(s => new SchoolListItemDto
            {
                Id = s.Id,
                Name = s.Name,
                ShortName = s.ShortName,
                Province = s.Province,
                City = s.City,
                LevelTag = s.LevelTag,
                Type = s.Type,
                Property = s.Property
            })
            .ToList();
    }

    public async Task<SchoolDetailDto> GetByIdAsync(int id, CancellationToken cancellationToken)
    {
        var s = await _schoolRepository.GetByIdAsync(id, cancellationToken);
        if (s is null)
        {
            throw new NotFoundException("未找到该学校");
        }

        return new SchoolDetailDto
        {
            Id = s.Id,
            Name = s.Name,
            ShortName = s.ShortName,
            Province = s.Province,
            City = s.City,
            LevelTag = s.LevelTag,
            Type = s.Type,
            Property = s.Property,
            Website = s.Website
        };
    }

    public async Task<int> CreateAsync(SchoolDetailDto dto, CancellationToken cancellationToken)
    {
        if (dto is null)
        {
            throw new ArgumentNullException(nameof(dto));
        }

        if (string.IsNullOrWhiteSpace(dto.Name))
        {
            throw new ArgumentException("学校名称必填");
        }

        var entity = new School
        {
            Name = dto.Name.Trim(),
            ShortName = dto.ShortName?.Trim(),
            Province = dto.Province.Trim(),
            City = dto.City.Trim(),
            LevelTag = dto.LevelTag.Trim(),
            Type = dto.Type.Trim(),
            Property = dto.Property.Trim(),
            Website = dto.Website?.Trim()
        };

        return await _schoolRepository.CreateAsync(entity, cancellationToken);
    }
}

