namespace Web.Features.Admins.Athletes.AssignTeam;

public class AssignTeamToAthleteRequest
{
    public Guid Id { get; set; }
    public Guid? TeamId { get; set; }
}
