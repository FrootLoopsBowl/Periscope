using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using Application.Interfaces.Services.Notifications;
using Application.Settings;
using Domain.Entities;
using Domain.Repositories;
using FastEndpoints;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Options;

namespace Web.Features.Admins.Athletes.ImportAthletes;

public class ImportAthletesEndpoint : Endpoint<ImportAthletesRequest, ImportAthletesResponse>
{
    private static readonly Regex EmailRegex = new(
        @"^[^\s@]+@[^\s@]+\.[^\s@]+$",
        RegexOptions.Compiled);

    private readonly string _publicBaseUrl;
    private readonly IAthleteRepository _athleteRepository;
    private readonly ITeamRepository _teamRepository;
    private readonly INotificationService _notificationService;

    public ImportAthletesEndpoint(
        IAthleteRepository athleteRepository,
        ITeamRepository teamRepository,
        INotificationService notificationService,
        IOptions<ApplicationSettings> applicationSettings)
    {
        _publicBaseUrl = string.IsNullOrWhiteSpace(applicationSettings.Value.PublicBaseUrl)
            ? applicationSettings.Value.BaseUrl
            : applicationSettings.Value.PublicBaseUrl;
        _athleteRepository = athleteRepository;
        _teamRepository = teamRepository;
        _notificationService = notificationService;
    }

    public override void Configure()
    {
        DontCatchExceptions();
        Post("athletes/import");
        Roles(Domain.Constants.User.Roles.ADMINISTRATOR);
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
        AllowFileUploads();
    }

    public override async Task HandleAsync(ImportAthletesRequest req, CancellationToken ct)
    {
        var response = new ImportAthletesResponse();

        var lines = await ReadLinesAsync(req.File, ct);
        var startIndex = 0;
        char separator = ',';

        if (lines.Count > 0 && lines[0].TrimStart().StartsWith("sep=", StringComparison.OrdinalIgnoreCase))
        {
            var sepLine = lines[0].TrimStart().Substring(4).Trim();
            if (sepLine.Length > 0)
                separator = sepLine[0];
            startIndex = 1;
        }

        if (lines.Count <= startIndex)
        {
            await Send.OkAsync(response, cancellation: ct);
            return;
        }

        // Auto-detect separator from header if not specified via sep=
        var headerLine = lines[startIndex];
        if (headerLine.Count(c => c == ';') > headerLine.Count(c => c == ','))
            separator = ';';
        else if (headerLine.Contains(','))
            separator = ',';

        // Skip header
        startIndex++;

        var baseUrl = _publicBaseUrl.TrimEnd('/');
        var relativePath = req.AthletePageRelativeUrl.Trim('/');

        for (var i = startIndex; i < lines.Count; i++)
        {
            var rawLine = lines[i];
            var rowNumber = i + 1;

            if (string.IsNullOrWhiteSpace(rawLine))
                continue;

            var fields = ParseCsvLine(rawLine, separator);
            if (fields.Count < 5)
            {
                response.Errors.Add(new ImportAthletesError
                {
                    Row = rowNumber,
                    Message = "Ligne invalide: 5 colonnes attendues."
                });
                continue;
            }

            var firstName = fields[0].Trim();
            var lastName = fields[1].Trim();
            var email = fields[2].Trim();
            var dateStr = fields[3].Trim();
            var teamName = fields[4].Trim();

            if (string.IsNullOrWhiteSpace(firstName) || string.IsNullOrWhiteSpace(lastName) || string.IsNullOrWhiteSpace(email))
            {
                response.Errors.Add(new ImportAthletesError
                {
                    Row = rowNumber,
                    Email = email,
                    Message = "Prénom, nom et courriel sont requis."
                });
                continue;
            }

            if (!EmailRegex.IsMatch(email))
            {
                response.Errors.Add(new ImportAthletesError
                {
                    Row = rowNumber,
                    Email = email,
                    Message = "Format de courriel invalide."
                });
                continue;
            }

            if (!DateTime.TryParseExact(dateStr, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var dateOfBirth))
            {
                response.Errors.Add(new ImportAthletesError
                {
                    Row = rowNumber,
                    Email = email,
                    Message = "Date de naissance invalide (format attendu: AAAA-MM-JJ)."
                });
                continue;
            }

            dateOfBirth = DateTime.SpecifyKind(dateOfBirth, DateTimeKind.Utc);

            Guid? teamId = null;
            if (!string.IsNullOrWhiteSpace(teamName))
            {
                var team = await _teamRepository.FindByNameNormalizedAsync(teamName);
                if (team is null)
                {
                    team = new Team(teamName);
                    await _teamRepository.CreateAsync(team);
                }
                teamId = team.Id;
            }

            var existing = await _athleteRepository.FindByEmailAsync(email);
            if (existing is not null)
            {
                existing.SetFirstName(firstName);
                existing.SetLastName(lastName);
                existing.SetDateOfBirth(dateOfBirth);
                if (teamId.HasValue)
                    existing.AssignTeam(teamId.Value);
                else
                    existing.RemoveTeam();
                await _athleteRepository.UpdateAsync(existing);
                response.UpdatedCount++;
            }
            else
            {
                var athlete = new Athlete(firstName, lastName, email, dateOfBirth);
                if (teamId.HasValue)
                    athlete.AssignTeam(teamId.Value);
                await _athleteRepository.CreateAsync(athlete);

                var athleteLink = $"{baseUrl}/{relativePath}/{athlete.SubmissionToken}";
                var emailResponse = await _notificationService.SendAthleteAccessNotification(athlete.Email, athleteLink);

                if (!emailResponse.Succeeded)
                {
                    response.Errors.Add(new ImportAthletesError
                    {
                        Row = rowNumber,
                        Email = email,
                        Message = "Athlète créé mais l'envoi du courriel d'accès a échoué."
                    });
                }

                response.CreatedCount++;
            }
        }

        await Send.OkAsync(response, cancellation: ct);
    }

    private static async Task<List<string>> ReadLinesAsync(Microsoft.AspNetCore.Http.IFormFile file, CancellationToken ct)
    {
        var lines = new List<string>();
        using var stream = file.OpenReadStream();
        using var reader = new StreamReader(stream, Encoding.UTF8, detectEncodingFromByteOrderMarks: true);
        while (!reader.EndOfStream)
        {
            var line = await reader.ReadLineAsync(ct);
            if (line is not null)
                lines.Add(line);
        }
        return lines;
    }

    private static List<string> ParseCsvLine(string line, char separator)
    {
        var fields = new List<string>();
        var current = new StringBuilder();
        var inQuotes = false;

        for (var i = 0; i < line.Length; i++)
        {
            var c = line[i];
            if (inQuotes)
            {
                if (c == '"')
                {
                    if (i + 1 < line.Length && line[i + 1] == '"')
                    {
                        current.Append('"');
                        i++;
                    }
                    else
                    {
                        inQuotes = false;
                    }
                }
                else
                {
                    current.Append(c);
                }
            }
            else
            {
                if (c == separator)
                {
                    fields.Add(current.ToString());
                    current.Clear();
                }
                else if (c == '"')
                {
                    inQuotes = true;
                }
                else
                {
                    current.Append(c);
                }
            }
        }
        fields.Add(current.ToString());
        return fields;
    }
}
