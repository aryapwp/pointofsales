namespace ASPNETMaker2024.Models;

// Partial class
public partial class PointofSales {
    /// <summary>
    /// Crosstab Column class
    /// </summary>
    public class CrosstabColumn
    {
        public string Caption = "";

        public object Value;

        public bool Visible = true;

        public CrosstabColumn(string caption, object val, bool visible = true)
        {
            Caption = caption;
            Value = val;
            Visible = visible;
        }
    }
} // End Partial class
