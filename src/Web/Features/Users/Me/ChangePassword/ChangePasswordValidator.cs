using FastEndpoints;
using FluentValidation;

namespace Web.Features.Users.Me.ChangePassword;

public class ChangePasswordValidator : Validator<ChangePasswordRequest>
{
    private const string DisallowedPassword = "Qwerty123!";

    public ChangePasswordValidator()
    {
        RuleFor(x => x.CurrentPassword)
            .NotNull()
            .NotEmpty()
            .WithErrorCode("InvalidCurrentPassword")
            .WithMessage("Current password should not be null or empty.");

        RuleFor(x => x.NewPassword)
            .NotNull()
            .NotEmpty()
            .WithErrorCode("InvalidNewPassword")
            .WithMessage("New password should not be null or empty.");

        RuleFor(x => x.NewPasswordConfirmation)
            .NotNull()
            .NotEmpty()
            .WithErrorCode("InvalidNewPasswordConfirmation")
            .WithMessage("New password confirmation should not be null or empty.")
            .Equal(x => x.NewPassword)
            .WithErrorCode("PasswordAndConfirmationMustMatch")
            .WithMessage("The password and its confirmation must match.");

        RuleFor(x => x.NewPassword)
            .Must(password => !string.Equals(password, DisallowedPassword, StringComparison.OrdinalIgnoreCase))
            .WithErrorCode("PasswordTooPredictable")
            .WithMessage("Please choose a password that is less predictable.");
    }
}
