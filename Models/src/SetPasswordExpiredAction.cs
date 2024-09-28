namespace ASPNETMaker2024.Models;

// Partial class
public partial class PointofSales {
    /// <summary>
    /// Set password expired action class
    /// </summary>
    public class SetPasswordExpiredAction : ListAction
    {
        // Constructor
        public SetPasswordExpiredAction(
            string action = "setpasswordexpired",
            string caption = "",
            bool allowed = true,
            string method = Config.ActionAjax,
            string select = Config.ActionSingle,
            string message = "",
            string icon = "fa-solid fa-star ew-icon",
            string success = ""
        ) : base(action, caption, allowed, method, select, message, icon, success)
        {
            Caption = Language.Phrase("SetPasswordExpiredBtn");
            SuccessMessage = Language.Phrase("SetPasswordExpiredSuccess");
            FailureMessage = Language.Phrase("SetPasswordExpiredFailure");
            Allowed = IsAdmin();
        }

        // Handle the action
        public override async Task<JsonBoolResult> Handle(Dictionary<string, object> row, dynamic listPage)
        {
            bool result = false;
            if (listPage.TableName == Config.UserTableName) {
                string user = row.ContainsKey(Config.LoginUsernameFieldName) ? ConvertToString(row[Config.LoginUsernameFieldName]) : "";
                if (!Empty(user))
                    result = await (new UserProfile(user)).SetPasswordExpired();
            }
            return new JsonBoolResult(result);
        }
    }
} // End Partial class
