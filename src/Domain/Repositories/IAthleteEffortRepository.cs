using Domain.Common;
using Domain.Entities;

namespace Domain.Repositories;

public interface IAthleteEffortRepository
{
    Task CreateAsync(AthleteEffort effort);
    PaginatedList<AthleteEffort> GetForAthletePaginated(Guid athleteId, int pageIndex, int pageSize);
}
