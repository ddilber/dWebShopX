using System.Text;
using dWebShop.Application.Services;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Configuration;
using MimeKit;

namespace dWebShop.Infrastructure.Services;

public class SmtpEmailService : IEmailService
{
    private readonly IConfiguration _configuration;

    public SmtpEmailService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    // Public site (dWebShop.Web) and admin site (dWebShop.Admin) base URLs, from config.
    private string SiteUrl => (_configuration["SiteUrl"] ?? "https://asgifiks.ba").TrimEnd('/');
    private string AdminUrl => (_configuration["AdminUrl"] ?? "https://admin.asgifiks.ba").TrimEnd('/');

    public async Task SendRegistrationNotificationToAdminAsync(string adminEmail, string userEmail, string userName)
    {
        var subject = "New client registration pending approval";
        var body = $"<p>A new client has registered and is awaiting your approval.</p>" +
                   $"<p><strong>Name:</strong> {userName}<br/>" +
                   $"<strong>Email:</strong> {userEmail}</p>" +
                   $"<p><a href=\"{AdminUrl}/users\">Open the admin panel</a> to approve or reject this registration.</p>";

        await SendEmailAsync(adminEmail, subject, body);
    }

    public async Task SendApprovalConfirmationToClientAsync(string clientEmail, string clientName)
    {
        var subject = "Your account has been approved";
        var body = $"<p>Dear {clientName},</p>" +
                   $"<p>Your account has been approved. You can now <a href=\"{SiteUrl}/login\">log in to the portal</a>.</p>";

        await SendEmailAsync(clientEmail, subject, body);
    }

    public async Task SendContactFormAsync(string adminEmail, string senderName, string senderEmail, string phone, string message)
    {
        var subject = $"Contact form message from {senderName}";
        var body = $"<p>A visitor has submitted the contact form.</p>" +
                   $"<p><strong>Name:</strong> {senderName}<br/>" +
                   $"<strong>Email:</strong> {senderEmail}<br/>" +
                   $"<strong>Phone:</strong> {(string.IsNullOrWhiteSpace(phone) ? "—" : phone)}</p>" +
                   $"<p><strong>Message:</strong><br/>{System.Net.WebUtility.HtmlEncode(message).Replace("\n", "<br/>")}</p>";

        await SendEmailAsync(adminEmail, subject, body);
    }

    public async Task SendOrderConfirmationToClientAsync(string clientEmail, string clientName, Guid orderGuid, IEnumerable<OrderEmailItem> items, decimal total, string deliveryAddress)
    {
        var subject = $"Order Confirmation #{orderGuid.ToString()[..8].ToUpper()}";
        var body = BuildOrderEmailBody(
            $"<p>Dear {clientName},</p><p>Thank you for your order! Here is a summary:</p>",
            orderGuid, items, total,
            $"<p><strong>Delivery Address:</strong> {System.Net.WebUtility.HtmlEncode(deliveryAddress)}</p>");

        await SendEmailAsync(clientEmail, subject, body);
    }

    public async Task SendOrderConfirmationToAdminAsync(string adminEmail, string clientName, string clientEmail, Guid orderGuid, IEnumerable<OrderEmailItem> items, decimal total)
    {
        var subject = $"New Order #{orderGuid.ToString()[..8].ToUpper()} from {clientName}";
        var body = BuildOrderEmailBody(
            $"<p>A new order has been placed by <strong>{System.Net.WebUtility.HtmlEncode(clientName)}</strong> ({System.Net.WebUtility.HtmlEncode(clientEmail)}).</p>",
            orderGuid, items, total, string.Empty);

        await SendEmailAsync(adminEmail, subject, body);
    }

    public async Task SendOrderStatusChangedToClientAsync(string clientEmail, string clientName, Guid orderGuid, string newStatus)
    {
        var subject = $"Order #{orderGuid.ToString()[..8].ToUpper()} Status Update";
        var body = $"<p>Dear {System.Net.WebUtility.HtmlEncode(clientName)},</p>" +
                   $"<p>Your order <strong>#{orderGuid.ToString()[..8].ToUpper()}</strong> status has been updated to: <strong>{System.Net.WebUtility.HtmlEncode(newStatus)}</strong>.</p>";

        await SendEmailAsync(clientEmail, subject, body);
    }

    private static string BuildOrderEmailBody(string intro, Guid orderGuid, IEnumerable<OrderEmailItem> items, decimal total, string extra)
    {
        var sb = new StringBuilder();
        sb.Append(intro);
        sb.Append($"<p><strong>Order Reference:</strong> #{orderGuid.ToString()[..8].ToUpper()}</p>");
        sb.Append("<table border='1' cellpadding='6' cellspacing='0' style='border-collapse:collapse;'>");
        sb.Append("<tr><th>Product</th><th>SKU</th><th>Qty</th><th>Unit Price</th><th>Line Total</th></tr>");
        foreach (var item in items)
        {
            sb.Append($"<tr><td>{System.Net.WebUtility.HtmlEncode(item.Name)}</td><td>{System.Net.WebUtility.HtmlEncode(item.SKU)}</td>" +
                      $"<td>{item.Quantity:F2}</td><td>{item.Price:F2}</td><td>{item.Quantity * item.Price:F2}</td></tr>");
        }
        sb.Append($"<tr><td colspan='4' style='text-align:right'><strong>Total</strong></td><td><strong>{total:F2}</strong></td></tr>");
        sb.Append("</table>");
        if (!string.IsNullOrEmpty(extra)) sb.Append(extra);
        return sb.ToString();
    }

    private async Task SendEmailAsync(string toEmail, string subject, string htmlBody)
    {
        var host = _configuration["Email:Host"] ?? "localhost";
        var port = int.Parse(_configuration["Email:Port"] ?? "25");
        var username = _configuration["Email:Username"];
        var password = _configuration["Email:Password"];
        var senderEmail = _configuration["Email:SenderEmail"] ?? "noreply@dwebshop.local";
        var senderName = _configuration["Email:SenderName"] ?? "dWebShop";

        var message = new MimeMessage();
        message.From.Add(new MailboxAddress(senderName, senderEmail));
        message.To.Add(MailboxAddress.Parse(toEmail));
        message.Subject = subject;
        message.Body = new TextPart("html") { Text = htmlBody };

        using var client = new SmtpClient();
        var secureSocketOptions = (port == 465)
            ? SecureSocketOptions.SslOnConnect
            : SecureSocketOptions.StartTlsWhenAvailable;

        await client.ConnectAsync(host, port, secureSocketOptions);

        if (!string.IsNullOrEmpty(username) && !string.IsNullOrEmpty(password))
            await client.AuthenticateAsync(username, password);

        await client.SendAsync(message);
        await client.DisconnectAsync(true);
    }
}
