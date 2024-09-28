namespace ASPNETMaker2024.Models;

// Partial class
public partial class PointofSales {
    /// <summary>
    /// Email class
    /// </summary>
    public class AzureEmail : EmailBase
    {
        public static string ConnectionString = "";

        // Convert email address to EmailAddress
        public EmailAddress ConvertToEmailAddress(string email)
        {
            email = email.Trim();
            var m = Regex.Match(email, @"^(.+)<([\w.%+-]+@[\w.-]+\.[A-Z]{2,6})>$", RegexOptions.IgnoreCase);
            if (m.Success)
                return new EmailAddress(m.Groups[2].Value.Trim(), m.Groups[1].Value);
            return new EmailAddress(email);
        }

        // Send email
        public override bool Send() => SendAsync().GetAwaiter().GetResult();

        // Send email
        public override async Task<bool> SendAsync()
        {
            if (Mailer == null && AzureEmail.ConnectionString == "")
                throw new Exception("Missing Azure Communication Services connection string.");
            var emailClient = Mailer != null ? (EmailClient)Mailer : new EmailClient(AzureEmail.ConnectionString);
            var emailContent = new EmailContent(Subject);
            if (SameText(Format, "html")) {
                emailContent.Html = Content;
            } else {
                emailContent.PlainText = Content;
            }
            var emailRecipients = new EmailRecipients(Recipient.Replace(';', ',').Split(',').Select(x => ConvertToEmailAddress(x)).ToList(), // Recipients
                Cc.Replace(';', ',').Split(',').Select(x => ConvertToEmailAddress(x)).ToList(), // Cc
                Bcc.Replace(';', ',').Split(',').Select(x => ConvertToEmailAddress(x)).ToList()); // Bcc
            var emailMessage = new EmailMessage(Sender, emailRecipients, emailContent); // Note: EmailMessage does not support LinkedResources
            Attachments?.ForEach(attachment => {
                if (attachment.TryGetValue("filename", out string? filename)) {
                    if (attachment.TryGetValue("content", out string? content))
                        emailMessage.Attachments.Add(new EmailAttachment(Path.GetFileName(filename), "text/plain", BinaryData.FromBytes(Encoding.UTF8.GetBytes(content))));
                    else
                        emailMessage.Attachments.Add(new EmailAttachment(Path.GetFileName(filename), "application/octet-stream", BinaryData.FromStream(File.OpenRead(filename))));
                }
            });
            try {
                SendError = "";
                var emailSendOperation = await emailClient.SendAsync(Azure.WaitUntil.Completed, emailMessage);
                // Get the Operation ID so that it can be used for tracking the message for troubleshooting
                if (Config.Debug) {
                    string operationId = emailSendOperation.Id;
                    Log($"Email operation id = {operationId}");
                }
                return true;
            } catch (Azure.RequestFailedException e) {
                SendError = $"Email send operation failed with error code: {e.ErrorCode}, message: {e.Message}";
                if (Config.Debug) {
                    SetDebugMessage(SendError);
                    Log(SendError);
                }
                return false;
            }
        }
    }
} // End Partial class
