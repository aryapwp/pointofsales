namespace ASPNETMaker2024.Models;

// Partial class
public partial class PointofSales {
    /// <summary>
    /// Email class
    /// </summary>
    public class EmailBase
    {
        public static object? Mailer { get; set; } = null;

        protected object? Options { get; set; } = null; // Options for mailer

        public string LanguageFolder { get; set; } = ""; // Language folder

        public string Sender { get; set; } = ""; // Sender

        public string Recipient { get; set; } = ""; // Recipient

        public string Cc { get; set; } = ""; // Cc

        public string Bcc { get; set; } = ""; // Bcc

        public string Subject { get; set; } = ""; // Subject

        public string Format { get; set; } = "HTML"; // Format

        public string Content { get; set; } = ""; // Content

        public string Charset { get; set; } = Config.EmailCharset; // Charset

        public List<Dictionary<string, string>>? Attachments = []; // Attachments

        public List<string>? Images = []; // Embedded image

        public string SendError = ""; // Send error description

        /// <summary>
        /// Constructor
        /// </summary>
        public EmailBase()
        {
            LanguageFolder = Config.LanguageFolder; // Language folder under Views
        }

        public string To
        {
            get => Recipient;
            set => Recipient = value;
        } // Alias of Recipient

        public string From
        {
            get => Sender;
            set => Sender = value;
        } // Alias of Sender

        // Load email from template
        public async Task Load(string name, string langId = "", object? data = null)
        {
            langId = Empty(langId) ? CurrentLanguage : langId;
            if (data != null) // Add data to ViewData // DN
                ConvertToDictionary<object>(data).ToList().ForEach(kvp => Controller.ViewData[kvp.Key] = kvp.Value);
            string wrk = "";
            wrk = await GetViewOutput(name + "." + langId, LanguageFolder); // Get language specific view output // DN
            if (Empty(wrk)) // Fallback to en-US
                wrk = await GetViewOutput(name + ".en-US", LanguageFolder); // Get default view output // DN
            wrk = wrk.Replace("\r\n", "\n").Replace("\r", "\n").Replace("\n", "\r\n"); // Convert line breaks
            if (!Empty(wrk)) {
                string[] ar = wrk.Split(["\r\n\r\n"], 2, StringSplitOptions.RemoveEmptyEntries); // Locate header and mail content
                if (ar.Length > 1) {
                    Content = ar[1];
                    string[] headers = ar[0].Split(["\r\n"], StringSplitOptions.RemoveEmptyEntries);
                    foreach (string header in headers) {
                        string[] ar2 = header.Split(':');
                        if (ar2.Length > 1)
                            SetPropertyValue(this, TitleCaseInvariant(ar2[0].Trim()), ar2[1].Trim());
                    }
                }
            }
        }

        // Get email address with display name
        private string GetEmailAddress(string email, string displayName = "") =>
            Empty(displayName) ? email : displayName + " <" + email + ">";

        // Replace sender
        public void ReplaceSender(string sender, string senderName = "") =>
            Sender = GetEmailAddress(sender, senderName);

        // Replace recipient
        public void ReplaceRecipient(string recipient, string recipientName = "") =>
            Recipient = GetEmailAddress(recipient, recipientName);

        // Method to add recipient
        public void AddRecipient(string recipient, string recipientName = "") =>
            Recipient = Concatenate(Recipient, GetEmailAddress(recipient, recipientName), ",");

        // Add cc email
        public void AddCc(string cc, string ccName = "") =>
            Cc = Concatenate(Cc, GetEmailAddress(cc, ccName), ",");

        // Add bcc email
        public void AddBcc(string bcc, string bccName = "") =>
            Bcc = Concatenate(Bcc, GetEmailAddress(bcc, bccName), ",");

        // Replace subject
        public void ReplaceSubject(string subject) =>
            Subject = subject;

        // Replace content
        public void ReplaceContent(string find, string replaceWith) =>
            Content = Content.Replace(find, replaceWith);

        // Method to add embedded image
        public void AddEmbeddedImage(string image)
        {
            if (!Empty(image))
                Images?.Add(image);
        }

        // Method to add attachment
        public void AddAttachment(string fileName, string content = "")
        {
            if (!Empty(fileName))
                Attachments?.Add(new() { { "filename", fileName }, { "content", content } });
        }

        // Send email
        public virtual bool Send() => false; // Not implemented

        // Send email
        public virtual Task<bool> SendAsync() => Task.FromResult(false); // Not implemented
    }
} // End Partial class
