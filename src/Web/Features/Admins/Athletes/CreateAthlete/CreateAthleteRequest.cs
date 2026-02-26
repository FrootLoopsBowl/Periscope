using Web.Features.Common;

namespace Web.Features.Admins.Athletes.CreateAthlete;

public class CreateAthleteRequest : ISanitizable
{
    public string FirstName { get; set; } = null!;
    public string LastName { get; set; } = null!;
    public string Email { get; set; } = null!;
    public DateTime DateOfBirth { get; set; }

    public void Sanitize()
    {
        FirstName = FirstName.Trim();
        LastName = LastName.Trim();
        Email = Email.Trim();
    }
}
