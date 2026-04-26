namespace Domain.Entities;

public class TeamEvent : Common.Entity
{
    public Guid TeamId { get; private set; }
    public Team Team { get; private set; } = null!;
    public EventType Type { get; private set; }
    public DateTime StartDateTime { get; private set; }
    public DateTime EndDateTime { get; private set; }
    public string? Description { get; private set; }
    public DateTime CreatedAt { get; private set; }

    public TeamEvent() { }

    public TeamEvent(Guid teamId, EventType type, DateTime startDateTime, DateTime endDateTime, string? description = null)
    {
        if (teamId == Guid.Empty)
            throw new ArgumentException("TeamId cannot be empty.", nameof(teamId));
        if (startDateTime >= endDateTime)
            throw new ArgumentException("StartDateTime must be before EndDateTime.", nameof(startDateTime));

        TeamId = teamId;
        Type = type;
        StartDateTime = startDateTime;
        EndDateTime = endDateTime;
        Description = description;
        CreatedAt = DateTime.UtcNow;
    }

    public void Update(EventType type, DateTime startDateTime, DateTime endDateTime, string? description = null)
    {
        if (startDateTime >= endDateTime)
            throw new ArgumentException("StartDateTime must be before EndDateTime.", nameof(startDateTime));

        Type = type;
        StartDateTime = startDateTime;
        EndDateTime = endDateTime;
        Description = description;
    }
}
