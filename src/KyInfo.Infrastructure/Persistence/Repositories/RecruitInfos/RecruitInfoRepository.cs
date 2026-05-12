using KyInfo.Application.Abstractions.Repositories;
using KyInfo.Domain.Entities;
using KyInfo.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace KyInfo.Infrastructure.Persistence.Repositories.RecruitInfos;

public class RecruitInfoRepository : IRecruitInfoRepository
{
    private readonly AppDbContext _db;

    public RecruitInfoRepository(AppDbContext db)
    {
        _db = db;
    }

    public async Task<List<RecruitInfo>> SearchAsync(
        int? schoolId,
        int? majorId,
        int? year,
        CancellationToken cancellationToken)
    {
        var query = _db.RecruitInfos
            .AsNoTracking()
            .Include(x => x.School)
            .Include(x => x.Major)
            .AsQueryable();

        if (schoolId.HasValue)
        {
            query = query.Where(x => x.SchoolId == schoolId.Value);
        }

        if (majorId.HasValue)
        {
            query = query.Where(x => x.MajorId == majorId.Value);
        }

        if (year.HasValue)
        {
            query = query.Where(x => x.Year == year.Value);
        }

        return await query
            .OrderByDescending(x => x.Year)
            .ThenBy(x => x.School.Name)
            .ThenBy(x => x.Major.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<RecruitInfo?> GetByIdAsync(int id, CancellationToken cancellationToken)
    {
        return await _db.RecruitInfos
            .AsNoTracking()
            .Include(r => r.School)
            .Include(r => r.Major)
            .FirstOrDefaultAsync(r => r.Id == id, cancellationToken);
    }

    public Task<bool> SchoolExistsAsync(int schoolId, CancellationToken cancellationToken)
    {
        return _db.Schools.AnyAsync(s => s.Id == schoolId, cancellationToken);
    }

    public Task<bool> MajorExistsAsync(int majorId, CancellationToken cancellationToken)
    {
        return _db.Majors.AnyAsync(m => m.Id == majorId, cancellationToken);
    }

    public Task<bool> ExistsAsync(int year, int schoolId, int majorId, CancellationToken cancellationToken)
    {
        return _db.RecruitInfos.AnyAsync(r =>
                r.Year == year &&
                r.SchoolId == schoolId &&
                r.MajorId == majorId,
            cancellationToken);
    }

    public async Task<int> CreateAsync(RecruitInfo entity, CancellationToken cancellationToken)
    {
        _db.RecruitInfos.Add(entity);
        await _db.SaveChangesAsync(cancellationToken);
        return entity.Id;
    }
}

