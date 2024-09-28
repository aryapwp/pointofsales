namespace ASPNETMaker2024.Models;

// Partial class
public partial class PointofSales {
    /// <summary>
    /// Common class for table and report
    /// </summary>
    public class DbTableBase : AspNetMakerPage
    {
        private string? _tableCaption = null;

        private Dictionary<int, string> _pageCaption = new();

        public string Name = "";

        public string Type = "";

        public bool UseSelectLimit = true;

        public bool Visible = true;

        public bool EncodeSlash = true;

        public Dictionary<string, DbField> Fields { get; } = new(); // DN

        public Dictionary<string, DbChart> Charts { get; } = new(); // DN

        public List<Dictionary<string, object?>> Rows = []; // Data for Custom Template DN

        public string OldKey = ""; // Old key (for edit/copy)

        public string OldKeyName; // Old key name (for edit/copy)

        public bool UseCustomTemplate = false; // Use custom template // DN

        public string Export = ""; // Export

        public bool ExportAll;

        public int ExportPageBreakCount; // Page break per every n record (PDF only)

        public string ExportPageOrientation = ""; // Page orientation (PDF only)

        public string ExportPageSize = ""; // Page size (PDF only)

        public float[] ExportColumnWidths = []; // Column widths (PDF only) // DN

        public string ExportExcelPageOrientation = ""; // Page orientation (Excel only)

        public string ExportExcelPageSize = ""; // Page size (Excel only)

        public string ExportWordPageOrientation = ""; // Page orientation (Word only)

        public bool SendEmail; // Send email

        public string PageBreakHtml;

        public bool ExportPageBreaks = true; // Page breaks when export

        public bool ImportInsertOnly; // Import by insert only

        public bool ImportUseTransaction = Config.ImportUseTransaction; // Import use transaction

        public int ImportMaxFailures = 0; // Import maximum number of failures

        private Lazy<BasicSearch> _basicSearch; // Basic search

        public string QueryRules = ""; // Rules from jQuery Query builder

        public string CurrentFilter = ""; // Current filter

        public string CurrentOrder = ""; // Current order

        public string CurrentOrderType = ""; // Current order type

        public RowType RowType; // Row type

        public string CssClass = ""; // CSS class

        public string CssStyle = ""; // CSS style

        public Attributes RowAttrs = new(); // DN

        public string CurrentAction = ""; // Current action

        public string ActionValue = ""; // Action value

        public string LastAction = ""; // Last action

        public int UserIdAllowSecurity = 0; // User ID Allow

        public string Command = "";

        public int Count = 0; // Record count (as detail table)

        public string SearchOption; // Search option

        public string Filter = "";

        public string DefaultFilter = "";

        public string Sort = "";

        public bool AutoHidePager;

        public bool AutoHidePageSizeSelector;

        public string RowAction = ""; // Row action

        // Charts related
        public bool SourceTableIsCustomView = false;

        public string TableReportType = "";

        public bool ShowDrillDownFilter;

        public bool UseDrillDownPanel; // Use drill down panel

        public bool DrillDown = false;

        public bool DrillDownInPanel = false;

        // Table
        public string TableClass = "";

        public string TableGridClass = ""; // CSS class for .card (with a leading space)

        public string TableContainerClass = ""; // CSS class for .card-body (e.g. height of the main table)

        public string TableContainerStyle = ""; // CSS style for .card-body (e.g. height of the main table)

        public bool UseResponsiveTable = false;

        public string ResponsiveTableClass = "";

        public string ContainerClass = "p-0";

        public string ContextClass = ""; // CSS class name as context

        public bool ShowCurrentFilter;

        // Default field properties
        public string UploadPath;

        public string OldUploadPath;

        public string UploadAllowedFileExtensions;

        public int UploadMaxFileSize;

        public int? UploadMaxFileCount;

        public bool ImageCropper;

        public bool UseColorbox;

        public bool AutoFillOriginalValue;

        public bool UseLookupCache;

        public int LookupCacheCount;

        public bool ExportOriginalValue;

        public bool ExportFieldCaption;

        public bool ExportFieldImage;

        public string DefaultNumberFormat;

        public bool SearchHighlight = false;

        public virtual Dictionary<string, string> KeyFields { get; set; } = new();

        // Constructor
        public DbTableBase()
        {
            _basicSearch = new Lazy<BasicSearch>(() => new BasicSearch(this));
            OldKeyName = Config.FormOldKeyName;
            SearchOption = Config.SearchOption;
            ImportInsertOnly = Config.ImportInsertOnly;
            ImportMaxFailures = Config.ImportMaxFailures;
            AutoHidePager = Config.AutoHidePager;
            AutoHidePageSizeSelector = Config.AutoHidePageSizeSelector;
            UseResponsiveTable = !IsExport() && Config.UseResponsiveTable;
            ResponsiveTableClass = Config.ResponsiveTableClass;
            TableContainerClass = UseResponsiveTable ? ResponsiveTableClass : "";
            ShowCurrentFilter = Config.ShowCurrentFilter;

            // Default field properties
            UploadPath = Config.UploadDestPath;
            OldUploadPath = Config.UploadDestPath;
            UploadAllowedFileExtensions = Config.UploadAllowedFileExtensions;
            UploadMaxFileSize = Config.MaxFileSize;
            UploadMaxFileCount = Config.MaxFileCount;
            ImageCropper = Config.ImageCropper;
            UseColorbox = Config.UseColorbox;
            AutoFillOriginalValue = Config.AutoFillOriginalValue;
            UseLookupCache = Config.UseLookupCache;
            LookupCacheCount = Config.LookupCacheCount;
            ExportOriginalValue = Config.ExportOriginalValue;
            ExportFieldCaption = Config.ExportFieldCaption;
            ExportFieldImage = Config.ExportFieldImage;
            DefaultNumberFormat = Config.DefaultNumberFormat;

            // Page break
            PageBreakHtml = Config.PageBreakHtml;
        }

        // Basic search
        public BasicSearch BasicSearch => _basicSearch.Value;

        // Table caption
        public string Caption
        {
            get => _tableCaption ?? Language.TablePhrase(TableVar, "TblCaption");
            set => _tableCaption = value;
        }

