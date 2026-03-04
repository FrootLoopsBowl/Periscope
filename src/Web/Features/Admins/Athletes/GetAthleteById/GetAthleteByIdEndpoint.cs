using Domain.Repositories;
using FastEndpoints;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Web.Features.Admins.Athletes.CreateAthlete;

namespace Web.Features.Admins.Athletes.GetAthleteById;

public class GetAthleteByIdEndpoint : Endpoint<GetAthleteByIdRequest, AthleteResponse>
{
    private readonly IAthleteRepository _athleteRepository;

    public GetAthleteByIdEndpoint(IAthleteRepository athleteRepository)
    {
        _athleteRepository = athleteRepository;
    }

    public override void Configure()
    {
        DontCatchExceptions();
        Get("athletes/{id}");
        Roles(Domain.Constants.User.Roles.ADMINISTRATOR);
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
    }

    public override async Task HandleAsync(GetAthleteByIdRequest req, CancellationToken ct)
    {
        var athlete = await _athleteRepository.FindByIdAsync(req.Id);
        if (athlete is null)
        {
            await Send.NotFoundAsync(ct);
            return;
        }

        var response = new AthleteResponse
        {
            Id = athlete.Id,
            FirstName = athlete.FirstName,
            LastName = athlete.LastName,
            Email = athlete.Email,
            DateOfBirth = athlete.DateOfBirth,
            SubmissionToken = athlete.SubmissionToken,
            Active = athlete.Active,
            CreatedAt = athlete.CreatedAt,
            TeamId = athlete.TeamId,
            TeamName = athlete.Team?.Name
        };

        await Send.OkAsync(response, cancellation: ct);
    }
}
