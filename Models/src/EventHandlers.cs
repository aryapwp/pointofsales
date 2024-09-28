namespace ASPNETMaker2024.Models;

// Partial class
public partial class PointofSales {
    /// <summary>
    /// EventArgs class of T
    /// </summary>
    /// <typeparam name="T">Type of value</typeparam>
    public class EventArgs<T> : EventArgs
    {
        public T Value { get; set; }

        public EventArgs(T value)
        {
            Value = value;
        }
    }

    // Event handlers
    public static event EventHandler<EventArgs<DbConnectionStringBuilder>>? DatabaseConnectingEventHandler; // Sender type: DatabaseConnection

    public static event EventHandler<EventArgs<DbConnection>>? DatabaseConnectedEventHandler; // Sender type: DatabaseConnection

    public static event EventHandler<EventArgs<MenuItem>>? MenuItemAddingEventHandler; // Sender type: Menu

    public static event EventHandler? LoginStatusEventHandler; // Sender type: Dictionary<string, object>

    public static event EventHandler? LanguageLoadEventHandler; // Sender type: Lang

    public static event EventHandler? MenuRenderingEventHandler; // Sender type: Menu

    public static event EventHandler? MenuRenderedEventHandler; // Sender type: Menu

    public static event EventHandler? PageLoadingEventHandler;

    public static event EventHandler? PageRenderingEventHandler;

    public static event EventHandler? PageUnloadedEventHandler;

    public static event EventHandler? ContainerBuildEventHandler; // Sender type: ContainerBuilder

    public static event EventHandler? ConfigInitEventHandler;

    public static event EventHandler? RouteActionEventHandler; // Sender type: IEndpointRouteBuilder

    public static event EventHandler? AppBuildEventHandler; // Sender type: WebApplicationBuilder

    public static event EventHandler? ServiceAddEventHandler; // Sender type: IServiceCollection

    public static void InvokeRouteActionEvent(object? sender) => RouteActionEventHandler?.Invoke(sender, EventArgs.Empty);

    public static void InvokeAppBuildEvent(object? sender) => AppBuildEventHandler?.Invoke(sender, EventArgs.Empty);

    public static void InvokeServiceAddEvent(object? sender) => ServiceAddEventHandler?.Invoke(sender, EventArgs.Empty);
} // End Partial class
