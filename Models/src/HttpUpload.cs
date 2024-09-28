namespace ASPNETMaker2024.Models;

// Partial class
public partial class PointofSales {
    /// <summary>
    /// Upload class
    /// </summary>
    public class HttpUpload
    {
        public int Index = -1; // Index to handle multiple form elements

        public DbField Parent; // Parent field object

        public string UploadPath = ""; // Upload path

        public object DbValue = DbNullValue; // Value from database // DN

        public string Error = ""; // Error message

        public object Value = DbNullValue; // Upload value

        public string FileName = ""; // Upload file name

        public long FileSize; // Upload file size

        public string ContentType = ""; // File content type

        public int ImageWidth = -1; // Image width

        public int ImageHeight = -1; // Image height

        public bool UploadMultiple = false; // Multiple upload

        public bool KeepFile = true; // Keep old file

        public List<Action<MagickImage>> Plugins = new(); // Plugins for Resize()

        // Contructor
        public HttpUpload(DbField fld)
        {
            Parent = fld;
            UploadMultiple = fld.UploadMultiple; // DN
        }

        // Is empty
        public bool IsEmpty => DbValue == DbNullValue;

        // Last error message
        public string Message => Error;

        // Check the file type of the uploaded file
        private static bool UploadAllowedFileExtensions(string fileName) => CheckFileType(fileName);

        // Upload file
        public async Task<bool> UploadFile()
        {
            try {
                Value = DbNullValue; // Reset first
                var fldvar = (Index < 0) ? Parent.FieldVar : Parent.FieldVar.Substring(0, 1) + Index + Parent.FieldVar.Substring(1);

                // Get file from token or FormData for API request
                if (IsApi() && Post("addopt") != "1") {
                    await RenderUploadField(Parent, Index); // Set up old files
                    string oldFileName = FileName;
                    if (await GetUploadedFiles(Parent)) // Try to get from FormData / Token
                        KeepFile = false;
                    string wrkvar = "fn_" + fldvar;
                    if (Form.TryGetValue(wrkvar, out StringValues fileNames)) { // Get post back file names
                        if (Parent.DataType != DataType.Blob || Empty(fileNames)) // Non blob or delete file action
                            FileName = fileNames.ToString();
                    }
                    if (FileName != oldFileName) // File names changed
                        KeepFile = false;
                } else {
                    Error = ""; // Reset first
                    var wrkvar = "fn_" + fldvar;
                    FileName = Post(wrkvar); // Get file name
                    wrkvar = "fa_" + fldvar;
                    KeepFile = Post(wrkvar) == "1"; // Check if keep old file
                }
                if (!KeepFile && !Empty(FileName) && !UploadMultiple) {
                    string f = UploadTempPath(Parent) + FileName;
                    var fi = GetFileInfo(f);
                    if (fi.Exists) {
                        Value = await FileReadAllBytesAsync(f);
                        FileSize = fi.Length;
                        ContentType = ContentType((byte[])Value, f);
                        try {
                            var info = new MagickImageInfo(f);
                            ImageWidth = ConvertToInt(info.Width);
                            ImageHeight = ConvertToInt(info.Height);
                        } catch {}
                    }
                }
                return true;
            } catch (Exception e) {
                Error = e.Message;
                return false;
            }
        }

        /// <summary>
        /// Get uploaded files info
        /// </summary>
        /// <param name="fld">Field object. If unspecified, all uploaded files info is returned</param>
        /// <returns>Action result</returns>
        public static async Task<JsonBoolResult> GetUploadedFiles(DbField? fld = null)
        {
            // Validate request
            if (Files.Count <= 0)
                return new JsonBoolResult(false, "No uploaded files");

            // Language object
            Language = ResolveLanguage();

            // Create temp folder
            string filetoken = Random().ToString();
            string path = UploadTempPath(filetoken);
            string name = "";
            if (fld != null) {
                path = UploadTempPath(fld);
                name = fld.Name;
            }
            if (!CreateFolder(path))
                return new JsonBoolResult(false, "Create folder '" + path + "' failed");
            var files = new Dictionary<string, object>();
            CurrentForm ??= new();
            await CurrentForm.Init();

            // Move files to temp folder
            string fileName = "";
            List<Dictionary<string, object>> filelist = [];
            bool res = true;
            for (int i = 0; i < Files.Count; i++) {
                var file = Files[i];
                if (SameText(file.Name, name) || Empty(name)) {
                    string clientFileName = file.FileName;
                    if (fileName != "")
                        fileName += Config.MultipleUploadSeparator;
                    fileName += clientFileName;
                    long fileSize = file.Length;
                    Dictionary<string, object> d = new();
                    d.Add("name", clientFileName);
                    if (!UploadAllowedFileExtensions(clientFileName)) { // Check file extensions
                        d.Add("success", false);
                        d.Add("error", Language.Phrase("UploadErrorAcceptFileTypes"));
                        res = false;
                    } else if (Config.MaxFileSize > 0 && fileSize > Config.MaxFileSize) { // Check file size
                        d.Add("success", false);
                        d.Add("error", Language.Phrase("UploadErrorMaxFileSize"));
                        res = false;
                    } else {
                        d.Add("success", await CurrentForm.SaveUploadFile(file.Name, IncludeTrailingDelimiter(path, true) + clientFileName, i));
                    }
                    filelist.Add(d);
                }
            }

            // Process file token (uploaded in previous API file upload request)
            if (!Empty(name) && Post(name, out StringValues token)) {
                string tokenPath = UploadTempPath(token.ToString());
                try {
                    if (DirectoryExists(tokenPath)) {
                        // Get all files in the folder
                        var uploadedFiles = GetFiles(tokenPath);
                        foreach (string file in uploadedFiles) {
                            try {
                                if (FileExists(path + file)) // Delete old file first
                                    FileDelete(path + file);
                                FileMove(tokenPath + file, path + file); // Move to temp folder
                                if (fileName != "")
                                    fileName += Config.MultipleUploadSeparator;
                                fileName += file;
                                CleanUploadTempPath(tokenPath); // Clean up
                            } catch {
                                if (Config.Debug)
                                    throw;
                            }
                        }
                    }
                } catch {
                    if (Config.Debug)
                        throw;
                } finally {
                    Collect(); // DN
                }
                res = !Empty(fileName);
            }
            files.Add("importfiles", filelist);
            var result = new Dictionary<string, object>();
            if (res) {
                // FileName = fileName;
                result.Add(Config.ApiFileTokenName, filetoken);
            }
            result.Add("files", files);
            return new JsonBoolResult(result, res);
        }

