namespace ASPNETMaker2024.Models;

// Partial class
public partial class PointofSales {
    /// <summary>
    /// Language class
    /// </summary>
    public class Lang
    {
        public static IComparer<String>? FileComparer = null;

        public static IComparer<Dictionary<string, object>>? LanguageComparer = null;

        public string LanguageId = "en-US";

        [AllowNull]
        public DotAccessData PhraseData = new(); // DN

        public string LanguageFolder = Config.LanguageFolder; // DN

        public string Template = ""; // JsRender template

        public string Method = "prependTo"; // JsRender template method

        public string Target = ".navbar-nav.ms-auto"; // JsRender template target

        public string Type = "LI"; // LI/DROPDOWN (for used with top Navbar) or SELECT/RADIO (NOT for used with top Navbar)

        private JsonTextWriter _writer; // DN

        private StringWriter _stringWriter = new(); // DN

        // Constructor
        public Lang(string langId)
        {
            _writer = new JsonTextWriter(_stringWriter);
            SetLanguage(langId);
        }

        public void SetLanguage(string langId)
        {
            // Set up language ID
            if (IsValidLocaleId(langId)) { // DN
                LanguageId = langId;
            } else { // Locale not supported by server
                Log("Locale " + LanguageId + " not supported by server.");
                LanguageId = "en-US"; // Fallback to "en-US"
            }
            if (HttpContext != null)
                CurrentLanguage = LanguageId;
            LoadLanguage(LanguageId);

            // Call Language Load event
            LanguageLoad();
            LanguageLoadEventHandler?.Invoke(this, EventArgs.Empty);
        }

        // Load language file
        private void LoadLanguage(string id)
        {
            List<string> fileNames = GetFileNames(id);
            if (fileNames.Count == 0)
                throw new Exception("Missing language files");
            foreach (string fileName in fileNames) {
                var phrases = XmlToDictionary(fileName);
                PhraseData.Import(phrases.ToDictionary(kvp => kvp.Key, kvp => (dynamic?)kvp.Value)); // DN
            }

            // Set up locale settings
            JObject locale = LocaleConvert(id).GetAwaiter().GetResult(); // Sync
            CultureInfo ci = CultureInfo.CreateSpecificCulture(id);
            NumberFormat = locale["number"]?.Value<string>() ?? NumberFormat;
            CurrencyFormat = locale["currency"]?.Value<string>() ?? CurrencyFormat;
            PercentFormat = locale["percent"]?.Value<string>() ?? PercentFormat;
            NumberingSystem = locale["numbering_system"]?.Value<string>() ?? "";
            CurrentNumberFormat = ci.NumberFormat;
            CurrentNumberFormat.NumberDecimalSeparator = locale["decimal_separator"]?.Value<string>() ?? CurrentNumberFormat.NumberDecimalSeparator;
            CurrentNumberFormat.CurrencyDecimalSeparator = locale["decimal_separator"]?.Value<string>() ?? CurrentNumberFormat.CurrencyDecimalSeparator;
            CurrentNumberFormat.PercentDecimalSeparator = locale["decimal_separator"]?.Value<string>() ?? CurrentNumberFormat.PercentDecimalSeparator;
            CurrentNumberFormat.NumberGroupSeparator = locale["grouping_separator"]?.Value<string>() ?? CurrentNumberFormat.CurrencyGroupSeparator;
            CurrentNumberFormat.CurrencyGroupSeparator = locale["grouping_separator"]?.Value<string>() ?? CurrentNumberFormat.NumberGroupSeparator;
            CurrentNumberFormat.PercentGroupSeparator = locale["grouping_separator"]?.Value<string>() ?? CurrentNumberFormat.PercentGroupSeparator;
            CurrentNumberFormat.CurrencySymbol = locale["currency_symbol"]?.Value<string>() ?? CurrentNumberFormat.CurrencySymbol;
            CurrentNumberFormat.CurrencyDecimalSeparator = locale["decimal_separator"]?.Value<string>() ?? CurrentNumberFormat.CurrencyDecimalSeparator;
            CurrentNumberFormat.CurrencyGroupSeparator = locale["grouping_separator"]?.Value<string>() ?? CurrentNumberFormat.CurrencyGroupSeparator;
            CurrentNumberFormat.PercentSymbol = locale["percent_symbol"]?.Value<string>() ?? CurrentNumberFormat.PercentSymbol;
            CurrentNumberFormat.PercentDecimalSeparator = locale["decimal_separator"]?.Value<string>() ?? CurrentNumberFormat.PercentDecimalSeparator;
            CurrentNumberFormat.PercentGroupSeparator = locale["grouping_separator"]?.Value<string>() ?? CurrentNumberFormat.PercentGroupSeparator;
            CurrentDateTimeFormat = ci.DateTimeFormat;
            CurrentDateTimeFormat.DateSeparator = locale["date_separator"]?.Value<string>() ?? CurrentDateTimeFormat.DateSeparator;
            CurrentDateTimeFormat.TimeSeparator = locale["time_separator"]?.Value<string>() ?? CurrentDateTimeFormat.TimeSeparator;
            CurrentDateTimeFormat.ShortDatePattern = Regex.Replace(locale["date"]?.Value<string>() ?? CurrentDateTimeFormat.ShortDatePattern, @"\b(G+)\b", m => m.Value.ToLower()); // Make sure "g" in C# format
            CurrentDateTimeFormat.ShortTimePattern = locale["time"]?.Value<string>() ?? CurrentDateTimeFormat.ShortTimePattern;
        }

