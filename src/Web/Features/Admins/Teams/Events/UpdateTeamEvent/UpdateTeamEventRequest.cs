namespace Web.Features.Admins.Teams.Events.UpdateTeamEvent;

public class UpdateTeamEventRequest
{
    public Guid TeamId { get; set; }
    public Guid EventId { get; set; }
    public string Type { get; set; } = null!;
    public DateTime StartDateTime { get; set; }
    public DateTime EndDateTime { get; set; }
    public string? Description { get; set; }
}
