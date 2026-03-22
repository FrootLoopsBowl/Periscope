namespace Web.Features.Admins.Athletes.GetAthleteEfforts;

public class AthleteEffortResponse
{
    public Guid Id { get; set; }
    public int Effort { get; set; }
    public int DurationMinutes { get; set; }
    public int? Pleasure { get; set; }
    public DateTime CreatedAt { get; set; }
}