using RAerp.Domain.DataChanges;

namespace RAerp.Services.DataChangeServices
{
    public interface IDataChangeService
    {
        Task ApplyChangesAsync(DataChange data);
        Task<DataChange> GetByIdAsync(Guid id);
        Task<DataChange> GetByDataIdAndItemDataIdAsync(Guid dataId, Guid itemDataId);
        Task<IEnumerable<DataChange>> GetListAsync();
        Task<IEnumerable<DataChange>> GetListByDataIdAsync(Guid dataId);
        Task InsertAsync(DataChange data);
        Task UpdateAsync(DataChange data);
        Task DeleteAsync(DataChange data, bool saveChangesToDb = false);
    }
}