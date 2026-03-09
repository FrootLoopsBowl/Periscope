using System.Text.Json;
using Application.Interfaces.Mailing;
using Application.Services.Notifications.Models;
using Domain.Common;
using Microsoft.Extensions.Logging;
using SendGrid;

namespace Infrastructure.Mailing;

public class SendGridSender : IEmailSender
{
    private readonly ILogger<SendGridSender> _logger;
    private readonly ISendGridClient _sendGridClient;
    private readonly ISendGridMessageFactory _sendGridMessageFactory;

    public SendGridSender(
        ILogger<SendGridSender> logger,
        ISendGridClient sendGridClient,
        ISendGridMessageFactory sendGridMessageFactory)
    {
        _logger = logger;
        _sendGridClient = sendGridClient;
        _sendGridMessageFactory = sendGridMessageFactory;
    }

    public async Task<SucceededOrNotResponse> SendAsync<TModel>(TModel model) where TModel : NotificationModel
    {
        var msg = _sendGridMessageFactory.CreateFromModel(model);
        var response = await _sendGridClient.SendEmailAsync(msg);

        if (response.IsSuccessStatusCode)
            return new SucceededOrNotResponse(response.IsSuccessStatusCode);

        var errors = await GetErrorListFromResponse(response);
        _logger.LogError("Error occured while sending email. Errors : {errors}", JsonSerializer.Serialize(errors));

        return new SucceededOrNotResponse(response.IsSuccessStatusCode, errors);
    }

    private async Task<List<Error>> GetErrorListFromResponse(Response response)
    {
        if (response.Body is null)
            return [new Error("SendGridError", "Unknown SendGrid error.")];

        var payload = await response.Body.ReadAsStringAsync();
        if (string.IsNullOrWhiteSpace(payload))
            return [new Error("SendGridError", "Unknown SendGrid error.")];

        using var jsonDocument = JsonDocument.Parse(payload);
        if (!jsonDocument.RootElement.TryGetProperty("errors", out var errorsElement) ||
            errorsElement.ValueKind != JsonValueKind.Array)
        {
            return [new Error("SendGridError", "Unknown SendGrid error.")];
        }

        var errors = new List<Error>();
        foreach (var errorElement in errorsElement.EnumerateArray())
        {
            string? message = null;
            if (errorElement.TryGetProperty("message", out var messageElement))
                message = messageElement.GetString();
            else if (errorElement.TryGetProperty("Message", out var pascalMessageElement))
                message = pascalMessageElement.GetString();

            if (string.IsNullOrWhiteSpace(message))
                message = "Unknown SendGrid error.";

            errors.Add(new Error("SendGridError", message));
        }

        return errors;
    }
}
