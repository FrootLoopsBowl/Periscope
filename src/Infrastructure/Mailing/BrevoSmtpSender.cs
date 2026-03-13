using System.Net;
using System.Net.Mail;
using Application.Interfaces.Mailing;
using Application.Services.Notifications.Models;
using Domain.Common;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Infrastructure.Mailing;

public class BrevoSmtpSender : IEmailSender
{
    private readonly ILogger<BrevoSmtpSender> _logger;
    private readonly MailingSettings _mailingSettings;
    private readonly BrevoSettings _brevoSettings;

    public BrevoSmtpSender(
        ILogger<BrevoSmtpSender> logger,
        IOptions<MailingSettings> mailingSettings,
        IOptions<BrevoSettings> brevoSettings)
    {
        _logger = logger;
        _mailingSettings = mailingSettings.Value;
        _brevoSettings = brevoSettings.Value;
    }

    public async Task<SucceededOrNotResponse> SendAsync<TModel>(TModel model) where TModel : NotificationModel
    {
        try
        {
            var (subject, htmlBody) = BuildMessageContent(model);

            using var message = new MailMessage
            {
                From = new MailAddress(_mailingSettings.FromAddress, _mailingSettings.FromName),
                Subject = subject,
                Body = htmlBody,
                IsBodyHtml = true
            };
            message.To.Add(model.Destination);

            using var smtpClient = new SmtpClient(_brevoSettings.SmtpHost, _brevoSettings.SmtpPort)
            {
                EnableSsl = _brevoSettings.EnableSsl,
                Credentials = new NetworkCredential(_brevoSettings.SmtpLogin, _brevoSettings.SmtpKey)
            };

            await smtpClient.SendMailAsync(message);
            return new SucceededOrNotResponse(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occured while sending email with Brevo SMTP.");
            return new SucceededOrNotResponse(false, [
                new Error("BrevoError", ex.Message)
            ]);
        }
    }

    private static (string subject, string htmlBody) BuildMessageContent(NotificationModel model)
    {
        return model switch
        {
            ForgotPasswordNotificationModel forgotPassword => BuildForgotPasswordContent(forgotPassword),
            TwoFactorAuthenticationNotificationModel twoFactor => BuildTwoFactorContent(twoFactor),
            AthleteAccessNotificationModel athleteAccess => BuildAthleteAccessContent(athleteAccess),
            _ => ("Notification", "<p>Nouvelle notification.</p>")
        };
    }

    private static (string subject, string htmlBody) BuildForgotPasswordContent(ForgotPasswordNotificationModel model)
    {
        var isFr = model.Locale == "fr";
        var subject = isFr ? "Réinitialisation du mot de passe" : "Reset your password";
        var body = isFr
            ? $"<p>Bonjour,</p><p>Utilisez ce lien pour réinitialiser votre mot de passe:</p><p><a href=\"{model.Link}\">{model.Link}</a></p>"
            : $"<p>Hello,</p><p>Use this link to reset your password:</p><p><a href=\"{model.Link}\">{model.Link}</a></p>";
        return (subject, body);
    }

    private static (string subject, string htmlBody) BuildTwoFactorContent(TwoFactorAuthenticationNotificationModel model)
    {
        var isFr = model.Locale == "fr";
        var subject = isFr ? "Code d'authentification" : "Authentication code";
        var body = isFr
            ? $"<p>Votre code d'authentification est: <strong>{model.Code}</strong></p>"
            : $"<p>Your authentication code is: <strong>{model.Code}</strong></p>";
        return (subject, body);
    }

    private static (string subject, string htmlBody) BuildAthleteAccessContent(AthleteAccessNotificationModel model)
    {
        var isFr = model.Locale == "fr";
        var subject = isFr ? "Accès à votre page athlète" : "Access your athlete page";
        var body = isFr
            ? $"<p>Bonjour,</p><p>Utilisez ce lien pour accéder à votre page athlète:</p><p><a href=\"{model.Link}\">{model.Link}</a></p>"
            : $"<p>Hello,</p><p>Use this link to access your athlete page:</p><p><a href=\"{model.Link}\">{model.Link}</a></p>";
        return (subject, body);
    }
}
