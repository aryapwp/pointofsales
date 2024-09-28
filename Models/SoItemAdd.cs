namespace ASPNETMaker2024.Models;

// Partial class
public partial class PointofSales {
    /// <summary>
    /// soItemAdd
    /// </summary>
    public static SoItemAdd soItemAdd
    {
        get => HttpData.Get<SoItemAdd>("soItemAdd")!;
        set => HttpData["soItemAdd"] = value;
    }

    /// <summary>
    /// Page class for so_item
    /// </summary>
    public class SoItemAdd : SoItemAddBase
    {
        // Constructor
        public SoItemAdd(Controller controller) : base(controller)
        {
        }

        // Constructor
        public SoItemAdd() : base()
        {
        }
    }

    /// <summary>
    /// Page base class
    /// </summary>
    public class SoItemAddBase : SoItem
    {
        // Page ID
        public string PageID = "add";

        // Project ID
        public string ProjectID = "{C2008EF4-FC4B-4028-B6F7-41627F994A94}";

        // Page object name
        public string PageObjName = "soItemAdd";

        // Title
        public string? Title = null; // Title for <title> tag

        // Page headings
        public string Heading = "";

        public string Subheading = "";

        public string PageHeader = "";

        public string PageFooter = "";

        // Token
        public string? Token = null; // DN

        public bool CheckToken = Config.CheckToken;

        // Action result // DN
        public IActionResult? ActionResult;

        // Cache // DN
        public IMemoryCache? Cache;

        // Page layout
        public bool UseLayout = true;

        // Page terminated // DN
        private bool _terminated = false;

        // Is terminated
        public bool IsTerminated => _terminated;

        // Is lookup
        public bool IsLookup => IsApi() && RouteValues.TryGetValue("controller", out object? name) && SameText(name, Config.ApiLookupAction);

        // Is AutoFill
        public bool IsAutoFill => IsLookup && SameText(Post("ajax"), "autofill");

        // Is AutoSuggest
        public bool IsAutoSuggest => IsLookup && SameText(Post("ajax"), "autosuggest");

        // Is modal lookup
        public bool IsModalLookup => IsLookup && SameText(Post("ajax"), "modal");

        // Page URL
        private string _pageUrl = "";

        // Constructor
        public SoItemAddBase()
        {
            TableName = "so_item";

            // Initialize
            CurrentPage = this;

            // Table CSS class
            TableClass = "table table-striped table-bordered table-hover table-sm ew-desktop-table ew-add-table";

            // Language object
            Language = ResolveLanguage();

            // Table object (soItem)
            if (soItem == null || soItem is SoItem)
                soItem = this;

            // Start time
            StartTime = Environment.TickCount;

            // Debug message
            LoadDebugMessage();

            // Open connection
            Conn = Connection; // DN
        }

        // Page action result
        public IActionResult PageResult()
        {
            if (ActionResult != null)
                return ActionResult;
            SetupMenus();
            return Controller.View();
        }

        // Page heading
        public string PageHeading
        {
            get {
                if (!Empty(Heading))
                    return Heading;
                else if (!Empty(Caption))
                    return Caption;
                else
                    return "";
            }
        }

        // Page subheading
        public string PageSubheading
        {
            get {
                if (!Empty(Subheading))
                    return Subheading;
                if (!Empty(TableName))
                    return Language.Phrase(PageID);
                return "";
            }
        }

        // Page name
        public string PageName => "SoItemAdd";

        // Page URL
        public string PageUrl
        {
            get {
                if (_pageUrl == "") {
                    _pageUrl = PageName + "?";
                }
                return _pageUrl;
            }
        }

        // Show Page Header
        public IHtmlContent ShowPageHeader()
        {
            string header = PageHeader;
            PageDataRendering(ref header);
            if (!Empty(header)) // Header exists, display
                return new HtmlString("<div id=\"ew-page-header\">" + header + "</div>");
            return HtmlString.Empty;
        }

        // Show Page Footer
        public IHtmlContent ShowPageFooter()
        {
            string footer = PageFooter;
            PageDataRendered(ref footer);
            if (!Empty(footer)) // Footer exists, display
                return new HtmlString("<div id=\"ew-page-footer\">" + footer + "</div>");
            return HtmlString.Empty;
        }

        // Valid post
        protected async Task<bool> ValidPost() => !CheckToken || !IsPost() || IsApi() || Antiforgery != null && HttpContext != null && await Antiforgery.IsRequestValidAsync(HttpContext);

        // Create token
        public void CreateToken()
        {
            Token ??= HttpContext != null ? Antiforgery?.GetAndStoreTokens(HttpContext).RequestToken : null;
            CurrentToken = Token ?? ""; // Save to global variable
        }

        // Set field visibility
        public void SetVisibility()
        {
            SO_ITEM_ID.Visible = false;
            SO_ORDER_ID.SetVisibility();
            ITEM_NAME.SetVisibility();
            QUANTITY.SetVisibility();
            PRICE.SetVisibility();
        }

        // Constructor
        public SoItemAddBase(Controller? controller = null): this() { // DN
            if (controller != null)
                Controller = controller;
        }

