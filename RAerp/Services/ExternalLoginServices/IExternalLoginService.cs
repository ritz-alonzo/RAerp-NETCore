using RA.WebFramework.Models.Pagination;
using RAerp.Domain.Users;

namespace RAerp.Services.ExternalLoginServices
{
    public interface IExternalLoginService
    {
        Task DeleteAsync(ExternalLogin externalLogin);
        Task<ExternalLogin> GetByIdAsync(Guid id);
        Task<ExternalLogin> GetByProviderAndProviderEmailAsync(string provider, string email);
        Task<ExternalLogin> GetByUserIdAsync(Guid userId);
        Task<ExternalLogin> GetByProviderAndUserIdAsync(string provider, Guid userId);
        Task<ExternalLogin> GetByProviderNameAsync(string providerName);
        Task<ExternalLogin> GetByProviderAndSubjectIdAsync(string provider, string subjectId);
        Task<IEnumerable<ExternalLogin>> GetListAsync(Guid? userId = null, string provider = null, string providerSubjectId = null, string providerEmail = null, string providerName = null, DateTime? createdOn = null, DateTime? lastLoginOn = null);
        Task<PagedResult<ExternalLogin>> GetPaagedResultListAsync(Guid? userId = null, string provider = null, string providerSubjectId = null, string providerEmail = null, string providerName = null, DateTime? createdOn = null, DateTime? lastLoginOn = null, int? pageNumber = 0, int? pageSize = int.MaxValue);
        Task InsertAsync(ExternalLogin externalLogin);
        Task UpdateAsync(ExternalLogin externalLogin);
    }
}