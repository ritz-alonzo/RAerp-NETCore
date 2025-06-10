using Microsoft.EntityFrameworkCore;
using RA.Data.App_Data;
using RA.Data.Domain.DataChanges;

namespace RAerp.Services.DataChangeServices
{
    public class DataChangeService
    {
        private readonly RAerpContext _erpContext;
        private readonly DbSet<DataChange> _dataChange;

        public DataChangeService(RAerpContext erpContext)
        {
            _erpContext = erpContext;
            _dataChange = _erpContext.Set<DataChange>();
        }

        public async Task<DataChange> GetById(Guid id)
        {
            return await _dataChange.FirstOrDefaultAsync(c => c.Id == id);
        }

        public async Task<List<DataChange>> GetList()
        {
            return await _dataChange.ToListAsync();
        }

        public async Task Insert(DataChange data)
        {
            data.CreatedOn = DateTime.Now;
            await _dataChange.AddAsync(data);
            await _erpContext.SaveChangesAsync();
        }

        public async Task ApplyChanges(DataChange data)
        {
            data.IsApplied = true;
            data.ModifiedOn = DateTime.Now;
            _dataChange.Update(data);
            await _erpContext.SaveChangesAsync();
        }

    }
}