        // Table caption (alias)
        public string TableCaption => Caption;

        // Page caption
        public string PageCaption(int page)
        {
            _pageCaption.TryGetValue(page, out string? caption);
            if (!Empty(caption)) {
                return caption;
            } else {
                caption = Language.TablePhrase(TableVar, "TblPageCaption" + page.ToString());
                if (Empty(caption))
                    caption = "Page " + page.ToString();
                return caption;
            }
        }

        // Row Styles
        public string RowStyles
        {
            get {
                string att = "";
                string style = Concatenate(CssStyle, ConvertToString(RowAttrs["style"]), ";");
                string cls = AppendClass(CssClass, ConvertToString(RowAttrs["class"]));
                if (!Empty(style))
                    att += " style=\"" + style.Trim() + "\"";
                if (!Empty(cls))
                    att += " class=\"" + cls.Trim() + "\"";
                return att;
            }
        }

        // Row Attribute
        public string RowAttributes
        {
            get {
                string att = RowStyles;
                if (!IsExport()) {
                    string attrs = RowAttrs.ToString(["class", "style"]);
                    if (!Empty(attrs))
                        att += attrs;
                }
                return att;
            }
        }

        // Convert dictionary to filter // DN
        public string ConvertToFilter(IDictionary<string, object> dict)
        {
            string filter = "";
            foreach (var (name, value) in dict) {
                var fld = FieldByName(name);
                if (fld != null)
                    AddFilter(ref filter, QuotedName(fld.Name, DbId) + " = " + QuotedValue(value, fld.DataType, DbId));
            }
            return filter;
        }

        // Reset attributes for table object
        public void ResetAttributes()
        {
            CssClass = "";
            CssStyle = "";
            RowAttrs.Clear();
            foreach (var (key, fld) in Fields)
                fld.ResetAttributes();
        }

        // Set a property of all fields
        public void SetFieldProperties(string name, object val)
        {
            foreach (var (key, fld) in Fields)
                SetPropertyValue(fld, name, val);
        }

        // Set current values (for number/date/time fields only)
        public void SetCurrentValues(Dictionary<string, object> row)
        {
            foreach (var (name, value) in row) {
                var fld = FieldByName(name);
                if (fld != null && (new[] { DataType.Number, DataType.Date, DataType.Time }).Contains(fld.DataType))
                    fld.CurrentValue = value;
            }
        }

        // Setup field titles
        public void SetupFieldTitles()
        {
            foreach (var (key, fld) in Fields.Where(kvp => !Empty(kvp.Value.Title))) {
                fld.EditAttrs["data-bs-toggle"] = "tooltip";
                fld.EditAttrs["title"] = HtmlEncode(fld.Title);
            }
        }

        // Get form values (for validation)
        public Dictionary<string, string?> GetFormValues() => Fields.ToDictionary(kvp => kvp.Value.Name, kvp => kvp.Value.FormValue); // Use field name

        // Get field values
        public Dictionary<string, object?> GetFieldValues(string name) => Fields.ToDictionary(
                kvp => kvp.Value.Name, // Use field name
                kvp => kvp.Value.GetType().GetProperty(name) is var pi // Property
                    ? pi?.GetValue(kvp.Value, null)
                    : kvp.Value.GetType().GetField(name) is var fi // Field
                    ? fi?.GetValue(kvp.Value)
                    : null
            );

        // Field cell attributes
        public Dictionary<string, string> FieldCellAttributes => Fields.ToDictionary(kvp => kvp.Value.Param, kvp => kvp.Value.CellAttributes); // Use Parm

        // Get field DB values for Custom Template
        public Dictionary<string, object?> CustomTemplateFieldValues()
        {
            return Fields.Where(kvp => Config.CustomTemplateDataTypes.Contains(kvp.Value.DataType) && kvp.Value.Visible).ToDictionary(
                kvp => kvp.Value.Param, // Use Param
                kvp => kvp.Value.DbValue is string value && value.Length > Config.DataStringMaxLength
                    ? value.Substring(0, Config.DataStringMaxLength)
                    : (kvp.Value.HtmlTag == "FILE" ? kvp.Value.Upload.DbValue : kvp.Value.DbValue)
            );
        }

        // Set page caption
        public void SetPageCaption(int page, string v) => _pageCaption[page] = v;

        // Get chart object by Id
        public DbChart? ChartByID(string id) => Charts.FirstOrDefault(kvp => kvp.Value.ID == id).Value;

        // Get chart object by name
        public DbChart? ChartByName(string name) => Charts.FirstOrDefault(kvp => kvp.Key == name).Value;

        // Get chart object by param
        public DbChart? ChartByParam(string parm) => Charts.FirstOrDefault(kvp => kvp.Value.ChartVar == parm).Value;

        // Get field object by name
        public DbField? FieldByName(string name) => Fields.FirstOrDefault(kvp => kvp.Key == name).Value;

        // Get field object by param
        public DbField? FieldByParam(string parm) => Fields.FirstOrDefault(kvp => kvp.Value.Param == parm).Value;

        // Check if fixed header table
        public bool IsFixedHeaderTable => ContainsClass(TableClass, Config.FixedHeaderTableClass);

        /// <summary>
        /// Set fixed header table
        /// </summary>
        /// <param name="enabled">Enable fixed header table</param>
        /// <param name="height">Css height</param>
        public void SetFixedHeaderTable(bool enabled, string? height = null)
        {
            if (enabled && !IsExport()) {
                TableClass = AppendClass(TableClass, Config.FixedHeaderTableClass);
                height ??= Config.FixedHeaderTableHeight;
                if (!Empty(height)) {
                    TableContainerClass = AppendClass(TableContainerClass, height);
                    TableContainerClass = AppendClass(TableContainerClass, "overflow-y-auto");
                }
            } else {
                TableClass = RemoveClass(TableClass, Config.FixedHeaderTableClass);
                TableContainerClass = AppendClass(TableContainerClass, "h-auto"); // Override height class
                TableContainerClass = AppendClass(TableContainerClass, "overflow-y-auto");
            }
        }

