namespace ASPNETMaker2024.Models;

// Partial class
public partial class PointofSales {
    /// <summary>
    /// Sms class
    /// </summary>
    public class Sms
    {
        public string LanguageFolder; // Language folder

        public string Recipient { get; set; } = ""; // Recipient

        public string Content { get; set; } = ""; // Content

        public string SendError { get; set; } = ""; // Send error description

        /// <summary>
        /// Constructor
        /// </summary>
        public Sms()
        {
            LanguageFolder = Config.LanguageFolder; // Language folder under Views
        }

        // Load message from template
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
            Content = wrk;
        }

        // Replace content
        public void ReplaceContent(string find, string replaceWith) =>
            Content = Content.Replace(find, replaceWith);

        // Send SMS
        public bool Send() => SendAsync().GetAwaiter().GetResult();

        // Send SMS Async
        public virtual Task<bool> SendAsync() => Task.FromResult(false); // Not implemented
    }
} // End Partial class
