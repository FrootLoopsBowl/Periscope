using Microsoft.AspNetCore.Http;

namespace Web.Features.Admins.Athletes.ImportAthletes;

public class ImportAthletesRequest
{
    public IFormFile File { get; set; } = null!;
    public string AthletePageRelativeUrl { get; set; } = null!;
}
