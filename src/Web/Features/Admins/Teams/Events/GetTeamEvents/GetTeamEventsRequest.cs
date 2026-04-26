namespace Web.Features.Admins.Teams.Events.GetTeamEvents;

public class GetTeamEventsRequest
{
    public Guid TeamId { get; set; }
    public DateTime? From { get; set; }
    public DateTime? To { get; set; }
}
