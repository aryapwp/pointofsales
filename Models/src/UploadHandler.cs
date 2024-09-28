namespace ASPNETMaker2024.Models;

// Partial class
public partial class PointofSales {
    /// <summary>
    /// Page class
    /// </summary>
    public class UploadHandler
    {
        public string UploadTable = "";

        public string SessionId = "";

        public string SessionIdEncrypted = "";

        // Page terminated // DN
        private bool _terminated = false;

        // Page class constructor
        public UploadHandler(Controller controller) { // DN
            Controller = controller;
        }

        // Page class constructor
        public UploadHandler() { // DN
        }

        // Download file content
        public async Task<IActionResult> DownloadFileContent()
        {
            string name = Get("id");
            UploadTable = Get("table");
            string filename = Get(name);
            string folder = UploadTempPath(name, UploadTable);
            string version = Get("version");
            if (!Empty(version))
                folder = PathCombine(folder, version, true);

            // Show file content (Config.ImageAllowedFileExtensions and Config.DownloadAllowedFileExtensions only)
            var ar = Config.ImageAllowedFileExtensions.Concat(Config.DownloadAllowedFileExtensions).ToArray();
            string file = IncludeTrailingDelimiter(folder, true) + filename;
            if (Regex.IsMatch(filename.ToLower(), @"\.(" + String.Join("|", ar) + ")$")) {
                if (FileExists(file)) {
                    var value = await FileReadAllBytesAsync(file);
                    string contentType = ContentType(value, filename);
                    if (Regex.IsMatch(filename.ToLower(), @"\.pdf$") && Config.EmbedPdf)
                        return Controller.File(value, contentType);
                    else
                        return Controller.File(value, contentType, filename);
                }
            }
            return new EmptyResult();
        }

        // Delete file
        public IActionResult Delete()
        {
            string name = Get("id");
            if (name != "") {
                UploadTable = Get("table");
                string filename = Get(name);
                string folder = UploadTempPath(name, UploadTable);
                DeleteFile(IncludeTrailingDelimiter(folder, true) + filename);
                string version = Config.UploadThumbnailFolder;
                folder = PathCombine(folder, version, true);
                DeleteFile(IncludeTrailingDelimiter(folder, true) + filename);
                return Controller.Json(new { success = true });
            }
            return new EmptyResult();
        }

        // Download file list
        public async Task<IActionResult> DownloadFileList()
        {
            string name = Get("id");
            string token = Get<string>(Config.TokenName) ?? "";
            UploadTable = Get("table");
            List<object[]> files = [];
            if (name != "") {
                string folder = UploadTempPath(name, UploadTable);
                if (DirectoryExists(folder)) {
                    var ar = GetFiles(folder);
                    foreach (var file in ar) {
                        var value = await FileReadAllBytesAsync(file);
                        var filesize = value.Length;
                        string filetype = ContentType(value, file);
                        files.Add([name, file, filetype, filesize, token]);
                    }
                }
                return OutputJson(name, files);
            }
            return new EmptyResult();
        }

        // Upload file
        public async Task<IActionResult> Upload()
        {
            if (Files.Count > 0) { // DN
                Language = ResolveLanguage();
                HttpForm form = new();
                await form.Init();
                string name = form.GetValue("id");
                UploadTable = form.GetValue("table");
                string folder = UploadTempPath(name, UploadTable);
                string token = Post<string>(Config.TokenName) ?? "";
                string exts = form.GetValue("exts");
                List<string> extList = exts.Split(',').ToList();
                if (!Empty(Config.UploadAllowedFileExtensions)) {
                    var allowedExtList = Config.UploadAllowedFileExtensions.Split(',');
                    exts = String.Join(",", extList.Where(ext => allowedExtList.Contains(ext, StringComparer.OrdinalIgnoreCase))); // Make sure exts is a subset of Config.UploadAllowedFileExtensions
                    if (Empty(exts))
                        exts = Config.UploadAllowedFileExtensions;
                }
                if (Empty(exts))
                    exts = @"\w+";
                string filetypes = @"\.(" + exts.Replace(",", "|") + ")$";
                int maxsize = form.GetInt("maxsize");
                int maxfilecount = form.GetInt("maxfilecount");
                string filename = form.GetUploadFileName(name);

                // Skip if no file uploaded
                if (Empty(filename))
                    return Controller.BadRequest(Language.Phrase("MissingFileName"));
                if (Config.UploadConvertAccentedChars) {
                    filename = HtmlEncode(filename);
                    filename = Regex.Replace(filename, @"&([a-zA-Z])(uml|acute|grave|circ|tilde|cedil);", "$1");
                    filename = HtmlDecode(filename);
                }
                string filetype = form.GetUploadFileContentType(name);
                long filesize = form.GetUploadFileSize(name);
                byte[] value = await form.GetUploadFileData(name);

                // Check file types
                if (!Regex.IsMatch(filename, filetypes, RegexOptions.IgnoreCase)) {
                    string fileerror = Language.Phrase("UploadErrorAcceptFileTypes");
                    return OutputJson("files", [ [name, filename, filetype, filesize, token, fileerror] ]);
                }

                // Check file size
                if (maxsize > 0 && maxsize < filesize) {
                    string fileerror = Language.Phrase("UploadErrorMaxFileSize");
                    return OutputJson("files", [ [name, filename, filetype, filesize, token, fileerror] ]);
                }

                // Check max file count
                int filecount = FolderFileCount(folder);
                if (maxfilecount > 0 && maxfilecount <= filecount) {
                    string fileerror = Language.Phrase("UploadErrorMaxNumberOfFiles");
                    return OutputJson("files", [ [name, filename, filetype, filesize, token, fileerror] ]);
                }

                // Delete all files in directory if replace
                string version = Config.UploadThumbnailFolder;
                if (form.GetBool("replace"))
                    CleanPath(folder, false);
                await SaveFile(folder, filename, value);
                folder = PathCombine(folder, version, true);
                int w = Config.UploadThumbnailWidth;
                int h = Config.UploadThumbnailHeight;
                ResizeBinary(ref value, ref w, ref h);
                await SaveFile(folder, filename, value);
                return OutputJson("files", [ [name, Path.Join(folder, filename), filetype, filesize, token] ]);
            }
            return new EmptyResult();
        }

