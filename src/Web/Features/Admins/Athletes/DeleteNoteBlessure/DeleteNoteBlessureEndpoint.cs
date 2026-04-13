using Domain.Repositories;
using FastEndpoints;
using Microsoft.AspNetCore.Authentication.JwtBearer;

namespace Web.Features.Admins.Athletes.DeleteNoteBlessure;

public class DeleteNoteBlessureEndpoint : Endpoint<DeleteNoteBlessureRequest, EmptyResponse>
{
    private readonly INoteBlessureRepository _noteBlessureRepository;

    public DeleteNoteBlessureEndpoint(INoteBlessureRepository noteBlessureRepository)
    {
        _noteBlessureRepository = noteBlessureRepository;
    }

    public override void Configure()
    {
        DontCatchExceptions();
        Delete("athletes/{athleteId}/notes-blessure/{noteId}");
        Roles(Domain.Constants.User.Roles.ADMINISTRATOR);
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
    }

    public override async Task HandleAsync(DeleteNoteBlessureRequest req, CancellationToken ct)
    {
        var note = await _noteBlessureRepository.GetByIdAsync(req.NoteId);
        if (note is null || note.AthleteId != req.AthleteId)
        {
            await Send.NotFoundAsync(ct);
            return;
        }

        await _noteBlessureRepository.DeleteAsync(note);
        await Send.NoContentAsync(ct);
    }
}
