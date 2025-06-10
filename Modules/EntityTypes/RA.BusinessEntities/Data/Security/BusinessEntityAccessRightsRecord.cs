using RA.BusinessEntities.Domain;
using RAerp.Security.AccessRights;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RA.BusinessEntities.Data.Security
{
    public class BusinessEntityAccessRightsRecord
    {
        #region CRUD
        public static AccessRecord ViewBusinessEntity = new AccessRecord
        {
            Name = $"View {nameof(BusinessEntity)}",
            Classification = nameof(BusinessEntity),
            SystemName = typeof(BusinessEntity).FullName + $".View{nameof(BusinessEntity)}",
            AccessRecordType = AccessType.View
        };
        public static AccessRecord CreateBusinessEntity = new AccessRecord
        {
            Name = $"Create {nameof(BusinessEntity)}",
            Classification = nameof(BusinessEntity),
            SystemName = typeof(BusinessEntity).FullName + $".Create{nameof(BusinessEntity)}",
            AccessRecordType = AccessType.Create
        };
        public static AccessRecord UpdateBusinessEntity = new AccessRecord
        {
            Name = $"Update {nameof(BusinessEntity)}",
            Classification = nameof(BusinessEntity),
            SystemName = typeof(BusinessEntity).FullName + $".Update{nameof(BusinessEntity)}",
            AccessRecordType = AccessType.Update
        };
        public static AccessRecord DeleteBusinessEntity = new AccessRecord
        {
            Name = $"Delete {nameof(BusinessEntity)}",
            Classification = nameof(BusinessEntity),
            SystemName = typeof(BusinessEntity).FullName + $".Delete{nameof(BusinessEntity)}",
            AccessRecordType = AccessType.Delete
        };
        #endregion
    }
}
