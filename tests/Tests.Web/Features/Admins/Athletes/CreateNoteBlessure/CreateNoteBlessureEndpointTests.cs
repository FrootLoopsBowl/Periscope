using Domain.Constants.User;
using Domain.Entities;
using Domain.Repositories;
using FastEndpoints;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http;
using Web.Features.Admins.Athletes.CreateNoteBlessure;

namespace Tests.Web.Features.Admins.Athletes.CreateNoteBlessure;

public class CreateNoteBlessureEndpointTests
{
    private readonly Mock<IAthleteRepository> _athleteRepository;
    private readonly Mock<INoteBlessureRepository> _noteBlessureRepository;
    private readonly CreateNoteBlessureEndpoint _endpoint;

    public CreateNoteBlessureEndpointTests()
    {
        _athleteRepository = new Mock<IAthleteRepository>();
        _noteBlessureRepository = new Mock<INoteBlessureRepository>();
        _endpoint = Factory.Create<CreateNoteBlessureEndpoint>(
            _athleteRepository.Object,
            _noteBlessureRepository.Object
        );
    }

    [Fact]
    public void WhenConfigure_ThenConfigureVerbToBePost()
    {
        _endpoint.Configure();
        _endpoint.Definition.Verbs.ShouldContain(Http.POST.ToString());
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
        var request = new CreateNoteBlessureRequest { Id = Guid.NewGuid(), Contenu = "Entorse" };

        await _endpoint.HandleAsync(request, CancellationToken.None);

        _endpoint.HttpContext.Response.StatusCode.ShouldBe(StatusCodes.Status404NotFound);
    }

    [Fact]
    public async Task WhenHandleAsync_AndAthleteFound_ThenReturn201()
    {
        var athlete = new Athlete("Jean", "Tremblay", "jean@example.com");
        athlete.SetId(Guid.NewGuid());
        _athleteRepository.Setup(x => x.FindByIdAsync(athlete.Id)).ReturnsAsync(athlete);
        var request = new CreateNoteBlessureRequest { Id = athlete.Id, Contenu = "Entorse" };

        await _endpoint.HandleAsync(request, CancellationToken.None);

        _endpoint.HttpContext.Response.StatusCode.ShouldBe(StatusCodes.Status201Created);
    }

    [Fact]
    public async Task WhenHandleAsync_AndAthleteFound_ThenDelegateToRepository()
    {
        var athlete = new Athlete("Jean", "Tremblay", "jean@example.com");
        athlete.SetId(Guid.NewGuid());
        _athleteRepository.Setup(x => x.FindByIdAsync(athlete.Id)).ReturnsAsync(athlete);
        var request = new CreateNoteBlessureRequest { Id = athlete.Id, Contenu = "Entorse" };

        await _endpoint.HandleAsync(request, CancellationToken.None);

        _noteBlessureRepository.Verify(x => x.CreateAsync(It.Is<NoteBlessure>(n =>
            n.AthleteId == athlete.Id && n.Contenu == "Entorse")));
    }
}
