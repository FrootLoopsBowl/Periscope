using FluentValidation.TestHelper;
using Web.Features.Admins.Athletes.CreateNoteBlessure;

namespace Tests.Web.Features.Admins.Athletes.CreateNoteBlessure;

public class CreateNoteBlessureValidatorTests
{
    private readonly CreateNoteBlessureRequest _request = new()
    {
        Id = Guid.NewGuid(),
        Contenu = "Entorse cheville droite match"
    };
    private readonly CreateNoteBlessureValidator _validator = new();

    [Fact]
    public void GivenValidRequest_WhenValidate_ThenReturnNoErrors()
    {
        var validationResult = _validator.TestValidate(_request);
        validationResult.ShouldNotHaveAnyValidationErrors();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("\t")]
    public void GivenNullEmptyOrWhitespaceContenu_WhenValidate_ThenReturnError(string? contenu)
    {
        _request.Contenu = contenu!;
        var validationResult = _validator.TestValidate(_request);
        validationResult.ShouldHaveValidationErrorFor(x => x.Contenu);
    }
}
