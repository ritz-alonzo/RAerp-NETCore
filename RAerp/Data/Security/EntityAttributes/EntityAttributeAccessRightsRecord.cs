using RAerp.Domain.EntityAttributes;
using RAerp.Security.AccessRights;

namespace RAerp.Data.Security.EntityAttributes
{
    public class EntityAttributeAccessRightsRecord
    {
        #region CRUD
        public static AccessRecord ViewEntityAttribute = new AccessRecord
        {
            Name = $"View {nameof(EntityAttribute)}",
            Classification = nameof(EntityAttribute),
            SystemName = typeof(EntityAttribute).FullName + $".View{nameof(EntityAttribute)}",
            AccessRecordType = AccessType.View
        };
        public static AccessRecord CreateEntityAttribute = new AccessRecord
        {
            Name = $"Create {nameof(EntityAttribute)}",
            Classification = nameof(EntityAttribute),
            SystemName = typeof(EntityAttribute).FullName + $".Create{nameof(EntityAttribute)}",
            AccessRecordType = AccessType.Create
        };
        public static AccessRecord UpdateEntityAttribute = new AccessRecord
        {
            Name = $"Update {nameof(EntityAttribute)}",
            Classification = nameof(EntityAttribute),
            SystemName = typeof(EntityAttribute).FullName + $".Update{nameof(EntityAttribute)}",
            AccessRecordType = AccessType.Update
        };
        public static AccessRecord DeleteEntityAttribute = new AccessRecord
        {
            Name = $"Delete {nameof(EntityAttribute)}",
            Classification = nameof(EntityAttribute),
            SystemName = typeof(EntityAttribute).FullName + $".Delete{nameof(EntityAttribute)}",
            AccessRecordType = AccessType.Delete
        };
        #endregion
    }
}
