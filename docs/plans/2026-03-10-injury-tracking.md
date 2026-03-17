# Injury Tracking Implementation Plan

> **For Claude:** REQUIRED SUB-SKILL: Use superpowers:executing-plans to implement this plan task-by-task.

**Goal:** Allow admins to track athlete injuries via a free-text notes journal per athlete, with an "injured" status flag and a dashboard section showing currently injured players.

**Architecture:** New `NoteBlessure` entity (Id, AthleteId FK, Contenu, CreatedAt) + `IsInjured` bool on `Athlete`. Five new backend endpoints. New athlete detail page in Vue with notes journal + toggle. Dashboard gains an "injured athletes" card filterable by team.

**Tech Stack:** C# / FastEndpoints / EF Core (backend) — Vue 3 / TypeScript / InversifyJS / vue3-i18n (frontend)

---

## Task 1: Add `IsInjured` to `Athlete` entity

**Files:**
- Modify: `src/Domain/Entities/Athlete.cs`

**Step 1: Add the property and setter**

In `Athlete.cs`, add after `CreatedAt`:

```csharp
public bool IsInjured { get; private set; }

public void SetIsInjured(bool isInjured) => IsInjured = isInjured;
```

**Step 2: Commit**

```bash
git add src/Domain/Entities/Athlete.cs
git commit -m "feat: add IsInjured field to Athlete entity"
```

---

## Task 2: Create `NoteBlessure` entity

**Files:**
- Create: `src/Domain/Entities/NoteBlessure.cs`

**Step 1: Create the file**

```csharp
namespace Domain.Entities;

public class NoteBlessure : Common.Entity
{
    public Guid AthleteId { get; private set; }
    public Athlete Athlete { get; private set; } = null!;
    public string Contenu { get; private set; } = null!;
    public DateTime CreatedAt { get; private set; }

    public NoteBlessure() { }

    public NoteBlessure(Guid athleteId, string contenu)
    {
        AthleteId = athleteId;
        Contenu = contenu;
        CreatedAt = DateTime.UtcNow;
    }
}
```

**Step 2: Commit**

```bash
git add src/Domain/Entities/NoteBlessure.cs
git commit -m "feat: add NoteBlessure entity"
```

---

## Task 3: Create `INoteBlessureRepository` interface

**Files:**
- Create: `src/Domain/Repositories/INoteBlessureRepository.cs`

**Step 1: Create the file**

```csharp
using Domain.Entities;

namespace Domain.Repositories;

public interface INoteBlessureRepository
{
    Task CreateAsync(NoteBlessure note);
    Task<IEnumerable<NoteBlessure>> GetByAthleteIdAsync(Guid athleteId);
}
```

**Step 2: Commit**

```bash
git add src/Domain/Repositories/INoteBlessureRepository.cs
git commit -m "feat: add INoteBlessureRepository interface"
```

---

## Task 4: Extend `IAthleteRepository` with new methods

**Files:**
- Modify: `src/Domain/Repositories/IAthleteRepository.cs`

**Step 1: Add methods**

Add to the interface:

```csharp
Task<Athlete?> FindByIdAsync(Guid id);
Task UpdateAsync(Athlete athlete);
Task<IEnumerable<Athlete>> GetInjuredAsync();
```

**Step 2: Implement in `AthleteRepository`**

Modify: `src/Infrastructure/Repositories/Athletes/AthleteRepository.cs`

Add these implementations:

```csharp
public async Task<Athlete?> FindByIdAsync(Guid id)
{
    return await _context.Athletes
        .AsNoTracking()
        .FirstOrDefaultAsync(x => x.Id == id && x.Active);
}

public async Task UpdateAsync(Athlete athlete)
{
    _context.Athletes.Update(athlete);
    await _context.SaveChangesAsync();
}

public async Task<IEnumerable<Athlete>> GetInjuredAsync()
{
    return await _context.Athletes
        .AsNoTracking()
        .Where(x => x.Active && x.IsInjured)
        .OrderBy(x => x.LastName)
        .ToListAsync();
}
```

**Step 3: Commit**

```bash
git add src/Domain/Repositories/IAthleteRepository.cs src/Infrastructure/Repositories/Athletes/AthleteRepository.cs
git commit -m "feat: extend AthleteRepository with FindById, Update, GetInjured"
```

---

## Task 5: Create `NoteBlessureRepository` and register services

**Files:**
- Create: `src/Infrastructure/Repositories/Blessures/NoteBlessureRepository.cs`
- Modify: `src/Infrastructure/ConfigureServices.cs`

**Step 1: Create the repository**

