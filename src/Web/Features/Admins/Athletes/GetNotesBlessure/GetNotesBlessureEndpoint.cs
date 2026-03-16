using Domain.Repositories;
using FastEndpoints;
using Microsoft.AspNetCore.Authentication.JwtBearer;

namespace Web.Features.Admins.Athletes.GetNotesBlessure;

public class GetNotesBlessureEndpoint : Endpoint<GetNotesBlessureRequest, IReadOnlyList<NoteBlessureResponse>>
{
    private readonly IAthleteRepository _athleteRepository;
    private readonly INoteBlessureRepository _noteBlessureRepository;

    public GetNotesBlessureEndpoint(
        IAthleteRepository athleteRepository,
        INoteBlessureRepository noteBlessureRepository)
    {
        _athleteRepository = athleteRepository;
        _noteBlessureRepository = noteBlessureRepository;
    }

    public override void Configure()
    {
        DontCatchExceptions();
        Get("athletes/{id}/notes-blessure");
        Roles(Domain.Constants.User.Roles.ADMINISTRATOR);
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
    }

    public override async Task HandleAsync(GetNotesBlessureRequest req, CancellationToken ct)
    {
        var athlete = await _athleteRepository.FindByIdAsync(req.Id);
        if (athlete == null)
        {
            await Send.NotFoundAsync(ct);
            return;
        }

        var notes = await _noteBlessureRepository.GetByAthleteIdAsync(req.Id);
        var response = notes.Select(n => new NoteBlessureResponse
        {
            Id = n.Id,
            AthleteId = n.AthleteId,
            Contenu = n.Contenu,
            CreatedAt = n.CreatedAt
        }).ToList();

        await Send.OkAsync(response, cancellation: ct);
    }
}
