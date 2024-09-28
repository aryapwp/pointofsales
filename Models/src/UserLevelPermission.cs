namespace ASPNETMaker2024.Models;

// Partial class
public partial class PointofSales {
    /// <summary>
    /// User level permission class
    /// </summary>
    public class UserLevelPermission
    {
        // Table name
        [Column("")]
        public string Table { set; get; } = "";

        // User level ID
        [Column("")]
        public int Id { set; get; }

        // Permission
        [Column("")]
        public int Permission { set; get; } = 0;
    }
} // End Partial class
