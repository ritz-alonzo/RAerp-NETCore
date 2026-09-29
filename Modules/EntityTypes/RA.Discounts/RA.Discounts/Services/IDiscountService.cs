using RA.Discounts.App_Data;
using RA.Discounts.Data;
using RA.Discounts.Domain;
using RA.EntityTypes.Services;
using RA.WebFramework.Models.Pagination;

namespace RA.Discounts.Services
{
    public interface IDiscountService : IEntityTypeService<Discount, DiscountSetting, RADiscountContext>
    {
        Task<IEnumerable<Discount>> GetDiscountListAsync(string searchQuery = null,
            List<int> discountTypeIds = null,
            List<int> discountScopeIds = null,
            DateTime? createdOn = null,
            Guid? createdById = null,
            bool showActiveDiscountsOnly = false);
        Task CreateDiscountRedemptionAsync(DiscountRedemption redemption);
        Task DeleteDiscountRedemptionAsync(DiscountRedemption redemption);
        Task<DiscountRedemption> GetDiscountRedemptionByIdAsync(Guid redemptionId);
        Task<IEnumerable<DiscountRedemption>> GetDiscountRedemptionListAsync(Guid? searchDiscountId = null, 
            Guid? searchOrderId = null,
            string searchOrderNbr = null, 
            string searchDiscountCode = null, 
            Guid? searchCustomerId = null, 
            int pageNumber = 0, int pageSize = 0);
        Task<IEnumerable<DiscountRedemption>> GetDiscountRedemptionListByOrderIdAsync(Guid orderId);
        Task<IEnumerable<DiscountRedemption>> GetPendingDiscountRedemptionListByOrderIdAsync(Guid orderId);
        Task UpdateDiscountRedemptionAsync(DiscountRedemption redemption);
        Task<PagedResult<DiscountRedemption>> GetDiscountRedemptionPagedResultListAsync(Guid? searchDiscountId = null,
            Guid? searchOrderId = null,
            string searchOrderNbr = null,
            string searchDiscountCode = null,
            Guid? searchCustomerId = null,
            int pageNumber = 0,
            int pageSize = int.MaxValue);
        Task<PagedResult<Discount>> GetDiscountPagedResultListAsync(string searchQuery = null,
            List<int> discountTypeIds = null,
            List<int> discountScopeIds = null,
            DateTime? createdOn = null,
            Guid? createdById = null,
            bool showActiveDiscountsOnly = false,
            int pageNumber = 0,
            int pageSize = int.MaxValue);
    }
}