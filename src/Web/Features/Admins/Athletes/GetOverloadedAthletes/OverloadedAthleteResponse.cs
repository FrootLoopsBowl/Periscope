namespace Web.Features.Admins.Athletes.GetOverloadedAthletes;

public class OverloadedAthleteResponse
{
    public Guid Id { get; set; }
    public string FirstName { get; set; } = null!;
    public string LastName { get; set; } = null!;
    public string? TeamName { get; set; }
    public double OverloadPercentage { get; set; }
}
