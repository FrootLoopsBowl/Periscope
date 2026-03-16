using Domain.Repositories;
using FastEndpoints;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Web.Features.Admins.Athletes.CreateAthlete;

namespace Web.Features.Admins.Athletes.GetInjuredAthletes;

public class GetInjuredAthletesEndpoint : EndpointWithoutRequest<IReadOnlyList<AthleteResponse>>
{
    private readonly IAthleteRepository _athleteRepository;

    public GetInjuredAthletesEndpoint(IAthleteRepository athleteRepository)
    {
        _athleteRepository = athleteRepository;
    }

    public override void Configure()
    {
        DontCatchExceptions();
        Get("athletes/injured");
        Roles(Domain.Constants.User.Roles.ADMINISTRATOR);
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var athletes = await _athleteRepository.GetInjuredAsync();
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
            IsInjured = a.IsInjured
        }).ToList();

        await Send.OkAsync(response, cancellation: ct);
    }
}
