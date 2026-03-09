using RA.Catalogs.Domain;
using RAerp.Security.AccessRights;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RA.Catalogs.Data.Security
{
    public class CatalogAccessRightsRecord
    {
        #region CRUD
        public static AccessRecord ViewCatalog = new AccessRecord
        {
            Name = $"View {nameof(Catalog)}",
            Classification = nameof(Catalog),
            SystemName = typeof(Catalog).FullName + $".View{nameof(Catalog)}",
            AccessRecordType = AccessType.View
        };
        public static AccessRecord CreateCatalog = new AccessRecord
        {
            Name = $"Create {nameof(Catalog)}",
            Classification = nameof(Catalog),
            SystemName = typeof(Catalog).FullName + $".Create{nameof(Catalog)}",
            AccessRecordType = AccessType.Create
        };
        public static AccessRecord UpdateCatalog = new AccessRecord
        {
            Name = $"Update {nameof(Catalog)}",
            Classification = nameof(Catalog),
            SystemName = typeof(Catalog).FullName + $".Update{nameof(Catalog)}",
            AccessRecordType = AccessType.Update
        };
        public static AccessRecord DeleteCatalog = new AccessRecord
        {
            Name = $"Delete {nameof(Catalog)}",
            Classification = nameof(Catalog),
            SystemName = typeof(Catalog).FullName + $".Delete{nameof(Catalog)}",
            AccessRecordType = AccessType.Delete
        };
        #endregion
    }
}
