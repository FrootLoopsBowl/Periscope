using FastEndpoints;
using FluentValidation;

namespace Web.Features.Admins.Athletes.CreateAthlete;

public class CreateAthleteValidator : Validator<CreateAthleteRequest>
{
    public CreateAthleteValidator()
    {
        RuleFor(x => x.FirstName)
            .NotNull()
            .NotEmpty()
            .WithErrorCode("InvalidFirstName")
            .WithMessage("First name should not be empty.");

        RuleFor(x => x.LastName)
            .NotNull()
            .NotEmpty()
            .WithErrorCode("InvalidLastName")
            .WithMessage("Last name should not be empty.");

        RuleFor(x => x.Email)
            .NotNull()
            .NotEmpty()
            .WithErrorCode("InvalidEmail")
            .WithMessage("Email should not be empty.")
            .EmailAddress()
            .WithErrorCode("InvalidEmailFormat")
            .WithMessage("Email format is invalid.");

        RuleFor(x => x.DateOfBirth)
            .NotEmpty()
            .WithErrorCode("InvalidDateOfBirth")
            .WithMessage("Date of birth should not be empty.");
    }
}
