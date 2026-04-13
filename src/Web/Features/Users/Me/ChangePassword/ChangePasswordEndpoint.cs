using Application.Interfaces.Services.Users;
using Application.Settings;
using Domain.Common;
using Domain.Extensions;
using FastEndpoints;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Options;
using Web.Cookies;

namespace Web.Features.Users.Me.ChangePassword;

public class ChangePasswordEndpoint : Endpoint<ChangePasswordRequest, SucceededOrNotResponse>
{
    private readonly CookieSettings _cookieSettings;
    private readonly IAuthenticationService _authenticationService;
    private readonly IAuthenticatedUserService _authenticatedUserService;

    public ChangePasswordEndpoint(
        IAuthenticatedUserService authenticatedUserService,
        IAuthenticationService authenticationService,
        IOptions<CookieSettings> cookieSettings)
    {
        _authenticatedUserService = authenticatedUserService;
        _authenticationService = authenticationService;
        _cookieSettings = cookieSettings.Value;
    }

    public override void Configure()
    {
        DontCatchExceptions();

        Post("users/me/change-password");
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
    }

    public override async Task HandleAsync(ChangePasswordRequest req, CancellationToken ct)
    {
        var identityResult = await _authenticatedUserService.ChangeUserPassword(req.CurrentPassword, req.NewPassword);
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
