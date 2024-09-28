namespace ASPNETMaker2024.Controllers;

// Partial class
public partial class HomeController : Controller
{
    // list
    [Route("ComCustomerList/{COM_CUSTOMER_ID?}", Name = "ComCustomerList-com_customer-list")]
    [Route("Home/ComCustomerList/{COM_CUSTOMER_ID?}", Name = "ComCustomerList-com_customer-list-2")]
    public async Task<IActionResult> ComCustomerList()
    {
        // Create page object
        comCustomerList = new GLOBALS.ComCustomerList(this);
        comCustomerList.Cache = _cache;

        // Run the page
        return await comCustomerList.Run();
    }

    // add
    [Route("ComCustomerAdd/{COM_CUSTOMER_ID?}", Name = "ComCustomerAdd-com_customer-add")]
    [Route("Home/ComCustomerAdd/{COM_CUSTOMER_ID?}", Name = "ComCustomerAdd-com_customer-add-2")]
    public async Task<IActionResult> ComCustomerAdd()
    {
        // Create page object
        comCustomerAdd = new GLOBALS.ComCustomerAdd(this);

        // Run the page
        return await comCustomerAdd.Run();
    }

    // view
    [Route("ComCustomerView/{COM_CUSTOMER_ID?}", Name = "ComCustomerView-com_customer-view")]
    [Route("Home/ComCustomerView/{COM_CUSTOMER_ID?}", Name = "ComCustomerView-com_customer-view-2")]
    public async Task<IActionResult> ComCustomerView()
    {
        // Create page object
        comCustomerView = new GLOBALS.ComCustomerView(this);

        // Run the page
        return await comCustomerView.Run();
    }

    // edit
    [Route("ComCustomerEdit/{COM_CUSTOMER_ID?}", Name = "ComCustomerEdit-com_customer-edit")]
    [Route("Home/ComCustomerEdit/{COM_CUSTOMER_ID?}", Name = "ComCustomerEdit-com_customer-edit-2")]
    public async Task<IActionResult> ComCustomerEdit()
    {
        // Create page object
        comCustomerEdit = new GLOBALS.ComCustomerEdit(this);

        // Run the page
        return await comCustomerEdit.Run();
    }

    // delete
    [Route("ComCustomerDelete/{COM_CUSTOMER_ID?}", Name = "ComCustomerDelete-com_customer-delete")]
    [Route("Home/ComCustomerDelete/{COM_CUSTOMER_ID?}", Name = "ComCustomerDelete-com_customer-delete-2")]
    public async Task<IActionResult> ComCustomerDelete()
    {
        // Create page object
        comCustomerDelete = new GLOBALS.ComCustomerDelete(this);

        // Run the page
        return await comCustomerDelete.Run();
    }
}
