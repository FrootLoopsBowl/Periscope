using Web.Features.Common;

namespace Web.Features.Admins.Athletes.CreateNoteBlessure;

public class CreateNoteBlessureRequest : ISanitizable
{
    public Guid Id { get; set; }
    public string Contenu { get; set; } = null!;

    public void Sanitize()
    {
        Contenu = Contenu?.Trim()!;
    }
}
