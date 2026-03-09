using Application.Services.Notifications.Models;
using AutoMapper;
using Microsoft.Extensions.Options;
using SendGrid.Helpers.Mail;

namespace Infrastructure.Mailing;

public class SendGridMessageFactory : ISendGridMessageFactory
{
    private readonly MailingSettings _mailingSettings;
    private readonly IMapper _mapper;

    public SendGridMessageFactory(IOptions<MailingSettings> mailingSettings, IMapper mapper)
    {
        _mapper = mapper;
        _mailingSettings = mailingSettings.Value;
    }

    public SendGridMessage CreateFromModel<TModel>(TModel model) where TModel : NotificationModel
    {
        var msg = new SendGridMessage
        {
            From = new EmailAddress(_mailingSettings.FromAddress, _mailingSettings.FromName),
            TemplateId = model.TemplateId()
        };

        if (model.Attachments.Any())
            msg.Attachments = model.Attachments.Select(x => _mapper.Map<Attachment>(x)).ToList();

        msg.SetTemplateData(model.TemplateData());

        msg.AddTo(model.Destination);

        return msg;
    }
}
