using Microsoft.EntityFrameworkCore;
using RA.WebFramework.Extensions;
using RAerp.App_Data;
using RAerp.Domain.FileManager;

namespace RAerp.Services.FileServices
{
    public class FileService : IFileService
    {
        private readonly IWebHostEnvironment _env;
        private readonly RAerpContext _context;

        public FileService(IWebHostEnvironment env,
            RAerpContext context)
        {
            _env = env;
            _context = context;
        }

        public async Task<IEnumerable<FileEntityMapping>> GetListAsync()
        {
            return await _context.FileEntityMapping.AsNoTracking().ToListAsync();
        }

        public async Task<FileEntityMapping> GetByIdAsync(Guid id)
        {
            return await _context.FileEntityMapping.FirstOrDefaultAsync(f => f.Id == id);
        }

        public async Task<FileEntityMapping> GetByEntityIdAsync(Guid entityId)
        {
            return await _context.FileEntityMapping.FirstOrDefaultAsync(f => f.EntityId == entityId);
        }

        public async Task<FileEntityMapping> GetByFilePath(string filePath)
        {
            return await _context.FileEntityMapping.FirstOrDefaultAsync(f => f.FilePath == filePath);
        }

        public async Task<FileEntityMapping> UpdateAsync(FileEntityMapping fileEntity)
        {
            _context.FileEntityMapping.Update(fileEntity);
            await _context.SaveChangesAsync();
            return fileEntity;
        }

        public async Task<FileEntityMapping> CreateAsync(IFormFile file, Guid entityId, string systemName)
        {
            if (file.Length <= 0)
                throw new Exception("File is empty");
            if (file.Length > 10 * 1024 * 1024) // 10 MB limit
                throw new Exception("File size exceeds the limit of 10 MB");
            if (entityId.IsNullOrEmpty())
                throw new ArgumentNullException(nameof(entityId), "Entity ID cannot be null or empty");
            if (string.IsNullOrEmpty(systemName))
                throw new ArgumentNullException(nameof(systemName), "System name cannot be null or empty");

            var fileEntity = new FileEntityMapping
            {
                Id = Guid.NewGuid(),
                EntityId = entityId,
                EntitySystemName = systemName,
                FileName = file.FileName,
                FilePath = await SaveFileAsync(file),
                FileType = file.ContentType,
                FileSize = file.Length,
                CreatedAt = DateTime.UtcNow
            };

            _context.FileEntityMapping.Add(fileEntity);
            await _context.SaveChangesAsync();
            return fileEntity;
        }

        public async Task DeleteAsync(Guid id)
        {
            FileEntityMapping fileEntity = await GetByIdAsync(id);
            if (fileEntity == null)
                throw new Exception("File entity not found");

            _context.FileEntityMapping.Remove(fileEntity);
            await _context.SaveChangesAsync();
        }

        public async Task<string> SaveFileAsync(IFormFile file)
        {
            var uploads = Path.Combine(_env.WebRootPath, "uploads");
            Directory.CreateDirectory(uploads);

            var fileName = $"{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
            var filePath = Path.Combine(uploads, fileName);

            using var stream = new FileStream(filePath, FileMode.Create);
            await file.CopyToAsync(stream);

            return $"/uploads/{fileName}";
        }
    }
}
