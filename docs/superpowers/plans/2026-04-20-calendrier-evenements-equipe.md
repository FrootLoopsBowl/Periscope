# Calendrier d'événements par équipe — Plan d'implémentation

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Ajouter un calendrier vue mois style Google Calendar sur la page d'une équipe (CRUD admin) et en lecture seule sur le profil d'un athlète, avec deux types d'événements : Pratique et Match.

**Architecture:** Nouvel entity `TeamEvent` (backend C# / EF Core), 4 endpoints FastEndpoints (GET/POST/PUT/DELETE), nouveau service Vue 3 `TeamEventService`, deux composants `TeamCalendar.vue` (CRUD) et `AthleteCalendar.vue` (lecture seule) utilisant la librairie `vue-cal`. Les événements appartiennent à l'équipe ; les athlètes les voient via leur `teamId`.

**Tech Stack:** .NET 10 / FastEndpoints / EF Core / xUnit / Moq / Shouldly — Vue 3 / TypeScript / Inversify / Axios / vue-cal

---

## Structure des fichiers

| Fichier | Action |
|---|---|
| `src/Domain/Entities/EventType.cs` | Créer |
| `src/Domain/Entities/TeamEvent.cs` | Créer |
| `src/Domain/Repositories/ITeamEventRepository.cs` | Créer |
| `src/Infrastructure/Repositories/TeamEvents/TeamEventRepository.cs` | Créer |
| `src/Infrastructure/ConfigureServices.cs` | Modifier — ajouter `AddScoped<ITeamEventRepository, TeamEventRepository>()` |
| `src/Persistence/GarneauTemplateDbContext.cs` | Modifier — ajouter `DbSet<TeamEvent>` |
| `src/Persistence/Migrations/` | Nouvelle migration EF Core |
| `src/Web/Features/Admins/Teams/Events/GetTeamEvents/` | Créer (endpoint + request + response) |
| `src/Web/Features/Admins/Teams/Events/CreateTeamEvent/` | Créer (endpoint + request + validator) |
| `src/Web/Features/Admins/Teams/Events/UpdateTeamEvent/` | Créer (endpoint + request + validator) |
| `src/Web/Features/Admins/Teams/Events/DeleteTeamEvent/` | Créer (endpoint + request) |
| `tests/Tests.Domain/Entities/TeamEventTests.cs` | Créer |
| `tests/Tests.Web/Features/Admins/Teams/Events/CreateTeamEvent/` | Créer (endpoint + validator tests) |
| `tests/Tests.Web/Features/Admins/Teams/Events/UpdateTeamEvent/` | Créer (endpoint + validator tests) |
| `tests/Tests.Web/Features/Admins/Teams/Events/DeleteTeamEvent/` | Créer (endpoint tests) |
| `src/Web/vue-app/src/types/entities/teamEvent.ts` | Créer |
| `src/Web/vue-app/src/types/entities/index.ts` | Modifier |
| `src/Web/vue-app/src/types/requests/teamEventRequests.ts` | Créer |
| `src/Web/vue-app/src/types/requests/index.ts` | Modifier |
| `src/Web/vue-app/src/locales/fr.json` | Modifier |
| `src/Web/vue-app/src/locales/en.json` | Modifier |
| `src/Web/vue-app/src/injection/interfaces.ts` | Modifier |
| `src/Web/vue-app/src/injection/types.ts` | Modifier |
| `src/Web/vue-app/src/services/teamEventService.ts` | Créer |
| `src/Web/vue-app/src/services/index.ts` | Modifier |
| `src/Web/vue-app/src/inversify.config.ts` | Modifier |
| `src/Web/vue-app/src/components/calendar/TeamCalendar.vue` | Créer |
| `src/Web/vue-app/src/views/admin/teams/AdminTeamDetail.vue` | Modifier |
| `src/Web/vue-app/src/components/calendar/AthleteCalendar.vue` | Créer |
| `src/Web/vue-app/src/views/admin/athletes/AdminAthleteDetail.vue` | Modifier |

---

### Task 1 : Domain — EventType + TeamEvent entity

**Files:**
- Create: `src/Domain/Entities/EventType.cs`
- Create: `src/Domain/Entities/TeamEvent.cs`
- Create: `tests/Tests.Domain/Entities/TeamEventTests.cs`

- [ ] **Étape 1 : Écrire les tests qui échouent**

```csharp
// tests/Tests.Domain/Entities/TeamEventTests.cs
using Domain.Entities;

namespace Tests.Domain.Entities;

public class TeamEventTests
{
    private readonly Guid _teamId = Guid.NewGuid();
    private readonly DateTime _start = new DateTime(2026, 4, 9, 18, 0, 0, DateTimeKind.Utc);
    private readonly DateTime _end = new DateTime(2026, 4, 9, 19, 30, 0, DateTimeKind.Utc);

    [Fact]
    public void WhenConstructor_WithValidArgs_ThenCreatesTeamEvent()
    {
        var teamEvent = new TeamEvent(_teamId, EventType.Pratique, _start, _end);

        teamEvent.TeamId.ShouldBe(_teamId);
        teamEvent.Type.ShouldBe(EventType.Pratique);
        teamEvent.StartDateTime.ShouldBe(_start);
        teamEvent.EndDateTime.ShouldBe(_end);
    }

    [Fact]
    public void WhenConstructor_WithEmptyTeamId_ThenThrows()
    {
        Should.Throw<ArgumentException>(() =>
            new TeamEvent(Guid.Empty, EventType.Pratique, _start, _end));
    }

    [Fact]
    public void WhenConstructor_WithStartAfterEnd_ThenThrows()
    {
        Should.Throw<ArgumentException>(() =>
            new TeamEvent(_teamId, EventType.Pratique, _end, _start));
    }

    [Fact]
    public void WhenUpdate_WithValidArgs_ThenUpdatesFields()
    {
        var teamEvent = new TeamEvent(_teamId, EventType.Pratique, _start, _end);
        var newStart = new DateTime(2026, 4, 10, 14, 0, 0, DateTimeKind.Utc);
        var newEnd = new DateTime(2026, 4, 10, 16, 0, 0, DateTimeKind.Utc);

        teamEvent.Update(EventType.Match, newStart, newEnd);

        teamEvent.Type.ShouldBe(EventType.Match);
        teamEvent.StartDateTime.ShouldBe(newStart);
        teamEvent.EndDateTime.ShouldBe(newEnd);
    }

    [Fact]
    public void WhenUpdate_WithStartAfterEnd_ThenThrows()
    {
        var teamEvent = new TeamEvent(_teamId, EventType.Pratique, _start, _end);

        Should.Throw<ArgumentException>(() =>
            teamEvent.Update(EventType.Match, _end, _start));
    }
}
```

- [ ] **Étape 2 : Lancer les tests pour vérifier qu'ils échouent**

```bash
cd C:\Users\antho\Periscope\Periscope
dotnet test tests/Tests.Domain/Tests.Domain.csproj --filter "TeamEventTests"
```

Expected: compilation error — `TeamEvent` and `EventType` don't exist yet.

- [ ] **Étape 3 : Créer l'enum EventType**

```csharp
// src/Domain/Entities/EventType.cs
namespace Domain.Entities;

public enum EventType
{
    Pratique = 0,
    Match = 1
}
```

- [ ] **Étape 4 : Créer l'entité TeamEvent**

```csharp
// src/Domain/Entities/TeamEvent.cs
namespace Domain.Entities;

public class TeamEvent : Common.Entity
{
    public Guid TeamId { get; private set; }
    public Team Team { get; private set; } = null!;
    public EventType Type { get; private set; }
    public DateTime StartDateTime { get; private set; }
    public DateTime EndDateTime { get; private set; }
    public DateTime CreatedAt { get; private set; }

    public TeamEvent() { }

    public TeamEvent(Guid teamId, EventType type, DateTime startDateTime, DateTime endDateTime)
    {
        if (teamId == Guid.Empty)
            throw new ArgumentException("TeamId cannot be empty.", nameof(teamId));
        if (startDateTime >= endDateTime)
            throw new ArgumentException("StartDateTime must be before EndDateTime.", nameof(startDateTime));

        TeamId = teamId;
        Type = type;
        StartDateTime = startDateTime;
        EndDateTime = endDateTime;
        CreatedAt = DateTime.UtcNow;
    }

    public void Update(EventType type, DateTime startDateTime, DateTime endDateTime)
    {
        if (startDateTime >= endDateTime)
            throw new ArgumentException("StartDateTime must be before EndDateTime.", nameof(startDateTime));

        Type = type;
        StartDateTime = startDateTime;
        EndDateTime = endDateTime;
    }
}
```

- [ ] **Étape 5 : Lancer les tests pour vérifier qu'ils passent**

```bash
dotnet test tests/Tests.Domain/Tests.Domain.csproj --filter "TeamEventTests"
```

Expected: 5 tests PASS.

- [ ] **Étape 6 : Commit**

```bash
git add src/Domain/Entities/EventType.cs src/Domain/Entities/TeamEvent.cs tests/Tests.Domain/Entities/TeamEventTests.cs
git commit -m "feat: ajouter l'entité TeamEvent et l'enum EventType"
```

---

### Task 2 : Repository — interface + implémentation + DI + DbContext

**Files:**
- Create: `src/Domain/Repositories/ITeamEventRepository.cs`
- Create: `src/Infrastructure/Repositories/TeamEvents/TeamEventRepository.cs`
- Modify: `src/Infrastructure/ConfigureServices.cs`
- Modify: `src/Persistence/GarneauTemplateDbContext.cs`

- [ ] **Étape 1 : Créer l'interface du repository**

```csharp
// src/Domain/Repositories/ITeamEventRepository.cs
using Domain.Entities;

namespace Domain.Repositories;

public interface ITeamEventRepository
{
    Task CreateAsync(TeamEvent teamEvent);
    Task<IEnumerable<TeamEvent>> GetByTeamIdAndRangeAsync(Guid teamId, DateTime from, DateTime to);
    Task<TeamEvent?> FindByIdAsync(Guid id);
    Task UpdateAsync(TeamEvent teamEvent);
    Task DeleteAsync(TeamEvent teamEvent);
}
```

- [ ] **Étape 2 : Créer l'implémentation du repository**

```csharp
// src/Infrastructure/Repositories/TeamEvents/TeamEventRepository.cs
using Domain.Entities;
using Domain.Repositories;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Infrastructure.Repositories.TeamEvents;

public class TeamEventRepository : ITeamEventRepository
{
    private readonly GarneauTemplateDbContext _context;

    public TeamEventRepository(GarneauTemplateDbContext context)
    {
        _context = context;
    }

    public async Task CreateAsync(TeamEvent teamEvent)
    {
        _context.TeamEvents.Add(teamEvent);
        await _context.SaveChangesAsync();
    }

    public async Task<IEnumerable<TeamEvent>> GetByTeamIdAndRangeAsync(Guid teamId, DateTime from, DateTime to)
    {
        return await _context.TeamEvents
            .AsNoTracking()
            .Where(x => x.TeamId == teamId && x.StartDateTime >= from && x.StartDateTime <= to)
            .OrderBy(x => x.StartDateTime)
            .ToListAsync();
    }

    public async Task<TeamEvent?> FindByIdAsync(Guid id)
    {
        return await _context.TeamEvents.FindAsync(id);
    }

    public async Task UpdateAsync(TeamEvent teamEvent)
    {
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(TeamEvent teamEvent)
    {
        _context.TeamEvents.Remove(teamEvent);
        await _context.SaveChangesAsync();
    }
}
```

- [ ] **Étape 3 : Ajouter DbSet dans GarneauTemplateDbContext**

Dans `src/Persistence/GarneauTemplateDbContext.cs`, ajouter après la ligne `public DbSet<Team> Teams { get; set; } = null!;` :

```csharp
public DbSet<TeamEvent> TeamEvents { get; set; } = null!;
```

- [ ] **Étape 4 : Enregistrer le repository dans ConfigureServices**

Dans `src/Infrastructure/ConfigureServices.cs`, ajouter dans la méthode `ConfigureInfrastructureServices` après la ligne `services.AddScoped<ITeamRepository, TeamRepository>();` :

```csharp
services.AddScoped<ITeamEventRepository, TeamEventRepository>();
```

Et ajouter le using nécessaire en haut du fichier si absent :
```csharp
using Infrastructure.Repositories.TeamEvents;
```

- [ ] **Étape 5 : Vérifier que le projet compile**

```bash
dotnet build src/Web/Web.csproj
```

Expected: Build succeeded, 0 errors.

- [ ] **Étape 6 : Commit**

```bash
git add src/Domain/Repositories/ITeamEventRepository.cs src/Infrastructure/Repositories/TeamEvents/TeamEventRepository.cs src/Infrastructure/ConfigureServices.cs src/Persistence/GarneauTemplateDbContext.cs
git commit -m "feat: ajouter ITeamEventRepository, TeamEventRepository, enregistrement DI et DbSet"
```

---

### Task 3 : Migration EF Core

**Files:**
- New file in `src/Persistence/Migrations/`

- [ ] **Étape 1 : Générer la migration**

```bash
cd src/Persistence
dotnet ef migrations add AddTeamEvents --startup-project ../Web/
```

Expected: nouveau fichier migration créé dans `src/Persistence/Migrations/`.

- [ ] **Étape 2 : Vérifier le contenu de la migration**

Ouvrir le fichier de migration généré et vérifier qu'il contient :
- Une table `TeamEvents` avec les colonnes `Id` (Guid), `TeamId` (Guid, FK vers Teams), `Type` (int), `StartDateTime` (datetime2), `EndDateTime` (datetime2), `CreatedAt` (datetime2)
- Une foreign key vers la table `Teams`

- [ ] **Étape 3 : Appliquer la migration**

```bash
dotnet ef database update --startup-project ../Web/
```

Expected: `Done.`

- [ ] **Étape 4 : Commit**

```bash
cd ../..
git add src/Persistence/Migrations/
git commit -m "feat: migration EF Core pour la table TeamEvents"
```

---

### Task 4 : GET endpoint — GetTeamEventsEndpoint

**Files:**
- Create: `src/Web/Features/Admins/Teams/Events/GetTeamEvents/GetTeamEventsRequest.cs`
- Create: `src/Web/Features/Admins/Teams/Events/GetTeamEvents/TeamEventResponse.cs`
- Create: `src/Web/Features/Admins/Teams/Events/GetTeamEvents/GetTeamEventsEndpoint.cs`

- [ ] **Étape 1 : Créer la request**

```csharp
// src/Web/Features/Admins/Teams/Events/GetTeamEvents/GetTeamEventsRequest.cs
namespace Web.Features.Admins.Teams.Events.GetTeamEvents;

public class GetTeamEventsRequest
{
    public Guid TeamId { get; set; }
    public DateTime? From { get; set; }
    public DateTime? To { get; set; }
}
```

- [ ] **Étape 2 : Créer la response**

```csharp
// src/Web/Features/Admins/Teams/Events/GetTeamEvents/TeamEventResponse.cs
namespace Web.Features.Admins.Teams.Events.GetTeamEvents;

public class TeamEventResponse
{
    public Guid Id { get; set; }
    public string Type { get; set; } = null!;
    public DateTime StartDateTime { get; set; }
    public DateTime EndDateTime { get; set; }
}
```

- [ ] **Étape 3 : Créer l'endpoint**

```csharp
// src/Web/Features/Admins/Teams/Events/GetTeamEvents/GetTeamEventsEndpoint.cs
using Domain.Repositories;
using FastEndpoints;
using Microsoft.AspNetCore.Authentication.JwtBearer;

namespace Web.Features.Admins.Teams.Events.GetTeamEvents;

public class GetTeamEventsEndpoint : Endpoint<GetTeamEventsRequest, object>
{
    private readonly ITeamEventRepository _teamEventRepository;
    private readonly ITeamRepository _teamRepository;

    public GetTeamEventsEndpoint(ITeamEventRepository teamEventRepository, ITeamRepository teamRepository)
    {
        _teamEventRepository = teamEventRepository;
        _teamRepository = teamRepository;
    }

    public override void Configure()
    {
        DontCatchExceptions();
        Get("teams/{TeamId}/events");
        Roles(Domain.Constants.User.Roles.ADMINISTRATOR);
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
    }

    public override async Task HandleAsync(GetTeamEventsRequest req, CancellationToken ct)
    {
        var team = await _teamRepository.FindByIdAsync(req.TeamId);
        if (team is null)
        {
            await Send.NotFoundAsync(ct);
            return;
        }

        var from = req.From ?? DateTime.UtcNow.AddMonths(-1);
        var to = req.To ?? DateTime.UtcNow.AddMonths(1);

        var events = await _teamEventRepository.GetByTeamIdAndRangeAsync(req.TeamId, from, to);

        var response = events.Select(e => new TeamEventResponse
        {
            Id = e.Id,
            Type = e.Type.ToString(),
            StartDateTime = e.StartDateTime,
            EndDateTime = e.EndDateTime
        });

        await Send.OkAsync(response, cancellation: ct);
    }
}
```

- [ ] **Étape 4 : Vérifier que le projet compile**

```bash
dotnet build src/Web/Web.csproj
```

Expected: Build succeeded, 0 errors.

- [ ] **Étape 5 : Commit**

```bash
git add src/Web/Features/Admins/Teams/Events/GetTeamEvents/
git commit -m "feat: ajouter GetTeamEventsEndpoint"
```

---

### Task 5 : CREATE endpoint — CreateTeamEventEndpoint

**Files:**
- Create: `src/Web/Features/Admins/Teams/Events/CreateTeamEvent/CreateTeamEventRequest.cs`
- Create: `src/Web/Features/Admins/Teams/Events/CreateTeamEvent/CreateTeamEventValidator.cs`
- Create: `src/Web/Features/Admins/Teams/Events/CreateTeamEvent/CreateTeamEventEndpoint.cs`
- Create: `tests/Tests.Web/Features/Admins/Teams/Events/CreateTeamEvent/CreateTeamEventEndpointTests.cs`
- Create: `tests/Tests.Web/Features/Admins/Teams/Events/CreateTeamEvent/CreateTeamEventValidatorTests.cs`

- [ ] **Étape 1 : Écrire les tests qui échouent**

```csharp
// tests/Tests.Web/Features/Admins/Teams/Events/CreateTeamEvent/CreateTeamEventEndpointTests.cs
using Domain.Constants.User;
using Domain.Entities;
using Domain.Repositories;
using FastEndpoints;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http;
using Web.Features.Admins.Teams.Events.CreateTeamEvent;

namespace Tests.Web.Features.Admins.Teams.Events.CreateTeamEvent;

public class CreateTeamEventEndpointTests
{
    private readonly Mock<ITeamEventRepository> _teamEventRepository;
    private readonly Mock<ITeamRepository> _teamRepository;
    private readonly CreateTeamEventEndpoint _endpoint;

    public CreateTeamEventEndpointTests()
    {
        _teamEventRepository = new Mock<ITeamEventRepository>();
        _teamRepository = new Mock<ITeamRepository>();
        _endpoint = Factory.Create<CreateTeamEventEndpoint>(
            _teamEventRepository.Object,
            _teamRepository.Object
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
        _endpoint.Definition.Routes.ShouldContain("teams/{TeamId}/events");
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
    public async Task WhenHandleAsync_AndTeamNotFound_ThenReturn404()
    {
        _teamRepository.Setup(x => x.FindByIdAsync(It.IsAny<Guid>())).ReturnsAsync((Team?)null);
        var request = new CreateTeamEventRequest
        {
            TeamId = Guid.NewGuid(),
            Type = "Pratique",
            StartDateTime = new DateTime(2026, 4, 9, 18, 0, 0, DateTimeKind.Utc),
            EndDateTime = new DateTime(2026, 4, 9, 19, 30, 0, DateTimeKind.Utc)
        };

        await _endpoint.HandleAsync(request, CancellationToken.None);

        _endpoint.HttpContext.Response.StatusCode.ShouldBe(StatusCodes.Status404NotFound);
    }

    [Fact]
    public async Task WhenHandleAsync_AndTeamFound_ThenReturn201()
    {
        var team = new Team("Équipe A");
        team.SetId(Guid.NewGuid());
        _teamRepository.Setup(x => x.FindByIdAsync(team.Id)).ReturnsAsync(team);
        var request = new CreateTeamEventRequest
        {
            TeamId = team.Id,
            Type = "Pratique",
            StartDateTime = new DateTime(2026, 4, 9, 18, 0, 0, DateTimeKind.Utc),
            EndDateTime = new DateTime(2026, 4, 9, 19, 30, 0, DateTimeKind.Utc)
        };

        await _endpoint.HandleAsync(request, CancellationToken.None);

        _endpoint.HttpContext.Response.StatusCode.ShouldBe(StatusCodes.Status201Created);
    }

    [Fact]
    public async Task WhenHandleAsync_AndTeamFound_ThenDelegateToRepository()
    {
        var team = new Team("Équipe A");
        team.SetId(Guid.NewGuid());
        _teamRepository.Setup(x => x.FindByIdAsync(team.Id)).ReturnsAsync(team);
        var request = new CreateTeamEventRequest
        {
            TeamId = team.Id,
            Type = "Pratique",
            StartDateTime = new DateTime(2026, 4, 9, 18, 0, 0, DateTimeKind.Utc),
            EndDateTime = new DateTime(2026, 4, 9, 19, 30, 0, DateTimeKind.Utc)
        };

        await _endpoint.HandleAsync(request, CancellationToken.None);

        _teamEventRepository.Verify(x => x.CreateAsync(It.Is<TeamEvent>(e =>
            e.TeamId == team.Id &&
            e.Type == EventType.Pratique)));
    }
}
```

```csharp
// tests/Tests.Web/Features/Admins/Teams/Events/CreateTeamEvent/CreateTeamEventValidatorTests.cs
using FluentValidation.TestHelper;
using Web.Features.Admins.Teams.Events.CreateTeamEvent;

namespace Tests.Web.Features.Admins.Teams.Events.CreateTeamEvent;

public class CreateTeamEventValidatorTests
{
    private readonly CreateTeamEventRequest _request = new()
    {
        TeamId = Guid.NewGuid(),
        Type = "Pratique",
        StartDateTime = new DateTime(2026, 4, 9, 18, 0, 0, DateTimeKind.Utc),
        EndDateTime = new DateTime(2026, 4, 9, 19, 30, 0, DateTimeKind.Utc)
    };
    private readonly CreateTeamEventValidator _validator = new();

    [Fact]
    public void GivenValidRequest_WhenValidate_ThenReturnNoErrors()
    {
        var result = _validator.TestValidate(_request);
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Theory]
    [InlineData("")]
    [InlineData("InvalidType")]
    [InlineData("pratique")]
    public void GivenInvalidType_WhenValidate_ThenReturnError(string type)
    {
        _request.Type = type;
        var result = _validator.TestValidate(_request);
        result.ShouldHaveValidationErrorFor(x => x.Type);
    }

    [Fact]
    public void GivenStartAfterEnd_WhenValidate_ThenReturnError()
    {
        _request.StartDateTime = new DateTime(2026, 4, 9, 20, 0, 0, DateTimeKind.Utc);
        _request.EndDateTime = new DateTime(2026, 4, 9, 18, 0, 0, DateTimeKind.Utc);
        var result = _validator.TestValidate(_request);
        result.ShouldHaveValidationErrorFor(x => x.StartDateTime);
    }
}
```

- [ ] **Étape 2 : Lancer les tests pour vérifier qu'ils échouent**

```bash
dotnet test tests/Tests.Web/Tests.Web.csproj --filter "CreateTeamEvent"
```

Expected: compilation error.

- [ ] **Étape 3 : Créer la request**

```csharp
// src/Web/Features/Admins/Teams/Events/CreateTeamEvent/CreateTeamEventRequest.cs
namespace Web.Features.Admins.Teams.Events.CreateTeamEvent;

public class CreateTeamEventRequest
{
    public Guid TeamId { get; set; }
    public string Type { get; set; } = null!;
    public DateTime StartDateTime { get; set; }
    public DateTime EndDateTime { get; set; }
}
```

- [ ] **Étape 4 : Créer le validator**

```csharp
// src/Web/Features/Admins/Teams/Events/CreateTeamEvent/CreateTeamEventValidator.cs
using FastEndpoints;
using FluentValidation;

namespace Web.Features.Admins.Teams.Events.CreateTeamEvent;

public class CreateTeamEventValidator : Validator<CreateTeamEventRequest>
{
    public CreateTeamEventValidator()
    {
        RuleFor(x => x.Type)
            .Must(t => t == "Pratique" || t == "Match")
            .WithErrorCode("InvalidType")
            .WithMessage("Type must be 'Pratique' or 'Match'.");

        RuleFor(x => x.StartDateTime)
            .Must((req, start) => start < req.EndDateTime)
            .WithErrorCode("InvalidDateRange")
            .WithMessage("StartDateTime must be before EndDateTime.");
    }
}
```

- [ ] **Étape 5 : Créer l'endpoint**

```csharp
// src/Web/Features/Admins/Teams/Events/CreateTeamEvent/CreateTeamEventEndpoint.cs
using Domain.Entities;
using Domain.Repositories;
using FastEndpoints;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Web.Features.Admins.Teams.Events.GetTeamEvents;

namespace Web.Features.Admins.Teams.Events.CreateTeamEvent;

public class CreateTeamEventEndpoint : Endpoint<CreateTeamEventRequest, TeamEventResponse>
{
    private readonly ITeamEventRepository _teamEventRepository;
    private readonly ITeamRepository _teamRepository;

    public CreateTeamEventEndpoint(ITeamEventRepository teamEventRepository, ITeamRepository teamRepository)
    {
        _teamEventRepository = teamEventRepository;
        _teamRepository = teamRepository;
    }

    public override void Configure()
    {
        DontCatchExceptions();
        Post("teams/{TeamId}/events");
        Roles(Domain.Constants.User.Roles.ADMINISTRATOR);
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
    }

    public override async Task HandleAsync(CreateTeamEventRequest req, CancellationToken ct)
    {
        var team = await _teamRepository.FindByIdAsync(req.TeamId);
        if (team is null)
        {
            await Send.NotFoundAsync(ct);
            return;
        }

        var type = Enum.Parse<EventType>(req.Type);
        var teamEvent = new TeamEvent(req.TeamId, type, req.StartDateTime, req.EndDateTime);
        await _teamEventRepository.CreateAsync(teamEvent);

        var response = new TeamEventResponse
        {
            Id = teamEvent.Id,
            Type = teamEvent.Type.ToString(),
            StartDateTime = teamEvent.StartDateTime,
            EndDateTime = teamEvent.EndDateTime
        };

        HttpContext.Response.StatusCode = StatusCodes.Status201Created;
        await HttpContext.Response.WriteAsJsonAsync(response, ct);
    }
}
```

- [ ] **Étape 6 : Lancer les tests pour vérifier qu'ils passent**

```bash
dotnet test tests/Tests.Web/Tests.Web.csproj --filter "CreateTeamEvent"
```

Expected: tous les tests PASS.

- [ ] **Étape 7 : Commit**

```bash
git add src/Web/Features/Admins/Teams/Events/CreateTeamEvent/ tests/Tests.Web/Features/Admins/Teams/Events/CreateTeamEvent/
git commit -m "feat: ajouter CreateTeamEventEndpoint avec tests"
```

---

### Task 6 : UPDATE endpoint — UpdateTeamEventEndpoint

**Files:**
- Create: `src/Web/Features/Admins/Teams/Events/UpdateTeamEvent/UpdateTeamEventRequest.cs`
- Create: `src/Web/Features/Admins/Teams/Events/UpdateTeamEvent/UpdateTeamEventValidator.cs`
- Create: `src/Web/Features/Admins/Teams/Events/UpdateTeamEvent/UpdateTeamEventEndpoint.cs`
- Create: `tests/Tests.Web/Features/Admins/Teams/Events/UpdateTeamEvent/UpdateTeamEventEndpointTests.cs`
- Create: `tests/Tests.Web/Features/Admins/Teams/Events/UpdateTeamEvent/UpdateTeamEventValidatorTests.cs`

- [ ] **Étape 1 : Écrire les tests qui échouent**

```csharp
// tests/Tests.Web/Features/Admins/Teams/Events/UpdateTeamEvent/UpdateTeamEventEndpointTests.cs
using Domain.Constants.User;
using Domain.Entities;
using Domain.Repositories;
using FastEndpoints;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http;
using Web.Features.Admins.Teams.Events.UpdateTeamEvent;

namespace Tests.Web.Features.Admins.Teams.Events.UpdateTeamEvent;

public class UpdateTeamEventEndpointTests
{
    private readonly Mock<ITeamEventRepository> _teamEventRepository;
    private readonly UpdateTeamEventEndpoint _endpoint;

    public UpdateTeamEventEndpointTests()
    {
        _teamEventRepository = new Mock<ITeamEventRepository>();
        _endpoint = Factory.Create<UpdateTeamEventEndpoint>(_teamEventRepository.Object);
    }

    [Fact]
    public void WhenConfigure_ThenConfigureVerbToBePut()
    {
        _endpoint.Configure();
        _endpoint.Definition.Verbs.ShouldContain(Http.PUT.ToString());
    }

    [Fact]
    public void WhenConfigure_ThenConfigureRoute()
    {
        _endpoint.Configure();
        _endpoint.Definition.Routes.ShouldContain("teams/{TeamId}/events/{EventId}");
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
    public async Task WhenHandleAsync_AndEventNotFound_ThenReturn404()
    {
        _teamEventRepository.Setup(x => x.FindByIdAsync(It.IsAny<Guid>())).ReturnsAsync((TeamEvent?)null);
        var request = new UpdateTeamEventRequest
        {
            TeamId = Guid.NewGuid(),
            EventId = Guid.NewGuid(),
            Type = "Match",
            StartDateTime = new DateTime(2026, 4, 9, 18, 0, 0, DateTimeKind.Utc),
            EndDateTime = new DateTime(2026, 4, 9, 19, 30, 0, DateTimeKind.Utc)
        };

        await _endpoint.HandleAsync(request, CancellationToken.None);

        _endpoint.HttpContext.Response.StatusCode.ShouldBe(StatusCodes.Status404NotFound);
    }

    [Fact]
    public async Task WhenHandleAsync_AndEventBelongsToDifferentTeam_ThenReturn404()
    {
        var teamId = Guid.NewGuid();
        var differentTeamId = Guid.NewGuid();
        var teamEvent = new TeamEvent(differentTeamId, EventType.Pratique,
            new DateTime(2026, 4, 9, 18, 0, 0, DateTimeKind.Utc),
            new DateTime(2026, 4, 9, 19, 30, 0, DateTimeKind.Utc));
        teamEvent.SetId(Guid.NewGuid());
        _teamEventRepository.Setup(x => x.FindByIdAsync(teamEvent.Id)).ReturnsAsync(teamEvent);

        var request = new UpdateTeamEventRequest
        {
            TeamId = teamId,
            EventId = teamEvent.Id,
            Type = "Match",
            StartDateTime = new DateTime(2026, 4, 10, 14, 0, 0, DateTimeKind.Utc),
            EndDateTime = new DateTime(2026, 4, 10, 16, 0, 0, DateTimeKind.Utc)
        };

        await _endpoint.HandleAsync(request, CancellationToken.None);

        _endpoint.HttpContext.Response.StatusCode.ShouldBe(StatusCodes.Status404NotFound);
    }

    [Fact]
    public async Task WhenHandleAsync_AndEventFound_ThenReturn204()
    {
        var teamId = Guid.NewGuid();
        var teamEvent = new TeamEvent(teamId, EventType.Pratique,
            new DateTime(2026, 4, 9, 18, 0, 0, DateTimeKind.Utc),
            new DateTime(2026, 4, 9, 19, 30, 0, DateTimeKind.Utc));
        teamEvent.SetId(Guid.NewGuid());
        _teamEventRepository.Setup(x => x.FindByIdAsync(teamEvent.Id)).ReturnsAsync(teamEvent);

        var request = new UpdateTeamEventRequest
        {
            TeamId = teamId,
            EventId = teamEvent.Id,
            Type = "Match",
            StartDateTime = new DateTime(2026, 4, 10, 14, 0, 0, DateTimeKind.Utc),
            EndDateTime = new DateTime(2026, 4, 10, 16, 0, 0, DateTimeKind.Utc)
        };

        await _endpoint.HandleAsync(request, CancellationToken.None);

        _endpoint.HttpContext.Response.StatusCode.ShouldBe(StatusCodes.Status204NoContent);
    }
}
```

```csharp
// tests/Tests.Web/Features/Admins/Teams/Events/UpdateTeamEvent/UpdateTeamEventValidatorTests.cs
using FluentValidation.TestHelper;
using Web.Features.Admins.Teams.Events.UpdateTeamEvent;

namespace Tests.Web.Features.Admins.Teams.Events.UpdateTeamEvent;

public class UpdateTeamEventValidatorTests
{
    private readonly UpdateTeamEventRequest _request = new()
    {
        TeamId = Guid.NewGuid(),
        EventId = Guid.NewGuid(),
        Type = "Match",
        StartDateTime = new DateTime(2026, 4, 9, 14, 0, 0, DateTimeKind.Utc),
        EndDateTime = new DateTime(2026, 4, 9, 16, 0, 0, DateTimeKind.Utc)
    };
    private readonly UpdateTeamEventValidator _validator = new();

    [Fact]
    public void GivenValidRequest_WhenValidate_ThenReturnNoErrors()
    {
        var result = _validator.TestValidate(_request);
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Theory]
    [InlineData("")]
    [InlineData("InvalidType")]
    public void GivenInvalidType_WhenValidate_ThenReturnError(string type)
    {
        _request.Type = type;
        var result = _validator.TestValidate(_request);
        result.ShouldHaveValidationErrorFor(x => x.Type);
    }

    [Fact]
    public void GivenStartAfterEnd_WhenValidate_ThenReturnError()
    {
        _request.StartDateTime = new DateTime(2026, 4, 9, 20, 0, 0, DateTimeKind.Utc);
        _request.EndDateTime = new DateTime(2026, 4, 9, 18, 0, 0, DateTimeKind.Utc);
        var result = _validator.TestValidate(_request);
        result.ShouldHaveValidationErrorFor(x => x.StartDateTime);
    }
}
```

- [ ] **Étape 2 : Lancer les tests pour vérifier qu'ils échouent**

```bash
dotnet test tests/Tests.Web/Tests.Web.csproj --filter "UpdateTeamEvent"
```

Expected: compilation error.

- [ ] **Étape 3 : Créer la request**

```csharp
// src/Web/Features/Admins/Teams/Events/UpdateTeamEvent/UpdateTeamEventRequest.cs
namespace Web.Features.Admins.Teams.Events.UpdateTeamEvent;

public class UpdateTeamEventRequest
{
    public Guid TeamId { get; set; }
    public Guid EventId { get; set; }
    public string Type { get; set; } = null!;
    public DateTime StartDateTime { get; set; }
    public DateTime EndDateTime { get; set; }
}
```

- [ ] **Étape 4 : Créer le validator**

```csharp
// src/Web/Features/Admins/Teams/Events/UpdateTeamEvent/UpdateTeamEventValidator.cs
using FastEndpoints;
using FluentValidation;

namespace Web.Features.Admins.Teams.Events.UpdateTeamEvent;

public class UpdateTeamEventValidator : Validator<UpdateTeamEventRequest>
{
    public UpdateTeamEventValidator()
    {
        RuleFor(x => x.Type)
            .Must(t => t == "Pratique" || t == "Match")
            .WithErrorCode("InvalidType")
            .WithMessage("Type must be 'Pratique' or 'Match'.");

        RuleFor(x => x.StartDateTime)
            .Must((req, start) => start < req.EndDateTime)
            .WithErrorCode("InvalidDateRange")
            .WithMessage("StartDateTime must be before EndDateTime.");
    }
}
```

- [ ] **Étape 5 : Créer l'endpoint**

```csharp
// src/Web/Features/Admins/Teams/Events/UpdateTeamEvent/UpdateTeamEventEndpoint.cs
using Domain.Entities;
using Domain.Repositories;
using FastEndpoints;
using Microsoft.AspNetCore.Authentication.JwtBearer;

namespace Web.Features.Admins.Teams.Events.UpdateTeamEvent;

public class UpdateTeamEventEndpoint : Endpoint<UpdateTeamEventRequest, EmptyResponse>
{
    private readonly ITeamEventRepository _teamEventRepository;

    public UpdateTeamEventEndpoint(ITeamEventRepository teamEventRepository)
    {
        _teamEventRepository = teamEventRepository;
    }

    public override void Configure()
    {
        DontCatchExceptions();
        Put("teams/{TeamId}/events/{EventId}");
        Roles(Domain.Constants.User.Roles.ADMINISTRATOR);
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
    }

    public override async Task HandleAsync(UpdateTeamEventRequest req, CancellationToken ct)
    {
        var teamEvent = await _teamEventRepository.FindByIdAsync(req.EventId);
        if (teamEvent is null || teamEvent.TeamId != req.TeamId)
        {
            await Send.NotFoundAsync(ct);
            return;
        }

        var type = Enum.Parse<EventType>(req.Type);
        teamEvent.Update(type, req.StartDateTime, req.EndDateTime);
        await _teamEventRepository.UpdateAsync(teamEvent);

        await Send.NoContentAsync(ct);
    }
}
```

- [ ] **Étape 6 : Lancer les tests pour vérifier qu'ils passent**

```bash
dotnet test tests/Tests.Web/Tests.Web.csproj --filter "UpdateTeamEvent"
```

Expected: tous les tests PASS.

- [ ] **Étape 7 : Commit**

```bash
git add src/Web/Features/Admins/Teams/Events/UpdateTeamEvent/ tests/Tests.Web/Features/Admins/Teams/Events/UpdateTeamEvent/
git commit -m "feat: ajouter UpdateTeamEventEndpoint avec tests"
```

---

### Task 7 : DELETE endpoint — DeleteTeamEventEndpoint

**Files:**
- Create: `src/Web/Features/Admins/Teams/Events/DeleteTeamEvent/DeleteTeamEventRequest.cs`
- Create: `src/Web/Features/Admins/Teams/Events/DeleteTeamEvent/DeleteTeamEventEndpoint.cs`
- Create: `tests/Tests.Web/Features/Admins/Teams/Events/DeleteTeamEvent/DeleteTeamEventEndpointTests.cs`

- [ ] **Étape 1 : Écrire les tests qui échouent**

```csharp
// tests/Tests.Web/Features/Admins/Teams/Events/DeleteTeamEvent/DeleteTeamEventEndpointTests.cs
using Domain.Constants.User;
using Domain.Entities;
using Domain.Repositories;
using FastEndpoints;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http;
using Web.Features.Admins.Teams.Events.DeleteTeamEvent;

namespace Tests.Web.Features.Admins.Teams.Events.DeleteTeamEvent;

public class DeleteTeamEventEndpointTests
{
    private readonly Mock<ITeamEventRepository> _teamEventRepository;
    private readonly DeleteTeamEventEndpoint _endpoint;

    public DeleteTeamEventEndpointTests()
    {
        _teamEventRepository = new Mock<ITeamEventRepository>();
        _endpoint = Factory.Create<DeleteTeamEventEndpoint>(_teamEventRepository.Object);
    }

    [Fact]
    public void WhenConfigure_ThenConfigureVerbToBeDelete()
    {
        _endpoint.Configure();
        _endpoint.Definition.Verbs.ShouldContain(Http.DELETE.ToString());
    }

    [Fact]
    public void WhenConfigure_ThenConfigureRoute()
    {
        _endpoint.Configure();
        _endpoint.Definition.Routes.ShouldContain("teams/{TeamId}/events/{EventId}");
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
    public async Task WhenHandleAsync_AndEventNotFound_ThenReturn404()
    {
        _teamEventRepository.Setup(x => x.FindByIdAsync(It.IsAny<Guid>())).ReturnsAsync((TeamEvent?)null);
        var request = new DeleteTeamEventRequest { TeamId = Guid.NewGuid(), EventId = Guid.NewGuid() };

        await _endpoint.HandleAsync(request, CancellationToken.None);

        _endpoint.HttpContext.Response.StatusCode.ShouldBe(StatusCodes.Status404NotFound);
    }

    [Fact]
    public async Task WhenHandleAsync_AndEventBelongsToDifferentTeam_ThenReturn404()
    {
        var differentTeamId = Guid.NewGuid();
        var teamEvent = new TeamEvent(differentTeamId, EventType.Pratique,
            new DateTime(2026, 4, 9, 18, 0, 0, DateTimeKind.Utc),
            new DateTime(2026, 4, 9, 19, 30, 0, DateTimeKind.Utc));
        teamEvent.SetId(Guid.NewGuid());
        _teamEventRepository.Setup(x => x.FindByIdAsync(teamEvent.Id)).ReturnsAsync(teamEvent);

        var request = new DeleteTeamEventRequest { TeamId = Guid.NewGuid(), EventId = teamEvent.Id };

        await _endpoint.HandleAsync(request, CancellationToken.None);

        _endpoint.HttpContext.Response.StatusCode.ShouldBe(StatusCodes.Status404NotFound);
    }

    [Fact]
    public async Task WhenHandleAsync_AndEventFound_ThenReturn204()
    {
        var teamId = Guid.NewGuid();
        var teamEvent = new TeamEvent(teamId, EventType.Pratique,
            new DateTime(2026, 4, 9, 18, 0, 0, DateTimeKind.Utc),
            new DateTime(2026, 4, 9, 19, 30, 0, DateTimeKind.Utc));
        teamEvent.SetId(Guid.NewGuid());
        _teamEventRepository.Setup(x => x.FindByIdAsync(teamEvent.Id)).ReturnsAsync(teamEvent);

        var request = new DeleteTeamEventRequest { TeamId = teamId, EventId = teamEvent.Id };

        await _endpoint.HandleAsync(request, CancellationToken.None);

        _endpoint.HttpContext.Response.StatusCode.ShouldBe(StatusCodes.Status204NoContent);
    }

    [Fact]
    public async Task WhenHandleAsync_AndEventFound_ThenDelegateToRepository()
    {
        var teamId = Guid.NewGuid();
        var teamEvent = new TeamEvent(teamId, EventType.Pratique,
            new DateTime(2026, 4, 9, 18, 0, 0, DateTimeKind.Utc),
            new DateTime(2026, 4, 9, 19, 30, 0, DateTimeKind.Utc));
        teamEvent.SetId(Guid.NewGuid());
        _teamEventRepository.Setup(x => x.FindByIdAsync(teamEvent.Id)).ReturnsAsync(teamEvent);

        var request = new DeleteTeamEventRequest { TeamId = teamId, EventId = teamEvent.Id };

        await _endpoint.HandleAsync(request, CancellationToken.None);

        _teamEventRepository.Verify(x => x.DeleteAsync(teamEvent));
    }
}
```

- [ ] **Étape 2 : Lancer les tests pour vérifier qu'ils échouent**

```bash
dotnet test tests/Tests.Web/Tests.Web.csproj --filter "DeleteTeamEvent"
```

Expected: compilation error.

- [ ] **Étape 3 : Créer la request**

```csharp
// src/Web/Features/Admins/Teams/Events/DeleteTeamEvent/DeleteTeamEventRequest.cs
namespace Web.Features.Admins.Teams.Events.DeleteTeamEvent;

public class DeleteTeamEventRequest
{
    public Guid TeamId { get; set; }
    public Guid EventId { get; set; }
}
```

- [ ] **Étape 4 : Créer l'endpoint**

```csharp
// src/Web/Features/Admins/Teams/Events/DeleteTeamEvent/DeleteTeamEventEndpoint.cs
using Domain.Repositories;
using FastEndpoints;
using Microsoft.AspNetCore.Authentication.JwtBearer;

namespace Web.Features.Admins.Teams.Events.DeleteTeamEvent;

public class DeleteTeamEventEndpoint : Endpoint<DeleteTeamEventRequest, EmptyResponse>
{
    private readonly ITeamEventRepository _teamEventRepository;

    public DeleteTeamEventEndpoint(ITeamEventRepository teamEventRepository)
    {
        _teamEventRepository = teamEventRepository;
    }

    public override void Configure()
    {
        DontCatchExceptions();
        Delete("teams/{TeamId}/events/{EventId}");
        Roles(Domain.Constants.User.Roles.ADMINISTRATOR);
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
    }

    public override async Task HandleAsync(DeleteTeamEventRequest req, CancellationToken ct)
    {
        var teamEvent = await _teamEventRepository.FindByIdAsync(req.EventId);
        if (teamEvent is null || teamEvent.TeamId != req.TeamId)
        {
            await Send.NotFoundAsync(ct);
            return;
        }

        await _teamEventRepository.DeleteAsync(teamEvent);
        await Send.NoContentAsync(ct);
    }
}
```

- [ ] **Étape 5 : Lancer les tests pour vérifier qu'ils passent**

```bash
dotnet test tests/Tests.Web/Tests.Web.csproj --filter "DeleteTeamEvent"
```

Expected: tous les tests PASS.

- [ ] **Étape 6 : Lancer tous les tests backend pour vérifier qu'il n'y a pas de régression**

```bash
dotnet test tests/Tests.Web/Tests.Web.csproj
```

Expected: tous les tests PASS.

- [ ] **Étape 7 : Commit**

```bash
git add src/Web/Features/Admins/Teams/Events/DeleteTeamEvent/ tests/Tests.Web/Features/Admins/Teams/Events/DeleteTeamEvent/
git commit -m "feat: ajouter DeleteTeamEventEndpoint avec tests"
```

---

### Task 8 : Frontend — types, i18n, installation vue-cal

**Files:**
- Create: `src/Web/vue-app/src/types/entities/teamEvent.ts`
- Modify: `src/Web/vue-app/src/types/entities/index.ts`
- Create: `src/Web/vue-app/src/types/requests/teamEventRequests.ts`
- Modify: `src/Web/vue-app/src/types/requests/index.ts`
- Modify: `src/Web/vue-app/src/locales/fr.json`
- Modify: `src/Web/vue-app/src/locales/en.json`

- [ ] **Étape 1 : Installer vue-cal**

```bash
cd src/Web/vue-app
npm install vue-cal
```

Expected: `vue-cal` ajouté dans `package.json` et `node_modules`.

- [ ] **Étape 2 : Créer le type TeamEvent**

```typescript
// src/Web/vue-app/src/types/entities/teamEvent.ts
export class TeamEvent {
  id?: string
  type?: string
  startDateTime?: string
  endDateTime?: string
}
```

- [ ] **Étape 3 : Exporter TeamEvent depuis l'index**

Dans `src/Web/vue-app/src/types/entities/index.ts`, ajouter à la fin :

```typescript
export * from "./teamEvent"
```

- [ ] **Étape 4 : Créer les interfaces de requête**

```typescript
// src/Web/vue-app/src/types/requests/teamEventRequests.ts
export interface ICreateTeamEventRequest {
  type: string
  startDateTime: string
  endDateTime: string
}

export interface IUpdateTeamEventRequest {
  type: string
  startDateTime: string
  endDateTime: string
}
```

- [ ] **Étape 5 : Exporter depuis l'index des requests**

Dans `src/Web/vue-app/src/types/requests/index.ts`, ajouter à la fin :

```typescript
export * from './teamEventRequests'
```

- [ ] **Étape 6 : Ajouter les clés i18n dans fr.json**

Dans `src/Web/vue-app/src/locales/fr.json`, dans la section `"pages"` → `"teams"`, après le bloc `"edit"` et avant la fermeture `}` de `"teams"`, ajouter :

```json
"calendar": {
  "title": "Calendrier",
  "addEvent": "Ajouter un événement",
  "editEvent": "Modifier l'événement",
  "deleteEvent": "Supprimer",
  "typeLabel": "Type",
  "typePratique": "Pratique",
  "typeMatch": "Match",
  "dateLabel": "Date",
  "startLabel": "Heure de début",
  "endLabel": "Heure de fin",
  "saveSuccess": "L'événement a été sauvegardé.",
  "saveError": "Erreur lors de la sauvegarde de l'événement.",
  "deleteSuccess": "L'événement a été supprimé.",
  "deleteError": "Erreur lors de la suppression de l'événement."
}
```

Et dans la section `"pages"` → `"athletes"`, après le bloc `"import"`, ajouter :

```json
"calendar": {
  "title": "Calendrier de l'équipe",
  "noTeam": "Cet athlète n'appartient à aucune équipe."
}
```

- [ ] **Étape 7 : Ajouter les clés i18n dans en.json**

Dans `src/Web/vue-app/src/locales/en.json`, dans la section `"pages"` → `"teams"`, après le bloc `"edit"` et avant la fermeture `}` de `"teams"`, ajouter :

```json
"calendar": {
  "title": "Calendar",
  "addEvent": "Add event",
  "editEvent": "Edit event",
  "deleteEvent": "Delete",
  "typeLabel": "Type",
  "typePratique": "Practice",
  "typeMatch": "Match",
  "dateLabel": "Date",
  "startLabel": "Start time",
  "endLabel": "End time",
  "saveSuccess": "Event saved successfully.",
  "saveError": "Error saving the event.",
  "deleteSuccess": "Event deleted successfully.",
  "deleteError": "Error deleting the event."
}
```

Et dans la section `"pages"` → `"athletes"`, après le bloc `"import"`, ajouter :

```json
"calendar": {
  "title": "Team calendar",
  "noTeam": "This athlete does not belong to any team."
}
```

- [ ] **Étape 8 : Vérifier que le frontend compile**

```bash
cd src/Web/vue-app
npm run build
```

Expected: build réussi, 0 erreurs TypeScript.

- [ ] **Étape 9 : Commit**

```bash
cd ../../..
git add src/Web/vue-app/src/types/ src/Web/vue-app/src/locales/ src/Web/vue-app/package.json src/Web/vue-app/package-lock.json
git commit -m "feat: ajouter types TeamEvent, requêtes et clés i18n pour le calendrier"
```

---

### Task 9 : Frontend — TeamEventService + DI

**Files:**
- Modify: `src/Web/vue-app/src/injection/interfaces.ts`
- Modify: `src/Web/vue-app/src/injection/types.ts`
- Create: `src/Web/vue-app/src/services/teamEventService.ts`
- Modify: `src/Web/vue-app/src/services/index.ts`
- Modify: `src/Web/vue-app/src/inversify.config.ts`

- [ ] **Étape 1 : Ajouter l'interface ITeamEventService dans interfaces.ts**

Dans `src/Web/vue-app/src/injection/interfaces.ts`, à la fin du fichier, ajouter :

```typescript
export interface ITeamEventService {
  getEvents(teamId: string, from: string, to: string): Promise<TeamEvent[]>
  createEvent(teamId: string, request: ICreateTeamEventRequest): Promise<SucceededOrNotResponse>
  updateEvent(teamId: string, eventId: string, request: IUpdateTeamEventRequest): Promise<SucceededOrNotResponse>
  deleteEvent(teamId: string, eventId: string): Promise<SucceededOrNotResponse>
}
```

Et ajouter les imports manquants au début du fichier (si non déjà présents) :

```typescript
import { ICreateTeamEventRequest, IUpdateTeamEventRequest } from "@/types/requests"
import { TeamEvent } from "@/types/entities"
```

> Note: `TeamEvent` est déjà importé dans `interfaces.ts` via `@/types/entities` si la ligne `import {... Team ...} from "@/types/entities"` est déjà là. Sinon, ajouter `TeamEvent` à cette ligne.

- [ ] **Étape 2 : Ajouter le symbole DI dans types.ts**

Dans `src/Web/vue-app/src/injection/types.ts`, ajouter dans l'objet `TYPES` :

```typescript
ITeamEventService: Symbol.for("ITeamEventService"),
```

- [ ] **Étape 3 : Créer TeamEventService**

```typescript
// src/Web/vue-app/src/services/teamEventService.ts
import { AxiosError, AxiosResponse } from "axios"
import { injectable } from "inversify"
import { ApiService } from "@/services/apiService"
import { ITeamEventService } from "@/injection/interfaces"
import { SucceededOrNotResponse } from "@/types/responses"
import { ICreateTeamEventRequest, IUpdateTeamEventRequest } from "@/types/requests"
import { TeamEvent } from "@/types/entities"

@injectable()
export class TeamEventService extends ApiService implements ITeamEventService {
  public async getEvents(teamId: string, from: string, to: string): Promise<TeamEvent[]> {
    const response = await this._httpClient
      .get<any, AxiosResponse<TeamEvent[]>>(
        `${import.meta.env.VITE_API_BASE_URL}/teams/${teamId}/events?from=${encodeURIComponent(from)}&to=${encodeURIComponent(to)}`
      )
      .catch(function (error: AxiosError): AxiosResponse<any> {
        return error.response as AxiosResponse<any>
      })
    if (response.status === 200) {
      return response.data as TeamEvent[]
    }
    return []
  }

  public async createEvent(teamId: string, request: ICreateTeamEventRequest): Promise<SucceededOrNotResponse> {
    const response = await this._httpClient
      .post<any, AxiosResponse<any>>(
        `${import.meta.env.VITE_API_BASE_URL}/teams/${teamId}/events`,
        request,
        this.headersWithJsonContentType()
      )
      .catch(function (error: AxiosError): AxiosResponse<any> {
        return error.response as AxiosResponse<any>
      })
    if (response.status === 201) {
      return new SucceededOrNotResponse(true, undefined, response.data)
    }
    const errorResponse = response.data as SucceededOrNotResponse
    return new SucceededOrNotResponse(false, errorResponse?.errors)
  }

  public async updateEvent(teamId: string, eventId: string, request: IUpdateTeamEventRequest): Promise<SucceededOrNotResponse> {
    const response = await this._httpClient
      .put<any, AxiosResponse<any>>(
        `${import.meta.env.VITE_API_BASE_URL}/teams/${teamId}/events/${eventId}`,
        request,
        this.headersWithJsonContentType()
      )
      .catch(function (error: AxiosError): AxiosResponse<any> {
        return error.response as AxiosResponse<any>
      })
    return new SucceededOrNotResponse(response.status === 204)
  }

  public async deleteEvent(teamId: string, eventId: string): Promise<SucceededOrNotResponse> {
    const response = await this._httpClient
      .delete<any, AxiosResponse<any>>(
        `${import.meta.env.VITE_API_BASE_URL}/teams/${teamId}/events/${eventId}`
      )
      .catch(function (error: AxiosError): AxiosResponse<any> {
        return error.response as AxiosResponse<any>
      })
    return new SucceededOrNotResponse(response.status === 204)
  }
}
```

- [ ] **Étape 4 : Exporter depuis services/index.ts**

Dans `src/Web/vue-app/src/services/index.ts`, ajouter à la fin :

```typescript
export * from './teamEventService';
```

- [ ] **Étape 5 : Enregistrer dans inversify.config.ts**

Dans `src/Web/vue-app/src/inversify.config.ts` :

1. Ajouter l'import de l'interface :
```typescript
import { ..., ITeamEventService } from "@/injection/interfaces";
```

2. Ajouter l'import du service (dans le bloc d'imports des services) :
```typescript
import { ..., TeamEventService } from "@/services";
```

