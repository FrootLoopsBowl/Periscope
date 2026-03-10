using Domain.Entities;

namespace Domain.Repositories;

public interface INoteBlessureRepository
{
    Task CreateAsync(NoteBlessure note);
    Task<IEnumerable<NoteBlessure>> GetByAthleteIdAsync(Guid athleteId);
}
