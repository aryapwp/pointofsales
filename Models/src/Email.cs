namespace ASPNETMaker2024.Models;

// Partial class
public partial class PointofSales {
    /// <summary>
    /// Email class
    /// </summary>
    public class Email : EmailBase
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public Email(): base()
        {
            Options = Enum.Parse<SecureSocketOptions>(Config.SmtpSecureOption);
        }

        // Convert email address to MailboxAddress
        public MailboxAddress ConvertToMailboxAddress(string email)
        {
            email = email.Trim();
            var m = Regex.Match(email, @"^(.+)<([\w.%+-]+@[\w.-]+\.[A-Z]{2,6})>$", RegexOptions.IgnoreCase);
            if (m.Success)
                return new MailboxAddress(m.Groups[1].Value.Trim(), m.Groups[2].Value);
            return new MailboxAddress(email, email);
        }

        // Send email
        public override bool Send() => SendAsync().GetAwaiter().GetResult();

        // Send email
        public override async Task<bool> SendAsync()
        {
            var mail = new MimeMessage();
            if (!Empty(Sender))
                mail.From.Add(ConvertToMailboxAddress(Sender));
            if (!Empty(Recipient))
                Recipient.Replace(';', ',').Split(',').Select(x => ConvertToMailboxAddress(x)).ToList().ForEach(address => mail.To.Add(address));
            if (!Empty(Cc))
                Cc.Replace(';', ',').Split(',').Select(x => ConvertToMailboxAddress(x)).ToList().ForEach(address => mail.Cc.Add(address));
            if (!Empty(Bcc))
                Bcc.Replace(';', ',').Split(',').Select(x => ConvertToMailboxAddress(x)).ToList().ForEach(address => mail.Bcc.Add(address));
            mail.Subject = Subject;
            var builder = new BodyBuilder();
            if (SameText(Format, "html")) {
                builder.HtmlBody = Content;
            } else {
                builder.TextBody = Content;
            }
            Attachments?.ForEach(attachment => {
                if (attachment.TryGetValue("filename", out string? filename)) {
                    if (attachment.TryGetValue("content", out string? content))
                        builder.Attachments.Add(filename, Encoding.UTF8.GetBytes(content));
                    else
                        builder.Attachments.Add(filename);
                }
            });
            Images?.ForEach(tmpimage => {
                string file = UploadTempPath() + tmpimage;
                if (FileExists(file)) {
                    var res = builder.LinkedResources.Add(file);
                    res.ContentId = Path.GetFileNameWithoutExtension(file); // Remove extension (filename as cid)
                }
            });
            mail.Body = builder.ToMessageBody();
            var client = Mailer != null ? (SmtpClient)Mailer : new SmtpClient();
            string host = !Empty(Config.SmtpServer) ? Config.SmtpServer : "localhost";
            int port = Config.SmtpServerPort > 0 ? Config.SmtpServerPort : 0;
            string smtpServerUsername = Config.SmtpServerUsername;
            string smtpServerPassword = Config.SmtpServerPassword;
            if (!Empty(Config.SmtpServerUsername) && !Empty(Config.SmtpServerPassword)) {
                if (Config.EncryptUserNameAndPassword) {
                    smtpServerUsername = AesDecrypt(smtpServerUsername);
                    smtpServerPassword = AesDecrypt(smtpServerPassword);
                }
            }
            try {
                SendError = "";
                await client.ConnectAsync(host, port, Options != null ? (SecureSocketOptions)Options : SecureSocketOptions.Auto);
                if (!Empty(smtpServerUsername) && !Empty(smtpServerPassword))
                    await client.AuthenticateAsync(smtpServerUsername, smtpServerPassword);
                await client.SendAsync(mail);
                await client.DisconnectAsync(true);
                return true;
            } catch (Exception e) {
                SendError = $"Email send operation failed with message: {e.Message}";
                if (Config.Debug) {
                    SetDebugMessage(SendError);
                    Log(SendError);
                }
                return false;
            }
        }
    }
} // End Partial class