        /// <summary>
        /// Terminate page
        /// </summary>
        /// <param name="url">URL to rediect to</param>
        /// <returns>Page result</returns>
        public override IActionResult Terminate(string url = "") { // DN
            if (_terminated) // DN
                return new EmptyResult();

            // Page Unload event
            PageUnload();

            // Global Page Unloaded event
            PageUnloaded();
            PageUnloadedEventHandler?.Invoke(this, EventArgs.Empty);
            if (!IsApi())
                PageRedirecting(ref url);

            // Gargage collection
            Collect(); // DN

            // Terminate
            _terminated = true; // DN

            // Return for API
            if (IsApi()) {
                var result = new Dictionary<string, string> { { "version", Config.ProductVersion } };
                if (!Empty(url)) // Add url
                    result.Add("url", GetUrl(url));
                foreach (var (key, value) in GetMessages()) // Add messages
                    result.Add(key, value);
                return Controller.Json(result);
            } else if (ActionResult != null) { // Check action result
                return ActionResult;
            }

            // Go to URL if specified
            if (!Empty(url)) {
                if (!Config.Debug)
                    ResponseClear();
                if (Response != null && !Response.HasStarted) {
                    // Handle modal response
                    if (IsModal) { // Show as modal
                        string pageName = GetPageName(url);
                        var result = new Dictionary<string, string> { {"url", GetUrl(url)}, {"modal", "1"} }; // Assume return to modal for simplicity
                        if (SameString(pageName, GetPageName(ListUrl)) ||
                            SameString(pageName, GetPageName(ViewUrl)) ||
                            SameString(pageName, GetPageName(GetCurrentMasterTable()?.ViewUrl ?? ""))
                        ) { // List / View / Master View page
                            if (!SameString(pageName, ListUrl)) { // Not List page
                                result.Add("caption", GetModalCaption(pageName));
                                result.Add("view", pageName == "SoItemView" ? "1" : "0"); // If View page, no primary button
                            } else { // List page
                                // result.Add("list", PageID == "search" ? "1" : "0"); // Refresh List page if current page is Search page
                                result.Add("error", FailureMessage); // List page should not be shown as modal => error
                                ClearFailureMessage();
                            }
                        } else { // Other pages (add messages and then clear messages)
                            result = GetMessages();
                            result.Add("modal", "1");
                            ClearMessages();
                        }
                        return Controller.Json(result);
                    } else {
                        SaveDebugMessage();
                        return Controller.LocalRedirect(AppPath(url));
                    }
                }
            }
            return new EmptyResult();
        }

        // Get all records from datareader
        [return: NotNullIfNotNull("dr")]
        protected async Task<List<Dictionary<string, object>>> GetRecordsFromRecordset(DbDataReader? dr)
        {
            List<Dictionary<string, object>> rows = [];
            while (dr != null && await dr.ReadAsync()) {
                await LoadRowValues(dr); // Set up DbValue/CurrentValue
                if (GetRecordFromDictionary(GetDictionary(dr)) is Dictionary<string, object> row)
                    rows.Add(row);
            }
            return rows;
        }

        // Get all records from the list of records
        #pragma warning disable 1998

        protected async Task<List<Dictionary<string, object>>> GetRecordsFromRecordset(List<Dictionary<string, object>>? list)
        {
            List<Dictionary<string, object>> rows = [];
            if (list != null) {
                foreach (var row in list) {
                    if (GetRecordFromDictionary(row) is Dictionary<string, object> d)
                       rows.Add(row);
                }
            }
            return rows;
        }
        #pragma warning restore 1998

        // Get the first record from datareader
        [return: NotNullIfNotNull("dr")]
        protected async Task<Dictionary<string, object>?> GetRecordFromRecordset(DbDataReader? dr)
        {
            if (dr != null) {
                await LoadRowValues(dr); // Set up DbValue/CurrentValue
                return GetRecordFromDictionary(GetDictionary(dr));
            }
            return null;
        }

        // Get the first record from the list of records
        protected Dictionary<string, object>? GetRecordFromRecordset(List<Dictionary<string, object>>? list) =>
            list != null && list.Count > 0 ? GetRecordFromDictionary(list[0]) : null;

        // Get record from Dictionary
        protected Dictionary<string, object>? GetRecordFromDictionary(Dictionary<string, object>? dict) {
            if (dict == null)
                return null;
            var row = new Dictionary<string, object>();
            foreach (var (key, value) in dict) {
                if (Fields.TryGetValue(key, out DbField? fld) && fld != null) {
                    if (fld.Visible || fld.IsPrimaryKey) { // Primary key or Visible
                        if (fld.HtmlTag == "FILE") { // Upload field
                            if (Empty(value)) {
                                // row[key] = null;
                            } else {
                                if (fld.DataType == DataType.Blob) {
                                    string url = FullUrl(GetPageName(Config.ApiUrl) + "/" + Config.ApiFileAction + "/" + fld.TableVar + "/" + fld.Param + "/" + GetRecordKeyValue(dict)); // Query string format
                                    row[key] = new Dictionary<string, object> { { "type", ContentType((byte[])value) }, { "url", url }, { "name", fld.Param + ContentExtension((byte[])value) } };
                                } else if (!fld.UploadMultiple || !ConvertToString(value).Contains(Config.MultipleUploadSeparator)) { // Single file
                                    string url = FullUrl(GetPageName(Config.ApiUrl) + "/" + Config.ApiFileAction + "/" + fld.TableVar + "/" + Encrypt(fld.PhysicalUploadPath + ConvertToString(value))); // Query string format
                                    row[key] = new Dictionary<string, object> { { "type", ContentType(ConvertToString(value)) }, { "url", url }, { "name", ConvertToString(value) } };
                                } else { // Multiple files
                                    var files = ConvertToString(value).Split(Config.MultipleUploadSeparator);
                                    row[key] = files.Where(file => !Empty(file)).Select(file => new Dictionary<string, object> { { "type", ContentType(file) }, { "url", FullUrl(GetPageName(Config.ApiUrl) + "/" + Config.ApiFileAction + "/" + fld.TableVar + "/" + Encrypt(fld.PhysicalUploadPath + file)) }, { "name", file } });
                                }
                            }
                        } else {
                            string val = ConvertToString(value);
                            if (fld.DataType == DataType.Date && value is DateTime dt)
                                val = dt.ToString("s");
                            row[key] = ConvertToString(val);
                        }
                    }
                }
            }
            return row;
        }

