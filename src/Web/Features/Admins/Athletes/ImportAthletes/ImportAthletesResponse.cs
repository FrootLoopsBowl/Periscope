namespace Web.Features.Admins.Athletes.ImportAthletes;

public class ImportAthletesResponse
{
    public int CreatedCount { get; set; }
    public int UpdatedCount { get; set; }
    public List<ImportAthletesError> Errors { get; set; } = new();
}

public class ImportAthletesError
{
    public int Row { get; set; }
    public string? Email { get; set; }
    public string Message { get; set; } = string.Empty;
}
