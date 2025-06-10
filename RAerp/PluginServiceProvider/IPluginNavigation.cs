using RAerp.Models.NavigationModel;

namespace RAerp.PluginServiceProvider
{
    public interface IPluginNavigation
    {
        Task<PluginNode> ManagePluginNode();
    }
}
