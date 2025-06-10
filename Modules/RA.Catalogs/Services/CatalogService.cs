using RA.Catalogs.App_Data;
using RA.Catalogs.Data;
using RA.Catalogs.Domain;
using RA.Core.DataCaching.CacheManagement;
using RA.EntityTypes.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RA.Catalogs.Services
{
    public class CatalogService : EntityTypeService<Catalog, CatalogSetting, RACatalogContext>, ICatalogService
    {
        private readonly RACatalogContext _context;

        public CatalogService(RACatalogContext erpContext, 
            ICacheManager<Catalog> cacheManager, 
            IEntityTypeManager entityTypeManager) 
            : base(erpContext, cacheManager, entityTypeManager)
        {
        }

        public void Insert()
        {

        }

        public void Test()
        {
            var testing = _context.Set<Catalog>().FirstOrDefault();
        }
    }
}