        // Get record key value from array
        protected string GetRecordKeyValue(Dictionary<string, object> dict) {
            string key = "";
            key += UrlEncode(ConvertToString(dict.ContainsKey("SO_ITEM_ID") ? dict["SO_ITEM_ID"] : SO_ITEM_ID.CurrentValue));
            return key;
        }

        // Hide fields for Add/Edit
        protected void HideFieldsForAddEdit() {
            if (IsAdd || IsCopy || IsGridAdd)
                SO_ITEM_ID.Visible = false;
        }

        #pragma warning disable 219
        /// <summary>
        /// Lookup data from table
        /// </summary>
        public async Task<Dictionary<string, object>> Lookup(Dictionary<string, string>? dict = null)
        {
            Language = ResolveLanguage();
            Security = ResolveSecurity();
            string? v;

            // Get lookup object
            string fieldName = IsDictionary(dict) && dict.TryGetValue("field", out v) && v != null ? v : Post("field");
            var lookupField = FieldByName(fieldName);
            var lookup = lookupField?.Lookup;
            if (lookup == null) // DN
                return new Dictionary<string, object>();
            string lookupType = IsDictionary(dict) && dict.TryGetValue("ajax", out v) && v != null ? v : (Post("ajax") ?? "unknown");
            int pageSize = -1;
            int offset = -1;
            string searchValue = "";
            if (SameText(lookupType, "modal") || SameText(lookupType, "filter")) {
                searchValue = IsDictionary(dict) && (dict.TryGetValue("q", out v) && v != null || dict.TryGetValue("sv", out v) && v != null)
                    ? v
                    : (Param("q") ?? Post("sv"));
                pageSize = IsDictionary(dict) && (dict.TryGetValue("n", out v) || dict.TryGetValue("recperpage", out v))
                    ? ConvertToInt(v)
                    : (IsNumeric(Param("n")) ? Param<int>("n") : (Post("recperpage", out StringValues rpp) ? ConvertToInt(rpp.ToString()) : 10));
            } else if (SameText(lookupType, "autosuggest")) {
                searchValue = IsDictionary(dict) && dict.TryGetValue("q", out v) && v != null ? v : Param("q");
                pageSize = IsDictionary(dict) && dict.TryGetValue("n", out v) ? ConvertToInt(v) : (IsNumeric(Param("n")) ? Param<int>("n") : -1);
                if (pageSize <= 0)
                    pageSize = Config.AutoSuggestMaxEntries;
            }
            int start = IsDictionary(dict) && dict.TryGetValue("start", out v) ? ConvertToInt(v) : (IsNumeric(Param("start")) ? Param<int>("start") : -1);
            int page = IsDictionary(dict) && dict.TryGetValue("page", out v) ? ConvertToInt(v) : (IsNumeric(Param("page")) ? Param<int>("page") : -1);
            offset = start >= 0 ? start : (page > 0 && pageSize > 0 ? (page - 1) * pageSize : 0);
            string userSelect = Decrypt(IsDictionary(dict) && dict.TryGetValue("s", out v) && v != null ? v : Post("s"));
            string userFilter = Decrypt(IsDictionary(dict) && dict.TryGetValue("f", out v) && v != null ? v : Post("f"));
            string userOrderBy = Decrypt(IsDictionary(dict) && dict.TryGetValue("o", out v) && v != null ? v : Post("o"));

            // Selected records from modal, skip parent/filter fields and show all records
            lookup.LookupType = lookupType; // Lookup type
            lookup.FilterValues.Clear(); // Clear filter values first
            StringValues keys = IsDictionary(dict) && dict.TryGetValue("keys", out v) && !Empty(v)
                ? (StringValues)v
                : (Post("keys[]", out StringValues k) ? (StringValues)k : StringValues.Empty);
            if (!Empty(keys)) { // Selected records from modal
                lookup.FilterFields = new(); // Skip parent fields if any
                pageSize = -1; // Show all records
                lookup.FilterValues.Add(String.Join(",", keys.ToArray()));
            } else { // Lookup values
                string lookupValue = IsDictionary(dict) && (dict.TryGetValue("v0", out v) && v != null || dict.TryGetValue("lookupValue", out v) && v != null)
                    ? v
                    : (Post<string>("v0") ?? Post("lookupValue"));
                lookup.FilterValues.Add(lookupValue);
            }
            int cnt = IsDictionary(lookup.FilterFields) ? lookup.FilterFields.Count : 0;
            for (int i = 1; i <= cnt; i++) {
                var val = UrlDecode(IsDictionary(dict) && dict.TryGetValue("v" + i, out v) ? v : Post("v" + i));
                if (val != null) // DN
                    lookup.FilterValues.Add(val);
            }
            lookup.SearchValue = searchValue;
            lookup.PageSize = pageSize;
            lookup.Offset = offset;
            if (userSelect != "")
                lookup.UserSelect = userSelect;
            if (userFilter != "")
                lookup.UserFilter = userFilter;
            if (userOrderBy != "")
                lookup.UserOrderBy = userOrderBy;
            return await lookup.ToJson(this);
        }
        #pragma warning restore 219

