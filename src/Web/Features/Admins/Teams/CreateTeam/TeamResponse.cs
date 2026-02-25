namespace Web.Features.Admins.Teams.CreateTeam;

public class TeamResponse
{
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;
    public DateTime CreatedAt { get; set; }
}
