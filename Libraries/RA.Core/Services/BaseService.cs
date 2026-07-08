using Microsoft.EntityFrameworkCore;
using RA.Core.DataCaching.CacheManagement;
using RA.Core.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RA.Core.Services
{
    /// <summary>
    /// Base Service for modules that doesn't inherit EntityTypes, FormTypes etc. base service class
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    public class BaseService<TEntity, TContext> : IBaseService<TEntity> where TEntity : BaseEntity
        where TContext : DbContext
    {

        #region Constants
        private readonly TContext _context;
        private readonly ICacheManager<TEntity> _cacheManager;
        #endregion

        #region Ctor
        public BaseService(TContext context, ICacheManager<TEntity> cacheManager)
        {
            _context = context;
            _cacheManager = cacheManager;
        }
        #endregion

        #region CRUD

        public virtual async Task<TEntity> GetByIdAsync(Guid id)
        {
            return await _context.Set<TEntity>().FirstOrDefaultAsync(c => c.Id == id);
        }

        public virtual async Task<IEnumerable<TEntity>> GetListAsync()
        {
            return _cacheManager.EntityCacheNotExists(typeof(TEntity).FullName) ?
                await _cacheManager.GenerateCacheAsync(await _context.Set<TEntity>().ToListAsync(), typeof(TEntity).FullName)
                : _cacheManager.GetEntityCacheData(typeof(TEntity).FullName);
        }

        public virtual async Task InsertAsync(TEntity entity)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                await _context.Set<TEntity>().AddAsync(entity);
                await _context.SaveChangesAsync();
                _cacheManager.ClearCache(typeof(TEntity).FullName);
                await transaction.CommitAsync();
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                throw new Exception(ex?.Message);
            }
        }

        public virtual async Task UpdateAsync(TEntity entity)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                _context.Set<TEntity>().Update(entity);
                await _context.SaveChangesAsync();
                _cacheManager.ClearCache(typeof(TEntity).FullName);
                await transaction.CommitAsync();
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                throw new Exception(ex?.Message);
            }
        }

        public virtual async Task DeleteAsync(TEntity entity)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                _context.Set<TEntity>().Update(entity);
                await _context.SaveChangesAsync();
                _cacheManager.ClearCache(typeof(TEntity).FullName);
                await transaction.CommitAsync();
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                throw new Exception(ex?.Message);
            }
        }

        #endregion

        #region Methods
        public virtual Task<List<TEntity>> ToPagedListAsync(IEnumerable<TEntity> query, int pageNumber, int pageSize)
        {
            // Default values
            if (pageNumber <= 0)
                pageNumber = 1;

            if (pageSize <= 0)
                pageSize = 10;

            // Skip rows
            var skip = (pageNumber - 1) * pageSize;

            // Fetch paged data (synchronous LINQ — works for both in-memory and EF queryables)
            var result = query.Skip(skip).Take(pageSize).ToList();

            return Task.FromResult(result);
        }
        #endregion
    }
}
