using RAerp.Domain.FileManager;

namespace RAerp.Services.FileServices
{
    public interface IFileService
    {
        Task<FileEntityMapping> CreateAsync(IFormFile file, Guid entityId, string systemName);
        Task DeleteAsync(Guid id);
        Task<FileEntityMapping> GetByEntityIdAsync(Guid entityId);
        Task<FileEntityMapping> GetByFilePath(string filePath);
        Task<FileEntityMapping> GetByIdAsync(Guid id);
        Task<IEnumerable<FileEntityMapping>> GetListAsync();
        Task<string> SaveFileAsync(IFormFile file);
        Task<FileEntityMapping> UpdateAsync(FileEntityMapping fileEntity);
    }
}