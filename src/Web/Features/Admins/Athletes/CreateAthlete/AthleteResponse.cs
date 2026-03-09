namespace Web.Features.Admins.Athletes.CreateAthlete;

public class AthleteResponse
{
    public Guid Id { get; set; }
    public string FirstName { get; set; } = null!;
    public string LastName { get; set; } = null!;
    public string Email { get; set; } = null!;
    public DateTime DateOfBirth { get; set; }
    public Guid SubmissionToken { get; set; }
    public bool Active { get; set; }
    public DateTime CreatedAt { get; set; }
    public Guid? TeamId { get; set; }
    public string? TeamName { get; set; }
}
