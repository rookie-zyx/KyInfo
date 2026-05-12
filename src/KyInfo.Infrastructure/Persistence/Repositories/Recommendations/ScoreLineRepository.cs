using KyInfo.Application.Abstractions.Repositories;
using KyInfo.Application.Common;
using KyInfo.Domain.Entities;
using KyInfo.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace KyInfo.Infrastructure.Persistence.Repositories.Recommendations;

public class ScoreLineRepository : IScoreLineRepository
{
    private readonly AppDbContext _db;

    public ScoreLineRepository(AppDbContext db)
    {
        _db = db;
    }

    public async Task<List<ScoreLine>> GetProfessionalScoreLinesAsync(
        int year,
        CancellationToken cancellationToken)
    {
        return await _db.ScoreLines
            .AsNoTracking()
            .Include(x => x.Major)
            .ThenInclude(m => m!.School)
            .Where(x => x.Year == year && !x.IsNational && x.MajorId != null)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<ScoreLine>> SearchAsync(
        int? schoolId,
        int? majorId,
        int? year,
        bool? isNational,
        CancellationToken cancellationToken)
    {
        var query = _db.ScoreLines
            .AsNoTracking()
            .Include(x => x.School)
            .Include(x => x.Major)
            .AsQueryable();

        if (isNational.HasValue)
        {
            query = query.Where(x => x.IsNational == isNational.Value);
        }

        if (year.HasValue)
        {
            query = query.Where(x => x.Year == year.Value);
        }

        if (schoolId.HasValue)
        {
            query = query.Where(x => x.SchoolId == schoolId.Value);
        }

        if (majorId.HasValue)
        {
            query = query.Where(x => x.MajorId == majorId.Value);
        }

        return await query
            .OrderByDescending(x => x.Year)
            .ThenByDescending(x => x.IsNational)
            .ThenBy(x => x.School != null ? x.School.Name : string.Empty)
            .ThenBy(x => x.Major != null ? x.Major.Name : string.Empty)
            .ToListAsync(cancellationToken);
    }

    public async Task<ScoreLine?> GetByIdAsync(int id, CancellationToken cancellationToken)
    {
        return await _db.ScoreLines
            .AsNoTracking()
            .Include(s => s.School)
            .Include(m => m.Major)
            .FirstOrDefaultAsync(v => v.Id == id, cancellationToken);
    }

    public Task<bool> SchoolExistsAsync(int schoolId, CancellationToken cancellationToken)
    {
        return _db.Schools.AnyAsync(s => s.Id == schoolId, cancellationToken);
    }

    public Task<bool> MajorExistsAsync(int majorId, CancellationToken cancellationToken)
    {
        return _db.Majors.AnyAsync(m => m.Id == majorId, cancellationToken);
    }

    public async Task<int> CreateAsync(ScoreLine entity, CancellationToken cancellationToken)
    {
        _db.ScoreLines.Add(entity);
        await _db.SaveChangesAsync(cancellationToken);
        return entity.Id;
    }

    public async Task<List<ScoreLineTrendPoint>> GetTrendAsync(
        int? schoolId,
        int? majorId,
        bool? isNational,
        CancellationToken cancellationToken)
    {
        var query = _db.ScoreLines.AsNoTracking().AsQueryable();

        if (isNational.HasValue)
        {
            query = query.Where(x => x.IsNational == isNational.Value);
        }

        if (schoolId.HasValue)
        {
            query = query.Where(x => x.SchoolId == schoolId.Value);
        }

        if (majorId.HasValue)
        {
            query = query.Where(x => x.MajorId == majorId.Value);
        }

        // 先拉取最小列再在内存聚合，避免不同 SQL 方言/版本对 AVG+ROUND 的翻译差异导致 500。
        var rows = await query
            .Select(x => new { x.Year, x.Score })
            .ToListAsync(cancellationToken);

        return rows
            .GroupBy(x => x.Year)
            .Select(g => new ScoreLineTrendPoint(
                g.Key,
                (int)Math.Round(g.Average(x => x.Score))))
            .OrderBy(x => x.Year)
            .ToList();
    }
}

