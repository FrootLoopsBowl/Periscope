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
        if (athleteId == Guid.Empty)
            throw new ArgumentException("AthleteId cannot be empty.", nameof(athleteId));
        if (string.IsNullOrWhiteSpace(contenu))
            throw new ArgumentException("Contenu cannot be empty.", nameof(contenu));

        AthleteId = athleteId;
        Contenu = contenu;
        CreatedAt = DateTime.UtcNow;
    }

    public void UpdateContenu(string contenu)
    {
        if (string.IsNullOrWhiteSpace(contenu))
            throw new ArgumentException("Contenu cannot be empty.", nameof(contenu));
        Contenu = contenu;
    }
}
