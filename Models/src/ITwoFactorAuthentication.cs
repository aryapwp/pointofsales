namespace ASPNETMaker2024.Models;

// Partial class
public partial class PointofSales {
    /// <summary>
    /// Two Factor Authentication interface
    /// </summary>
    public interface ITwoFactorAuthentication
    {
        bool CheckCode(string secret, string code);
        string GenerateSecret();
        string GetQrCodeUrl(string user, string secret, string? issuer = null, int size = 0);
        Task<IActionResult> Show();
        List<string> GenerateBackupCodes();
        List<string> GetBackupCodes();
        Task<List<string>> GetNewBackupCodes();
        Task<bool> Verify(string code);
        Task<bool> Reset(string? user);
        Task<JsonBoolResult> SendOneTimePassword(string user, string? account = null);
        string GetAccount(string user);
    }
} // End Partial class
