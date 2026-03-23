using Domain.Common;

namespace Domain.Entities;

public class AthleteEffort : Common.Entity
{
    public Guid AthleteId { get; private set; }
    public Athlete Athlete { get; private set; } = null!;

    public int Effort { get; private set; }
    public int DurationMinutes { get; private set; }
    public int? Pleasure { get; private set; }
    public DateTime CreatedAt { get; private set; }

    public AthleteEffort() { }

    public AthleteEffort(Guid athleteId, int effort, int durationMinutes, int? pleasure = null)
    {
        AthleteId = athleteId;
        Effort = effort;
        DurationMinutes = durationMinutes;
        Pleasure = pleasure;
        CreatedAt = DateTime.UtcNow;
    }

    public void SetEffort(int effort) => Effort = effort;
    public void SetDurationMinutes(int minutes) => DurationMinutes = minutes;
    public void SetPleasure(int? pleasure) => Pleasure = pleasure;
}
