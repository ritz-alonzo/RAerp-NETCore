using RA.Core.Models.PluginModels.Inventory;

namespace RA.Inventory.Factories
{
    public interface IInventoryModelFactory
    {
        Task<InventoryConfigureModel> PrepareInventoryConfigureModelAsync(string systemName);
    }
}