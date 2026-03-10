using Domain.Entities;
using Domain.Repositories;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Infrastructure.Repositories.Blessures;

public class NoteBlessureRepository : INoteBlessureRepository
{
    private readonly GarneauTemplateDbContext _context;

    public NoteBlessureRepository(GarneauTemplateDbContext context)
    {
        _context = context;
    }

    public async Task CreateAsync(NoteBlessure note)
    {
        _context.NotesBlessure.Add(note);
        await _context.SaveChangesAsync();
    }

    public async Task<IEnumerable<NoteBlessure>> GetByAthleteIdAsync(Guid athleteId)
    {
        return await _context.NotesBlessure
            .AsNoTracking()
            .Where(x => x.AthleteId == athleteId)
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync();
    }
}
