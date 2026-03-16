using FastEndpoints;
using FluentValidation;

namespace Web.Features.Admins.Athletes.CreateNoteBlessure;

public class CreateNoteBlessureValidator : Validator<CreateNoteBlessureRequest>
{
    public CreateNoteBlessureValidator()
    {
        RuleFor(x => x.Contenu)
            .NotNull()
            .NotEmpty()
            .WithErrorCode("InvalidContenu")
            .WithMessage("Contenu should not be empty.");
    }
}
