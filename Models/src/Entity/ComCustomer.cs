namespace ASPNETMaker2024.Entities;

/**
 * Entity class for "com_customer" table
 */
[Table("com_customer")]
public class ComCustomer
{
    [Key("COM_CUSTOMER_ID")]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int ComCustomerId { get; set; } = default!;

    [Column("CUSTOMER_NAME")]
    public string? CustomerName { get; set; }
}
