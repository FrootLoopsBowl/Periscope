namespace Domain.Entities;

public class Athlete : Common.Entity
{
    public string FirstName { get; private set; } = null!;
    public string LastName { get; private set; } = null!;
    public string Email { get; private set; } = null!;
    public DateTime DateOfBirth { get; private set; }
    public Guid SubmissionToken { get; private set; }
    public bool Active { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public bool IsInjured { get; private set; }
    public Guid? TeamId { get; private set; }
    public Team? Team { get; private set; }

    public void SetIsInjured(bool isInjured) => IsInjured = isInjured;

    public Athlete() { }

    public Athlete(string firstName, string lastName, string email)
    {
        FirstName = firstName;
        LastName = lastName;
        Email = email;
        SubmissionToken = Guid.NewGuid();
        Active = true;
        CreatedAt = DateTime.UtcNow;
    }

    public Athlete(string firstName, string lastName, string email, DateTime dateOfBirth)
        : this(firstName, lastName, email)
    {
        DateOfBirth = dateOfBirth;
    }

    public void SetFirstName(string firstName) => FirstName = firstName;
    public void SetLastName(string lastName) => LastName = lastName;
    public void SetEmail(string email) => Email = email;
    public void SetDateOfBirth(DateTime dateOfBirth) => DateOfBirth = dateOfBirth;

    public void Activate() => Active = true;
    public void Deactivate() => Active = false;

    public void AssignTeam(Guid teamId) => TeamId = teamId;
    public void RemoveTeam() => TeamId = null;
}
