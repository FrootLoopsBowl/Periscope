using Application.Interfaces.Mailing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure.Mailing;

public static class MailingInitializer
{
    public static void Configure(
        IServiceCollection services,
        IConfiguration configuration)
    {
        services.Configure<MailingSettings>(configuration.GetSection("Mailing"));
        services.Configure<GmailSettings>(configuration.GetSection("Gmail"));

        services.AddTransient<IEmailSender, GmailSmtpSender>();
    }
}
