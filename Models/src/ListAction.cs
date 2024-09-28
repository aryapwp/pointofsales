namespace ASPNETMaker2024.Models;

// Partial class
public partial class PointofSales {
    /// <summary>
    /// List action class
    /// </summary>
    public class ListAction
    {
        public string Action = "";

        public string Caption = "";

        public bool Allowed = true;

        public string Method = Config.ActionPostback; // Post back (p) / Ajax (a)

        public string Select = Config.ActionMultiple; // Multiple (m) / Single (s) / Custom (c)

        public string ConfirmMessage = "";

        public string Icon = "fa-solid fa-star ew-icon"; // Icon

        public string Success = ""; // JavaScript callback function name

        public string SuccessMessage = ""; // Default success message

        public string FailureMessage = ""; // Default failure message

        public Func<Dictionary<string, object>, dynamic, Task<bool>>? Handler = null; // Handler for the action

        // Constructor
        public ListAction(string action, string caption, bool allowed = true, string method = Config.ActionPostback, string select = Config.ActionMultiple, string confirmMessage = "", string icon = "fa-solid fa-star ew-icon", string success = "", string successMessage = "", string failureMessage = "", Func<Dictionary<string, object>, dynamic, Task<bool>>? handler = null)
        {
            Action = action;
            Caption = caption;
            Allowed = allowed;
            Method = method;
            Select = select;
            ConfirmMessage = confirmMessage;
            Icon = icon;
            Success = success;
            SuccessMessage = successMessage;
            FailureMessage = failureMessage;
            Handler = handler;
        }

        // Handle the action
        public async virtual Task<JsonBoolResult> Handle(Dictionary<string, object> row, dynamic listPage)
        {
            if (Handler is Func<Dictionary<string, object>, dynamic, Task<bool>> handler) {
                bool result = await handler(row, listPage);
                return new JsonBoolResult(result, result ? SuccessMessage : FailureMessage);
            }
            return new JsonBoolResult(true);
        }

        // To JSON
        public string ToJson(bool htmlencode = false)
        {
            var d = new { msg = ConfirmMessage, action = Action, method = Method, select = Select, success = Success };
            var json = ConvertToJson(d);
            if (htmlencode)
                json = HtmlEncode(json);
            return json;
        }

        // To data-* attributes
        public string ToDataAttributes()
        {
            return (new Attributes() {
                { "data-msg", HtmlEncode(ConfirmMessage) },
                { "data-action", HtmlEncode(Action) },
                { "data-method", HtmlEncode(Method) },
                { "data-select", HtmlEncode(Select) },
                { "data-success", HtmlEncode(Success) }
            }).ToString();
        }
    }
} // End Partial class
