using RAerp.Domain.Users;
using RAerp.Security.AccessRights;

namespace RAerp.Data.Security.Users
{
    public class UserAccessRightsRecord
    {
        #region CRUD
        public static AccessRecord ViewUser = new AccessRecord 
        { 
            Name = $"View {nameof(User)}", 
            Classification = nameof(User), 
            SystemName = typeof(User).FullName + $".View{nameof(User)}", 
            AccessRecordType = AccessType.View 
        };
        public static AccessRecord CreateUser = new AccessRecord 
        { 
            Name = $"Create {nameof(User)}", 
            Classification = nameof(User), 
            SystemName = typeof(User).FullName + $".Create{nameof(User)}", 
            AccessRecordType = AccessType.Create 
        };
        public static AccessRecord UpdateUser = new AccessRecord 
        { 
            Name = $"Update {nameof(User)}", 
            Classification = nameof(User), 
            SystemName = typeof(User).FullName + $".Update{nameof(User)}",
            AccessRecordType = AccessType.Update 
        };
        public static AccessRecord DeleteUser = new AccessRecord 
        { 
            Name = $"Delete {nameof(User)}", 
            Classification = nameof(User), 
            SystemName = typeof(User).FullName + $".Delete{nameof(User)}", 
            AccessRecordType = AccessType.Delete 
        };
        #endregion
    }
}
