using Application.Interfaces.Services.Notifications;
using Application.Settings;
using Domain.Common;
using Domain.Repositories;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Options;
using Web.Features.Common;

namespace Web.Features.Admins.Athletes.ResendAccessLink;

public class ResendAccessLinkEndpoint : EndpointWithSanitizedRequest<ResendAccessLinkRequest, SucceededOrNotResponse>
{
    private readonly string _publicBaseUrl;
    private readonly IAthleteRepository _athleteRepository;
    private readonly INotificationService _notificationService;

    public ResendAccessLinkEndpoint(
        IAthleteRepository athleteRepository,
        INotificationService notificationService,
        IOptions<ApplicationSettings> applicationSettings)
    {
        _athleteRepository = athleteRepository;
        _notificationService = notificationService;
        _publicBaseUrl = string.IsNullOrWhiteSpace(applicationSettings.Value.PublicBaseUrl)
            ? applicationSettings.Value.BaseUrl
            : applicationSettings.Value.PublicBaseUrl;
    }

    public override void Configure()
    {
        DontCatchExceptions();
        Post("athletes/{id}/resend-access-link");
        Roles(Domain.Constants.User.Roles.ADMINISTRATOR);
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
    }

    public override async Task HandleAsync(ResendAccessLinkRequest req, CancellationToken ct)
    {
        var athlete = await _athleteRepository.FindByIdAsync(req.Id);
        if (athlete is null)
        {
            HttpContext.Response.StatusCode = StatusCodes.Status404NotFound;
            await HttpContext.Response.WriteAsJsonAsync(new SucceededOrNotResponse(false, [
                new Error("AthleteNotFound", "Athlete could not be found.")
            ]), ct);
            return;
        }

        var baseUrl = ResolvePublicBaseUrl();
        var relativePath = req.AthletePageRelativeUrl.Trim('/');
        var athleteLink = $"{baseUrl}/{relativePath}/{athlete.SubmissionToken}";

        var response = await _notificationService.SendAthleteAccessNotification(athlete.Email, athleteLink);
        await Send.OkAsync(new SucceededOrNotResponse(response.Succeeded, response.Errors), ct);
    }

    private string ResolvePublicBaseUrl()
    {
        var origin = HttpContext.Request.Headers.Origin.ToString().Trim();

        return Uri.TryCreate(origin, UriKind.Absolute, out var originUri)
            ? originUri.GetLeftPart(UriPartial.Authority).TrimEnd('/')
            : _publicBaseUrl.TrimEnd('/');
    }
}
