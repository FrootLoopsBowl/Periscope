# Design — Calendrier d'événements par équipe

**Date:** 2026-04-20
**Branche:** calendrier-evenements

## Contexte

Le client veut pouvoir planifier des événements (Pratique, Match) pour chaque équipe via un calendrier style Google Calendar. Les événements doivent aussi être visibles en lecture seule sur le profil de chaque joueur appartenant à l'équipe.

## Décisions clés

- Seul l'admin peut créer, modifier et supprimer des événements
- Les événements ont : type (Pratique | Match) + date + heure début + heure fin — pas de description ni lieu
- Vue calendrier : mois uniquement
- Création via clic sur un jour du calendrier (date pré-remplie dans le modal)
- Librairie : `vue-cal` (Vue 3 native, ~50KB)
- Les événements appartiennent à l'équipe ; les joueurs les voient via leur `TeamId`

---

## Modèle de données

### Nouvelle entité : `TeamEvent`

```csharp
public class TeamEvent
{
    public Guid Id { get; private set; }
    public Guid TeamId { get; private set; }
    public Team Team { get; private set; }
    public EventType Type { get; private set; }
    public DateTime StartDateTime { get; private set; }
    public DateTime EndDateTime { get; private set; }
    public DateTime CreatedAt { get; private set; }
}

public enum EventType { Pratique = 0, Match = 1 }
```

### Migration EF Core

Nouvelle table `TeamEvents` avec FK vers `Teams`.

---

## Backend

### Repository

**Interface :** `src/Domain/Repositories/ITeamEventRepository.cs`
```csharp
Task CreateAsync(TeamEvent teamEvent);
Task<IEnumerable<TeamEvent>> GetByTeamIdAndRangeAsync(Guid teamId, DateTime from, DateTime to);
Task<TeamEvent?> FindByIdAsync(Guid id);
Task UpdateAsync(TeamEvent teamEvent);
Task DeleteAsync(TeamEvent teamEvent);
```

**Implémentation :** `src/Infrastructure/Repositories/TeamEvents/TeamEventRepository.cs`

### Endpoints FastEndpoints (admin, JWT requis)

| Méthode | Route | Endpoint |
|---|---|---|
| GET | `/api/teams/{teamId}/events?from=&to=` | `GetTeamEventsEndpoint` |
| POST | `/api/teams/{teamId}/events` | `CreateTeamEventEndpoint` |
| PUT | `/api/teams/{teamId}/events/{eventId}` | `UpdateTeamEventEndpoint` |
| DELETE | `/api/teams/{teamId}/events/{eventId}` | `DeleteTeamEventEndpoint` |

**Dossier :** `src/Web/Features/Admins/Teams/Events/`

**Validation :**
- `StartDateTime` < `EndDateTime`
- `Type` doit être `Pratique` ou `Match`
- L'événement doit appartenir à l'équipe spécifiée dans la route (vérification avant update/delete)

**Réponse GET :** liste de `TeamEventResponse { Id, Type (string : "Pratique" | "Match"), StartDateTime (ISO 8601), EndDateTime (ISO 8601) }`

> L'enum `EventType` est sérialisé en string via `JsonStringEnumConverter` pour que le frontend reçoive `"Pratique"` et `"Match"`, pas `0` et `1`.

---

## Frontend

### Nouveau service : `teamEventService.ts`

```typescript
interface ITeamEventService {
    getEvents(teamId: string, from: string, to: string): Promise<TeamEvent[]>
    createEvent(teamId: string, payload: CreateTeamEventRequest): Promise<SucceededOrNotResponse>
    updateEvent(teamId: string, eventId: string, payload: UpdateTeamEventRequest): Promise<SucceededOrNotResponse>
    deleteEvent(teamId: string, eventId: string): Promise<SucceededOrNotResponse>
}
```

### Nouveau type : `TeamEvent`

