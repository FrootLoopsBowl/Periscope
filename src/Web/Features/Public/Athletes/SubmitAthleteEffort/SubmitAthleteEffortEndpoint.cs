using Domain.Common;
using Domain.Entities;
using Domain.Repositories;
using FastEndpoints;
using Web.Features.Common;

namespace Web.Features.Public.Athletes.SubmitAthleteEffort;

public class SubmitAthleteEffortEndpoint : Endpoint<SubmitAthleteEffortRequest, SucceededOrNotResponse>
{
    private readonly IAthleteRepository _athleteRepository;
    private readonly IAthleteEffortRepository _athleteEffortRepository;

    public SubmitAthleteEffortEndpoint(IAthleteRepository athleteRepository, IAthleteEffortRepository athleteEffortRepository)
    {
        _athleteRepository = athleteRepository;
        _athleteEffortRepository = athleteEffortRepository;
    }

    public override void Configure()
    {
        Post("athletes/submission");
        AllowAnonymous();
    }

    public override async Task HandleAsync(SubmitAthleteEffortRequest req, CancellationToken ct)
    {
        var athlete = await _athleteRepository.FindBySubmissionTokenAsync(req.Token);
        if (athlete == null)
        {
            await Send.NotFoundAsync(ct);
            return;
        }

        if (req.Effort < 1 || req.Effort > 10)
        {
            HttpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
            await HttpContext.Response.WriteAsJsonAsync(new { error = "Effort must be between 1 and 10." }, ct);
            return;
        }

        var effort = new AthleteEffort(athlete.Id, req.Effort, req.DurationMinutes, req.Pleasure);
        await _athleteEffortRepository.CreateAsync(effort);

        await Send.OkAsync(new Domain.Common.SucceededOrNotResponse(true), ct);
    }
}
