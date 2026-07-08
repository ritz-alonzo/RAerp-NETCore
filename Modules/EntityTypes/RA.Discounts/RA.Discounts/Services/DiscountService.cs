using Microsoft.EntityFrameworkCore;
using RA.Core.DataCaching.CacheManagement;
using RA.Data.App_Data;
using RA.Data.Domain.EntityTypes;
using RA.Discounts.App_Data;
using RA.Discounts.Data;
using RA.Discounts.Domain;
using RA.EntityTypes.Services;
using RA.WebFramework.Extensions;
using RA.WebFramework.Models.Pagination;

namespace RA.Discounts.Services
{
    public class DiscountService : EntityTypeService<Discount, DiscountSetting, RADiscountContext>, IDiscountService
    {
        #region Constants
        private readonly RADiscountContext _context;
        private readonly IEntityTypeManager _entityTypeManager;
        private readonly ICacheManager<Discount> _discCacheManager;
        private readonly ICacheManager<DiscountRedemption> _redemptionCacheManager;
        private readonly EntityType _discountEntityType;
        #endregion

        #region Ctor
        public DiscountService(RADiscountContext context,
            ICacheManager<Discount> discCacheManager,
            ICacheManager<DiscountRedemption> redemptionCacheManager,
            IEntityTypeManager entityTypeManager)
            : base(context, discCacheManager, entityTypeManager)
        {
            _context = context;
            _entityTypeManager = entityTypeManager;
            _discCacheManager = discCacheManager;
            _redemptionCacheManager = redemptionCacheManager;
            _discountEntityType = _entityTypeManager.GetTypeBySystemNameAsync(typeof(Discount).FullName).Result;
        }
        #endregion

        #region Discount
        public async Task<IEnumerable<Discount>> GetDiscountListAsync(string searchQuery = null,
            List<int> discountTypeIds = null,
            List<int> discountScopeIds = null,
            DateTime? createdOn = null,
            Guid? createdById = null,
            bool showActiveDiscountsOnly = false)
        {
            var query = await GetListAsync(_discountEntityType.Id);

            if (!string.IsNullOrEmpty(searchQuery))
                query = query.Where(c => c.Code.ToLower() == searchQuery.ToLower() ||
                                         c.Name.ToLower() == searchQuery.ToLower());

            if (discountTypeIds.HasAny())
                query = query.Where(c => discountTypeIds.Contains(c.DiscountTypeId));

            if (discountScopeIds.HasAny())
                query = query.Where(c => discountScopeIds.Contains(c.DiscountScopeId));

            if (createdOn.HasValue)
                query = query.Where(c => c.CreatedOn.Date == createdOn.Value);

            if (createdById.IsNotNullOrEmpty())
                query = query.Where(c => c.CreatedById ==  createdById.Value);

            return query.ToList();
        }
        #endregion

        #region Discount Redemption
        public async Task<IEnumerable<DiscountRedemption>> GetDiscountRedemptionListAsync(Guid? searchDiscountId = null,
            Guid? searchOrderId = null,
            string searchDiscountCode = null,
            Guid? searchCustomerId = null,
            int pageNumber = 0,
            int pageSize = 0)
        {
            var query = _redemptionCacheManager.EntityCacheNotExists(typeof(DiscountRedemption).FullName) ?
                await _redemptionCacheManager.GenerateCacheAsync(await _context.DiscountRedemption.ToListAsync(), typeof(DiscountRedemption).FullName)
                : _redemptionCacheManager.GetEntityCacheData(typeof(DiscountRedemption).FullName);
            
            if (searchDiscountId.IsNotNullOrEmpty())
                query = query.Where(c => c.DiscountId == searchDiscountId.Value);

            if (searchOrderId.IsNotNullOrEmpty())
                query = query.Where(c => c.OrderId == searchOrderId.Value);

            if (!string.IsNullOrEmpty(searchDiscountCode))
                query = query.Where(c => c.DiscountCode.Equals(searchDiscountCode));

            if (searchCustomerId.IsNotNullOrEmpty())
                query = query.Where(c => c.CustomerId == searchCustomerId);

            // Fetch paged data (synchronous LINQ — works for both in-memory and EF queryables)
            return query.ToList();
        }

        public async Task<DiscountRedemption> GetDiscountRedemptionByIdAsync(Guid redemptionId)
        {
            if (redemptionId.IsNullOrEmpty())
                throw new ArgumentNullException(nameof(DiscountRedemption));

            return await _context.DiscountRedemption.FirstOrDefaultAsync(c => c.Id == redemptionId);
        }

