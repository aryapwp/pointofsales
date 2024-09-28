namespace ASPNETMaker2024.Models;

// Partial class
public partial class PointofSales {
    // Configuration
    public static partial class Config
    {
        //
        // ASP.NET Maker 2024 user level settings
        //

        // User level info
        public static List<UserLevel> UserLevels = [
            new() { Id = -2, Name = "Anonymous" }
        ];

        // User level priv info
        public static List<UserLevelPermission> UserLevelPermissions = [
            new() { Table = "{C2008EF4-FC4B-4028-B6F7-41627F994A94}com_customer", Id = -2, Permission = 0 },
            new() { Table = "{C2008EF4-FC4B-4028-B6F7-41627F994A94}so_item", Id = -2, Permission = 0 },
            new() { Table = "{C2008EF4-FC4B-4028-B6F7-41627F994A94}so_order", Id = -2, Permission = 0 }
        ];

        // User level table info // DN
        public static List<UserLevelTablePermission> UserLevelTablePermissions = [
            new() { TableName = "com_customer", TableVar = "com_customer", Caption = "com customer", Allowed = true, ProjectId = "{C2008EF4-FC4B-4028-B6F7-41627F994A94}", Url = "ComCustomerList" },
            new() { TableName = "so_item", TableVar = "so_item", Caption = "so item", Allowed = true, ProjectId = "{C2008EF4-FC4B-4028-B6F7-41627F994A94}", Url = "SoItemList" },
            new() { TableName = "so_order", TableVar = "so_order", Caption = "so order", Allowed = true, ProjectId = "{C2008EF4-FC4B-4028-B6F7-41627F994A94}", Url = "SoOrderList" }
        ];
    }
} // End Partial class
