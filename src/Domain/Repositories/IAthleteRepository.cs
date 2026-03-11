using Domain.Common;
using Domain.Entities;

namespace Domain.Repositories;

public interface IAthleteRepository
{
    Task CreateAsync(Athlete athlete);
    Task<bool> ExistsByEmailAsync(string email);
    Task<Athlete?> FindBySubmissionTokenAsync(Guid token);
    PaginatedList<Athlete> GetAllPaginated(int pageIndex, int pageSize);
    Task<Athlete?> FindByIdAsync(Guid id);
    Task UpdateAsync(Athlete athlete);
    Task<IReadOnlyList<Athlete>> GetInjuredAsync();
}
