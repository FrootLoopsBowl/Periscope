using Web.Features.Common;

namespace Web.Features.Admins.Athletes.GetAthleteEfforts;

public class GetAthleteEffortsRequest : PaginateRequest
{
    public Guid AthleteId { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
}