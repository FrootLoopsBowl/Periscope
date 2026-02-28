using Domain.Common;
using Domain.Entities;
using Domain.Repositories;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Infrastructure.Repositories.Teams;

public class TeamRepository : ITeamRepository
{
    private readonly GarneauTemplateDbContext _context;

    public TeamRepository(GarneauTemplateDbContext context)
    {
        _context = context;
    }

    public async Task CreateAsync(Team team)
    {
        _context.Teams.Add(team);
        await _context.SaveChangesAsync();
    }

    public async Task<bool> ExistsByNameAsync(string name)
    {
        return await _context.Teams
            .AsNoTracking()
            .AnyAsync(x => x.Name == name);
    }

    public PaginatedList<Team> GetAllPaginated(int pageIndex, int pageSize)
    {
        var total = _context.Teams.Count();
        var items = _context.Teams
            .AsNoTracking()
            .OrderBy(x => x.Name)
            .Skip((pageIndex - 1) * pageSize)
            .Take(pageSize)
            .ToList();
        return new PaginatedList<Team>(items, total);
    }

    public async Task<Team?> FindByIdAsync(Guid id)
    {
        return await _context.Teams.FindAsync(id);
    }

    public async Task DeleteAsync(Team team)
    {
        _context.Teams.Remove(team);
        await _context.SaveChangesAsync();
    }
}