        // Output JSON
        public IActionResult OutputJson(string id, List<object[]> files)
        {
            List<Dictionary<string, object>> list = [];
            if (IsList(files)) {
                foreach (var file in files) {
                    if (file.Length >= 5) {
                        string name = ConvertToString(file[0]);
                        string fileName = Path.GetFileName(ConvertToString(file[1])); // Full path file in file[1] // DN
                        string fileError = (file.Length > 5) ? ConvertToString(file[5]) : "";
                        string version = Config.UploadThumbnailFolder;
                        string baseurl = FullUrl(CurrentPageName(), "upload");
                        string table = (UploadTable != "") ? "&table=" + UploadTable : "";
                        string session = "&session=" + SessionIdEncrypted;
                        string url = baseurl + "?id=" + name + table + session + "&" + name + "=" + RawUrlEncode(fileName) + "&download=1";
                        string thumbnail_url = baseurl + "?id=" + name + table + session + "&" + name + "=" + RawUrlEncode(fileName) + "&version=" + version + "&download=1";
                        string delete_url = baseurl + "?id=" + name + table + session + "&" + name + "=" + RawUrlEncode(fileName) + "&delete=1";
                        // if (ConvertToString(file[4]) is string token && !Empty(token)) {
                        //     url += "&" + Config.TokenName + "=" + token;
                        //     thumbnail_url += "&" + Config.TokenName + "=" + token;
                        // }
                        string ext = Path.GetExtension(fileName).Replace(".", "").ToLower();
                        if (!Config.ImageAllowedFileExtensions.Contains(ext, StringComparer.OrdinalIgnoreCase)) // Non image files
                            thumbnail_url = "";
                        var obj = new Dictionary<string, object>() {
                            { "name", fileName },
                            { "extension", Path.GetExtension(fileName).Replace(".", "") },
                            { "size", ConvertToInt(file[3]) },
                            { "type", ConvertToString(file[2]) },
                            { "url", url },
                            { "deleteUrl", delete_url },
                            { "deleteType", "GET" } // Use GET
                        };
                        if (!Empty(fileError)) {
                            obj.Add("error", fileError);
                        } else {
                            obj.Add(version + "Url", thumbnail_url);
                            if (Config.ImageAllowedFileExtensions.Contains(ext, StringComparer.OrdinalIgnoreCase)) { // Image files
                                try {
                                    IEnumerable<MetadataExtractor.Directory> directories = MetadataExtractor.ImageMetadataReader.ReadMetadata(ConvertToString(file[1]));
                                    obj.Add("exists", !directories.Any(directory => directory.Name == "IPTC" && directory.Tags.Any(tag => tag.Description == "FileNotFound")));
                                } catch {}
                            } else { // Non image files
                                obj.Add("exists", true);
                            }
                        }
                        list.Add(obj);
                    }
                }
            }

            // Set file header / content type
            AddHeader(HeaderNames.ContentDisposition, "inline; filename=files.json");

            // Output json
            var dict = new Dictionary<string, List<Dictionary<string, object>>> { { id, list } };
            return Controller.Json(dict); // Returns utf-8 data
        }

        /// <summary>
        /// Page run
        /// </summary>
        /// <returns>Page result</returns>
        [HttpCacheExpiration(CacheLocation = CacheLocation.Private, NoStore = true, MaxAge = 0)]
        public async Task<IActionResult> Run() { // DN
            SessionIdEncrypted = Param("session");
            SessionId = Decrypt(SessionIdEncrypted);
            if (EmptyString(SessionIdEncrypted) || EmptyString(SessionId))
                return new EmptyResult();
            // Handle download file content
            if (Get("download") != "") {
                return await DownloadFileContent();
            } else if (Get("delete") != "") { // Handle delete file
                return Delete();
            } else if (Get("id") != "") { // Handle download file list
                return await DownloadFileList();
            } else if (Files.Count > 0) { // Handle upload file (multi-part)
                return await Upload();
            }
            return new EmptyResult();
        }

        // Terminate page
        public IActionResult Terminate(string url = "") { // DN
            if (_terminated)
                return new EmptyResult();
            _terminated = true;
            return new EmptyResult();
        }
    }
} // End Partial class