        // Properties
        public string FormClassName = "ew-form ew-add-form";

        public bool IsModal = false;

        public bool IsMobileOrModal = false;

        public string DbMasterFilter = "";

        public string DbDetailFilter = "";

        public int StartRecord;

        public DbDataReader? Recordset = null; // Reserved // DN

        public bool CopyRecord;

        /// <summary>
        /// Page run
        /// </summary>
        /// <returns>Page result</returns>
        public override async Task<IActionResult> Run()
        {
            // Is modal
            IsModal = Param<bool>("modal");
            UseLayout = UseLayout && !IsModal;

            // Use layout
            if (!Empty(Param("layout")) && !Param<bool>("layout"))
                UseLayout = false;

            // User profile
            Profile = ResolveProfile();

            // Security
            Security = ResolveSecurity();
            if (TableVar != "")
                Security.LoadTablePermissions(TableVar);

            // Load user profile
            if (IsLoggedIn()) {
                await Profile.SetUserName(CurrentUserName()).LoadFromStorageAsync();
            }

            // Create form object
            CurrentForm ??= new();
            await CurrentForm.Init();
            CurrentAction = Param("action"); // Set up current action
            SetVisibility();

            // Do not use lookup cache
            if (!Config.LookupCachePageIds.Contains(PageID))
                SetUseLookupCache(false);

            // Global Page Loading event
            PageLoading();
            PageLoadingEventHandler?.Invoke(this, EventArgs.Empty);

            // Page Load event
            PageLoad();

            // Check token
            if (!await ValidPost())
                End(Language.Phrase("InvalidPostRequest"));

            // Check action result
            if (ActionResult != null) // Action result set by server event // DN
                return ActionResult;

            // Create token
            CreateToken();

            // Hide fields for add/edit
            if (!UseAjaxActions)
                HideFieldsForAddEdit();
            // Use inline delete
            if (UseAjaxActions)
                InlineDelete = true;

            // Load default values for add
            LoadDefaultValues();

            // Check modal
            if (IsModal)
                SkipHeaderFooter = true;
            IsMobileOrModal = IsMobile() || IsModal;
            bool postBack = false;
            StringValues sv;

            // Set up current action
            if (IsApi()) {
                CurrentAction = "insert"; // Add record directly
                postBack = true;
            } else if (!Empty(Post("action"))) {
                CurrentAction = Post("action"); // Get form action
                if (Post(OldKeyName, out sv))
                    SetKey(sv.ToString());
                postBack = true;
            } else {
                // Load key from QueryString
                string[] keyValues = {};
                object? rv;
                if (RouteValues.TryGetValue("key", out object? k))
                    keyValues = ConvertToString(k).Split('/'); // For Copy page
                if (RouteValues.TryGetValue("SO_ITEM_ID", out rv)) { // DN
                    SO_ITEM_ID.QueryValue = UrlDecode(rv); // DN
                } else if (Get("SO_ITEM_ID", out sv)) {
                    SO_ITEM_ID.QueryValue = sv.ToString();
                }
                OldKey = GetKey(true); // Get from CurrentValue
                CopyRecord = !Empty(OldKey);
                if (CopyRecord) {
                    CurrentAction = "copy"; // Copy record
                    SetKey(OldKey); // Set up record key
                } else {
                    CurrentAction = "show"; // Display blank record
                }
            }

            // Load old record / default values
            var rsold = await LoadOldRecord();

            // Load form values
            if (postBack) {
                await LoadFormValues(); // Load form values
            }

            // Validate form if post back
            if (postBack) {
                if (!await ValidateForm()) {
                    EventCancelled = true; // Event cancelled
                    RestoreFormValues(); // Restore form values
                    if (IsApi())
                        return Terminate();
                    else
                        CurrentAction = "show"; // Form error, reset action
                }
            }

            // Perform current action
            switch (CurrentAction) {
                case "copy": // Copy an existing record
                    if (rsold == null) { // Record not loaded
                        if (Empty(FailureMessage))
                            FailureMessage = Language.Phrase("NoRecord"); // No record found
                        return Terminate("SoItemList"); // No matching record, return to List page // DN
                    }
                    break;
                case "insert": // Add new record // DN
                    SendEmail = true; // Send email on add success
                    var res = await AddRow(rsold);
                    if (res) { // Add successful
                        if (Empty(SuccessMessage) && Post("addopt") != "1") // Skip success message for addopt (done in JavaScript)
                            SuccessMessage = Language.Phrase("AddSuccess"); // Set up success message
                        string returnUrl = "";
                        returnUrl = ReturnUrl;
                        if (GetPageName(returnUrl) == "SoItemList")
                            returnUrl = AddMasterUrl(ListUrl); // List page, return to List page with correct master key if necessary
                        else if (GetPageName(returnUrl) == "SoItemView")
                            returnUrl = ViewUrl; // View page, return to View page with key URL directly

                        // Handle UseAjaxActions
                        if (IsModal && UseAjaxActions) {
                            IsModal = false;
                            if (GetPageName(returnUrl) != "SoItemList") {
                                TempData["Return-Url"] = returnUrl; // Save return URL
                                returnUrl = "SoItemList"; // Return list page content
                            }
                        }
                        if (IsJsonResponse()) { // Return to caller
                            ClearMessages(); // Clear messages for Json response
                            return res;
                        } else {
                            return Terminate(returnUrl);
                        }
                    } else if (IsApi()) { // API request, return
                        return Terminate();
                    } else {
                        EventCancelled = true; // Event cancelled
                        RestoreFormValues(); // Add failed, restore form values
                    }
                    break;
            }

            // Set up Breadcrumb
            SetupBreadcrumb();

            // Render row based on row type
            RowType = RowType.Add; // Render add type

            // Render row
            ResetAttributes();
            await RenderRow();

            // Set LoginStatus, Page Rendering and Page Render
            if (!IsApi() && !IsTerminated) {
                // Pass login status to client side
                SetClientVar("login", LoginStatus);

                // Global Page Rendering event
                PageRendering();
                PageRenderingEventHandler?.Invoke(this, EventArgs.Empty);

                // Page Render event
                soItemAdd?.PageRender();
            }
            return PageResult();
        }

