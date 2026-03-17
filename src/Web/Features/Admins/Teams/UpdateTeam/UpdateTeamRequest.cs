namespace Web.Features.Admins.Teams.UpdateTeam;

public class UpdateTeamRequest
{
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;
}