        // Convert XML to dictionary
        private Dictionary<string, dynamic?> XmlToDictionary(string file) => XElementToDictionary(XElement.Load(file));

        // Convert XElement to dictionary
        private Dictionary<string, dynamic?> XElementToDictionary(XElement el)
        {
            var dict = new Dictionary<string, dynamic?>(StringComparer.InvariantCultureIgnoreCase);
            if (el.HasElements) {
                foreach (var e in el.Elements()) {
                    var name = e.Name.LocalName;
                    var id = e.Attribute("id")?.Value;
                    if (!dict.ContainsKey(name))
                        dict.Add(name, new Dictionary<string, dynamic?>(StringComparer.InvariantCultureIgnoreCase));
                    if (id != null && !e.HasElements && e.Name.LocalName == "phrase") { // phrase
                        var d = e.Attributes().Where(attr => attr.Name.LocalName != "id").ToDictionary(attr => attr.Name.LocalName, attr => (object)attr.Value);
                        dict[name]?.Add(el.Name.LocalName == "global" ? id.ToLowerInvariant() : id, d); // Lower case for global phrase only // DN
                    } else if (id != null && e.HasElements) { // table, field, menu
                        dict[name]?.Add(id, XElementToDictionary(e));
                    } else if (id == null && e.HasElements) { // global, project, datetimepicker, etc.
                        dict[name] = XElementToDictionary(e);
                    }
                }
            }
            return dict;
        }

        // Get language file names by language ID // DN
        private List<string> GetFileNames(string id)
        {
            var files = SearchFiles(MapPath(LanguageFolder), $"*.en-US.xml").ToList(); // Find all *.en-US.xml first
            if (id != "en-US") {
                files = files.Select(f => {
                    string file = Regex.Replace(f, @".en-US.xml$", $"*.{id}.xml");
                    return FileExists(file) ? file : f; // If language file for the specified language ID exists, replace it
                }).ToList();
                files.AddRange(SearchFiles(MapPath(LanguageFolder), $"*.{id}.xml")); // Add other language files for the specified language ID
                files = files.Distinct().ToList(); // Remove duplicates
            }
            if (FileComparer != null)
                files = files.Order(FileComparer).ToList();
            return files;
        }

        // Convert ID for extensions // DN
        private string ConvertId(string id, string name = "")
        {
            string suffix = Empty(name) ? "" : $".{name}";
            if (id.Contains(".")) { // id for extension, e.g. "chat.Foobar"
                var parts = id.Split('.');
                return $"global.{parts.ElementAt(0)}.phrase.{String.Join(".", parts.Skip(1))}" + suffix; // e.g. "global.chat.phrase.Foobar"
            }
            return $"global.phrase.{id.ToLowerInvariant()}" + suffix;
        }