        // Has Invalid fields
        public bool HasInvalidFields() => Fields.Values.Any(v => v.IsInvalid);

        // Get validation errors
        public Dictionary<string, string>? GetValidationErrors()
        {
            // Check if validation required
            if (!Config.ServerValidate)
                return null;
            Dictionary<string, string> errors = new();
            foreach (var (key, field) in Fields) {
                if (field.IsInvalid)
                    errors.Add(field.Param, field.GetErrorMessage());
            }
            return errors.Count > 0 ? errors : null;
        }

        // Visible field count
        public int VisibleFieldCount => Fields.Values.Where(fld => fld.Visible).Count();

        // Is export
        public bool IsExport(string format = "") => Empty(format) ? !Empty(Export) : SameText(Export, format);

        // Get field visibility
        public virtual bool GetFieldVisibility(string name) => FieldByName(name)?.Visible ?? false;

        /// <summary>
        /// Set use lookup cache
        /// </summary>
        /// <param name="useLookupCache">Use lookup cache or not</param>
        public void SetUseLookupCache(bool useLookupCache)
        {
            foreach (var (key, fld) in Fields)
                fld.UseLookupCache = useLookupCache;
        }

        /// <summary>
        /// Set lookup cache count
        /// </summary>
        /// <param name="count">The maximum number of options to cache</param>
        public void SetLookupCacheCount(int count)
        {
            foreach (var (key, fld) in Fields)
                fld.LookupCacheCount = count;
        }

        /// <summary>
        /// Convert properties to client side variables
        /// </summary>
        /// <param name="tablePropertyNames">Table property names</param>
        /// <param name="fieldPropertyNames">Field property names</param>
        /// <returns>ClientVar as dictionary</returns>
        public Dictionary<string, dynamic?> ToClientVar(List<string>? tablePropertyNames = null, List<string>? fieldPropertyNames = null)
        {
            Dictionary<string, dynamic?> props = new();
            if (tablePropertyNames == null || tablePropertyNames.Count == 0)
                tablePropertyNames = Config.TableClientVars;
            if (fieldPropertyNames == null || fieldPropertyNames.Count == 0)
                fieldPropertyNames = Config.FieldClientVars;
            foreach (string name in tablePropertyNames) {
                if (this.GetType().GetMethod(name) is MethodInfo mi)
                    props.Add(LowerCaseFirst(name), mi.Invoke(this, null));
                else if (this.GetType().GetProperty(name) is PropertyInfo pi)
                    props.Add(LowerCaseFirst(name), pi.GetValue(this, null));
                else if (this.GetType().GetField(name) is FieldInfo fi)
                    props.Add(LowerCaseFirst(name), fi.GetValue(this));
            }
            if (fieldPropertyNames != null && fieldPropertyNames.Count > 0) {
                Dictionary<string, object> fieldsProps = new();
                foreach (var (key, fld) in Fields) {
                    Dictionary<string, object?> fieldProps = new();
                    fieldsProps.Add(fld.Param, fieldProps);
                    foreach (string name in fieldPropertyNames) {
                        if (fld.GetType().GetMethod(name) is MethodInfo mi)
                            fieldProps.Add(LowerCaseFirst(name), mi.Invoke(fld, null));
                        else if (fld.GetType().GetProperty(name) is PropertyInfo pi)
                            fieldProps.Add(LowerCaseFirst(name), pi.GetValue(fld, null));
                        else if (fld.GetType().GetField(name) is FieldInfo fi)
                            fieldProps.Add(LowerCaseFirst(name), fi.GetValue(fld));
                    };
                }
                props.Add("fields", fieldsProps);
            }
            var clientVar = GetClientVar("tables", TableVar);
            if (clientVar is Dictionary<string, dynamic?> dict)
                props.Concat(dict).GroupBy(p => p.Key).ToDictionary(g => g.Key, g => g.Last().Value);
            return props;
        }

        protected string LowerCaseFirst(string name) => Regex.Replace(name, @"^([A-Za-z])", m => m.Value.ToLower());

        // Encode key value
        public string EncodeKeyValue(object key)
        {
            if (Empty(key))
                return ConvertToString(key);
            else if (EncodeSlash)
                return RawUrlEncode(key);
            else
                return String.Join("/", ConvertToString(key).Split('/').Select(v => RawUrlEncode(v)));
        }

        // Session Rule (QueryBuilder)
        private string _rules = ""; // DN

        public string SessionRules
        {
            get => UseSession ? Session.GetString(Config.ProjectName + "_" + TableVar + "_" + Config.TableRules) : _rules;
            set {
                _rules = value;
                Session[Config.ProjectName + "_" + TableVar + "_" + Config.TableRules] = value;
            }
        }

        // Dashboard Filter
        public string GetDashboardFilter(string dashboardVar, string tableVar) => Session.GetString(Config.ProjectName + "_" + dashboardVar + "_" + tableVar + "_" + Config.DashboardFilter);

        // Set Dashboard Filter
        public void SetDashboardFilter(string dashboardVar, string tableVar, string v) => Session[Config.ProjectName + "_" + dashboardVar + "_" + tableVar + "_" + Config.DashboardFilter] = v;

        // Run
        public override Task<IActionResult> Run() => Task.FromResult<IActionResult>(new EmptyResult()); // To be implemented by subclass

        // Terminate
        public override IActionResult Terminate(string url = "") => new EmptyResult();

        // Get transaction
        public DbTransaction? GetTransaction(bool main) => main ? Connection?.Transaction : Connection?.Transaction2;

        // Get record filter as Dictionary // DN
        public virtual Dictionary<string, object>? GetRowFilter(object? data = null, bool remove = false) => null; // To be implemented by subclass

        // Get record filter as Dictionary // DN
        public virtual Dictionary<string, object>? GetRowFilter(object[]? ids = null) => null; // To be implemented by subclass

        // Invoke method // DN
        public object? Invoke(MethodInfo? mi, object?[]? parameters = null) => mi?.Invoke(this, parameters);

        // Invoke method // DN
        public object? Invoke(string name, object?[]? parameters = null) => Invoke(this.GetType().GetMethod(name), parameters);

