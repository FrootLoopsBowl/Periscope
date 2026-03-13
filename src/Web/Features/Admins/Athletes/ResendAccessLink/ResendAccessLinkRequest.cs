using Web.Features.Common;

namespace Web.Features.Admins.Athletes.ResendAccessLink;

public class ResendAccessLinkRequest : ISanitizable
{
    public Guid Id { get; set; }
    public string AthletePageRelativeUrl { get; set; } = null!;

    public void Sanitize()
    {
        AthletePageRelativeUrl = AthletePageRelativeUrl.Trim();
    }
}