3. Ajouter le binding après les bindings existants :
```typescript
dependencyInjection.bind<ITeamEventService>(TYPES.ITeamEventService).to(TeamEventService).inSingletonScope()
```

4. Ajouter la fonction d'accès avant le bloc `export` :
```typescript
function useTeamEventService() {
  return dependencyInjection.get<ITeamEventService>(TYPES.ITeamEventService);
}
```

5. Ajouter `useTeamEventService` dans le bloc `export { ... }`.

- [ ] **Étape 6 : Vérifier que le frontend compile**

```bash
cd src/Web/vue-app
npm run build
```

Expected: build réussi, 0 erreurs TypeScript.

- [ ] **Étape 7 : Commit**

```bash
cd ../../..
git add src/Web/vue-app/src/injection/ src/Web/vue-app/src/services/ src/Web/vue-app/src/inversify.config.ts
git commit -m "feat: ajouter TeamEventService et enregistrement DI"
```

---

### Task 10 : Frontend — TeamCalendar.vue (CRUD admin)

**Files:**
- Create: `src/Web/vue-app/src/components/calendar/TeamCalendar.vue`

- [ ] **Étape 1 : Créer le composant TeamCalendar.vue**

```vue
<!-- src/Web/vue-app/src/components/calendar/TeamCalendar.vue -->
<template>
  <div class="team-calendar">
    <!-- En-tête section -->
    <div class="flex items-center gap-3 px-6 py-4 bg-green-lighter border-b border-green-light">
      <span class="block w-1.5 h-7 rounded-full bg-green"></span>
      <h2 class="font-montserrat font-semibold text-green-dark text-base">
        {{ t('pages.teams.calendar.title') }}
      </h2>
    </div>

    <div class="p-6">
      <Loader v-if="isLoading" />

      <vue-cal
        v-else
        active-view="month"
        :disable-views="['years', 'year', 'week', 'day']"
        :events="calendarEvents"
        :locale="currentLocale"
        :time="false"
        hide-view-selector
        @cell-click="onCellClick"
        @event-click="onEventClick"
        @view-change="onViewChange"
        class="team-calendar__cal"
      />
    </div>

    <!-- Modal création -->
    <Transition name="fade">
      <div v-if="showCreateModal" class="calendar-modal">
        <span class="calendar-modal__bg" @click="showCreateModal = false"></span>
        <div class="calendar-modal__container">
          <div class="calendar-modal__header">
            <h3 class="font-montserrat font-semibold text-green-dark">
              {{ t('pages.teams.calendar.addEvent') }}
            </h3>
            <button type="button" class="calendar-modal__close" @click="showCreateModal = false">×</button>
          </div>
          <div class="calendar-modal__body">
            <div class="flex flex-col gap-4">
              <div class="flex flex-col gap-1">
                <label class="text-xs font-montserrat uppercase tracking-widest text-grey-dark">
                  {{ t('pages.teams.calendar.typeLabel') }}
                </label>
                <select v-model="createForm.type" class="border border-grey rounded-lg px-3 py-2 font-montserrat text-grey-darker focus:outline-none focus:border-green">
                  <option value="Pratique">{{ t('pages.teams.calendar.typePratique') }}</option>
                  <option value="Match">{{ t('pages.teams.calendar.typeMatch') }}</option>
                </select>
              </div>
              <div class="flex flex-col gap-1">
                <label class="text-xs font-montserrat uppercase tracking-widest text-grey-dark">
                  {{ t('pages.teams.calendar.dateLabel') }}
                </label>
                <input type="date" v-model="createForm.date" class="border border-grey rounded-lg px-3 py-2 font-montserrat text-grey-darker focus:outline-none focus:border-green" />
              </div>
              <div class="grid grid-cols-2 gap-3">
                <div class="flex flex-col gap-1">
                  <label class="text-xs font-montserrat uppercase tracking-widest text-grey-dark">
                    {{ t('pages.teams.calendar.startLabel') }}
                  </label>
                  <input type="time" v-model="createForm.startTime" class="border border-grey rounded-lg px-3 py-2 font-montserrat text-grey-darker focus:outline-none focus:border-green" />
                </div>
                <div class="flex flex-col gap-1">
                  <label class="text-xs font-montserrat uppercase tracking-widest text-grey-dark">
                    {{ t('pages.teams.calendar.endLabel') }}
                  </label>
                  <input type="time" v-model="createForm.endTime" class="border border-grey rounded-lg px-3 py-2 font-montserrat text-grey-darker focus:outline-none focus:border-green" />
                </div>
              </div>
            </div>
          </div>
          <div class="calendar-modal__actions">
            <button type="button" class="btn" @click="showCreateModal = false">{{ t('global.cancel') }}</button>
            <button type="button" class="btn btn--primary" :disabled="isSaving" @click="handleCreate">
              {{ t('global.save') }}
            </button>
          </div>
        </div>
      </div>
    </Transition>

    <!-- Modal édition/suppression -->
    <Transition name="fade">
      <div v-if="showEditModal && selectedEvent" class="calendar-modal">
        <span class="calendar-modal__bg" @click="showEditModal = false"></span>
        <div class="calendar-modal__container">
          <div class="calendar-modal__header">
            <h3 class="font-montserrat font-semibold text-green-dark">
              {{ t('pages.teams.calendar.editEvent') }}
            </h3>
            <button type="button" class="calendar-modal__close" @click="showEditModal = false">×</button>
          </div>
          <div class="calendar-modal__body">
            <div class="flex flex-col gap-4">
              <div class="flex flex-col gap-1">
                <label class="text-xs font-montserrat uppercase tracking-widest text-grey-dark">
                  {{ t('pages.teams.calendar.typeLabel') }}
                </label>
                <select v-model="editForm.type" class="border border-grey rounded-lg px-3 py-2 font-montserrat text-grey-darker focus:outline-none focus:border-green">
                  <option value="Pratique">{{ t('pages.teams.calendar.typePratique') }}</option>
                  <option value="Match">{{ t('pages.teams.calendar.typeMatch') }}</option>
                </select>
              </div>
              <div class="flex flex-col gap-1">
                <label class="text-xs font-montserrat uppercase tracking-widest text-grey-dark">
                  {{ t('pages.teams.calendar.dateLabel') }}
                </label>
                <input type="date" v-model="editForm.date" class="border border-grey rounded-lg px-3 py-2 font-montserrat text-grey-darker focus:outline-none focus:border-green" />
              </div>
              <div class="grid grid-cols-2 gap-3">
                <div class="flex flex-col gap-1">
                  <label class="text-xs font-montserrat uppercase tracking-widest text-grey-dark">
                    {{ t('pages.teams.calendar.startLabel') }}
                  </label>
                  <input type="time" v-model="editForm.startTime" class="border border-grey rounded-lg px-3 py-2 font-montserrat text-grey-darker focus:outline-none focus:border-green" />
                </div>
                <div class="flex flex-col gap-1">
                  <label class="text-xs font-montserrat uppercase tracking-widest text-grey-dark">
                    {{ t('pages.teams.calendar.endLabel') }}
                  </label>
                  <input type="time" v-model="editForm.endTime" class="border border-grey rounded-lg px-3 py-2 font-montserrat text-grey-darker focus:outline-none focus:border-green" />
                </div>
              </div>
            </div>
          </div>
          <div class="calendar-modal__actions">
            <button type="button" class="btn btn--danger" :disabled="isSaving" @click="handleDelete">
              {{ t('pages.teams.calendar.deleteEvent') }}
            </button>
            <div class="flex gap-2">
              <button type="button" class="btn" @click="showEditModal = false">{{ t('global.cancel') }}</button>
              <button type="button" class="btn btn--primary" :disabled="isSaving" @click="handleUpdate">
                {{ t('global.save') }}
              </button>
            </div>
          </div>
        </div>
      </div>
    </Transition>
  </div>
</template>

<script lang="ts" setup>
import { ref, computed, onMounted } from 'vue'
import { useI18n } from 'vue3-i18n'
import VueCal from 'vue-cal'
import 'vue-cal/dist/vuecal.css'
import { useTeamEventService } from '@/inversify.config'
import { notifySuccess, notifyError } from '@/notify'
import { TeamEvent } from '@/types/entities'
import Loader from '@/components/layouts/items/Loader.vue'

interface Props {
  teamId: string
}

const props = defineProps<Props>()
const { t, locale } = useI18n()
const teamEventService = useTeamEventService()

const currentLocale = computed(() => locale.value === 'fr' ? 'fr' : 'en')

const isLoading = ref(false)
const isSaving = ref(false)
const rawEvents = ref<TeamEvent[]>([])

const showCreateModal = ref(false)
const showEditModal = ref(false)
const selectedEvent = ref<TeamEvent | null>(null)

const createForm = ref({ type: 'Pratique', date: '', startTime: '08:00', endTime: '10:00' })
const editForm = ref({ type: 'Pratique', date: '', startTime: '08:00', endTime: '10:00' })

let currentFrom = ''
let currentTo = ''

const calendarEvents = computed(() =>
  rawEvents.value.map(e => ({
    start: toVueCalDate(e.startDateTime!),
    end: toVueCalDate(e.endDateTime!),
    title: e.type === 'Pratique' ? t('pages.teams.calendar.typePratique') : t('pages.teams.calendar.typeMatch'),
    class: e.type === 'Pratique' ? 'event-pratique' : 'event-match',
    _raw: e
  }))
)

function toVueCalDate(iso: string): string {
  return iso.replace('T', ' ').substring(0, 16)
}

function toDateInputValue(iso: string): string {
  return iso.substring(0, 10)
}

function toTimeInputValue(iso: string): string {
  return iso.substring(11, 16)
}

function toIso(date: string, time: string): string {
  return `${date}T${time}:00`
}

async function loadEvents(from: Date, to: Date) {
  isLoading.value = true
  currentFrom = from.toISOString()
  currentTo = to.toISOString()
  rawEvents.value = await teamEventService.getEvents(props.teamId, currentFrom, currentTo)
  isLoading.value = false
}

function onViewChange({ startDate, endDate }: { startDate: Date; endDate: Date }) {
  loadEvents(startDate, endDate)
}

function onCellClick(date: Date) {
  const dateStr = date.toISOString().substring(0, 10)
  createForm.value = { type: 'Pratique', date: dateStr, startTime: '08:00', endTime: '10:00' }
  showCreateModal.value = true
}

function onEventClick({ event }: { event: any }) {
  const raw = event._raw as TeamEvent
  selectedEvent.value = raw
  editForm.value = {
    type: raw.type ?? 'Pratique',
    date: toDateInputValue(raw.startDateTime!),
    startTime: toTimeInputValue(raw.startDateTime!),
    endTime: toTimeInputValue(raw.endDateTime!)
  }
  showEditModal.value = true
}

async function handleCreate() {
  isSaving.value = true
  const result = await teamEventService.createEvent(props.teamId, {
    type: createForm.value.type,
    startDateTime: toIso(createForm.value.date, createForm.value.startTime),
    endDateTime: toIso(createForm.value.date, createForm.value.endTime)
  })
  isSaving.value = false
  if (result.succeeded) {
    notifySuccess(t('pages.teams.calendar.saveSuccess'))
    showCreateModal.value = false
    await loadEvents(new Date(currentFrom), new Date(currentTo))
  } else {
    notifyError(t('pages.teams.calendar.saveError'))
  }
}

async function handleUpdate() {
  if (!selectedEvent.value?.id) return
  isSaving.value = true
  const result = await teamEventService.updateEvent(props.teamId, selectedEvent.value.id, {
    type: editForm.value.type,
    startDateTime: toIso(editForm.value.date, editForm.value.startTime),
    endDateTime: toIso(editForm.value.date, editForm.value.endTime)
  })
  isSaving.value = false
  if (result.succeeded) {
    notifySuccess(t('pages.teams.calendar.saveSuccess'))
    showEditModal.value = false
    await loadEvents(new Date(currentFrom), new Date(currentTo))
  } else {
    notifyError(t('pages.teams.calendar.saveError'))
  }
}

async function handleDelete() {
  if (!selectedEvent.value?.id) return
  isSaving.value = true
  const result = await teamEventService.deleteEvent(props.teamId, selectedEvent.value.id)
  isSaving.value = false
  if (result.succeeded) {
    notifySuccess(t('pages.teams.calendar.deleteSuccess'))
    showEditModal.value = false
    await loadEvents(new Date(currentFrom), new Date(currentTo))
  } else {
    notifyError(t('pages.teams.calendar.deleteError'))
  }
}

onMounted(() => {
  const now = new Date()
  const from = new Date(now.getFullYear(), now.getMonth(), 1)
  const to = new Date(now.getFullYear(), now.getMonth() + 1, 0)
  loadEvents(from, to)
})
</script>

<style scoped>
.team-calendar__cal {
  height: 600px;
}

:deep(.event-pratique) {
  background-color: #4caf50;
  color: white;
  border-radius: 4px;
}

:deep(.event-match) {
  background-color: #1565c0;
  color: white;
  border-radius: 4px;
}

.calendar-modal {
  position: fixed;
  inset: 0;
  display: flex;
  align-items: center;
  justify-content: center;
  z-index: 1000;
}

.calendar-modal__bg {
  position: absolute;
  inset: 0;
  background: rgba(0, 0, 0, 0.5);
}

.calendar-modal__container {
  position: relative;
  background: #fff;
  border-radius: 0.75rem;
  width: 95%;
  max-width: 480px;
  box-shadow: var(--shadow-bold, 0 20px 40px rgba(0,0,0,0.2));
  overflow: hidden;
}

.calendar-modal__header {
  display: flex;
  align-items: center;
  justify-content: space-between;
  padding: 1rem 1.5rem;
  background: var(--color-green-lighter, #e8f5e9);
  border-bottom: 1px solid var(--color-green-light, #c8e6c9);
}

.calendar-modal__close {
  background: none;
  border: none;
  font-size: 1.75rem;
  line-height: 1;
  cursor: pointer;
  color: var(--color-green-dark, #2c3e50);
}

.calendar-modal__body {
  padding: 1.5rem;
}

.calendar-modal__actions {
  display: flex;
  justify-content: space-between;
  align-items: center;
  gap: 0.75rem;
  padding: 1rem 1.5rem;
  border-top: 1px solid var(--color-grey-light, #e0e0e0);
  background: var(--color-grey-lighter, #fafafa);
}

.fade-leave-active,
.fade-enter-active {
  transition: opacity 0.2s ease;
}
.fade-enter-from,
.fade-leave-to {
  opacity: 0;
}
</style>
```