        // Confirm page
        public bool ConfirmPage = false; // DN

        #pragma warning disable 1998
        // Get upload files
        public async Task GetUploadFiles()
        {
            // Get upload data
        }
        #pragma warning restore 1998

        // Load default values
        protected void LoadDefaultValues() {
        }

        #pragma warning disable 1998
        // Load form values
        protected async Task LoadFormValues() {
            if (CurrentForm == null)
                return;
            bool validate = !Config.ServerValidate;
            string val;

            // Check field name 'SO_ORDER_ID' before field var 'x_SO_ORDER_ID'
            val = CurrentForm.HasValue("SO_ORDER_ID") ? CurrentForm.GetValue("SO_ORDER_ID") : CurrentForm.GetValue("x_SO_ORDER_ID");
            if (!SO_ORDER_ID.IsDetailKey) {
                if (IsApi() && !CurrentForm.HasValue("SO_ORDER_ID") && !CurrentForm.HasValue("x_SO_ORDER_ID")) // DN
                    SO_ORDER_ID.Visible = false; // Disable update for API request
                else
                    SO_ORDER_ID.SetFormValue(val, true, validate);
            }

            // Check field name 'ITEM_NAME' before field var 'x_ITEM_NAME'
            val = CurrentForm.HasValue("ITEM_NAME") ? CurrentForm.GetValue("ITEM_NAME") : CurrentForm.GetValue("x_ITEM_NAME");
            if (!ITEM_NAME.IsDetailKey) {
                if (IsApi() && !CurrentForm.HasValue("ITEM_NAME") && !CurrentForm.HasValue("x_ITEM_NAME")) // DN
                    ITEM_NAME.Visible = false; // Disable update for API request
                else
                    ITEM_NAME.SetFormValue(val);
            }

            // Check field name 'QUANTITY' before field var 'x_QUANTITY'
            val = CurrentForm.HasValue("QUANTITY") ? CurrentForm.GetValue("QUANTITY") : CurrentForm.GetValue("x_QUANTITY");
            if (!QUANTITY.IsDetailKey) {
                if (IsApi() && !CurrentForm.HasValue("QUANTITY") && !CurrentForm.HasValue("x_QUANTITY")) // DN
                    QUANTITY.Visible = false; // Disable update for API request
                else
                    QUANTITY.SetFormValue(val, true, validate);
            }

            // Check field name 'PRICE' before field var 'x_PRICE'
            val = CurrentForm.HasValue("PRICE") ? CurrentForm.GetValue("PRICE") : CurrentForm.GetValue("x_PRICE");
            if (!PRICE.IsDetailKey) {
                if (IsApi() && !CurrentForm.HasValue("PRICE") && !CurrentForm.HasValue("x_PRICE")) // DN
                    PRICE.Visible = false; // Disable update for API request
                else
                    PRICE.SetFormValue(val, true, validate);
            }

            // Check field name 'SO_ITEM_ID' before field var 'x_SO_ITEM_ID'
            val = CurrentForm.HasValue("SO_ITEM_ID") ? CurrentForm.GetValue("SO_ITEM_ID") : CurrentForm.GetValue("x_SO_ITEM_ID");
        }
        #pragma warning restore 1998

        // Restore form values
        public void RestoreFormValues()
        {
            SO_ORDER_ID.CurrentValue = SO_ORDER_ID.FormValue;
            ITEM_NAME.CurrentValue = ITEM_NAME.FormValue;
            QUANTITY.CurrentValue = QUANTITY.FormValue;
            PRICE.CurrentValue = PRICE.FormValue;
        }

        // Load row based on key values
        public async Task<bool> LoadRow()
        {
            string filter = GetRecordFilter();

            // Call Row Selecting event
            RowSelecting(ref filter);

            // Load SQL based on filter
            CurrentFilter = filter;
            string sql = CurrentSql;
            bool res = false;
            try {
                var row = await Connection.GetRowAsync(sql);
                if (row != null) {
                    await LoadRowValues(row);
                    res = true;
                } else {
                    return false;
                }
            } catch {
                if (Config.Debug)
                    throw;
            }
            return res;
        }

