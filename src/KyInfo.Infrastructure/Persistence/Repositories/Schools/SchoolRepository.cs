using KyInfo.Application.Abstractions.Repositories;
using KyInfo.Domain.Entities;
using KyInfo.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace KyInfo.Infrastructure.Persistence.Repositories.Schools;

public class SchoolRepository : ISchoolRepository
{
    private readonly AppDbContext _db;

    public SchoolRepository(AppDbContext db)
    {
        _db = db;
    }

    public async Task<List<School>> SearchAsync(
        string? keyword,
        string? province,
        string? levelTag,
        CancellationToken cancellationToken)
    {
        var query = _db.Schools.AsQueryable();

        if (!string.IsNullOrWhiteSpace(keyword))
        {
            query = query.Where(s =>
                s.Name.Contains(keyword) ||
                (s.ShortName != null && s.ShortName.Contains(keyword)));
        }

        if (!string.IsNullOrWhiteSpace(province))
        {
            query = query.Where(s => s.Province == province);
        }

        if (!string.IsNullOrWhiteSpace(levelTag))
        {
            query = query.Where(s => s.LevelTag == levelTag);
        }

        return await query
            .OrderBy(s => s.Province)
            .ThenBy(s => s.Name)
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }

    public async Task<School?> GetByIdAsync(int id, CancellationToken cancellationToken)
    {
        return await _db.Schools.AsNoTracking().FirstOrDefaultAsync(s => s.Id == id, cancellationToken);
    }

    public async Task<int> CreateAsync(School school, CancellationToken cancellationToken)
    {
        _db.Schools.Add(school);
        await _db.SaveChangesAsync(cancellationToken);
        return school.Id;
    }
}

