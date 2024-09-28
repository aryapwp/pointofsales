namespace ASPNETMaker2024.Models;

// Partial class
public partial class PointofSales {
    /// <summary>
    /// Menu class
    /// </summary>
    public class Menu
    {
        public object Id;

        public bool IsRoot;

        public bool IsNavbar;

        public bool Accordion = true; // For sidebar menu only

        public bool Compact = false; // For sidebar menu only

        public List<MenuItem> Items = [];

        public bool UseSubmenu;

        public int Level = 0;

        private bool? _renderMenu = null;

        private bool? _isOpened = null;

        // Constructor
        public Menu(object id, bool isRoot = false, bool isNavbar = false, string? languageFolder = null)
        {
            Id = id;
            IsRoot = isRoot;
            IsNavbar = isNavbar;
            if (isNavbar) {
                UseSubmenu = true;
                Accordion = false;
            }
            Language = ResolveLanguage();
        }

        // Add a menu item
        public void AddMenuItem(MenuItem item)
        {
            MenuItemAdding(item);
            MenuItemAddingEventHandler?.Invoke(this, new EventArgs<MenuItem>(item));
            if (!item.Allowed)
                return;
            if (item.ParentId < 0)
                AddItem(item);
            else if (FindItem(item.ParentId, out MenuItem? parentMenu))
                parentMenu?.AddItem(item);
        }

        // Add a menu item
        public void AddMenuItem(int id, string name, string text, string url, int parentId = -1, bool allowed = true, bool isHeader = false, bool isCustomUrl = false, string icon = "", string label = "", bool isNavbarItem = false, bool isSidebarItem = false) =>
            AddMenuItem(new MenuItem(this, id, name, text, url, parentId, allowed, isHeader, isCustomUrl, icon, label, isNavbarItem, isSidebarItem));

        // Add menu items // DN
        public void AddMenuItems(List<MenuItem> items) => items.ForEach(item => AddMenuItem(item.SetMenu(this)));

        public string Phrase(string menuId, string id) => Language.MenuPhrase(menuId, id);

        // Get menu item count
        public int Count => Items.Count;

        // Add item
        public void AddItem(MenuItem item)
        {
            item.Level = Level;
            item.Menu = this;
            Items.Add(item);
        }

        // Clear all menu items
        public void Clear() => Items.Clear();

        // Find item
        public bool FindItem(int id, out MenuItem? outitem)
        {
            outitem = null;
            foreach (var item in Items) {
                if (item.Id == id) {
                    outitem = item;
                    return true;
                } else if (item.Submenu != null) {
                    if (item.Submenu.FindItem(id, out outitem))
                        return true;
                }
            }
            return false;
        }

        // Find item by menu text
        public bool FindItemByText(string txt, out MenuItem? outitem)
        {
            outitem = null;
            foreach (var item in Items) {
                if (item.Text == txt) {
                    outitem = item;
                    return true;
                } else if (item.Submenu != null) {
                    if (item.Submenu.FindItemByText(txt, out outitem))
                        return true;
                }
            }
            return false;
        }

        // Move item to position
        public void MoveItem(string text, int pos)
        {
            int oldpos = 0;
            int newpos = pos;
            pos = pos < 0 ? 0 : pos > Items.Count ? Items.Count : pos;
            if (Items.Find(item => SameString(item.Text, text)) is MenuItem currentItem) {
                oldpos = Items.IndexOf(currentItem);
                if (pos != oldpos) {
                    Items.RemoveAt(oldpos); // Remove old item
                    if (oldpos < pos)
                        newpos--; // Adjust new position
                    Items.Insert(newpos, currentItem); // Insert new item
                }
            }
        }

        // Check if this menu should be rendered
        public bool RenderMenu
        {
            get {
                _renderMenu ??= Items.Any(item => item.CanRender);
                return _renderMenu.Value;
            }
        }

        // Check if this menu should be opened
        public bool IsOpened
        {
            get {
                _isOpened ??= Items.Any(item => item.IsOpened);
                return _isOpened.Value;
            }
        }

        // Render the menu as JSON // DN
        public virtual async Task<string> ToJson()
        {
            MenuRendering();
            MenuRenderingEventHandler?.Invoke(this, EventArgs.Empty);
            if (!RenderMenu || Count == 0)
                return "null";
            // Write JSON
            var sw = new StringWriter();
            using var writer = new JsonTextWriter(sw);
            if (IsRoot) {
                await writer.WriteStartObjectAsync();
                await writer.WritePropertyNameAsync("accordion");
                await writer.WriteValueAsync(Accordion);
                await writer.WritePropertyNameAsync("compact");
                await writer.WriteValueAsync(Compact);
                await writer.WritePropertyNameAsync("items");
            }
            await writer.WriteStartArrayAsync();
            foreach (MenuItem item in Items) {
                if (item.CanRender) {
                    if (item.IsHeader && (!IsRoot || !UseSubmenu)) { // Group title (Header)
                        await writer.WriteRawValueAsync(await item.ToJson(false));
                        if (item.Submenu != null) {
                            foreach (MenuItem subitem in item.Submenu.Items) {
                                if (subitem.CanRender)
                                    await writer.WriteRawValueAsync(await subitem.ToJson());
                            }
                        }
                    } else {
                        await writer.WriteRawValueAsync(await item.ToJson());
                    }
                }
            }
            await writer.WriteEndArrayAsync();
            if (IsRoot)
                await writer.WriteEndObjectAsync();
            MenuRendered();
            MenuRenderedEventHandler?.Invoke(this, EventArgs.Empty);
            return sw.ToString();
        }

        // Returns the menu as script tag
        public async Task<string> ToScript() => "<script>ew.vars." + Id + " = " + await ToJson() + ";</script>";

        public void MenuItemAdding(MenuItem item) {
            //VarDump(item);
            //item.Allowed = false; // Set to false if menu item not allowed
        }

        public void MenuRendering() {
            // Change menu items here
        }

        public void MenuRendered() {
            // Change menu here
        }
    }
} // End Partial class
