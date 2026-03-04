using Domain.Repositories;
using FastEndpoints;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Web.Features.Admins.Athletes.CreateAthlete;

namespace Web.Features.Admins.Athletes.GetAllAthletesNonPaginated;

public class GetAllAthletesNonPaginatedEndpoint : EndpointWithoutRequest<List<AthleteResponse>>
{
    private readonly IAthleteRepository _athleteRepository;

    public GetAllAthletesNonPaginatedEndpoint(IAthleteRepository athleteRepository)
    {
        _athleteRepository = athleteRepository;
    }

    public override void Configure()
    {
        DontCatchExceptions();
        Get("athletes/all");
        Roles(Domain.Constants.User.Roles.ADMINISTRATOR);
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var athletes = await _athleteRepository.GetAllAsync();
        var response = athletes.Select(a => new AthleteResponse
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
        }).ToList();
        await Send.OkAsync(response, cancellation: ct);
    }
}
