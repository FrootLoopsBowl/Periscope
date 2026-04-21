using Domain.Entities;

namespace Tests.Domain.Entities;

public class TeamEventTests
{
    private readonly Guid _teamId = Guid.NewGuid();
    private readonly DateTime _start = new DateTime(2026, 4, 9, 18, 0, 0, DateTimeKind.Utc);
    private readonly DateTime _end = new DateTime(2026, 4, 9, 19, 30, 0, DateTimeKind.Utc);

    [Fact]
    public void WhenConstructor_WithValidArgs_ThenCreatesTeamEvent()
    {
        var teamEvent = new TeamEvent(_teamId, EventType.Pratique, _start, _end);

        teamEvent.TeamId.ShouldBe(_teamId);
        teamEvent.Type.ShouldBe(EventType.Pratique);
        teamEvent.StartDateTime.ShouldBe(_start);
        teamEvent.EndDateTime.ShouldBe(_end);
    }

    [Fact]
    public void WhenConstructor_WithEmptyTeamId_ThenThrows()
    {
        Should.Throw<ArgumentException>(() =>
            new TeamEvent(Guid.Empty, EventType.Pratique, _start, _end));
    }

    [Fact]
    public void WhenConstructor_WithStartAfterEnd_ThenThrows()
    {
        Should.Throw<ArgumentException>(() =>
            new TeamEvent(_teamId, EventType.Pratique, _end, _start));
    }

    [Fact]
    public void WhenUpdate_WithValidArgs_ThenUpdatesFields()
    {
        var teamEvent = new TeamEvent(_teamId, EventType.Pratique, _start, _end);
        var newStart = new DateTime(2026, 4, 10, 14, 0, 0, DateTimeKind.Utc);
        var newEnd = new DateTime(2026, 4, 10, 16, 0, 0, DateTimeKind.Utc);

        teamEvent.Update(EventType.Match, newStart, newEnd);

        teamEvent.Type.ShouldBe(EventType.Match);
        teamEvent.StartDateTime.ShouldBe(newStart);
        teamEvent.EndDateTime.ShouldBe(newEnd);
    }

    [Fact]
    public void WhenUpdate_WithStartAfterEnd_ThenThrows()
    {
        var teamEvent = new TeamEvent(_teamId, EventType.Pratique, _start, _end);

        Should.Throw<ArgumentException>(() =>
            teamEvent.Update(EventType.Match, _end, _start));
    }
}
