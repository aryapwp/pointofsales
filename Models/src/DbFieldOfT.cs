namespace ASPNETMaker2024.Models;

// Partial class
public partial class PointofSales {
    /// <summary>
    /// Field class of T
    /// /// </summary>
    /// <typeparam name="T">Field data type</typeparam>
    public class DbField<T> : DbField
    {
        public readonly T DbType;

        public DbField(object table, string fieldVar, int type, T dbType) : base(table, fieldVar, type)
        {
            DbType = dbType;
        }
    }
} // End Partial class
