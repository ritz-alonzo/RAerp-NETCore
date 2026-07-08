using RA.Core.Models.PluginModels.Discounts;

namespace RA.Discounts.Factories
{
    public interface IDiscountModelFactory
    {
        Task<DiscountConfigureModel> PrepareDiscountConfigureModelAsync(Guid entityTypeId, string systemName);
    }
}