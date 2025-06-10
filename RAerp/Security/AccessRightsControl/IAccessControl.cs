using RAerp.Security.AccessRights;

namespace RAerp.Security.AccessRightsControl
{
    public interface IAccessControl
    {
        #region CRUD Access

        Task<bool> HasCreateAccess<TEntity>() where TEntity : class;
        Task<bool> HasDeleteAccess<TEntity>() where TEntity : class;
        Task<bool> HasUpdateAccess<TEntity>() where TEntity : class;
        Task<bool> HasViewAccess<TEntity>() where TEntity : class;

        #endregion

        #region Direct Access checking with Access Record
        Task<bool> AccessPermitted<TEntity>(AccessRecord accessRecord)
            where TEntity : class;
        #endregion

        #region Super Admin Access

        bool HasSuperAdminAccess();

        #endregion
    }
}