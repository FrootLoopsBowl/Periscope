using Domain.Entities;

namespace Domain.Repositories;

public interface INoteBlessureRepository
{
    Task CreateAsync(NoteBlessure note);
    Task<IReadOnlyList<NoteBlessure>> GetByAthleteIdAsync(Guid athleteId);
    Task<NoteBlessure?> GetByIdAsync(Guid noteId);
    Task UpdateAsync(NoteBlessure note);
    Task DeleteAsync(NoteBlessure note);
}
