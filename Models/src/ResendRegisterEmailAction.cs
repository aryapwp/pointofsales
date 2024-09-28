namespace ASPNETMaker2024.Models;

// Partial class
public partial class PointofSales {
    /// <summary>
    /// Resend register email action class
    /// </summary>
    public class ResendRegisterEmailAction : ListAction
    {
        // Constructor
        public ResendRegisterEmailAction(
            string action = "resendregisteremail",
            string caption = "",
            bool allowed = true,
            string method = Config.ActionAjax,
            string select = Config.ActionSingle,
            string message = "",
            string icon = "fa-solid fa-star ew-icon",
            string success = ""
        ) : base(action, caption, allowed, method, select, message, icon, success)
        {
            Caption = Language.Phrase("ResendRegisterEmailBtn");
            SuccessMessage = Language.Phrase("ResendRegisterEmailSuccess");
            FailureMessage = Language.Phrase("ResendRegisterEmailFailure");
            Allowed = IsAdmin();
        }

        // Handle the action
        public async override Task<JsonBoolResult> Handle(Dictionary<string, object> row, dynamic listPage)
        {
            bool result = (bool)(await listPage?.InvokeAsync("SendRegisterEmail", new object?[] { row }));
            return new JsonBoolResult(result);
        }
    }
} // End Partial class
