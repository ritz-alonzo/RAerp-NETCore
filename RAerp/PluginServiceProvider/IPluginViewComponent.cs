using Microsoft.AspNetCore.Mvc.ModelBinding;
using RA.Core.Models.BaseModels;
using RA.Core.Models.PortableViewModels;

namespace RAerp.PluginServiceProvider
{
    public interface IPluginViewComponent
    {
        List<PluginViewComponentModel> ManagePluginViewComponent();
    }
}
