namespace ASPNETMaker2024.Models;

// Partial class
public partial class PointofSales {
    /// <summary>
    /// Abstract Two Factor Authentication class
    /// </summary>
    public abstract class AbstractTwoFactorAuthentication : ITwoFactorAuthentication
    {
        // Check code
        public abstract bool CheckCode(string secret, string code);

        // Generate secret
        public abstract string GenerateSecret();

        public virtual string GetQrCodeUrl(string user, string secret, string? issuer = null, int size = 0) => ""; // To be implemented by subclasses

        // Show (API action)
        public abstract Task<IActionResult> Show();

        // Generate backup codes
        public List<string> GenerateBackupCodes()
        {
            int length = Config.TwoFactorAuthenticationBackupCodeLength;
            int count = Config.TwoFactorAuthenticationBackupCodeCount;
            List<string> list = [];
            Random random = new();
            for (int i = 0; i < count; i++) {
                string code = "";
                for (int j = 0; j < length; j++)
                    code += random.Next(0, 9).ToString();
                list.Add(code);
            }
            return list;
        }

        // Get backup codes
        public List<string> GetBackupCodes()
        {
            string user = CurrentUserName(); // Must be current user
            var profile = new UserProfile(user); // DN
            return profile.GetBackupCodes();
        }

        // Get new backup codes
        public async Task<List<string>> GetNewBackupCodes()
        {
            string user = CurrentUserName(); // Must be current user
            var profile = new UserProfile(user); // DN
            return await profile.GetNewBackupCodes();
        }

        // Verify
        public async Task<bool> Verify(string code)
        {
            string user = CurrentUserName(); // Must be current user
            var profile = new UserProfile(user); // DN
            if (Empty(code)) // Verify if user has secret only
                return profile.HasUserSecret(true);
            else // Verified, just check code
                if (profile.HasUserSecret())
                    return await profile.Verify2FACode(code);
            return false;
        }

        // Reset
        public async Task<bool> Reset(string? user)
        {
            user = IsSysAdmin() && !Empty(user) ? user : Config.ForceTwoFactorAuthentication ? "" : CurrentUserName();
            if (!Empty(user)) {
                var profile = new UserProfile(user); // DN
                if (profile.HasUserSecret())
                    return await profile.ResetUserSecret();
            }
            return false;
        }

        public virtual Task<JsonBoolResult> SendOneTimePassword(string user, string? account = null) => Task.FromResult(new JsonBoolResult(false)); // To be implemented by subclasses

        public virtual string GetAccount(string user) => ""; // To be implemented by subclasses
    }
} // End Partial class
