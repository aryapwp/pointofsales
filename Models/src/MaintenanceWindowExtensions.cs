namespace ASPNETMaker2024.Models;

public static class MaintenanceWindowExtensions
{
    public static IApplicationBuilder UseMaintenance(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<MaintenanceMiddleware>();
    }
}
