namespace Web.Features.Admins.Teams.Events.GetTeamEvents;

public class TeamEventResponse
{
    public Guid Id { get; set; }
    public string Type { get; set; } = null!;
    public DateTime StartDateTime { get; set; }
    public DateTime EndDateTime { get; set; }
}
