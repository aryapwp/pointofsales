namespace ASPNETMaker2024.Models;

// Partial class
public partial class PointofSales {
    /// <summary>
    /// Force logout user action class
    /// </summary>
    public class ForceLogoutUserAction : ListAction
    {
        // Constructor
        public ForceLogoutUserAction(
            string action = "forcelogoutuser",
            string caption = "",
            bool allowed = true,
            string method = Config.ActionAjax,
            string select = Config.ActionSingle,
            string message = "",
            string icon = "fa-solid fa-star ew-icon",
            string success = ""
        ) : base(action, caption, allowed, method, select, message, icon, success)
        {
            Caption = Language.Phrase("ForceLogoutUserBtn");
            SuccessMessage = Language.Phrase("ForceLogoutUserSuccess");
            FailureMessage = Language.Phrase("ForceLogoutUserFailure");
            Allowed = IsAdmin();
        }

        // Handle the action
        public override async Task<JsonBoolResult> Handle(Dictionary<string, object> row, dynamic listPage)
        {
            bool result = false;
            if (listPage.TableName == Config.UserTableName && UserProfile.IsForceLogoutUser) {
                string user = row.ContainsKey(Config.LoginUsernameFieldName) ? ConvertToString(row[Config.LoginUsernameFieldName]) : "";
                if (!Empty(user))
                    result = await (new UserProfile(user)).ForceLogoutUser();
            }
            return new JsonBoolResult(result);
        }
    }
} // End Partial class
