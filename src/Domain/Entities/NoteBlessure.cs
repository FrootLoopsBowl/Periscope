namespace Domain.Entities;

public class NoteBlessure : Common.Entity
{
    public Guid AthleteId { get; private set; }
    public Athlete Athlete { get; private set; } = null!;
    public string Contenu { get; private set; } = null!;
    public DateTime CreatedAt { get; private set; }

    public NoteBlessure() { }

    public NoteBlessure(Guid athleteId, string contenu)
    {
        AthleteId = athleteId;
        Contenu = contenu;
        CreatedAt = DateTime.UtcNow;
    }
}
