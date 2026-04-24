namespace Web.Features.Admins.Teams.Events.CreateTeamEvent;

public class CreateTeamEventRequest
{
    public Guid TeamId { get; set; }
    public string Type { get; set; } = null!;
    public DateTime StartDateTime { get; set; }
    public DateTime EndDateTime { get; set; }
    public string? Description { get; set; }
}
