using Web.Features.Common;

namespace Web.Features.Admins.Teams.CreateTeam;

public class CreateTeamRequest : ISanitizable
{
    public string Name { get; set; } = null!;

    public void Sanitize()
    {
        Name = Name.Trim();
    }
}
