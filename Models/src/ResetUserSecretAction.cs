namespace ASPNETMaker2024.Models;

// Partial class
public partial class PointofSales {
    /// <summary>
    /// Reset user secret action class
    /// </summary>
    public class ResetUserSecretAction : ListAction
    {
        // Constructor
        public ResetUserSecretAction(
            string action = "resetusersecret",
            string caption = "",
            bool allowed = true,
            string method = Config.ActionAjax,
            string select = Config.ActionSingle,
            string message = "",
            string icon = "fa-solid fa-star ew-icon",
            string success = ""
        ) : base(action, caption, allowed, method, select, message, icon, success)
        {
            Caption = Language.Phrase("ResetUserSecretBtn");
            SuccessMessage = Language.Phrase("ResetUserSecretSuccess");
            FailureMessage = Language.Phrase("ResetUserSecretFailure");
            Allowed = IsAdmin();
        }

        // Handle the action
        public override async Task<JsonBoolResult> Handle(Dictionary<string, object> row, dynamic listPage)
        {
            bool result = false;
            if (listPage.TableName == Config.UserTableName) {
                string user = row.ContainsKey(Config.LoginUsernameFieldName) ? ConvertToString(row[Config.LoginUsernameFieldName]) : "";
                if (!Empty(user))
                    result = await (new UserProfile(user)).ResetUserSecret();
                return result
                    ? new JsonBoolResult(new { success = true, successMessage = SuccessMessage.Replace("%u", user), disabled = true, version = Config.ProductVersion }, true)
                    : new JsonBoolResult(false, FailureMessage.Replace("%u", user));
            }
            return new JsonBoolResult(result);
        }
    }
} // End Partial class