        /// <summary>
        /// Resize image
        /// </summary>
        /// <param name="width">Target width of image</param>
        /// <param name="height">Target height of image</param>
        /// <returns>Whether file is resized successfully</returns>
        public bool Resize(int width, int height)
        {
            bool result = false;
            if (!IsNull(Value)) {
                int wrkWidth = width;
                int wrkHeight = height;
                byte[] data = (byte[])Value;
                result = ResizeBinary(ref data, ref wrkWidth, ref wrkHeight, Plugins);
                if (result) {
                    Value = data;
                    if (wrkWidth > 0 && wrkHeight > 0) {
                        ImageWidth = wrkWidth;
                        ImageHeight = wrkHeight;
                    }
                    FileSize = data.Length;
                }
            }
            return result;
        }

        /// <summary>
        /// Get file count
        /// </summary>
        /// <value>Uploaded file count</value>
        public int Count
        {
            get {
                if (!UploadMultiple && !Empty(Value)) {
                    return 1;
                } else if (UploadMultiple && FileName != "") {
                    return FileName.Split(Config.MultipleUploadSeparator).Length;
                }
                return 0;
            }
        }

        /// <summary>
        /// Get temp file path
        /// </summary>
        /// <param name="idx">Index of file</param>
        /// <returns>Temp file path of the uploaded file</returns>
        public string? GetTempFile(int idx = 0)
        {
            if (FileName != "") {
                if (UploadMultiple) {
                    var ar = FileName.Split(Config.MultipleUploadSeparator);
                    if (idx > -1 && idx < ar.Length)
                        return UploadTempPath(Parent, Index) + ar[idx];
                }
                return UploadTempPath(Parent, Index) + FileName;
            }
            return null;
        }

        /// <summary>
        /// Get temp file paths
        /// </summary>
        /// <returns>Temp file paths of all uploaded files</returns>
        public List<string>? GetTempFiles()
        {
            string fldvar = Parent.FieldVar;
            if (FileName != "") {
                if (UploadMultiple) {
                    var files = FileName.Split(Config.MultipleUploadSeparator);
                    return files.Select(fn => UploadTempPath(Parent) + fn).ToList();
                } else {
                    string file = UploadTempPath(Parent) + FileName;
                    return [file];
                }
            }
            return null;
        }

        /// <summary>
        /// Save uploaded data to file
        /// </summary>
        /// <param name="newFileName">New file name</param>
        /// <param name="overWrite">Overwrite file or not</param>
        /// <param name="idx">Index of file</param>
        /// <returns>Whether file is saved successfully</returns>
        public async Task<bool> SaveToFile(string newFileName, bool overWrite, int idx = 0)
        {
            string path = ServerMapPath(Empty(UploadPath) ? Parent.UploadPath : UploadPath);
            if (!IsNull(Value)) {
                if (Empty(newFileName))
                    newFileName = FileName;
                byte[] data = (byte[])Value;
                if (!overWrite)
                    newFileName = UploadFileName(path, newFileName);
                return await SaveFile(path, newFileName, data);
            } else if (idx >= 0) { // Use file from upload temp folder
                var file = GetTempFile(idx);
                if (!Empty(file) && FileExists(file)) {
                    if (!overWrite)
                        newFileName = UploadFileName(path, newFileName);
                    return IsRemote(path)
                        ? await SaveFile(path, newFileName, await FileReadAllBytesAsync(file))
                        : CopyFile(path, newFileName, file, overWrite); // DN
                }
            }
            return false;
        }

        /// <summary>
        /// Resize and save uploaded data to file
        /// </summary>
        /// <param name="width">Target width of image</param>
        /// <param name="height">Target height of image</param>
        /// <param name="newFileName">New file name</param>
        /// <param name="overwrite">Overwrite existing file or not</param>
        /// <param name="idx">Index of file</param>
        /// <returns>Whether file is resized and saved successfully</returns>
        public async Task<bool> ResizeAndSaveToFile(int width, int height, string newFileName, bool overwrite, int idx = 0)
        {
            var result = false;
            if (!IsNull(Value)) {
                // Save old values
                var oldValue = Value;
                var oldWidth = ImageWidth;
                var oldHeight = ImageHeight;
                var oldFileSize = FileSize;
                try {
                    Resize(width, height);
                    result = await SaveToFile(newFileName, overwrite);
                } finally { // Restore old values
                    Value = oldValue;
                    ImageWidth = oldWidth;
                    ImageHeight = oldHeight;
                    FileSize = oldFileSize;
                }
            } else if (idx >= 0) { // Use file from upload temp folder
                var file = GetTempFile(idx);
                if (file != null && FileExists(file)) {
                    Value = await FileReadAllBytesAsync(file);
                    Resize(width, height);
                    try {
                        result = await SaveToFile(newFileName, overwrite);
                    } finally {
                        Value = DbNullValue;
                    }
                }
            }
            return result;
        }
    }
} // End Partial class
