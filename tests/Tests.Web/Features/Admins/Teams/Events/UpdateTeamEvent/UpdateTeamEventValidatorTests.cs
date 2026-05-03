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