        #pragma warning disable 162, 168, 1998, 4014
        // Load row values from data reader
        public async Task LoadRowValues(DbDataReader? dr = null) => await LoadRowValues(GetDictionary(dr));

        // Load row values from recordset
        public async Task LoadRowValues(Dictionary<string, object>? row)
        {
            row ??= NewRow();

            // Call Row Selected event
            RowSelected(row);
            SO_ITEM_ID.SetDbValue(row["SO_ITEM_ID"]);
            SO_ORDER_ID.SetDbValue(row["SO_ORDER_ID"]);
            ITEM_NAME.SetDbValue(row["ITEM_NAME"]);
            QUANTITY.SetDbValue(row["QUANTITY"]);
            PRICE.SetDbValue(row["PRICE"]);
        }
        #pragma warning restore 162, 168, 1998, 4014

        // Return a row with default values
        protected Dictionary<string, object> NewRow() {
            var row = new Dictionary<string, object>();
            row.Add("SO_ITEM_ID", SO_ITEM_ID.DefaultValue ?? DbNullValue); // DN
            row.Add("SO_ORDER_ID", SO_ORDER_ID.DefaultValue ?? DbNullValue); // DN
            row.Add("ITEM_NAME", ITEM_NAME.DefaultValue ?? DbNullValue); // DN
            row.Add("QUANTITY", QUANTITY.DefaultValue ?? DbNullValue); // DN
            row.Add("PRICE", PRICE.DefaultValue ?? DbNullValue); // DN
            return row;
        }

        #pragma warning disable 618, 1998
        // Load old record
        protected async Task<Dictionary<string, object>?> LoadOldRecord(DatabaseConnection<MySqlConnection, MySqlDbType>? cnn = null) {
            // Load old record
            Dictionary<string, object>? row = null;
            bool validKey = !Empty(OldKey);
            if (validKey) {
                SetKey(OldKey);
                CurrentFilter = GetRecordFilter();
                string sql = CurrentSql;
                try {
                    row = await (cnn ?? Connection).GetRowAsync(sql);
                } catch {
                    row = null;
                }
            }
            await LoadRowValues(row); // Load row values
            return row;
        }
        #pragma warning restore 618, 1998

        #pragma warning disable 1998
        // Render row values based on field settings
        public async Task RenderRow()
        {
            // Call Row Rendering event
            RowRendering();

            // Common render codes for all row types

            // SO_ITEM_ID
            SO_ITEM_ID.RowCssClass = "row";

            // SO_ORDER_ID
            SO_ORDER_ID.RowCssClass = "row";

            // ITEM_NAME
            ITEM_NAME.RowCssClass = "row";

            // QUANTITY
            QUANTITY.RowCssClass = "row";

            // PRICE
            PRICE.RowCssClass = "row";

            // View row
            if (RowType == RowType.View) {
                // SO_ITEM_ID
                SO_ITEM_ID.ViewValue = SO_ITEM_ID.CurrentValue;
                SO_ITEM_ID.ViewCustomAttributes = "";

                // SO_ORDER_ID
                SO_ORDER_ID.ViewValue = SO_ORDER_ID.CurrentValue;
                SO_ORDER_ID.ViewValue = FormatNumber(SO_ORDER_ID.ViewValue, SO_ORDER_ID.FormatPattern);
                SO_ORDER_ID.ViewCustomAttributes = "";

                // ITEM_NAME
                ITEM_NAME.ViewValue = ConvertToString(ITEM_NAME.CurrentValue); // DN
                ITEM_NAME.ViewCustomAttributes = "";

                // QUANTITY
                QUANTITY.ViewValue = QUANTITY.CurrentValue;
                QUANTITY.ViewValue = FormatNumber(QUANTITY.ViewValue, QUANTITY.FormatPattern);
                QUANTITY.ViewCustomAttributes = "";

                // PRICE
                PRICE.ViewValue = PRICE.CurrentValue;
                PRICE.ViewValue = FormatNumber(PRICE.ViewValue, PRICE.FormatPattern);
                PRICE.ViewCustomAttributes = "";

                // SO_ORDER_ID
                SO_ORDER_ID.HrefValue = "";

                // ITEM_NAME
                ITEM_NAME.HrefValue = "";

                // QUANTITY
                QUANTITY.HrefValue = "";

                // PRICE
                PRICE.HrefValue = "";
            } else if (RowType == RowType.Add) {
                // SO_ORDER_ID
                SO_ORDER_ID.SetupEditAttributes();
                SO_ORDER_ID.EditValue = SO_ORDER_ID.CurrentValue;
                SO_ORDER_ID.PlaceHolder = RemoveHtml(SO_ORDER_ID.Caption);
                if (!Empty(SO_ORDER_ID.EditValue) && IsNumeric(SO_ORDER_ID.EditValue))
                    SO_ORDER_ID.EditValue = FormatNumber(SO_ORDER_ID.EditValue, null);

                // ITEM_NAME
                ITEM_NAME.SetupEditAttributes();
                if (!ITEM_NAME.Raw)
                    ITEM_NAME.CurrentValue = HtmlDecode(ITEM_NAME.CurrentValue);
                ITEM_NAME.EditValue = HtmlEncode(ITEM_NAME.CurrentValue);
                ITEM_NAME.PlaceHolder = RemoveHtml(ITEM_NAME.Caption);

                // QUANTITY
                QUANTITY.SetupEditAttributes();
                QUANTITY.EditValue = QUANTITY.CurrentValue;
                QUANTITY.PlaceHolder = RemoveHtml(QUANTITY.Caption);
                if (!Empty(QUANTITY.EditValue) && IsNumeric(QUANTITY.EditValue))
                    QUANTITY.EditValue = FormatNumber(QUANTITY.EditValue, null);

                // PRICE
                PRICE.SetupEditAttributes();
                PRICE.EditValue = PRICE.CurrentValue;
                PRICE.PlaceHolder = RemoveHtml(PRICE.Caption);
                if (!Empty(PRICE.EditValue) && IsNumeric(PRICE.EditValue))
                    PRICE.EditValue = FormatNumber(PRICE.EditValue, null);

                // Add refer script

                // SO_ORDER_ID
                SO_ORDER_ID.HrefValue = "";

                // ITEM_NAME
                ITEM_NAME.HrefValue = "";

                // QUANTITY
                QUANTITY.HrefValue = "";

                // PRICE
                PRICE.HrefValue = "";
            }
            if (RowType == RowType.Add || RowType == RowType.Edit || RowType == RowType.Search) // Add/Edit/Search row
                SetupFieldTitles();

            // Call Row Rendered event
            if (RowType != RowType.AggregateInit)
                RowRendered();
        }
        #pragma warning restore 1998

