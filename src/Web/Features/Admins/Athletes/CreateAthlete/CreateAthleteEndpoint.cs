using Application.Interfaces.Services.Notifications;
using Application.Settings;
using Domain.Entities;
using Domain.Repositories;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Options;
using Web.Features.Common;

namespace Web.Features.Admins.Athletes.CreateAthlete;

public class CreateAthleteEndpoint : EndpointWithSanitizedRequest<CreateAthleteRequest, AthleteResponse>
{
    private readonly string _baseUrl;
    private readonly IAthleteRepository _athleteRepository;
    private readonly INotificationService _notificationService;

    public CreateAthleteEndpoint(
        IAthleteRepository athleteRepository,
        INotificationService notificationService,
        IOptions<ApplicationSettings> applicationSettings)
    {
        _baseUrl = applicationSettings.Value.BaseUrl;
        _athleteRepository = athleteRepository;
        _notificationService = notificationService;
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
            var existingAthlete = await _athleteRepository.FindByEmailAsync(req.Email);
            if (existingAthlete is null)
            {
                HttpContext.Response.StatusCode = StatusCodes.Status500InternalServerError;
                await HttpContext.Response.WriteAsJsonAsync(new
                {
                    errors = new[] { new { errorType = "AthleteNotFound", errorMessage = "Athlete with this email could not be found." } }
                }, ct);
                return;
            }

            var resendSucceeded = await SendAccessLinkAsync(existingAthlete, req.AthletePageRelativeUrl, ct);
            if (!resendSucceeded)
                return;

            var existingResponse = new AthleteResponse
            {
                Id = existingAthlete.Id,
                FirstName = existingAthlete.FirstName,
                LastName = existingAthlete.LastName,
                Email = existingAthlete.Email,
                DateOfBirth = existingAthlete.DateOfBirth,
                SubmissionToken = existingAthlete.SubmissionToken,
                Active = existingAthlete.Active,
                CreatedAt = existingAthlete.CreatedAt
            };

            HttpContext.Response.StatusCode = StatusCodes.Status200OK;
            await HttpContext.Response.WriteAsJsonAsync(existingResponse, ct);
            return;
        }

        var athlete = new Athlete(req.FirstName, req.LastName, req.Email, req.DateOfBirth);

        await _athleteRepository.CreateAsync(athlete);

        var sendSucceeded = await SendAccessLinkAsync(athlete, req.AthletePageRelativeUrl, ct);
        if (!sendSucceeded)
            return;

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

    private async Task<bool> SendAccessLinkAsync(Athlete athlete, string athletePageRelativeUrl, CancellationToken ct)
    {
        var baseUrl = _baseUrl.TrimEnd('/');
        var relativePath = athletePageRelativeUrl.TrimEnd('/');
        var athleteLink = $"{baseUrl}{relativePath}/{athlete.SubmissionToken}";
        var emailResponse = await _notificationService.SendAthleteAccessNotification(athlete.Email, athleteLink);

        if (!emailResponse.Succeeded)
        {
            HttpContext.Response.StatusCode = StatusCodes.Status500InternalServerError;
            await HttpContext.Response.WriteAsJsonAsync(new
            {
                errors = emailResponse.Errors
            }, ct);
            return false;
        }
        
        return true;
    }
}
