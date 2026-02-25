namespace Domain.Entities;

public class Team : Common.Entity
{
    public string Name { get; private set; } = null!;
    public DateTime CreatedAt { get; private set; }

    public Team() { }

    public Team(string name)
    {
        Name = name;
        CreatedAt = DateTime.UtcNow;
    }
}
