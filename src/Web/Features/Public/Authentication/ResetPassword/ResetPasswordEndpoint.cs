using Application.Extensions;
using Application.Interfaces.Services.Users;
using Application.Settings;
using Domain.Common;
using Domain.Extensions;
using Domain.Repositories;
using FastEndpoints;
using Microsoft.Extensions.Options;
using Web.Cookies;

namespace Web.Features.Public.Authentication.ResetPassword;

public class ResetPasswordEndpoint : Endpoint<ResetPasswordRequest, SucceededOrNotResponse>
{
    private readonly CookieSettings _cookieSettings;
    private readonly IAuthenticationService _authenticationService;
    private readonly IUserRepository _userRepository;
    private readonly ILogger<ResetPasswordEndpoint> _logger;

    public ResetPasswordEndpoint(
        IUserRepository userRepository,
        ILogger<ResetPasswordEndpoint> logger,
        IAuthenticationService authenticationService,
        IOptions<CookieSettings> cookieSettings)
    {
        _logger = logger;
        _userRepository = userRepository;
        _authenticationService = authenticationService;
        _cookieSettings = cookieSettings.Value;
    }

    public override void Configure()
    {
        DontCatchExceptions();

        Post("authentication/reset-password");
        AllowAnonymous();
    }

    public override async Task HandleAsync(ResetPasswordRequest req, CancellationToken ct)
    {
        var user = _userRepository.FindById(req.UserId);
        if (user == null)
        {
            _logger.LogInformation("Could not reset password since no user with user id {id} exists.", req.UserId);
            await Send.OkAsync(new SucceededOrNotResponse(false), ct);
            return;
        }

        var identityResult = await _userRepository.ResetUserPassword(user, req.Password, req.Token.Base64UrlDecode());
        if (!identityResult.Succeeded)
        {
            await Send.OkAsync(new SucceededOrNotResponse(false, identityResult.GetErrors()), ct);
            return;
        }

        var currentRefreshToken = HttpContext.GetCookieValue(CookieName.REFRESH);
        if (!string.IsNullOrWhiteSpace(currentRefreshToken))
            await _authenticationService.DeleteRefreshToken(currentRefreshToken);

        HttpContext.Response.DeleteCookieValue(CookieName.ACCESS, _cookieSettings.Domain, _cookieSettings.Secure);
        HttpContext.Response.DeleteCookieValue(CookieName.REFRESH, _cookieSettings.Domain, _cookieSettings.Secure);

        await Send.OkAsync(new SucceededOrNotResponse(true), ct);
    }
}
