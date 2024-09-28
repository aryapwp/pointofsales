namespace ASPNETMaker2024.Models;

// Partial class
public partial class PointofSales {
    /// <summary>
    /// Reset concurrent user action class
    /// </summary>
    public class ResetConcurrentUserAction : ListAction
    {
        // Constructor
        public ResetConcurrentUserAction(
            string action = "resetconcurrentuser",
            string caption = "",
            bool allowed = true,
            string method = Config.ActionAjax,
            string select = Config.ActionSingle,
            string message = "",
            string icon = "fa-solid fa-star ew-icon",
            string success = ""
        ) : base(action, caption, allowed, method, select, message, icon, success)
        {
            Caption = Language.Phrase("ResetConcurrentUserBtn");
            SuccessMessage = Language.Phrase("ResetConcurrentUserSuccess");
            FailureMessage = Language.Phrase("ResetConcurrentUserFailure");
            Allowed = IsAdmin();
        }

        // Handle the action
        public override async Task<JsonBoolResult> Handle(Dictionary<string, object> row, dynamic listPage)
        {
            bool result = false;
            if (listPage.TableName == Config.UserTableName) {
                string user = row.ContainsKey(Config.LoginUsernameFieldName) ? ConvertToString(row[Config.LoginUsernameFieldName]) : "";
                if (!Empty(user))
                    result = await (new UserProfile(user)).ResetConcurrentUser();
            }
            return new JsonBoolResult(result);
        }
    }
} // End Partial class
