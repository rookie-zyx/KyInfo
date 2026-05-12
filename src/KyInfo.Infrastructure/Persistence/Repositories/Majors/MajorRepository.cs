using KyInfo.Application.Abstractions.Repositories;
using KyInfo.Domain.Entities;
using KyInfo.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace KyInfo.Infrastructure.Persistence.Repositories.Majors;

public class MajorRepository : IMajorRepository
{
    private readonly AppDbContext _db;

    public MajorRepository(AppDbContext db)
    {
        _db = db;
    }

    public async Task<List<Major>> SearchAsync(string? keyword, int? schoolId, CancellationToken cancellationToken)
    {
        var query = _db.Majors
            .AsNoTracking()
            .Include(m => m.School)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(keyword))
        {
            query = query.Where(m => m.Name.Contains(keyword) || m.Code.Contains(keyword));
        }

        if (schoolId.HasValue)
        {
            query = query.Where(m => m.SchoolId == schoolId.Value);
        }

        return await query
            .OrderBy(m => m.School.Name)
            .ThenBy(m => m.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<Major?> GetByIdAsync(int id, CancellationToken cancellationToken)
    {
        return await _db.Majors
            .AsNoTracking()
            .Include(x => x.School)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public Task<bool> SchoolExistsAsync(int schoolId, CancellationToken cancellationToken)
    {
        return _db.Schools.AnyAsync(s => s.Id == schoolId, cancellationToken);
    }

    public async Task<int> CreateAsync(Major entity, CancellationToken cancellationToken)
    {
        _db.Majors.Add(entity);
        await _db.SaveChangesAsync(cancellationToken);
        return entity.Id;
    }
}

