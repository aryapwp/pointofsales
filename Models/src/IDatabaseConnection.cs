namespace ASPNETMaker2024.Models;

// Partial class
public partial class PointofSales {
    // IDatabaseConnection interface // DN
    public interface IDatabaseConnection
    {
        DbTransaction? Transaction { get; set; }
        DbTransaction? Transaction2 { get; set; }
        DbConnection DbConn { get; }
        DbConnection DbConn2 { get; }
        QueryFactory GetQueryFactory(bool main);
        QueryBuilder GetQueryBuilder(string table = "", bool main = false);
        int Execute(string sql, object? param = null, IDbTransaction? transaction = null, int? timeout = null, CommandType? commandType = null, bool main = false);
        Task<int> ExecuteAsync(string sql, object? param = null, IDbTransaction? transaction = null, int? timeout = null, CommandType? commandType = null, bool main = false);
        object? ExecuteScalar(string sql, object? param = null, IDbTransaction? transaction = null, int? timeout = null, CommandType? commandType = null, bool main = false);
        Task<object?> ExecuteScalarAsync(string sql, object? param = null, IDbTransaction? transaction = null, int? timeout = null, CommandType? commandType = null, bool main = false);
        T? ExecuteScalar<T>(string sql, object? param = null, IDbTransaction? transaction = null, int? timeout = null, CommandType? commandType = null, bool main = false);
        Task<T?> ExecuteScalarAsync<T>(string sql, object? param = null, IDbTransaction? transaction = null, int? timeout = null, CommandType? commandType = null, bool main = false);
    }
} // End Partial class
