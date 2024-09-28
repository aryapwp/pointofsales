namespace ASPNETMaker2024.Models;

// Partial class
public partial class PointofSales {
    /// <summary>
    /// IQueryFactory interface
    /// </summary>
    interface IQueryFactory
    {
        // Get query factory
        QueryFactory GetQueryFactory(bool main = false);

        // Get query builder
        QueryBuilder GetQueryBuilder(bool main = false);
    }
} // End Partial class