```csharp
using Domain.Entities;
using Domain.Repositories;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Infrastructure.Repositories.Blessures;

public class NoteBlessureRepository : INoteBlessureRepository
{
    private readonly GarneauTemplateDbContext _context;

    public NoteBlessureRepository(GarneauTemplateDbContext context)
    {
        _context = context;
    }

    public async Task CreateAsync(NoteBlessure note)
    {
        _context.NotesBlessure.Add(note);
        await _context.SaveChangesAsync();
    }

    public async Task<IEnumerable<NoteBlessure>> GetByAthleteIdAsync(Guid athleteId)
    {
        return await _context.NotesBlessure
            .AsNoTracking()
            .Where(x => x.AthleteId == athleteId)
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync();
    }
}
```

**Step 2: Register in `ConfigureServices.cs`**

In `ConfigureInfrastructureServices`, add:

```csharp
services.AddScoped<INoteBlessureRepository, NoteBlessureRepository>();
```

Also add the using at the top:

```csharp
using Infrastructure.Repositories.Blessures;
```

**Step 3: Commit**

```bash
git add src/Infrastructure/Repositories/Blessures/NoteBlessureRepository.cs src/Infrastructure/ConfigureServices.cs
git commit -m "feat: add NoteBlessureRepository and register it"
```

---

## Task 6: Add persistence (DbSet + Migration)

**Files:**
- Modify: `src/Persistence/GarneauTemplateDbContext.cs`

**Step 1: Add `DbSet<NoteBlessure>`**

In `GarneauTemplateDbContext.cs`, add after the `Athletes` DbSet:

```csharp
public DbSet<NoteBlessure> NotesBlessure { get; set; } = null!;
```

Also add the using if not present:
```csharp
using Domain.Entities;
```

**Step 2: Create the EF Core migration**

Run from the solution root (requires the project to build):

```bash
cd src/Persistence
dotnet ef migrations add AddIsInjuredAndNoteBlessure --startup-project ../Web
```

**Step 3: Commit**

```bash
git add src/Persistence/GarneauTemplateDbContext.cs src/Persistence/Migrations/
git commit -m "feat: add NoteBlessure table and IsInjured column migration"
```

---

## Task 7: Backend endpoint — `GetAthleteById`

**Files:**
- Create: `src/Web/Features/Admins/Athletes/GetAthleteById/GetAthleteByIdEndpoint.cs`
- Create: `src/Web/Features/Admins/Athletes/GetAthleteById/GetAthleteByIdRequest.cs`

**Step 1: Create the request**

```csharp
namespace Web.Features.Admins.Athletes.GetAthleteById;

public class GetAthleteByIdRequest
{
    public Guid Id { get; set; }
}
```

**Step 2: Create the endpoint**

```csharp
using Domain.Repositories;
using FastEndpoints;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Web.Features.Admins.Athletes.CreateAthlete;

namespace Web.Features.Admins.Athletes.GetAthleteById;

public class GetAthleteByIdEndpoint : Endpoint<GetAthleteByIdRequest, AthleteResponse>
{
    private readonly IAthleteRepository _athleteRepository;

    public GetAthleteByIdEndpoint(IAthleteRepository athleteRepository)
    {
        _athleteRepository = athleteRepository;
    }

    public override void Configure()
    {
        DontCatchExceptions();
        Get("athletes/{id}");
        Roles(Domain.Constants.User.Roles.ADMINISTRATOR);
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
    }

    public override async Task HandleAsync(GetAthleteByIdRequest req, CancellationToken ct)
    {
        var athlete = await _athleteRepository.FindByIdAsync(req.Id);
        if (athlete == null)
        {
            await Send.NotFoundAsync(ct);
            return;
        }

        var response = new AthleteResponse
        {
            Id = athlete.Id,
            FirstName = athlete.FirstName,
            LastName = athlete.LastName,
            Email = athlete.Email,
            DateOfBirth = athlete.DateOfBirth,
            SubmissionToken = athlete.SubmissionToken,
            Active = athlete.Active,
            CreatedAt = athlete.CreatedAt,
            IsInjured = athlete.IsInjured
        };

        await Send.OkAsync(response, cancellation: ct);
    }
}
```

**Step 3: Update `AthleteResponse` to include `IsInjured`**

Modify `src/Web/Features/Admins/Athletes/CreateAthlete/AthleteResponse.cs` — add:

```csharp
public bool IsInjured { get; set; }
```

**Step 4: Commit**

```bash
git add src/Web/Features/Admins/Athletes/GetAthleteById/ src/Web/Features/Admins/Athletes/CreateAthlete/AthleteResponse.cs
git commit -m "feat: add GetAthleteById endpoint"
```

