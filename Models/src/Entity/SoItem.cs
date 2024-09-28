namespace ASPNETMaker2024.Entities;

/**
 * Entity class for "so_item" table
 */
[Table("so_item")]
public class SoItem
{
    [Key("SO_ITEM_ID")]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public long SoItemId { get; set; } = default!;

    [Column("SO_ORDER_ID")]
    public required long SoOrderId { get; set; } = default!;

    [Column("ITEM_NAME")]
    public required string ItemName { get; set; } = default!;

    [Column("QUANTITY")]
    public required int Quantity { get; set; } = default!;

    [Column("PRICE")]
    public required double Price { get; set; } = default!;
}
