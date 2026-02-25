using Domain.Common;
using Domain.Repositories;
using FastEndpoints;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Web.Features.Admins.Teams.CreateTeam;
using Web.Features.Common;

namespace Web.Features.Admins.Teams.GetAllTeams;

public class GetAllTeamsEndpoint : Endpoint<PaginateRequest, PaginatedList<TeamResponse>>
{
    private readonly ITeamRepository _teamRepository;

    public GetAllTeamsEndpoint(ITeamRepository teamRepository)
    {
        _teamRepository = teamRepository;
    }

    public override void Configure()
    {
        DontCatchExceptions();
        Get("teams");
        Roles(Domain.Constants.User.Roles.ADMINISTRATOR);
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
    }

    public override async Task HandleAsync(PaginateRequest req, CancellationToken ct)
    {
        var paginatedList = _teamRepository.GetAllPaginated(req.PageIndex, req.PageSize);
        var response = new PaginatedList<TeamResponse>(
            paginatedList.Items.Select(t => new TeamResponse
            {
                Id = t.Id,
                Name = t.Name,
                CreatedAt = t.CreatedAt
            }),
            paginatedList.TotalItems);
        await Send.OkAsync(response, cancellation: ct);
    }
}
