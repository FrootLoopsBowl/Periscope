using FastEndpoints;
using FluentValidation;

namespace Web.Features.Public.Authentication.ResetPassword;

public class ResetPasswordValidator : Validator<ResetPasswordRequest>
{
    private const string DisallowedPassword = "Qwerty123!";

    public ResetPasswordValidator()
    {
        RuleFor(x => x.UserId)
            .NotEqual(Guid.Empty)
            .WithErrorCode("InvalidUserId")
            .WithMessage("User id should not be null or empty.");

        RuleFor(x => x.Token)
            .NotNull()
            .NotEmpty()
            .WithErrorCode("InvalidToken")
            .WithMessage("Token should not be null or empty.");

        RuleFor(x => x.Password)
            .NotNull()
            .NotEmpty()
            .WithErrorCode("InvalidPassword")
            .WithMessage("Password should not be null or empty.");

        RuleFor(x => x.PasswordConfirmation)
            .NotNull()
            .NotEmpty()
            .WithErrorCode("InvalidPasswordConfirmation")
            .WithMessage("Password should not be null or empty.")
            .Equal(x => x.Password)
            .WithErrorCode("PasswordAndConfirmationMustMatch")
            .WithMessage("The password and its confirmation must match.");

        RuleFor(x => x.Password)
            .Must(password => !string.Equals(password, DisallowedPassword, StringComparison.OrdinalIgnoreCase))
            .WithErrorCode("PasswordTooPredictable")
            .WithMessage("Please choose a password that is less predictable.");
    }
}
