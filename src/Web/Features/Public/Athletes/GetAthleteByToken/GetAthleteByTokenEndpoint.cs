using Domain.Repositories;
using FastEndpoints;

namespace Web.Features.Public.Athletes.GetAthleteByToken;

public class GetAthleteByTokenEndpoint : Endpoint<GetAthleteByTokenRequest, GetAthleteByTokenResponse>
{
    private readonly IAthleteRepository _athleteRepository;

    public GetAthleteByTokenEndpoint(IAthleteRepository athleteRepository)
    {
        _athleteRepository = athleteRepository;
    }

    public override void Configure()
    {
        DontCatchExceptions();
        Get("athletes/submission/{token}");
        AllowAnonymous();
    }

    public override async Task HandleAsync(GetAthleteByTokenRequest req, CancellationToken ct)
    {
        var athlete = await _athleteRepository.FindBySubmissionTokenAsync(req.Token);

        if (athlete is null)
        {
            await Send.NotFoundAsync(ct);
            return;
        }

        await Send.OkAsync(new GetAthleteByTokenResponse
        {
            FirstName = athlete.FirstName,
            LastName = athlete.LastName
        }, ct);
    }
}
