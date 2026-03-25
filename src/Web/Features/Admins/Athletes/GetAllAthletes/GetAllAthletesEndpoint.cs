using Domain.Common;
using Domain.Repositories;
using FastEndpoints;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Web.Features.Admins.Athletes.CreateAthlete;
using Web.Features.Common;

namespace Web.Features.Admins.Athletes.GetAllAthletes;

public class GetAllAthletesEndpoint : Endpoint<PaginateRequest, PaginatedList<AthleteResponse>>
{
    private readonly IAthleteRepository _athleteRepository;

    public GetAllAthletesEndpoint(IAthleteRepository athleteRepository)
    {
        _athleteRepository = athleteRepository;
    }

    public override void Configure()
    {
        DontCatchExceptions();
        Get("athletes");
        Roles(Domain.Constants.User.Roles.ADMINISTRATOR);
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
    }

    public override async Task HandleAsync(PaginateRequest req, CancellationToken ct)
    {
        var paginatedList = _athleteRepository.GetAllPaginated(req.PageIndex, req.PageSize);
        var response = new PaginatedList<AthleteResponse>(
            paginatedList.Items.Select(a => new AthleteResponse
            {
                Id = a.Id,
                FirstName = a.FirstName,
                LastName = a.LastName,
                Email = a.Email,
                DateOfBirth = a.DateOfBirth,
                SubmissionToken = a.SubmissionToken,
                Active = a.Active,
                CreatedAt = a.CreatedAt,
                TeamId = a.TeamId,
                TeamName = a.Team?.Name
            }),
            paginatedList.TotalItems);
        await Send.OkAsync(response, cancellation: ct);
    }
}
