using Domain.Common;
using Domain.Entities;
using Domain.Repositories;
using FastEndpoints;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Web.Features.Common;

namespace Web.Features.Admins.Athletes.GetAthleteEfforts;

public class GetAthleteEffortsEndpoint : Endpoint<GetAthleteEffortsRequest, object>
{
    private readonly IAthleteEffortRepository _athleteEffortRepository;

    public GetAthleteEffortsEndpoint(IAthleteEffortRepository athleteEffortRepository)
    {
        _athleteEffortRepository = athleteEffortRepository;
    }

    public override void Configure()
    {
        Get("athletes/{AthleteId}/efforts");
        Roles(Domain.Constants.User.Roles.ADMINISTRATOR);
    }

    public override async Task HandleAsync(GetAthleteEffortsRequest req, CancellationToken ct)
    {
        var query = _athleteEffortRepository.GetForAthletePaginated(
            req.AthleteId,
            req.PageIndex,
            req.PageSize,
            req.StartDate,
            req.EndDate);
        
        var response = new 
        {
            totalItems = query.TotalItems,
            items = query.Items.Select(e => new AthleteEffortResponse
            {
                Id = e.Id,
                Effort = e.Effort,
                DurationMinutes = e.DurationMinutes,
                Pleasure = e.Pleasure,
                CreatedAt = e.CreatedAt
            }).ToList()
        };
        
        await Send.OkAsync(response, cancellation: ct);
    }
}