        // Invoke async method // DN
        public async Task<object?> InvokeAsync(MethodInfo? mi, object?[]? parameters = null)
        {
            if (IsAsyncMethod(mi)) {
                dynamic? awaitable = mi?.Invoke(this, parameters);
                await awaitable;
                return awaitable?.GetAwaiter().GetResult();
            }
            return Invoke(mi, parameters);
        }

        // Invoke async method // DN
        public async Task<object?> InvokeAsync(string name, object[]? parameters = null) => await InvokeAsync(this.GetType().GetMethod(name), parameters);

        // Check if Invoke async method // DN
        public bool IsAsyncMethod([NotNullWhen(true)] MethodInfo? mi) => mi?.GetCustomAttribute(typeof(AsyncStateMachineAttribute)) is AsyncStateMachineAttribute || false;

        // Check if Invoke async method // DN
        public bool IsAsyncMethod(string name) => IsAsyncMethod(this.GetType().GetMethod(name));

        /// <summary>
        /// Gather a list of key-values representing the properties of the object and their values.
        /// </summary>
        /// <param name="data">The plain C# object</param>
        /// <param name="crud">Operation</param>
        /// <returns></returns>
        public virtual Dictionary<string, object> BuildDictionaryFromObject(object data, Crud crud = Crud.None) => new(); // To be implemented by subclass

        /// <summary>
        /// Convert entity values
        /// </summary>
        /// <param name="entity">Entity returned by Dapper</param>
        /// <returns>Entity</returns>
        [return: NotNullIfNotNull("entity")]
        public virtual TEntity? ConvertEntity<TEntity>(TEntity? entity)
            where TEntity : class
            => entity; // To be implemented by subclass

        /// <summary>
        /// Retrieves the entity of type <typeparamref name="TEntity"/> with the specified id
        /// </summary>
        /// <typeparam name="TEntity">The type of the entity</typeparam>
        /// <param name="id">The id of the entity in the database</param>
        /// <param name="transaction">Optional transaction for the command</param>
        /// <param name="timeout">Timeout period</param>
        /// <param name="main">Use main connection or not</param>
        /// <returns>The entity with the corresponding id</returns>
        public TEntity? Find<TEntity>(object id, IDbTransaction? transaction = null, int? timeout = null, bool main = false)
            where TEntity : class
            => ConvertEntity<TEntity>(GetQueryBuilder(main).Where(QuotedName(KeyFields.ElementAt(0).Key, DbId), id).FirstOrDefault<TEntity>(transaction ?? GetTransaction(main), timeout));

        /// <summary>
        /// Retrieves the entity of type <typeparamref name="TEntity"/> with the specified id
        /// </summary>
        /// <typeparam name="TEntity">The type of the entity</typeparam>
        /// <param name="id">The id of the entity in the database</param>
        /// <param name="transaction">Optional transaction for the command</param>
        /// <param name="timeout">Timeout period</param>
        /// <param name="cancellationToken">Optional cancellation token for the command</param>
        /// <param name="main">Use main connection or not</param>
        /// <returns>The entity with the corresponding id</returns>
        public async Task<TEntity?> FindAsync<TEntity>(object id, IDbTransaction? transaction = null, int? timeout = null, CancellationToken cancellationToken = default, bool main = false)
            where TEntity : class
            => ConvertEntity<TEntity>(await GetQueryBuilder(main).Where(QuotedName(KeyFields.ElementAt(0).Key, DbId), id).FirstOrDefaultAsync<TEntity>(transaction, timeout, cancellationToken));

        /// <summary>
        /// Retrieves the entity of type <typeparamref name="TEntity"/> with the specified id
        /// </summary>
        /// <typeparam name="TEntity">The type of the entity</typeparam>
        /// <param name="ids">The ids of the entity in the database</param>
        /// <returns>The entity with the corresponding ids</returns>
        public TEntity? Find<TEntity>(params object[] ids)
            where TEntity : class
            => Find<TEntity>(ids, null, null, false);

        /// <summary>
        /// Retrieves the entity of type <typeparamref name="TEntity"/> with the specified id
        /// </summary>
        /// <typeparam name="TEntity">The type of the entity</typeparam>
        /// <param name="ids">The ids of the entity in the database</param>
        /// <param name="transaction">Optional transaction for the command</param>
        /// <param name="timeout">Timeout period</param>
        /// <param name="main">Use main connection or not</param>
        /// <returns>The entity with the corresponding ids</returns>
        public TEntity? Find<TEntity>(object[] ids, IDbTransaction? transaction = null, int? timeout = null, bool main = false)
            where TEntity : class
        {
            if (ids.Length == 1)
                return Find<TEntity>(ids[0], transaction, timeout);
            var filter = GetRowFilter(ids);
            var entity = filter != null ? GetQueryBuilder(main).Where(filter).FirstOrDefault<TEntity>(transaction ?? GetTransaction(main), timeout) : null;
            return ConvertEntity<TEntity>(entity);
        }

        /// <summary>
        /// Retrieves the entity of type <typeparamref name="TEntity"/> with the specified id
        /// </summary>
        /// <typeparam name="TEntity">The type of the entity</typeparam>
        /// <param name="ids">The ids of the entity in the database</param>
        /// <returns>The entity with the corresponding ids</returns>
        public Task<TEntity?> FindAsync<TEntity>(params object[] ids)
            where TEntity : class
            => FindAsync<TEntity>(ids, null,  null, default, false);

        /// <summary>
        /// Retrieves the entity of type <typeparamref name="TEntity"/> with the specified id
        /// </summary>
        /// <typeparam name="TEntity">The type of the entity</typeparam>
        /// <param name="ids">The id of the entity in the database</param>
        /// <param name="transaction">Optional transaction for the command</param>
        /// <param name="timeout">Timeout period</param>
        /// <param name="cancellationToken">Optional cancellation token for the command</param>
        /// <param name="main">Use main connection or not</param>
        /// <returns>The entity with the corresponding ids</returns>
        public async Task<TEntity?> FindAsync<TEntity>(object[] ids, IDbTransaction? transaction = null, int? timeout = null, CancellationToken cancellationToken = default, bool main = false)
            where TEntity : class
        {
            if (ids.Length == 1)
                return await FindAsync<TEntity>(ids[0], transaction, timeout, cancellationToken);
            var filter = GetRowFilter(ids);
            var entity = filter != null ? await GetQueryBuilder(main).Where(filter).FirstOrDefaultAsync<TEntity>(transaction ?? GetTransaction(main), timeout, cancellationToken) : null;
            return ConvertEntity<TEntity>(entity);
        }