---

## Task 8: Backend endpoint — `ToggleAthleteInjured`

**Files:**
- Create: `src/Web/Features/Admins/Athletes/ToggleAthleteInjured/ToggleAthleteInjuredEndpoint.cs`
- Create: `src/Web/Features/Admins/Athletes/ToggleAthleteInjured/ToggleAthleteInjuredRequest.cs`

**Step 1: Create the request**

```csharp
namespace Web.Features.Admins.Athletes.ToggleAthleteInjured;

public class ToggleAthleteInjuredRequest
{
    public Guid Id { get; set; }
    public bool IsInjured { get; set; }
}
```

**Step 2: Create the endpoint**

```csharp
using Domain.Repositories;
using FastEndpoints;
using Microsoft.AspNetCore.Authentication.JwtBearer;

namespace Web.Features.Admins.Athletes.ToggleAthleteInjured;

public class ToggleAthleteInjuredEndpoint : Endpoint<ToggleAthleteInjuredRequest>
{
    private readonly IAthleteRepository _athleteRepository;

    public ToggleAthleteInjuredEndpoint(IAthleteRepository athleteRepository)
    {
        _athleteRepository = athleteRepository;
    }

    public override void Configure()
    {
        DontCatchExceptions();
        Patch("athletes/{id}/injured");
        Roles(Domain.Constants.User.Roles.ADMINISTRATOR);
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
    }

    public override async Task HandleAsync(ToggleAthleteInjuredRequest req, CancellationToken ct)
    {
        var athlete = await _athleteRepository.FindByIdAsync(req.Id);
        if (athlete == null)
        {
            await Send.NotFoundAsync(ct);
            return;
        }

        athlete.SetIsInjured(req.IsInjured);
        await _athleteRepository.UpdateAsync(athlete);

        await Send.NoContentAsync(ct);
    }
}
```

**Step 3: Commit**

```bash
git add src/Web/Features/Admins/Athletes/ToggleAthleteInjured/
git commit -m "feat: add ToggleAthleteInjured endpoint"
```

---

## Task 9: Backend endpoint — `GetAthleteNotes`

**Files:**
- Create: `src/Web/Features/Admins/Athletes/GetAthleteNotes/GetAthleteNotesEndpoint.cs`
- Create: `src/Web/Features/Admins/Athletes/GetAthleteNotes/GetAthleteNotesRequest.cs`
- Create: `src/Web/Features/Admins/Athletes/GetAthleteNotes/NoteBlessureResponse.cs`

**Step 1: Create request and response**

```csharp
// GetAthleteNotesRequest.cs
namespace Web.Features.Admins.Athletes.GetAthleteNotes;

public class GetAthleteNotesRequest
{
    public Guid Id { get; set; }
}
```

```csharp
// NoteBlessureResponse.cs
namespace Web.Features.Admins.Athletes.GetAthleteNotes;

public class NoteBlessureResponse
{
    public Guid Id { get; set; }
    public Guid AthleteId { get; set; }
    public string Contenu { get; set; } = null!;
    public DateTime CreatedAt { get; set; }
}
```

**Step 2: Create the endpoint**

```csharp
// GetAthleteNotesEndpoint.cs
using Domain.Repositories;
using FastEndpoints;
using Microsoft.AspNetCore.Authentication.JwtBearer;

namespace Web.Features.Admins.Athletes.GetAthleteNotes;

public class GetAthleteNotesEndpoint : Endpoint<GetAthleteNotesRequest, IEnumerable<NoteBlessureResponse>>
{
    private readonly INoteBlessureRepository _noteBlessureRepository;

    public GetAthleteNotesEndpoint(INoteBlessureRepository noteBlessureRepository)
    {
        _noteBlessureRepository = noteBlessureRepository;
    }

    public override void Configure()
    {
        DontCatchExceptions();
        Get("athletes/{id}/notes");
        Roles(Domain.Constants.User.Roles.ADMINISTRATOR);
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
    }

    public override async Task HandleAsync(GetAthleteNotesRequest req, CancellationToken ct)
    {
        var notes = await _noteBlessureRepository.GetByAthleteIdAsync(req.Id);
        var response = notes.Select(n => new NoteBlessureResponse
        {
            Id = n.Id,
            AthleteId = n.AthleteId,
            Contenu = n.Contenu,
            CreatedAt = n.CreatedAt
        });
        await Send.OkAsync(response, cancellation: ct);
    }
}
```

**Step 3: Commit**

```bash
git add src/Web/Features/Admins/Athletes/GetAthleteNotes/
git commit -m "feat: add GetAthleteNotes endpoint"
```