```typescript
export class TeamEvent {
    id?: string
    type?: 'Pratique' | 'Match'
    startDateTime?: string
    endDateTime?: string
}
```

### Composant `TeamCalendar.vue`

- Intègre `vue-cal` en vue mois avec navigation mensuelle
- Couleurs : Pratique = `#4caf50` (vert), Match = `#1565c0` (bleu)
- Chargement des événements à l'initialisation et à chaque changement de mois
- **Clic sur case vide** → modal de création avec date pré-remplie
- **Clic sur événement existant** → modal édition/suppression
- Notifications `notifySuccess()` / `notifyError()` sur chaque opération CRUD
- Intégré dans `AdminTeamDetail.vue` comme nouvelle section en bas de page

### Composant `AthleteCalendar.vue`

- Même vue mois `vue-cal`, sans interactions de création/édition
- Reçoit `teamId` en prop
- Si `teamId` est null : affiche "Aucune équipe assignée"
- Intégré dans `AdminAthleteDetail.vue` sous la section "Suivi hebdomadaire"

### i18n

Nouvelles clés dans `fr.json` et `en.json` :
```
pages.teams.calendar.title
pages.teams.calendar.addEvent
pages.teams.calendar.editEvent
pages.teams.calendar.deleteEvent
pages.teams.calendar.typeLabel
pages.teams.calendar.startLabel
pages.teams.calendar.endLabel
pages.teams.calendar.saveSuccess
pages.teams.calendar.saveError
pages.teams.calendar.deleteSuccess
pages.teams.calendar.deleteError
pages.athletes.calendar.title
pages.athletes.calendar.noTeam
```

---

## Flux de données

1. **Chargement :** À l'initialisation et à chaque navigation mensuelle → `GET /api/teams/{teamId}/events?from=&to=`
2. **Création :** Clic sur jour → modal (date pré-remplie) → `POST` → ajout local de l'événement
3. **Édition :** Clic sur événement → modal pré-rempli → `PUT` → mise à jour locale
4. **Suppression :** Modal d'édition → bouton supprimer → `DELETE` → retrait local

Pas de rechargement complet de la page pour chaque opération.

## Gestion d'erreurs

- Erreurs réseau : `notifyError()` + message dans la zone calendrier
- Validation backend : erreurs affichées dans le modal
- Athlète sans équipe : message informatif, pas d'erreur

## Fichiers créés / modifiés

| Fichier | Action |
|---|---|
| `src/Domain/Entities/TeamEvent.cs` | Créer |
| `src/Domain/Enums/EventType.cs` | Créer |
| `src/Domain/Repositories/ITeamEventRepository.cs` | Créer |
| `src/Infrastructure/Repositories/TeamEvents/TeamEventRepository.cs` | Créer |
| `src/Persistence/GarneauTemplateDbContext.cs` | Modifier (ajouter DbSet) |
| `src/Persistence/Migrations/` | Nouvelle migration |
| `src/Web/Features/Admins/Teams/Events/` | Créer (4 endpoints) |
| `src/Web/vue-app/src/types/entities/teamEvent.ts` | Créer |
| `src/Web/vue-app/src/services/teamEventService.ts` | Créer |
| `src/Web/vue-app/src/injection/interfaces.ts` | Modifier (ajouter interface + type) |
| `src/Web/vue-app/src/inversify.config.ts` | Modifier (enregistrer service) |
| `src/Web/vue-app/src/components/calendar/TeamCalendar.vue` | Créer |
| `src/Web/vue-app/src/components/calendar/AthleteCalendar.vue` | Créer |
| `src/Web/vue-app/src/views/admin/teams/AdminTeamDetail.vue` | Modifier |
| `src/Web/vue-app/src/views/admin/athletes/AdminAthleteDetail.vue` | Modifier |
| `src/Web/vue-app/src/locales/fr.json` | Modifier |
| `src/Web/vue-app/src/locales/en.json` | Modifier |