        /// <summary>
        /// Retrieves all the entities of type <typeparamref name="TEntity"/>
        /// </summary>
        /// <typeparam name="TEntity">The type of the entity</typeparam>
        /// <param name="transaction">Optional transaction for the command</param>
        /// <param name="timeout">Timeout period</param>
        /// <param name="main">Use main connection or not</param>
        /// <returns>A collection of entities of type <typeparamref name="TEntity"/></returns>
        public IEnumerable<TEntity> GetAll<TEntity>(IDbTransaction? transaction = null, int? timeout = null, bool main = false)
            where TEntity : class
            => GetQueryBuilder(main).Get<TEntity>(transaction ?? GetTransaction(main), timeout).Select(entity => ConvertEntity<TEntity>(entity));

        /// <summary>
        /// Retrieves all the entities of type <typeparamref name="TEntity"/>
        /// </summary>
        /// <typeparam name="TEntity">The type of the entity</typeparam>
        /// <param name="transaction">Optional transaction for the command</param>
        /// <param name="timeout">Timeout period</param>///
        /// <param name="cancellationToken">Optional cancellation token for the command</param>
        /// <param name="main">Use main connection or not</param>
        /// <returns>A collection of entities of type <typeparamref name="TEntity"/></returns>
        public async Task<IEnumerable<TEntity>> GetAllAsync<TEntity>(IDbTransaction? transaction = null, int? timeout = null, CancellationToken cancellationToken = default, bool main = false)
            where TEntity : class
            => (await GetQueryBuilder(main).GetAsync<TEntity>(transaction ?? GetTransaction(main), timeout, cancellationToken)).Select(entity => ConvertEntity<TEntity>(entity));

        /// <summary>
        /// Retrieves a paged set of entities of type <typeparamref name="TEntity"/>
        /// </summary>
        /// <typeparam name="TEntity">The type of the entity</typeparam>
        /// <param name="pageNumber">The number of the page to fetch, starting at 1</param>
        /// <param name="pageSize">The page size</param>
        /// <param name="transaction">Optional transaction for the command</param>
        /// <param name="timeout">Timeout period</param>
        /// <param name="main">Use main connection or not</param>
        /// <returns>A paged collection of entities of type <typeparamref name="TEntity"/></returns>
        public IEnumerable<TEntity> GetPaged<TEntity>(int pageNumber, int pageSize, IDbTransaction? transaction = null, int? timeout = null, bool main = false)
            where TEntity : class
        {
            var entities = GetQueryBuilder(main).Paginate<TEntity>(pageNumber, pageSize, transaction ?? GetTransaction(main), timeout);
            List<TEntity> list = [];
            foreach (var entity in entities.List)
                list.Add(ConvertEntity<TEntity>(entity));
            return list;
        }

        /// <summary>
        /// Retrieves a paged set of entities of type <typeparamref name="TEntity"/>
        /// </summary>
        /// <typeparam name="TEntity">The type of the entity</typeparam>
        /// <param name="pageNumber">The number of the page to fetch, starting at 1</param>
        /// <param name="pageSize">The page size</param>
        /// <param name="transaction">Optional transaction for the command</param>
        /// <param name="timeout">Timeout period</param>
        /// <param name="cancellationToken">Optional cancellation token for the command</param>
        /// <param name="main">Use main connection or not</param>
        /// <returns>A paged collection of entities of type <typeparamref name="TEntity"/></returns>
        public async Task<IEnumerable<TEntity>> GetPagedAsync<TEntity>(int pageNumber, int pageSize, IDbTransaction? transaction = null, int? timeout = null, CancellationToken cancellationToken = default, bool main = false)
            where TEntity : class
        {
            var entities = await GetQueryBuilder(main).PaginateAsync<TEntity>(pageNumber, pageSize, transaction ?? GetTransaction(main), timeout);
            List<TEntity> list = [];
            foreach (var entity in entities.List)
                list.Add(ConvertEntity<TEntity>(entity));
            return list;
        }

        /// <summary>
        /// Selects all the entities matching the specified predicate.
        /// </summary>
        /// <typeparam name="TEntity">The type of the entity.</typeparam>
        /// <param name="callback">A predicate to filter the results.</param>
        /// <param name="transaction">Optional transaction for the command.</param>
        /// <param name="timeout">Timeout period</param>
        /// <param name="main">Use main connection or not</param>
        /// <returns></returns>
        public IEnumerable<TEntity> Select<TEntity>(Func<QueryBuilder, QueryBuilder> callback, IDbTransaction? transaction = null, int? timeout = null, bool main = false)
            where TEntity : class
            => GetQueryBuilder(main).Where(callback).Get<TEntity>(transaction ?? GetTransaction(main), timeout).Select(entity => ConvertEntity<TEntity>(entity));

        /// <summary>
        /// Selects all the entities matching the specified predicate.
        /// </summary>
        /// <typeparam name="TEntity">The type of the entity.</typeparam>
        /// <param name="callback">A predicate to filter the results.</param>
        /// <param name="transaction">Optional transaction for the command.</param>
        /// <param name="timeout">Timeout period</param>
        /// <param name="cancellationToken">Optional cancellation token for the command.</param>
        /// <param name="main">Use main connection or not</param>
        /// <returns>A collection of entities of type <typeparamref name="TEntity"/> matching the specified <paramref name="callback"/>.</returns>
        public async Task<IEnumerable<TEntity>> SelectAsync<TEntity>(Func<QueryBuilder, QueryBuilder> callback, IDbTransaction? transaction = null, int? timeout = null, CancellationToken cancellationToken = default, bool main = false)
            where TEntity : class
            => (await GetQueryBuilder(main).Where(callback).GetAsync<TEntity>(transaction ?? GetTransaction(main), timeout, cancellationToken)).Select(entity => ConvertEntity<TEntity>(entity));

