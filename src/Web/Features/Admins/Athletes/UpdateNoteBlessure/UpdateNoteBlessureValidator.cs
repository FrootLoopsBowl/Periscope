using FastEndpoints;
using FluentValidation;

namespace Web.Features.Admins.Athletes.UpdateNoteBlessure;

public class UpdateNoteBlessureValidator : Validator<UpdateNoteBlessureRequest>
{
    public UpdateNoteBlessureValidator()
    {
        RuleFor(x => x.Contenu)
            .NotEmpty()
            .WithErrorCode("InvalidContenu")
            .WithMessage("Contenu should not be empty.");
    }
}
