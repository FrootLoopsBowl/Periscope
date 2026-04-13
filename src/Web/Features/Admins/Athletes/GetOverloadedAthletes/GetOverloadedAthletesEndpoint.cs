using Domain.Repositories;
using FastEndpoints;
using Microsoft.AspNetCore.Authentication.JwtBearer;

namespace Web.Features.Admins.Athletes.GetOverloadedAthletes;

public class GetOverloadedAthletesEndpoint : EndpointWithoutRequest<IReadOnlyList<OverloadedAthleteResponse>>
{
    private readonly IAthleteEffortRepository _athleteEffortRepository;

    public GetOverloadedAthletesEndpoint(IAthleteEffortRepository athleteEffortRepository)
    {
        _athleteEffortRepository = athleteEffortRepository;
    }

    public override void Configure()
    {
        DontCatchExceptions();
        Get("athletes/overloaded");
        Roles(Domain.Constants.User.Roles.ADMINISTRATOR);
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        // Calculate Monday of current week
        var today = DateTime.UtcNow.Date;
        var daysSinceMonday = ((int)today.DayOfWeek + 6) % 7;
        var currentWeekMonday = today.AddDays(-daysSinceMonday);

        // We need 6 weeks of data: 5 previous weeks + current week
        var sixWeeksAgo = currentWeekMonday.AddDays(-35);

        var allEfforts = await _athleteEffortRepository.GetEffortsSinceAsync(sixWeeksAgo);

        // Group efforts by athlete
        var effortsByAthlete = allEfforts.GroupBy(e => e.AthleteId);

        var overloadedAthletes = new List<OverloadedAthleteResponse>();

        foreach (var group in effortsByAthlete)
        {
            var athlete = group.First().Athlete;

            // Current week load (Monday to now)
            var currentWeekLoad = group
                .Where(e => e.CreatedAt >= currentWeekMonday)
                .Sum(e => e.Effort * e.DurationMinutes);

            if (currentWeekLoad == 0) continue;

            // Previous 5 weeks loads
            var previousWeekLoads = new List<int>();
            for (var i = 1; i <= 5; i++)
            {
                var weekStart = currentWeekMonday.AddDays(-7 * i);
                var weekEnd = weekStart.AddDays(7);
                var weekLoad = group
                    .Where(e => e.CreatedAt >= weekStart && e.CreatedAt < weekEnd)
                    .Sum(e => e.Effort * e.DurationMinutes);
                previousWeekLoads.Add(weekLoad);
            }

            var average = previousWeekLoads.Average();
            if (average == 0) continue;

            var overloadPercentage = ((currentWeekLoad - average) / average) * 100;

            if (overloadPercentage > 10)
            {
                overloadedAthletes.Add(new OverloadedAthleteResponse
                {
                    Id = athlete.Id,
                    FirstName = athlete.FirstName,
                    LastName = athlete.LastName,
                    TeamName = athlete.Team?.Name,
                    OverloadPercentage = Math.Round(overloadPercentage, 1)
                });
            }
        }

        var sorted = overloadedAthletes.OrderByDescending(a => a.OverloadPercentage).ToList();
        await Send.OkAsync(sorted, cancellation: ct);
    }
}
