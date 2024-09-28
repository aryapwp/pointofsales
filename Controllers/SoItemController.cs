namespace ASPNETMaker2024.Controllers;

// Partial class
public partial class HomeController : Controller
{
    // list
    [Route("SoItemList/{SO_ITEM_ID?}", Name = "SoItemList-so_item-list")]
    [Route("Home/SoItemList/{SO_ITEM_ID?}", Name = "SoItemList-so_item-list-2")]
    public async Task<IActionResult> SoItemList()
    {
        // Create page object
        soItemList = new GLOBALS.SoItemList(this);
        soItemList.Cache = _cache;

        // Run the page
        return await soItemList.Run();
    }

    // add
    [Route("SoItemAdd/{SO_ITEM_ID?}", Name = "SoItemAdd-so_item-add")]
    [Route("Home/SoItemAdd/{SO_ITEM_ID?}", Name = "SoItemAdd-so_item-add-2")]
    public async Task<IActionResult> SoItemAdd()
    {
        // Create page object
        soItemAdd = new GLOBALS.SoItemAdd(this);

        // Run the page
        return await soItemAdd.Run();
    }

    // view
    [Route("SoItemView/{SO_ITEM_ID?}", Name = "SoItemView-so_item-view")]
    [Route("Home/SoItemView/{SO_ITEM_ID?}", Name = "SoItemView-so_item-view-2")]
    public async Task<IActionResult> SoItemView()
    {
        // Create page object
        soItemView = new GLOBALS.SoItemView(this);

        // Run the page
        return await soItemView.Run();
    }

    // edit
    [Route("SoItemEdit/{SO_ITEM_ID?}", Name = "SoItemEdit-so_item-edit")]
    [Route("Home/SoItemEdit/{SO_ITEM_ID?}", Name = "SoItemEdit-so_item-edit-2")]
    public async Task<IActionResult> SoItemEdit()
    {
        // Create page object
        soItemEdit = new GLOBALS.SoItemEdit(this);

        // Run the page
        return await soItemEdit.Run();
    }

    // delete
    [Route("SoItemDelete/{SO_ITEM_ID?}", Name = "SoItemDelete-so_item-delete")]
    [Route("Home/SoItemDelete/{SO_ITEM_ID?}", Name = "SoItemDelete-so_item-delete-2")]
    public async Task<IActionResult> SoItemDelete()
    {
        // Create page object
        soItemDelete = new GLOBALS.SoItemDelete(this);

        // Run the page
        return await soItemDelete.Run();
    }
}
