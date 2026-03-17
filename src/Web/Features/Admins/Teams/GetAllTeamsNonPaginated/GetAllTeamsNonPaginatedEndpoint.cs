using Domain.Repositories;
using FastEndpoints;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Web.Features.Admins.Teams.CreateTeam;

namespace Web.Features.Admins.Teams.GetAllTeamsNonPaginated;

public class GetAllTeamsNonPaginatedEndpoint : EndpointWithoutRequest<List<TeamResponse>>
{
    private readonly ITeamRepository _teamRepository;

    public GetAllTeamsNonPaginatedEndpoint(ITeamRepository teamRepository)
    {
        _teamRepository = teamRepository;
    }

    public override void Configure()
    {
        DontCatchExceptions();
        Get("teams/all");
        Roles(Domain.Constants.User.Roles.ADMINISTRATOR);
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var teams = await _teamRepository.GetAllAsync();
        var response = teams.Select(t => new TeamResponse
        {
            Id = t.Id,
            Name = t.Name,
            CreatedAt = t.CreatedAt
        }).ToList();
        await Send.OkAsync(response, cancellation: ct);
    }
}
