using Microsoft.EntityFrameworkCore;
using RA.BusinessEntities.App_Data;
using RA.BusinessEntities.Data;
using RA.BusinessEntities.Domain;
using RA.Core.DataCaching.CacheManagement;
using RA.EntityTypes.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RA.BusinessEntities.Services
{
    public class BusinessEntityService : EntityTypeService<BusinessEntity, BusinessEntitySetting, RABusinessEntityContext>, IBusinessEntityService
    {
        private readonly RABusinessEntityContext _context;
        private readonly DbSet<BusinessEntity> _businessEntity;
        private readonly ICacheManager<BusinessEntity> _cacheManager;
        private readonly IEntityTypeManager _entityTypeManager;

        public BusinessEntityService(RABusinessEntityContext context, 
            ICacheManager<BusinessEntity> cacheManager, 
            IEntityTypeManager entityTypeManager)
            : base (context, cacheManager, entityTypeManager)
        {
            _context = context;
            _businessEntity = _context.Set<BusinessEntity>();
            _cacheManager = cacheManager;
            _entityTypeManager = entityTypeManager;
        }

        // override when there's an additional
        public override async Task Insert(BusinessEntity entity)
        {
            await base.Insert(entity);
        }

        public void Test()
        {
            var testing = _context.Set<BusinessEntity>().FirstOrDefault();
        }
    }
}
