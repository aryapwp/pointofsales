namespace ASPNETMaker2024.Models;

// Partial class
public partial class PointofSales {

    public class CitextString
    {
        protected string? _str;

        public CitextString(string? value)
        {
            _str = value;
        }

        public static implicit operator string?(CitextString citext) => citext._str;

        public static implicit operator CitextString(string? value) => new CitextString(value);
    }
} // End Partial class
