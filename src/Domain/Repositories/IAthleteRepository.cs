using Domain.Common;
using Domain.Entities;

namespace Domain.Repositories;

public interface IAthleteRepository
{
    Task CreateAsync(Athlete athlete);
    Task<bool> ExistsByEmailAsync(string email);
    Task<Athlete?> FindByEmailAsync(string email);
    Task<Athlete?> FindByIdAsync(Guid id);
    Task<Athlete?> FindBySubmissionTokenAsync(Guid token);
    Task UpdateAsync(Athlete athlete);
    PaginatedList<Athlete> GetAllPaginated(int pageIndex, int pageSize);
    Task<IReadOnlyList<Athlete>> GetInjuredAsync();
    Task<IEnumerable<Athlete>> GetAllAsync();
    Task DeleteAsync(Athlete athlete);
}
