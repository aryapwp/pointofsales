namespace ASPNETMaker2024.Models;

// Partial class
public partial class PointofSales {
    /// <summary>
    /// Class OptionValues
    /// </summary>
    public class OptionValues : IHtmlValue
    {
        public List<string> Values = [];

        // Constructor
        public OptionValues(IEnumerable<string>? list = null)
        {
            if (list != null)
                Values = new List<string>(list);
        }

        // Constructor
        public OptionValues(IEnumerable<object> list)
        {
            Values = list.Select(v => ConvertToString(v)).ToList();
        }

        // Add value
        public void Add(string value) => Values.Add(value);

        // Get HTML markup of an option
        public string OptionHtml(string val) => Regex.Replace(Config.OptionHtmlTemplate, "{value}", val);

        // Get HTML markup for options
        public string OptionsHtml(List<string> values) => values.Aggregate("", (html, next) => html + OptionHtml(next));

        // Convert to HTML
        public string ToHtml() => OptionsHtml(Values) ?? ToString();

        // Convert to string (MUST return a string value)
        public override string ToString() => String.Join(Config.OptionSeparator, Values);
    }
} // End Partial class