---

## Task 10: Backend endpoint — `CreateAthleteNote`

**Files:**
- Create: `src/Web/Features/Admins/Athletes/CreateAthleteNote/CreateAthleteNoteEndpoint.cs`
- Create: `src/Web/Features/Admins/Athletes/CreateAthleteNote/CreateAthleteNoteRequest.cs`
- Create: `src/Web/Features/Admins/Athletes/CreateAthleteNote/CreateAthleteNoteValidator.cs`

**Step 1: Create request**

```csharp
// CreateAthleteNoteRequest.cs
using Web.Features.Common;

namespace Web.Features.Admins.Athletes.CreateAthleteNote;

public class CreateAthleteNoteRequest : ISanitizable
{
    public Guid Id { get; set; }
    public string Contenu { get; set; } = null!;

    public void Sanitize()
    {
        Contenu = Contenu.Trim();
    }
}
```

**Step 2: Create validator**

```csharp
// CreateAthleteNoteValidator.cs
using FastEndpoints;
using FluentValidation;

namespace Web.Features.Admins.Athletes.CreateAthleteNote;

public class CreateAthleteNoteValidator : Validator<CreateAthleteNoteRequest>
{
    public CreateAthleteNoteValidator()
    {
        RuleFor(x => x.Contenu)
            .NotNull()
            .NotEmpty()
            .WithErrorCode("InvalidContenu")
            .WithMessage("Note content should not be empty.");
    }
}
```

**Step 3: Create endpoint**

```csharp
// CreateAthleteNoteEndpoint.cs
using Domain.Entities;
using Domain.Repositories;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Web.Features.Common;

namespace Web.Features.Admins.Athletes.CreateAthleteNote;

public class CreateAthleteNoteEndpoint : EndpointWithSanitizedRequest<CreateAthleteNoteRequest>
{
    private readonly IAthleteRepository _athleteRepository;
    private readonly INoteBlessureRepository _noteBlessureRepository;

    public CreateAthleteNoteEndpoint(
        IAthleteRepository athleteRepository,
        INoteBlessureRepository noteBlessureRepository)
    {
        _athleteRepository = athleteRepository;
        _noteBlessureRepository = noteBlessureRepository;
    }

    public override void Configure()
    {
        DontCatchExceptions();
        Post("athletes/{id}/notes");
        Roles(Domain.Constants.User.Roles.ADMINISTRATOR);
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
    }

    public override async Task HandleAsync(CreateAthleteNoteRequest req, CancellationToken ct)
    {
        var athlete = await _athleteRepository.FindByIdAsync(req.Id);
        if (athlete == null)
        {
            await Send.NotFoundAsync(ct);
            return;
        }

        var note = new NoteBlessure(req.Id, req.Contenu);
        await _noteBlessureRepository.CreateAsync(note);

        HttpContext.Response.StatusCode = StatusCodes.Status201Created;
        await HttpContext.Response.WriteAsJsonAsync(new { id = note.Id }, ct);
    }
}
```

**Step 4: Commit**

```bash
git add src/Web/Features/Admins/Athletes/CreateAthleteNote/
git commit -m "feat: add CreateAthleteNote endpoint"
```

---

## Task 11: Backend endpoint — `GetInjuredAthletes`

**Files:**
- Create: `src/Web/Features/Admins/Athletes/GetInjuredAthletes/GetInjuredAthletesEndpoint.cs`

**Step 1: Create endpoint**

```csharp
using Domain.Repositories;
using FastEndpoints;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Web.Features.Admins.Athletes.CreateAthlete;

namespace Web.Features.Admins.Athletes.GetInjuredAthletes;

public class GetInjuredAthletesEndpoint : EndpointWithoutRequest<IEnumerable<AthleteResponse>>
{
    private readonly IAthleteRepository _athleteRepository;

    public GetInjuredAthletesEndpoint(IAthleteRepository athleteRepository)
    {
        _athleteRepository = athleteRepository;
    }

    public override void Configure()
    {
        DontCatchExceptions();
        Get("athletes/injured");
        Roles(Domain.Constants.User.Roles.ADMINISTRATOR);
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var athletes = await _athleteRepository.GetInjuredAsync();
        var response = athletes.Select(a => new AthleteResponse
        {
            Id = a.Id,
            FirstName = a.FirstName,
            LastName = a.LastName,
            Email = a.Email,
            DateOfBirth = a.DateOfBirth,
            SubmissionToken = a.SubmissionToken,
            Active = a.Active,
            CreatedAt = a.CreatedAt,
            IsInjured = a.IsInjured
        });
        await Send.OkAsync(response, cancellation: ct);
    }
}
```

