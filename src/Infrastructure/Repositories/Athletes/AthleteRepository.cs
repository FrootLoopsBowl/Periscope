using Domain.Common;
using Domain.Entities;
using Domain.Repositories;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Infrastructure.Repositories.Athletes;

public class AthleteRepository : IAthleteRepository
{
    private readonly GarneauTemplateDbContext _context;

    public AthleteRepository(GarneauTemplateDbContext context)
    {
        _context = context;
    }

    public async Task CreateAsync(Athlete athlete)
    {
        _context.Athletes.Add(athlete);
        await _context.SaveChangesAsync();
    }

    public async Task<bool> ExistsByEmailAsync(string email)
    {
        return await _context.Athletes
            .AsNoTracking()
            .AnyAsync(x => x.Email == email);
    }

    public async Task<Athlete?> FindBySubmissionTokenAsync(Guid token)
    {
        return await _context.Athletes
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.SubmissionToken == token && x.Active);
    }

    public async Task<Athlete?> FindByIdAsync(Guid id)
    {
        return await _context.Athletes
            .Include(x => x.Team)
            .FirstOrDefaultAsync(x => x.Id == id);
    }

    public async Task UpdateAsync(Athlete athlete)
    {
        _context.Athletes.Update(athlete);
        await _context.SaveChangesAsync();
    }

    public PaginatedList<Athlete> GetAllPaginated(int pageIndex, int pageSize)
    {
        var total = _context.Athletes.Count(x => x.Active);
        var items = _context.Athletes
            .AsNoTracking()
            .Include(x => x.Team)
            .Where(x => x.Active)
            .OrderBy(x => x.LastName)
            .Skip((pageIndex - 1) * pageSize)
            .Take(pageSize)
            .ToList();
        return new PaginatedList<Athlete>(items, total);
    }

    public async Task<IEnumerable<Athlete>> GetAllAsync()
    {
        return await _context.Athletes
            .AsNoTracking()
            .Include(x => x.Team)
            .Where(x => x.Active)
            .OrderBy(x => x.LastName)
            .ToListAsync();
    }

    public async Task DeleteAsync(Athlete athlete)
    {
        _context.Athletes.Remove(athlete);
        await _context.SaveChangesAsync();
    }
}
