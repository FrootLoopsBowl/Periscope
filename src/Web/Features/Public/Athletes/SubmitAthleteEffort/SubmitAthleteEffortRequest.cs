namespace Web.Features.Public.Athletes.SubmitAthleteEffort;

public class SubmitAthleteEffortRequest
{
    public Guid Token { get; set; }
    public int Effort { get; set; }
    public int DurationMinutes { get; set; }
    public int? Pleasure { get; set; }
    public DateTime TrainingDate { get; set; }
}
