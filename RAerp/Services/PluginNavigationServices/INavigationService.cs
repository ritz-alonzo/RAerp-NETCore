using RAerp.Models.NavigationModel;

namespace RAerp.Services.PluginNavigationServices
{ 
    public interface INavigationService
    {
        Task<List<PluginNode>> GetAdminNodesMenu(HttpContext httpContext);
        Task<List<PluginNode>> GetPluginNodesMenu();
    }
}