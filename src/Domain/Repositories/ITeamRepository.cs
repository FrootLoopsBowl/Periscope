using Domain.Common;
using Domain.Entities;

namespace Domain.Repositories;

public interface ITeamRepository
{
    Task CreateAsync(Team team);
    Task<bool> ExistsByNameAsync(string name);
    PaginatedList<Team> GetAllPaginated(int pageIndex, int pageSize);
}
