namespace Web.Features.Admins.Teams.GetTeamById;

public class TeamDetailResponse
{
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;
    public DateTime CreatedAt { get; set; }
    public List<AthleteInTeamResponse> Athletes { get; set; } = new();
}

public class AthleteInTeamResponse
{
    public Guid Id { get; set; }
    public string FirstName { get; set; } = null!;
    public string LastName { get; set; } = null!;
    public string Email { get; set; } = null!;
}
