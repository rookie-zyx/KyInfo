using KyInfo.Application.Abstractions.Repositories;
using KyInfo.Domain.Entities;
using KyInfo.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace KyInfo.Infrastructure.Persistence.Repositories.Recommendations;

public class ExamScoreRepository : IExamScoreRepository
{
    private readonly AppDbContext _db;

    public ExamScoreRepository(AppDbContext db)
    {
        _db = db;
    }

    public async Task<ExamScore?> GetLatestByUserIdAsync(
        int userId,
        int? year,
        CancellationToken cancellationToken)
    {
        var query = _db.ExamScores
            .AsNoTracking()
            .Include(x => x.User)
            .Where(x => x.UserId == userId);

        if (year.HasValue)
        {
            query = query.Where(x => x.Year == year.Value);
        }

        return await query
            .OrderByDescending(x => x.Year)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<List<ExamScore>> SearchAsync(
        int? userId,
        int? year,
        int? schoolId,
        int? majorId,
        CancellationToken cancellationToken)
    {
        var query = _db.ExamScores
            .AsNoTracking()
            .Include(x => x.User)
            .Include(x => x.School)
            .Include(x => x.Major)
            .AsQueryable();

        if (userId.HasValue)
        {
            query = query.Where(x => x.UserId == userId.Value);
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
            .ThenByDescending(x => x.TotalScore)
            .ToListAsync(cancellationToken);
    }

    public async Task<ExamScore?> GetByIdAsync(int id, CancellationToken cancellationToken)
    {
        return await _db.ExamScores
            .AsNoTracking()
            .Include(e => e.User)
            .Include(e => e.School)
            .Include(e => e.Major)
            .FirstOrDefaultAsync(e => e.Id == id, cancellationToken);
    }

    public Task<bool> UserExistsAsync(int userId, CancellationToken cancellationToken)
    {
        return _db.Users.AnyAsync(u => u.Id == userId, cancellationToken);
    }

    public Task<bool> SchoolExistsAsync(int schoolId, CancellationToken cancellationToken)
    {
        return _db.Schools.AnyAsync(s => s.Id == schoolId, cancellationToken);
    }

    public Task<bool> MajorExistsAsync(int majorId, CancellationToken cancellationToken)
    {
        return _db.Majors.AnyAsync(m => m.Id == majorId, cancellationToken);
    }

    public Task<bool> ExistsDuplicateAsync(
        int userId,
        int year,
        int? schoolId,
        int? majorId,
        CancellationToken cancellationToken)
    {
        return _db.ExamScores.AnyAsync(
            x =>
                x.UserId == userId &&
                x.Year == year &&
                x.SchoolId == schoolId &&
                x.MajorId == majorId,
            cancellationToken);
    }

    public async Task<int> AddAsync(ExamScore entity, CancellationToken cancellationToken)
    {
        _db.ExamScores.Add(entity);
        await _db.SaveChangesAsync(cancellationToken);
        return entity.Id;
    }
}

