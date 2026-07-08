using RA.Discounts.Domain;
using RAerp.Security.AccessRights;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RA.Discounts.Data.Security
{
    public class DiscountAccessRightsRecord
    {
        #region CRUD
        public static AccessRecord ViewDiscount = new AccessRecord
        {
            Name = $"View {nameof(Discount)}",
            Classification = nameof(Discount),
            SystemName = typeof(Discount).FullName + $".View{nameof(Discount)}",
            AccessRecordType = AccessType.View
        };
        public static AccessRecord CreateDiscount = new AccessRecord
        {
            Name = $"Create {nameof(Discount)}",
            Classification = nameof(Discount),
            SystemName = typeof(Discount).FullName + $".Create{nameof(Discount)}",
            AccessRecordType = AccessType.Create
        };
        public static AccessRecord UpdateDiscount = new AccessRecord
        {
            Name = $"Update {nameof(Discount)}",
            Classification = nameof(Discount),
            SystemName = typeof(Discount).FullName + $".Update{nameof(Discount)}",
            AccessRecordType = AccessType.Update
        };
        public static AccessRecord DeleteDiscount = new AccessRecord
        {
            Name = $"Delete {nameof(Discount)}",
            Classification = nameof(Discount),
            SystemName = typeof(Discount).FullName + $".Delete{nameof(Discount)}",
            AccessRecordType = AccessType.Delete
        };
        #endregion
    }
}
