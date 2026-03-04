using Domain.Repositories;
using FastEndpoints;
using Microsoft.AspNetCore.Authentication.JwtBearer;

namespace Web.Features.Admins.Athletes.DeleteAthlete;

public class DeleteAthleteEndpoint : Endpoint<DeleteAthleteRequest, EmptyResponse>
{
    private readonly IAthleteRepository _athleteRepository;

    public DeleteAthleteEndpoint(IAthleteRepository athleteRepository)
    {
        _athleteRepository = athleteRepository;
    }

    public override void Configure()
    {
        DontCatchExceptions();
        Delete("athletes/{id}");
        Roles(Domain.Constants.User.Roles.ADMINISTRATOR);
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
    }

    public override async Task HandleAsync(DeleteAthleteRequest req, CancellationToken ct)
    {
        var athlete = await _athleteRepository.FindByIdAsync(req.Id);
        if (athlete is null)
        {
            await Send.NotFoundAsync(ct);
            return;
        }

        await _athleteRepository.DeleteAsync(athlete);
        await Send.NoContentAsync(ct);
    }
}