**Step 2: Commit**

```bash
git add src/Web/Features/Admins/Athletes/GetInjuredAthletes/
git commit -m "feat: add GetInjuredAthletes endpoint"
```

---

## Task 12: Frontend types

**Files:**
- Modify: `src/Web/vue-app/src/types/entities/athlete.ts`
- Create: `src/Web/vue-app/src/types/entities/noteBlessure.ts`
- Create: `src/Web/vue-app/src/types/requests/createNoteBlessureRequest.ts`
- Modify: `src/Web/vue-app/src/types/entities/index.ts`
- Modify: `src/Web/vue-app/src/types/requests/index.ts`

**Step 1: Update `athlete.ts`**

Add `isInjured` field:

```typescript
export class Athlete {
  id?: string
  firstName?: string
  lastName?: string
  email?: string
  dateOfBirth?: string
  submissionToken?: string
  active?: boolean
  createdAt?: string
  isInjured?: boolean
}
```

**Step 2: Create `noteBlessure.ts`**

```typescript
export class NoteBlessure {
  id?: string
  athleteId?: string
  contenu?: string
  createdAt?: string
}
```

**Step 3: Create `createNoteBlessureRequest.ts`**

```typescript
export interface ICreateNoteBlessureRequest {
  contenu: string
}
```

**Step 4: Export from index files**

In `types/entities/index.ts`, add:
```typescript
export { NoteBlessure } from './noteBlessure'
```

In `types/requests/index.ts`, add:
```typescript
export type { ICreateNoteBlessureRequest } from './createNoteBlessureRequest'
```

**Step 5: Commit**

```bash
git add src/Web/vue-app/src/types/
git commit -m "feat: add NoteBlessure types and update Athlete type"
```

---

## Task 13: Frontend service — extend `IAthleteService` and `AthleteService`

**Files:**
- Modify: `src/Web/vue-app/src/injection/interfaces.ts`
- Modify: `src/Web/vue-app/src/services/athleteService.ts`

**Step 1: Update `IAthleteService` interface**

In `injection/interfaces.ts`, update the `IAthleteService` interface to add:

```typescript
getById(id: string): Promise<Athlete | null>
toggleInjured(id: string, isInjured: boolean): Promise<SucceededOrNotResponse>
getNotes(id: string): Promise<NoteBlessure[]>
createNote(id: string, request: ICreateNoteBlessureRequest): Promise<SucceededOrNotResponse>
getInjured(): Promise<Athlete[]>
```

Add `NoteBlessure` and `ICreateNoteBlessureRequest` to the imports at the top:
```typescript
import { ..., NoteBlessure } from "@/types/entities"
import { ..., ICreateNoteBlessureRequest } from "@/types/requests"
```

**Step 2: Implement in `athleteService.ts`**

Add the following methods to the `AthleteService` class:

```typescript
public async getById(id: string): Promise<Athlete | null> {
  const response = await this
    ._httpClient
    .get<any, AxiosResponse<Athlete>>(
      `${import.meta.env.VITE_API_BASE_URL}/athletes/${id}`)
    .catch(function (error: AxiosError): AxiosResponse<any> {
      return error.response as AxiosResponse<any>
    })
  if (response.status === 200) return response.data as Athlete
  return null
}

public async toggleInjured(id: string, isInjured: boolean): Promise<SucceededOrNotResponse> {
  const response = await this
    ._httpClient
    .patch<any, AxiosResponse<any>>(
      `${import.meta.env.VITE_API_BASE_URL}/athletes/${id}/injured`,
      { isInjured },
      this.headersWithJsonContentType())
    .catch(function (error: AxiosError): AxiosResponse<any> {
      return error.response as AxiosResponse<any>
    })
  if (response.status === 204) return new SucceededOrNotResponse(true)
  return new SucceededOrNotResponse(false)
}

public async getNotes(id: string): Promise<NoteBlessure[]> {
  const response = await this
    ._httpClient
    .get<any, AxiosResponse<NoteBlessure[]>>(
      `${import.meta.env.VITE_API_BASE_URL}/athletes/${id}/notes`)
    .catch(function (error: AxiosError): AxiosResponse<any> {
      return error.response as AxiosResponse<any>
    })
  if (response.status === 200) return response.data as NoteBlessure[]
  return []
}

public async createNote(id: string, request: ICreateNoteBlessureRequest): Promise<SucceededOrNotResponse> {
  const response = await this
    ._httpClient
    .post<any, AxiosResponse<any>>(
      `${import.meta.env.VITE_API_BASE_URL}/athletes/${id}/notes`,
      request,
      this.headersWithJsonContentType())
    .catch(function (error: AxiosError): AxiosResponse<any> {
      return error.response as AxiosResponse<any>
    })
  if (response.status === 201) return new SucceededOrNotResponse(true)
  return new SucceededOrNotResponse(false)
}

public async getInjured(): Promise<Athlete[]> {
  const response = await this
    ._httpClient
    .get<any, AxiosResponse<Athlete[]>>(
      `${import.meta.env.VITE_API_BASE_URL}/athletes/injured`)
    .catch(function (error: AxiosError): AxiosResponse<any> {
      return error.response as AxiosResponse<any>
    })
  if (response.status === 200) return response.data as Athlete[]
  return []
}
```

