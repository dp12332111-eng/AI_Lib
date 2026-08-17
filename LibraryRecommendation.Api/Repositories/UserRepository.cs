using LibraryRecommendation.Api.Data;
using LibraryRecommendation.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace LibraryRecommendation.Api.Repositories;

public class UserRepository : IUserRepository
{
    private readonly LibraryDbContext _context;

    public UserRepository(LibraryDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<User>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Users
            .AsNoTracking()
            .OrderBy(u => u.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<User?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _context.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == id, cancellationToken);
    }

    public async Task<User?> GetWithHistoryAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _context.Users
            .AsNoTracking()
            .Include(u => u.Ratings)
            .Include(u => u.ReadingHistory)
            .FirstOrDefaultAsync(u => u.Id == id, cancellationToken);
    }

    public async Task<IEnumerable<Rating>> GetRatingsAsync(int userId, CancellationToken cancellationToken = default)
    {
        return await _context.Ratings
            .AsNoTracking()
            .Where(r => r.UserId == userId)
            .OrderByDescending(r => r.Stars)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<ReadingHistory>> GetReadingHistoryAsync(
        int userId,
        CancellationToken cancellationToken = default)
    {
        return await _context.ReadingHistory
            .AsNoTracking()
            .Where(h => h.UserId == userId)
            .ToListAsync(cancellationToken);
    }
}
