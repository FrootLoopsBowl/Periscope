namespace Web.Features.Admins.Athletes.ToggleAthleteInjured;

public class ToggleAthleteInjuredRequest
{
    public Guid Id { get; set; }
    public bool IsInjured { get; set; }
}
