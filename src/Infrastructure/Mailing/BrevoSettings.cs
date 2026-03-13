namespace Infrastructure.Mailing;

public class BrevoSettings
{
    public string SmtpHost { get; set; } = "smtp-relay.brevo.com";
    public int SmtpPort { get; set; } = 587;
    public string SmtpLogin { get; set; } = null!;
    public string SmtpKey { get; set; } = null!;
    public bool EnableSsl { get; set; } = true;
}
