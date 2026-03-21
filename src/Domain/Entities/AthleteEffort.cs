using Domain.Common;

namespace Domain.Entities;

public class AthleteEffort : Common.Entity
{
    public Guid AthleteId { get; private set; }
    public Athlete Athlete { get; private set; } = null!;

    public int Effort { get; private set; }
    public int DurationMinutes { get; private set; }
    public DateTime CreatedAt { get; private set; }

    public AthleteEffort() { }

    public AthleteEffort(Guid athleteId, int effort, int durationMinutes)
    {
        AthleteId = athleteId;
        Effort = effort;
        DurationMinutes = durationMinutes;
        CreatedAt = DateTime.UtcNow;
    }

    public void SetEffort(int effort) => Effort = effort;
    public void SetDurationMinutes(int minutes) => DurationMinutes = minutes;
}
