namespace ASPNETMaker2024.Models;

// Partial class
public partial class PointofSales {
    /// <summary>
    /// File Viewer class
    /// </summary>
    public class FileViewer
    {
        // Constructor
        public FileViewer(Controller controller) { // DN
            Controller = controller;
        }

        // Constructor
        public FileViewer() { // DN
        }

        /// <summary>
        /// Output file by file path // DN
        /// </summary>
        /// <returns>Action result</returns>
        public async Task<IActionResult> GetFile(string fn)
        {
            // Get parameters
            string sessionId = Get("session");
            sessionId = Decrypt(sessionId);
            if (UseSession && sessionId != Session.SessionId) {
                return JsonBoolResult.FalseResult;
            }
            bool resize = Get<bool>("resize");
            int width = Get<int>("width");
            int height = Get<int>("height");
            bool download = Get("download", out StringValues d) ? ConvertToBool(d) : true; // Download by default
            if (width == 0 && height == 0 && resize) {
                width = Config.ThumbnailDefaultWidth;
                height = Config.ThumbnailDefaultHeight;
            }

            // If using session (internal request), file path is always encrypted.
            // If not (external request), DO NOT support external request for file path.
            string key = sessionId + Config.RandomKey;
            fn = UseSession ? Decrypt(fn, key) : "";
            if (FileExists(fn)) {
                Response?.Clear();
                string ext = Path.GetExtension(fn).Replace(".", "").ToLower();
                var data = await FileReadAllBytesAsync(fn);
                string ct = ContentType(data);
                if (Config.ImageAllowedFileExtensions.Contains(ext, StringComparer.OrdinalIgnoreCase)) {
                    if (width > 0 || height > 0)
                        ResizeBinary(ref data, ref width, ref height);
                    return Controller.File(data, ct, download ? Path.GetFileName(fn) : null);
                } else if (Config.DownloadAllowedFileExtensions.Contains(ext, StringComparer.OrdinalIgnoreCase)) {
                    bool isPdf = SameText(ext, "pdf");
                    bool downloadPdf = !Config.EmbedPdf && Config.DownloadPdfFile;
                    download = download && !(isPdf && !downloadPdf); // Skip header if embed/inline PDF
                    return Controller.File(data, ct, download ? Path.GetFileName(fn) : null);
                }
            }
            return JsonBoolResult.FalseResult;
        }

        /// <summary>
        /// Output file by table name and file name // DN
        /// </summary>
        /// <returns>Action result</returns>
        public async Task<IActionResult> GetFile(string table, string fn)
        {
            // Get parameters
            //string sessionId = Get("session");
            bool resize = Get<bool>("resize");
            int width = Get<int>("width");
            int height = Get<int>("height");
            bool download = Get("download", out StringValues d) ? ConvertToBool(d) : true; // Download by default
            if (width == 0 && height == 0 && resize) {
                width = Config.ThumbnailDefaultWidth;
                height = Config.ThumbnailDefaultHeight;
            }

            // Get table object
            string tableName = "";
            dynamic? tbl = null;
            if (!Empty(table)) {
                tbl = Resolve(table);
                tableName = tbl?.Name ?? "";
            }

            // API request with table/fn
            if (tableName != "") {
                fn = Decrypt(fn, Config.RandomKey);
            } else {
                fn = "";
            }

            // Check file
            if (FileExists(fn)) {
                Response?.Clear();
                string ext = Path.GetExtension(fn).Replace(".", "").ToLower();
                var data = await FileReadAllBytesAsync(fn);
                string ct = ContentType(data);
                if (Config.ImageAllowedFileExtensions.Contains(ext, StringComparer.OrdinalIgnoreCase)) {
                    if (width > 0 || height > 0)
                        ResizeBinary(ref data, ref width, ref height);
                    return Controller.File(data, ct, download ? Path.GetFileName(fn) : null);
                } else if (Config.DownloadAllowedFileExtensions.Contains(ext, StringComparer.OrdinalIgnoreCase)) {
                    return Controller.File(data, ct, download ? Path.GetFileName(fn) : null);
                }
            }
            return JsonBoolResult.FalseResult;
        }

        /// <summary>
        /// Output file by table name, field name and primary key
        /// </summary>
        /// <returns>Action result</returns>
        public async Task<IActionResult> GetFile(string table, string field, string[] keys)
        {
            // Get parameters
            //string sessionId = Get("session");
            bool resize = Get<bool>("resize");
            int width = Get<int>("width");
            int height = Get<int>("height");
            // bool download = Get("download", out StringValues d) ? ConvertToBool(d) : true; // Download by default
            if (width == 0 && height == 0 && resize) {
                width = Config.ThumbnailDefaultWidth;
                height = Config.ThumbnailDefaultHeight;
            }

            // Get table object
            string tableName = "";
            dynamic? tbl = null;
            if (!Empty(table)) {
                tbl = Resolve(table);
                tableName = tbl?.Name ?? "";
            }
            if (Empty(tableName) || Empty(field) || keys.Length == 0)
                return JsonBoolResult.FalseResult;
            return await tbl?.GetFileData(field, keys, resize, width, height) ?? new EmptyResult();
        }
    }
} // End Partial class
