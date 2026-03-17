using Domain.Repositories;
using FastEndpoints;
using Microsoft.AspNetCore.Authentication.JwtBearer;

namespace Web.Features.Admins.Athletes.UpdateAthlete;

public class UpdateAthleteEndpoint : Endpoint<UpdateAthleteRequest, EmptyResponse>
{
    private readonly IAthleteRepository _athleteRepository;

    public UpdateAthleteEndpoint(IAthleteRepository athleteRepository)
    {
        _athleteRepository = athleteRepository;
    }

    public override void Configure()
    {
        DontCatchExceptions();
        Put("athletes/{id}");
        Roles(Domain.Constants.User.Roles.ADMINISTRATOR);
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
    }

    public override async Task HandleAsync(UpdateAthleteRequest req, CancellationToken ct)
    {
        var athlete = await _athleteRepository.FindByIdAsync(req.Id);
        if (athlete is null)
        {
            await Send.NotFoundAsync(ct);
            return;
        }

        if (athlete.Email != req.Email)
        {
            var emailAlreadyUsed = await _athleteRepository.ExistsByEmailAsync(req.Email);
            if (emailAlreadyUsed)
            {
                HttpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
                await HttpContext.Response.WriteAsJsonAsync(new
                {
                    errors = new[] { new { errorType = "AthleteWithEmailAlreadyExists", errorMessage = "An athlete with this email already exists." } }
                }, ct);
                return;
            }
        }

        athlete.SetFirstName(req.FirstName);
        athlete.SetLastName(req.LastName);
        athlete.SetEmail(req.Email);
        athlete.SetDateOfBirth(req.DateOfBirth);

        await _athleteRepository.UpdateAsync(athlete);
        await Send.NoContentAsync(ct);
    }
}