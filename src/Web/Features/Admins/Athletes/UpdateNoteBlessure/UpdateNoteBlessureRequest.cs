using Web.Features.Common;

namespace Web.Features.Admins.Athletes.UpdateNoteBlessure;

public class UpdateNoteBlessureRequest : ISanitizable
{
    public Guid AthleteId { get; set; }
    public Guid NoteId { get; set; }
    public string Contenu { get; set; } = null!;

    public void Sanitize()
    {
        Contenu = Contenu?.Trim()!;
    }
}
