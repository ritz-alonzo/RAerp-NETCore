using Microsoft.EntityFrameworkCore;
using RA.Core.Data;
using RA.FormTypes.Data;
using RA.FormTypes.Domain;

namespace RA.FormTypes.Services
{
    public interface IFormItemService<TForm, TItem, TSettings, TContext>
        where TForm : BaseForm
        where TItem : BaseFormItem
        where TSettings : BaseFormSetting
        where TContext : DbContext
    {
        TForm CreateTempForm();
        Task DeleteFormAsync(TForm form);
        Task DeleteItemAsync(TItem formItem, bool saveChangesToDb = false);
        Task<TForm> GetFormByIdAsync(Guid id);
        Task<TForm> GetFormByFormNbrAsync(string formNbr);
        Task<IEnumerable<TForm>> GetFormListAsync();
        Task<TItem> GetItemByIdAsync(Guid id);
        Task<TItem> GetItemByFormIdAndCatalogId(Guid formId, Guid catalogId);
        Task<TItem> GetTempItemAsync(Guid formId, Guid catalogId);
        Task<IEnumerable<TItem>> GetItemListAsync();
        Task<IEnumerable<TItem>> GetItemsByFormIdAsync(Guid formId);
        Task InsertFormAsync(TForm form);
        Task InsertItemAsync(TItem formItem, bool saveChangesToDb = false);
        Task InsertTempItemAsync(TItem formItem, DataChangeStatus dataChangeStatus);
        Task UpdateFormAsync(TForm form);
        Task UpdateItemAsync(TItem formItem, bool saveChangesToDb = false);
    }
}