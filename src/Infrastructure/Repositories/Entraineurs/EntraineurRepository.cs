using Application.Exceptions.Members;
using Domain.Common;
using Domain.Entities;
using Domain.Repositories;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Infrastructure.Repositories.Entraineurs;

public class EntraineurRepository : IEntraineurRepository
{
    private readonly GarneauTemplateDbContext _context;

    public EntraineurRepository(GarneauTemplateDbContext context)
    {
        _context = context;
    }

    public PaginatedList<Entraineur> GetAllPaginated(int pageIndex, int pageSize)
    {
        var query = _context.Entraineurs
            .Include(x => x.User)
            .AsNoTracking();
        var pageItems = query.OrderByDescending(x => x.Created).Skip((pageIndex-1) * pageSize).Take(pageSize);
        return new PaginatedList<Entraineur>(pageItems.ToList(), query.Count());
    }

    public Entraineur FindById(Guid id)
    {
        var entraineur = _context.Entraineurs
            .Include(x => x.User)
            .ThenInclude(x => x.UserRoles)
            .ThenInclude(x => x.Role)
            .FirstOrDefault(x => x.Id == id);
        if (entraineur == null)
            throw new Exception($"No entraineur with id {id} was found.");
        return entraineur;
    }

    public Entraineur? FindByUserId(Guid userId, bool asNoTracking = true)
    {
        var query = _context.Entraineurs as IQueryable<Entraineur>;
        if (asNoTracking)
            query = query.AsNoTracking();
        return query
            .Include(x => x.User)
            .ThenInclude(x => x.UserRoles)
            .ThenInclude(x => x.Role)
            .FirstOrDefault(x => x.User.Id == userId);
    }

    public Entraineur? FindByUserEmail(string userEmail)
    {
        return _context.Entraineurs
            .Include(x => x.User)
            .ThenInclude(x => x.UserRoles)
            .ThenInclude(x => x.Role)
            .FirstOrDefault(x => x.User.Email == userEmail);
    }

    public async Task Create(Entraineur entraineur)
    {
        _context.Entraineurs.Add(entraineur);
        await _context.SaveChangesAsync();
    }

    public async Task Update(Entraineur entraineur)
    {
        if (!_context.Entraineurs.Any(x => x.Id == entraineur.Id))
            throw new Exception($"Could not find entraineur with id {entraineur.Id}.");

        _context.Entraineurs.Update(entraineur);
        await _context.SaveChangesAsync();
    }
}