Add the import at the top of `athleteService.ts`:
```typescript
import { NoteBlessure } from "@/types/entities"
import { ICreateNoteBlessureRequest } from "@/types/requests"
```

**Step 3: Commit**

```bash
git add src/Web/vue-app/src/injection/interfaces.ts src/Web/vue-app/src/services/athleteService.ts
git commit -m "feat: add injury-related methods to AthleteService"
```

---

## Task 14: Frontend i18n — add translations

**Files:**
- Modify: `src/Web/vue-app/src/locales/fr.json`
- Modify: `src/Web/vue-app/src/locales/en.json`

**Step 1: Add to `fr.json`**

In the `routes.admin.children.athletes` object, add a `detail` child:

```json
"detail": {
  "path": ":id",
  "fullPath": "/administration/athletes/:id",
  "name": "Fiche athlète"
}
```

In the `pages.athletes` object, add:

```json
"detail": {
  "injuredLabel": "Athlète blessé",
  "injuredToggleSuccess": "Statut de blessure mis à jour.",
  "injuredToggleFailed": "Erreur lors de la mise à jour du statut.",
  "notesTitle": "Journal de suivi",
  "notesEmpty": "Aucune note pour cet athlète.",
  "notePlaceholder": "Ajouter une note de suivi...",
  "noteAddButton": "Ajouter",
  "noteAddSuccess": "Note ajoutée.",
  "noteAddFailed": "Erreur lors de l'ajout de la note."
},
"injuredSection": {
  "title": "Joueurs blessés",
  "empty": "Aucun joueur blessé en ce moment."
}
```

**Step 2: Add to `en.json`**

In the `routes.admin.children.athletes` object, add a `detail` child:

```json
"detail": {
  "path": ":id",
  "fullPath": "/administration/athletes/:id",
  "name": "Athlete profile"
}
```

In the `pages.athletes` object, add:

```json
"detail": {
  "injuredLabel": "Injured athlete",
  "injuredToggleSuccess": "Injury status updated.",
  "injuredToggleFailed": "Error updating injury status.",
  "notesTitle": "Injury log",
  "notesEmpty": "No notes for this athlete.",
  "notePlaceholder": "Add a follow-up note...",
  "noteAddButton": "Add",
  "noteAddSuccess": "Note added.",
  "noteAddFailed": "Error adding the note."
},
"injuredSection": {
  "title": "Injured players",
  "empty": "No injured players at the moment."
}
```

**Step 3: Commit**

```bash
git add src/Web/vue-app/src/locales/
git commit -m "feat: add injury tracking i18n translations"
```

---

## Task 15: Frontend router — add athlete detail route

**Files:**
- Modify: `src/Web/vue-app/src/router/index.ts`

**Step 1: Add the import at the top**

```typescript
import AdminAthleteDetail from "@/views/admin/athletes/AdminAthleteDetail.vue"
```

**Step 2: Add the route inside the `athletes` children array**

After the existing `athletes.add` route, add:

```typescript
{
  path: i18n.t("routes.admin.children.athletes.detail.path"),
  alias: getLocalizedRoutes("routes.admin.children.athletes.detail.path"),
  name: "admin.children.athletes.detail",
  component: AdminAthleteDetail,
  props: true
},
```

**Step 3: Commit**

```bash
git add src/Web/vue-app/src/router/index.ts
git commit -m "feat: add athlete detail route"
```

---

## Task 16: Frontend view — `AdminAthleteDetail.vue`

**Files:**
- Create: `src/Web/vue-app/src/views/admin/athletes/AdminAthleteDetail.vue`

**Step 1: Create the file**

