using Microsoft.EntityFrameworkCore;
using RAerp.App_Data;
using RAerp.Domain.DataChanges;

namespace RAerp.Services.DataChangeServices
{
    public class DataChangeService : IDataChangeService
    {
        #region Constants
        private readonly RAerpContext _erpContext;
        private readonly DbSet<DataChange> _dataChange;
        #endregion

        #region Ctor
        public DataChangeService(RAerpContext erpContext)
        {
            _erpContext = erpContext;
            _dataChange = _erpContext.Set<DataChange>();
        }
        #endregion

        #region CRUD
        public async Task<DataChange> GetByIdAsync(Guid id)
        {
            return await _dataChange.FirstOrDefaultAsync(c => c.Id == id);
        }

        public async Task<DataChange> GetByDataIdAndItemDataIdAsync(Guid dataId, Guid itemDataId)
        {
            return await _dataChange.FirstOrDefaultAsync(c => c.DataId == dataId && c.ItemDataId == itemDataId);
        }

        public async Task<IEnumerable<DataChange>> GetListAsync()
        {
            return await _dataChange.AsNoTracking().ToListAsync();
        }

        public async Task<IEnumerable<DataChange>> GetListByDataIdAsync(Guid dataId)
        {
            return await _dataChange.Where(c => c.DataId == dataId).AsNoTracking().ToListAsync();
        }

        public async Task InsertAsync(DataChange data)
        {
            data.CreatedOn = DateTime.UtcNow;
            await _dataChange.AddAsync(data);
            await _erpContext.SaveChangesAsync();
        }

        public async Task UpdateAsync(DataChange data)
        {
            _dataChange.Update(data);
            await _erpContext.SaveChangesAsync();
        }

        public async Task DeleteAsync(DataChange data, bool saveChangesToDb = false)
        {
            _dataChange.Remove(data);
            if (saveChangesToDb)
                await _erpContext.SaveChangesAsync();
        }

        public async Task ApplyChangesAsync(DataChange data)
        {
            data.IsApplied = true;
            _dataChange.Update(data);
            await _erpContext.SaveChangesAsync();
        }
        #endregion
    }
}
