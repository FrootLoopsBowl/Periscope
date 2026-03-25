using Domain.Common;
using Domain.Entities;

namespace Domain.Repositories;

public interface ITeamRepository
{
    Task CreateAsync(Team team);
    Task<bool> ExistsByNameAsync(string name);
    PaginatedList<Team> GetAllPaginated(int pageIndex, int pageSize);
    Task<IEnumerable<Team>> GetAllAsync();
    Task<Team?> FindByIdAsync(Guid id);
    Task<Team?> FindByIdWithAthletesAsync(Guid id);
    Task DeleteAsync(Team team);

    Task UpdateAsync(Team team);
}
