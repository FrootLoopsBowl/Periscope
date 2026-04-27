namespace Web.Features.Admins.Teams.Events.DeleteTeamEvent;

public class DeleteTeamEventRequest
{
    public Guid TeamId { get; set; }
    public Guid EventId { get; set; }
}