        /// <summary>
        /// Selects the first entity matching the specified predicate, or a default value if no entity matched.
        /// </summary>
        /// <typeparam name="TEntity">The type of the entity.</typeparam>
        /// <param name="callback">A predicate to filter the results.</param>
        /// <param name="transaction">Optional transaction for the command.</param>
        /// <param name="timeout">Timeout period</param>
        /// <param name="main">Use main connection or not</param>
        /// <returns>A instance of type <typeparamref name="TEntity"/> matching the specified <paramref name="callback"/></returns>
        public TEntity? First<TEntity>(Func<QueryBuilder, QueryBuilder> callback, IDbTransaction? transaction = null, int? timeout = null, bool main = false)
            where TEntity : class
            => ConvertEntity<TEntity>(GetQueryBuilder(main).Where(callback).FirstOrDefault<TEntity>(transaction ?? GetTransaction(main), timeout));

        /// <summary>
        /// Selects the first entity matching the specified predicate, or a default value if no entity matched.
        /// </summary>
        /// <typeparam name="TEntity">The type of the entity.</typeparam>
        /// <param name="callback">A predicate to filter the results.</param>
        /// <param name="transaction">Optional transaction for the command.</param>
        /// <param name="timeout">Timeout period</param>
        /// <param name="cancellationToken">Optional cancellation token for the command.</param>
        /// <param name="main">Use main connection or not</param>
        /// <returns>A instance of type <typeparamref name="TEntity"/> matching the specified <paramref name="callback"/></returns>
        public async Task<TEntity?> FirstAsync<TEntity>(Func<QueryBuilder, QueryBuilder> callback, IDbTransaction? transaction = null, int? timeout = null, CancellationToken cancellationToken = default, bool main = false)
            where TEntity : class
            => ConvertEntity<TEntity>(await GetQueryBuilder(main).Where(callback).FirstOrDefaultAsync<TEntity>(transaction ?? GetTransaction(main), timeout, cancellationToken));

        /// <summary>
        /// Selects all the entities matching the specified predicate.
        /// </summary>
        /// <typeparam name="TEntity">The type of the entity.</typeparam>
        /// <param name="callback">A predicate to filter the results.</param>
        /// <param name="pageNumber">The number of the page to fetch, starting at 1.</param>
        /// <param name="pageSize">The page size.</param>
        /// <param name="transaction">Optional transaction for the command.</param>
        /// <param name="timeout">Timeout period</param>
        /// <param name="main">Use main connection or not</param>
        /// <returns>
        /// A collection of entities of type <typeparamref name="TEntity"/> matching the specified <paramref name="callback"/>.
        /// </returns>
        public IEnumerable<TEntity> SelectPaged<TEntity>(Func<QueryBuilder, QueryBuilder> callback, int pageNumber, int pageSize, IDbTransaction? transaction = null, int? timeout = null, bool main = false)
            where TEntity : class
        {
            var entities = GetQueryBuilder(main).Where(callback).Paginate<TEntity>(pageNumber, pageSize, transaction ?? GetTransaction(main), timeout);
            List<TEntity> list = [];
            foreach (var entity in entities.List)
                list.Add(ConvertEntity<TEntity>(entity));
            return list;
        }

        /// <summary>
        /// Selects all the entities matching the specified predicate.
        /// </summary>
        /// <typeparam name="TEntity">The type of the entity.</typeparam>
        /// <param name="callback">A predicate to filter the results.</param>
        /// <param name="pageNumber">The number of the page to fetch, starting at 1.</param>
        /// <param name="pageSize">The page size.</param>
        /// <param name="transaction">Optional transaction for the command.</param>
        /// <param name="cancellationToken">Optional cancellation token for the command.</param>
        /// <param name="timeout">Timeout period</param>
        /// <param name="main">Use main connection or not</param>
        /// <returns>A collection of entities of type <typeparamref name="TEntity"/> matching the specified <paramref name="callback"/>.</returns>
        public async Task<IEnumerable<TEntity>> SelectPagedAsync<TEntity>(Func<QueryBuilder, QueryBuilder> callback, int pageNumber, int pageSize, IDbTransaction? transaction = null, int? timeout = null, CancellationToken cancellationToken = default, bool main = false)
            where TEntity : class
        {
            var entities = await GetQueryBuilder(main).Where(callback).PaginateAsync<TEntity>(pageNumber, pageSize, transaction ?? GetTransaction(main), timeout, cancellationToken);
            List<TEntity> list = [];
            foreach (var entity in entities.List)
                list.Add(ConvertEntity<TEntity>(entity));
            return list;
        }

        /// <summary>
        /// Inserts the specified entity into the database and returns the ID
        /// </summary>
        /// <typeparam name="TEntity">The type of the entity</typeparam>
        /// <param name="entity">The entity to be inserted</param>
        /// <param name="transaction">Optional transaction for the command</param>
        /// <param name="timeout">Timeout period</param>
        /// <param name="main">Use main connection or not</param>
        /// <returns>A value indicating whether the update insert succeeded</returns>
        public bool Insert<TEntity>(TEntity entity, IDbTransaction? transaction = null, int? timeout = null, bool main = false)
            where TEntity : class
        {
            var prop = typeof(TEntity).GetRuntimeProperties().FirstOrDefault(p =>
                p.GetCustomAttribute(typeof(DatabaseGeneratedAttribute)) is DatabaseGeneratedAttribute attr &&
                attr.DatabaseGeneratedOption == DatabaseGeneratedOption.Identity);
            if (prop?.PropertyType == typeof(int) || prop?.PropertyType == typeof(Guid))
                return ConvertToBool(Invoke(GetMethod(this, "InsertGetId")?.MakeGenericMethod(typeof(TEntity), prop.PropertyType), [entity, transaction, timeout, main]));
            return GetQueryBuilder(main).Insert(BuildDictionaryFromObject(entity, Crud.Create), transaction ?? GetTransaction(main), timeout) > 0;
        }

