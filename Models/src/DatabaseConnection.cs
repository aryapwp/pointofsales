namespace ASPNETMaker2024.Models;

// Partial class
public partial class PointofSales {
    /// <summary>
    /// Database connection class
    /// </summary>
    /// <typeparam name="N">DbConnection</typeparam>
    /// <typeparam name="T">Field data type</typeparam>
    public class DatabaseConnection<N, T> : IDatabaseConnection, IDisposable
        where N : DbConnection
    {
        private bool _disposed = false;

        public string ConnectionString { get; set; } = "";

        public ConfigurationSection Info { get; set; }

        public string DbId { get; set; }

        private Lazy<N> _conn; // Main connection

        private Lazy<N> _conn2; // Secondary connection

        private Lazy<QueryFactory> _queryFactory; // Query factory for main connection

        private Lazy<QueryFactory> _queryFactory2; // Query factory for secondary connection

        private DbTransaction? _trans;

        private DbTransaction? _trans2;

        // Constructor
        public DatabaseConnection(string dbid)
        {
            Info = Db(dbid);
            DbId = Info["id"] ?? "";
            if (Empty(DbId))
                throw new Exception($"Database ID '{DbId}' not found.");
            _conn = new Lazy<N>(() => OpenConnection());
            _conn2 = new Lazy<N>(() => OpenConnection());
            _queryFactory = new Lazy<QueryFactory>(() => new QueryFactory {
                Connection = DbConn, // Main connection
                Compiler = GetCompiler(),
                Logger = compiled => Log(compiled.ToString()), // Log the compiled query
                QueryTimeout = Config.QueryTimeout
            });
            _queryFactory2 = new Lazy<QueryFactory>(() => new QueryFactory {
                Connection = DbConn2, // Secondary connection
                Compiler = GetCompiler(),
                Logger = compiled => Log(compiled.ToString()), // Log the compiled query
                QueryTimeout = Config.QueryTimeout
            });
        }

        // Constructor
        public DatabaseConnection() : this("DB") {}

        // Overrides Object.Finalize
        ~DatabaseConnection() => Dispose(false);

        // Main connection as IDbConnection
        public DbConnection DbConn => (DbConnection)_conn.Value;

        // Secondary connection as IDbConnection
        public DbConnection DbConn2 => (DbConnection)_conn2.Value;

        // Get connection string builder
        public DbConnectionStringBuilder GetConnectionStringBuilder(string? connectionString = null)
        {
            connectionString ??= Info["connectionstring"];
            if (IsMySql && connectionString != null)
                return new MySqlConnectionStringBuilder(connectionString);
            return new DbConnectionStringBuilder();
        }

        // Get connection string builder
        public DbConnectionStringBuilder GetConnectionStringBuilder(ConfigurationSection info) => GetConnectionStringBuilder(info["connectionstring"]);

        // Query builder compiler
        public Compiler GetCompiler()
        {
            if (IsMySql)
                return new CustomMySqlCompiler();
            else if (IsMsSql2012)
                return new CustomSqlServerCompiler();
            else if (IsMsSql)
                return new CustomSqlServerCompiler { UseLegacyPagination = true };
            else if (IsOracle)
                return new CustomOracleCompiler();
            else if (IsPostgreSql)
                return new CustomPostgresCompiler();
            else if (IsSqlite)
                return new CustomSqliteCompiler();
            throw new Exception("Database type not supported.");
        }

        // Get query factory
        public QueryFactory GetQueryFactory(bool main = false) => main ? _queryFactory.Value : _queryFactory2.Value;

        // Get query builder
        public QueryBuilder GetQueryBuilder(string table = "", bool main = false)
        {
            var factory = GetQueryFactory(main);
            return (table != "") ? factory.Query(table) : factory.Query();
        }

        // Get data type
        public T GetDataType(object dt) => (T)dt;

        // Microsoft SQL Server
        public bool IsMsSql => typeof(N).ToString().Contains(".SqlConnection");

        // Microsoft SQL Server >= 2012
        public bool IsMsSql2012;

        // MySQL
        public bool IsMySql => typeof(N).ToString().Contains(".MySqlConnection");

        // PostgreSQL
        public bool IsPostgreSql => typeof(N).ToString().Contains(".NpgsqlConnection");

        // Oracle
        public bool IsOracle => typeof(N).ToString().Contains(".OracleConnection");

        // Oracle >= 12.1
        public bool IsOracle12;

        // Sqlite
        public bool IsSqlite => typeof(N).ToString().Contains(".SqliteConnection");

        /// <summary>
        /// Select limit SQL
        /// </summary>
        /// <param name="sql">SQL to execute</param>
        /// <param name="nrows">Number of rows</param>
        /// <param name="offset">Offset</param>
        /// <param name="hasOrderBy">SQL has ORDER BY clause or now</param>
        /// <returns>SQL</returns>
        public string SelectLimitSql(string sql, int nrows = -1, int offset = -1, bool hasOrderBy = false)
        {
            string offsetPart, limitPart, originalSql = sql;
            if (IsMsSql) {
                if (IsMsSql2012) { // Microsoft SQL Server >= 2012
                    if (!hasOrderBy)
                        sql += " ORDER BY @@version"; // Dummy ORDER BY clause
                    if (offset > -1)
                        sql += " OFFSET " + ConvertToString(offset) + " ROWS";
                    if (nrows > 0) {
                        if (offset < 0)
                            sql += " OFFSET 0 ROWS";
                        sql += " FETCH NEXT " + ConvertToString(nrows) + " ROWS ONLY";
                    }
                } else { // Select top
                    if (nrows > 0) {
                        if (offset > 0)
                            nrows += offset;
                        sql = Regex.Replace(sql, @"(^\s*SELECT\s+(DISTINCT)?)", @"$1 TOP " + ConvertToString(nrows) + " ", RegexOptions.IgnoreCase); // DN
                    }
                }
            } else if (IsMySql) {
                offsetPart = (offset >= 0) ? ConvertToString(offset) + "," : "";
                limitPart = (nrows < 0) ? "18446744073709551615" : ConvertToString(nrows);
                sql += " LIMIT " + offsetPart + limitPart;
            } else if (IsPostgreSql || IsSqlite) {
                offsetPart = (offset >= 0) ? " OFFSET " + ConvertToString(offset) : "";
                limitPart = (nrows >= 0) ? " LIMIT " + ConvertToString(nrows) : "";
                sql += limitPart + offsetPart;
            } else if (IsOracle) { // Select top
                if (IsOracle12) { // Oracle >= 12.1
                    if (offset > -1)
                        sql += " OFFSET " + ConvertToString(offset) + " ROWS";
                    if (nrows > 0)
                        sql += " FETCH NEXT " + ConvertToString(nrows) + " ROWS ONLY";
                } else {
                    if (nrows > 0) {
                        if (offset > 0)
                            nrows += offset;
                        sql = "SELECT * FROM (" + sql + ") WHERE ROWNUM <= " + ConvertToString(nrows);
                    }
                }
            }
            return sql;
        }

        // Supports select offset
        public bool SelectOffset => IsMySql || IsPostgreSql || IsMsSql2012 || IsOracle12 || IsSqlite;

        /// <summary>
        /// Execute a command
        /// </summary>
        /// <param name="sql">The SQL to execute for this query</param>
        /// <param name="param">The parameters to use for this command</param>
        /// <param name="transaction">The transaction to use for this query</param>
        /// <param name="timeout">Number of seconds before command execution timeout</param>
        /// <param name="commandType">Is it a stored proc or a batch?</param>
        /// <param name="main">Whether to use the main connection</param>
        /// <returns>The number of rows affected</returns>
        public int Execute(string sql, object? param = null, IDbTransaction? transaction = null, int? timeout = null, CommandType? commandType = null, bool main = false) =>
            main // Main connection
            ? DbConn.Execute(LogSql(sql), param, transaction ?? _trans, timeout, commandType)
            : DbConn2.Execute(LogSql(sql), param, transaction ?? _trans2, timeout, commandType);

        /// <summary>
        /// Execute a command (async)
        /// </summary>
        /// <param name="sql">The SQL to execute for this query</param>
        /// <param name="param">The parameters to use for this command</param>
        /// <param name="transaction">The transaction to use for this query</param>
        /// <param name="timeout">Number of seconds before command execution timeout</param>
        /// <param name="commandType">Is it a stored proc or a batch?</param>
        /// <param name="main">Whether to use the main connection</param>
        /// <returns>The number of rows affected</returns>
        public async Task<int> ExecuteAsync(string sql, object? param = null, IDbTransaction? transaction = null, int? timeout = null, CommandType? commandType = null, bool main = false) =>
            main // Main connection
            ? await DbConn.ExecuteAsync(LogSql(sql), param, transaction ?? _trans, timeout, commandType)
            : await DbConn2.ExecuteAsync(LogSql(sql), param, transaction ?? _trans2, timeout, commandType);

        /// <summary>
        /// Execute parameterized SQL that selects a single value
        /// </summary>
        /// <param name="sql">The SQL to execute</param>
        /// <param name="param">The parameters to use for this command</param>
        /// <param name="transaction">The transaction to use for this command</param>
        /// <param name="timeout">Number of seconds before command execution timeout</param>
        /// <param name="commandType">Is it a stored proc or a batch?</param>
        /// <param name="main">Whether to use the main connection</param>
        /// <returns>The first cell selected as <see cref="object"/></returns>
        public object? ExecuteScalar(string sql, object? param = null, IDbTransaction? transaction = null, int? timeout = null, CommandType? commandType = null, bool main = false) =>
            main // Main connection
            ? DbConn.ExecuteScalar(LogSql(sql), param, transaction ?? _trans, timeout, commandType)
            : DbConn2.ExecuteScalar(LogSql(sql), param, transaction ?? _trans2, timeout, commandType);

        /// <summary>
        /// Execute parameterized SQL that selects a single value (async)
        /// </summary>
        /// <param name="sql">The SQL to execute</param>
        /// <param name="param">The parameters to use for this command</param>
        /// <param name="transaction">The transaction to use for this command</param>
        /// <param name="timeout">Number of seconds before command execution timeout</param>
        /// <param name="commandType">Is it a stored proc or a batch?</param>
        /// <param name="main">Whether to use the main connection</param>
        /// <returns>The first cell returned, as <see cref="object"/></returns>
        public async Task<object?> ExecuteScalarAsync(string sql, object? param = null, IDbTransaction? transaction = null, int? timeout = null, CommandType? commandType = null, bool main = false) =>
            main // Main connection
            ? await DbConn.ExecuteScalarAsync(LogSql(sql), param, transaction ?? _trans, timeout, commandType)
            : await DbConn2.ExecuteScalarAsync(LogSql(sql), param, transaction ?? _trans2, timeout, commandType);

        /// <summary>
        /// Execute parameterized SQL that selects a single value
        /// </summary>
        /// <typeparam name="TValue">The type to return.</typeparam>
        /// <param name="sql">The SQL to execute</param>
        /// <param name="param">The parameters to use for this command</param>
        /// <param name="transaction">The transaction to use for this command</param>
        /// <param name="timeout">Number of seconds before command execution timeout</param>
        /// <param name="commandType">Is it a stored proc or a batch?</param>
        /// <param name="main">Whether to use the main connection</param>
        /// <returns>The first cell selected as <typeparamref name="TValue"/></returns>
        public TValue? ExecuteScalar<TValue>(string sql, object? param = null, IDbTransaction? transaction = null, int? timeout = null, CommandType? commandType = null, bool main = false) =>
            main // Main connection
            ? DbConn.ExecuteScalar<TValue>(LogSql(sql), param, transaction ?? _trans, timeout, commandType)
            : DbConn2.ExecuteScalar<TValue>(LogSql(sql), param, transaction ?? _trans2, timeout, commandType);

        /// <summary>
        /// Execute parameterized SQL that selects a single value (async)
        /// </summary>
        /// <typeparam name="TValue">The type to return.</typeparam>
        /// <param name="sql">The SQL to execute</param>
        /// <param name="param">The parameters to use for this command</param>
        /// <param name="transaction">The transaction to use for this command</param>
        /// <param name="timeout">Number of seconds before command execution timeout</param>
        /// <param name="commandType">Is it a stored proc or a batch?</param>
        /// <param name="main">Whether to use the main connection</param>
        /// <returns>The first cell returned, as <typeparamref name="TValue"/></returns>
        public async Task<TValue?> ExecuteScalarAsync<TValue>(string sql, object? param = null, IDbTransaction? transaction = null, int? timeout = null, CommandType? commandType = null, bool main = false) =>
            main // Main connection
            ? await DbConn.ExecuteScalarAsync<TValue>(LogSql(sql), param, transaction ?? _trans, timeout, commandType)
            : await DbConn2.ExecuteScalarAsync<TValue>(LogSql(sql), param, transaction ?? _trans2, timeout, commandType);

        /// <summary>
        /// Executes the query, and returns the row(s) as JSON (async)
        /// </summary>
        /// <param name="sql">SQL to execute</param>
        /// <param name="options">
        /// Options: (Dictionary or Anonymous Type)
        /// <list type="bullet">
        /// <item><term>firstOnly (bool)</term><description>Returns the first row only</description></item>
        /// <item><term>array (bool)</term><description>Returns the result as array</description></item>
        /// <item><term>convertDate (bool)</term><description>Convert date by JavaScriptDateTimeConverter</description></item>
        /// </list>
        /// </param>
        /// <param name="main">Whether to use the main connection</param>
        /// <param name="timeout">Number of seconds before command execution timeout</param>
        /// <param name="commandType">Is it a stored proc or a batch?</param>
        /// <returns>Tasks that returns JSON string</returns>
        public async Task<string> ExecuteJsonAsync(string sql, dynamic? options = null, bool main = false, int? timeout = null, CommandType? commandType = null)
        {
            IDictionary<string, object> opts = ConvertToDictionary<object>(options);
            bool firstOnly = opts.TryGetValue("firstOnly", out object? first) && ConvertToBool(first);
            bool array = opts.TryGetValue("array", out object? ar) && ConvertToBool(ar);
            bool convertDate = opts.TryGetValue("convertDate", out object? cd) && ConvertToBool(cd);
            Newtonsoft.Json.Converters.JavaScriptDateTimeConverter? convertor = convertDate ? new() : null;
            if (firstOnly) {
                var row = await GetRowAsync(sql, main, null, timeout, commandType);
                if (convertor != null)
                    return (array) ? ConvertToJson(row?.Values, convertor) : ConvertToJson(row, convertor);
                else
                    return (array) ? ConvertToJson(row?.Values) : ConvertToJson(row);
            } else {
                var rows = await GetRowsAsync(sql, main, null, timeout, commandType);
                if (convertor != null)
                    return (array) ? ConvertToJson(rows?.Select(d => d.Values), convertor) : ConvertToJson(rows, convertor);
                else
                    return (array) ? ConvertToJson(rows?.Select(d => d.Values)) : ConvertToJson(rows);
            }
        }

        /// <summary>
        /// Executes the query, and returns the row(s) as JSON
        /// </summary>
        /// <param name="sql">SQL to execute</param>
        /// <param name="options">
        /// Options: (Dictionary or Anonymous Type)
        /// <list type="bullet">
        /// <item><term>firstOnly (bool)</term><description>Returns the first row only</description></item>
        /// <item><term>array (bool)</term><description>Returns the result as array</description></item>
        /// <item><term>convertDate (bool)</term><description>Convert date by JavaScriptDateTimeConverter</description></item>
        /// </list>
        /// </param>
        /// <param name="main">Whether to use the main connection</param>
        /// <param name="timeout">Number of seconds before command execution timeout</param>
        /// <param name="commandType">Is it a stored proc or a batch?</param>
        /// <returns>JSON string</returns>
        public string ExecuteJson(string sql, dynamic? options = null, bool main = false, int? timeout = null, CommandType? commandType = null) =>
            ExecuteJsonAsync(sql, options, main, timeout, commandType).GetAwaiter().GetResult();

        // Get data reader
        public DbDataReader ExecuteReader(string sql, object? param = null, IDbTransaction? transaction = null, int? timeout = null, CommandType? commandType = null, bool main = true) =>
            (DbDataReader)(main // Main connection
                ? DbConn.ExecuteReader(LogSql(sql), param, transaction ?? _trans, timeout, commandType)
                : DbConn2.ExecuteReader(LogSql(sql), param, transaction ?? _trans2, timeout, commandType));

        // Get data reader
        public async Task<DbDataReader> ExecuteReaderAsync(string sql, object? param = null, IDbTransaction? transaction = null, int? timeout = null, CommandType? commandType = null, bool main = true) =>
            (DbDataReader)(main // Main connection
                ? await DbConn.ExecuteReaderAsync(LogSql(sql), param, transaction ?? _trans, timeout, commandType)
                : await DbConn2.ExecuteReaderAsync(LogSql(sql), param, transaction ?? _trans2, timeout, commandType));

        // Get a new connection
        public virtual async Task<N> OpenConnectionAsync()
        {
            var connstr = GetConnectionString(Info);
            string dbid = ConvertToString(Info["id"]);
            if (IsSqlite) {
                var relpath = Info["relpath"];
                var dbname = Info["dbname"];
                if (Empty(relpath)) {
                    connstr += AppRoot() + dbname;
                } else if (Path.IsPathRooted(relpath)) { // Physical path
                    connstr += relpath + dbname;
                } else { // Relative to wwwroot
                    connstr += ServerMapPath(relpath) + dbname;
                }
                connstr = connstr.Replace("\\", "/");
                ConnectionString = connstr;
            }
            DatabaseConnecting(ref connstr);
            var connectingArgs = new EventArgs<DbConnectionStringBuilder>(GetConnectionStringBuilder(connstr));
            DatabaseConnectingEventHandler?.Invoke(this, connectingArgs);
            connstr = connectingArgs.Value.ConnectionString;
            var c = Activator.CreateInstance(typeof(N), [connstr]) is N n
                ? n
                : throw new Exception($"Failed to create connection for database '{dbid}'");
            await c.OpenAsync();
            string timezone = Info["timezone"] ?? Config.DbTimeZone;
            if (IsOracle) {
                if (!Empty(Info["schema"]))
                    await c.ExecuteAsync("ALTER SESSION SET CURRENT_SCHEMA = " + QuotedName(Info["schema"]!, dbid)); // Set current schema
                await c.ExecuteAsync("ALTER SESSION SET NLS_TIMESTAMP_FORMAT = 'yyyy-mm-dd hh24:mi:ss'");
                await c.ExecuteAsync("ALTER SESSION SET NLS_TIMESTAMP_TZ_FORMAT = 'yyyy-mm-dd hh24:mi:ss'");
                if (timezone != "")
                    await c.ExecuteAsync("ALTER SESSION SET TIME_ZONE = '" + timezone + "'");
                var m = Regex.Match(c.ExecuteScalar<string>("SELECT * FROM v$version WHERE banner LIKE 'Oracle%'")!, @"Release (\d+\.\d+)");
                IsOracle12 = m.Success && Convert.ToDouble(m.Groups[1].Value) >= 12.1;
            } else if (IsMsSql) {
                var m = Regex.Match(c.ExecuteScalar<string>("SELECT @@version")!, @"(\d+)\.(\d+)\.(\d+)(\.(\d+))");
                IsMsSql2012 = m.Success && ConvertToInt(m.Groups[1].Value) >= 11;
            } else if (IsPostgreSql) {
                if (!Empty(Info["schema"]))
                    await c.ExecuteAsync("SET search_path TO " + QuotedName(Info["schema"]!, dbid)); // Set current schema
                if (timezone != "")
                    await c.ExecuteAsync("SET TIME ZONE '" + timezone + "'");
            } else if (IsMySql && timezone != "") {
                await c.ExecuteAsync("SET time_zone = '" + timezone + "'");
            }
            DatabaseConnected(c);
            DatabaseConnectedEventHandler?.Invoke(this, new EventArgs<DbConnection>(c));
            return c;
        }

        // Get a new connection
        public virtual N OpenConnection() => OpenConnectionAsync().GetAwaiter().GetResult();

        /// <summary>
        /// Get a row by SQL
        /// </summary>
        /// <param name="sql">SQL to execute</param>
        /// <param name="main">Use main connection or not</param>
        /// <param name="param">Paramters for SQL</param>
        /// <param name="timeout">Number of seconds before command execution timeout</param>
        /// <param name="commandType">Is it a stored proc or a batch?</param>
        /// <returns>Row as dictionary</returns>
        public Dictionary<string, object>? GetRow(string sql, bool main = false, object? param = null, int? timeout = null, CommandType? commandType = null)
        {
            var row = main
                ? DbConn.QueryFirstOrDefault(LogSql(sql), param, _trans, timeout, commandType)
                : DbConn2.QueryFirstOrDefault(LogSql(sql), param, _trans2, timeout, commandType);
            return (row != null) ? new Dictionary<string, object>((IDictionary<string, object>)row) : null;
        }

        /// <summary>
        /// Get a row by SQL (async)
        /// </summary>
        /// <param name="sql">SQL to execute</param>
        /// <param name="main">Use main connection or not</param>
        /// <param name="param">Paramters for SQL</param>
        /// <param name="timeout">Number of seconds before command execution timeout</param>
        /// <param name="commandType">Is it a stored proc or a batch?</param>
        /// <returns>Task that returns row as dictionary</returns>
        public async Task<Dictionary<string, object>?> GetRowAsync(string sql, bool main = false, object? param = null, int? timeout = null, CommandType? commandType = null)
        {
            var row = main
                ? await DbConn.QueryFirstOrDefaultAsync(LogSql(sql), param, _trans, timeout, commandType)
                : await DbConn2.QueryFirstOrDefaultAsync(LogSql(sql), param, _trans2, timeout, commandType);
            return (row != null) ? new Dictionary<string, object>((IDictionary<string, object>)row) : null;
        }

        /// <summary>
        /// Get rows by SQL
        /// </summary>
        /// <param name="sql">SQL to execute</param>
        /// <param name="main">Use main connection or not</param>
        /// <param name="param">Paramters for SQL</param>
        /// <param name="timeout">Number of seconds before command execution timeout</param>
        /// <param name="commandType">Is it a stored proc or a batch?</param>
        /// <returns>Rows as list of dictionary</returns>
        public List<Dictionary<string, object>> GetRows(string sql, bool main = false, object? param = null, int? timeout = null, CommandType? commandType = null)
            => (main
                ? DbConn.Query(LogSql(sql), param, _trans, commandTimeout: timeout, commandType: commandType)
                : DbConn2.Query(LogSql(sql), param, _trans2, commandTimeout: timeout, commandType: commandType))
                    .Select(row => (Dictionary<string, object>)ConvertToDictionary<object>(row)).ToList();

        /// <summary>
        /// Get rows by SQL (async)
        /// </summary>
        /// <param name="sql">SQL to execute</param>
        /// <param name="main">Use main connection or not</param>
        /// <param name="param">Paramters for SQL</param>
        /// <param name="timeout">Number of seconds before command execution timeout</param>
        /// <param name="commandType">Is it a stored proc or a batch?</param>
        /// <returns>Task that returns rows as list of dictionary</returns>
        public async Task<List<Dictionary<string, object>>> GetRowsAsync(string sql, bool main = false, object? param = null, int? timeout = null, CommandType? commandType = null)
            => (await (main
                ? DbConn.QueryAsync(LogSql(sql), param, _trans, timeout, commandType)
                : DbConn2.QueryAsync(LogSql(sql), param, _trans2, timeout, commandType)))
                    .Select(row => (Dictionary<string, object>)ConvertToDictionary<object>(row)).ToList();

        // Get count (by dataset)
        public int GetCount(string sql)
        {
            int cnt = 0;
            using var dr = ExecuteReader(sql, main: false);
            if (dr != null) {
                while (dr.Read())
                    cnt++;
            }
            return cnt;
        }

        // Get transaction
        public DbTransaction? Transaction
        {
            get => _trans;
            set => _trans = value;
        }

        // Get transaction
        public DbTransaction? Transaction2
        {
            get => _trans2;
            set => _trans2 = value;
        }

        // Begin transaction
        public DbTransaction BeginTrans(bool main = true)
        {
            if (main) {
                _trans = (IsOracle || IsSqlite)
                    ? DbConn.BeginTransaction()
                    : DbConn.BeginTransaction(IsolationLevel.ReadUncommitted);
                return _trans;
            } else {
                _trans2 = (IsOracle || IsSqlite)
                    ? DbConn2.BeginTransaction()
                    : DbConn2.BeginTransaction(IsolationLevel.ReadUncommitted);
                return _trans2;
            }
        }

        // Commit transaction
        public void CommitTrans(bool main = true)
        {
            try {
                (main ? _trans : _trans2)?.Commit();
            } catch {
                if (Config.Debug)
                    throw;
            }
        }

        // Rollback transaction
        public void RollbackTrans(bool main = true)
        {
            try {
                (main ? _trans : _trans2)?.Rollback();
            } catch {
                if (Config.Debug)
                    throw;
            }
        }

        // Concat // DN
        public string Concat(params string[] list)
        {
            if (IsMsSql) {
                return String.Join(" + ", list);
            } else if (IsOracle || IsPostgreSql) {
                return String.Join(" || ", list);
            }
            return "CONCAT(" + String.Join(", ", list) + ")";
        }

        // Table CSS class name
        public string TableClass { get; set; } = "table table-sm ew-db-table";

        /// <summary>
        /// Get result in HTML table by SQL (async)
        /// </summary>
        /// <param name="sql">SQL to execute</param>
        /// <param name="options">
        /// Options: (Dictionary or Anonymous Type)
        /// <list type="bullet">
        /// <item><term>fieldcaption (bool|Dictionary)</term><description>true (Use caption and use language object), or false (Use field names directly), or Dictionary of fieid caption for looking up field caption by field name</description></item>
        /// <item><term>horizontal (bool)</term><description>Whether HTML table is horizontal, default: false</description></item>
        /// <item><term>tablename (string|List&lt;string&gt;)</term><description>Table name(s) for the language object</description></item>
        /// <item><term>tableclass (string)</term><description>CSS class names of the table, default: "table table-bordered ew-db-table"</description></item>
        /// </list>
        /// </param>
        /// <param name="timeout">Number of seconds before command execution timeout</param>
        /// <param name="commandType">Is it a stored proc or a batch?</param>
        /// <returns>Task that returns HTML string as IHtmlContent</returns>
        public async Task<IHtmlContent> ExecuteHtmlAsync(string sql, dynamic? options = null, int? timeout = null, CommandType? commandType = null)
        {
            var rows = await GetRowsAsync(sql, false, null, timeout, commandType);
            if (rows.Count() == 0)
                return HtmlString.Empty;
            IDictionary<string, object> opts = ConvertToDictionary<object>(options);
            bool horizontal = opts.TryGetValue("horizontal", out object? horiz) && ConvertToBool(horiz);
            string classname = opts.TryGetValue("tableclass", out object? tc) && !Empty(tc) ? (string)tc : TableClass;
            bool hasTableName = opts.TryGetValue("tablename", out object? tablename) && tablename != null;
            bool useFieldCaption = opts.TryGetValue("fieldcaption", out object? caps) && caps != null;
            var captions = (useFieldCaption && caps is not bool) ? ConvertToDictionary<string>(caps) : null;
            string html = "", vhtml = "";
            int cnt = 0;
            foreach (var row in rows) {
                if (cnt == 0) { // First row
                    // Vertical table
                    vhtml = "<table class=\"" + classname + "\"><tbody>";
                    foreach (var fld in row)
                        vhtml += "<tr><td>" + GetFieldCaption(fld.Key) + "</td><td>" + ConvertToString(fld.Value) + "</td></tr>";
                    vhtml += "</tbody></table>";
                    // Horizontal table
                    html = "<table class=\"" + classname + "\">";
                    html += "<thead><tr>";
                    foreach (var key in row.Keys)
                        html += "<th>" + GetFieldCaption(key) + "</th>";
                    html += "</tr></thead>";
                    html += "<tbody>";
                }
                cnt++;
                html += "<tr>";
                foreach (var val in row.Values)
                    html += "<td>" + ConvertToString(val) + "</td>";
                html += "</tr>";
            }
            if (html != "")
                html += "</tbody></table>";
            var str = (cnt > 1 || horizontal) ? html : vhtml;
            return new HtmlString(str);

            // Local function to get field caption
            string GetFieldCaption(string key) {
                string? caption = "";
                if (useFieldCaption) {
                    if (captions != null) {
                        captions.TryGetValue(key, out caption);
                    } else if (hasTableName && !Empty(Language)) {
                        caption = tablename switch {
                            IEnumerable<string> names => names.Select(name => Language.FieldPhrase(name, key, "FldCaption")).First(caption => !Empty(caption)),
                            string str => Language.FieldPhrase(str, key, "FldCaption"),
                            _ => ""
                        };
                    }
                }
                return !Empty(caption) ? caption : key;
            }
        }

        /// <summary>
        /// Get result in HTML table by SQL
        /// </summary>
        /// <param name="sql">SQL to execute</param>
        /// <param name="options">
        /// Options: (Dictionary or Anonymous Type)
        /// <list type="bullet">
        /// <item><term>fieldcaption (bool|Dictionary)</term><description>true (Use caption and use language object), or false (Use field names directly), or Dictionary of fieid caption for looking up field caption by field name</description></item>
        /// <item><term>horizontal (bool)</term><description>Whether HTML table is horizontal, default: false</description></item>
        /// <item><term>tablename (string|List&lt;string&gt;)</term><description>Table name(s) for the language object</description></item>
        /// <item><term>tableclass (string)</term><description>CSS class names of the table, default: "table table-bordered ew-db-table"</description></item>
        /// </list>
        /// </param>
        /// <param name="timeout">Number of seconds before command execution timeout</param>
        /// <param name="commandType">Is it a stored proc or a batch?</param>
        /// <returns>HTML string as IHtmlContent</returns>
        public IHtmlContent ExecuteHtml(string sql, dynamic? options = null, int? timeout = null, CommandType? commandType = null)
            => ExecuteHtmlAsync(sql, options, timeout, commandType).GetAwaiter().GetResult();

        // Close
        public void Close()
        {
            if (_conn.IsValueCreated) {
                using (_trans) {} // Dispose
                using (_conn.Value) {} // Dispose
            }
            if (_conn2.IsValueCreated) {
                using (_trans2) {} // Dispose
                using (_conn2.Value) {} // Dispose
            }
        }

        // Releases all resources used by this object
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        // Custom Dispose method to clean up unmanaged resources
        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
                return;
            if (disposing)
                Close();
            _disposed = true;
        }

        #pragma warning disable 162
        // Get connecton string from connection info
        public string GetConnectionString(ConfigurationSection Info)
        {
            string connectionString = Info["connectionstring"] ?? "";
            string uid = Info["username"] ?? "";
            string pwd = Info["password"] ?? "";
            string dbname = Info["dbname"] ?? "";
            if (Config.EncryptUserNameAndPassword) {
                uid = AesDecrypt(uid);
                pwd = AesDecrypt(pwd);
            }
            return connectionString.Replace("{uid}", uid).Replace("{pwd}", pwd).Replace("{dbname}", dbname);
        }
        #pragma warning restore 162

        // Database Connecting event
        public void DatabaseConnecting(ref string connstr) {
            //if (Info["id"] == "DB" && IsLocal()) { // Testing on local PC
            //    connstr = "...";
            //}
        }

        // Database Connected event
        public void DatabaseConnected(DbConnection conn) {
            // Example:
            //conn.Execute("Your SQL");
        }
    }
} // End Partial class
