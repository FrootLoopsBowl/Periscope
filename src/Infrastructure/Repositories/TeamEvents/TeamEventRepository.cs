using Domain.Entities;
using Domain.Repositories;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Infrastructure.Repositories.TeamEvents;

public class TeamEventRepository : ITeamEventRepository
{
    private readonly GarneauTemplateDbContext _context;

    public TeamEventRepository(GarneauTemplateDbContext context)
    {
        _context = context;
    }

    public async Task CreateAsync(TeamEvent teamEvent)
    {
        _context.TeamEvents.Add(teamEvent);
        await _context.SaveChangesAsync();
    }

    public async Task<IEnumerable<TeamEvent>> GetByTeamIdAndRangeAsync(Guid teamId, DateTime from, DateTime to)
    {
        return await _context.TeamEvents
            .AsNoTracking()
            .Where(x => x.TeamId == teamId && x.StartDateTime >= from && x.StartDateTime <= to)
            .OrderBy(x => x.StartDateTime)
            .ToListAsync();
    }

    public async Task<TeamEvent?> FindByIdAsync(Guid id)
    {
        return await _context.TeamEvents.FindAsync(id);
    }

    public async Task UpdateAsync(TeamEvent teamEvent)
    {
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(TeamEvent teamEvent)
    {
        _context.TeamEvents.Remove(teamEvent);
        await _context.SaveChangesAsync();
    }
}
