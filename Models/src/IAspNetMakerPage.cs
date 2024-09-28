namespace ASPNETMaker2024.Models;

// Partial class
public partial class PointofSales {
    // IAspNetMakerPage interface // DN
    public interface IAspNetMakerPage
    {
        Task<IActionResult> Run();
        IActionResult Terminate(string url = "");
    }
} // End Partial class
