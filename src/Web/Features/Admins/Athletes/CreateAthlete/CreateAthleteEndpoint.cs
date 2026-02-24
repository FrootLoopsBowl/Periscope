using Domain.Entities;
using Domain.Repositories;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Web.Features.Common;

namespace Web.Features.Admins.Athletes.CreateAthlete;

public class CreateAthleteEndpoint : EndpointWithSanitizedRequest<CreateAthleteRequest, AthleteResponse>
{
    private readonly IAthleteRepository _athleteRepository;

    public CreateAthleteEndpoint(IAthleteRepository athleteRepository)
    {
        _athleteRepository = athleteRepository;
    }

    public override void Configure()
    {
        DontCatchExceptions();
        Post("athletes");
        Roles(Domain.Constants.User.Roles.ADMINISTRATOR);
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
    }

    public override async Task HandleAsync(CreateAthleteRequest req, CancellationToken ct)
    {
        if (await _athleteRepository.ExistsByEmailAsync(req.Email))
        {
            HttpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
            await HttpContext.Response.WriteAsJsonAsync(new
            {
                errors = new[] { new { errorType = "AthleteWithEmailAlreadyExists", errorMessage = "An athlete with this email already exists." } }
            }, ct);
            return;
        }

        var athlete = new Athlete(req.FirstName, req.LastName, req.Email, req.DateOfBirth);

        await _athleteRepository.CreateAsync(athlete);

        var response = new AthleteResponse
        {
            Id = athlete.Id,
            FirstName = athlete.FirstName,
            LastName = athlete.LastName,
            Email = athlete.Email,
            DateOfBirth = athlete.DateOfBirth,
            SubmissionToken = athlete.SubmissionToken,
            Active = athlete.Active,
            CreatedAt = athlete.CreatedAt
        };

        HttpContext.Response.StatusCode = StatusCodes.Status201Created;
        await HttpContext.Response.WriteAsJsonAsync(response, ct);
    }
}