        // Get phrase
        public string Phrase(string id, bool? useText = false)
        {
            if (Empty(id))
                return "";
            var attrs = PhraseAttrs(id);
            if (attrs == null)
                return id;
            attrs.TryGetValue("class", out object? phraseClass);
            if (!attrs.TryGetValue("value", out object? text))
                text = id; // Return the id if phrase not found
            string res = ConvertToString(text);
            string className = ConvertToString(phraseClass);
            if (useText != true && !Empty(className)) {
                if (useText == null && !Empty(text)) // Use both icon and text
                    className = AppendClass(className, "me-2");
                if (Regex.IsMatch(className, @"\bspinner\b")) // Spinner
                    res = "<div class=\"" + className + "\" role=\"status\"><span class=\"visually-hidden\">" + HtmlEncode(text) + "</span></div>";
                else // Icon
                    res = "<i data-phrase=\"" + id + "\" class=\"" + className + "\"><span class=\"visually-hidden\">" + HtmlEncode(text) + "</span></i>";
                if (useText == null && !Empty(text)) // Use both icon and text
                    res += ConvertToString(text);
            }
            return res;
        }

        // Set phrase
        public void SetPhrase(string id, string value) =>  SetPhraseAttr(id, "value", value);

        // Get data
        public string GetData(string id) => PhraseData.Get(id.ToLowerInvariant()) ?? "";

        // Set data
        public void SetData(string id, string value) => PhraseData.Set(id.ToLowerInvariant(), value);

        // Get project phrase
        public string ProjectPhrase(string id) => GetData($"project.phrase.{id}.value");

        // Set project phrase
        public void SetProjectPhrase(string id, string value) => SetData($"project.phrase.{id}.value", value);

        // Get menu phrase
        public string MenuPhrase(string menuId, string id) => GetData($"project.menu.{menuId}.phrase.{id}.value");

        // Set menu phrase
        public void SetMenuPhrase(string menuId, string id, string value) => SetData($"project.menu.{menuId}.phrase.{id}.value", value);

        // Get table phrase
        public string TablePhrase(string tblVar, string id) => GetData($"project.table.{tblVar}.phrase.{id}.value");

        // Set table phrase
        public void SetTablePhrase(string tblVar, string id, string value) => SetData($"project.table.{tblVar}.phrase.{id}.value", value);

        // Get field phrase
        public string FieldPhrase(string tblVar, string fldVar, string id) => GetData($"project.table.{tblVar}.field.{fldVar}.phrase.{id}.value");

        // Set field phrase
        public void SetFieldPhrase(string tblVar, string fldVar, string id, string value) => SetData($"project.table.{tblVar}.field.{fldVar}.phrase.{id}.value", value);

        // Get chart phrase // DN
        public string ChartPhrase(string tblVar, string fldVar, string id) => GetData($"project.table.{tblVar}.chart.{fldVar}.phrase.{id}.value");

        // Set chart phrase // DN
        public void SetChartPhrase(string tblVar, string fldVar, string id, string value) => SetData($"project.table.{tblVar}.chart.{fldVar}.phrase.{id}.value", value);

        // Get phrase attributes
        public dynamic? PhraseAttrs(string id) => PhraseData.Get(ConvertId(id));

        // Get phrase attribute
        public string PhraseAttr(string id, string name = "value") =>
            PhraseAttrs(id) is Dictionary<string, dynamic> d && d.TryGetValue(name, out dynamic? value) ? ConvertToString(value) : "";

        // Set phrase attribute
        public void SetPhraseAttr(string id, string name, string value) => SetData(ConvertId(id, name), value);

        // Get phrase class
        public string PhraseClass(string id) => PhraseAttr(id, "class");

        // Set phrase attribute
        public void SetPhraseClass(string id, string value) => SetPhraseAttr(id, "class", value);

