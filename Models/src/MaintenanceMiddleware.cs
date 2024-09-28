namespace ASPNETMaker2024.Models;

// Partial class
public partial class PointofSales {

    public class MaintenanceMiddleware
    {
        private readonly RequestDelegate next;

        public static int StatusCode = 503;

        public static string Content = @"<html>
    <head>
        <meta name=""viewport"" content=""width=device-width, initial-scale=1"">
        <title>{0}</title>
        <link rel=""stylesheet"" href=""{1}/adminlte3/css/{2}"">
        <link rel=""stylesheet"" href=""{1}/plugins/fontawesome-free/css/all.css"">
        <link rel=""stylesheet"" href=""{1}/{3}"">
    </head>
    <body class=""container-fluid"">
        <div class=""d-flex justify-content-center align-items-center h-100"">
            <div class=""error-page"">
                <h2 class=""headline text-danger"">{4}</h2>
                <div class=""error-content"">
                    <h3><i class=""fa-solid fa-triangle-exclamation text-danger""></i> {0}</h3>
                    <p>{5}</p>
                </div><!-- /.error-content -->
            </div><!-- /.error-page -->
        </div>
    </body>
</html>";

        public MaintenanceMiddleware(RequestDelegate next)
        {
            this.next = next;
        }

        public async Task Invoke(HttpContext context)
        {
            bool enabled = ConvertToBool(Configuration["MaintenanceMode:Enabled"]);
            if (enabled) {
                Language = ResolveLanguage();
                context.Response.StatusCode = StatusCode; // Set status code
                context.Response.Headers.Append("Retry-After", Configuration["MaintenanceMode:RetryAfter"]);
                string statusCode = StatusCode.ToString();
                string content = String.Format(Content,
                    Language.Phrase(statusCode), // {0}
                    context.Request.PathBase.ToString(), // {1}
                    CssFile("adminlte.css"), // {2}
                    CssFile(Config.ProjectStylesheetFilename), // {3}
                    statusCode, // {4}
                    Language.Phrase(statusCode + "Desc") // {5}
                );
                await context.Response.WriteAsync(content, Encoding.UTF8);
            } else { // Go to next if not in Maintenance mode
                await next.Invoke(context);
            }
        }
    }
} // End Partial class
