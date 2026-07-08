using RA.WebServiceEndpoints.Domain;

namespace RA.WebServiceEndpoints.Services.RefreshTokenServices
{
    public interface IRefreshTokenService
    {
        Task<RefreshToken> CreateAsync(Guid userId, string accessToken, string family = null);
        Task<RefreshToken> GetByTokenAsync(string token);
        Task MarkAsUsedAsync(Guid id);
        Task RevokeAllForUserAsync(Guid userId);
        Task RevokeByFamilyAsync(string family);
    }
}