        /// <summary>
        /// Inserts the specified entity into the database and returns the ID
        /// </summary>
        /// <typeparam name="TEntity">The type of the entity</typeparam>
        /// <param name="entity">The entity to be inserted</param>
        /// <param name="transaction">Optional transaction for the command</param>
        /// <param name="timeout">Timeout period</param>
        /// <param name="cancellationToken">Optional cancellation token for the command</param>
        /// <param name="main">Use main connection or not</param>
        /// <returns>A value indicating whether the insert operation succeeded</returns>
        public async Task<bool> InsertAsync<TEntity>(TEntity entity, IDbTransaction? transaction = null, int? timeout = null, CancellationToken cancellationToken = default, bool main = false)
            where TEntity : class
        {
            var prop = typeof(TEntity).GetRuntimeProperties().FirstOrDefault(p =>
                p.GetCustomAttribute(typeof(DatabaseGeneratedAttribute)) is DatabaseGeneratedAttribute attr &&
                attr.DatabaseGeneratedOption == DatabaseGeneratedOption.Identity);
            if (prop?.PropertyType == typeof(int) || prop?.PropertyType == typeof(Guid))
                return ConvertToBool(await InvokeAsync(GetMethod(this, "InsertGetIdAsync")?.MakeGenericMethod(typeof(TEntity), prop.PropertyType), [entity, transaction, timeout, cancellationToken, main]));
            return await GetQueryBuilder(main).InsertAsync(BuildDictionaryFromObject(entity, Crud.Create), transaction ?? GetTransaction(main), timeout) > 0;
        }

        /// <summary>
        /// Inserts the specified entity into the database and returns the ID
        /// </summary>
        /// <typeparam name="TEntity">The type of the entity</typeparam>
        /// <typeparam name="T">The type of the ID</typeparam>
        /// <param name="entity">The entity to be inserted</param>
        /// <param name="transaction">Optional transaction for the command</param>
        /// <param name="timeout">Timeout period</param>
        /// <param name="main">Use main connection or not</param>
        /// <returns>The ID of the inserted entity</returns>
        public virtual T InsertGetId<TEntity, T>(TEntity entity, IDbTransaction? transaction = null, int? timeout = null, bool main = false)
            where TEntity : class
        {
            var row = BuildDictionaryFromObject(entity, Crud.Create);
            T id = GetQueryBuilder(main).InsertGetId<T>(row, transaction ?? GetTransaction(main), timeout);
            var prop = typeof(TEntity).GetRuntimeProperties().FirstOrDefault(p =>
                p.GetCustomAttribute(typeof(DatabaseGeneratedAttribute)) is DatabaseGeneratedAttribute attr &&
                attr.DatabaseGeneratedOption == DatabaseGeneratedOption.Identity);
            prop?.SetValue(entity, id);
            return id;
        }

        /// <summary>
        /// Inserts the specified entity into the database and returns the ID
        /// </summary>
        /// <typeparam name="TEntity">The type of the entity</typeparam>
        /// <typeparam name="T">The type of the ID</typeparam>
        /// <param name="entity">The entity to be inserted</param>
        /// <param name="transaction">Optional transaction for the command</param>
        /// <param name="timeout">Timeout period</param>
        /// <param name="cancellationToken">Optional cancellation token for the command</param>
        /// <param name="main">Use main connection or not</param>
        /// <returns>The ID of the inserted entity</returns>
        public virtual async Task<T> InsertGetIdAsync<TEntity, T>(TEntity entity, IDbTransaction? transaction = null, int? timeout = null, CancellationToken cancellationToken = default, bool main = false)
            where TEntity : class
        {
            var row = BuildDictionaryFromObject(entity, Crud.Create);
            T id = await GetQueryBuilder(main).InsertGetIdAsync<T>(row, transaction ?? GetTransaction(main), timeout, cancellationToken);
            var prop = typeof(TEntity).GetRuntimeProperties().FirstOrDefault(p =>
                p.GetCustomAttribute(typeof(DatabaseGeneratedAttribute)) is DatabaseGeneratedAttribute attr &&
                attr.DatabaseGeneratedOption == DatabaseGeneratedOption.Identity);
            prop?.SetValue(entity, id);
            return id;
        }

        /// <summary>
        /// Inserts the specified collection of entities into the database
        /// </summary>
        /// <typeparam name="TEntity">The type of the entity</typeparam>
        /// <param name="entities">The entities to be inserted</param>
        /// <param name="transaction">Optional transaction for the command</param>
        /// <param name="timeout">Timeout period</param>
        /// <param name="main">Use main connection or not</param>
        /// <returns>The number of rows affected</returns>
        public int InsertAll<TEntity>(IEnumerable<TEntity> entities, IDbTransaction? transaction = null, int? timeout = null, bool main = false)
            where TEntity : class
        {
            var qb = GetQueryBuilder(main);
            int count = 0;
            foreach (var entity in entities)
                count += qb.Insert(BuildDictionaryFromObject(entity, Crud.Create), transaction ?? GetTransaction(main), timeout);
            return count;
        }

        /// <summary>
        /// Inserts the specified collection of entities into the database
        /// </summary>
        /// <typeparam name="TEntity">The type of the entity</typeparam>
        /// <param name="entities">The entities to be inserted</param>
        /// <param name="transaction">Optional transaction for the command</param>
        /// <param name="timeout">Timeout period</param>
        /// <param name="cancellationToken">Optional cancellation token for the command</param>
        /// <param name="main">Use main connection or not</param>
        /// <returns>The number of rows affected</returns>
        public async Task<int> InsertAllAsync<TEntity>(IEnumerable<TEntity> entities, IDbTransaction? transaction = null, int? timeout = null, CancellationToken cancellationToken = default, bool main = false)
            where TEntity : class
        {
            var qb = GetQueryBuilder(main);
            int count = 0;
            foreach (var entity in entities)
                count += await qb.InsertAsync(BuildDictionaryFromObject(entity, Crud.Create), transaction ?? GetTransaction(main), timeout, cancellationToken);
            return count;
        }