        // Output phrases as JSON
        public async Task PhrasesToJson(dynamic phrases)
        {
            Dictionary<string, string> dict = ((Dictionary<string, dynamic>)phrases)
                .ToDictionary(kvp => kvp.Key, kvp => (string)kvp.Value["value"]);
            foreach (var (key, value) in dict) {
                await _writer.WritePropertyNameAsync(key);
                await _writer.WriteValueAsync(value);
            }
        }

        // Output dictionary as JSON
        public async Task DictinoaryToJson(dynamic value)
        {
            var dict = (Dictionary<string, dynamic>)(value);
            await _writer.WriteStartObjectAsync();
            foreach (var (key, val) in dict) {
                if (key == "phrase") {
                    await PhrasesToJson(val);
                } else {
                    await _writer.WritePropertyNameAsync(key);
                    await DictinoaryToJson(val);
                }
            }
            await _writer.WriteEndObjectAsync();
        }

        // Output phrases as Json (Async)
        public async Task<string> ToJsonAsync()
        {
            await DictinoaryToJson(PhraseData.Get("global")!);
            return "ew.language.phrases = " + _stringWriter.ToString() + ";";
        }

        // Output phrases as Json
        public string ToJson() => ToJsonAsync().GetAwaiter().GetResult();

        // Get language info
        public List<Dictionary<string, object>> GetLanguages()
        {
            List<Dictionary<string, object>> languages = new();
            if (Config.Languages.Count > 1) {
                string file = MapPath(LanguageFolder) + Config.LanguagesFile;
                DotAccessData data = new(XmlToDictionary(file));
                foreach (string langId in Config.Languages) {
                    Dictionary<string, object> lang = new() {
                        { "id", langId },
                        { "desc", data.Get("global.phrase." + langId.ToLowerInvariant() + ".desc", Phrase(langId)) },
                        { "selected", langId == CurrentLanguage }
                    };
                    languages.Add(lang);
                }
                if (LanguageComparer != null)
                    languages = languages.Order(LanguageComparer).ToList();
            }
            return languages;
        }

        // Get template
        public string GetTemplate() =>
            Type.ToUpper() switch {
                "LI" => // LI template (for used with top Navbar)
                    "{{for languages}}<li class=\"nav-item\"><a class=\"nav-link{{if selected}} active{{/if}} ew-tooltip\" title=\"{{>desc}}\" data-ew-action=\"language\" data-language=\"{{:id}}\">{{:id}}</a></li>{{/for}}",
                "DROPDOWN" => // DROPDOWN template (for used with top Navbar)
                    "<li class=\"nav-item dropdown\"><a class=\"nav-link\" data-bs-toggle=\"dropdown\"><i class=\"fa-solid fa-globe ew-icon\"></i></span></a><div class=\"dropdown-menu dropdown-menu-lg dropdown-menu-end\">{{for languages}}<a class=\"dropdown-item{{if selected}} active{{/if}}\" data-ew-action=\"language\" data-language=\"{{:id}}\">{{>desc}}</a>{{/for}}</div></li>",
                "SELECT" => // SELECT template (NOT for used with top Navbar)
                    "<div class=\"ew-language-option\"><select class=\"form-select\" id=\"ew-language\" name=\"ew-language\" data-ew-action=\"language\">{{for languages}}<option value=\"{{:id}}\"{{if selected}} selected{{/if}}>{{:desc}}</option>{{/for}}</select></div>",
                "RADIO" => // RADIO template (NOT for used with top Navbar)
                    "<div class=\"ew-language-option\"><div class=\"btn-group\" data-bs-toggle=\"buttons\">{{for languages}}<input type=\"radio\" name=\"ew-language\" id=\"ew-Language-{{:id}}\" autocomplete=\"off\" data-ew-action=\"language\"{{if selected}} checked{{/if}} value=\"{{:id}}\"><label class=\"btn btn-default ew-tooltip\" for=\"ew-language-{{:id}}\" data-container=\"body\" data-bs-placement=\"bottom\" title=\"{{>desc}}\">{{:id}}</label>{{/for}}</div></div>",
                _ => Template
            };

        // Language Load event
        public void LanguageLoad() {
            // Example:
            //SetPhrase("SaveBtn", "Save Me"); // Refer to language XML file for phrase IDs
        }
    }
} // End Partial class
