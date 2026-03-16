using Domain.Entities;
using Domain.Repositories;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Web.Features.Common;

namespace Web.Features.Admins.Athletes.CreateNoteBlessure;

public class CreateNoteBlessureEndpoint : EndpointWithSanitizedRequest<CreateNoteBlessureRequest>
{
    private readonly IAthleteRepository _athleteRepository;
    private readonly INoteBlessureRepository _noteBlessureRepository;

    public CreateNoteBlessureEndpoint(
        IAthleteRepository athleteRepository,
        INoteBlessureRepository noteBlessureRepository)
    {
        _athleteRepository = athleteRepository;
        _noteBlessureRepository = noteBlessureRepository;
    }

    public override void Configure()
    {
        DontCatchExceptions();
        Post("athletes/{id}/notes-blessure");
        Roles(Domain.Constants.User.Roles.ADMINISTRATOR);
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
    }

    public override async Task HandleAsync(CreateNoteBlessureRequest req, CancellationToken ct)
    {
        var athlete = await _athleteRepository.FindByIdAsync(req.Id);
        if (athlete == null)
        {
            await Send.NotFoundAsync(ct);
            return;
        }

        var note = new NoteBlessure(athlete.Id, req.Contenu);
        await _noteBlessureRepository.CreateAsync(note);

        HttpContext.Response.StatusCode = StatusCodes.Status201Created;
    }
}
