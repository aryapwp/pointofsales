namespace ASPNETMaker2024.Entities;

/**
 * Entity class for "so_order" table
 */
[Table("so_order")]
public class SoOrder
{
    [Key("SO_ORDER_ID")]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public long SoOrderId { get; set; } = default!;

    [Column("ORDER_NO")]
    public required string OrderNo { get; set; } = default!;

    [Column("ORDER_DATE")]
    public required DateTime OrderDate { get; set; } = default!;

    [Column("COM_CUSTOMER_ID")]
    public required int ComCustomerId { get; set; } = default!;

    [Column("ADDRESS")]
    public required string Address { get; set; } = default!;
}
