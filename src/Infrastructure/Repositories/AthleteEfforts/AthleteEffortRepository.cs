using Domain.Common;
using Domain.Entities;
using Domain.Repositories;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Infrastructure.Repositories.AthleteEfforts;

public class AthleteEffortRepository : IAthleteEffortRepository
{
    private readonly GarneauTemplateDbContext _context;

    public AthleteEffortRepository(GarneauTemplateDbContext context)
    {
        _context = context;
    }

    public async Task CreateAsync(AthleteEffort effort)
    {
        _context.Set<AthleteEffort>().Add(effort);
        await _context.SaveChangesAsync();
    }

    public PaginatedList<AthleteEffort> GetForAthletePaginated(Guid athleteId, int pageIndex, int pageSize, DateTime? startDate = null, DateTime? endDate = null)
    {
        var query = _context.Set<AthleteEffort>().Where(x => x.AthleteId == athleteId).AsNoTracking();

        if (startDate.HasValue)
            query = query.Where(e => e.CreatedAt >= startDate.Value.Date);

        if (endDate.HasValue)
        {
            var endExclusive = endDate.Value.Date.AddDays(1);
            query = query.Where(e => e.CreatedAt < endExclusive);
        }

        var ordered = query.OrderByDescending(x => x.CreatedAt);
        var total = ordered.Count();
        var items = ordered.Skip((pageIndex - 1) * pageSize).Take(pageSize).ToList();
        return new PaginatedList<AthleteEffort>(items, total);
    }

    public async Task<List<AthleteEffort>> GetEffortsSinceAsync(DateTime since)
    {
        return await _context.Set<AthleteEffort>()
            .Include(e => e.Athlete)
                .ThenInclude(a => a!.Team)
            .Where(e => e.CreatedAt >= since)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<AthleteEffort?> GetByIdAsync(Guid id)
    {
        return await _context.Set<AthleteEffort>().FirstOrDefaultAsync(x => x.Id == id);
    }

    public async Task UpdateAsync(AthleteEffort effort)
    {
        _context.Set<AthleteEffort>().Update(effort);
        await _context.SaveChangesAsync();
    }
}