- [ ] **Étape 2 : Vérifier que le frontend compile**

```bash
cd src/Web/vue-app
npm run build
```

Expected: build réussi.

- [ ] **Étape 3 : Commit**

```bash
cd ../../..
git add src/Web/vue-app/src/components/calendar/TeamCalendar.vue
git commit -m "feat: créer le composant TeamCalendar avec CRUD"
```

---

### Task 11 : Intégrer TeamCalendar dans AdminTeamDetail.vue

**Files:**
- Modify: `src/Web/vue-app/src/views/admin/teams/AdminTeamDetail.vue`

- [ ] **Étape 1 : Lire le fichier pour trouver la fin du bloc `<template v-if="!isLoading && team">`**

Lire `src/Web/vue-app/src/views/admin/teams/AdminTeamDetail.vue` et repérer la fermeture du bloc `</template>` qui correspond à `<template v-if="!isLoading && team">`.

- [ ] **Étape 2 : Ajouter l'import de TeamCalendar dans le script**

Dans la section `<script lang="ts" setup>`, ajouter après les imports existants :

```typescript
import TeamCalendar from '@/components/calendar/TeamCalendar.vue'
```

- [ ] **Étape 3 : Ajouter le composant dans le template**

Juste avant la balise fermante `</template>` du bloc `<template v-if="!isLoading && team">`, ajouter :