```vue
<template>
  <div class="content-grid">

    <!-- En-tête -->
    <div class="flex flex-col gap-3 pb-6 border-b-2 border-green-light">
      <span class="text-xs font-montserrat uppercase tracking-widest text-green-dark">Administration</span>
      <div class="flex items-center justify-between flex-wrap gap-4">
        <h1 class="text-4xl font-montserrat font-semibold text-grey-darker">
          {{ athleteFullName }}
        </h1>
        <BackLink />
      </div>
    </div>

    <Loader v-if="isLoading" />

    <template v-else-if="athlete">

      <!-- Statut blessé -->
      <div class="bg-white rounded-xl border border-grey overflow-hidden" style="box-shadow: var(--shadow-bold)">
        <div class="flex items-center gap-3 px-6 py-4 bg-green-lighter border-b border-green-light">
          <span class="block w-1.5 h-7 rounded-full bg-green"></span>
          <h2 class="font-montserrat font-semibold text-green-dark text-base">Statut</h2>
        </div>
        <div class="p-6 flex items-center gap-3">
          <input
            id="injured-toggle"
            type="checkbox"
            :checked="athlete.isInjured"
            @change="handleToggleInjured"
            class="w-5 h-5 cursor-pointer"
          />
          <label for="injured-toggle" class="cursor-pointer font-montserrat text-grey-darker">
            {{ t('pages.athletes.detail.injuredLabel') }}
          </label>
        </div>
      </div>

      <!-- Journal de suivi -->
      <div class="bg-white rounded-xl border border-grey overflow-hidden" style="box-shadow: var(--shadow-bold)">
        <div class="flex items-center gap-3 px-6 py-4 bg-green-lighter border-b border-green-light">
          <span class="block w-1.5 h-7 rounded-full bg-green"></span>
          <h2 class="font-montserrat font-semibold text-green-dark text-base">
            {{ t('pages.athletes.detail.notesTitle') }}
          </h2>
        </div>
        <div class="p-6 flex flex-col gap-4">

          <!-- Liste des notes existantes -->
          <div v-if="notes.length > 0" class="flex flex-col gap-2">
            <div
              v-for="note in notes"
              :key="note.id"
              class="font-mono text-sm text-grey-darker border-b border-grey-light pb-2"
            >
              {{ formatDate(note.createdAt) }} — {{ note.contenu }}
            </div>
          </div>
          <p v-else class="text-sm text-grey-dark italic">
            {{ t('pages.athletes.detail.notesEmpty') }}
          </p>

          <!-- Formulaire d'ajout -->
          <div class="flex flex-col gap-2 pt-4 border-t border-grey-light">
            <textarea
              v-model="newNote"
              :placeholder="t('pages.athletes.detail.notePlaceholder')"
              rows="3"
              class="form__field w-full border border-grey rounded p-3 text-sm font-montserrat resize-none focus:outline-none focus:border-green"
            ></textarea>
            <button
              class="btn self-end"
              :disabled="!newNote.trim() || isSubmitting"
              @click="handleAddNote"
            >
              {{ t('pages.athletes.detail.noteAddButton') }}
            </button>
          </div>

        </div>
      </div>

    </template>

  </div>
</template>

<script lang="ts" setup>
import { ref, computed, onMounted } from 'vue'
import { useI18n } from 'vue3-i18n'
import { useAthleteService } from '@/inversify.config'
import { notifySuccess, notifyError } from '@/notify'
import { Athlete, NoteBlessure } from '@/types/entities'
import BackLink from '@/components/layouts/items/BackLink.vue'
import Loader from '@/components/layouts/items/Loader.vue'

const props = defineProps<{ id: string }>()
const { t } = useI18n()
const athleteService = useAthleteService()

const isLoading = ref(true)
const isSubmitting = ref(false)
const athlete = ref<Athlete | null>(null)
const notes = ref<NoteBlessure[]>([])
const newNote = ref('')

const athleteFullName = computed(() =>
  athlete.value ? `${athlete.value.firstName} ${athlete.value.lastName}` : ''
)

onMounted(async () => {
  const [athleteData, notesData] = await Promise.all([
    athleteService.getById(props.id),
    athleteService.getNotes(props.id)
  ])
  athlete.value = athleteData
  notes.value = notesData
  isLoading.value = false
})

async function handleToggleInjured(event: Event) {
  const isInjured = (event.target as HTMLInputElement).checked
  const result = await athleteService.toggleInjured(props.id, isInjured)
  if (result.succeeded) {
    if (athlete.value) athlete.value.isInjured = isInjured
    notifySuccess(t('pages.athletes.detail.injuredToggleSuccess'))
  } else {
    notifyError(t('pages.athletes.detail.injuredToggleFailed'))
    // Revert the checkbox if failed
    if (athlete.value) athlete.value.isInjured = !isInjured
  }
}

async function handleAddNote() {
  if (!newNote.value.trim() || isSubmitting.value) return
  isSubmitting.value = true
  const result = await athleteService.createNote(props.id, { contenu: newNote.value.trim() })
  if (result.succeeded) {
    notifySuccess(t('pages.athletes.detail.noteAddSuccess'))
    newNote.value = ''
    notes.value = await athleteService.getNotes(props.id)
  } else {
    notifyError(t('pages.athletes.detail.noteAddFailed'))
  }
  isSubmitting.value = false
}

function formatDate(dateStr?: string): string {
  if (!dateStr) return ''
  return new Intl.DateTimeFormat('fr-CA', {
    year: 'numeric',
    month: '2-digit',
    day: '2-digit'
  }).format(new Date(dateStr))
}
</script>
```

