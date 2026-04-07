using Domain.Repositories;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Web.Features.Common;

namespace Web.Features.Admins.Athletes.UpdateNoteBlessure;

public class UpdateNoteBlessureEndpoint : EndpointWithSanitizedRequest<UpdateNoteBlessureRequest>
{
    private readonly INoteBlessureRepository _noteBlessureRepository;

    public UpdateNoteBlessureEndpoint(INoteBlessureRepository noteBlessureRepository)
    {
        _noteBlessureRepository = noteBlessureRepository;
    }

    public override void Configure()
    {
        DontCatchExceptions();
        Put("athletes/{athleteId}/notes-blessure/{noteId}");
        Roles(Domain.Constants.User.Roles.ADMINISTRATOR);
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
    }

    public override async Task HandleAsync(UpdateNoteBlessureRequest req, CancellationToken ct)
    {
        var note = await _noteBlessureRepository.GetByIdAsync(req.NoteId);
        if (note is null || note.AthleteId != req.AthleteId)
        {
            await Send.NotFoundAsync(ct);
            return;
        }

        note.UpdateContenu(req.Contenu);
        await _noteBlessureRepository.UpdateAsync(note);
        await Send.NoContentAsync(ct);
    }
}