```html
      <!-- Section calendrier -->
      <div class="bg-white rounded-xl border border-grey overflow-hidden" style="box-shadow: var(--shadow-bold)">
        <TeamCalendar :team-id="team.id!" />
      </div>
```

- [ ] **Étape 4 : Vérifier que le frontend compile**

```bash
cd src/Web/vue-app
npm run build
```

Expected: build réussi.

- [ ] **Étape 5 : Commit**

```bash
cd ../../..
git add src/Web/vue-app/src/views/admin/teams/AdminTeamDetail.vue
git commit -m "feat: intégrer TeamCalendar dans la page d'équipe"
```

---

### Task 12 : Frontend — AthleteCalendar.vue (lecture seule)

**Files:**
- Create: `src/Web/vue-app/src/components/calendar/AthleteCalendar.vue`

- [ ] **Étape 1 : Créer le composant AthleteCalendar.vue**

```vue
<!-- src/Web/vue-app/src/components/calendar/AthleteCalendar.vue -->
<template>
  <div>
    <!-- En-tête section -->
    <div class="flex items-center gap-3 px-6 py-4 bg-green-lighter border-b border-green-light">
      <span class="block w-1.5 h-7 rounded-full bg-green"></span>
      <h2 class="font-montserrat font-semibold text-green-dark text-base">
        {{ t('pages.athletes.calendar.title') }}
      </h2>
    </div>

    <div class="p-6">
      <p v-if="!teamId" class="font-montserrat text-grey-dark italic">
        {{ t('pages.athletes.calendar.noTeam') }}
      </p>

      <template v-else>
        <Loader v-if="isLoading" />
        <vue-cal
          v-else
          active-view="month"
          :disable-views="['years', 'year', 'week', 'day']"
          :events="calendarEvents"
          :locale="currentLocale"
          :time="false"
          hide-view-selector
          :editable-events="false"
          @view-change="onViewChange"
          style="height: 600px;"
        />
      </template>
    </div>
  </div>
</template>

<script lang="ts" setup>
import { ref, computed, onMounted } from 'vue'
import { useI18n } from 'vue3-i18n'
import VueCal from 'vue-cal'
import 'vue-cal/dist/vuecal.css'
import { useTeamEventService } from '@/inversify.config'
import { TeamEvent } from '@/types/entities'
import Loader from '@/components/layouts/items/Loader.vue'

interface Props {
  teamId?: string | null
}

const props = defineProps<Props>()
const { t, locale } = useI18n()
const teamEventService = useTeamEventService()

const currentLocale = computed(() => locale.value === 'fr' ? 'fr' : 'en')
const isLoading = ref(false)
const rawEvents = ref<TeamEvent[]>([])

const calendarEvents = computed(() =>
  rawEvents.value.map(e => ({
    start: e.startDateTime!.replace('T', ' ').substring(0, 16),
    end: e.endDateTime!.replace('T', ' ').substring(0, 16),
    title: e.type === 'Pratique' ? t('pages.teams.calendar.typePratique') : t('pages.teams.calendar.typeMatch'),
    class: e.type === 'Pratique' ? 'event-pratique' : 'event-match'
  }))
)

async function loadEvents(from: Date, to: Date) {
  if (!props.teamId) return
  isLoading.value = true
  rawEvents.value = await teamEventService.getEvents(props.teamId, from.toISOString(), to.toISOString())
  isLoading.value = false
}

function onViewChange({ startDate, endDate }: { startDate: Date; endDate: Date }) {
  loadEvents(startDate, endDate)
}

onMounted(() => {
  if (!props.teamId) return
  const now = new Date()
  const from = new Date(now.getFullYear(), now.getMonth(), 1)
  const to = new Date(now.getFullYear(), now.getMonth() + 1, 0)
  loadEvents(from, to)
})
</script>

<style scoped>
:deep(.event-pratique) {
  background-color: #4caf50;
  color: white;
  border-radius: 4px;
}

:deep(.event-match) {
  background-color: #1565c0;
  color: white;
  border-radius: 4px;
}
</style>
```

