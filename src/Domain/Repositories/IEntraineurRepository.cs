using Domain.Common;
using Domain.Entities;

namespace Domain.Repositories;

public interface IEntraineurRepository
{
    PaginatedList<Entraineur> GetAllPaginated(int pageIndex, int pageSize);
    Entraineur FindById(Guid id);
    Entraineur? FindByUserId(Guid userId, bool asNoTracking = true);
    Entraineur? FindByUserEmail(string userEmail);
    Task Create(Entraineur entraineur);
    Task Update(Entraineur entraineur);
}
