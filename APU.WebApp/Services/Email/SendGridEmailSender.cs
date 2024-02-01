﻿using Microsoft.Extensions.Options;
using SendGrid;
using SendGrid.Helpers.Mail;

namespace APU.WebApp.Services.Email;

public class SendGridEmailSender : IEmailSender
{
    public SendGridEmailSender(
        IOptions<SendGridEmailSenderOptions> options
    )
    {
        this.Options = options.Value;
    }

    public SendGridEmailSenderOptions Options { get; set; }

    public async Task<bool> SendEmailAsync(string email, string subject, string message)
    {
        var response = await Execute(Options.ApiKey, subject, message, email);
        if (!response.IsSuccessStatusCode)
            return false;
        return true;
    }

    private async Task<Response> Execute(
        string apiKey,
        string subject,
        string message,
        string email)
    {
        var client = new SendGridClient(apiKey);
        var msg = new SendGridMessage()
        {
            From = new EmailAddress(Options.SenderEmail, Options.SenderName),
            Subject = subject,
            PlainTextContent = message,
            HtmlContent = message
        };
        msg.AddTo(new EmailAddress(email));

        // disable tracking settings
        // ref.: https://sendgrid.com/docs/User_Guide/Settings/tracking.html
        msg.SetClickTracking(false, false);
        msg.SetOpenTracking(false);
        msg.SetGoogleAnalytics(false);
        msg.SetSubscriptionTracking(false);

        return await client.SendEmailAsync(msg);
    }
}