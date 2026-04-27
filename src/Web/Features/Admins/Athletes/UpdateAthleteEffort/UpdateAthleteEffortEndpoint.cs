using Domain.Repositories;
using FastEndpoints;
using Domain.Common;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Domain.Entities;

namespace Web.Features.Admins.Athletes.UpdateAthleteEffort;

public class UpdateAthleteEffortEndpoint : Endpoint<UpdateAthleteEffortRequest, SucceededOrNotResponse>
{
    private readonly IAthleteEffortRepository _athleteEffortRepository;

    public UpdateAthleteEffortEndpoint(IAthleteEffortRepository athleteEffortRepository)
    {
        _athleteEffortRepository = athleteEffortRepository;
    }

    public override void Configure()
    {
        Put("athletes/{AthleteId}/efforts/{EffortId}");
        Roles(Domain.Constants.User.Roles.ADMINISTRATOR);
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
    }

    public override async Task HandleAsync(UpdateAthleteEffortRequest req, CancellationToken ct)
    {
        if (req.Effort < 1 || req.Effort > 10)
        {
            HttpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
            await HttpContext.Response.WriteAsJsonAsync(new { error = "Effort must be between 1 and 10." }, ct);
            return;
        }

        if (req.Pleasure.HasValue && (req.Pleasure < 1 || req.Pleasure > 10))
        {
            HttpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
            await HttpContext.Response.WriteAsJsonAsync(new { error = "Pleasure must be between 1 and 10." }, ct);
            return;
        }

        var existing = await _athleteEffortRepository.GetByIdAsync(req.EffortId);
        if (existing == null || existing.AthleteId != req.AthleteId)
        {
            await Send.NotFoundAsync(ct);
            return;
        }

        if (req.TrainingDate.HasValue)
        {
            var date = DateTime.SpecifyKind(req.TrainingDate.Value.Date, DateTimeKind.Utc);
            existing.SetTrainingDate(date);
        }

        existing.SetEffort(req.Effort);
        existing.SetDurationMinutes(req.DurationMinutes);
        existing.SetPleasure(req.Pleasure);

        await _athleteEffortRepository.UpdateAsync(existing);

        await Send.OkAsync(new SucceededOrNotResponse(true), cancellation: ct);
    }
}
