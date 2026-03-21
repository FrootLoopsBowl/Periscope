using Domain.Constants.User;
using Domain.Entities;
using Domain.Repositories;
using FastEndpoints;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http;
using Web.Features.Admins.Athletes.CreateAthlete;
using Web.Features.Admins.Athletes.GetInjuredAthletes;

namespace Tests.Web.Features.Admins.Athletes.GetInjuredAthletes;

public class GetInjuredAthletesEndpointTests
{
    private readonly Mock<IAthleteRepository> _athleteRepository;
    private readonly GetInjuredAthletesEndpoint _endpoint;

    public GetInjuredAthletesEndpointTests()
    {
        _athleteRepository = new Mock<IAthleteRepository>();
        _endpoint = Factory.Create<GetInjuredAthletesEndpoint>(_athleteRepository.Object);
    }

    [Fact]
    public void WhenConfigure_ThenConfigureVerbToBeGet()
    {
        _endpoint.Configure();
        _endpoint.Definition.Verbs.ShouldContain(Http.GET.ToString());
    }

    [Fact]
    public void WhenConfigure_ThenConfigureRoute()
    {
        _endpoint.Configure();
        _endpoint.Definition.Routes.ShouldContain("athletes/injured");
    }

    [Fact]
    public void WhenConfigure_ThenConfigureAllowedRoles()
    {
        _endpoint.Configure();
        _endpoint.Definition.AllowedRoles!.ShouldContain(Roles.ADMINISTRATOR);
    }

    [Fact]
    public void WhenConfigure_ThenConfigureAuthSchemeToBeJwtBearer()
    {
        _endpoint.Configure();
        _endpoint.Definition.AuthSchemeNames!.ShouldContain(JwtBearerDefaults.AuthenticationScheme);
    }

    [Fact]
    public async Task WhenHandleAsync_ThenReturn200()
    {
        _athleteRepository
            .Setup(x => x.GetInjuredAsync())
            .ReturnsAsync(new List<Athlete>());

        await _endpoint.HandleAsync(CancellationToken.None);

        _endpoint.HttpContext.Response.StatusCode.ShouldBe(StatusCodes.Status200OK);
    }

    [Fact]
    public async Task WhenHandleAsync_ThenDelegateToRepository()
    {
        _athleteRepository
            .Setup(x => x.GetInjuredAsync())
            .ReturnsAsync(new List<Athlete>());

        await _endpoint.HandleAsync(CancellationToken.None);

        _athleteRepository.Verify(x => x.GetInjuredAsync());
    }

    [Fact]
    public async Task WhenHandleAsync_ThenReturnMappedAthletes()
    {
        var athlete = new Athlete("Jean", "Tremblay", "jean@example.com");
        athlete.SetId(Guid.NewGuid());
        athlete.SetIsInjured(true);
        _athleteRepository
            .Setup(x => x.GetInjuredAsync())
            .ReturnsAsync(new List<Athlete> { athlete });

        await _endpoint.HandleAsync(CancellationToken.None);

        _endpoint.Response.ShouldNotBeNull();
        _endpoint.Response.ShouldContain(r => r.Id == athlete.Id);
    }
}
