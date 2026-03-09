using RAerp.Security.AccessRights;

namespace RAerp.Security.AccessRightsControl
{
    public interface IAccessControl
    {
        #region CRUD Access

        Task<bool> HasCreateAccessAsync<TEntity>() where TEntity : class;
        Task<bool> HasDeleteAccessAsync<TEntity>() where TEntity : class;
        Task<bool> HasUpdateAccessAsync<TEntity>() where TEntity : class;
        Task<bool> HasViewAccessAsync<TEntity>() where TEntity : class;

        #endregion

        #region Direct Access checking with Access Record
        Task<bool> AccessPermittedAsync<TEntity>(AccessRecord accessRecord)
            where TEntity : class;
        #endregion

        #region Super Admin Access

        Task<bool> HasSuperAdminAccessAsync();

        #endregion
    }
}