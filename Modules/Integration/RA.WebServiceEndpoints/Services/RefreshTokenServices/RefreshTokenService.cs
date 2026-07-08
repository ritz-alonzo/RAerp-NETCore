using Microsoft.EntityFrameworkCore;
using RA.WebServiceEndpoints.App_Data;
using RA.WebServiceEndpoints.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace RA.WebServiceEndpoints.Services.RefreshTokenServices
{
    public class RefreshTokenService : IRefreshTokenService
    {
        private readonly RAWebServiceEndpointContext _db;

        public RefreshTokenService(RAWebServiceEndpointContext db)
        {
            _db = db;
        }

        public async Task<RefreshToken> CreateAsync(Guid userId, string accessToken, string family = null)
        {
            var token = new RefreshToken
            {
                UserId = userId,
                Token = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64)),
                AccessToken = accessToken,
                Family = family ?? Guid.NewGuid().ToString(),  // new family on first login
                ExpiresAt = DateTime.UtcNow.AddHours(1)
            };
            await _db.RefreshToken.AddAsync(token);
            await _db.SaveChangesAsync();
            return token;
        }

        public async Task<RefreshToken> GetByTokenAsync(string token)
        {
            return await _db.RefreshToken.FirstOrDefaultAsync(r => r.Token == token);
        }

        public async Task RevokeAllForUserAsync(Guid userId)
        {
            var tokens = _db.RefreshToken.Where(r => r.UserId == userId && !r.IsRevoked);
            await tokens.ForEachAsync(t => t.IsRevoked = true);
            await _db.SaveChangesAsync();
        }

        public async Task RevokeByFamilyAsync(string family)
        {
            // Breach detected — someone reused an old token — kill the whole family
            var tokens = _db.RefreshToken.Where(r => r.Family == family && !r.IsRevoked);
            await tokens.ForEachAsync(t => t.IsRevoked = true);
            await _db.SaveChangesAsync();
        }

        public async Task MarkAsUsedAsync(Guid id)
        {
            var token = await _db.RefreshToken.FindAsync(id);
            if (token != null) 
            { 
                token.IsUsed = true; 
                await _db.SaveChangesAsync(); 
            }
        }
    }
}
