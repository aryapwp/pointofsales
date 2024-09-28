namespace ASPNETMaker2024.Controllers;

// Partial class
public partial class HomeController : Controller
{
    // list
    [Route("SoOrderList/{SO_ORDER_ID?}", Name = "SoOrderList-so_order-list")]
    [Route("Home/SoOrderList/{SO_ORDER_ID?}", Name = "SoOrderList-so_order-list-2")]
    public async Task<IActionResult> SoOrderList()
    {
        // Create page object
        soOrderList = new GLOBALS.SoOrderList(this);
        soOrderList.Cache = _cache;

        // Run the page
        return await soOrderList.Run();
    }

    // add
    [Route("SoOrderAdd/{SO_ORDER_ID?}", Name = "SoOrderAdd-so_order-add")]
    [Route("Home/SoOrderAdd/{SO_ORDER_ID?}", Name = "SoOrderAdd-so_order-add-2")]
    public async Task<IActionResult> SoOrderAdd()
    {
        // Create page object
        soOrderAdd = new GLOBALS.SoOrderAdd(this);

        // Run the page
        return await soOrderAdd.Run();
    }

    // view
    [Route("SoOrderView/{SO_ORDER_ID?}", Name = "SoOrderView-so_order-view")]
    [Route("Home/SoOrderView/{SO_ORDER_ID?}", Name = "SoOrderView-so_order-view-2")]
    public async Task<IActionResult> SoOrderView()
    {
        // Create page object
        soOrderView = new GLOBALS.SoOrderView(this);

        // Run the page
        return await soOrderView.Run();
    }

    // edit
    [Route("SoOrderEdit/{SO_ORDER_ID?}", Name = "SoOrderEdit-so_order-edit")]
    [Route("Home/SoOrderEdit/{SO_ORDER_ID?}", Name = "SoOrderEdit-so_order-edit-2")]
    public async Task<IActionResult> SoOrderEdit()
    {
        // Create page object
        soOrderEdit = new GLOBALS.SoOrderEdit(this);

        // Run the page
        return await soOrderEdit.Run();
    }

    // delete
    [Route("SoOrderDelete/{SO_ORDER_ID?}", Name = "SoOrderDelete-so_order-delete")]
    [Route("Home/SoOrderDelete/{SO_ORDER_ID?}", Name = "SoOrderDelete-so_order-delete-2")]
    public async Task<IActionResult> SoOrderDelete()
    {
        // Create page object
        soOrderDelete = new GLOBALS.SoOrderDelete(this);

        // Run the page
        return await soOrderDelete.Run();
    }
}
