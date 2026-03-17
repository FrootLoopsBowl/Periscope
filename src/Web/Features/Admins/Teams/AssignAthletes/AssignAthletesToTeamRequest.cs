namespace Web.Features.Admins.Teams.AssignAthletes;

public class AssignAthletesToTeamRequest
{
    public Guid Id { get; set; }
    public List<Guid> AthleteIds { get; set; } = new();
}
