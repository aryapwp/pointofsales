namespace ASPNETMaker2024.Models;

// Partial class
public partial class PointofSales {
    /// <summary>
    /// JsonResult with a boolean Result property
    /// </summary>
    public class JsonBoolResult : JsonResult
    {
        public bool Result;

        public static JsonBoolResult FalseResult = new(new { success = false, version = Config.ProductVersion }, false);

        // Constructor
        public JsonBoolResult(object value, bool result) : base(value)
        {
            Result = result;
        }

        public JsonBoolResult(bool result) : base(null)
        {
            Result = result;
        }

        public JsonBoolResult(bool result, string message) : base(null)
        {
            Result = result;
            Value = result
                ? new { success = true, message = message, version = Config.ProductVersion }
                : new { success = false, error = message, version = Config.ProductVersion };
        }

        // Implicit operator
        public static implicit operator bool(JsonBoolResult res) => res.Result;
    }
} // End Partial class
