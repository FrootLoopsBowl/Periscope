using Domain.Entities;

namespace Domain.Repositories;

public interface ITeamEventRepository
{
    Task CreateAsync(TeamEvent teamEvent);
    Task<IEnumerable<TeamEvent>> GetByTeamIdAndRangeAsync(Guid teamId, DateTime from, DateTime to);
    Task<TeamEvent?> FindByIdAsync(Guid id);
    Task UpdateAsync(TeamEvent teamEvent);
    Task DeleteAsync(TeamEvent teamEvent);
}