        #pragma warning disable 1998
        // Validate form
        protected async Task<bool> ValidateForm() {
            // Check if validation required
            if (!Config.ServerValidate)
                return true;
            bool validateForm = true;
                if (SO_ORDER_ID.Visible && SO_ORDER_ID.Required) {
                    if (!SO_ORDER_ID.IsDetailKey && Empty(SO_ORDER_ID.FormValue)) {
                        SO_ORDER_ID.AddErrorMessage(ConvertToString(SO_ORDER_ID.RequiredErrorMessage).Replace("%s", SO_ORDER_ID.Caption));
                    }
                }
                if (!CheckInteger(SO_ORDER_ID.FormValue)) {
                    SO_ORDER_ID.AddErrorMessage(SO_ORDER_ID.GetErrorMessage(false));
                }
                if (ITEM_NAME.Visible && ITEM_NAME.Required) {
                    if (!ITEM_NAME.IsDetailKey && Empty(ITEM_NAME.FormValue)) {
                        ITEM_NAME.AddErrorMessage(ConvertToString(ITEM_NAME.RequiredErrorMessage).Replace("%s", ITEM_NAME.Caption));
                    }
                }
                if (QUANTITY.Visible && QUANTITY.Required) {
                    if (!QUANTITY.IsDetailKey && Empty(QUANTITY.FormValue)) {
                        QUANTITY.AddErrorMessage(ConvertToString(QUANTITY.RequiredErrorMessage).Replace("%s", QUANTITY.Caption));
                    }
                }
                if (!CheckInteger(QUANTITY.FormValue)) {
                    QUANTITY.AddErrorMessage(QUANTITY.GetErrorMessage(false));
                }
                if (PRICE.Visible && PRICE.Required) {
                    if (!PRICE.IsDetailKey && Empty(PRICE.FormValue)) {
                        PRICE.AddErrorMessage(ConvertToString(PRICE.RequiredErrorMessage).Replace("%s", PRICE.Caption));
                    }
                }
                if (!CheckNumber(PRICE.FormValue)) {
                    PRICE.AddErrorMessage(PRICE.GetErrorMessage(false));
                }

            // Return validate result
            validateForm = validateForm && !HasInvalidFields();

            // Call Form CustomValidate event
            string formCustomError = "";
            validateForm = validateForm && FormCustomValidate(ref formCustomError);
            if (!Empty(formCustomError))
                FailureMessage = formCustomError;
            return validateForm;
        }
        #pragma warning restore 1998

        // Add record
        #pragma warning disable 168, 219

        protected async Task<JsonBoolResult> AddRow(Dictionary<string, object>? rsold = null) { // DN
            bool result = false;

            // Get new row
            Dictionary<string, object> rsnew = GetAddRow();
            if (rsnew.Count == 0)
                return JsonBoolResult.FalseResult;

            // Update current values
            SetCurrentValues(rsnew);

            // Load db values from rsold
            LoadDbValues(rsold);

            // Call Row Inserting event
            bool insertRow = RowInserting(rsold, rsnew);
            if (insertRow) {
                try {
                    result = ConvertToBool(await InsertAsync(rsnew));
                    rsnew["SO_ITEM_ID"] = SO_ITEM_ID.CurrentValue!;
                } catch (Exception e) {
                    if (Config.Debug)
                        throw;
                    FailureMessage = e.Message;
                    result = false;
                }
            } else {
                if (SuccessMessage != "" || FailureMessage != "") {
                    // Use the message, do nothing
                } else if (CancelMessage != "") {
                    FailureMessage = CancelMessage;
                    CancelMessage = "";
                } else {
                    FailureMessage = Language.Phrase("InsertCancelled");
                }
                result = false;
            }

            // Call Row Inserted event
            if (result)
                RowInserted(rsold, rsnew);

            // Write JSON for API request
            Dictionary<string, object> d = new();
            d.Add("success", result);
            if (IsJsonResponse() && result) {
                if (GetRecordFromDictionary(rsnew) is var row && row != null) {
                    string table = TableVar;
                    d.Add(table, row);
                }
                d.Add("action", Config.ApiAddAction);
                d.Add("version", Config.ProductVersion);
                return new JsonBoolResult(d, result);
            }
            return new JsonBoolResult(d, result);
        }

