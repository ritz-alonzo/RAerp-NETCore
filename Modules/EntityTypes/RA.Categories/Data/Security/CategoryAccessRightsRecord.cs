using RA.Categories.Domain;
using RAerp.Security.AccessRights;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RA.Categories.Data.Security
{
    public class CategoryAccessRightsRecord
    {
        #region CRUD
        public static AccessRecord ViewCategory = new AccessRecord
        {
            Name = $"View {nameof(Category)}",
            Classification = nameof(Category),
            SystemName = typeof(Category).FullName + $".View{nameof(Category)}",
            AccessRecordType = AccessType.View
        };
        public static AccessRecord CreateCategory = new AccessRecord
        {
            Name = $"Create {nameof(Category)}",
            Classification = nameof(Category),
            SystemName = typeof(Category).FullName + $".Create{nameof(Category)}",
            AccessRecordType = AccessType.Create
        };
        public static AccessRecord UpdateCategory = new AccessRecord
        {
            Name = $"Update {nameof(Category)}",
            Classification = nameof(Category),
            SystemName = typeof(Category).FullName + $".Update{nameof(Category)}",
            AccessRecordType = AccessType.Update
        };
        public static AccessRecord DeleteCategory = new AccessRecord
        {
            Name = $"Delete {nameof(Category)}",
            Classification = nameof(Category),
            SystemName = typeof(Category).FullName + $".Delete{nameof(Category)}",
            AccessRecordType = AccessType.Delete
        };
        #endregion
    }
}
