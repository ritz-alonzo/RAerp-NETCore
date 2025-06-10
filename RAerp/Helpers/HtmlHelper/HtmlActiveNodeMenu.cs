using RAerp.Models.NavigationModel;

namespace RAerp.Helpers.HtmlHelper
{
    public static class HtmlActiveNodeMenu
    {
        // to get the current request path
        // Ex. /EntityTypes/ChildEntityTypeList/d4d5d28a-b183-4dae-2e0d-08dbf87d632f
        //var test = Context.Request.Path;
        // Ex. 0 - EntityTypes, 1 - ChildEntityTypeList, 2 - d4d5d28a-b183-4dae-2e0d-08dbf87d632f
        //var testing = Context.Request.RouteValues.Select(c => c.Value.ToString()).ToList();
        /// <summary>
        /// Sets the current selected plugin menu in Sidebar
        /// </summary>
        /// <param name="context"></param>
        /// <param name="systemName"></param>
        /// <returns></returns>
        public static void SetActivePluginMenu(HttpContext context, string systemName)
        {
            if (!string.IsNullOrEmpty(systemName))
            {
                context.Items.Add("ActiveMenu", systemName);
            }
        }
        /// <summary>
        /// Gets the current selected plugin menu in Sidebar
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public static string GetActivePluginMenu(HttpContext context)
        {
            var selectMenuSystemName = "";

            var contextItems = context.Items;

            if (contextItems.Any(c => c.Key.ToString() == "ActiveMenu"))
            {
                selectMenuSystemName = contextItems.Where(c => c.Key.ToString() == "ActiveMenu").Select(c => c.Value.ToString()).FirstOrDefault();
            }

            return selectMenuSystemName;
        }

        public static string GetActivePluginMenuController(HttpContext context)
        {
            var activeControllerMenu = "";

            var routeValueController = context.Request.RouteValues.Where(c => c.Key.ToString() == "controller").Select(c => c.Value.ToString()).FirstOrDefault();
            if (!string.IsNullOrEmpty(routeValueController)) 
                activeControllerMenu = routeValueController;

            return activeControllerMenu;
        }
    }
}

