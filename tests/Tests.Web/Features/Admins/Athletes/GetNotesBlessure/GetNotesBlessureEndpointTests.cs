using Domain.Constants.User;
using Domain.Entities;
using Domain.Repositories;
using FastEndpoints;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http;
using Web.Features.Admins.Athletes.GetNotesBlessure;

namespace Tests.Web.Features.Admins.Athletes.GetNotesBlessure;

public class GetNotesBlessureEndpointTests
{
    private readonly Mock<IAthleteRepository> _athleteRepository;
    private readonly Mock<INoteBlessureRepository> _noteBlessureRepository;
    private readonly GetNotesBlessureEndpoint _endpoint;

    public GetNotesBlessureEndpointTests()
    {
        _athleteRepository = new Mock<IAthleteRepository>();
        _noteBlessureRepository = new Mock<INoteBlessureRepository>();
        _endpoint = Factory.Create<GetNotesBlessureEndpoint>(
            _athleteRepository.Object,
            _noteBlessureRepository.Object
        );
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
        _endpoint.Definition.Routes.ShouldContain("athletes/{id}/notes-blessure");
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
    public async Task WhenHandleAsync_AndAthleteNotFound_ThenReturn404()
    {
        _athleteRepository.Setup(x => x.FindByIdAsync(It.IsAny<Guid>())).ReturnsAsync((Athlete?)null);
        var request = new GetNotesBlessureRequest { Id = Guid.NewGuid() };

        await _endpoint.HandleAsync(request, CancellationToken.None);

        _endpoint.HttpContext.Response.StatusCode.ShouldBe(StatusCodes.Status404NotFound);
    }

    [Fact]
    public async Task WhenHandleAsync_AndAthleteFound_ThenReturn200()
    {
        var athlete = new Athlete("Jean", "Tremblay", "jean@example.com");
        athlete.SetId(Guid.NewGuid());
        _athleteRepository.Setup(x => x.FindByIdAsync(athlete.Id)).ReturnsAsync(athlete);
        _noteBlessureRepository
            .Setup(x => x.GetByAthleteIdAsync(athlete.Id))
            .ReturnsAsync(new List<NoteBlessure>());
        var request = new GetNotesBlessureRequest { Id = athlete.Id };

        await _endpoint.HandleAsync(request, CancellationToken.None);

        _endpoint.HttpContext.Response.StatusCode.ShouldBe(StatusCodes.Status200OK);
    }

    [Fact]
    public async Task WhenHandleAsync_AndAthleteFound_ThenDelegateToRepository()
    {
        var athlete = new Athlete("Jean", "Tremblay", "jean@example.com");
        athlete.SetId(Guid.NewGuid());
        _athleteRepository.Setup(x => x.FindByIdAsync(athlete.Id)).ReturnsAsync(athlete);
        _noteBlessureRepository
            .Setup(x => x.GetByAthleteIdAsync(athlete.Id))
            .ReturnsAsync(new List<NoteBlessure>());
        var request = new GetNotesBlessureRequest { Id = athlete.Id };

        await _endpoint.HandleAsync(request, CancellationToken.None);

        _noteBlessureRepository.Verify(x => x.GetByAthleteIdAsync(athlete.Id));
    }
}