        /// <summary>
        /// Get add row
        /// </summary>
        /// <returns>New row</returns>
        protected Dictionary<string, object> GetAddRow()
        {
            try {
                Dictionary<string, object> rsnew = new();

                // SO_ORDER_ID
                SO_ORDER_ID.SetDbValue(rsnew, SO_ORDER_ID.CurrentValue, Empty(SO_ORDER_ID.CurrentValue));

                // ITEM_NAME
                ITEM_NAME.SetDbValue(rsnew, ITEM_NAME.CurrentValue);

                // QUANTITY
                QUANTITY.SetDbValue(rsnew, QUANTITY.CurrentValue, Empty(QUANTITY.CurrentValue));

                // PRICE
                PRICE.SetDbValue(rsnew, PRICE.CurrentValue, Empty(PRICE.CurrentValue));
                return rsnew;
            } catch (Exception e) {
                if (Config.Debug)
                    throw;
                FailureMessage = e.Message;
                return new();
            }
        }

        /// <summary>
        /// Restore add form from row
        /// </summary>
        /// <param name="row">Current row</param>
        private void RestoreAddFormFromRow(Dictionary<string, object> row)
        {
            object? value;
            if (row.TryGetValue("SO_ORDER_ID", out value)) // SO_ORDER_ID
                SO_ORDER_ID.SetFormValue(ConvertToString(value));
            if (row.TryGetValue("ITEM_NAME", out value)) // ITEM_NAME
                ITEM_NAME.SetFormValue(ConvertToString(value));
            if (row.TryGetValue("QUANTITY", out value)) // QUANTITY
                QUANTITY.SetFormValue(ConvertToString(value));
            if (row.TryGetValue("PRICE", out value)) // PRICE
                PRICE.SetFormValue(ConvertToString(value));
        }

        // Set up Breadcrumb
        protected void SetupBreadcrumb()
        {
            var breadcrumb = new Breadcrumb();
            string url = CurrentUrl();
            breadcrumb.Add("list", TableVar, AppPath(AddMasterUrl("SoItemList")), "", TableVar, true);
            string pageId = IsCopy ? "Copy" : "Add";
            breadcrumb.Add("add", pageId, url);
            CurrentBreadcrumb = breadcrumb;
        }

        // Setup lookup options
        public async Task SetupLookupOptions(DbField fld)
        {
            if (fld.Lookup == null)
                return;
            Func<string>? lookupFilter = null;
            dynamic conn = Connection;
            if (fld.Lookup.Options.Count is int c && c == 0) {
                // Always call to Lookup.GetSql so that user can setup Lookup.Options in Lookup Selecting server event
                var sql = fld.Lookup.GetSql(false, "", lookupFilter, this);

                // Set up lookup cache
                if (!fld.HasLookupOptions && fld.UseLookupCache && !Empty(sql) && fld.Lookup.ParentFields.Count == 0 && fld.Lookup.Options.Count == 0) {
                    int totalCnt = await TryGetRecordCountAsync(sql, conn);
                    if (totalCnt > fld.LookupCacheCount) // Total count > cache count, do not cache
                        return;
                    var dict = new Dictionary<string, Dictionary<string, object>>();
                    List<object> values = [];
                    List<Dictionary<string, object>> rs = await conn.GetRowsAsync(sql);
                    if (rs != null) {
                        for (int i = 0; i < rs.Count; i++) {
                            var row = rs[i];
                            row = fld.Lookup?.RenderViewRow(row, Resolve(fld.Lookup.LinkTable));
                            string key = row?.Values.First()?.ToString() ?? String.Empty;
                            if (!dict.ContainsKey(key) && row != null)
                                dict.Add(key, row);
                        }
                    }
                    fld.Lookup?.SetOptions(dict);
                }
            }
        }

        // Close recordset
        public void CloseRecordset()
        {
            using (Recordset) {} // Dispose
        }

        // Page Load event
        public virtual void PageLoad() {
            //Log("Page Load");
        }

        // Page Unload event
        public virtual void PageUnload() {
            //Log("Page Unload");
        }

        // Page Redirecting event
        public virtual void PageRedirecting(ref string url) {
            //url = newurl;
        }

        // Message Showing event
        // type = ""|"success"|"failure"|"warning"
        public virtual void MessageShowing(ref string msg, string type) {
            // Note: Do not change msg outside the following 4 cases.
            if (type == "success") {
                //msg = "your success message";
            } else if (type == "failure") {
                //msg = "your failure message";
            } else if (type == "warning") {
                //msg = "your warning message";
            } else {
                //msg = "your message";
            }
        }

        // Page Load event
        public virtual void PageRender() {
            //Log("Page Render");
        }

        // Page Data Rendering event
        public virtual void PageDataRendering(ref string header) {
            // Example:
            //header = "your header";
        }

        // Page Data Rendered event
        public virtual void PageDataRendered(ref string footer) {
            // Example:
            //footer = "your footer";
        }

        // Page Breaking event
        public void PageBreaking(ref bool brk, ref string content) {
            // Example:
            //	brk = false; // Skip page break, or
            //	content = "<div style=\"page-break-after:always;\">&nbsp;</div>"; // Modify page break content
        }

        // Form Custom Validate event
        public virtual bool FormCustomValidate(ref string customError) {
            //Return error message in customError
            return true;
        }
    } // End page class
} // End Partial class