**Step 2: Commit**

```bash
git add src/Web/vue-app/src/views/admin/athletes/AdminAthleteDetail.vue
git commit -m "feat: add AdminAthleteDetail page with notes journal"
```

---

## Task 17: Frontend — Add `view` action in `AdminAthleteIndex.vue`

**Files:**
- Modify: `src/Web/vue-app/src/views/admin/athletes/AdminAthleteIndex.vue`

**Step 1: Update `tableAthletes` computed**

Change the `tableAthletes` computed to add the `view` action:

```typescript
const tableAthletes = computed(() =>
  pageAthletes.value.map((x: Athlete) => ({
    id: x.id,
    firstName: x.firstName,
    lastName: x.lastName,
    email: x.email,
    team: undefined,
    actions: {
      view: { name: 'admin.children.athletes.detail', params: { id: x.id } },
    },
  }))
)
```

**Step 2: Add `actions` column to `athleteHeaders`**

```typescript
const athleteHeaders = computed(() => [
  {text: t("global.firstName"), value: 'firstName', width: 150},
  {text: t("global.lastName"), value: 'lastName', width: 150},
  {text: t("global.email"), value: 'email', width: 200},
  {text: t("global.team"), value: 'team', width: 150},
  {text: t("global.table.actions"), value: 'actions', width: 100},
])
```

**Step 3: Commit**

```bash
git add src/Web/vue-app/src/views/admin/athletes/AdminAthleteIndex.vue
git commit -m "feat: add view action to athlete index table"
```

---

## Task 18: Frontend — Add injured players section to Dashboard

**Files:**
- Modify: `src/Web/vue-app/src/views/admin/AdminDashboard.vue`

**Step 1: Add `injuredAthletes` ref and load function**

In the `<script>` section, add:

```typescript
const injuredAthletes = ref<Athlete[]>([])

// In onMounted, add:
injuredAthletes.value = await athleteService.getInjured()
```

Also add a computed for filtering by selected team:
```typescript
const injuredAthletesFiltered = computed(() => {
  if (!selectedTeamId.value) return injuredAthletes.value
  // For now show all since athletes don't have teamId yet — filter client-side when available
  return injuredAthletes.value
})
```

**Step 2: Add the card in the template**

Inside the `<div v-else class="admin-dashboard__tiles">`, add a new `<Card>` after the existing ones:

```html
<Card :title="t('pages.athletes.injuredSection.title')">
  <div v-if="injuredAthletes.length > 0" class="flex flex-col gap-2">
    <div
      v-for="athlete in injuredAthletes"
      :key="athlete.id"
      class="flex items-center justify-between py-2 border-b border-grey-light"
    >
      <span class="font-montserrat text-grey-darker text-sm">
        {{ athlete.firstName }} {{ athlete.lastName }}
      </span>
      <router-link
        :to="{ name: 'admin.children.athletes.detail', params: { id: athlete.id } }"
        class="text-green text-sm font-montserrat underline"
      >
        {{ t('global.actions.view') }}
      </router-link>
    </div>
  </div>
  <p v-else class="content-grid__text">
    {{ t('pages.athletes.injuredSection.empty') }}
  </p>
</Card>
```

**Step 3: Commit**

```bash
git add src/Web/vue-app/src/views/admin/AdminDashboard.vue
git commit -m "feat: add injured players section to dashboard"
```

---

## Final verification

1. Build the backend: `dotnet build src/Periscope.sln`
2. Run migrations: `dotnet ef database update --project src/Persistence --startup-project src/Web`
3. Start the API and test the new endpoints manually (Postman / browser)
4. Build the frontend: `cd src/Web/vue-app && npm run build`
5. Test the athlete detail page via the athlete list view action
6. Test toggling injured status and adding notes
7. Verify the dashboard shows injured athletes
