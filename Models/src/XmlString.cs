namespace ASPNETMaker2024.Models;

// Partial class
public partial class PointofSales {

    public class XmlString
    {
        protected string? _str;

        public XmlString(string? value)
        {
            _str = value;
        }

        public static implicit operator string?(XmlString xmlString) => xmlString._str;

        public static implicit operator XmlString(string? value) => new XmlString(value);
    }
} // End Partial class
