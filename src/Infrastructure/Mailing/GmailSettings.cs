namespace Infrastructure.Mailing;

public class GmailSettings
{
    public string SmtpHost { get; set; } = "smtp.gmail.com";
    public int SmtpPort { get; set; } = 587;
    public string EmailAddress { get; set; } = null!;
    public string AppPassword { get; set; } = null!;
    public bool EnableSsl { get; set; } = true;
}
