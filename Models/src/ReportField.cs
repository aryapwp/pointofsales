namespace ASPNETMaker2024.Models;

// Partial class
public partial class PointofSales {
    /// <summary>
    /// Report Field class
    /// </summary>
    public class ReportField : DbField
    {
        public object? SumValue; // Sum

        public object? AvgValue; // Average

        public object? MinValue; // Minimum

        public object? MaxValue; // Maximum

        public object? CntValue; // Count

        public object? SumViewValue; // Sum

        public object? AvgViewValue; // Average

        public object? MinViewValue; // Minimum

        public object? MaxViewValue; // Maximum

        public object? CntViewValue; // Count

        public string DrillDownTable = ""; // Drill down table name

        public string DrillDownUrl = ""; // Drill down URL

        public string CurrentFilter = ""; // Current filter in use

        public int GroupingFieldId = 0; // Grouping field id

        public bool ShowGroupHeaderAsRow = false; // Show grouping level as row

        public bool ShowCompactSummaryFooter = false; // Show compact summary footer

        public string GroupByType = ""; // Group By Type

        public string GroupInterval = ""; // Group Interval

        public string GroupSql = ""; // Group SQL

        public object? GroupViewValue; // Group View Value

        public string DateFilter = ""; // Date Filter

        public string Delimiter = ""; // Delimiter (e.g. comma) for delimiter separated value

        public List<object> DistinctValues = [];

        public List<Dictionary<string, object>> Records = [];

        public bool LevelBreak = false;

        public bool Expanded = true;

        public Dictionary<string, object> DashboardSearchSourceFields = new();

        public string SearchType = "";

        private object? _groupValue;

        public ReportField(object table, string fieldVar, int type) : base(table, fieldVar, type)
        {
        }

        // Group value
        public object? GroupValue
        {
            get => _groupValue;
            set {
                SetDbValue(value);
                _groupValue = DbValue;
            }
        }

        // Get distinct values
        public void GetDistinctValues(List<Dictionary<string, object>> records, string sort = "ASC") =>
            DistinctValues = SameText(sort, "DESC")
                ? records.Select(row => row[GroupName]).Distinct().OrderByDescending(value => value).ToList()
                : records.Select(row => row[GroupName]).Distinct().OrderBy(value => value).ToList();

        // Get distinct records
        public void GetDistinctRecords(List<Dictionary<string, object>> records, object val) =>
            Records = records.Where(row => SameString(row[GroupName], val)).ToList();

        // Get sum
        public void GetSum(List<Dictionary<string, object>> records, bool skipNull = false)
        {
            if (skipNull)
                records = records.Where(row => row[GroupName] != null).ToList();
            SumValue = records.Any() ? records.Sum(row => ConvertToDecimal(row[GroupName])) : 0;
        }

        // Get average
        public void GetAvg(List<Dictionary<string, object>> records, bool skipNull = false)
        {
            if (skipNull)
                records = records.Where(row => row[GroupName] != null).ToList();
            AvgValue = records.Any() ? records.Average(row => ConvertToDecimal(row[GroupName])) : 0;
        }

        // Get min
        public void GetMin(List<Dictionary<string, object>> records, bool skipNull = false)
        {
            if (skipNull)
                records = records.Where(row => row[GroupName] != null).ToList();
            MinValue = records.Any() ? records.Min(row => row[GroupName]) : null;
        }

        // Get max
        public void GetMax(List<Dictionary<string, object>> records, bool skipNull = false)
        {
            if (skipNull)
                records = records.Where(row => row[GroupName] != null).ToList();
            MaxValue = records.Any() ? records.Max(row => row[GroupName]) : null;
        }

        // Get count
        public void GetCnt(List<Dictionary<string, object>> records, bool skipNull = false)
        {
            if (skipNull)
                records = records.Where(row => row[GroupName] != null).ToList();
            int cnt = records.Any() ? records.Count() : 0;
            CntValue = cnt;
            Count = cnt;
        }

        // Get group name // DN
        public string GroupName => !Empty(GroupSql) ? "EW_GROUP_VALUE_" + GroupingFieldId : Name;

        /// <summary>
        /// FormatAdvancedFilters
        /// </summary>
        /// <param name="ar">Advanced filter</param>
        /// <returns>Formatted advanced filter</returns>
        public List<Dictionary<string, object>> FormatAdvancedFilters(dynamic ar) { // DN
            if (IsList(ar) && IsList(AdvancedFilters)) {
                foreach (var arwrk in ar) {
                    if (arwrk.ContainsKey("lf") && arwrk.ContainsKey("df")) {
                        string lf = arwrk["lf"];
                        string df = arwrk["df"];
                        if (lf.StartsWith("@@") && SameString(lf, df) && IsList(AdvancedFilters)) {
                            string key = lf.Substring(2);
                            foreach (var filter in AdvancedFilters) {
                                if (filter.Enabled && SameString(key, filter.ID))
                                    arwrk["df"] = filter.Name;
                            }
                        }
                    }
                }
            }
            return ar;
        }

        // Search expression
        public new string SearchExpression
        {
            get {
                if (!Empty(DateFilter)) { // Date filter
                    return DateFilter.ToLowerInvariant() switch {
                        "year" => GroupSql(Expression, "y", 0, Table!.DbId),
                        "quarter" => GroupSql(Expression, "q", 0, Table!.DbId),
                        "month" => GroupSql(Expression, "m", 0, Table!.DbId),
                        "week" => GroupSql(Expression, "w", 0, Table!.DbId),
                        "day" => GroupSql(Expression, "d", 0, Table!.DbId),
                        "hour" => GroupSql(Expression, "h", 0, Table!.DbId),
                        "minute" => GroupSql(Expression, "min", 0, Table!.DbId),
                        _ => Expression
                    };
                } else if (!Empty(GroupSql)) { // Use grouping SQL for search if exists
                    return GroupSql.Replace("%s", Expression);
                }
                return base.SearchExpression;
            }
        }

        // Search field type
        public new DataType SearchDataType
        {
            get {
                if (!Empty(DateFilter)) { // Date filter
                    return DateFilter.ToLowerInvariant() switch {
                        "year" => DataType.Number,
                        "quarter" => DataType.String,
                        "month" => DataType.String,
                        "week" => DataType.String,
                        "day" => DataType.String,
                        "hour" => DataType.Number,
                        "minute" => DataType.Number,
                        _ => DataType
                    };
                } else if (!Empty(GroupSql)) { // Use grouping SQL for search if exists
                    return DataType.String;
                }
                return base.SearchDataType;
            }
        }

        // Group toggle icon
        public string GroupToggleIcon
        {
            get {
                string iconClass = "ew-group-toggle fa-solid fa-caret-down" + (Expanded || Table is ReportTable rtbl && rtbl.HideGroupLevel != GroupingFieldId ? "" : " ew-rpt-grp-hide");
                return "<i class=\"" + iconClass + "\"></i>";
            }
        }

        // Expand group
        public void SetExpanded(bool value)
        {
            foreach (ReportField fld in Table!.Fields.Values)
                if (fld.GroupingFieldId >= GroupingFieldId)
                    fld.Expanded = value;
        }

        // Get Cell Attributes with class name // DN
        public string GetCellAttributes(string className = "")
        {
            if (!Empty(className))
                CellAttrs.AppendClass(className);
            string cellAttributes = CellAttributes;
            if (!Empty(className))
                CellAttrs.RemoveClass(className);
            return cellAttributes;
        }
    }
} // End Partial class
