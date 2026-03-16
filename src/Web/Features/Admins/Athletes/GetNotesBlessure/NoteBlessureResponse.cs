namespace Web.Features.Admins.Athletes.GetNotesBlessure;

public class NoteBlessureResponse
{
    public Guid Id { get; set; }
    public Guid AthleteId { get; set; }
    public string Contenu { get; set; } = null!;
    public DateTime CreatedAt { get; set; }
}