        public async Task<IEnumerable<DiscountRedemption>> GetDiscountRedemptionListByOrderId(Guid orderId)
        {
            if (orderId.IsNullOrEmpty())
                throw new ArgumentNullException("OrderId is empty. Cannot retrieve discount redemption");

            return await _context.DiscountRedemption.Where(c => c.OrderId == orderId && c.CustomerId.IsNotNullOrEmpty()).ToListAsync();
        }

        public async Task CreateDiscountRedemptionAsync(DiscountRedemption redemption)
        {
            if (redemption == null)
                throw new ArgumentNullException(nameof(DiscountRedemption));

            redemption.RedeemedAt = DateTime.UtcNow;
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                await _context.DiscountRedemption.AddAsync(redemption);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
                _redemptionCacheManager.ClearCache(typeof(DiscountRedemption).FullName);

            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                throw new Exception(ex.InnerException?.Message ?? ex.Message);
            }
        }

        public async Task UpdateDiscountRedemptionAsync(DiscountRedemption redemption)
        {
            if (redemption == null)
                throw new ArgumentNullException(nameof(DiscountRedemption));

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                _context.DiscountRedemption.Update(redemption);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
                _redemptionCacheManager.ClearCache(typeof(DiscountRedemption).FullName);

            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                throw new Exception(ex.InnerException?.Message ?? ex.Message);
            }
        }

        public async Task DeleteDiscountRedemptionAsync(DiscountRedemption redemption)
        {
            if (redemption == null)
                throw new ArgumentNullException(nameof(DiscountRedemption));

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                _context.DiscountRedemption.Remove(redemption);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
                _redemptionCacheManager.ClearCache(typeof(DiscountRedemption).FullName);

            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                throw new Exception(ex.InnerException?.Message ?? ex.Message);
            }
        }
        #endregion

        #region Paged Result
        public async Task<PagedResult<Discount>> GetDiscountPagedResultListAsync(string searchQuery = null,
            List<int> discountTypeIds = null,
            List<int> discountScopeIds = null,
            DateTime? createdOn = null,
            Guid? createdById = null,
            bool showActiveDiscountsOnly = false,
            int pageNumber = 0,
            int pageSize = int.MaxValue)
        {
            var query = await GetListAsync(_discountEntityType.Id);

            if (!string.IsNullOrEmpty(searchQuery))
                query = query.Where(c => c.Code.ToLower() == searchQuery.ToLower() ||
                                         c.Name.ToLower() == searchQuery.ToLower());

            if (discountTypeIds.HasAny())
                query = query.Where(c => discountTypeIds.Contains(c.DiscountTypeId));

            if (discountScopeIds.HasAny())
                query = query.Where(c => discountScopeIds.Contains(c.DiscountScopeId));

            if (createdOn.HasValue)
                query = query.Where(c => c.CreatedOn.Date == createdOn.Value);

            if (createdById.IsNotNullOrEmpty())
                query = query.Where(c => c.CreatedById == createdById.Value);

            return query.ToPagedResult(pageNumber, pageSize);
        }

        public async Task<PagedResult<DiscountRedemption>> GetDiscountRedemptionPagedResultListAsync(Guid? searchDiscountId = null,
            Guid? searchOrderId = null,
            string searchDiscountCode = null,
            Guid? searchCustomerId = null,
            int pageNumber = 0,
            int pageSize = int.MaxValue)
        {
            var query = _redemptionCacheManager.EntityCacheNotExists(typeof(DiscountRedemption).FullName) ?
                await _redemptionCacheManager.GenerateCacheAsync(await _context.DiscountRedemption.ToListAsync(), typeof(DiscountRedemption).FullName)
                : _redemptionCacheManager.GetEntityCacheData(typeof(DiscountRedemption).FullName);

            if (searchDiscountId.IsNotNullOrEmpty())
                query = query.Where(c => c.DiscountId == searchDiscountId.Value);

            if (searchOrderId.IsNotNullOrEmpty())
                query = query.Where(c => c.OrderId == searchOrderId.Value);

            if (!string.IsNullOrEmpty(searchDiscountCode))
                query = query.Where(c => c.DiscountCode.Equals(searchDiscountCode));

            if (searchCustomerId.IsNotNullOrEmpty())
                query = query.Where(c => c.CustomerId == searchCustomerId);

            return query.ToPagedResult(pageNumber, pageSize);
        }
        #endregion
    }
}