- [ ] **Étape 2 : Vérifier que le frontend compile**

```bash
cd src/Web/vue-app
npm run build
```

Expected: build réussi.

- [ ] **Étape 3 : Commit**

```bash
cd ../../..
git add src/Web/vue-app/src/components/calendar/AthleteCalendar.vue
git commit -m "feat: créer le composant AthleteCalendar en lecture seule"
```

---

### Task 13 : Intégrer AthleteCalendar dans AdminAthleteDetail.vue

**Files:**
- Modify: `src/Web/vue-app/src/views/admin/athletes/AdminAthleteDetail.vue`

- [ ] **Étape 1 : Lire le fichier pour trouver la fin de la section "suivi hebdomadaire"**

Lire `src/Web/vue-app/src/views/admin/athletes/AdminAthleteDetail.vue`. Repérer la fermeture du div de la section "suivi hebdomadaire" (section qui contient `weeklyTitle`). Elle se termine par `</div>` à la ligne ~152.

- [ ] **Étape 2 : Ajouter l'import dans le script**

Dans la section `<script lang="ts" setup>`, ajouter après les imports existants :

```typescript
import AthleteCalendar from '@/components/calendar/AthleteCalendar.vue'
```

- [ ] **Étape 3 : Ajouter le composant dans le template**

Après la fermeture `</div>` de la section "suivi hebdomadaire" (après le bloc contenant `weeklyTitle`, autour de la ligne 152) et avant la section "notes de blessure", ajouter :

```html
      <!-- Section calendrier de l'équipe -->
      <div class="bg-white rounded-xl border border-grey overflow-hidden" style="box-shadow: var(--shadow-bold)">
        <AthleteCalendar :team-id="athlete.teamId ?? null" />
      </div>
```

- [ ] **Étape 4 : Vérifier que le frontend compile**

```bash
cd src/Web/vue-app
npm run build
```

Expected: build réussi, 0 erreurs TypeScript.

- [ ] **Étape 5 : Lancer tous les tests backend**

```bash
cd C:\Users\antho\Periscope\Periscope
dotnet test
```

Expected: tous les tests PASS.

- [ ] **Étape 6 : Commit final**

```bash
git add src/Web/vue-app/src/views/admin/athletes/AdminAthleteDetail.vue
git commit -m "feat: intégrer AthleteCalendar dans le profil athlète"
```
