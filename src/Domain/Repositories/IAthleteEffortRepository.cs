using Domain.Common;
using Domain.Entities;

namespace Domain.Repositories;

public interface IAthleteEffortRepository
{
    Task CreateAsync(AthleteEffort effort);
    PaginatedList<AthleteEffort> GetForAthletePaginated(Guid athleteId, int pageIndex, int pageSize, DateTime? startDate = null, DateTime? endDate = null);
    Task<List<AthleteEffort>> GetEffortsSinceAsync(DateTime since);
    Task<AthleteEffort?> GetByIdAsync(Guid id);
    Task UpdateAsync(AthleteEffort effort);
}