        /// <summary>
        /// Updates the values of the specified entity in the database
        /// </summary>
        /// <typeparam name="TEntity">The type of the entity</typeparam>
        /// <param name="entity">The entity in the database</param>
        /// <param name="transaction">Optional transaction for the command</param>
        /// <param name="timeout">Timeout period</param>
        /// <param name="main">Use main connection or not</param>
        /// <returns>A value indicating whether the update operation succeeded</returns>
        public bool Update<TEntity>(TEntity entity, IDbTransaction? transaction = null, int? timeout = null, bool main = false)
            where TEntity : class
            {
            var row = BuildDictionaryFromObject(entity, Crud.Update);
            var filter = GetRowFilter(entity);
            if (filter != null)
                return GetQueryBuilder(main).Where(filter).Update(row, transaction ?? GetTransaction(main), timeout) > 0;
            return false;
        }

        /// <summary>
        /// Updates the values of the specified entity in the database
        /// </summary>
        /// <typeparam name="TEntity">The type of the entity</typeparam>
        /// <param name="entity">The entity in the database</param>
        /// <param name="transaction">Optional transaction for the command</param>
        /// <param name="timeout">Timeout period</param>
        /// <param name="cancellationToken">Optional cancellation token for the command</param>
        /// <param name="main">Use main connection or not</param>
        /// <returns>A value indicating whether the update operation succeeded</returns>
        public async Task<bool> UpdateAsync<TEntity>(TEntity entity, IDbTransaction? transaction = null, int? timeout = null, CancellationToken cancellationToken = default, bool main = false)
            where TEntity : class
            {
            var row = BuildDictionaryFromObject(entity, Crud.Update);
            var filter = GetRowFilter(entity);
            if (filter != null)
                return await GetQueryBuilder(main).Where(filter).UpdateAsync(row, transaction ?? GetTransaction(main), timeout, cancellationToken) > 0;
            return false;
        }

        /// <summary>
        /// Deletes the specified entity from the database
        /// </summary>
        /// <typeparam name="TEntity">The type of the entity</typeparam>
        /// <param name="entity">The entity to be deleted</param>
        /// <param name="transaction">Optional transaction for the command</param>
        /// <param name="timeout">Timeout period</param>
        /// <param name="main">Use main connection or not</param>
        /// <returns>A value indicating whether the delete operation succeeded</returns>
        public bool Delete<TEntity>(TEntity entity, IDbTransaction? transaction = null, int? timeout = null, bool main = false)
            where TEntity : class
        {
            var row = BuildDictionaryFromObject(entity, Crud.Delete);
            var filter = GetRowFilter(entity);
            if (filter != null)
                return GetQueryBuilder(main).Where(filter).Delete(transaction ?? GetTransaction(main), timeout) > 0;
            return false;
        }

        /// <summary>
        /// Deletes the specified entity from the database
        /// </summary>
        /// <typeparam name="TEntity">The type of the entity</typeparam>
        /// <param name="entity">The entity to be deleted</param>
        /// <param name="transaction">Optional transaction for the command</param>
        /// <param name="timeout">Timeout period</param>
        /// <param name="cancellationToken">Optional cancellation token for the command</param>
        /// <param name="main">Use main connection or not</param>
        /// <returns>A value indicating whether the delete operation succeeded</returns>
        public async Task<bool> DeleteAsync<TEntity>(TEntity entity, IDbTransaction? transaction = null, int? timeout = null, CancellationToken cancellationToken = default, bool main = false)
            where TEntity : class
        {
            var row = BuildDictionaryFromObject(entity, Crud.Delete);
            var filter = GetRowFilter(entity);
            if (filter != null)
                return await GetQueryBuilder(main).Where(filter).DeleteAsync(transaction ?? GetTransaction(main), timeout, cancellationToken) > 0;
            return false;
        }

        /// <summary>
        /// Deletes all records from the table
        /// </summary>
        /// <param name="transaction">Optional transaction for the command</param>
        /// <param name="timeout">Timeout period</param>
        /// <param name="main">Use main connection or not</param>
        /// <returns>The number of rows affected</returns>
        public int DeleteAll(IDbTransaction? transaction = null, int? timeout = null, bool main = false)
            => GetQueryBuilder(main).Delete(transaction ?? GetTransaction(main), timeout);

        /// <summary>
        /// Deletes all records from the table
        /// </summary>
        /// <param name="transaction">Optional transaction for the command</param>
        /// <param name="timeout">Timeout period</param>
        /// <param name="cancellationToken">Optional cancellation token for the command</param>
        /// <param name="main">Use main connection or not</param>
        /// <returns>The number of rows affected</returns>
        public async Task<int> DeleteAllAsync(IDbTransaction? transaction = null, int? timeout = null, CancellationToken cancellationToken = default, bool main = false)
            => await GetQueryBuilder(main).DeleteAsync(transaction ?? GetTransaction(main), timeout, cancellationToken);

        /// <summary>
        /// Checks if entity exists
        /// </summary>
        /// <param name="callback">A predicate to filter the results.</param>
        /// <param name="transaction">Optional transaction for the command.</param>
        /// <param name="timeout">Timeout period</param>
        /// <param name="main">Use main connection or not</param>
        /// <returns></returns>
        public bool Exists(Func<QueryBuilder, QueryBuilder> callback, IDbTransaction? transaction = null, int? timeout = null, bool main = false)
            => GetQueryBuilder(main).Where(callback).Exists(transaction ?? GetTransaction(main), timeout);

        /// <summary>
        /// Checks if entity exists
        /// </summary>
        /// <param name="callback">A predicate to filter the results.</param>
        /// <param name="transaction">Optional transaction for the command.</param>
        /// <param name="timeout">Timeout period</param>
        /// <param name="cancellationToken">Optional cancellation token for the command.</param>
        /// <param name="main">Use main connection or not</param>
        /// <returns></returns>
        public async Task<bool> ExistsAsync(Func<QueryBuilder, QueryBuilder> callback, IDbTransaction? transaction = null, int? timeout = null, CancellationToken cancellationToken = default, bool main = false)
            => await GetQueryBuilder(main).Where(callback).ExistsAsync(transaction ?? GetTransaction(main), timeout, cancellationToken);
    }
} // End Partial class
