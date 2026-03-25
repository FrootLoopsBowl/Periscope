namespace Domain.Entities;

public class Team : Common.Entity
{
    public string Name { get; private set; } = null!;
    public DateTime CreatedAt { get; private set; }
    public ICollection<Athlete> Athletes { get; private set; } = new List<Athlete>();

    public Team() { }

    public Team(string name)
    {
        Name = name;
        CreatedAt = DateTime.UtcNow;
    }

    public void SetName(string name) => Name = name;
}
